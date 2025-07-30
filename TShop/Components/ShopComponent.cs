using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TShop.Models;
using Tavstal.TShop.Models.Enums;
using UnityEngine.Serialization;

namespace Tavstal.TShop.Components
{
    /// <summary>
    /// Represents a shop component for managing player interactions with the shop system.
    /// </summary>
    public class ShopComponent : UnturnedPlayerComponent, IPlayerComponent
    {
        public bool IsUIOpened {  get; set; }
        public DateTime LastButtonClick = DateTime.Now;
        public DateTime ProductRefreshTime = DateTime.Now;
        public ITransportConnection TransportConnection => Player.SteamPlayer().transportConnection;
        public EMenuCategory MenuCategory { get; set; }
        public EItemFilter? ItemFilter { get; set; }
        public ESortType SortType { get; set; }
        public string ProductSearch { get; set; }
        public bool IsProductSearchDirty { get; set; }
        public EEngine? VehicleFilter { get; set; }
        public EPaymentMethod PaymentMethod { get; set; } = EPaymentMethod.CASH;
        public int PageItem { get; set; } = 1;
        public int PageVehicle { get; set; } = 1;
        public bool IsVehiclePage { get; set; }
        public int PageBasket { get; set; } = 1;
        public List<Product> ProductsCache { get; set; } = new List<Product>();
        public readonly Dictionary<Product, byte> Basket = new Dictionary<Product, byte>();
        public bool HasActiveNotify { get; set; }
        public int[][] PageIndexes { get; set; } = new int[3][];

        [FormerlySerializedAs("NotifiesOnQueue")] 
        public List<string> notifiesOnQueue = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ShopComponent"/> class.
        /// </summary>
        public ShopComponent()
        {
            PageIndexes[0] = new int[5];
            PageIndexes[1] = new int[5];
            PageIndexes[2] = new int[5];
        }

        /// <summary>
        /// Adds a notification message to the queue and sends it if no active notification exists.
        /// </summary>
        /// <param name="message">The notification message to add.</param>
        public void AddNotifyToQueue(string message)
        {
            try
            {
                notifiesOnQueue.Add(message);

                if (!HasActiveNotify)
                {
                    HasActiveNotify = true;
                    SendNotify();
                }
            }
            catch (Exception ex)
            {
                TShop.Logger.Error(ex);
            }
        }

        /// <summary>
        /// Sends the next notification from the queue to the player.
        /// </summary>
        private void SendNotify()
        {
            try
            {
                if (!notifiesOnQueue.IsValidIndex(0))
                {
                    HasActiveNotify = false;
                    return;
                }

                string notify = notifiesOnQueue.ElementAt(0);
                if (notify != null)
                {
                    UEffectHelper.SendUIEffectText((short)TShop.Instance.Config.EffectID, TransportConnection, true, "tb_notification#1#text", notify);
                    UEffectHelper.SendUIEffectVisibility((short)TShop.Instance.Config.EffectID, TransportConnection, true, "notification#1", true);

                    TShop.Instance.InvokeAction(2f, () =>
                    {
                        UEffectHelper.SendUIEffectVisibility((short)TShop.Instance.Config.EffectID, TransportConnection, true, "notification#1", false);
                        SendNotify();
                    });
                    notifiesOnQueue.RemoveAt(0);
                }
                else
                    HasActiveNotify = false;
            }
            catch (Exception ex)
            {
                TShop.Logger.Exception("Error in Component SendNotify():");
                TShop.Logger.Error(ex);
            }
        }
    }
}
