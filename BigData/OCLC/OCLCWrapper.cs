using System;
using System.Xml;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BigData
{

    public class OCLCWrapper
    {
        XmlTextReader reader;
        String xml;
        List<Publication> publications;
        public OCLCWrapper(string rssURL)
        {
            reader = new XmlTextReader(rssURL);
            xml = new System.Net.WebClient().DownloadString(rssURL);
        }

         public List<Publication> createPublications()
         {
            publications = new List<Publication>();
            var xdoc = XDocument.Load(reader);
            var entries = from e in xdoc.Descendants("item")
                          select new
                          {
                              Title = (String)e.Attribute("title"),
                              Link = (String)e.Attribute("link"),
                              Description = (String)e.Element("description")
                          };
            var array = entries.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                publications.Add(new Publication(array[i].Title, array[i].Link, array[i].Description));   
            }
            System.Console.WriteLine(publications.Count);
            return publications;            
        }

        public String getXML()
        {
            return xml;
        }

    }
}
