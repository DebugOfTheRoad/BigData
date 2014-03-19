using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigData.OCLC
{
    interface PublicationSource
    {
        IEnumerable<Publication> GetPublications();
    }
}
