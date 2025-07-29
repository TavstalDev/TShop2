using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TShop.Components;
using Tavstal.TShop.Models;
using Tavstal.TShop.Utils.Helpers;

namespace Tavstal.TShop.Commands
{
    public class CommandBuyVehicle : CommandBase
    {
        protected override IPlugin Plugin => TShop.Instance;
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "buyvehicle";
        public override string Help => "Buys a specific vehicle";
        public override string Syntax => "[vehicleID]";
        public override List<string> Aliases => new List<string> { "buyv" };
        public override List<string> Permissions => new List<string> { "tshop.buy.vehicle", "tshop.commands.buy.vehicle" };
        protected override List<SubCommand> SubCommands => new List<SubCommand>();

        protected override async Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            ShopComponent comp = callerPlayer.GetComponent<ShopComponent>();

            if (args.Length != 1)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_command_buyvehicle_args");
                return true;
            }

            ushort id = 0;
            try
            {
                ushort.TryParse(args[0], out id);
            }
            catch
            {
                /* ignore */
            }

            VehicleAsset asset;
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
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_vehicle_not_exists", args[0]);
                return true;
            }

            Product item = await TShop.DatabaseManager.FindVehicleAsync(id);
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

            decimal cost = item.GetBuyCost();

            if (await TShop.EconomyProvider.GetBalanceAsync(callerPlayer.CSteamID) < cost)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_balance_not_enough",
                    cost.ToString("0.00"), TShop.EconomyProvider.GetCurrencyName());
                return true;
            }

            if (cost == 0)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_vehicle_buy_error");
                return true;
            }

            if (!await ShopHelper.BuyVehicleAsync(callerPlayer, id, item.GetVehicleColor(), cost, comp.PaymentMethod))
                return true;

            TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "success_vehicle_buy", asset.vehicleName, cost,
                TShop.EconomyProvider.GetCurrencyName());

            return true;
        }
    }
}
