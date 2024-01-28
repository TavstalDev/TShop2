using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers.General;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Compability;
using Tavstal.TShop.Helpers;
using Tavstal.TShop.Model.Interfaces;

namespace Tavstal.TShop.Managers
{
    public static class HUDManager
    {
        private static TShopConfiguration Config => TShop.Instance.Config;

        public static void Init(UnturnedPlayer player)
        {
            EffectManager.sendUIEffect(Config.EffectID, (short)Config.EffectID, player.SteamPlayer().transportConnection, true);
            Hide(player, false); // Just in case
            Translate(player);
        }

        private static void Translate(UnturnedPlayer player)
        {
            var transportCon = player.SteamPlayer().transportConnection;

            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_tshop_name", TShop.Instance.Localize("ui_shopname"));

            #region Navbar
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tshop_nav_title#market", TShop.Instance.Localize("ui_text_market"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_nav_tshop#items_name", TShop.Instance.Localize("ui_text_items"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_nav_tshop#vehicles_name", TShop.Instance.Localize("ui_text_vehicles"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_nav_tshop#basket_name", TShop.Instance.Localize("ui_text_basket"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tshop_nav_title#account", TShop.Instance.Localize("ui_text_account"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_nav_tshop#logout_name", TShop.Instance.Localize("ui_text_logout"));
            #endregion

            #region Products Content
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_title_products", TShop.Instance.Localize("ui_text_products"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_tshop_products#empty", TShop.Instance.Localize("ui_text_no_products"));
            for (int i = 0; i < 10; i++)
            {
                EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, $"tb_tshop_product#{i + 1}#add_to_basket", TShop.Instance.Localize("ui_text_add_to_basket"));
            }
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_product#category#item#all", TShop.Instance.Localize("ui_text_all"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_product#category#vehicle#all", TShop.Instance.Localize("ui_text_all"));
            #endregion

            #region Basket Content
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_title_mybasket", TShop.Instance.Localize("ui_text_my_basket"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_text_basket_icon", TShop.Instance.Localize("ui_text_icon"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_text_basket_name", TShop.Instance.Localize("ui_text_product_name"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_text_basket_quantity", TShop.Instance.Localize("ui_text_quantity"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_text_basket_price", TShop.Instance.Localize("ui_text_price"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_text_basket_actions", TShop.Instance.Localize("ui_text_actions"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_tshop_basket#empty", TShop.Instance.Localize("ui_text_basket_empty"));
            for (int i = 0; i < 14; i++)
            {
                EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, $"tb_tshop_basket#product#{i+1}#amt_placeholder", TShop.Instance.Localize("ui_text_range"));
            }
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_text_basket_buyinfo", TShop.Instance.Localize("ui_text_buy_info"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_text_basket_sellinfo", TShop.Instance.Localize("ui_text_sell_info"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_tshop_basket#buy#nocontent", TShop.Instance.Localize("ui_text_buy_disabled"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_tshop_basket#sell#nocontent", TShop.Instance.Localize("ui_text_sell_disabled"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_tshop_basket#buy", TShop.Instance.Localize("ui_text_complete_order"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_tshop_basket#sell", TShop.Instance.Localize("ui_text_complete_order"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tshop_basket#buy#subtotal", TShop.Instance.Localize("ui_text_subtotal"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tshop_basket#sell#subtotal", TShop.Instance.Localize("ui_text_subtotal"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tshop_basket#buy#discount", TShop.Instance.Localize("ui_text_discount"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tshop_basket#buy#total", TShop.Instance.Localize("ui_text_total"));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tshop_basket#sell#total", TShop.Instance.Localize("ui_text_total"));
            #endregion
        }

        public static void Show(UnturnedPlayer player, bool handleCursor = true)
        {
            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "Panel_TShop", true);
            player.GetComponent<TShopComponent>().IsUIOpened = true;
            if (handleCursor)
                player.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, true);
        }

        public static void Hide(UnturnedPlayer player, bool handleCursor = true)
        {
            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "Panel_TShop", false);
            player.GetComponent<TShopComponent>().IsUIOpened = false;
            if (handleCursor)
                player.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
        }

        public static async void UpdateProductPage(UnturnedPlayer player)
        {
            try
            {
                TShopComponent comp = player.GetComponent<TShopComponent>();

                int page = 1;
                int arrayIndex = 0;
                if (comp.IsVehiclePage)
                {
                    page = comp.PageVehicle;
                    arrayIndex = 1;
                }
                else
                    page = comp.PageItem;

                if (page < 1)
                    page = 1;

                int itemPerPage = 10;

                ITransportConnection playerTC = player.SteamPlayer().transportConnection;
                List<Product> products = await (comp.IsVehiclePage ? TShop.Database.GetVehicles(comp.VehicleFilter) : TShop.Database.GetItems(comp.ItemFilter));
                int maxPage = products.Count / itemPerPage + (products.Count % itemPerPage > 0 ? 1 : 0);
                #region Body
                int validCount = 0;
                for (int i = 0; i < itemPerPage; i++)
                {
                    int index = i + (page - 1) * itemPerPage;
                    int uiIndex = i + 1;
                    if (!products.IsValidIndex(index))
                    {
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tshop_product#{uiIndex}", false);
                        continue;
                    }

                    Product product = products.ElementAt(index);
                    if (comp.IsVehiclePage)
                    {
                        VehicleAsset vehicle = UAssetHelper.FindVehicleAsset(product.UnturnedId);
                        if (vehicle == null)
                        {
                            TShop.Logger.LogWarning($"# Failed to get the vehicle asset with {product.UnturnedId} ID.");
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tshop_product#{uiIndex}", false);
                            continue;
                        }

                        EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:1", comp.IsVehiclePage ? UnturnedHelper.GetVehicleIcon(product.UnturnedId) : UnturnedHelper.GetItemIcon(product.UnturnedId));
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:1", true);
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:2", false);
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#2:1", false);

                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_product#{uiIndex}#name", vehicle.vehicleName);

                    }
                    else
                    {
                        ItemAsset item = UAssetHelper.FindItemAsset(product.UnturnedId);
                        if (item == null)
                        {
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tshop_product#{uiIndex}", false);
                            continue;
                        }

                        #region Update Icon
                        if (item.size_x == item.size_y)
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:1", comp.IsVehiclePage ? UnturnedHelper.GetVehicleIcon(product.UnturnedId) : UnturnedHelper.GetItemIcon(product.UnturnedId));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:1", true);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:2", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#2:1", false);
                        }
                        else if (item.size_x > item.size_y)
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#2:1", comp.IsVehiclePage ? UnturnedHelper.GetVehicleIcon(product.UnturnedId) : UnturnedHelper.GetItemIcon(product.UnturnedId));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:1", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:2", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#2:1", true);
                        }
                        else
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:2", comp.IsVehiclePage ? UnturnedHelper.GetVehicleIcon(product.UnturnedId) : UnturnedHelper.GetItemIcon(product.UnturnedId));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:1", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:2", true);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#2:1", false);
                        }
                        #endregion

                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_product#{uiIndex}#name", item.itemName);
                    }

                    EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tshop_product#{uiIndex}#buycost", product.BuyCost <= 0 ? TShop.Instance.Localize("ui_product_free") : TShop.Instance.Localize("ui_product_buycost", product.BuyCost.ToString("0.00")));
                    EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tshop_product#{uiIndex}#sellcost", product.SellCost <= 0 ? TShop.Instance.Localize("ui_product_notavailable") : TShop.Instance.Localize("ui_product_sellcost", product.BuyCost.ToString("0.00")));
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tshop_product#{uiIndex}", true);
                    validCount++;
                }

                EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tb_tshop_products#empty", validCount == 0);
                #endregion

                UEffectHelper.UpdatePagination<TShopComponent>(Config.EffectID, player, "tshop_products", arrayIndex, page, maxPage);
            }
            catch (Exception ex)
            {
                TShop.Logger.LogException("Error in HUDManager: ");
                TShop.Logger.LogError(ex);
            }
        }

        public static void UpdateBasketPage(UnturnedPlayer player)
        {
            TShopComponent comp = player.GetComponent<TShopComponent>();

            try
            {
                int page = comp.PageBasket;

                if (page < 1)
                    page = 1;

                int itemPerPage = 12;

                ITransportConnection playerTC = player.SteamPlayer().transportConnection;
                int maxPage = MathHelper.Clamp(comp.Basket.Count / itemPerPage, 1, int.MaxValue);

                #region LeftBox
                #region Body
                int validCount = 0;
                for (int i = 0; i < itemPerPage; i++)
                {
                    int index = i + (page - 1) * itemPerPage;
                    int uiIndex = i + 1;
                    if (!comp.Basket.IsValidIndex(index))
                    {
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tshop_basket#product#{uiIndex}", false);
                        continue;
                    }

                    Product product = comp.Basket.Keys.ElementAt(index);
                    int amount = comp.Basket.Values.ElementAt(index);
                    if (product.IsVehicle)
                    {
                        VehicleAsset vehicle = UAssetHelper.FindVehicleAsset(product.UnturnedId);
                        if (vehicle == null)
                        {
                            TShop.Logger.LogWarning($"# Failed to get the vehicle asset with {product.UnturnedId} ID.");
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tshop_basket#product#{uiIndex}", false);
                            continue;
                        }

                        EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:1", UnturnedHelper.GetVehicleIcon(product.UnturnedId));
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:1", true);
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:2", false);
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#2:1", false);

                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_basket#product#{uiIndex}#name", vehicle.vehicleName);

                    }
                    else
                    {
                        ItemAsset item = UAssetHelper.FindItemAsset(product.UnturnedId);
                        if (item == null)
                        {
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tshop_basket#product#{uiIndex}", false);
                            continue;
                        }

                        #region Update Icon
                        if (item.size_x == item.size_y)
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:1", UnturnedHelper.GetItemIcon(product.UnturnedId));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:1", true);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:2", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#2:1", false);
                        }
                        else if (item.size_x > item.size_y)
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#2:1", UnturnedHelper.GetItemIcon(product.UnturnedId));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:1", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:2", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#2:1", true);
                        }
                        else
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:2",  UnturnedHelper.GetItemIcon(product.UnturnedId));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:1", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:2", true);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#2:1", false);
                        }
                        #endregion

                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_basket#product#{uiIndex}#name", item.itemName);
                    }

                    EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_basket#product#{uiIndex}#price", product.BuyCost <= 0 ? TShop.Instance.Localize("ui_product_free") : TShop.Instance.Localize("ui_product_buycost", product.BuyCost.ToString("0.00")));
                    EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"inputf_tshop_basket#product#{uiIndex}#amt", amount.ToString());
                    //EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tshop_basket#product#{uiIndex}#sellcost", product.SellCost <= 0 ? TShop.Instance.Localize("ui_product_notavailable") : TShop.Instance.Localize("ui_product_sellcost", product.BuyCost.ToString("0.00")));
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tshop_basket#product#{uiIndex}", true);
                    validCount++;
                }

                EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tb_tshop_basket#empty", validCount == 0);
                #endregion

                UEffectHelper.UpdatePagination<TShopComponent>(Config.EffectID, player, "tshop_basket", 2, page, maxPage);
                #endregion

                UpdateBasketPayment(player);
            }
            catch (Exception ex)
            {
                TShop.Logger.LogException("Error in HUDManager: ");
                TShop.Logger.LogError(ex);
            }
        }

        public static void UpdateBasketPayment(UnturnedPlayer player)
        {
            TShopComponent comp = player.GetComponent<TShopComponent>();
            ITransportConnection playerTC = player.SteamPlayer().transportConnection;
            decimal subtotalBuyPrice = 0;
            decimal discountBuyPrice = 0;
            decimal totalBuyPrice = 0;
            decimal subtotalSellPrice = 0;

            foreach (var elem in comp.Basket)
            {
                subtotalBuyPrice += elem.Key.BuyCost * elem.Value;
                subtotalSellPrice += elem.Key.SellCost * elem.Value;
                if (elem.Key.IsDiscounted)
                {
                    decimal discountedPrice = elem.Key.BuyCost - elem.Key.BuyCost * (elem.Key.DiscountPercent / 100);
                    discountBuyPrice += elem.Key.BuyCost - discountedPrice;
                    totalBuyPrice += elem.Value * discountedPrice;
                }
                else
                    totalBuyPrice += elem.Value * elem.Key.BuyCost;
            }

            EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_basket#buy#subtotal", TShop.Instance.Localize("ui_product_buycost", subtotalBuyPrice));
            EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_basket#buy#discount", TShop.Instance.Localize("ui_product_discount", discountBuyPrice));
            EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_basket#buy#total", TShop.Instance.Localize("ui_product_buycost", totalBuyPrice));

            EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_basket#sell#subtotal", TShop.Instance.Localize("ui_product_sellcost", subtotalSellPrice));
            EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_basket#sell#total", TShop.Instance.Localize("ui_product_sellcost", subtotalSellPrice));

        } 
    }
}
