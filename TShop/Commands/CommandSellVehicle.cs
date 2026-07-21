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

// ReSharper disable UnusedType.Global

namespace Tavstal.TShop.Commands
{
    public class CommandSellVehicle : CustomCommandBase
    {
        public override IPlugin Plugin => TShop.Instance;
        public override bool UseBackgroundThread => true;
        
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "sellvehicle";
        public override string Help => "Sells the current vehicle.";
        public override string Syntax => "";
        public override List<string> Aliases => new List<string> { "sellv" };
        public override List<string> Permissions => new List<string> { "tshop.sell.vehicle", "tshop.commands.sell.vehicle" };
        public override List<ISubcommand>? SubCommands => null;

        protected override async Task<bool> HandleExecuteAsync(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            ShopComponent comp = callerPlayer.GetComponent<ShopComponent>();

            if (args.Length != 0)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_command_sellvehicle_args", TShop.Instance.Config.General.MessageIcon);
                return true;
            }

            InteractableVehicle vehicle = callerPlayer.CurrentVehicle;
            if (vehicle == null)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_vehicle_sell_null", TShop.Instance.Config.General.MessageIcon);
                return true;
            }

            VehicleAsset asset = vehicle.asset;

            if (vehicle.lockedOwner != callerPlayer.CSteamID || !vehicle.isLocked || vehicle.isDead)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_vehicle_sell_owner", TShop.Instance.Config.General.MessageIcon);
                return true;
            }

            Product? product = await TShop.DatabaseManager.FindVehicleAsync(vehicle.id);
            if (product == null)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_vehicle_not_added", TShop.Instance.Config.General.MessageIcon, args[0]);
                return true;
            }

            if (product.HasPermission && !callerPlayer.HasPermission(product.Permission))
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_no_permission", TShop.Instance.Config.General.MessageIcon);
                return true;
            }

            decimal cost = TShop.Instance.Config.UseQuality
                ? product.GetSellCostByQuality(
                    (byte)(vehicle.health / vehicle.asset.healthMax *
                           100))
                : product.GetSellCost();

            if (cost == 0)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_vehicle_sell_error", TShop.Instance.Config.General.MessageIcon);
                return true;
            }

            await MainThreadDispatcher.RunAsync(async () =>
            {
                VehicleManager.askVehicleDestroy(vehicle);
                await BackgroundThreadDispatcher.RunAsync(async () =>
                {
                    await TShop.EconomyProvider.DepositAsync(callerPlayer.CSteamID, cost, comp.PaymentMethod);

                    if (!TShop.EconomyProvider.HasTransactionSystem())
                        return;

                    await TShop.EconomyProvider.AddTransactionAsync(
                        callerPlayer.CSteamID,
                        new Transaction(
                            Guid.NewGuid().ToString(),
                            ETransaction.SALE,
                            comp.PaymentMethod,
                            TShop.Instance.Localize(true, "ui_shop_name"),
                            0,
                            callerPlayer.CSteamID.m_SteamID,
                            cost,
                            DateTime.Now
                        )
                    );
                });

                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "success_vehicle_sell", TShop.Instance.Config.General.MessageIcon, asset.vehicleName,
                    cost,
                    TShop.EconomyProvider.GetCurrencyName());
            });

            return true;
        }
    }
}
