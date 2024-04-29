namespace TodoApi.Models;

public class TodoList
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public List<Item> Items { get; set; } = new List<Item>();
}
