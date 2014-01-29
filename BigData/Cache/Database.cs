using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace BigData.Cache {
    public class Database {
        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        //private SQLiteDataAdapter db;
        
        public Database() {
            sql_con = new SQLiteConnection("Data Source = publications.db, Version = 3, New = false, Compress = true");
        }
        
        private void create_db() {
            string create_table = "CREATE TABLE Publications(title char(64), link char(64), Description char(128))";
            this.query(create_table);
            this.update_db();
        }
        
        private void update_db() {
            sql_con.Open();
        }

        private void query(string query) {
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = query;
            sql_cmd.ExecuteNonQuery();
            sql_con.Close();
        }
    }
}
