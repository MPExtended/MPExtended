#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MPExtended.Libraries.SQLitePlugin
{
    // TElement and TResult are always equal to T, but the compiler doesn't know that, so we do some needless casting and dynamic typing here.
    internal class LazyQueryProvider<T> : IQueryProvider where T : class, new()
    {
        private LazyQuery<T> Query { get; set; }

        public LazyQueryProvider(LazyQuery<T> query)
        {
            Query = query;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            // try to do it in LazyQuery, if possible
            MethodCallExpression mce = expression as MethodCallExpression;
            var res = CreateQuerySmart(mce) as IQueryable<TElement>;
            if (res != null)
                return res;

            // if not possible just execute the damn method
            return mce.Method.Invoke(null, GetArgumentsFromExpression(mce)) as IQueryable<TElement>;
        }

        private IQueryable<T> CreateQuerySmart(MethodCallExpression mce)
        {
            string method = mce.Method.Name;
            if (method == "Where" && mce.Arguments.Count == 2 && mce.Arguments[1] is UnaryExpression)
            {
                var expr = (mce.Arguments[1] as UnaryExpression).Operand as Expression<Func<T, bool>>;
                if (expr != null)
                {
                    return Query.Where(expr);
                }
            }

            if ((method.StartsWith("OrderBy") || method.StartsWith("ThenBy")) && mce.Arguments.Count == 2 && mce.Arguments[1] is UnaryExpression)
            {
                var expr = (mce.Arguments[1] as UnaryExpression).Operand;
                var methodInstance = Query.GetType().GetMethod(mce.Method.Name);
                if (expr == null || method == null || !(expr is LambdaExpression))
                {
                    return null;
                }

                Type genericType = (expr as LambdaExpression).ReturnType;
                var res = methodInstance.MakeGenericMethod(genericType).Invoke(Query, new object[] { expr }) as IQueryable<T>;
                if (res != null)
                {
                    return res;
                }
            }

            if ((method == "Skip" || method == "Take") && mce.Arguments.Count == 2 && mce.Arguments[1] is ConstantExpression)
            {
                var expr = (mce.Arguments[1] as ConstantExpression).Value;
                if (expr is int)
                {
                    return method == "Skip" ? Query.Skip((int)expr) : Query.Take((int)expr);
                }
            }

            return null;
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            // The compiler keeps whining about wanting a where TResult : class, new(), but that constraint is actually there because of the T
            // constraint and the fact that TResult is always equal to T (but the compiler doesn't get that).

            // try to do it in LazyQuery, if possible
            MethodCallExpression mce = expression as MethodCallExpression;
            dynamic smartRes = ExecuteQuerySmart(mce);
            if(smartRes != null)
                return (TResult)smartRes;

            // if not possible just execute the damn method
            dynamic res = mce.Method.Invoke(null, GetArgumentsFromExpression(mce));
            return (TResult)res;
        }

        public T ExecuteQuerySmart(MethodCallExpression mce)
        {
            if (mce.Method.Name == "First" && mce.Arguments.Count == 2 && mce.Arguments[1] is UnaryExpression)
            {
                var expr = (mce.Arguments[1] as UnaryExpression).Operand as Expression<Func<T, bool>>;
                if (expr != null)
                {
                    return Query.Where(expr).ToList().First();
                }
            }

            if (mce.Method.Name == "First" && mce.Arguments.Count == 1)
            {
                return Query.GetRange(0, 1).ToList().First();
            }

            return default(T);
        }

        private object[] GetArgumentsFromExpression(MethodCallExpression mce)
        {
            // get arguments for execution of the method
            object[] arguments = new object[mce.Arguments.Count];
            arguments[0] = Query.ExecuteQuery().AsQueryable();
            for (int i = 1; i < mce.Arguments.Count; i++)
            {
                if (mce.Arguments[i] is UnaryExpression)
                {
                    arguments[i] = (mce.Arguments[i] as UnaryExpression).Operand;
                }
                else if (mce.Arguments[i] is ConstantExpression)
                {
                    arguments[i] = (mce.Arguments[i] as ConstantExpression).Value;
                }
                else
                {
                    arguments[i] = mce.Arguments[i];
                }
            }

            return arguments;
        }
    }
}
