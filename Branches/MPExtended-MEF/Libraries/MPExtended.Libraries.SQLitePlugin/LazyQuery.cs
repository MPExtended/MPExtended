#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Libraries.SQLitePlugin
{
    public class LazyQuery<T> : ILazyQuery<T> where T : new()
    {
        public delegate T FillMethod(T obj, SQLiteDataReader reader);
        public delegate T CreateMethod(SQLiteDataReader reader);
        public delegate List<T> ListCreateMethod(SQLiteDataReader reader);

        private Database db;
        private string inputQuery;
        private SQLiteParameter[] parameters;
        private IEnumerable<SQLFieldMapping> mapping;
        private ObjectFactory<T> factory;

        private List<Tuple<string, string, bool>> orderItems = new List<Tuple<string, string, bool>>(); // fieldname, sqlname, descending
        private List<Tuple<string, string, object>> whereItems = new List<Tuple<string, string, object>>(); // sqlname, operator, value

        public LazyQuery(Database db, string sql, IEnumerable<SQLFieldMapping> mapping)
        {
            this.db = db;
            this.inputQuery = sql;
            this.mapping = mapping;
            this.parameters = new SQLiteParameter[] { };
            this.factory = ObjectFactory<T>.FromList(new AutoFiller<T>(mapping).AutoCreateAndFill);
        }

        public LazyQuery(Database db, string sql, SQLiteParameter[] parameters, IEnumerable<SQLFieldMapping> mapping)
            : this(db, sql, mapping)
        {
            this.parameters = parameters;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ExecuteQuery().GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ExecuteQuery().GetEnumerator();
        }

        public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            // TODO: this one really needs improvement to do subsequent ordering instead
            if (descending)
                return ExecuteQuery().OrderByDescending(keySelector, comparer);
            return ExecuteQuery().OrderBy(keySelector, comparer);
        }

        private Tuple<string, SQLiteParameter[]> PrepareQuery()
        {
            // prepare order
            string sql = this.inputQuery;
            string orderSql = "ORDER BY " + String.Join(", ", orderItems.Select(x => x.Item2 + " " + (x.Item3 ? "DESC" : "ASC")));
            sql = sql.Replace("%order", orderItems.Count == 0 ? String.Empty : orderSql);

            // prepare where
            SQLiteParameter[] realParams = whereItems.Select((x, index) => new SQLiteParameter("@lazyQuery" + index, x.Item3)).Union(parameters).ToArray();
            string whereSql = String.Join(" AND ", whereItems.Select((x, index) => "(" + x.Item1 + " " + x.Item2 + " @lazyQuery" + index + ")"));
            sql = sql.Replace("%where", whereItems.Count == 0 ? "1" : whereSql);

            return new Tuple<string, SQLiteParameter[]>(sql, realParams);
        }

        private List<T> ExecuteQuery()
        {
            Tuple<string, SQLiteParameter[]> prepared = PrepareQuery();

            // execute query
            List<T> ret = new List<T>();
            using (Query query = new Query(db.DatabasePath, prepared.Item1, prepared.Item2))
            {
                while (query.Reader.Read())
                {
                    ret.AddRange(factory.CreateObjects(query.Reader));
                }
            }

            return ret;
        }

        private IEnumerable<T> SmartWhere(Expression<Func<T, bool>> predicate)
        {
            // validate the parameter
            if (predicate.Parameters.Count != 1)
                return null;
            if (!predicate.Parameters[0].Type.IsAssignableFrom(typeof(T)))
                return null;

            // parse the body
            if (predicate.NodeType != ExpressionType.Lambda || !(predicate.Body is BinaryExpression))
                return null;
            BinaryExpression ex = (BinaryExpression)predicate.Body;
            if (ex.NodeType != ExpressionType.Equal)
                return null;

            // get left value
            MemberExpression left = (MemberExpression)ex.Left;
            if (left.Expression.NodeType != ExpressionType.Parameter)
                return null;
            string leftName = left.Member.Name;

            // get right value and check for support
            if (ex.Right.NodeType != ExpressionType.MemberAccess)
                return null;
            MemberExpression right = (MemberExpression)ex.Right;
            if (right.Expression.NodeType != ExpressionType.Constant)
                return null;
            ConstantExpression rightval = (ConstantExpression)right.Expression;
            object value = ((System.Reflection.FieldInfo)right.Member).GetValue(rightval.Value);

            // and return value if supported
            if (!(value is Int32) && !(value is String))
                return null;
            SQLFieldMapping thisMapping = mapping.Where(x => x.PropertyName == leftName).First();
            whereItems.Add(new Tuple<string, string, object>(thisMapping.FullSQLName, "=", value));
            return this;
        }

        public IEnumerable<T> Where(Expression<Func<T, bool>> predicate)
        {
            try
            {
                IEnumerable<T> smart = SmartWhere(predicate);
                if (smart != null)
                    return smart;
            }
            catch (Exception ex)
            {
                // TODO: do some logging here
            }

            // if we can't do it in SQL just execute the code
            Func<T, bool> comp = predicate.Compile();
            return ExecuteQuery().Where(comp);
        }

        private IOrderedEnumerable<T> SmartAddOrder<TKey>(bool desc, Expression<Func<T, TKey>> keySelector)
        {
            // validate the parameter
            if (keySelector.Parameters.Count != 1)
                return null;
            if (!keySelector.Parameters[0].Type.IsAssignableFrom(typeof(T)))
                return null;

            // parse the body
            if (!(keySelector.Body is MemberExpression))
                return null;
            
            // we expect a parameter or a cast to an interface here
            MemberExpression ex = (MemberExpression)keySelector.Body;
            if (ex.Expression.NodeType != ExpressionType.Convert && ex.Expression.NodeType != ExpressionType.Parameter)
                return null;

            // we got the fieldname, map it to an SQL name
            string fieldName = ex.Member.Name;
            SQLFieldMapping thisMapping = mapping.Where(x => x.PropertyName == fieldName).First();

            // add to the order clausule
            orderItems.Add(new Tuple<string, string, bool>(fieldName, thisMapping.FullSQLName, desc));
            return this;
        }

        private IOrderedEnumerable<T> AddOrder<TKey>(bool desc, Expression<Func<T, TKey>> keySelector)
        {
            // first try to do it in SQL
            try
            {
                IOrderedEnumerable<T> smart = SmartAddOrder(desc, keySelector);
                if (smart != null)
                    return smart;
            }
            catch (Exception)
            {
                // TODO: do some logging here
            }

            // if we can't do it in SQL just execute the code
            Func<T, TKey> comp = keySelector.Compile();
            if (!desc)
            {
                return ExecuteQuery().OrderBy(comp);
            }
            else
            {
                return ExecuteQuery().OrderByDescending(comp);
            }
        }

        public IOrderedEnumerable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return AddOrder(false, keySelector);
        }

        public IOrderedEnumerable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return AddOrder(true, keySelector);
        }

        public int Count()
        {
            Tuple<string, SQLiteParameter[]> prepared = PrepareQuery();
            string sql = "SELECT COUNT(*) AS count FROM (" + prepared.Item1 + ") tbl";
            using (Query query = new Query(db.DatabasePath, sql, prepared.Item2))
            {
                query.Reader.Read();
                return query.Reader.ReadInt32(0);
            }
        }
    }
}
