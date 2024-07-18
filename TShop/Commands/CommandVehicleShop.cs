using Rocket.API;
using SDG.Unturned;
using System.Collections.Generic;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Compatibility.Interfaces;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Compability;

namespace Tavstal.TShop
{
    public class CommandVehicleShop : CommandBase
    {
        public override AllowedCaller AllowedCaller => AllowedCaller.Both;
        public override string Name => "vehicleshop";
        public override string Help => "Manages the vehicle shop.";
        public override string Syntax => "add | remove | update";
        public override List<string> Aliases => new List<string> { "vshop" };
        public override List<string> Permissions => new List<string> { "tshop.vehicleshop", "tshop.commands.vehicleshop" };

        public override IPlugin Plugin => TShop.Instance;

        public override List<SubCommand> SubCommands => new List<SubCommand>()
        {
            new SubCommand("add", "Adds vehicle to the shop.", "add [vehicle name | id] <buycost> <sellcost> <permission>", new List<string>() { "insert", "create" }, new List<string>() { "tshop.vehicleshop.add", "tshop.commands.vehicleshop.add" },
                async (IRocketPlayer caller, string[] args) =>
                {
                    ushort id = 0;
                    VehicleAsset asset = null;

                    if (args.Length < 3 || args.Length > 4)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_command_vehicleshop_add_args");
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { }

                    TShop.Logger.LogDebug($"Id > 0 ?: {id > 0}");
                    if (id > 0)
                        asset = UAssetHelper.FindVehicleAsset(id);
                    else
                    {
                        asset = UAssetHelper.FindVehicleAsset(args[0]);
                        if (asset != null)
                            id = asset.id;
                    }


                    if (asset == null)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_vehicle_not_exists", args[0]);
                        return;
                    }

                    Product item = await TShop.Database.FindVehicleAsync(id);
                    if (item != null)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_vehicle_already_added", asset.vehicleName, id);
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

                    if (await TShop.Database.AddProductAsync(id, true, buycost, sellcost, permission != null, permission))
                        UChatHelper.SendCommandReply(TShop.Instance, caller, "success_vehicle_added", asset.vehicleName, id);
                    else
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_vehicle_added", asset.vehicleName);
                }),
            new SubCommand("remove", "Removes a vehicle from the shop.", "remove [vehicle name | id]", new List<string>() { "delete" }, new List<string>() { "tshop.vehicleshop.remove", "tshop.commands.vehicleshop.remove" },
                async (IRocketPlayer caller, string[] args) =>
                {
                    ushort id = 0;
                    VehicleAsset asset = null;

                    if (args.Length != 1)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_command_vehicleshop_remove_args", asset.vehicleName);
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { }

                    if (id > 0)
                        asset = UAssetHelper.FindVehicleAsset(id);
                    else
                    {
                        asset = UAssetHelper.FindVehicleAsset(args[0]);
                        if (asset != null)
                            id = asset.id;
                    }


                    if (asset == null)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_vehicle_not_exists", args[0]);
                        return;
                    }

                    Product item = await TShop.Database.FindVehicleAsync(id);
                    if (item == null)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_vehicle_not_added", args[0]);
                        return;
                    }

                    if (await TShop.Database.RemoveProductAsync(id, true))
                        UChatHelper.SendCommandReply(TShop.Instance, caller, "success_vehicle_removed", asset.vehicleName);
                    else
                        UChatHelper.SendCommandReply(TShop.Instance, caller, "error_vehicle_removed", asset.vehicleName);
                }),
            new SubCommand("update", "Updates a vehicle in the shop.", "update [vehicle name | id] <buycost> <sellcost> <permission>", new List<string>() { "change" }, new List<string>() { "tshop.vehicleshop.update", "tshop.commands.vehicleshop.update" },
                async (IRocketPlayer caller, string[] args) =>
                {
                    ushort id = 0;
                    VehicleAsset asset = null;

                    if (args.Length < 3|| args.Length > 4)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller, "error_command_vehicleshop_update_args", asset.vehicleName);
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { }

                    if (id > 0)
                        asset = UAssetHelper.FindVehicleAsset(id);
                    else
                    {
                        asset = UAssetHelper.FindVehicleAsset(args[0]);
                        if (asset != null)
                            id = asset.id;
                    }


                    if (asset == null)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "error_vehicle_not_exists", args[0]);
                        return;
                    }

                    Product item = await TShop.Database.FindVehicleAsync(id);
                    if (item == null)
                    {
                        UChatHelper.SendCommandReply(TShop.Instance, caller, "error_vehicle_not_added", asset.vehicleName);
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

                    if (await TShop.Database.UpdateProductAsync(id, true, buycost, sellcost, permission != null, permission))
                        UChatHelper.SendCommandReply(TShop.Instance, caller,  "success_vehicle_updated", asset.vehicleName, id);
                    else
                        UChatHelper.SendCommandReply(TShop.Instance, caller, "error_vehicle_updated", asset.vehicleName);
                })

        };

        public override bool ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            return false;
        }
    }
}
