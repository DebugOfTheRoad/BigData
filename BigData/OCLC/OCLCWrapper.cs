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
        public OCLCWrapper(string rssURL)
        {
            reader = new XmlTextReader(rssURL);
        }

        public List<Publication> createPublications()
        {
            List<Publication> publications = new List<Publication>();
            var xdoc = new XDocument();
            try
            {
                xdoc = XDocument.Load(reader);
            }
            catch (System.Net.WebException e)
            {
                System.Console.WriteLine("could not connect to oclc");
                System.Console.WriteLine(e.Message);
            }
            var entries = from e in xdoc.Descendants("item")
                          select new
                          {
                              Title = (String)e.Attribute("title"),
                              Link = (String)e.Attribute("link"),
                              Description = (String)e.Element("description")
                              //TODO: get OCLC query numbers
                          };
            var array = entries.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                publications.Add(new Publication(array[i].Title, array[i].Link, array[i].Description));
            }
            System.Console.WriteLine(publications.Count);
            return publications;
        }
        
        

    }
}
