using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TShop.Components;
using Tavstal.TShop.Models;
using Tavstal.TShop.Utils.Helpers;

namespace Tavstal.TShop.Commands
{
    public class CommandSellVehicle : CommandBase
    {
        protected override IPlugin Plugin => TShop.Instance;
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "sellvehicle";
        public override string Help => "Sells the current vehicle.";
        public override string Syntax => "";
        public override List<string> Aliases => new List<string> { "sellv" };
        public override List<string> Permissions => new List<string> { "tshop.sell.vehicle", "tshop.commands.sell.vehicle" };
        protected override List<SubCommand> SubCommands => new List<SubCommand>();

        protected override async Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            ShopComponent comp = callerPlayer.GetComponent<ShopComponent>();

            if (args.Length != 0)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_command_sellvehicle_args");
                return true;
            }

            int amount = 1;

            InteractableVehicle vehicle = callerPlayer.CurrentVehicle;
            if (vehicle == null)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_vehicle_sell_null");
                return true;
            }

            VehicleAsset asset = vehicle.asset;

            if (vehicle.lockedOwner != callerPlayer.CSteamID || !vehicle.isLocked || vehicle.isDead)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_vehicle_sell_owner");
                return true;
            }

            Product item = await TShop.DatabaseManager.FindVehicleAsync(vehicle.id);
            if (item == null)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_vehicle_not_added", args[0]);
                return true;
            }

            if (item.HasPermission && !callerPlayer.HasPermission(item.Permission))
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_no_permission");
                return true;
            }

            decimal cost = item.GetSellCost(amount);
            if (cost == 0)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_vehicle_sell_error");
                return true;
            }

            if (!await ShopHelper.SellVehicleAsync(callerPlayer, vehicle, cost, comp.PaymentMethod))
                return true;
            
            TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "success_vehicle_sell", asset.vehicleName, cost,
                TShop.EconomyProvider.GetCurrencyName());

            return true;
        }
    }
}
