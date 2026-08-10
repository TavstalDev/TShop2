using System;
using System.Linq;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tavstal.RocketFlow.Attributes;
using Tavstal.RocketFlow.Core;
using Tavstal.RocketFlow.Events.Effect;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Extensions.General;
using Tavstal.TLibrary.Models.Logging;
using Tavstal.TShop.Components;
using Tavstal.TShop.Handlers.Buttons;
using Tavstal.TShop.Utils.Managers;
// ReSharper disable UnusedMember.Global

namespace Tavstal.TShop.Handlers
{
    public class EffectEventListener : EventListener
    {
        private static readonly TLogger _logger = new TLogger(TShop.Instance, typeof(EffectEventListener), TShop.Instance.GetLogLevel());
        
        [EventHandler]
        public void OnButtonClicked(EffectButtonEvent e)
        {
            string button = e.ButtonName;
            try
            {
                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(e.Player);
                ShopComponent? comp = ComponentManager.Get(uPlayer);
                if (comp == null)
                    return;
                
                if (comp.LastButtonClick > DateTime.Now)
                    return;

                var transportConnection = uPlayer.SteamPlayer().transportConnection;
                if (NavigationButtonHandler.Handle(uPlayer, transportConnection, comp, button) ||
                    ProductsButtonHandler.Handle(uPlayer, transportConnection, comp, button) ||
                    BasketButtonHandler.Handle(uPlayer, transportConnection, comp, button))
                {
                    comp.LastButtonClick = DateTime.Now.AddSeconds(TShop.Instance.Config.UiButtonDelay);
                    e.IsCancelled = true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in EffectEventListener -> OnButtonClick({button}):", ex);
            }
        }

        [EventHandler]
        public void OnText(EffectTextEvent e)
        {
            bool shouldCancel = false;
            string button = e.ButtonName;
            string text = e.Text;
            try
            {
                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(e.Player);
                ShopComponent? comp = ComponentManager.Get(uPlayer);
                if (comp == null)
                    return;

                if (button.StartsWith("inputf_tshop_basket#product#"))
                {
                    shouldCancel = true;
                    int buttonIndex = Convert.ToInt32(button.Replace("inputf_tshop_basket#product#", "").Replace("#amt", "")) - 1;
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
                    shouldCancel = true;
                    if (comp.ProductSearch.EqualsIgnoreCase(text)) 
                        return;
                    comp.ProductSearch = text;
                    UIManager.UpdateProductPage(uPlayer);
                }
            }
            finally
            {
                e.IsCancelled = shouldCancel;
            }
        }
    }
}