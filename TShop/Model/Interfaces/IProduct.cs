using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavstal.TShop.Model.Interfaces
{
    public interface IProduct
    {
        ushort UnturnedId { get; set; }
        bool IsVehicle { get; set; }
        decimal BuyCost { get; set; }
        decimal SellCost { get; set; }
    }
}
