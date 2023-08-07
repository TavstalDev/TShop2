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

namespace Tavstal.TShop.Managers
{
    public class UIManager
    {
        public  static void UpdateItemsPage(UnturnedPlayer player)
        {
            try
            { 
                TShopComponent comp = player.GetComponent<TShopComponent>();
                List<ShopItem> products = TShop.Database.GetItems().FindAll(x => !comp.products.Any(y => y.Id == x.Id && !y.isVehicle));

                for (int i = 0; i < 8; i++)
                {
                    int nameindex = i + 1;
                    int index = i + comp.item_page * 8;
                    if (products.IsValidIndex(index))
                    {
                        ShopItem item = products[index];
                        ItemAsset asset = (ItemAsset)Assets.find(EAssetType.ITEM, item.Id);
                        if (asset == null) // It's not the best solution, will be replaced in the future
                        {
                            i--;
                            products.RemoveAll(x => x.Id == item.Id);
                            continue;
                        }

                        if (asset.size_x == asset.size_y)
                        {
                            EffectManager.sendUIEffectImageURL((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_item#" + nameindex + "_1:1", UnturnedHelper.GetItemIcon(asset.id));
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_item#" + nameindex + "_1:1", true);
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_item#" + nameindex + "_2:1", false);
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_item#" + nameindex + "_1:2", false);
                        }
                        else if (asset.size_x > asset.size_y)
                        {
                            EffectManager.sendUIEffectImageURL((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_item#" + nameindex + "_2:1", UnturnedHelper.GetItemIcon(asset.id));
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_item#" + nameindex + "_1:1", false);
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_item#" + nameindex + "_2:1", true);
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_item#" + nameindex + "_1:2", false);
                        }
                        else
                        {
                            EffectManager.sendUIEffectImageURL((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_item#" + nameindex + "_1:2", UnturnedHelper.GetItemIcon(asset.id));
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_item#" + nameindex + "_1:1", false);
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_item#" + nameindex + "_2:1", false);
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_item#" + nameindex + "_1:2", true);
                        }

                        EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_item#" + nameindex + "_name", asset.itemName);
                        EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_item#" + nameindex + "_cost", item != null ? item.GetBuyCostFormatted() : "ShopItem not found");
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "shop_item#" + nameindex, true);
                    }
                    else
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "shop_item#" + nameindex, false);
                }

                EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_cont_items_page", (comp.item_page + 1) + "/" + (products.Count / 8 + 1));
            }
            catch (Exception ex)
            {
                Logger.LogException("Error in UpdateItemsPage():");
                Logger.LogError(ex);
            }
        }

        public static void UpdateVehiclessPage(UnturnedPlayer player)
        {
            try
            {
                TShopComponent comp = player.GetComponent<TShopComponent>();
                List<ShopItem> products = TShop.Database.GetVehicles().FindAll(x => !comp.products.Any(y => y.Id == x.Id && y.isVehicle));

                for (int i = 0; i < 8; i++)
                {
                    int nameindex = i + 1;
                    int index = i + comp.vehicle_page * 8;
                    if (products.IsValidIndex(index))
                    {
                        ShopItem item = products[index];
                        VehicleAsset asset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, item.Id);
                        if (asset == null) // It's not the best solution, will be replaced in the future
                        {
                            i--;
                            products.RemoveAll(x => x.Id == item.Id);
                            continue;
                        }

                        EffectManager.sendUIEffectImageURL((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_vehicles#" + nameindex + "_1:1", "");

                        EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_vehicles#" + nameindex + "_name", asset.vehicleName);
                        EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_vehicles#" + nameindex + "_cost", item != null ? item.GetBuyCostFormatted() : "ShopItem not found");
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "shop_vehicles#" + nameindex, true);
                    }
                    else
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "shop_vehicles#" + nameindex, false);
                }

                EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_cont_vehicles_page", (comp.item_page + 1) + "/" + (products.Count / 8 + 1));
            }
            catch (Exception ex)
            {
                Logger.LogException("Error in UpdateVehiclesPage():");
                Logger.LogError(ex);
            }
        }

        public static void UpdatePaymentPage(UnturnedPlayer player)
        {
            try
            {
                TShopComponent comp = player.GetComponent<TShopComponent>();

                List<ShopItem> items = TShop.Database.GetItems();
                List<ShopItem> vehs = TShop.Database.GetVehicles();

                for (int i = 0; i < 6; i++)
                {
                    int nameindex = i + 1;
                    int index = i + comp.cart_page * 6;
                    if (comp.products.IsValidIndex(index))
                    {
                        Product item = comp.products.ElementAt(index);
                        if (item.isVehicle)
                        {
                            VehicleAsset asset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, item.Id);
                            if (asset == null) // It's not the best solution, will be replaced in the future
                            {
                                i--;
                                vehs.RemoveAll(x => x.Id == item.Id);
                                continue;
                            }
                            ShopItem sitem = vehs.Find(x => x.Id == item.Id);

                            EffectManager.sendUIEffectImageURL((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_1:1", "");
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_1:1", true);
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_1:2", false);
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_2:1", false);


                            EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "inputf_shop_cont_cart_item#" + nameindex + "_amount", item.Amount.ToString());
                            EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_cont_cart_item#" + nameindex + "_name", asset.vehicleName);
                            EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_cont_cart_item#" + nameindex + "_cost", sitem != null ? sitem.GetBuyCostFormatted(item.Amount) : "ShopItem not found");
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "shop_cont_cart_item#" + nameindex, true);
                        }
                        else
                        {
                            ItemAsset asset = (ItemAsset)Assets.find(EAssetType.ITEM, item.Id);
                            if (asset == null) // It's not the best solution, will be replaced in the future
                            {
                                i--;
                                items.RemoveAll(x => x.Id == item.Id);
                                continue;
                            }
                            ShopItem sitem = items.Find(x => x.Id == item.Id);

                            if (asset.size_x == asset.size_y)
                            {
                                EffectManager.sendUIEffectImageURL((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_1:1", UnturnedHelper.GetItemIcon(asset.id));
                                EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_1:1", true);
                                EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_1:2", false);
                                EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_2:1", false);
                            }
                            else if (asset.size_x > asset.size_y)
                            {
                                EffectManager.sendUIEffectImageURL((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_2:1", UnturnedHelper.GetItemIcon(asset.id));
                                EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_1:1", false);
                                EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_1:2", false);
                                EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_2:1", true);
                            }
                            else
                            {
                                EffectManager.sendUIEffectImageURL((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_1:2", UnturnedHelper.GetItemIcon(asset.id));
                                EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_1:1", false);
                                EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_1:2", true);
                                EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "img_shop_cont_cart_item#" + nameindex + "_2:1", false);
                            }

                            EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "inputf_shop_cont_cart_item#" + nameindex + "_amount", item.Amount.ToString());
                            EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_cont_cart_item#" + nameindex + "_name", asset.itemName);
                            EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_cont_cart_item#" + nameindex + "_cost", sitem != null ? sitem.GetBuyCostFormatted(item.Amount) : "ShopItem not found");
                            EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "shop_cont_cart_item#" + nameindex, true);
                        }
                    }
                    else
                        EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "shop_cont_cart_item#" + nameindex, false);
                }

                EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_cont_cart_page", (comp.item_page + 1) + "/" + (comp.products.Count / 6 + 1));
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in UpdatePaymentPage():");
                Logger.LogError(ex);
            }
        }

        public static void UpdateTotalPay(UnturnedPlayer player)
        {
            try
            {
                TShopComponent comp = player.GetComponent<TShopComponent>();
                List<ShopItem> items = TShop.Database.GetItems();
                List<ShopItem> vehicles = TShop.Database.GetVehicles();
                decimal buyCost = 0;
                decimal sellCost = 0;

                foreach (Product p in comp.products)
                {
                    buyCost += p.isVehicle ? vehicles.Find(x => x.Id == p.Id).GetBuyCost(p.Amount) : items.Find(x => x.Id == p.Id).GetBuyCost(p.Amount);
                    sellCost += p.isVehicle ? 0 : items.Find(x => x.Id == p.Id).GetSellCost(p.Amount);
                }

                EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_cart_payment_total", TShop.Instance.Localize(true, "ui_total_buy", buyCost, TShop.economyProvider.GetConfigValue<string>("MoneySymbol")));
                EffectManager.sendUIEffectText((short)TShop.Instance.Config.EffectID, player.SteamPlayer().transportConnection, false, "tb_shop_cart_payment_total_sell", TShop.Instance.Localize(true, "ui_total_sell", sellCost, TShop.economyProvider.GetConfigValue<string>("MoneySymbol")));
            }
            catch (Exception ex)
            {
                Logger.LogException("Error in UpdateTotalPay():");
                Logger.LogError(ex);
            }
        }

    }
}
