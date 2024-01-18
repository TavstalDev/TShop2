namespace Tavstal.TShop.Compability
{
    public class Product
    {
        public ushort Id { get; set; }
        public int Amount { get; set; }
        public bool IsVehicle { get; set; }

        public Product() { }

        public Product(ushort id, int amount, bool vehicle)
        {
            Id = id;
            Amount = amount;
            IsVehicle = vehicle;
        }
    }
}