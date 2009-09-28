using System;

using System.Collections.Generic;
using System.Text;

namespace PockeTwit
{
    static class AddressBook
    {
        
        public static string[] GetList(string startsWith)
        {
            List<string> ret = new List<string>();
            using (System.Data.SQLite.SQLiteConnection conn = LocalStorage.DataBaseUtility.GetConnection())
            {
                string SQL = "SELECT DISTINCT screenname FROM users WHERE screenname LIKE @startswith";
                using (System.Data.SQLite.SQLiteCommand comm = new System.Data.SQLite.SQLiteCommand(SQL, conn))
                {
                    comm.Parameters.Add(new System.Data.SQLite.SQLiteParameter("@startswith", startsWith + '%'));
                    conn.Open();
                    using (System.Data.SQLite.SQLiteDataReader r = comm.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            ret.Add(r.GetString(0));
                        }
                    }
                    conn.Clone();
                }
            }
            return ret.ToArray();
        }

    }
}
