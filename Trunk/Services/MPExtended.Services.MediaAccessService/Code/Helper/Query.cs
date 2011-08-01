using System;
using System.Data.SQLite;

namespace MPExtended.Services.MediaAccessService.Code.Helper
{
    internal class Query : IDisposable
    {
        SQLiteConnection db;
        SQLiteCommand cmd;

        public SQLiteDataReader Reader
        {
            get;
            private set;
        }

        public Query(string databasePath, string query)
            : this(databasePath, query, new SQLiteParameter[] { })
        {
        }

        public Query(string databasePath, string query, SQLiteParameter[] parameters)
        {
            db = new SQLiteConnection(DatabaseHelperMethods.SQLiteConnStr(databasePath));
            db.Open();
            cmd = db.CreateCommand();
            cmd.CommandText = query;
            foreach (SQLiteParameter param in parameters)
                cmd.Parameters.Add(param);
            Reader = cmd.ExecuteReader();
        }

        public void Dispose()
        {
            Reader.Close();
            Reader.Dispose();
            cmd.Dispose();
            db.Close();
            db.Dispose();
        }
    }
}
