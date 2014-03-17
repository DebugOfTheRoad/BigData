using System;
using System.Collections.Generic;
using System.Drawing;


namespace BigData
{
    public class Publication
    {
        public String title;
        public String link;
        public String desc;
        public String oclcNumber;
        public String isbn;
        public Image coverImage;
        public List<String> authors;

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

        public String printBook()
        {
            String image;
            if (coverImage != null)
                 image = "cover exists";
            else image = "cover does not exist";

            String authorsString = (authors.Count == 1) ? "\nAuthor: " : "\nAuthors: ";
            foreach (String author in authors)
            {
                authorsString += author + ", ";
            }
            authorsString = authorsString.Substring(0, authorsString.Length - 2);
            return "Publication:\nTitle: " + title + authorsString +  "\nISBN: " + isbn + "\nAccess URL:" + link +  "\nDescription: " + desc + "\nCover: " + image + "\n\n";

        }
    }
}
