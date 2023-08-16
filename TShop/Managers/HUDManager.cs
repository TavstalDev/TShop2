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
using Logger = Tavstal.TShop.Helpers.LoggerHelper;
using Tavstal.TShop.Compability.Hooks;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Extensions;
using SDG.NetTransport;
using Tavstal.TLibrary.Helpers;
using System.Reflection;
using System.Numerics;

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

                ITransportConnection playerTC = player.SteamPlayer().transportConnection;
                List<ShopItem> products = comp.IsVehiclePage ? TShop.Database.GetVehicles() : TShop.Database.GetItems();
                int maxPage = 1 + products.Count / 8;
                #region Body
                int validCount = 0;
                for (int i = 0; i < 8; i++)
                {
                    int index = i + (page - 1) * 8;
                    int uiIndex = i + 1;
                    if (!products.IsValidIndex(index))
                    {
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tshop_product#{uiIndex}", false);
                        continue;
                    }

                    ShopItem product = products.ElementAt(index);
                    if (comp.IsVehiclePage)
                    {
                        VehicleAsset vehicle = UAssetHelper.FindVehicleAsset(product.Id);
                        if (vehicle == null)
                        {
                            Logger.LogWarning($"# Failed to get the vehicle asset with {product.Id} ID.");
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tshop_product#{uiIndex}", false);
                            continue;
                        }

                        EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:1", comp.IsVehiclePage ? UnturnedHelper.GetVehicleIcon(product.Id) : UnturnedHelper.GetItemIcon(product.Id));
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:1", true);
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:2", false);
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#2:1", false);

                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_product#{uiIndex}#name", vehicle.vehicleName);

                    }
                    else
                    {
                        ItemAsset item = UAssetHelper.FindItemAsset(product.Id);
                        if (item == null)
                        {
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tshop_product#{uiIndex}", false);
                            continue;
                        }

                        #region Update Icon
                        if (item.size_x == item.size_y)
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:1", comp.IsVehiclePage ? UnturnedHelper.GetVehicleIcon(product.Id) : UnturnedHelper.GetItemIcon(product.Id));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:1", true);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:2", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#2:1", false);
                        }
                        else if (item.size_x > item.size_y)
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#2:1", comp.IsVehiclePage ? UnturnedHelper.GetVehicleIcon(product.Id) : UnturnedHelper.GetItemIcon(product.Id));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:1", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:2", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#2:1", true);
                        }
                        else
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:2", comp.IsVehiclePage ? UnturnedHelper.GetVehicleIcon(product.Id) : UnturnedHelper.GetItemIcon(product.Id));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:1", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#1:2", true);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"img_tshop_product#{uiIndex}#2:1", false);
                        }
                        #endregion

                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_product#{uiIndex}#name", item.itemName);
                    }

                    EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_product#{uiIndex}#buycost", product.BuyCost <= 0 ? TShop.Instance.Localize("ui_product_free") : TShop.Instance.Localize("ui_product_buycost", product.BuyCost.ToString("0.00")));
                    EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tb_tshop_product#{uiIndex}#sellcost", product.SellCost <= 0 ? TShop.Instance.Localize("ui_product_notavailable") : TShop.Instance.Localize("ui_product_sellcost", product.BuyCost.ToString("0.00")));
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tshop_product#{uiIndex}", true);
                    validCount++;
                }

                EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTC, true, $"tb_tshop_products#empty", validCount == 0);
                #endregion

                #region Pagination
                if (page == 1)
                {
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "bt_tshop_products#page#prev", false);
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "bt_tshop_products#page#next", maxPage > 1);
                }
                else if (page == maxPage)
                {
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "bt_tshop_products#page#prev", true);
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "bt_tshop_products#page#next", false);
                }

                // Pages between 1 and 5
                if (maxPage <= 5)
                {
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "bt_tshop_products#page#dots#left", false);
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "bt_tshop_products#page#dots#centerleft", false);
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "bt_tshop_products#page#dots#centerright", false);
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "bt_tshop_products#page#dots#right", false);
                    for (int i = 0; i < 5; i++)
                    {
                        int uiIndex = i + 1;
                        int index = i + (page - 1) * 5;
                        if (!products.IsValidIndex(index))
                        {
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#{uiIndex}", false);
                            comp.PageIndexes[arrayIndex][i] = -1;
                        }
                        else
                        {
                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#{uiIndex}", (index + 1).ToString());
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#{uiIndex}", true);
                            comp.PageIndexes[arrayIndex][i] = index;
                        }
                    }
                }
                // Pages after 5
                else
                {
                    // First Page Button
                    if (page == 1)
                    {
                        EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#1", "1");
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#1", true);
                        // Disable button
                        comp.PageIndexes[arrayIndex][0] = -1;
                    }
                    else
                    {
                        EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#1", "1");
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#1", true);
                        comp.PageIndexes[arrayIndex][0] = 1;
                    }

                    // Button After First Page 
                    if (page - 2 == 1 || page == 1)
                    {
                        EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", "2");
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", true);
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#dots#left", false);
                        comp.PageIndexes[arrayIndex][1] = 2;
                    }
                    else if (page - 1 == 1)
                    {
                        EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", "2");
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", true);
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#dots#left", false);
                        comp.PageIndexes[arrayIndex][1] = -1;
                        // Disable button
                    }
                    else
                    {
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#dots#left", true);
                    }

                    // Center 
                    if (maxPage > 6)
                    {
                        if (page - 3 >= 1 && page + 3 <= maxPage)
                        {
                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", (page - 1).ToString());
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", true);
                            comp.PageIndexes[arrayIndex][1] = page - 1;

                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", page.ToString());
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", true);
                            comp.PageIndexes[arrayIndex][2] = -1;

                            EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#4", (page + 1).ToString());
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#4", true);
                            comp.PageIndexes[arrayIndex][3] = page + 1;
                        }
                        else
                        {
                            if (page <= 4)
                            {
                                if (page == 3)
                                {
                                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", "3");
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", true);
                                    // Disable button
                                    comp.PageIndexes[arrayIndex][1] = -1;
                                }
                                else
                                {
                                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", "3");
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", true);
                                    comp.PageIndexes[arrayIndex][1] = 3;
                                }

                                if (page == 4)
                                {
                                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", "4");
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", true);
                                    // Disable button
                                    comp.PageIndexes[arrayIndex][2] = -1;
                                }
                                else
                                {
                                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", "4");
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", true);
                                    comp.PageIndexes[arrayIndex][2] = 4;
                                }

                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#5", false);
                                comp.PageIndexes[arrayIndex][4] = -1;
                            }
                            else
                            {
                                if (page == maxPage - 3)
                                {
                                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", (maxPage - 3).ToString());
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", true);
                                    // Disable button
                                    comp.PageIndexes[arrayIndex][1] = -1;
                                }
                                else
                                {
                                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", (maxPage - 3).ToString());
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", true);
                                    comp.PageIndexes[arrayIndex][1] = maxPage - 3;
                                }

                                if (page == maxPage - 2)
                                {
                                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", (maxPage - 2).ToString());
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", true);
                                    // Disable button
                                    comp.PageIndexes[arrayIndex][2] = -1;
                                }
                                else
                                {
                                    EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", (maxPage - 2).ToString());
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", true);
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
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", "3");
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", true);
                                // Disable button
                            }
                            else
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", "3");
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", true);
                            }

                            if (page == 4)
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", "4");
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", true);
                                // Disable button
                            }
                            else
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", "4");
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", true);
                            }

                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#4", true);
                        }
                        else
                        {
                            if (page == maxPage - 3)
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", (maxPage - 3).ToString());
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", true);
                                // Disable button
                                comp.PageIndexes[arrayIndex][1] = -1;
                            }
                            else
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", (maxPage - 3).ToString());
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#2", true);
                                comp.PageIndexes[arrayIndex][1] = maxPage - 3;
                            }

                            if (page == maxPage - 2)
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", (maxPage - 2).ToString());
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", true);
                                // Disable button
                                comp.PageIndexes[arrayIndex][2] = -1;
                            }
                            else
                            {
                                EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", (maxPage - 2).ToString());
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#3", true);
                                comp.PageIndexes[arrayIndex][2] = maxPage - 2;
                            }
                        }
                    }

                    // Button before Last Page
                    if (page + 2 == maxPage || page == maxPage)
                    {
                        EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#4", (maxPage - 1).ToString());
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#4", true);
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#dots#right", false);
                        comp.PageIndexes[arrayIndex][3] = maxPage - 1;
                    }
                    else if (page + 1 == maxPage)
                    {
                        EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#4", (page).ToString());
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#4", true);
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#dots#right", false);
                        comp.PageIndexes[arrayIndex][3] = -1;
                    }
                    else
                    {
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#dots#right", true);
                    }

                    // Last Page Button 
                    if (page == maxPage)
                    {
                        EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#5", maxPage.ToString());
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#5", true);
                        // Disable button
                        comp.PageIndexes[arrayIndex][4] = -1;
                    }
                    else
                    {
                        EffectManager.sendUIEffectText((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#5", maxPage.ToString());
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, $"bt_tshop_products#page#5", true);
                        comp.PageIndexes[arrayIndex][4] = maxPage;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogException("Error in HUDManager: ");
                Logger.LogError(ex);
            }
        }

        public static void UpdateBasketPage(UnturnedPlayer player)
        {
            TShopComponent comp = player.GetComponent<TShopComponent>();
        }
    }
}
