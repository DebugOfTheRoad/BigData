using System;
using urllib;
using Publication;

public class OCLCWrapper
{
    String feed;
	public OCLCWrapper(string rss)
	{
        feed = urlopen(rss);

	}

    List<Publication> PublicationsByISBN(String isbn)
    {

    }
}
