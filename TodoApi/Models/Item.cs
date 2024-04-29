namespace TodoApi.Models;

public class Item
{

    public Item(string name) { Name = name; }

    public long Id { get; set; }
    public string Name { get; set; }
    public bool Done { get; set; } = false;
}
