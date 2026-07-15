using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rocket.API;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Extensions.General;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Threading;
using Tavstal.TShop.Components;
using Tavstal.TShop.Models;
using Tavstal.TShop.Models.Enums;
using Tavstal.TShop.Utils.Helpers;

namespace Tavstal.TShop.Utils.Managers
{
    public static class UIManager
    {
        private static ShopConfiguration Config => TShop.Instance.Config;
        
        public static void Init(UnturnedPlayer player)
        {
            UEffectHelper.SendUIEffect(Config.EffectID, (short)Config.EffectID, player.SteamPlayer().transportConnection, true);
            // Delay the packets to make sure the UI is ready.
            BackgroundThreadDispatcher.Run(async () =>
            {
                await Task.Delay(100);
                await MainThreadDispatcher.RunAsync(() =>
                {
                    Hide(player, false);
                    Translate(player);
                });
            });
        }

        
        private static void Translate(UnturnedPlayer player)
        {
            var transportCon = player.SteamPlayer().transportConnection;

            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_tshop_name", TShop.Instance.Localize("ui_shop_name"));

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
            
            #region Sort
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "inputf_product_search#placeholder", 
                TShop.Instance.Localize("ui_product_search"));
            
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_products#sort#featured", 
                TShop.Instance.Localize("ui_sort_selected", TShop.Instance.Localize("ui_sort_featured")));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_products#sort#nameaz", 
                TShop.Instance.Localize("ui_sort_unselected", TShop.Instance.Localize("ui_sort_az")));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_products#sort#nameza", 
                TShop.Instance.Localize("ui_sort_unselected",TShop.Instance.Localize("ui_sort_za")));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_products#sort#priceasc", 
                TShop.Instance.Localize("ui_sort_unselected",TShop.Instance.Localize("ui_sort_price_ascending")));
            EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, "tb_products#sort#pricedesc", 
                TShop.Instance.Localize("ui_sort_unselected",TShop.Instance.Localize("ui_sort_price_descending")));
            #endregion
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
                EffectManager.sendUIEffectText((short)Config.EffectID, transportCon, true, $"tb_tshop_basket#product#{i+1}#amt_placeholder", TShop.Instance.Localize("ui_text_range"));
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
            player.GetComponent<ShopComponent>().IsUIOpened = true;
            if (handleCursor)
                player.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, true);
        }
        
        public static void Hide(UnturnedPlayer player, bool handleCursor = true)
        {
            EffectManager.sendUIEffectVisibility((short)Config.EffectID, player.SteamPlayer().transportConnection, true, "Panel_TShop", false);
            player.GetComponent<ShopComponent>().IsUIOpened = false;
            if (handleCursor)
                player.Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
        }
        
        
        public static void UpdateProductPage(UnturnedPlayer player)
        {
            try
            {
                ShopComponent comp = player.GetComponent<ShopComponent>();
                int myGeneration = ++comp.ProductsGenerationCount;

                int page;
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
                ITransportConnection playerTc = player.SteamPlayer().transportConnection;

                BackgroundThreadDispatcher.RunAsync(async () =>
                {
                    if (comp.ProductsGenerationCount != myGeneration)
                        return;
                    
                    bool isSearchEmpty = string.IsNullOrEmpty(comp.ProductSearch);
                    var prods  = (await (comp.IsVehiclePage
                        ? TShop.DatabaseManager.GetVehiclesAsync(comp.VehicleFilter)
                        : TShop.DatabaseManager.GetItemsAsync(comp.ItemFilter))
                        ).FindAll(x => (isSearchEmpty || x.DisplayName.ContainsIgnoreCase(comp.ProductSearch)) && 
                                       (!x.HasPermission || player.HasPermission(x.Permission)));
                    List<Product> products = comp.SortType switch
                    {
                        ESortType.Featured => prods,
                        ESortType.NameAz => prods.OrderBy(x => x.DisplayName).ToList(),
                        ESortType.NameZa => prods.OrderByDescending(x => x.DisplayName).ToList(),
                        ESortType.PriceAscending => prods.OrderBy(x => x.BuyCost).ToList(),
                        ESortType.PriceDescending => prods.OrderByDescending(x => x.BuyCost).ToList(),
                        _ => prods
                    };

                    await MainThreadDispatcher.RunAsync(() =>
                    {
                        if (comp.ProductsGenerationCount != myGeneration)
                            return;
                        comp.ProductsCache = products;

                        #region Sort

                        string stateKey = comp.SortType == ESortType.Featured
                            ? "ui_sort_selected"
                            : "ui_sort_unselected";
                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true,
                            "tb_products#sort#featured",
                            TShop.Instance.Localize(stateKey, TShop.Instance.Localize("ui_sort_featured")));

                        stateKey = comp.SortType == ESortType.NameAz ? "ui_sort_selected" : "ui_sort_unselected";
                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true,
                            "tb_products#sort#nameaz",
                            TShop.Instance.Localize(stateKey, TShop.Instance.Localize("ui_sort_az")));

                        stateKey = comp.SortType == ESortType.NameZa ? "ui_sort_selected" : "ui_sort_unselected";
                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true,
                            "tb_products#sort#nameza",
                            TShop.Instance.Localize(stateKey, TShop.Instance.Localize("ui_sort_za")));

                        stateKey = comp.SortType == ESortType.PriceAscending
                            ? "ui_sort_selected"
                            : "ui_sort_unselected";
                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true,
                            "tb_products#sort#priceasc",
                            TShop.Instance.Localize(stateKey, TShop.Instance.Localize("ui_sort_price_ascending")));

                        stateKey = comp.SortType == ESortType.PriceDescending
                            ? "ui_sort_selected"
                            : "ui_sort_unselected";
                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true,
                            "tb_products#sort#pricedesc",
                            TShop.Instance.Localize(stateKey, TShop.Instance.Localize("ui_sort_price_descending")));

                        #endregion

                        int maxPage = products.Count / itemPerPage + (products.Count % itemPerPage > 0 ? 1 : 0);

                        #region Body

                        int validCount = 0;
                        for (int i = 0; i < itemPerPage; i++)
                        {
                            int index = i + (page - 1) * itemPerPage;
                            int uiIndex = i + 1;
                            if (!products.IsValidIndex(index))
                            {
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                    $"tshop_product#{uiIndex}", false);
                                continue;
                            }

                            Product product = products.ElementAt(index);
                            if (comp.IsVehiclePage)
                            {
                                VehicleAsset? vehicle = UAssetHelper.FindVehicleAsset(product.UnturnedId);
                                if (vehicle == null)
                                {
                                    TShop.Logger.Warning(
                                        $"# Failed to get the vehicle asset with {product.UnturnedId} ID.");
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                        $"tshop_product#{uiIndex}", false);
                                    continue;
                                }

                                EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTc, true,
                                    $"img_tshop_product#{uiIndex}#1:1",
                                    comp.IsVehiclePage
                                        ? UnturnedHelper.GetVehicleIcon(product.UnturnedId)
                                        : UnturnedHelper.GetItemIcon(product.UnturnedId));
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                    $"img_tshop_product#{uiIndex}#1:1", true);
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                    $"img_tshop_product#{uiIndex}#1:2", false);
                                EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                    $"img_tshop_product#{uiIndex}#2:1", false);

                                EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true,
                                    $"tb_tshop_product#{uiIndex}#name", vehicle.vehicleName);

                            }
                            else
                            {
                                ItemAsset? item = UAssetHelper.FindItemAsset(product.UnturnedId);
                                if (item == null)
                                {
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                        $"tshop_product#{uiIndex}", false);
                                    continue;
                                }

                                #region Update Icon

                                if (item.size_x == item.size_y)
                                {
                                    EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTc, true,
                                        $"img_tshop_product#{uiIndex}#1:1",
                                        comp.IsVehiclePage
                                            ? UnturnedHelper.GetVehicleIcon(product.UnturnedId)
                                            : UnturnedHelper.GetItemIcon(product.UnturnedId));
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                        $"img_tshop_product#{uiIndex}#1:1", true);
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                        $"img_tshop_product#{uiIndex}#1:2", false);
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                        $"img_tshop_product#{uiIndex}#2:1", false);
                                }
                                else if (item.size_x > item.size_y)
                                {
                                    EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTc, true,
                                        $"img_tshop_product#{uiIndex}#2:1",
                                        comp.IsVehiclePage
                                            ? UnturnedHelper.GetVehicleIcon(product.UnturnedId)
                                            : UnturnedHelper.GetItemIcon(product.UnturnedId));
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                        $"img_tshop_product#{uiIndex}#1:1", false);
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                        $"img_tshop_product#{uiIndex}#1:2", false);
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                        $"img_tshop_product#{uiIndex}#2:1", true);
                                }
                                else
                                {
                                    EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTc, true,
                                        $"img_tshop_product#{uiIndex}#1:2",
                                        comp.IsVehiclePage
                                            ? UnturnedHelper.GetVehicleIcon(product.UnturnedId)
                                            : UnturnedHelper.GetItemIcon(product.UnturnedId));
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                        $"img_tshop_product#{uiIndex}#1:1", false);
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                        $"img_tshop_product#{uiIndex}#1:2", true);
                                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                        $"img_tshop_product#{uiIndex}#2:1", false);
                                }

                                #endregion

                                EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true,
                                    $"tb_tshop_product#{uiIndex}#name", item.itemName);
                            }

                            EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true,
                                $"tshop_product#{uiIndex}#buycost",
                                product.BuyCost <= 0
                                    ? TShop.Instance.Localize("ui_product_free")
                                    : TShop.Instance.Localize("ui_product_buycost", product.BuyCost.ToString("0.00")));
                            EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true,
                                $"tshop_product#{uiIndex}#sellcost",
                                product.SellCost <= 0
                                    ? TShop.Instance.Localize("ui_product_notavailable")
                                    : TShop.Instance.Localize("ui_product_sellcost",
                                        product.SellCost.ToString("0.00")));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                                $"tshop_product#{uiIndex}", true);
                            validCount++;
                        }

                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true,
                            $"tb_tshop_products#empty", validCount == 0);

                        #endregion

                        UEffectHelper.UpdatePagination<ShopComponent>(Config.EffectID, player, "tshop_products",
                            arrayIndex, page, maxPage);
                    });
                });
            }
            catch (Exception ex)
            {
                TShop.Logger.Error("Error in HUDManager: ", ex);
            }
        }

        /// <summary>
        /// Updates the basket page for the specified Unturned player.
        /// </summary>
        /// <param name="player">The Unturned player for which to update the basket page.</param>
        public static void UpdateBasketPage(UnturnedPlayer player)
        {
            ShopComponent comp = player.GetComponent<ShopComponent>();

            try
            {
                int page = comp.PageBasket;

                if (page < 1)
                    page = 1;

                int itemPerPage = 12;

                ITransportConnection playerTc = player.SteamPlayer().transportConnection;
                int maxPage = comp.Basket.Count / itemPerPage + (comp.Basket.Count % itemPerPage > 0 ? 1 : 0);

                #region LeftBox
                #region Body
                int validCount = 0;
                for (int i = 0; i < itemPerPage; i++)
                {
                    int index = i + (page - 1) * itemPerPage;
                    int uiIndex = i + 1;
                    if (comp.Basket.Count - 1 < index)
                    {
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"tshop_basket#product#{uiIndex}", false);
                        continue;
                    }

                    Product product = comp.Basket.Keys.ElementAt(index);
                    int amount = comp.Basket.Values.ElementAt(index);
                    if (product.IsVehicle)
                    {
                        VehicleAsset? vehicle = UAssetHelper.FindVehicleAsset(product.UnturnedId);
                        if (vehicle == null)
                        {
                            TShop.Logger.Warning($"# Failed to get the vehicle asset with {product.UnturnedId} ID.");
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"tshop_basket#product#{uiIndex}", false);
                            continue;
                        }

                        EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#1:1", UnturnedHelper.GetVehicleIcon(product.UnturnedId));
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#1:1", true);
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#1:2", false);
                        EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#2:1", false);

                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true, $"tb_tshop_basket#product#{uiIndex}#name", vehicle.vehicleName);

                    }
                    else
                    {
                        ItemAsset? item = UAssetHelper.FindItemAsset(product.UnturnedId);
                        if (item == null)
                        {
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"tshop_basket#product#{uiIndex}", false);
                            continue;
                        }

                        #region Update Icon
                        if (item.size_x == item.size_y)
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#1:1", UnturnedHelper.GetItemIcon(product.UnturnedId));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#1:1", true);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#1:2", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#2:1", false);
                        }
                        else if (item.size_x > item.size_y)
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#2:1", UnturnedHelper.GetItemIcon(product.UnturnedId));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#1:1", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#1:2", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#2:1", true);
                        }
                        else
                        {
                            EffectManager.sendUIEffectImageURL((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#1:2",  UnturnedHelper.GetItemIcon(product.UnturnedId));
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#1:1", false);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#1:2", true);
                            EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"img_tshop_basket#product#{uiIndex}#2:1", false);
                        }
                        #endregion

                        EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true, $"tb_tshop_basket#product#{uiIndex}#name", item.itemName);
                    }

                    EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true, $"tb_tshop_basket#product#{uiIndex}#price", product.BuyCost <= 0 ? TShop.Instance.Localize("ui_product_free") : TShop.Instance.Localize("ui_product_buycost", product.BuyCost.ToString("0.00")));
                    EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true, $"tb_tshop_basket#product#{uiIndex}#amount", amount.ToString());
                    EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true, $"inputf_tshop_basket#product#{uiIndex}#amt", amount.ToString());
                    //EffectManager.sendUIEffectText((short)Config.EffectID, playerTC, true, $"tshop_basket#product#{uiIndex}#sellcost", product.SellCost <= 0 ? TShop.Instance.Localize("ui_product_notavailable") : TShop.Instance.Localize("ui_product_sellcost", product.BuyCost.ToString("0.00")));
                    EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"tshop_basket#product#{uiIndex}", true);
                    validCount++;
                }

                EffectManager.sendUIEffectVisibility((short)Config.EffectID, playerTc, true, $"tb_tshop_basket#empty", validCount == 0);
                #endregion

                UEffectHelper.UpdatePagination<ShopComponent>(Config.EffectID, player, "tshop_basket", 2, page, maxPage);
                #endregion

                UpdateBasketPayment(player);
            }
            catch (Exception ex)
            {
                TShop.Logger.Error("Error in HUDManager: ", ex);
            }
        }

        /// <summary>
        /// Updates the payment information on the basket page for the specified Unturned player.
        /// </summary>
        /// <param name="player">The Unturned player for which to update the basket payment information.</param>
        public static void UpdateBasketPayment(UnturnedPlayer player)
        {
            ShopComponent comp = player.GetComponent<ShopComponent>();
            ITransportConnection playerTc = player.SteamPlayer().transportConnection;
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

            EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true, $"tb_tshop_basket#buy#subtotal", TShop.Instance.Localize("ui_product_buycost", subtotalBuyPrice));
            EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true, $"tb_tshop_basket#buy#discount", TShop.Instance.Localize("ui_product_discount", discountBuyPrice));
            EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true, $"tb_tshop_basket#buy#total", TShop.Instance.Localize("ui_product_buycost", totalBuyPrice));

            EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true, $"tb_tshop_basket#sell#subtotal", TShop.Instance.Localize("ui_product_sellcost", subtotalSellPrice));
            EffectManager.sendUIEffectText((short)Config.EffectID, playerTc, true, $"tb_tshop_basket#sell#total", TShop.Instance.Localize("ui_product_sellcost", subtotalSellPrice));

        } 
    }
}
