using System;
using System.Collections.Generic;


namespace BigData
{
    public class Publication
    {
        public List<String> ISBNs;
        public List<String> Authors;
        public List<Publication> RelatedPublications;
        //public DateTime dateAdded;
        // public Image coverImage;

        public Publication( List<String> ISBNs, List<String> Authors, List<Publication> RelatedPublications)
        {
            this.ISBNs = ISBNs;
            this.Authors = Authors;
            this.RelatedPublications = RelatedPublications;
        }
    }
}
