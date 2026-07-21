using Rocket.API;
using System.Collections.Generic;
using Tavstal.TLibrary.Extensions.General;
using Tavstal.TLibrary.Extensions.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Models;
// ReSharper disable UnusedType.Global

namespace Tavstal.TShop.Commands
{
    public class CommandItemShop : CustomCommandBase
    {
        public override IPlugin Plugin => TShop.Instance;
        public override bool UseBackgroundThread => false;
        
        public override AllowedCaller AllowedCaller => AllowedCaller.Both;
        public override string Name => "itemshop";
        public override string Help => "Manages the item shop.";
        public override string Syntax => "add | remove | update";
        public override List<string> Aliases => new List<string> { "ishop" };
        public override List<string> Permissions => new List<string> { "tshop.itemshop", "tshop.commands.itemshop" };
        public override List<ISubcommand>? SubCommands => new List<ISubcommand>
        {
            new SubCommand("add", "Adds an item to the shop.", "add [item name | id] [buycost] [sellcost] <permission>", 
                new List<string> { "insert", "create" }, new List<string> { "tshop.itemshop.add", "tshop.commands.itemshop.add" }, 
                Plugin, AllowedCaller,
                async (caller, args) =>
                {
                    ushort id = 0;

                    if (args.Length < 3 || args.Length > 4)
                    {
                        this.ExecuteHelp(caller, true, "add");
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { /* ignore */ }


                    var asset = id > 0 ? UAssetHelper.FindItemAsset(id) : UAssetHelper.FindItemAsset(args[0]);

                    if (asset == null)
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_item_not_found", TShop.Instance.Config.General.MessageIcon, args[0]);
                        return;
                    }
                    id = asset.id;

                    Product? item = await TShop.DatabaseManager.FindItemAsync(id);
                    if (item != null)
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_item_already_added", TShop.Instance.Config.General.MessageIcon, asset.itemName, asset.id);
                        return;
                    }

                    decimal buycost = 0;
                    decimal sellcost = 0;
                    string? permission = null;

                    try
                    {
                        decimal.TryParse(args[1], out buycost);
                    }
                    catch { /* ignore */ }

                    try
                    {
                        decimal.TryParse(args[2], out sellcost);
                    }
                    catch { /* ignore */ }

                    if (args.Length == 5)
                        permission = args[3];

                    if (permission != null && (permission.ContainsIgnoreCase("null") || permission.ContainsIgnoreCase("none") || permission.Length == 0))
                        permission = null;

                    if (await TShop.DatabaseManager.AddProductAsync(asset.id, false, null, buycost, sellcost, permission != null, permission))
                        TShop.Instance.SendCommandReply(caller,  "success_item_added", TShop.Instance.Config.General.MessageIcon, asset.itemName, asset.id);
                    else
                        TShop.Instance.SendCommandReply(caller,  "error_item_added", TShop.Instance.Config.General.MessageIcon, asset.itemName);
                }),
            new SubCommand("remove", "Removes an item from the shop", "remove [item name | id]", new List<string> { "delete" }, 
                new List<string> { "tshop.itemshop.remove", "tshop.commands.itemshop.remove" },
                Plugin, AllowedCaller,
                async (caller, args) =>
                {
                    ushort id = 0;

                    if (args.Length != 1)
                    {
                        this.ExecuteHelp(caller,  true, "remove");
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { /* ignore */ }

                    var asset = id > 0 ? UAssetHelper.FindItemAsset(id) : UAssetHelper.FindItemAsset(args[0]);

                    if (asset == null)
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_item_not_found", TShop.Instance.Config.General.MessageIcon, args[0]);
                        return;
                    }
                    id = asset.id;

                    Product? item = await TShop.DatabaseManager.FindItemAsync(id);
                    if (item == null)
                    {
                        TShop.Instance.SendCommandReply(caller, "error_item_not_added", TShop.Instance.Config.General.MessageIcon, args[0]);
                        return;
                    }

                    if (await TShop.DatabaseManager.RemoveProductAsync(id, false))
                        TShop.Instance.SendCommandReply(caller, "success_item_removed", TShop.Instance.Config.General.MessageIcon, asset.itemName);
                    else
                        TShop.Instance.SendCommandReply(caller, "error_item_removed", TShop.Instance.Config.General.MessageIcon, asset.itemName);
                }),
            new SubCommand("update", "Updates an item in the shop.", "update [item name | id] [buycost] [sellcost] <permission>", 
                new List<string> { "change" }, new List<string> { "tshop.itemshop.update", "tshop.commands.itemshop.update"  },
                Plugin, AllowedCaller,
                async (caller, args) =>
                {
                    ushort id = 0;

                    if (args.Length < 3 || args.Length > 4)
                    {
                        this.ExecuteHelp(caller, true, "update");
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { /* ignore */ }

                    var asset = id > 0 ? UAssetHelper.FindItemAsset(id) : UAssetHelper.FindItemAsset(args[0]);

                    if (asset == null)
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_item_not_found", TShop.Instance.Config.General.MessageIcon, args[0]);
                        return;
                    }
                    id = asset.id;

                    Product? item = await TShop.DatabaseManager.FindItemAsync(id);
                    if (item == null)
                    {
                        TShop.Instance.SendCommandReply(caller, "error_item_not_added", TShop.Instance.Config.General.MessageIcon, asset.itemName);
                        return;
                    }

                    decimal buycost = 0;
                    decimal sellcost = 0;
                    string? permission = null;

                    try
                    {
                        decimal.TryParse(args[1], out buycost);
                    }
                    catch { /* ignore */ }

                    try
                    {
                        decimal.TryParse(args[2], out sellcost);
                    }
                    catch { /* ignore */ }

                    if (args.Length == 4)
                        permission = args[3];

                    if (permission != null && (permission.ContainsIgnoreCase("null") || permission.ContainsIgnoreCase("none") || permission.Length == 0))
                        permission = null;

                    if (await TShop.DatabaseManager.UpdateProductAsync(id, false, buycost, sellcost, permission != null, permission))
                        TShop.Instance.SendCommandReply(caller,  "success_item_updated", TShop.Instance.Config.General.MessageIcon, asset.itemName, asset.id);
                    else
                        TShop.Instance.SendCommandReply(caller, "error_item_updated", TShop.Instance.Config.General.MessageIcon, asset.itemName);
                })
        };

        protected override bool HandleExecute(IRocketPlayer caller, string[] command) => false;
    }
}
