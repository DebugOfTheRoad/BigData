using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigData;

namespace TestBigData {
    class TestDB : Test{
        public void TestDatabaseCreation() {
            var listURL = "https://bucknell.worldcat.org/profiles/danieleshleman/lists/3234701/rss";
            Database db = new Database(listURL);
            db.create_db();
            System.Console.WriteLine(db.print_all());
            System.Console.WriteLine("bitches");
            Pass(); // Look at output Muhahah
        }
    }
}
