using Rocket.API;
using Rocket.Core.Plugins;
using System;
using Rocket.API.Collections;
using UnityEngine;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using System.Linq;
using Rocket.Unturned.Player;
using Rocket.Unturned.Events;
using Rocket.Unturned;
using System.Collections.Generic;
using System.IO;
using Math = System.Math;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Tavstal.TShop.Compability;
using Tavstal.TShop.Managers;

using Tavstal.TShop.Compability.Hooks;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Extensions;
using SDG.NetTransport;
using Tavstal.TLibrary.Helpers;
using System.Reflection;
using System.Numerics;
using System.Web.UI;
using Tavstal.TShop.Helpers;

namespace Tavstal.TShop.Managers
{
    public static class HUDManager
    {
        private static TShopConfiguration Config => TShop.Instance.Config;

        public static void Init(UnturnedPlayer player)
        {
            EffectManager.sendUIEffect(Config.EffectID, (short)Config.EffectID, true);
            Hide(player, false); // Just in case
        }

        public static void Show(UnturnedPlayer player, bool handleCursor = true)
        {
            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "Panel_TShop", true);
            if (handleCursor)
                player.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, true);
        }

        public static void Hide(UnturnedPlayer player, bool handleCursor = true)
        {
            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "Panel_TShop", false);
            if (handleCursor)
                player.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
        }

        public static void UpdateProductPage(UnturnedPlayer player)
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
                List<ShopItem> products = comp.IsVehiclePage ? TShop.Database.GetVehicles(comp.VehicleFilter) : TShop.Database.GetItems(comp.ItemFilter);
                int maxPage = 1 + products.Count / itemPerPage;
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

                    ShopItem product = products.ElementAt(index);
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

                UpdatePagination(player, "tshop_products", arrayIndex, page, maxPage);
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
                int maxPage = 1 + comp.Basket.Count / itemPerPage;

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

                    ShopItem product = comp.Basket.Keys.ElementAt(index);
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

                        EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:1", comp.IsVehiclePage ? UnturnedHelper.GetVehicleIcon(product.UnturnedId) : UnturnedHelper.GetItemIcon(product.UnturnedId));
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
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:1", comp.IsVehiclePage ? UnturnedHelper.GetVehicleIcon(product.UnturnedId) : UnturnedHelper.GetItemIcon(product.UnturnedId));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:1", true);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:2", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#2:1", false);
                        }
                        else if (item.size_x > item.size_y)
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#2:1", comp.IsVehiclePage ? UnturnedHelper.GetVehicleIcon(product.UnturnedId) : UnturnedHelper.GetItemIcon(product.UnturnedId));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:1", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:2", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#2:1", true);
                        }
                        else
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_basket#product#{uiIndex}#1:2", comp.IsVehiclePage ? UnturnedHelper.GetVehicleIcon(product.UnturnedId) : UnturnedHelper.GetItemIcon(product.UnturnedId));
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

                UpdatePagination(player, "tshop_basket", 2, page, maxPage);
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

        private static void UpdatePagination(UnturnedPlayer player, string uiName, int arrayIndex, int page, int maxPage)
        {
            TShopComponent comp = player.GetComponent<TShopComponent>();

            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#prev", page != 1);

            if (page == maxPage)
                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#next", false);
            else
                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#next", maxPage > 1);

            // Pages between 1 and 5
            if (maxPage <= 5)
            {
                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#dots#left", false);
                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#dots#centerleft", false);
                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#dots#centerright", false);
                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#dots#right", false);
                for (int i = 0; i < 5; i++)
                {
                    int uiIndex = i + 1;
                    if (uiIndex > maxPage)
                    {
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#{uiIndex}", false);
                        comp.PageIndexes[arrayIndex][i] = -1;
                    }
                    else
                    {
                        if (page == uiIndex)
                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#{uiIndex}", $"<color=#6C757D>{uiIndex}");
                        else
                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#{uiIndex}", (uiIndex).ToString());
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#{uiIndex}", true);
                        comp.PageIndexes[arrayIndex][i] = uiIndex;
                    }
                }
            }
            // Pages after 5
            else
            {
                // First Page Button
                if (page == 1)
                {
                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#1", $"<color=#6C757D>1");
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#1", true);
                    // Disable button
                    comp.PageIndexes[arrayIndex][0] = -1;
                }
                else
                {
                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#1", "1");
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#1", true);
                    comp.PageIndexes[arrayIndex][0] = 1;
                }

                // Button After First Page 
                if (page - 2 == 1 || page == 1)
                {
                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#2", "2");
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#2", true);
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#dots#left", false);
                    comp.PageIndexes[arrayIndex][1] = 2;
                }
                else if (page - 1 == 1)
                {
                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#2", "<color=#6C757D>2");
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#2", true);
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#dots#left", false);
                    comp.PageIndexes[arrayIndex][1] = -1;
                    // Disable button
                }
                else
                {
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#dots#left", true);
                }

                // Center 
                if (maxPage > 6)
                {
                    if (page - 3 >= 1 && page + 3 <= maxPage)
                    {
                        EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#2", (page - 1).ToString());
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#2", true);
                        comp.PageIndexes[arrayIndex][1] = page - 1;

                        EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#3", $"<color=#6C757D>{page}");
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#3", true);
                        comp.PageIndexes[arrayIndex][2] = -1;

                        EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#4", (page + 1).ToString());
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#4", true);
                        comp.PageIndexes[arrayIndex][3] = page + 1;
                    }
                    else
                    {
                        if (page <= 4)
                        {
                            if (page == 3)
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#3", $"<color=#6C757D>3");
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#3", true);
                                // Disable button
                                comp.PageIndexes[arrayIndex][2] = -1;
                            }
                            else
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#3", "3");
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#3", true);
                                comp.PageIndexes[arrayIndex][2] = 3;
                            }

                            if (page == 4)
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#4", $"<color=#6C757D>4");
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#4", true);
                                // Disable button
                                comp.PageIndexes[arrayIndex][3] = -1;
                            }
                            else
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#4", "4");
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#4", true);
                                comp.PageIndexes[arrayIndex][3] = 4;
                            }

                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#5", false);
                            comp.PageIndexes[arrayIndex][4] = -1;
                        }
                        else
                        {
                            if (page == maxPage - 3)
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#2", $"<color=#6C757D>{maxPage - 3}");
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#2", true);
                                // Disable button
                                comp.PageIndexes[arrayIndex][1] = -1;
                            }
                            else
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#2", (maxPage - 3).ToString());
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#2", true);
                                comp.PageIndexes[arrayIndex][1] = maxPage - 3;
                            }

                            if (page == maxPage - 2)
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#3", $"<color=#6C757D>{maxPage - 2}");
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#3", true);
                                // Disable button
                                comp.PageIndexes[arrayIndex][2] = -1;
                            }
                            else
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#3", (maxPage - 2).ToString());
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#3", true);
                                comp.PageIndexes[arrayIndex][2] = maxPage - 2;
                            }
                        }
                    }
                }
                else
                {
                    if (page <= 4)
                    {
                        if (page == 3)
                        {
                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#2", $"<color=#6C757D>3");
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#2", true);
                            // Disable button
                            comp.PageIndexes[arrayIndex][1] = -1;
                        }
                        else
                        {
                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#2", "3");
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#2", true);
                            comp.PageIndexes[arrayIndex][1] = 3;
                        }

                        if (page == 4)
                        {
                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#3", $"<color=#6C757D>4");
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#3", true);
                            // Disable button
                            comp.PageIndexes[arrayIndex][2] = -1;
                        }
                        else
                        {
                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#3", "4");
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#3", true);
                            comp.PageIndexes[arrayIndex][2] = 4;
                        }

                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#4", true);
                    }
                    else
                    {
                        if (page == maxPage - 3)
                        {
                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#2", (maxPage - 3).ToString());
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#2", true);
                            // Disable button
                            comp.PageIndexes[arrayIndex][1] = -1;
                        }
                        else
                        {
                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#2", (maxPage - 3).ToString());
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#2", true);
                            comp.PageIndexes[arrayIndex][1] = maxPage - 3;
                        }

                        if (page == maxPage - 2)
                        {
                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#3", (maxPage - 2).ToString());
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#3", true);
                            // Disable button
                            comp.PageIndexes[arrayIndex][2] = -1;
                        }
                        else
                        {
                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#3", (maxPage - 2).ToString());
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#3", true);
                            comp.PageIndexes[arrayIndex][2] = maxPage - 2;
                        }
                    }
                }

                // Button before Last Page
                if (page + 2 == maxPage || page == maxPage)
                {
                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#4", (maxPage - 1).ToString());
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#4", true);
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#dots#right", false);
                    comp.PageIndexes[arrayIndex][3] = maxPage - 1;
                }
                else if (page + 1 == maxPage)
                {
                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#4", $"<color=#6C757D>{page}");
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#4", true);
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#dots#right", false);
                    comp.PageIndexes[arrayIndex][3] = -1;
                }
                else if (page - 3 <= 0)
                {
                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#4", "4");
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#4", true);
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#dots#right", true);
                    comp.PageIndexes[arrayIndex][3] = 4;
                }
                else
                {
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#dots#right", true);
                }

                // Last Page Button 
                if (page == maxPage)
                {
                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#5", $"<color=#6C757D>{maxPage}");
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#5", true);
                    // Disable button
                    comp.PageIndexes[arrayIndex][4] = -1;
                }
                else
                {
                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"tb_{uiName}#page#5", maxPage.ToString());
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_{uiName}#page#5", true);
                    comp.PageIndexes[arrayIndex][4] = maxPage;
                }
            }
        }
    }
}
