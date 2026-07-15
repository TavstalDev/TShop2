using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using Tavstal.TShop.Components;
using Tavstal.TShop.Models.Enums;
using Tavstal.TShop.Utils.Managers;

namespace Tavstal.TShop.Handlers.Buttons
{
    public static class NavigationButtonHandler
    {
        public static bool Handle(UnturnedPlayer player, ITransportConnection transportConnection, ShopComponent component, string button)
        {
            switch (button)
            {
                case "bt_nav_tshop_items":
                {
                    if (component.MenuCategory == EMenuCategory.ProductItems)
                        return true;

                    component.MenuCategory = EMenuCategory.ProductItems;
                    component.IsVehiclePage = false;
                    UIManager.UpdateProductPage(player);
                    EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, transportConnection,
                        true,
                        "tshop_products", true);
                    EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, transportConnection,
                        true,
                        "tshop_basket", false);
                    EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, transportConnection,
                        true,
                        "categorybox#item", true);
                    EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, transportConnection,
                        true,
                        "categorybox#vehicle", false);
                    return true;
                }
                case "bt_nav_tshop_vehicles":
                {
                    if (component.MenuCategory == EMenuCategory.ProductVehicles)
                        return true;

                    component.MenuCategory = EMenuCategory.ProductVehicles;
                    component.IsVehiclePage = true;
                    UIManager.UpdateProductPage(player);
                    EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, transportConnection,
                        true,
                        "tshop_products", true);
                    EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, transportConnection,
                        true,
                        "tshop_basket", false);
                    EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, transportConnection,
                        true,
                        "categorybox#item", false);
                    EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, transportConnection,
                        true,
                        "categorybox#vehicle", true);
                    return true;
                }
                case "bt_nav_tshop_basket":
                {
                    if (component.MenuCategory == EMenuCategory.Basket)
                        return true;

                    component.MenuCategory = EMenuCategory.Basket;
                    UIManager.UpdateBasketPage(player);
                    EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, transportConnection,
                        true,
                        "tshop_products", false);
                    EffectManager.sendUIEffectVisibility((short)TShop.Instance.Config.EffectID, transportConnection,
                        true,
                        "tshop_basket", true);
                    return true;
                }
                case "bt_nav_tshop_logout":
                {
                    UIManager.Hide(player);
                    return true;
                }
                default:
                    return false;
            }
        }
    }
}