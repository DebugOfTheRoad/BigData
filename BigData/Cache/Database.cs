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
    /// <summary>
    /// Manage access to the database.
    /// </summary>
    public class Database : PublicationSource {
        private SQLiteConnection SQLiteConnection;
        private SQLiteCommand SQLiteCommand;
        //private SQLiteDataAdapter db;
        private string rssFeed;
        private string wsKey;
        private uint count;
        
        /// <summary>
        /// Create a database instance.
        /// </summary>
        /// <param name="key">The WSKey required to access the database.</param>
        /// <param name="feed">The rss feed that will be passed to the OCLC client.</param>
        public Database(string key, string feed) {
            var source = String.Format("Data Source = {0}; Version = 3; New = false; Compress = true", GetDatabasePath());
            SQLiteConnection = new SQLiteConnection(source);
            SQLiteConnection.Open();
            rssFeed = feed;
            wsKey = key;
        }

        /// <summary>
        /// Returns the location of the database file in the file system.
        /// </summary>
        /// <returns>The path to the database file as a string.</returns>
        private string GetDatabasePath() {
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BigData");

            if (!Directory.Exists(appData)) {
                Directory.CreateDirectory(appData);
            }

            return Path.Combine(appData, "publications.db");
        }
        
        /// <summary>
        /// Creates the Publication and Author tables.
        /// </summary>
        public void create_db() {
            //SQLiteConnection.CreateFile(GetDatabasePath());

            string PubTable = "CREATE TABLE Publications(" +
                                  "isbn TEXT, " +
                                  "title TEXT, " +
                                  "link TEXT, " +
                                  "desc TEXT, " + 
                                  "cover BLOB" +
                                  ")";
            string AuthorTable = "CREATE TABLE Authors(" +
                                   "isbn TEXT, " +
                                   "author TEXT" +
                                   ")";
            this.ExecuteSQLiteCommand(PubTable);
            this.ExecuteSQLiteCommand(AuthorTable);
        }
        
        /// <summary>
        /// Gets publication data from the OCLC client and stores it into the database.
        /// </summary>
        /// <returns>The number of publications entered as an unsigned int.</returns>
        public async Task<uint> update_db() {
            Client oclc = new Client(this.wsKey, this.rssFeed) ;
            var pub_list = await oclc.GetPublications();
            this.count = 0;

            string InsertQuery;
            foreach (var pub in pub_list) {
                // Insert publication
                InsertQuery =  "INSERT INTO Publications VALUES (" + 
                                "(@isbn), " +
                                "(@title), " +
                                "(@link), " +
                                "(@desc), " +
                                "(@cover)" +
                                ");";
                
                // Adding parameters
                SQLiteCommand = new SQLiteCommand(InsertQuery, SQLiteConnection);
                SQLiteCommand.Parameters.Add(new SQLiteParameter("@isbn", pub.ISBNs[0]));
                SQLiteCommand.Parameters.Add(new SQLiteParameter("@title", pub.Title));
                SQLiteCommand.Parameters.Add(new SQLiteParameter("@link", "google.com"));
                SQLiteCommand.Parameters.Add(new SQLiteParameter("@desc", pub.Description));

                // Adding cover to query
                byte[] cover = BitmapToByteArray(pub.CoverImage);
                SQLiteCommand.Parameters.Add(new SQLiteParameter("@cover", cover));
                SQLiteCommand.ExecuteNonQuery();

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

                this.count++;
            }

            return this.count;
        }

        /// <summary>
        /// Executes a non-query type SQLite command.
        /// </summary>
        /// <param name="cmd">The SQLite command as a string.</param>
        private void ExecuteSQLiteCommand(string cmd) {
            SQLiteCommand = new SQLiteCommand(cmd, SQLiteConnection);
            SQLiteCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a SQLite query.
        /// </summary>
        /// <param name="query">The Sqlite query as a string</param>
        /// <returns>A SQLiteDataReader with the results from the query.</returns>
        private SQLiteDataReader ExecuteSQLiteQuery(string query) {
            SQLiteCommand = new SQLiteCommand(query, SQLiteConnection);
            return SQLiteCommand.ExecuteReader();
        }

        /// <summary>
        /// Converts a BitmapImage into an array of bytes.
        /// </summary>
        /// <param name="img">The image to be converted.</param>
        /// <returns>The resulting byte array.</returns>
        private static byte[] BitmapToByteArray(BitmapImage img) {
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

        /// <summary>
        /// Used for printing the contents of the database.
        /// </summary>
        /// <returns>A formatted string of the database.</returns>
        public string PrintDatabase() {
            string s = "";
            string query = "SELECT isbn FROM Publications;";
            SQLiteDataReader reader = this.ExecuteSQLiteQuery(query);
            while (reader.Read()) {
                s = s + "Entry: " + reader["isbn"] + 
                        "\n\tTitle:\t" + reader["title"] + "\n";
            }

            return s;
        }
        
        /// <summary>
        /// Pulls all information from all publications from the database.
        /// </summary>
        /// <returns>An array of the complete list of publications.</returns>
        public async Task<IEnumerable<Publication>> GetPublications() {
            var PubList = new List<Publication>();

            //int i = 0;
            string query = "SELECT * FROM Publications;";
            SQLiteDataReader reader = this.ExecuteSQLiteQuery(query);
        
            while (reader.Read()) {
                Publication pub = new Publication();
                pub.Title = (string) reader["title"];
                pub.ISBNs = new List<string>();
                pub.ISBNs.Add((string) reader["isbn"]);
                
                Console.WriteLine(pub.Title);

                // Get the cover
                MemoryStream ms = new MemoryStream((byte[]) reader["cover"]);
                pub.CoverImage = new BitmapImage();
                pub.CoverImage.BeginInit();
                pub.CoverImage.StreamSource = ms;
                pub.CoverImage.EndInit();
                pub.CoverImage.Freeze();

                // Get the authors
                /*query = "SELECT author FROM Authors WHERE isbn = \"" + pub.isbn + "\";";
                SQLiteDataReader author_reader = this.sql_query(query);
                while (author_reader.Read()) {
                    pub.authors.Add((string)reader["author"]);
                }*/

                //pub_list[i] = pub;
                PubList.Add(pub);
                //i++;
            }
            
            return PubList;
        }

        /// <summary>
        /// Closes the connection to the SQLite database.
        /// </summary>
        public void closeDatabase() {
            SQLiteConnection.Close();
        }
    }
}
