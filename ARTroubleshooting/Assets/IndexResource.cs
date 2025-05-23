using System.Collections.Generic;

/* This class should be used to represent an index file internally. */
public class IndexResource
{
    public string title;
    private List<Category> categories;
    public IndexResource(string title)
    {
        this.title = title;
        this.categories = new List<Category>();
    }

    public void addCategory(Category cat)
    {
        this.categories.Add(cat);
    }

    public List<Category> GetCategories() { return this.categories; }
}

public class Category
{
    public string title;
    private bool nextCollumn;
    private Dictionary<string, string> contents;

    public Category(string title, bool nextCollumn)
    {
        this.title = title;
        this.nextCollumn = nextCollumn;
        this.contents = new Dictionary<string, string>();
    }

    public void addContent(string displayText, string filename)
    {
        this.contents.Add(displayText, filename);
    }

    public Dictionary<string, string> getContents()
    {
        return this.contents;
    }

    public bool shouldNextCollumn(){ return this.nextCollumn; }
}