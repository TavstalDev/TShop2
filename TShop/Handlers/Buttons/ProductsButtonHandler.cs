using System;
using System.Linq;
using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using Tavstal.TLibrary.Extensions.General;
using Tavstal.TShop.Components;
using Tavstal.TShop.Models;
using Tavstal.TShop.Models.Enums;
using Tavstal.TShop.Utils.Managers;

namespace Tavstal.TShop.Handlers.Buttons
{
    public static class ProductsButtonHandler
    {
        public static bool Handle(UnturnedPlayer player, ITransportConnection transportConnection, ShopComponent component, string button)
        {
            switch (button)
            {
                case "bt_tshop_products#page#prev":
                {
                    switch (component.IsVehiclePage)
                    {
                        case true when component.PageVehicle > 1:
                            component.PageVehicle--;
                            break;
                        case false when component.PageItem > 1:
                            component.PageItem--;
                            break;
                    }
                    UIManager.UpdateProductPage(player);
                    break;
                }
                case "bt_tshop_products#page#next":
                {
                    if (component.IsVehiclePage)
                        component.PageVehicle++;
                    else
                        component.PageItem++;
                    UIManager.UpdateProductPage(player);
                    break;
                }
                case "bt_tshop_products#page#1":
                case "bt_tshop_products#page#2":
                case "bt_tshop_products#page#3":
                case "bt_tshop_products#page#4":
                case "bt_tshop_products#page#5":
                {
                    int btIndex = int.Parse(button.Replace("bt_tshop_products#page#", "")) - 1;
                    int arrayIndex = component.IsVehiclePage ? 1 : 0;

                    int page = component.PageIndexes[arrayIndex][btIndex];

                    if (page == -1)
                        break;
                    if (component.IsVehiclePage)
                        component.PageVehicle = page;
                    else
                        component.PageItem = page;

                    UIManager.UpdateProductPage(player);
                    break;
                }
                case "bt_product#search":
                {
                    if (!component.IsProductSearchDirty)
                        break;
                    UIManager.UpdateProductPage(player);
                    component.IsProductSearchDirty = false;
                    break;
                }
                case "bt_products#sort#featured":
                    UpdateSorting(player, component, ESortType.Featured);
                    break;
                case "bt_products#sort#nameaz":
                    UpdateSorting(player, component, ESortType.NameAz);
                    break;
                case "bt_products#sort#nameza":
                    UpdateSorting(player, component, ESortType.NameZa);
                    break;
                case "bt_products#sort#priceasc":
                    UpdateSorting(player, component, ESortType.PriceAscending);
                    break;
                case "bt_products#sort#pricedesc":
                    UpdateSorting(player, component, ESortType.PriceDescending);
                    break;
                case "bt_product#category#item#all":
                    UpdateProductItemFilter(player, component, null);
                    break;
                case "bt_product#category#item#cloth":
                    UpdateProductItemFilter(player, component, EItemFilter.CLOTHING);
                    break;
                case "bt_product#category#item#food":
                    UpdateProductItemFilter(player, component, EItemFilter.FOOD);
                    break;
                case "bt_product#category#item#medical":
                    UpdateProductItemFilter(player, component, EItemFilter.MEDICAL);
                    break;
                case "bt_product#category#item#tool":
                    UpdateProductItemFilter(player, component, EItemFilter.TOOL);
                    break;
                case "bt_product#category#item#barricade":
                    UpdateProductItemFilter(player, component, EItemFilter.BARRICADE);
                    break;
                case "bt_product#category#item#structure":
                    UpdateProductItemFilter(player, component, EItemFilter.STRUCTURE);
                    break;
                case "bt_product#category#item#electronic":
                    UpdateProductItemFilter(player, component, EItemFilter.ELECTRONIC);
                    break;
                case "bt_product#category#item#vehicle":
                    UpdateProductItemFilter(player, component, EItemFilter.VEHICLE);
                    break;
                case "bt_product#category#item#fuel":
                    UpdateProductItemFilter(player, component, EItemFilter.FUEL);
                    break;
                case "bt_product#category#item#melee":
                    UpdateProductItemFilter(player, component, EItemFilter.MELEE);
                    break;
                case "bt_product#category#item#gun":
                    UpdateProductItemFilter(player, component, EItemFilter.GUN);
                    break;
                case "bt_product#category#item#attachment":
                    UpdateProductItemFilter(player, component, EItemFilter.ATTACHMENT);
                    break;
                case "bt_product#category#item#misc":
                    UpdateProductItemFilter(player, component, EItemFilter.MISC);
                    break;
                case "bt_product#category#vehicle#all":
                    UpdateProductVehicleFilter(player, component, null);
                    break;
                case "bt_product#category#vehicle#car":
                    UpdateProductVehicleFilter(player, component, EEngine.CAR);
                    break;
                case "bt_product#category#vehicle#plane":
                    UpdateProductVehicleFilter(player, component, EEngine.PLANE);
                    break;
                case "bt_product#category#vehicle#heli":
                    UpdateProductVehicleFilter(player, component, EEngine.HELICOPTER);
                    break;
                case "bt_product#category#vehicle#blimp":
                    UpdateProductVehicleFilter(player, component, EEngine.BLIMP);
                    break;
                case "bt_product#category#vehicle#boat":
                    UpdateProductVehicleFilter(player, component, EEngine.BOAT);
                    break;
                case "bt_product#category#vehicle#train":
                    UpdateProductVehicleFilter(player, component, EEngine.TRAIN);
                    break;
                case "bt_tshop_product#1":
                case "bt_tshop_product#2":
                case "bt_tshop_product#3":
                case "bt_tshop_product#4":
                case "bt_tshop_product#5":
                case "bt_tshop_product#6":
                case "bt_tshop_product#7":
                case "bt_tshop_product#8":
                case "bt_tshop_product#9":
                case "bt_tshop_product#10":
                {
                    int index = Convert.ToInt32(button.Replace("bt_tshop_product#", "")) - 1 +
                                10 * ((component.IsVehiclePage ? component.PageVehicle : component.PageItem) - 1);
                    if (!component.ProductsCache.IsValidIndex(index))
                        break;

                    Product item = component.ProductsCache[index];
                    if (component.Basket.Any(x =>
                            x.Key.UnturnedId == item.UnturnedId && x.Key.IsVehicle == item.IsVehicle))
                    {
                        component.AddNotifyToQueue(TShop.Instance.Localize("ui_basket_contains_product_already",
                            item.GetName()));
                        break;
                    }

                    if (item.IsVehicle && component.Basket.Any(x => x.Key.IsVehicle))
                    {
                        component.AddNotifyToQueue(TShop.Instance.Localize("ui_basket_contains_vehicle_already",
                            item.GetName()));
                        break;
                    }

                    component.Basket.TryAdd(item, 1);
                    component.AddNotifyToQueue(TShop.Instance.Localize("ui_basket_product_added", item.GetName()));

                    UIManager.UpdateBasketPage(player);
                    break;
                }
                default:
                    return false;
            }
            return true;
        }

        private static void UpdateSorting(UnturnedPlayer player, ShopComponent component, ESortType value)
        {
            if (component.SortType == value)
                return;
            component.SortType = value;
            UIManager.UpdateProductPage(player);
        }
        
        private static void UpdateProductItemFilter(UnturnedPlayer player, ShopComponent component, EItemFilter? itemFilter)
        {
            component.ItemFilter = itemFilter;
            component.PageItem = 1;
            UIManager.UpdateProductPage(player);
        }

        private static void UpdateProductVehicleFilter(UnturnedPlayer player, ShopComponent component, EEngine? engine)
        {
            component.VehicleFilter = engine;
            component.PageVehicle = 1;
            UIManager.UpdateProductPage(player);
        }
    }
}