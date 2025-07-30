using Rocket.API;
using SDG.Unturned;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Models;

// ReSharper disable AsyncVoidLambda

namespace Tavstal.TShop.Commands
{
    public class CommandItemShop : CommandBase
    {
        public override AllowedCaller AllowedCaller => AllowedCaller.Both;
        public override string Name => "itemshop";
        public override string Help => "Manages the item shop.";
        public override string Syntax => "add | remove | update";
        public override List<string> Aliases => new List<string> { "ishop" };
        public override List<string> Permissions => new List<string> { "tshop.itemshop", "tshop.commands.itemshop" };
        protected override IPlugin Plugin => TShop.Instance;
        protected override List<SubCommand> SubCommands => new List<SubCommand>()
        {
            new SubCommand("add", "Adds an item to the shop.", "add [item name | id] [buycost] [sellcost] <permission>", new List<string>() { "insert", "create" }, new List<string>() { "tshop.itemshop.add", "tshop.commands.itemshop.add" }, 
                async (caller, args) =>
                {
                    ushort id = 0;
                    ItemAsset asset;
                    
                    if (args.Length < 3 || args.Length > 4)
                    {
                        await ExecuteHelp(caller, true, "add", args);
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { /* ignore */ }


                    if (id > 0)
                        asset = UAssetHelper.FindItemAsset(id);
                    else
                        asset = UAssetHelper.FindItemAsset(args[0]);

                    if (asset == null)
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_item_not_found", args[0]);
                        return;
                    }
                    id = asset.id;

                    Product item = await TShop.DatabaseManager.FindItemAsync(id);
                    if (item != null)
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_item_already_added", asset.itemName, asset.id);
                        return;
                    }

                    decimal buycost = 0;
                    decimal sellcost = 0;
                    string permission = null;

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
                        TShop.Instance.SendCommandReply(caller,  "success_item_added", asset.itemName, asset.id);
                    else
                        TShop.Instance.SendCommandReply(caller,  "error_item_added", asset.itemName);
                }),
            new SubCommand("remove", "Removes an item from the shop", "remove [item name | id]", new List<string>() { "delete" }, new List<string>() { "tshop.itemshop.remove", "tshop.commands.itemshop.remove" },
                async (caller, args) =>
                {
                    ushort id = 0;
                    ItemAsset asset;

                    if (args.Length != 1)
                    {
                        await ExecuteHelp(caller,  true, "remove", args);
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { /* ignore */ }

                    if (id > 0)
                        asset = UAssetHelper.FindItemAsset(id);
                    else
                        asset = UAssetHelper.FindItemAsset(args[0]);

                    if (asset == null)
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_item_not_found", args[0]);
                        return;
                    }
                    id = asset.id;

                    Product item = await TShop.DatabaseManager.FindItemAsync(id);
                    if (item == null)
                    {
                        TShop.Instance.SendCommandReply(caller, "error_item_not_added", args[0]);
                        return;
                    }

                    if (await TShop.DatabaseManager.RemoveProductAsync(id, false))
                        TShop.Instance.SendCommandReply(caller, "success_item_removed", asset.itemName);
                    else
                        TShop.Instance.SendCommandReply(caller, "error_item_removed", asset.itemName);
                }),
            new SubCommand("update", "Updates an item in the shop.", "update [item name | id] [buycost] [sellcost] <permission>", new List<string> { "change" }, new List<string>() { "tshop.itemshop.update", "tshop.commands.itemshop.update"  },
                async (caller, args) =>
                {
                    ushort id = 0;
                    ItemAsset asset;

                    if (args.Length < 3 || args.Length > 4)
                    {
                        await ExecuteHelp(caller, true, "update", args);
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { /* ignore */ }

                    if (id > 0)
                        asset = UAssetHelper.FindItemAsset(id);
                    else
                        asset = UAssetHelper.FindItemAsset(args[0]);

                    if (asset == null)
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_item_not_found", args[0]);
                        return;
                    }
                    id = asset.id;

                    Product item = await TShop.DatabaseManager.FindItemAsync(id);
                    if (item == null)
                    {
                        TShop.Instance.SendCommandReply(caller, "error_item_not_added", asset.itemName);
                        return;
                    }

                    decimal buycost = 0;
                    decimal sellcost = 0;
                    string permission = null;

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
                        TShop.Instance.SendCommandReply(caller,  "success_item_updated", asset.itemName, asset.id);
                    else
                        TShop.Instance.SendCommandReply(caller, "error_item_updated", asset.itemName);
                })
        };

        protected override Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            return Task.FromResult(false);
        }
    }
}
