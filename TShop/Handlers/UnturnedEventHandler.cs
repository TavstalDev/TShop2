using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Extensions.General;
using Tavstal.TLibrary.Models.Logging;
using Tavstal.TShop.Components;
using Tavstal.TShop.Handlers.Buttons;
using Tavstal.TShop.Utils.Managers;

namespace Tavstal.TShop.Handlers
{
    /// <summary>
    /// Handles various Unturned events related to the TShop.
    /// </summary>
    internal static class UnturnedEventHandler
    {
        private static bool _isAttached;
        private static readonly TLogger _logger = new TLogger(TShop.Instance, typeof(UnturnedEventHandler), TShop.Instance.GetLogLevel());

        /// <summary>
        /// Attaches event handlers to Unturned events.
        /// </summary>
        public static void Attach()
        {
            if (_isAttached)
                return;

            _isAttached = true;
            EffectManager.onEffectTextCommitted += OnInputFieldEdit;
            EffectManager.onEffectButtonClicked += OnButtonClick;
            U.Events.OnPlayerConnected += OnPlayerJoin;
            U.Events.OnPlayerDisconnected += OnPlayerLeft;
        }

        /// <summary>
        /// Detaches event handlers from Unturned events.
        /// </summary>
        public static void Detach()
        {
            if (!_isAttached)
                return;

            _isAttached = false;
            EffectManager.onEffectTextCommitted -= OnInputFieldEdit;
            EffectManager.onEffectButtonClicked -= OnButtonClick;
            U.Events.OnPlayerConnected -= OnPlayerJoin;
            U.Events.OnPlayerDisconnected -= OnPlayerLeft;
        }

        /// <summary>
        /// Event handler for player join event.
        /// Initializes the UI for the player.
        /// </summary>
        /// <param name="player">The player who joined.</param>
        private static void OnPlayerJoin(UnturnedPlayer player)
        {
            UIManager.Init(player);
            player.Player.equipment.onEquipRequested += OnPlayerEquipRequested;
        }

        /// <summary>
        /// Event handler for when a player leaves the server.
        /// Removes the event listener for equipment requests.
        /// </summary>
        /// <param name="player">The player who left the server.</param>
        private static void OnPlayerLeft(UnturnedPlayer player)
        {
            // Unsubscribe from the equipment request event for the player.
            player.Player.equipment.onEquipRequested -= OnPlayerEquipRequested;
        }

        /// <summary>
        /// Event handler for when a player attempts to equip an item.
        /// Prevents equipping if a transaction is in progress.
        /// </summary>
        /// <param name="equipment">The player's equipment instance.</param>
        /// <param name="jar">The item jar containing the item being equipped.</param>
        /// <param name="asset">The item asset being equipped.</param>
        /// <param name="shouldAllow">A reference to a boolean indicating whether the equip action should be allowed.</param>
        private static void OnPlayerEquipRequested(PlayerEquipment equipment, ItemJar jar, ItemAsset asset, ref bool shouldAllow)
        {
            // Get the UnturnedPlayer instance from the equipment's player.
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(equipment.player);

            // Retrieve the ShopComponent associated with the player.
            ShopComponent comp = player.GetComponent<ShopComponent>();
    
            // Prevent equipping if a transaction is currently in progress.
            if (comp.IsUIOpened)
                shouldAllow = false;
        }
        
        /// <summary>
        /// Event handler for input field edit event.
        /// Handles various input field edits in the TShop UI.
        /// </summary>
        /// <param name="player">The player who edited the input field.</param>
        /// <param name="button">The button identifier.</param>
        /// <param name="text">The text entered the input field.</param>
        private static void OnInputFieldEdit(Player player, string button, string text)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
            ShopComponent comp = player.GetComponent<ShopComponent>();

            if (button.StartsWith("inputf_tshop_basket#product#"))
            {
                int buttonIndex =
                    Convert.ToInt32(button.Replace("inputf_tshop_basket#product#", "").Replace("#amt", "")) - 1;

                int elementIndex = (comp.PageBasket - 1) * 12 + buttonIndex;
                if (comp.Basket.Count - 1 < elementIndex)
                    return;

                if (!byte.TryParse(text, out var v))
                    return;
                if (v > 100 || v < 1)
                    return;

                var key = comp.Basket.Keys.ElementAt(elementIndex);
                if (key.IsVehicle)
                {
                    comp.Basket[key] = 1;
                    comp.AddNotifyToQueue(TShop.Instance.Localize("ui_basket_vehicle_quantity_change_prevent"));
                    EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID,
                        uPlayer.SteamPlayer().transportConnection, true, button, "1");
                }
                else
                    comp.Basket[key] = v;

                UIManager.UpdateBasketPayment(uPlayer);

                return;
            }

            if (button.EqualsIgnoreCase("inputf_product_search"))
            {
                if (comp.ProductSearch!.EqualsIgnoreCase(text)) 
                    return;
                comp.ProductSearch = text;
                UIManager.UpdateProductPage(uPlayer);
            }
        }

        /// <summary>
        /// Event handler for button click event.
        /// Handles various button clicks in the TShop UI.
        /// </summary>
        /// <param name="player">The player who clicked the button.</param>
        /// <param name="button">The button identifier.</param>
        private static void OnButtonClick(Player player, string button)
        {
            try
            {
                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
                ShopComponent comp = player.GetComponent<ShopComponent>();
                var transportConnection = uPlayer.SteamPlayer().transportConnection;
                
                if (comp.LastButtonClick > DateTime.Now)
                    return;

                if (NavigationButtonHandler.Handle(uPlayer, transportConnection, comp, button) || 
                    ProductsButtonHandler.Handle(uPlayer, transportConnection, comp, button) || 
                    BasketButtonHandler.Handle(uPlayer, transportConnection, comp, button))
                    comp.LastButtonClick = DateTime.Now.AddSeconds(TShop.Instance.Config.UiButtonDelay);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in UEventHandler -> OnButtonClick({button}):", ex);
            }
        }
    }
}
