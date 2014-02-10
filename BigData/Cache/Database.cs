using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace BigData {
    public class Database {
        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        //private SQLiteDataAdapter db;
        private string rss_feed;
        
        public Database(string feed) {
            SQLiteConnection.CreateFile("publications.db");
            sql_con = new SQLiteConnection("Data Source = publications.db; Version = 3; New = false; Compress = true");
            sql_con.Open();
            rss_feed = feed;
        }
        
        public void create_db() {
            string create_table = "CREATE TABLE Publications(title TEXT)";
            this.sql_command(create_table);
            this.update_db();
        }
        
        public void update_db() {
            OCLCWrapper wrapper = new OCLCWrapper(this.rss_feed);
            List<Publication> pub_list = wrapper.createPublications();

            string insert_query;
            for (int i = 0; i < pub_list.Count; i++) {
                insert_query =  "INSERT INTO Publications VALUES (\"" + pub_list[i].title + "\");";
                this.sql_command(insert_query);
            }
        }

        private void sql_command(string cmd) {
            sql_cmd = new SQLiteCommand(cmd, sql_con);
            sql_cmd.ExecuteNonQuery();
        }

        private SQLiteDataReader sql_query(string query) {
            sql_cmd = new SQLiteCommand(query, sql_con);
            SQLiteDataReader reader = sql_cmd.ExecuteReader();
            return reader;
        }

        public string print_all() {
            string s = "";
            string query = "SELECT * FROM Publications;";
            SQLiteDataReader reader = this.sql_query(query);
            while (reader.Read()) {
                s = s + "Entry: " + reader["title"] + "\n";
            }
            return s;
        }

        public void close() {
            sql_con.Close();
        }
    }
}
