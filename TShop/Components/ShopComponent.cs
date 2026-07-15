using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TLibrary.Threading;
using Tavstal.TShop.Models;
using Tavstal.TShop.Models.Enums;

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
        
        public int ProductsGenerationCount { get; set; } = 0;
        public int BasketGenerationCount { get; set; } = 0;
        
        public EMenuCategory MenuCategory { get; set; }
        public EItemFilter? ItemFilter { get; set; }
        public ESortType SortType { get; set; }
        public string ProductSearch { get; set; } = string.Empty;
        public bool IsProductSearchDirty { get; set; }
        public EEngine? VehicleFilter { get; set; }
        public EPaymentMethod PaymentMethod { get; set; } = EPaymentMethod.CASH;
        public int PageItem { get; set; } = 1;
        public int PageVehicle { get; set; } = 1;
        public bool IsVehiclePage { get; set; }
        public int PageBasket { get; set; } = 1;
        public List<Product> ProductsCache { get; set; } = new List<Product>();
        public readonly ConcurrentDictionary<Product, byte> Basket = new ConcurrentDictionary<Product, byte>();
        public bool HasActiveNotify { get; set; }
        public int[][] PageIndexes { get; set; } = new int[3][];

        public readonly ConcurrentQueue<string> notifiesOnQueue = new ConcurrentQueue<string>();

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
                notifiesOnQueue.Enqueue(message);

                if (!HasActiveNotify)
                {
                    HasActiveNotify = true;
                    SendNotify();
                }
            }
            catch (Exception ex)
            {
                TShop.Logger.Error("Failed to add notification to the queue.", ex);
            }
        }

        /// <summary>
        /// Sends the next notification from the queue to the player.
        /// </summary>
        private void SendNotify()
        {
            try
            {
                if (notifiesOnQueue.Count == 0)
                {
                    HasActiveNotify = false;
                    return;
                }

                notifiesOnQueue.TryDequeue(out string notify);
                if (notify != null)
                {
                    MainThreadDispatcher.Run(() =>
                    {
                        EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, TransportConnection, true, "tb_notification#1#text", notify);
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, TransportConnection, true, "notification#1", true);

                        TShop.Instance.InvokeAction(2f, () =>
                        {
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, TransportConnection, true, "notification#1", false);
                            SendNotify();
                        });
                    });
                }
                else
                    HasActiveNotify = false;
            }
            catch (Exception ex)
            {
                TShop.Logger.Error("Error in Component SendNotify():", ex);
            }
        }
    }
}
