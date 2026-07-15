using System;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TLibrary.Threading;
using Tavstal.TShop.Components;
using Tavstal.TShop.Models;
using Tavstal.TShop.Utils.Helpers;
// ReSharper disable UnusedType.Global

namespace Tavstal.TShop.Commands
{
    public class CommandBuyVehicle : CustomCommandBase
    {
        public override IPlugin Plugin => TShop.Instance;
        public override bool UseBackgroundThread => true;
        
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "buyvehicle";
        public override string Help => "Buys a specific vehicle";
        public override string Syntax => "[vehicleID]";
        public override List<string> Aliases => new List<string> { "buyv" };
        public override List<string> Permissions => new List<string> { "tshop.buy.vehicle", "tshop.commands.buy.vehicle" };
        public override List<ISubcommand>? SubCommands => null;

        protected override async Task<bool> HandleExecuteAsync(IRocketPlayer caller, string[] args)
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

            VehicleAsset? asset;
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

            Product? product = await TShop.DatabaseManager.FindVehicleAsync(id);
            if (product == null)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_vehicle_not_added", args[0]);
                return true;
            }

            if (product.HasPermission && !callerPlayer.HasPermission(product.Permission))
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_no_permission");
                return true;
            }

            decimal cost = product.GetBuyCost();

            if (cost == 0)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_vehicle_buy_error");
                return true;
            }

            if (await TShop.EconomyProvider.GetBalanceAsync(callerPlayer.CSteamID) < cost)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_balance_not_enough",
                    cost.ToString("0.00"), TShop.EconomyProvider.GetCurrencyName());
                return true;
            }

            await TShop.EconomyProvider.WithdrawAsync(callerPlayer.CSteamID, cost, comp.PaymentMethod);

            if (TShop.EconomyProvider.HasTransactionSystem())
            {
                await TShop.EconomyProvider.AddTransactionAsync(
                    callerPlayer.CSteamID,
                    new Transaction(
                        Guid.NewGuid().ToString(),
                        ETransaction.PURCHASE,
                        comp.PaymentMethod,
                        TShop.Instance.Localize(true, "ui_shop_name"),
                        0,
                        callerPlayer.CSteamID.m_SteamID,
                        cost,
                        DateTime.Now
                    )
                );
            }

            await MainThreadDispatcher.RunAsync(() =>
            {
                InteractableVehicle vehicle =
                    UnturnedHelper.SpawnOwnedVehicle(asset.id, callerPlayer);
                var color = product.GetVehicleColor();
                if (vehicle && color != null)
                    vehicle.ServerSetPaintColor(color.Value);

                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "success_vehicle_buy",
                    asset.vehicleName, cost,
                    TShop.EconomyProvider.GetCurrencyName());
            });
            return true;
        }
    }
}
