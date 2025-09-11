namespace RazorTemplate.Model;

public class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Price { get; set; }
    public Category category { get; set; }
}
