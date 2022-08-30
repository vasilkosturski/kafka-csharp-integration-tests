namespace OrdersService;

public class Order
{
    public string Id { get; set; }
    public Product Product { get; set; }
    public double Price { get; set; }
}