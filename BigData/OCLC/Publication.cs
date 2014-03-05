using System;
using System.Collections.Generic;
using System.Drawing;


namespace BigData
{
    public class Publication
    {
        //public String ISBN;
        public String title;
        public String link;
        public String desc;
        public String oclcNumber;
        public String isbn;
        public DateTime dateAdded;
        public Image coverImage;

        public Publication()
        {
            title = "";
            link = "";
            desc = "";
            oclcNumber = "";
        }

        public Publication(String number)
        {
            this.oclcNumber = number;
            
        }

        public Publication(String title, String link, String desc)
        {
            this.title = title;
            this.link = link;
            this.desc = desc;
        }
    }
}
