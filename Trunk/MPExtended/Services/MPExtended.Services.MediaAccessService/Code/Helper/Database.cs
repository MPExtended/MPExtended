using System.Collections.Generic;
using System.Data.SQLite;

namespace MPExtended.Services.MediaAccessService.Code.Helper
{
    internal abstract class Database
    {
        protected delegate T FillObject<T>(SQLiteDataReader reader);

        protected string DatabasePath
        {
            get;
            set;
        }

        protected Database(string databasePath)
        {
            DatabasePath = databasePath;
        }

        protected T ReadRow<T>(string queryString, FillObject<T> builder, T defaultValue, params SQLiteParameter[] parameters)
        {
            using(Query query = new Query(DatabasePath, queryString, parameters)) {
                if(query.Reader.Read()) {
                    return builder(query.Reader);
                } else {
                    return defaultValue;
                }
            }
        }

        protected T ReadRow<T>(string queryString, FillObject<T> builder, T defaultValue)
        {
            return ReadRow<T>(queryString, builder, defaultValue, new SQLiteParameter[] { });
        }

        protected T ReadRow<T>(string queryString, FillObject<T> builder, params SQLiteParameter[] parameters)
        {
            return ReadRow<T>(queryString, builder, default(T), parameters);
        }

        protected T ReadRow<T>(string queryString, FillObject<T> builder)
        {
            return ReadRow<T>(queryString, builder, default(T), new SQLiteParameter[] { });
        }

        protected int ReadInt(string queryString) {
            return ReadRow(queryString, delegate(SQLiteDataReader reader)
            {
                return DatabaseHelperMethods.SafeInt32(reader, 0);
            }, 0);
        }

        protected List<T> ReadList<T>(string queryString, FillObject<T> builder, int? start, int? end, params SQLiteParameter[] parameters)
        {
            List<T> ret = new List<T>();

            using (Query query = new Query(DatabasePath, queryString, parameters))
            {
                int counter = 0;
                while (query.Reader.Read())
                {
                    // select from start to end
                    if (start.HasValue && counter < start)
                    {
                        counter++;
                        continue;
                    }
                    if (end.HasValue && counter > end) break;
                    counter++;

                    // add the item to the list
                    T item = builder(query.Reader);
                    if (item != null)
                        ret.Add(item);
                }
            }

            return ret;
        }

        protected List<T> ReadList<T>(string queryString, FillObject<T> builder, int? start, int? end)
        {
            return ReadList<T>(queryString, builder, null, null, new SQLiteParameter[] { });
        }

        protected List<T> ReadList<T>(string queryString, FillObject<T> builder)
        {
            return ReadList<T>(queryString, builder, null, null, new SQLiteParameter[] { });
        }
    }
}
