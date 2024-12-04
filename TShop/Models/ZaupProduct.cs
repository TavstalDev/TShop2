using Tavstal.TShop.Models.Interfaces;

namespace Tavstal.TShop.Models
{
    public class ZaupProduct : IProduct
    {
        public ushort UnturnedId { get; set; }
        public bool IsVehicle { get; set; }
        public decimal BuyCost { get; set; }
        public decimal SellCost { get; set; }

        public ZaupProduct() { }

        public ZaupProduct(ushort unturnedId, bool isVehicle, decimal buyCost, decimal sellCost)
        {
            UnturnedId = unturnedId;
            IsVehicle = isVehicle;
            BuyCost = buyCost;
            SellCost = sellCost;
        }
    }
}
