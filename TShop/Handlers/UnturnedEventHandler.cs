using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Compability;
using Tavstal.TShop.Compatibility.Enums;
using Tavstal.TShop.Managers;

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

            if (button.StartsWith("inputf_tshop_basket#product#"))
            {
                int buttonIndex = Convert.ToInt32(button.Replace("inputf_tshop_basket#product#", "").Replace("#amt", "")) - 1;

                int elementIndex = (comp.PageBasket - 1) * 12 + buttonIndex;
                if (!comp.Basket.IsValidIndex(elementIndex))
                    return;

                if (int.TryParse(text, out int v))
                {
                    if (v > 100 || v < 0)
                        return;

                    var key = comp.Basket.Keys.ElementAt(elementIndex);
                    if (key.IsVehicle)
                    {
                        comp.Basket[key] = 1;
                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_basket_vehicle_quantity_change_prevent"));
                        EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, uPlayer.SteamPlayer().transportConnection, true, button, "1");
                    }
                    else
                        comp.Basket[key] = v;

                    HUDManager.UpdateBasketPayment(uPlayer);
                }
            }
        }

        private static void Event_OnButtonClick(Player player, string button)
        {
            try
            {
                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
                TShopComponent comp = player.GetComponent<TShopComponent>();
                var playerTC = uPlayer.SteamPlayer().transportConnection;

                if (comp.LastButtonClick > DateTime.Now)
                    return;

                comp.LastButtonClick = DateTime.Now.AddSeconds(TShop.Instance.Config.UIButtonDelay);

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
                    case "bt_tshop_basket#page#prev":
                        {
                            if (comp.PageBasket > 1)
                                comp.PageBasket--;

                            HUDManager.UpdateBasketPage(uPlayer);
                            return;
                        }
                    case "bt_tshop_basket#page#next":
                        {
                            comp.PageBasket++;

                            HUDManager.UpdateBasketPage(uPlayer);
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
                            HUDManager.UpdateProductPage(uPlayer);
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
                            HUDManager.UpdateProductPage(uPlayer);
                            return;
                        }
                    case "bt_tshop_basket#buy":
                        {
                            List<KeyValuePair<ShopItem, int>> toRemove = new List<KeyValuePair<ShopItem, int>>();
                            foreach (var prod in comp.Basket)
                            {
                                decimal cost = prod.Key.GetBuyCost(prod.Value);
                                if (prod.Key.IsVehicle)
                                {
                                    VehicleAsset asset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, prod.Key.UnturnedId);
                                    if (asset == null)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_vehicle_not_exists", prod.Key.UnturnedId));
                                        continue;
                                    }

                                    if (TShop.economyProvider.GetBalance(uPlayer) < cost)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_balance_not_enough"));
                                        continue;
                                    }

                                    if (cost == 0)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_vehicle_buy_error"));
                                        continue;
                                    }

                                    TShop.economyProvider.Withdraw(uPlayer, cost);
                                    VehicleManager.spawnLockedVehicleForPlayerV2(asset.id, uPlayer.Position + new UnityEngine.Vector3(0, 0, 5), uPlayer.Player.transform.rotation, uPlayer.Player);
                                    TShop.economyProvider.AddTransaction(uPlayer, new Transaction(ETransaction.PURCHASE, comp.PaymentMethod, TShop.Instance.Localize(true, "ui_shopname"), uPlayer.CSteamID.m_SteamID, 0, cost, DateTime.Now));
                                    comp.AddNotifyToQueue(TShop.Instance.Localize("ui_success_vehicle_buy", asset.vehicleName, prod.Value, cost, TShop.economyProvider.GetConfigValue<string>("MoneySymbol")));
                                }
                                else
                                {
                                    ItemAsset asset = UAssetHelper.FindItemAsset(prod.Key.UnturnedId);
                                    if (asset == null)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_item_not_found", prod.Key.UnturnedId));
                                        continue;
                                    }

                                    if (TShop.economyProvider.GetBalance(uPlayer) < cost)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_balance_not_enough"));
                                        continue;
                                    }

                                    if (cost == 0)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_item_buy_error"));
                                        continue;
                                    }

                                    TShop.economyProvider.Withdraw(uPlayer, cost);
                                    for (int i = 0; i < prod.Value; i++)
                                    {
                                        if (!uPlayer.Inventory.tryAddItem(new Item(asset.id, true), false))
                                            ItemManager.dropItem(new Item(asset.id, true), uPlayer.Position, true, true, false);
                                    }
                                    TShop.economyProvider.AddTransaction(uPlayer, new Transaction(ETransaction.PURCHASE, comp.PaymentMethod, TShop.Instance.Localize(true, "ui_shopname"), uPlayer.CSteamID.m_SteamID, 0, cost, DateTime.Now));
                                    comp.AddNotifyToQueue(TShop.Instance.Localize("ui_success_item_buy", asset.itemName, prod.Value, cost, TShop.economyProvider.GetConfigValue<string>("MoneySymbol")));
                                    toRemove.Add(prod);
                                }
                            }

                            if (toRemove.Count > 0)
                            {
                                foreach (var elem in toRemove)
                                    comp.Basket.Remove(elem.Key);
                                HUDManager.UpdateBasketPage(uPlayer);
                            }
                            break;
                        }
                    case "bt_tshop_basket#sell":
                        {
                            List<KeyValuePair<ShopItem, int>> toRemove = new List<KeyValuePair<ShopItem, int>>();

                            foreach (var prod in comp.Basket)
                            {
                                if (prod.Key.SellCost <= 0)
                                    continue;

                                decimal cost = prod.Key.GetSellCost(prod.Value);
                                if (prod.Key.IsVehicle)
                                {
                                    VehicleAsset asset = null;
                                    InteractableVehicle vehicle = uPlayer.CurrentVehicle;
                                    if (vehicle == null)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_vehicle_sell_null"));
                                        continue;
                                    }
                                    else if (vehicle.lockedOwner != uPlayer.CSteamID || !vehicle.isLocked || vehicle.isDead)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_vehicle_sell_owner"));
                                        continue;
                                    }
                                    else if (vehicle.id != prod.Key.UnturnedId)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_vehicle_not_found"));
                                        continue;
                                    }

                                    TShop.economyProvider.Deposit(uPlayer, cost);
                                    foreach (Passenger pas in vehicle.passengers)
                                    {
                                        VehicleManager.forceRemovePlayer(vehicle, pas.player.playerID.steamID);
                                    }
                                    VehicleManager.askVehicleDestroy(vehicle);
                                    TShop.economyProvider.AddTransaction(uPlayer, new Transaction(ETransaction.SALE, comp.PaymentMethod, TShop.Instance.Localize(true, "ui_shopname"), 0, uPlayer.CSteamID.m_SteamID, cost, DateTime.Now));
                                    comp.AddNotifyToQueue(TShop.Instance.Localize("ui_success_vehicle_sell", asset.vehicleName, 1, cost, TShop.economyProvider.GetConfigValue<string>("MoneySymbol")));
                                }
                                else
                                {
                                    ItemAsset asset = UAssetHelper.FindItemAsset(prod.Key.UnturnedId);

                                    if (asset == null)
                                    {
                                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_error_item_not_found", prod.Key.UnturnedId));
                                        continue;
                                    }

                                    List<InventorySearch> search = uPlayer.Inventory.search(asset.id, true, true);
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

                                    TShop.economyProvider.Deposit(uPlayer, cost);
                                    for (int i = 0; i < prod.Value; i++)
                                    {
                                        uPlayer.Inventory.removeItem(search[i].page, uPlayer.Inventory.getIndex(search[i].page, search[i].jar.x, search[i].jar.y));
                                    }
                                    TShop.economyProvider.AddTransaction(uPlayer, new Transaction(ETransaction.SALE, comp.PaymentMethod, TShop.Instance.Localize(true, "ui_shopname"), 0, uPlayer.CSteamID.m_SteamID, cost, DateTime.Now));
                                    comp.AddNotifyToQueue(TShop.Instance.Localize("ui_success_item_sell", asset.itemName, prod.Value, cost, TShop.economyProvider.GetConfigValue<string>("MoneySymbol")));
                                    toRemove.Add(prod);
                                }
                            }

                            if (toRemove.Count > 0)
                            {
                                foreach (var elem in toRemove)
                                    comp.Basket.Remove(elem.Key);
                                HUDManager.UpdateBasketPage(uPlayer);
                            }
                            break;
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
                else if (button.StartsWith("bt_tshop_basket#page#"))
                {
                    int btIndex = int.Parse(button.Replace("bt_tshop_basket#page#", "")) - 1;
                    int page = comp.PageIndexes[2][btIndex];

                    if (page != -1)
                    {
                        comp.PageBasket = page;

                        HUDManager.UpdateBasketPage(uPlayer);
                    }

                    return;
                }
                else if (button.StartsWith("bt_tshop_product#"))
                {
                    int index = (Convert.ToInt32(button.Replace("bt_tshop_product#", "")) - 1) + 10 * ((comp.IsVehiclePage ? comp.PageVehicle : comp.PageItem) - 1);
                    List<ShopItem> products = comp.IsVehiclePage ? TShop.Database.GetVehicles(comp.VehicleFilter) : TShop.Database.GetItems(comp.ItemFilter);

                    if (!products.IsValidIndex(index))
                        return;

                    ShopItem item = products[index];
                    if (comp.Basket.Any(x => x.Key.UnturnedId == item.UnturnedId && x.Key.IsVehicle == item.IsVehicle))
                    {
                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_basket_contains_product_already", item.GetName()));
                        return;
                    }

                    if (item.IsVehicle && comp.Basket.Any(x => x.Key.IsVehicle))
                    {
                        comp.AddNotifyToQueue(TShop.Instance.Localize("ui_basket_contains_vehicle_already", item.GetName()));
                        return;
                    }

                    comp.Basket.Add(item, 1);
                    comp.AddNotifyToQueue(TShop.Instance.Localize("ui_basket_product_added", item.GetName()));
                }
                else if (button.StartsWith("bt_tshop_basket#product#"))
                {
                    if (button.EndsWith("#delete"))
                    {
                        int buttonIndex = Convert.ToInt32(button.Replace("bt_tshop_basket#product#", "").Replace("#delete", "")) - 1;

                        int elementIndex = (comp.PageBasket - 1) * 12 + buttonIndex;
                        if (!comp.Basket.IsValidIndex(elementIndex))
                            return;

                        var key = comp.Basket.Keys.ElementAt(elementIndex);
                        comp.Basket.Remove(key);

                        HUDManager.UpdateBasketPage(uPlayer);
                    }
                }
            }
            catch (Exception ex)
            {
                TShop.Logger.LogException("Error in UEventHandler:");
                TShop.Logger.LogError(ex);
            }
        }
    }
}
