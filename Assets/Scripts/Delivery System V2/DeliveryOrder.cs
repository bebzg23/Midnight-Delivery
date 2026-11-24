public class DeliveryOrder
{
    public int Id;
    public NpcCustomer Customer;
    public ShopPoint Shop;
    public ParcelDefinition Item;
    public Parcel Instance;
    public bool Delivered;
    public int Reward;
}