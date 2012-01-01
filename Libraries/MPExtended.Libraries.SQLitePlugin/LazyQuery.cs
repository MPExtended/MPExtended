#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
    public class LazyQuery<T> : ILazyQuery<T>, IEnumerable<T> where T : new()
    {
        private Database db;
        private string inputQuery;
        private SQLiteParameter[] parameters;
        private IEnumerable<SQLFieldMapping> mapping;
        private ObjectFactory<T> factory;
        private Delegates<T>.FinalizeObject finalize;

        private List<Tuple<string, string, bool>> orderItems = new List<Tuple<string, string, bool>>(); // fieldname, sqltext, descending
        private List<Tuple<string, object>> whereItems = new List<Tuple<string, object>>(); // sqltext (with %prepared), value
        private Tuple<int, int> range = null;

        private List<T> result = null;

        public LazyQuery(Database db, string sql, SQLiteParameter[] parameters, IEnumerable<SQLFieldMapping> mapping, Delegates<T>.CreateMethod createmethod, Delegates<T>.FinalizeObject finalize)
        {
            this.db = db;
            this.inputQuery = sql;
            this.mapping = mapping;
            this.parameters = new SQLiteParameter[] { };
            this.factory = ObjectFactory<T>.FromCreate(createmethod);
            this.finalize = finalize;
        }

        public LazyQuery(Database db, string sql, SQLiteParameter[] parameters, IEnumerable<SQLFieldMapping> mapping, Delegates<T>.CreateMethod createmethod)
            : this(db, sql, parameters, mapping, createmethod, null)
        {
        }

        public LazyQuery(Database db, string sql, SQLiteParameter[] parameters, IEnumerable<SQLFieldMapping> mapping, Delegates<T>.FinalizeObject finalize)
            : this(db, sql, parameters, mapping, new AutoFiller<T>(mapping).AutoCreate, finalize)
        {
        }

        public LazyQuery(Database db, string sql, IEnumerable<SQLFieldMapping> mapping, Delegates<T>.CreateMethod createmethod)
            : this(db, sql, new SQLiteParameter[] { }, mapping, createmethod)
        {
        }

        public LazyQuery(Database db, string sql, IEnumerable<SQLFieldMapping> mapping, Delegates<T>.FinalizeObject finalize)
            : this(db, sql, new SQLiteParameter[] { }, mapping, finalize)
        {
        }

        public LazyQuery(Database db, string sql, SQLiteParameter[] parameters, IEnumerable<SQLFieldMapping> mapping)
            : this(db, sql, parameters, mapping, (Delegates<T>.FinalizeObject)null)
        {
        }

        public LazyQuery(Database db, string sql, IEnumerable<SQLFieldMapping> mapping)
            : this(db, sql, mapping, (Delegates<T>.FinalizeObject)null)
        {
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
            string sql = this.inputQuery;

            // fix query if needed
            if (!sql.Contains("%order"))
                sql = sql + " %order"; // at the end works in 99,9% of the cases. just add it yourself for the other 0,1% (mainly UNION)

            // prepare order
            string orderSql = "ORDER BY " + String.Join(", ", orderItems.Select(x => x.Item2));
            if (range != null)
            {
                orderSql += " LIMIT " + range.Item1 + ", " + range.Item2;
            }
            sql = sql.Replace("%order", orderItems.Count == 0 ? String.Empty : orderSql);

            // prepare where
            SQLiteParameter[] realParams = whereItems.Select((x, index) => new SQLiteParameter("@lazyQuery" + index, x.Item2)).Union(parameters).ToArray();
            string whereSql = "(" + String.Join(" AND ", whereItems.Select((x, index) => "(" + x.Item1.Replace("%prepared", "@lazyQuery" + index) + ")")) + ")";
            sql = sql.Replace("%where", whereItems.Count == 0 ? "1" : whereSql);

            return new Tuple<string, SQLiteParameter[]>(sql, realParams);
        }

        private List<T> ExecuteQuery()
        {
            // don't execute queries twice
            if (result != null)
            {
                return result;
            }

            // execute query
            Tuple<string, SQLiteParameter[]> prepared = PrepareQuery();
            result = new List<T>();
            using (Query query = new Query(db.DatabasePath, prepared.Item1, prepared.Item2))
            {
                while (query.Reader.Read())
                {
                    T obj = factory.CreateObject(query.Reader);
                    if(finalize != null) 
                    {
                        obj = finalize(obj);
                    }
                    result.Add(obj);
                }
            }

            return result;
        }

        private IEnumerable<T> SmartWhere(Expression<Func<T, bool>> predicate)
        {
            // make sure query is valid and not yet executed
            if (!this.inputQuery.Contains("%where"))
                return null;
            if (result != null)
                return null;

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

            // check right value
            if (ex.Right.NodeType == ExpressionType.MemberAccess && ((MemberExpression)ex.Right).Expression.NodeType == ExpressionType.Parameter)
                return null;
            object value = Expression.Lambda(ex.Right).Compile().DynamicInvoke();
            if (value is Boolean)
                value = (bool)value ? 1 : 0;
            if (!(value is Int32) && !(value is String))
                return null;

            // check if mapping supports SQL compare
            var mappingList = mapping.Where(x => x.PropertyName == leftName);
            if (mappingList.Count() == 0)
                return null;
            SQLFieldMapping thisMapping = mappingList.First();
            if (!Attribute.IsDefined(thisMapping.Reader.Method, typeof(AllowSQLCompareAttribute)))
                return null;

            // build SQL
            AllowSQLCompareAttribute attr = (AllowSQLCompareAttribute)Attribute.GetCustomAttribute(thisMapping.Reader.Method, typeof(AllowSQLCompareAttribute));

            // create smart where
            whereItems.Add(new Tuple<string, object>(attr.GetSQLCondition(thisMapping), value));
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
            catch (Exception)
            {
                // just ignore it
            }

            // if we can't do it in SQL just execute the code
            Func<T, bool> comp = predicate.Compile();
            return ExecuteQuery().Where(comp);
        }

        private IOrderedEnumerable<T> SmartAddOrder<TKey>(bool desc, Expression<Func<T, TKey>> keySelector)
        {
            // don't execute query twice
            if (result != null)
                return null;

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
            var list = mapping.Where(x => x.PropertyName == fieldName);
            if (list.Count() == 0)
                return null;
            SQLFieldMapping thisMapping = list.First();

            // check if supported on reader
            if (!Attribute.IsDefined(thisMapping.Reader.Method, typeof(AllowSQLSortAttribute)))
                return null;

            // add to the order clausule
            AllowSQLSortAttribute attr = (AllowSQLSortAttribute)Attribute.GetCustomAttribute(thisMapping.Reader.Method, typeof(AllowSQLSortAttribute));
            orderItems.Add(new Tuple<string, string, bool>(fieldName, attr.GetSQLText(thisMapping).Replace("%order", desc ? "DESC" : "ASC"), desc));
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
                // ah well, just ignore it
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

        public IOrderedEnumerable<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return AddOrder(false, keySelector);
        }

        public IOrderedEnumerable<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return AddOrder(true, keySelector);
        }

        public IEnumerable<T> GetRange(int index, int count)
        {
            // don't execute query twice
            if (result != null)
            {
                return result.GetRange(index, count);
            }

            range = new Tuple<int, int>(index, count);
            return this;
        }

        public int Count()
        {
            // if we already know it, just return that instead of querying again
            if (result != null)
            {
                return result.Count;
            }

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
