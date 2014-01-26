using System;

public class Publication    
{
    public List<String> ISBNs;
    public List<String> Authors;
    public List<Publication> RelatedPublications;
    public DateTime dateAdded;
    // public Image coverImage;

	public Publication(String isbn, String title, List<String> author) 
	{
        this.isbn = isbn;
        this.title = title;
        this.author = author;
	}
}
