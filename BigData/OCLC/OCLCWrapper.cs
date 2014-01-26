using System;
using System.Xml;
using System.Net;
using System.Collections.Generic;

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
            return publications;            
        }

        public String getXML()
        {
            return xml;
        }

    }
}
