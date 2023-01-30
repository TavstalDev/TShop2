using Rocket.Unturned.Player;
using Tavstal.TShop.Compability;
using System.Collections;
using System.Collections.Generic;

namespace Tavstal.TShop
{
    public class TShopComponent : UnturnedPlayerComponent
    {
        public EPaymentMethod PaymentMethod { get; set; } = EPaymentMethod.wallet;
        public int item_page { get; set; } = 0;
        public int vehicle_page { get; set; } = 0;
        public int cart_page { get; set; } = 0;

        public List<Product> products = new List<Product>();
    }
}
