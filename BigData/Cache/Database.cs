using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;


namespace BigData.OCLC {
    public class Database : PublicationSource {
        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        //private SQLiteDataAdapter db;
        private string rss_feed;
        private string WSKey = "XYBOEZiodAgSpDI9gLiQcv6o4r78ZuHOELWT2c7F5F9iqIx7VXnbXrt4a2HTpUYCDSKOwoD25joHpkVy";
        private string secretKey;
        private uint count;
        
        public Database(string feed) {
            SQLiteConnection.CreateFile("publications.db");
            sql_con = new SQLiteConnection("Data Source = publications.db; Version = 3; New = false; Compress = true");
            sql_con.Open();
            rss_feed = feed;
        }
        
        public void create_db() {
            string create_table = "CREATE TABLE Publications(" +
                                  "isbn TEXT, " +
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
        
        public async void update_db() {
            Client oclc = new Client(this.secretKey, this.rss_feed) ;
            var pub_list = await oclc.GetPublications();
            count = 0;

            string insert_query;
            foreach (var pub in pub_list) {
                // Insert publication
                insert_query =  "INSERT INTO Publications VALUES (" + 
                                "\"" + pub.ISBNs + "\", " + 
                                "\"" + pub.Title + "\", " +
                                //"\"" + pub.link + "\", " +
                                "\"google.com" + 
                                "\"" + pub.Description + "\", " +
                                //"\"" + pub.dateAdded + "\", " + 
                                "0, " +
                                "(@cover)" +
                                ");";
                
                // Adding cover to query
                byte[] cover = image_to_byte_array(pub.CoverImage);
                sql_cmd = new SQLiteCommand(insert_query, sql_con);
                sql_cmd.Parameters.Add(new SQLiteParameter("@cover", cover));
                sql_cmd.ExecuteNonQuery();

                // Dan is gon learn today... bout this long dick
                //if (pub_list[i].authors == null) continue;

                // Insert authors
                /*for (int j = 0; j < pub_list[i].authors.Count; j++) {
                    insert_query = "INSERT INTO Authors VALUES (" +
                                   "\"" + pub_list[i].isbn + "\", " +
                                   "\"" + pub_list[i].authors[j] + "\"" +
                                   ");";
                    this.sql_command(insert_query);
                }*/

                count++;
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

        private static byte[] image_to_byte_array(BitmapImage img) {
            try {
                MemoryStream ms = new MemoryStream();
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(ms);
                return ms.ToArray();
            } catch (NullReferenceException e) {
                Console.WriteLine("No Image Found");
                return new byte[0];
            }
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

        public async Task<IEnumerable<Publication>> GetPublications() {
            Publication[] pub_list = new Publication[count];
            int date = 0;
            int i = 0;
            string query = "SELECT * FROM Publications WHERE date_added >= " + date + ";";
            SQLiteDataReader reader = this.sql_query(query);
            while (reader.Read()) {
                Publication pub = new Publication();
                pub.Title = (string) reader["title"];
                pub.ISBNs[0] = (string) reader["isbn"];
                //book.dateAdded = (int) reader["date_added"];
                
                // Get the cover
                MemoryStream ms = new MemoryStream((byte[]) reader["cover"]);
                pub.CoverImage.BeginInit();
                pub.CoverImage.StreamSource = ms;
                pub.CoverImage.EndInit();

                // Get the authors
                /*query = "SELECT author FROM Authors WHERE isbn = \"" + pub.isbn + "\";";
                SQLiteDataReader author_reader = this.sql_query(query);
                while (author_reader.Read()) {
                    pub.authors.Add((string)reader["author"]);
                }*/

                pub_list[i] = pub;
                i++;
            }
            return pub_list;
        }

        public void close() {
            sql_con.Close();
        }
    }
}
