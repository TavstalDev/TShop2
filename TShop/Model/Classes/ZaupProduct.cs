using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavstal.TShop.Model.Interfaces;

namespace Tavstal.TShop.Model.Classes
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
