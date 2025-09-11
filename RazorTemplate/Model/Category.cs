namespace RazorTemplate.Model;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Product> products { get; set; } = new();
}
