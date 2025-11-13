namespace Order.Domain.Common;

public class OrderItemData
{
    public Guid ProductId { get; }
    public int Quantity { get; }

    public OrderItemData(Guid productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
}