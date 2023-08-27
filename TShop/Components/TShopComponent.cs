using Rocket.Unturned.Player;
using Tavstal.TShop.Compability;
using System.Collections;
using System.Collections.Generic;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TShop.Compatibility.Enums;
using SDG.Unturned;

namespace Tavstal.TShop
{
    public class TShopComponent : UnturnedPlayerComponent
    {
        public EMenuCategory MenuCategory { get; set; }
        public EItemFilter? ItemFilter { get; set; }
        public EEngine? VehicleFilter { get; set; }
        public EPaymentMethod PaymentMethod { get; set; } = EPaymentMethod.WALLET;
        public bool IsVehiclePage { get; set; }
        public int PageItem { get; set; } = 0;
        public int PageVehicle { get; set; } = 0;
        public int PageBasket { get; set; } = 0;
        public int[][] PageIndexes = new int[2][];

        public TShopComponent()
        {
            PageIndexes[0] = new int[5];
            PageIndexes[1] = new int[5];
        }
    }
}
