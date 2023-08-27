using Rocket.Unturned.Player;
using Tavstal.TShop.Compability;
using System.Collections;
using System.Collections.Generic;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TShop.Compatibility.Enums;
using SDG.Unturned;
using Tavstal.TShop.Compatibility;
using SDG.NetTransport;
using System.Linq;
using System;
using Tavstal.TLibrary.Extensions;
using System.Runtime.CompilerServices;
using Tavstal.TShop.Helpers;

namespace Tavstal.TShop
{
    public class TShopComponent : UnturnedPlayerComponent
    {
        public DateTime LastButtonClick = DateTime.Now;
        public ITransportConnection TransportConnection => Player.SteamPlayer().transportConnection;
        public EMenuCategory MenuCategory { get; set; }
        public EItemFilter? ItemFilter { get; set; }
        public EEngine? VehicleFilter { get; set; }
        public EPaymentMethod PaymentMethod { get; set; } = EPaymentMethod.WALLET;
        public int PageItem { get; set; } = 0;
        public int PageVehicle { get; set; } = 0;
        public bool IsVehiclePage { get; set; }
        public int PageBasket { get; set; } = 0;
        public int[][] PageIndexes = new int[2][];
        public List<ShopItem> Basket = new List<ShopItem>();
        public bool HasActiveNotify { get; set; }
        public List<string> NotifiesOnQueue = new List<string>();

        public TShopComponent()
        {
            PageIndexes[0] = new int[5];
            PageIndexes[1] = new int[5];
        }

        public void AddNotifyToQueue(string message)
        {
            try
            {
                NotifiesOnQueue.Add(message);

                if (!HasActiveNotify)
                {
                    HasActiveNotify = true;
                    SendNotify();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(ex);
            }
        }

        private void SendNotify()
        {
            try
            {
                if (!NotifiesOnQueue.IsValidIndex(0))
                {
                    HasActiveNotify = false;
                    return;
                }

                string notify = NotifiesOnQueue.ElementAt(0);
                if (notify != null)
                {
                    EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, TransportConnection, true, "tb_notification#1#text", notify);
                    EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, TransportConnection, true, "notification#1", true);

                    TShop.Instance.InvokeAction(2f, () =>
                    {
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, TransportConnection, true, "notification#1", false);
                        SendNotify();
                    });
                    NotifiesOnQueue.RemoveAt(0);
                }
                else
                    HasActiveNotify = false;
            }
            catch (Exception ex)
            {
                LoggerHelper.LogException("Error in Component SendNotify():");
                LoggerHelper.LogError(ex);
            }
        }
    }
}
