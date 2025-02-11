using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Components;
using Tavstal.TShop.Models;
using Tavstal.TShop.Models.Enums;
using Tavstal.TShop.Utils.Helpers;
using Tavstal.TShop.Utils.Managers;

namespace Tavstal.TShop.Utils.Handlers
{
    /// <summary>
    /// Handles various Unturned events related to the TShop.
    /// </summary>
    internal static class UnturnedEventHandler
    {
        private static bool _isAttached;

        /// <summary>
        /// Attaches event handlers to Unturned events.
        /// </summary>
        public static void Attach()
        {
            if (_isAttached)
                return;

            _isAttached = true;
            EffectManager.onEffectTextCommitted += Event_OnInputFieldEdit;
            EffectManager.onEffectButtonClicked += Event_OnButtonClick;
            U.Events.OnPlayerConnected += Event_OnPlayerJoin;
        }

        /// <summary>
        /// Detaches event handlers from Unturned events.
        /// </summary>
        public static void Detach()
        {
            if (!_isAttached)
                return;

            _isAttached = false;
            EffectManager.onEffectTextCommitted -= Event_OnInputFieldEdit;
            EffectManager.onEffectButtonClicked -= Event_OnButtonClick;
            U.Events.OnPlayerConnected -= Event_OnPlayerJoin;
        }

        /// <summary>
        /// Event handler for player join event.
        /// Initializes the UI for the player.
        /// </summary>
        /// <param name="player">The player who joined.</param>
        private static void Event_OnPlayerJoin(UnturnedPlayer player)
        {
            UIManager.Init(player);
        }

        /// <summary>
        /// Event handler for input field edit event.
        /// Handles various input field edits in the TShop UI.
        /// </summary>
        /// <param name="player">The player who edited the input field.</param>
        /// <param name="button">The button identifier.</param>
        /// <param name="text">The text entered in the input field.</param>
        private static void Event_OnInputFieldEdit(Player player, string button, string text)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
            ShopComponent comp = player.GetComponent<ShopComponent>();

            if (button.StartsWith("inputf_tshop_basket#product#"))
            {
                int buttonIndex =
                    Convert.ToInt32(button.Replace("inputf_tshop_basket#product#", "").Replace("#amt", "")) - 1;

                int elementIndex = (comp.PageBasket - 1) * 12 + buttonIndex;
                if (!comp.Basket.IsValidIndex(elementIndex))
                    return;

                if (!int.TryParse(text, out int v))
                    return;
                if (v > 100 || v < 1)
                    return;

                var key = comp.Basket.Keys.ElementAt(elementIndex);
                if (key.IsVehicle)
                {
                    comp.Basket[key] = 1;
                    comp.AddNotifyToQueue(TShop.Instance.Localize("ui_basket_vehicle_quantity_change_prevent"));
                    UEffectHelper.SendUIEffectText((short)TShop.Instance.Config.EffectID,
                        uPlayer.SteamPlayer().transportConnection, true, button, "1");
                }
                else
                    comp.Basket[key] = v;

                UIManager.UpdateBasketPayment(uPlayer);

                return;
            }

            if (button.EqualsIgnoreCase("inputf_product_search"))
            {
                if (comp.ProductSearch.EqualsIgnoreCase(text)) 
                    return;
                comp.ProductSearch = text;
                Task.Run(async () => await UIManager.UpdateProductPage(uPlayer));
            }
        }

        /// <summary>
        /// Event handler for button click event.
        /// Handles various button clicks in the TShop UI.
        /// </summary>
        /// <param name="player">The player who clicked the button.</param>
        /// <param name="button">The button identifier.</param>
        private static void Event_OnButtonClick(Player player, string button)
        {
            try
            {
                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
                ShopComponent comp = player.GetComponent<ShopComponent>();
                var playerTc = uPlayer.SteamPlayer().transportConnection;

                if (comp.LastButtonClick > DateTime.Now)
                    return;

                comp.LastButtonClick = DateTime.Now.AddSeconds(TShop.Instance.Config.UIButtonDelay);
                Task.Run(async () =>
                {
                    switch (button.ToLower())
                    {
                        case "bt_nav_tshop_items":
                        {
                            if (comp.MenuCategory == EMenuCategory.ProductItems)
                                return;

                            comp.MenuCategory = EMenuCategory.ProductItems;
                            comp.IsVehiclePage = false;
                            await UIManager.UpdateProductPage(uPlayer);
                            UEffectHelper.SendUIEffectVisibility((short)TShop.Instance.Config.EffectID, playerTc, true,
                                "tshop_products", true);
                            UEffectHelper.SendUIEffectVisibility((short)TShop.Instance.Config.EffectID, playerTc, true,
                                "tshop_basket", false);
                            return;
                        }
                        case "bt_nav_tshop_vehicles":
                        {
                            if (comp.MenuCategory == EMenuCategory.ProductVehicles)
                                return;

                            comp.MenuCategory = EMenuCategory.ProductVehicles;
                            comp.IsVehiclePage = true;
                            await UIManager.UpdateProductPage(uPlayer);
                            UEffectHelper.SendUIEffectVisibility((short)TShop.Instance.Config.EffectID, playerTc, true,
                                "tshop_products", true);
                            UEffectHelper.SendUIEffectVisibility((short)TShop.Instance.Config.EffectID, playerTc, true,
                                "tshop_basket", false);
                            return;
                        }
                        case "bt_nav_tshop_basket":
                        {
                            if (comp.MenuCategory == EMenuCategory.Basket)
                                return;

                            comp.MenuCategory = EMenuCategory.Basket;
                            UIManager.UpdateBasketPage(uPlayer);
                            UEffectHelper.SendUIEffectVisibility((short)TShop.Instance.Config.EffectID, playerTc, true,
                                "tshop_products", false);
                            UEffectHelper.SendUIEffectVisibility((short)TShop.Instance.Config.EffectID, playerTc, true,
                                "tshop_basket", true);
                            return;
                        }
                        case "bt_nav_tshop_logout":
                        {
                            UIManager.Hide(uPlayer);
                            return;
                        }
                        case "bt_tshop_products#page#prev":
                        {
                            if (comp.IsVehiclePage && comp.PageVehicle > 1)
                                comp.PageVehicle--;
                            if (!comp.IsVehiclePage && comp.PageItem > 1)
                                comp.PageItem--;

                            await UIManager.UpdateProductPage(uPlayer);
                            return;
                        }
                        case "bt_product#search":
                        {
                            if (!comp.IsProductSearchDirty)
                                return;

                            await UIManager.UpdateProductPage(uPlayer);
                            comp.IsProductSearchDirty = false;
                            break;
                        }
                        case "bt_products#sort#featured":
                        {
                            if (comp.SortType != ESortType.Featured)
                            {
                                comp.SortType = ESortType.Featured;
                                await UIManager.UpdateProductPage(uPlayer);
                            }

                            break;
                        }
                        case "bt_products#sort#nameaz":
                        {
                            if (comp.SortType != ESortType.NameAz)
                            {
                                comp.SortType = ESortType.NameAz;
                                await UIManager.UpdateProductPage(uPlayer);
                            }

                            break;
                        }
                        case "bt_products#sort#nameza":
                        {
                            if (comp.SortType != ESortType.NameZa)
                            {
                                comp.SortType = ESortType.NameZa;
                                await UIManager.UpdateProductPage(uPlayer);
                            }

                            break;
                        }
                        case "bt_products#sort#priceasc":
                        {
                            if (comp.SortType != ESortType.PriceAscending)
                            {
                                comp.SortType = ESortType.PriceAscending;
                                await UIManager.UpdateProductPage(uPlayer);
                            }

                            break;
                        }
                        case "bt_products#sort#pricedesc":
                        {
                            if (comp.SortType != ESortType.PriceDescending)
                            {
                                comp.SortType = ESortType.PriceDescending;
                                await UIManager.UpdateProductPage(uPlayer);
                            }

                            break;
                        }
                        case "bt_tshop_products#page#next":
                        {
                            if (comp.IsVehiclePage)
                                comp.PageVehicle++;
                            else
                                comp.PageItem++;

                            await UIManager.UpdateProductPage(uPlayer);
                            return;
                        }
                        case "bt_tshop_basket#page#prev":
                        {
                            if (comp.PageBasket > 1)
                                comp.PageBasket--;

                            UIManager.UpdateBasketPage(uPlayer);
                            return;
                        }
                        case "bt_tshop_basket#page#next":
                        {
                            comp.PageBasket++;

                            UIManager.UpdateBasketPage(uPlayer);
                            return;
                        }
                        case "bt_product#category#item#all":
                        case "bt_product#category#item#cloth":
                        case "bt_product#category#item#food":
                        case "bt_product#category#item#medical":
                        case "bt_product#category#item#tool":
                        case "bt_product#category#item#barricade":
                        case "bt_product#category#item#structure":
                        case "bt_product#category#item#electronic":
                        case "bt_product#category#item#vehicle":
                        case "bt_product#category#item#fuel":
                        case "bt_product#category#item#melee":
                        case "bt_product#category#item#gun":
                        case "bt_product#category#item#attachment":
                        case "bt_product#category#item#misc":
                        {
                            if (button.EndsWith("all"))
                                comp.ItemFilter = null;
                            else if (button.EndsWith("cloth"))
                                comp.ItemFilter = EItemFilter.Clothing;
                            else if (button.EndsWith("food"))
                                comp.ItemFilter = EItemFilter.Food;
                            else if (button.EndsWith("medical"))
                                comp.ItemFilter = EItemFilter.Medical;
                            else if (button.EndsWith("tool"))
                                comp.ItemFilter = EItemFilter.Tools;
                            else if (button.EndsWith("barricade"))
                                comp.ItemFilter = EItemFilter.Barricades;
                            else if (button.EndsWith("structure"))
                                comp.ItemFilter = EItemFilter.Structures;
                            else if (button.EndsWith("electronic"))
                                comp.ItemFilter = EItemFilter.Electronic;
                            else if (button.EndsWith("vehicle"))
                                comp.ItemFilter = EItemFilter.Vehicles;
                            else if (button.EndsWith("fuel"))
                                comp.ItemFilter = EItemFilter.Fuel;
                            else if (button.EndsWith("melee"))
                                comp.ItemFilter = EItemFilter.Melees;
                            else if (button.EndsWith("gun"))
                                comp.ItemFilter = EItemFilter.Guns;
                            else if (button.EndsWith("attachment"))
                                comp.ItemFilter = EItemFilter.Attachments;
                            else if (button.EndsWith("misc"))
                                comp.ItemFilter = EItemFilter.Misc;

                            comp.PageItem = 1;
                            await UIManager.UpdateProductPage(uPlayer);
                            return;
                        }
                        case "bt_product#category#vehicle#all":
                        case "bt_product#category#vehicle#car":
                        case "bt_product#category#vehicle#plane":
                        case "bt_product#category#vehicle#heli":
                        case "bt_product#category#vehicle#blimp":
                        case "bt_product#category#vehicle#boat":
                        case "bt_product#category#vehicle#train":
                        {
                            if (button.EndsWith("all"))
                                comp.VehicleFilter = null;
                            else if (button.EndsWith("car"))
                                comp.VehicleFilter = EEngine.CAR;
                            else if (button.EndsWith("plane"))
                                comp.VehicleFilter = EEngine.PLANE;
                            else if (button.EndsWith("heli"))
                                comp.VehicleFilter = EEngine.HELICOPTER;
                            else if (button.EndsWith("blimp"))
                                comp.VehicleFilter = EEngine.BLIMP;
                            else if (button.EndsWith("boat"))
                                comp.VehicleFilter = EEngine.BOAT;
                            else if (button.EndsWith("train"))
                                comp.VehicleFilter = EEngine.TRAIN;

                            comp.PageVehicle = 1;
                            await UIManager.UpdateProductPage(uPlayer);
                            return;
                        }
                        case "bt_tshop_basket#buy":
                        {
                            List<KeyValuePair<Product, int>> toRemove = new List<KeyValuePair<Product, int>>();
                            foreach (var prod in comp.Basket)
                            {
                                decimal cost = prod.Key.GetBuyCost(prod.Value);
                                if (prod.Key.IsVehicle)
                                {
                                    VehicleAsset asset = UAssetHelper.FindVehicleAsset(prod.Key.UnturnedId);
                                    if (asset == null)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_vehicle_not_exists",
                                            prod.Key.UnturnedId));
                                        continue;
                                    }

                                    if (await TShop.EconomyProvider.GetBalanceAsync(uPlayer.CSteamID) < cost)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_balance_not_enough",
                                            cost.ToString("0.00"), TShop.EconomyProvider.GetCurrencyName()));
                                        continue;
                                    }

                                    if (cost == 0)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_vehicle_buy_error"));
                                        continue;
                                    }

                                    await TShop.EconomyProvider.WithdrawAsync(uPlayer.CSteamID, cost);
                                    await MainThreadDispatcher.RunOnMainThreadAsync(() =>
                                    {
                                        InteractableVehicle vehicle = UnturnedHelper.SpawnOwnedVehicle(prod.Key.UnturnedId, uPlayer);;

                                        if (!prod.Key.VehicleColor.IsNullOrEmpty())
                                            vehicle.ServerSetPaintColor(prod.Key.GetVehicleColor());
                                    });
                                    

                                    if (TShop.EconomyProvider.HasTransactionSystem())
                                        await TShop.EconomyProvider.AddTransactionAsync(uPlayer.CSteamID, new Transaction(Guid.NewGuid().ToString(), ETransaction.PURCHASE,
                                                comp.PaymentMethod, TShop.Instance.Localize(true, "ui_shopname"),
                                                uPlayer.CSteamID.m_SteamID, 0, cost, DateTime.Now));
                                    comp.AddNotifyToQueue(TShop.Instance.Localize("ui_success_vehicle_buy",
                                        asset.vehicleName, prod.Value, cost, TShop.EconomyProvider.GetCurrencyName()));
                                    toRemove.Add(prod);
                                }
                                else
                                {
                                    ItemAsset asset = UAssetHelper.FindItemAsset(prod.Key.UnturnedId);
                                    if (asset == null)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_item_not_found",
                                            prod.Key.UnturnedId));
                                        continue;
                                    }

                                    if (await TShop.EconomyProvider.GetBalanceAsync(uPlayer.CSteamID) < cost)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_balance_not_enough",
                                            cost.ToString("0.00"), TShop.EconomyProvider.GetCurrencyName()));
                                        continue;
                                    }

                                    if (cost == 0)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_item_buy_error"));
                                        continue;
                                    }

                                    await TShop.EconomyProvider.WithdrawAsync(uPlayer.CSteamID, cost);
                                    await MainThreadDispatcher.RunOnMainThreadAsync(() =>
                                    {
                                        for (int i = 0; i < prod.Value; i++)
                                        {
                                            if (!uPlayer.Inventory.tryAddItem(new Item(prod.Key.UnturnedId, true), false))
                                                ItemManager.dropItem(new Item(prod.Key.UnturnedId, true), uPlayer.Position,
                                                    true, true, false);
                                        }
                                    });

                                    if (TShop.EconomyProvider.HasTransactionSystem())
                                        await TShop.EconomyProvider.AddTransactionAsync(uPlayer.CSteamID,
                                            new Transaction(Guid.NewGuid().ToString(), ETransaction.PURCHASE,
                                                comp.PaymentMethod, TShop.Instance.Localize(true, "ui_shopname"),
                                                uPlayer.CSteamID.m_SteamID, 0, cost, DateTime.Now));
                                    comp.AddNotifyToQueue(TShop.Instance.Localize("ui_success_item_buy", asset.itemName,
                                        prod.Value, cost, TShop.EconomyProvider.GetCurrencyName()));
                                    toRemove.Add(prod);
                                }
                            }

                            if (toRemove.Count > 0)
                            {
                                foreach (var elem in toRemove)
                                    comp.Basket.Remove(elem.Key);
                                UIManager.UpdateBasketPage(uPlayer);
                            }

                            break;
                        }
                        case "bt_tshop_basket#sell":
                        {
                            List<KeyValuePair<Product, int>> toRemove = new List<KeyValuePair<Product, int>>();

                            foreach (var prod in comp.Basket)
                            {
                                if (prod.Key.SellCost <= 0)
                                    continue;

                                decimal cost = prod.Key.GetSellCost(prod.Value);
                                if (prod.Key.IsVehicle)
                                {
                                    InteractableVehicle vehicle = uPlayer.CurrentVehicle;
                                    if (vehicle == null)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_vehicle_sell_null"));
                                        continue;
                                    }

                                    if (vehicle.lockedOwner != uPlayer.CSteamID || !vehicle.isLocked || vehicle.isDead)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_vehicle_sell_owner"));
                                        continue;
                                    }

                                    if (vehicle.id != prod.Key.UnturnedId)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_vehicle_not_found"));
                                        continue;
                                    }

                                    VehicleManager.askVehicleDestroy(vehicle);
                                    await TShop.EconomyProvider.DepositAsync(uPlayer.CSteamID, cost);
                                    if (TShop.EconomyProvider.HasTransactionSystem())
                                        await TShop.EconomyProvider.AddTransactionAsync(uPlayer.CSteamID,
                                            new Transaction(Guid.NewGuid().ToString(), ETransaction.SALE,
                                                comp.PaymentMethod, TShop.Instance.Localize(true, "ui_shopname"), 0,
                                                uPlayer.CSteamID.m_SteamID, cost, DateTime.Now));
                                    comp.AddNotifyToQueue(TShop.Instance.Localize("ui_success_vehicle_sell",
                                        vehicle.asset.vehicleName, 1, cost, TShop.EconomyProvider.GetCurrencyName()));
                                }
                                else
                                {
                                    ItemAsset asset = UAssetHelper.FindItemAsset(prod.Key.UnturnedId);

                                    if (asset == null)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_item_not_found",
                                            prod.Key.UnturnedId));
                                        continue;
                                    }

                                    List<InventorySearch> search =
                                        uPlayer.Inventory.search(prod.Key.UnturnedId, true, true);
                                    if (search.Count < prod.Value)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_item_not_enough"));
                                        continue;
                                    }

                                    if (cost == 0)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_item_sell_error"));
                                        continue;
                                    }

                                    await TShop.EconomyProvider.DepositAsync(uPlayer.CSteamID, cost);
                                    for (int i = 0; i < prod.Value; i++)
                                    {
                                        uPlayer.Inventory.removeItem(search[i].page,
                                            uPlayer.Inventory.getIndex(search[i].page, search[i].jar.x,
                                                search[i].jar.y));
                                    }

                                    if (TShop.EconomyProvider.HasTransactionSystem())
                                        await TShop.EconomyProvider.AddTransactionAsync(uPlayer.CSteamID,
                                            new Transaction(Guid.NewGuid().ToString(), ETransaction.SALE,
                                                comp.PaymentMethod, TShop.Instance.Localize(true, "ui_shopname"), 0,
                                                uPlayer.CSteamID.m_SteamID, cost, DateTime.Now));
                                    comp.AddNotifyToQueue(TShop.Instance.Localize("ui_success_item_sell",
                                        asset.itemName, prod.Value, cost, TShop.EconomyProvider.GetCurrencyName()));
                                    toRemove.Add(prod);
                                }
                            }

                            if (toRemove.Count > 0)
                            {
                                foreach (var elem in toRemove)
                                    comp.Basket.Remove(elem.Key);
                                UIManager.UpdateBasketPage(uPlayer);
                            }

                            break;
                        }
                    }
                    
                    if (button.StartsWith("bt_tshop_products#page#"))
                    {
                        if (button.Contains("dots"))
                            return;

                        int btIndex = int.Parse(button.Replace("bt_tshop_products#page#", "")) - 1;
                        int arrayIndex = comp.IsVehiclePage ? 1 : 0;

                        int page = comp.PageIndexes[arrayIndex][btIndex];

                        if (page != -1)
                        {
                            if (comp.IsVehiclePage)
                                comp.PageVehicle = page;
                            else
                                comp.PageItem = page;

                            await UIManager.UpdateProductPage(uPlayer);
                        }
                        return;
                    }
                    if (button.StartsWith("bt_tshop_basket#page#"))
                    {
                        if (button.Contains("dots"))
                            return;

                        int btIndex = int.Parse(button.Replace("bt_tshop_basket#page#", "")) - 1;
                        int page = comp.PageIndexes[2][btIndex];

                        if (page != -1)
                        {
                            comp.PageBasket = page;

                            UIManager.UpdateBasketPage(uPlayer);
                        }
                        return;
                    }
                    if (button.StartsWith("bt_tshop_product#"))
                    {
                        int index = (Convert.ToInt32(button.Replace("bt_tshop_product#", "")) - 1) +
                                    10 * ((comp.IsVehiclePage ? comp.PageVehicle : comp.PageItem) - 1);
                        if (!comp.ProductsCache.IsValidIndex(index))
                            return;

                        Product item = comp.ProductsCache[index];
                        if (comp.Basket.Any(x =>
                                x.Key.UnturnedId == item.UnturnedId && x.Key.IsVehicle == item.IsVehicle))
                        {
                            comp.AddNotifyToQueue(TShop.Instance.Localize("ui_basket_contains_product_already",
                                item.GetName()));
                            return;
                        }

                        if (item.IsVehicle && comp.Basket.Any(x => x.Key.IsVehicle))
                        {
                            comp.AddNotifyToQueue(TShop.Instance.Localize("ui_basket_contains_vehicle_already",
                                item.GetName()));
                            return;
                        }

                        comp.Basket.Add(item, 1);
                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_basket_product_added", item.GetName()));

                        UIManager.UpdateBasketPage(uPlayer);
                        return;
                    }

                    if (button.StartsWith("bt_tshop_basket#product#"))
                    {
                        if (button.EndsWith("#delete"))
                        {
                            int buttonIndex =
                                Convert.ToInt32(button.Replace("bt_tshop_basket#product#", "").Replace("#delete", "")) -
                                1;

                            int elementIndex = (comp.PageBasket - 1) * 12 + buttonIndex;
                            if (!comp.Basket.IsValidIndex(elementIndex))
                                return;

                            var key = comp.Basket.Keys.ElementAt(elementIndex);
                            comp.Basket.Remove(key);

                            UIManager.UpdateBasketPage(uPlayer);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                TShop.Logger.LogException($"Error in UEventHandler -> OnButtonClick({button}):");
                TShop.Logger.LogError(ex);
            }
        }
    }
}
