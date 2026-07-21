using System;
using System.Collections.Generic;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Extensions.Unturned;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Models;

namespace Tavstal.TShop.Utils.Helpers
{
    /// <summary>
    /// Provides helper methods for shop-related operations such as buying and selling items and vehicles.
    /// </summary>
    public static class ShopHelper
    {
        public static decimal? RemoveAndGetCost(UnturnedPlayer seller, Product product, byte amount = 1)
        {
            try
            {
                List<InventorySearch> search = seller.Inventory.Search(product.UnturnedId, true);
                if (search.Count < amount)
                {
                    TShop.Instance.SendCommandReply(seller.SteamPlayer(), "error_item_not_enough", TShop.Instance.Config.General.MessageIcon);
                    return null;
                }
                
                seller.Player.equipment.dequip();
                decimal totalCost = 0;
                for (int i = 0; i < amount; i++)
                {
                    var page = search[i].page;
                    var itemIndex = seller.Inventory.getIndex(search[i].page, search[i].jar.x, search[i].jar.y);
                    var item = seller.Inventory.getItem(page, itemIndex);
                    totalCost += TShop.Instance.Config.UseQuality
                        ? product.GetSellCostByQuality(item.item.quality)
                        : product.GetSellCost();

                    seller.Inventory.removeItem(page, itemIndex);
                }

                return totalCost;
            }
            catch (Exception ex)
            {
                TShop.Logger.Error("Failed to remove item and get cost:", ex);
                return null;
            }
        }
    }
}