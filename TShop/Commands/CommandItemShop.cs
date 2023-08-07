using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Chat;
using Rocket.API;
using Logger = Tavstal.TShop.Helpers.LoggerHelper;
using Tavstal.TShop.Compability;
using Tavstal.TShop.Managers;

namespace Tavstal.TShop
{
    public class CommandItemShop : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "itemshop";
        public string Help => "Manages the item shop.";
        public string Syntax => "add [item name | id] [buycost] [sellcost] <permission> | remove  [item name | id] | update [item name | id] [buycost] [sellcost] <permission>";
        public List<string> Aliases => new List<string> { "ishop" };
        public List<string> Permissions => new List<string> { "tshop.admin.itemshop" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            TShopComponent comp = callerPlayer.GetComponent<TShopComponent>();

            if (args.Length == 0)
            {
                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_itemshop_args"));
                return;
            }
            else
            {
                ushort id = 0;
                ItemAsset asset = null;


                if (args[0].EqualsIgnoreCase("add"))
                {
                    if (args.Length < 4 || args.Length > 5)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_itemshop_add_args"));
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[1], out id);
                    }
                    catch { }

                    if (id > 0)
                        asset = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                    else
                        asset = UnturnedHelper.FindItemAsset(args[1]);

                    if (asset == null)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_not_exists", args[0]));
                        return;
                    }
                    id = asset.id;

                    ShopItem item = TShop.Database.FindItem(id);
                    if (item != null)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_already_added", asset.itemName, asset.id));
                        return;
                    }

                    decimal buycost = 0;
                    decimal sellcost = 0;
                    string permission = null;

                    try
                    {
                        decimal.TryParse(args[2], out buycost);
                    }
                    catch { }

                    try
                    {
                        decimal.TryParse(args[3], out sellcost);
                    }
                    catch { }

                    if (args.Length == 5)
                        permission = args[4];

                    if (permission != null && (permission.ContainsIgnoreCase("null") || permission.ContainsIgnoreCase("none") || permission.Length == 0))
                        permission = null;

                    if (TShop.Database.AddItem(asset.id, buycost, sellcost, permission != null, permission))
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "success_item_added", asset.itemName, asset.id));
                    else
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_added", asset.itemName));
                }
                else if (args[0].EqualsIgnoreCase("remove"))
                {
                    if (args.Length != 2)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_itemshop_remove_args", asset.itemName));
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[1], out id);
                    }
                    catch { }

                    if (id > 0)
                        asset = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                    else
                        asset = UnturnedHelper.FindItemAsset(args[1]);

                    if (asset == null)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_not_exists", args[0]));
                        return;
                    }
                    id = asset.id;

                    ShopItem item = TShop.Database.FindItem(id);
                    if (item == null)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_not_added", args[0]));
                        return;
                    }

                    if (TShop.Database.RemoveItem(id))
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "success_item_removed", asset.itemName));
                    else
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_removed", asset.itemName));
                }
                else if (args[0].EqualsIgnoreCase("update"))
                {
                    if (args.Length < 4 || args.Length > 5)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_itemshop_update_args", asset.itemName));
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[1], out id);
                    }
                    catch { }

                    if (id > 0)
                        asset = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                    else
                        asset = UnturnedHelper.FindItemAsset(args[1]);

                    if (asset == null)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_not_exists", args[0]));
                        return;
                    }
                    id = asset.id;

                    ShopItem item = TShop.Database.FindItem(id);
                    if (item == null)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_not_added", asset.itemName));
                        return;
                    }

                    decimal buycost = 0;
                    decimal sellcost = 0;
                    string permission = null;

                    try
                    {
                        decimal.TryParse(args[2], out buycost);
                    }
                    catch { }

                    try
                    {
                        decimal.TryParse(args[3], out sellcost);
                    }
                    catch { }

                    if (args.Length == 5)
                        permission = args[4];

                    if (permission != null && (permission.ContainsIgnoreCase("null") || permission.ContainsIgnoreCase("none") || permission.Length == 0))
                        permission = null;

                    if (TShop.Database.UpdateItem(id, buycost, sellcost, permission != null, permission))
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "success_item_updated", asset.itemName, asset.id));
                    else
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_item_updated", asset.itemName));
                }
                else
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_itemshop_args"));
            }
        }
    }
}
