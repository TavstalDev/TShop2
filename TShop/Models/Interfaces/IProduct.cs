namespace Tavstal.TShop.Models.Interfaces
{
    public interface IProduct
    {
        ushort UnturnedId { get; set; }
        bool IsVehicle { get; set; }
        decimal BuyCost { get; set; }
        decimal SellCost { get; set; }
    }
}
