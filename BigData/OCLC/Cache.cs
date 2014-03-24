using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace BigData.OCLC
{
    public class Cache : PublicationSource
    {
        public void Serialize(IEnumerable<Publication> publications)
        {
            var serializer = new XmlSerializer(publications.GetType());
            using(var fs = new FileStream("cache.dat", FileMode.Create))
            {
                Console.WriteLine(fs.Name);
                serializer.Serialize(fs, publications);
            }
        }

        public Cache(string fileName)
        {
            CacheFileName = fileName;
        }

        public async Task<IEnumerable<Publication>> GetPublications()
        {
            using (var fs = new FileStream(CacheFileName, FileMode.Open))
            {
                var serializer = new XmlSerializer(
                    Type.GetType("BigData.Publication[]", true));
                return (Publication[])serializer.Deserialize(fs);
            }
        }

        public string CacheFileName { get; set; }
    }
}
