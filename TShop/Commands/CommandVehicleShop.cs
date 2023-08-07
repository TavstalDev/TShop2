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
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers;

namespace Tavstal.TShop
{
    public class CommandVehicleShop : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "vehicleshop";
        public string Help => "Manages the vehicle shop.";
        public string Syntax => "add [vehicle name | id] <buycost> <sellcost> <permission> | remove  [vehicle name | id] | update [vehicle name | id] <buycost> <sellcost> <permission>";
        public List<string> Aliases => new List<string> { "vshop" };
        public List<string> Permissions => new List<string> { "tshop.admin.vehicleshop" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            TShopComponent comp = callerPlayer.GetComponent<TShopComponent>();

            if (args.Length == 0)
            {
                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_vehicleshop_args"));
                return;
            }
            else
            {
                ushort id = 0;
                VehicleAsset asset = null;


                if (args[0].EqualsIgnoreCase("add"))
                {
                    if (args.Length < 4 || args.Length > 5)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_vehicleshop_add_args"));
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[1], out id);
                    }
                    catch { }

                    if (id > 0)
                        asset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
                    else
                        asset = UAssetHelper.FindVehicleAsset(args[1]);


                    if (asset == null)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_vehicle_not_exists", args[0]));
                        return;
                    }
                    id = asset.id;

                    ShopItem item = TShop.Database.FindVehicle(id);
                    if (item != null)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_vehicle_already_added", asset.vehicleName, asset.id));
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

                    if (TShop.Database.AddVehicle(asset.id, buycost, sellcost, permission != null, permission))
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "success_vehicle_added", asset.vehicleName, asset.id));
                    else
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_vehicle_added", asset.vehicleName));
                }
                else if (args[0].EqualsIgnoreCase("remove"))
                {
                    if (args.Length != 2)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_vehicleshop_remove_args", asset.vehicleName));
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[1], out id);
                    }
                    catch { }

                    if (id > 0)
                        asset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
                    else
                        asset = UAssetHelper.FindVehicleAsset(args[1]);


                    if (asset == null)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_vehicle_not_exists", args[0]));
                        return;
                    }
                    id = asset.id;

                    ShopItem item = TShop.Database.FindVehicle(id);
                    if (item == null)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_vehicle_not_added", args[0]));
                        return;
                    }

                    if (TShop.Database.RemoveVehicle(id))
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "success_vehicle_removed", asset.vehicleName));
                    else
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_vehicle_removed", asset.vehicleName));
                }
                else if (args[0].EqualsIgnoreCase("update"))
                {
                    if (args.Length < 4 || args.Length > 5)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_vehicleshop_update_args", asset.vehicleName));
                        return;
                    }

                    try
                    {
                        ushort.TryParse(args[1], out id);
                    }
                    catch { }

                    if (id > 0)
                        asset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
                    else
                        asset = UAssetHelper.FindVehicleAsset(args[1]);


                    if (asset == null)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_vehicle_not_exists", args[0]));
                        return;
                    }
                    id = asset.id;

                    ShopItem item = TShop.Database.FindVehicle(id);
                    if (item == null)
                    {
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_vehicle_not_added", asset.vehicleName));
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

                    if (TShop.Database.UpdateVehicle(id, buycost, sellcost, permission != null, permission))
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "success_vehicle_updated", asset.vehicleName, asset.id));
                    else
                        UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_vehicle_updated", asset.vehicleName));
                }
                else
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_vehicleshop_args"));
            }

        }
    }
}
