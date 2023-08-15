using Rocket.Unturned.Player;
using Tavstal.TShop.Compability;
using System.Collections;
using System.Collections.Generic;
using Tavstal.TLibrary.Compatibility.Economy;

namespace Tavstal.TShop
{
    public class TShopComponent : UnturnedPlayerComponent
    {
        public EPaymentMethod PaymentMethod { get; set; } = EPaymentMethod.WALLET;
        public bool IsVehiclePage { get; set; }
        public int PageItem { get; set; } = 0;
        public int PageVehicle { get; set; } = 0;
        public int PageBasket { get; set; } = 0;

        public List<Product> Products = new List<Product>();
    }
}
