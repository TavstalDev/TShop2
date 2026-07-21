using Rocket.API;
using SDG.Unturned;
using System.Collections.Generic;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Extensions.General;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Models;
using UnityEngine;
// ReSharper disable UnusedType.Global

namespace Tavstal.TShop.Commands
{
    public class CommandVehicleShop : CustomCommandBase
    {
        public override IPlugin Plugin => TShop.Instance;
        public override bool UseBackgroundThread => true;

        public override AllowedCaller AllowedCaller => AllowedCaller.Both;
        public override string Name => "vehicleshop";
        public override string Help => "Manages the vehicle shop.";
        public override string Syntax => "add | remove | update";
        public override List<string> Aliases => new List<string> { "vshop" };
        public override List<string> Permissions => new List<string> { "tshop.vehicleshop", "tshop.commands.vehicleshop" };

        public override List<ISubcommand>? SubCommands => new List<ISubcommand>
        {
            new SubCommand("add", "Adds vehicle to the shop.", "add [vehicle name | id] [buycost] [sellcost] <vehicleColor> <permission>", 
                new List<string> { "insert", "create" }, new List<string> { "tshop.vehicleshop.add", "tshop.commands.vehicleshop.add" },
                Plugin, AllowedCaller,
                async (caller, args) =>
                {
                    ushort id = 0;
                    VehicleAsset? asset;

                    if (args.Length < 3 || args.Length > 5)
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_command_vehicleshop_add_args", TShop.Instance.Config.General.MessageIcon);
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { /* ignore */ }

                    TShop.Logger.Debug($"Id > 0 ?: {id > 0}");
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
                        TShop.Instance.SendCommandReply(caller,  "error_vehicle_not_exists", TShop.Instance.Config.General.MessageIcon, args[0]);
                        return;
                    }

                    Product? item = await TShop.DatabaseManager.FindVehicleAsync(id);
                    if (item != null)
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_vehicle_already_added", TShop.Instance.Config.General.MessageIcon, asset.vehicleName, id);
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

                    string? vehicleColor = null;

                    if (args.Length == 4)
                    {
                        if (!ColorUtility.TryParseHtmlString(args[3], out _))
                        {
                            TShop.Instance.SendCommandReply(caller,  "error_vehicle_color_not_hex", TShop.Instance.Config.General.MessageIcon, args[3]);
                            return;
                        }
                        vehicleColor = args[3];
                    }
                    
                    if (args.Length == 5)
                        permission = args[4];

                    if (permission != null && (permission.ContainsIgnoreCase("null") || permission.ContainsIgnoreCase("none") || permission.Length == 0))
                        permission = null;

                    if (await TShop.DatabaseManager.AddProductAsync(id, true, vehicleColor, buycost, sellcost, permission != null, permission))
                        TShop.Instance.SendCommandReply(caller, "success_vehicle_added", TShop.Instance.Config.General.MessageIcon, asset.vehicleName, id);
                    else
                        TShop.Instance.SendCommandReply(caller,  "error_vehicle_added", TShop.Instance.Config.General.MessageIcon, asset.vehicleName);
                }),
            new SubCommand("remove", "Removes a vehicle from the shop.", "remove [vehicle name | id]",
                new List<string> { "delete" }, new List<string> { "tshop.vehicleshop.remove", "tshop.commands.vehicleshop.remove" },
                Plugin, AllowedCaller,
                async (caller, args) =>
                {
                    ushort id = 0;
                    VehicleAsset? asset;

                    if (args.Length != 1)
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_command_vehicleshop_remove_args", TShop.Instance.Config.General.MessageIcon);
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { /* ignore */ }

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
                        TShop.Instance.SendCommandReply(caller,  "error_vehicle_not_exists", TShop.Instance.Config.General.MessageIcon, args[0]);
                        return;
                    }

                    Product? item = await TShop.DatabaseManager.FindVehicleAsync(id);
                    if (item == null)
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_vehicle_not_added", TShop.Instance.Config.General.MessageIcon, args[0]);
                        return;
                    }

                    if (await TShop.DatabaseManager.RemoveProductAsync(id, true))
                        TShop.Instance.SendCommandReply(caller, "success_vehicle_removed", TShop.Instance.Config.General.MessageIcon, asset.vehicleName);
                    else
                        TShop.Instance.SendCommandReply(caller, "error_vehicle_removed", TShop.Instance.Config.General.MessageIcon, asset.vehicleName);
                }),
            new SubCommand("update", "Updates a vehicle in the shop.", "update [vehicle name | id] <buycost> <sellcost> <permission>", 
                new List<string> { "change" }, new List<string> { "tshop.vehicleshop.update", "tshop.commands.vehicleshop.update" },
                Plugin, AllowedCaller,
                async (caller, args) =>
                {
                    ushort id = 0;
                    VehicleAsset? asset;

                    if (args.Length < 3|| args.Length > 4)
                    {
                        TShop.Instance.SendCommandReply(caller, "error_command_vehicleshop_update_args", TShop.Instance.Config.General.MessageIcon);
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { /* ignore */ }

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
                        TShop.Instance.SendCommandReply(caller,  "error_vehicle_not_exists", TShop.Instance.Config.General.MessageIcon, args[0]);
                        return;
                    }

                    Product? item = await TShop.DatabaseManager.FindVehicleAsync(id);
                    if (item == null)
                    {
                        TShop.Instance.SendCommandReply(caller, "error_vehicle_not_added", TShop.Instance.Config.General.MessageIcon, asset.vehicleName);
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

                    if (await TShop.DatabaseManager.UpdateProductAsync(id, true, buycost, sellcost, permission != null, permission))
                        TShop.Instance.SendCommandReply(caller,  "success_vehicle_updated", TShop.Instance.Config.General.MessageIcon, asset.vehicleName, id);
                    else
                        TShop.Instance.SendCommandReply(caller, "error_vehicle_updated", TShop.Instance.Config.General.MessageIcon, asset.vehicleName);
                }),
            new SubCommand("color", "Updates the tint color of a vehicle.", "color [vehicle name | id] [vehicleColor]", 
                new List<string> { "tint", "paint" }, new List<string> { "tshop.vehicleshop.color", "tshop.commands.vehicleshop.color" },
                Plugin, AllowedCaller,
                async (caller, args) =>
                {
                    ushort id = 0;
                    VehicleAsset? asset;

                    if (args.Length != 2)
                    {
                        TShop.Instance.SendCommandReply(caller, "error_command_vehicleshop_color_args", TShop.Instance.Config.General.MessageIcon);
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[0], out id);
                    }
                    catch { /* ignore */ }

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
                        TShop.Instance.SendCommandReply(caller,  "error_vehicle_not_exists", TShop.Instance.Config.General.MessageIcon, args[0]);
                        return;
                    }

                    Product? item = await TShop.DatabaseManager.FindVehicleAsync(id);
                    if (item == null)
                    {
                        TShop.Instance.SendCommandReply(caller, "error_vehicle_not_added", TShop.Instance.Config.General.MessageIcon, asset.vehicleName);
                        return;
                    }
                    
                    if (!ColorUtility.TryParseHtmlString(args[1], out _))
                    {
                        TShop.Instance.SendCommandReply(caller,  "error_vehicle_color_not_hex", TShop.Instance.Config.General.MessageIcon, args[1]);
                        return;
                    }
                    
                    if (await TShop.DatabaseManager.UpdateProductAsync(id, args[1]))
                        TShop.Instance.SendCommandReply(caller,  "success_vehicle_updated", TShop.Instance.Config.General.MessageIcon, asset.vehicleName, id);
                    else
                        TShop.Instance.SendCommandReply(caller, "error_vehicle_updated", TShop.Instance.Config.General.MessageIcon, asset.vehicleName);
                })
            };

        protected override bool HandleExecute(IRocketPlayer caller, string[] command) => false;
    }
}
