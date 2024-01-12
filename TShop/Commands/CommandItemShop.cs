using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Chat;
using Rocket.API;

using Tavstal.TShop.Compability;
using Tavstal.TShop.Managers;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers;
using Tavstal.TLibrary.Compatibility.Interfaces;
using Rocket.Core.Assets;
using UnityEngine.Assertions;

namespace Tavstal.TShop
{
    public class CommandItemShop : CommandBase
    {
        public override AllowedCaller AllowedCaller => AllowedCaller.Both;
        public override string Name => "itemshop";
        public override string Help => "Manages the item shop.";
        public override string Syntax => "add | remove | update";
        public override List<string> Aliases => new List<string> { "ishop" };
        public override List<string> Permissions => new List<string> { "tshop.itemshop" };
        public override IPlugin Plugin => TShop.Instance;
        public override List<SubCommand> SubCommands => new List<SubCommand>()
        {
            new SubCommand("add", "Adds an item to the shop.", "add [item name | id] [buycost] [sellcost] <permission>", new List<string>(), new List<string>() { "tshop.itemshop.add" }, 
                (IRocketPlayer caller, string[] args) =>
                {
                    ushort id = 0;
                    ItemAsset asset = null;

                    if (args.Length < 3 || args.Length > 4)
                    {
                        ExecuteHelp(caller, true, "add", args);
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { }


                    if (id > 0)
                        asset = UAssetHelper.FindItemAsset(id);
                    else
                        asset = UAssetHelper.FindItemAsset(args[0]);

                    if (asset == null)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_item_not_found", args[0]);
                        return;
                    }
                    id = asset.id;

                    ShopItem item = TShop.Database.FindItem(id);
                    if (item != null)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_item_already_added", asset.itemName, asset.id);
                        return;
                    }

                    decimal buycost = 0;
                    decimal sellcost = 0;
                    string permission = null;

                    try
                    {
                        decimal.TryParse(args[1], out buycost);
                    }
                    catch { }

                    try
                    {
                        decimal.TryParse(args[2], out sellcost);
                    }
                    catch { }

                    if (args.Length == 5)
                        permission = args[3];

                    if (permission != null && (permission.ContainsIgnoreCase("null") || permission.ContainsIgnoreCase("none") || permission.Length == 0))
                        permission = null;

                    if (TShop.Database.AddProduct(asset.id, false, buycost, sellcost, permission != null, permission))
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "success_item_added", asset.itemName, asset.id);
                    else
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_item_added", asset.itemName);
                }),
            new SubCommand("remove", "Removes an item from the shop", "remove [item name | id]", new List<string>() { "delete" }, new List<string>() { "tshop.itemshop.remove" },
                (IRocketPlayer caller, string[] args) =>
                {
                    ushort id = 0;
                    ItemAsset asset = null;

                    if (args.Length != 1)
                    {
                        this.ExecuteHelp(caller,  true, "remove", args);
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { }

                    if (id > 0)
                        asset = UAssetHelper.FindItemAsset(id);
                    else
                        asset = UAssetHelper.FindItemAsset(args[0]);

                    if (asset == null)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_item_not_found", args[0]);
                        return;
                    }
                    id = asset.id;

                    ShopItem item = TShop.Database.FindItem(id);
                    if (item == null)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller, "error_item_not_added", args[0]);
                        return;
                    }

                    if (TShop.Database.RemoveProduct(id, false))
                        UChatHelper.SendCommandReply(TShop.Instance, caller, "success_item_removed", asset.itemName);
                    else
                        UChatHelper.SendCommandReply(TShop.Instance, caller, "error_item_removed", asset.itemName);
                }),
            new SubCommand("update", "Updates an item in the shop.", "update [item name | id] [buycost] [sellcost] <permission>", new List<string> { "change" }, new List<string>() { "tshop.itemshop.update" },
                (IRocketPlayer caller, string[] args) =>
                {
                    ushort id = 0;
                    ItemAsset asset = null;

                    if (args.Length < 3 || args.Length > 4)
                    {
                        ExecuteHelp(caller, true, "update", args);
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { }

                    if (id > 0)
                        asset = UAssetHelper.FindItemAsset(id);
                    else
                        asset = UAssetHelper.FindItemAsset(args[0]);

                    if (asset == null)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_item_not_found", args[0]);
                        return;
                    }
                    id = asset.id;

                    ShopItem item = TShop.Database.FindItem(id);
                    if (item == null)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller, "error_item_not_added", asset.itemName);
                        return;
                    }

                    decimal buycost = 0;
                    decimal sellcost = 0;
                    string permission = null;

                    try
                    {
                        decimal.TryParse(args[1], out buycost);
                    }
                    catch { }

                    try
                    {
                        decimal.TryParse(args[2], out sellcost);
                    }
                    catch { }

                    if (args.Length == 4)
                        permission = args[3];

                    if (permission != null && (permission.ContainsIgnoreCase("null") || permission.ContainsIgnoreCase("none") || permission.Length == 0))
                        permission = null;

                    if (TShop.Database.UpdateProduct(id, false, buycost, sellcost, permission != null, permission))
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "success_item_updated", asset.itemName, asset.id);
                    else
                        UChatHelper.SendCommandReply(TShop.Instance, caller, "error_item_updated", asset.itemName);
                })
        };

        public override bool ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            return false;
        }
    }
}
