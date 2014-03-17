using System;
using System.Collections.Generic;
using System.Collections;
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
        private string WSKey = "XYBOEZiodAgSpDI9gLiQcv6o4r78ZuHOELWT2c7F5F9iqIx7VXnbXrt4a2HTpUYCDSKOwoD25joHpkVy";
        private string secretKey;
        
        public Database(string feed) {
            SQLiteConnection.CreateFile("publications.db");
            sql_con = new SQLiteConnection("Data Source = publications.db; Version = 3; New = false; Compress = true");
            sql_con.Open();
            rss_feed = feed;
        }
        
        public void create_db() {
            string create_table = "CREATE TABLE Publications(" +
                                  "isbn TEXT PRIMARY KEY, " +
                                  "title TEXT, " +
                                  "link TEXT, " +
                                  "desc TEXT, " + 
                                  "date_added INT, " +
                                  "cover BLOB" +
                                  ")";
            string create_table2 = "CREATE TABLE Authors(" +
                                   "isbn TEXT, " +
                                   "author TEXT" +
                                   ")";
            this.sql_command(create_table);
            this.sql_command(create_table2);
            this.update_db();
        }
        
        public void update_db() {
            OCLCWrapper wrapper = new OCLCWrapper(this.rss_feed, this.WSKey, this.secretKey) ;
            List<Publication> pub_list = wrapper.createPublications();

            string insert_query;
            for (int i = 0; i < pub_list.Count; i++) {
                // Insert publication
                insert_query =  "INSERT INTO Publications VALUES (" + 
                                "\"" + pub_list[i].isbn + "\", " + 
                                "\"" + pub_list[i].title + "\", " +
                                "\"" + pub_list[i].link + "\", " +
                                "\"" + pub_list[i].desc + "\", " +
                                //"\"" + pub_list[i].dateAdded + "\", " +
                                "0, " +
                                "\"" + "deadbeef" + "\"" +
                                ");";
                this.sql_command(insert_query);
                
                // Insert authors
                for (int j = 0; j < pub_list[i].authors.Count; j++) {
                    insert_query = "INSERT INTO Authors VALUES (" +
                                   "\"" + pub_list[i].isbn + "\", " +
                                   "\"" + pub_list[i].authors[j] + "\"" +
                                   ");";
                    this.sql_command(insert_query);
                }
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
                s = s + "Entry: " + reader["isbn"] + 
                        "\n\tTitle:\t" + reader["title"] +
                        "\tDate:\t" + reader["date_added"] + "\n";
            }
            return s;
        }

        public ArrayList get_all_books() {
            ArrayList book_list = new ArrayList();
            int date = 0;
            string query = "SELECT * FROM Publications WHERE date_added >= " + date + ";";
            SQLiteDataReader reader = this.sql_query(query);
            while (reader.Read()) {
                Publication book = new Publication((string) reader["title"], "", "");
                book.isbn = (string) reader["isbn"];
                //book.dateAdded = (int) reader["date_added"];
                //book.coverImage = (Image) reader["cover"];

                // Get the authors
                query = "SELECT author FROM Authors WHERE isbn = \"" + book.isbn + "\";";
                SQLiteDataReader author_reader = this.sql_query(query);
                while (author_reader.Read()) {
                    book.authors.Add((string)reader["author"]);
                }

                book_list.Add(book);
            }
            return book_list;
        }

        public Publication get_isbn(string isbn) {
            Publication book = new Publication("", "", "");
            string query = "SELECT * FROM Publications WHERE isbn = \"" + isbn + "\";";
            SQLiteDataReader reader = this.sql_query(query);
            while (reader.Read()) {
                // book.ISBN = (string) reader["title"];
                book.title = (string) reader["isbn"];
                // book.dateAdded = (int) reader["date_added"];
                // book.coverImage = (Image) reader["cover"];
            }
            return book;
        }

        public void close() {
            sql_con.Close();
        }
    }
}
