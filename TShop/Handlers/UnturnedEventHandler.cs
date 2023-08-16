using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers;
using Tavstal.TShop.Compability;
using Tavstal.TShop.Managers;
using Tavstal.TShop.Compatibility.Enums;

namespace Tavstal.TShop.Handlers
{
    internal static class UnturnedEventHandler
    {
        private static bool _isAttached = false;

        public static void Attach()
        {
            if (_isAttached)
                return;

            _isAttached = true;
            EffectManager.onEffectTextCommitted += Event_OnInputFieldEdit;
            EffectManager.onEffectButtonClicked += Event_OnButtonClick;
            U.Events.OnPlayerConnected += Event_OnPlayerJoin;
        }

        public static void Unattach()
        {
            if (!_isAttached)
                return;

            _isAttached = false;
            EffectManager.onEffectTextCommitted -= Event_OnInputFieldEdit;
            EffectManager.onEffectButtonClicked -= Event_OnButtonClick;
            U.Events.OnPlayerConnected -= Event_OnPlayerJoin;
        }

        private static void Event_OnPlayerJoin(UnturnedPlayer player)
        {
            HUDManager.Init(player);
        }

        private static void Event_OnInputFieldEdit(Player player, string button, string text)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
            TShopComponent comp = player.GetComponent<TShopComponent>();

            
        }

        private static void Event_OnButtonClick(Player player, string button)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
            TShopComponent comp = player.GetComponent<TShopComponent>();
            var playerTC = uPlayer.SteamPlayer().transportConnection;

            switch (button.ToLower())
            {
                case "bt_nav_tshop_items":
                    {
                        if (comp.MenuCategory == EMenuCategory.ProductItems)
                            return;

                        comp.MenuCategory = EMenuCategory.ProductItems;
                        comp.IsVehiclePage = false;
                        HUDManager.UpdateProductPage(uPlayer);
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, playerTC, true, "tshop_products", true);
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, playerTC, true, "tshop_basket", false);
                        return;
                    }
                case "bt_nav_tshop_vehicles":
                    {
                        if (comp.MenuCategory == EMenuCategory.ProductVehicles)
                            return;

                        comp.MenuCategory = EMenuCategory.ProductVehicles;
                        comp.IsVehiclePage = true;
                        HUDManager.UpdateProductPage(uPlayer);
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, playerTC, true, "tshop_products", true);
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, playerTC, true, "tshop_basket", false);
                        return;
                    }
                case "bt_nav_tshop_basket":
                    {
                        if (comp.MenuCategory == EMenuCategory.Basket)
                            return;

                        comp.MenuCategory = EMenuCategory.Basket;
                        HUDManager.UpdateBasketPage(uPlayer);
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, playerTC, true, "tshop_products", false);
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, playerTC, true, "tshop_basket", true);
                        return;
                    }
                case "bt_nav_tshop_logout":
                    {
                        HUDManager.Hide(uPlayer);
                        return;
                    }
                case "bt_tshop_products#page#prev":
                    {
                        if (comp.IsVehiclePage && comp.PageVehicle > 1)
                            comp.PageVehicle--;
                        if (!comp.IsVehiclePage && comp.PageItem > 1)
                            comp.PageItem--;

                        HUDManager.UpdateProductPage(uPlayer);
                        return;
                    }
                case "bt_tshop_products#page#next":
                    {
                        if (comp.IsVehiclePage)
                            comp.PageVehicle++;
                        else
                            comp.PageItem++;

                        HUDManager.UpdateProductPage(uPlayer);
                        return;
                    }
            }

            if (button.StartsWith("bt_tshop_products#page#"))
            {
                int btIndex = int.Parse(button.Replace("bt_tshop_products#page#", "")) - 1;
                int arrayIndex = comp.IsVehiclePage ? 1 : 0;

                int page = comp.PageIndexes[arrayIndex][btIndex];

                if (page != -1)
                {
                    if (comp.IsVehiclePage)
                        comp.PageVehicle = page;
                    else
                        comp.PageItem = page;

                    HUDManager.UpdateProductPage(uPlayer);
                }

                return;
            }
        }
    }
}
