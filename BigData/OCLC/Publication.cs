using System;
using System.Collections.Generic;


namespace BigData
{
    public class Publication
    {
        //public String ISBN;
        public String title;
        public String link;
        public String desc;
        // public DateTime dateAdded;
        // public Image coverImage;

        public Publication(String title, String link, String desc)
        {
            this.title = title;
            this.link = link;
            this.desc = desc;
        }
    }
}
