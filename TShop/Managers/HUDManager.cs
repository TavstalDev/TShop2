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

namespace Tavstal.TShop.Managers
{
    public static class HUDManager
    {
        private static TShopConfiguration Config => TShop.Instance.Config;

        public static void Init(UnturnedPlayer player)
        {
            EffectManager.sendUIEffect(Config.EffectID, (short)Config.EffectID, true);
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
            TShopComponent comp = player.GetComponent<TShopComponent>();

            int page = 1;
            if (comp.IsVehiclePage)
                page = comp.PageVehicle;
            else
                page = comp.PageItem;

            ITransportConnection playerTC = player.SteamPlayer().transportConnection;
            List<ShopItem> products = comp.IsVehiclePage ? TShop.Database.GetVehicles() : TShop.Database.GetItems();

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

            #endregion
        }

        public static void UpdateBasketPage(UnturnedPlayer player)
        {
            TShopComponent comp = player.GetComponent<TShopComponent>();
        }
    }
}
