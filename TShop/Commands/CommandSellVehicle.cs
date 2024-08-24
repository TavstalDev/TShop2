using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using Rocket.Core.Logging;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Model.Classes;
using Tavstal.TShop.Model.Components;

namespace Tavstal.TShop.Commands
{
    public class CommandSellVehicle : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "sellvehicle";
        public string Help => "Sells the current vehicle.";
        public string Syntax => "";
        public List<string> Aliases => new List<string> { "sellv" };
        public List<string> Permissions => new List<string> { "tshop.sell.vehicle", "tshop.commands.sell.vehicle" };

        public async void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            TShopComponent comp = callerPlayer.GetComponent<TShopComponent>();

            if (args.Length == 0)
            {
                int amount = 1;
                
                InteractableVehicle vehicle = callerPlayer.CurrentVehicle;
                if (vehicle == null)
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_vehicle_sell_null");
                    return;
                }
                VehicleAsset asset = vehicle.asset;
                
                if (vehicle.lockedOwner != callerPlayer.CSteamID || !vehicle.isLocked || vehicle.isDead)
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_vehicle_sell_owner");
                    return;
                }

                Product item = await TShop.Database.FindVehicleAsync(vehicle.id);
                if (item == null)
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_vehicle_not_added", args[0]);
                    return;
                }

                decimal cost = item.GetSellCost(amount);
                if (cost == 0)
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_vehicle_sell_error");
                    return;
                }
                
                VehicleManager.askVehicleDestroy(vehicle);
                await TShop.EconomyProvider.DepositAsync(callerPlayer.CSteamID, cost);
                if (TShop.EconomyProvider.HasTransactionSystem())
                    await TShop.EconomyProvider.AddTransactionAsync(callerPlayer.CSteamID, new Transaction(Guid.NewGuid(), ETransaction.SALE, comp.PaymentMethod, TShop.Instance.Localize(true, "ui_shopname"), 0, callerPlayer.CSteamID.m_SteamID, cost, DateTime.Now));
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "success_vehicle_sell", asset.vehicleName, cost, TShop.EconomyProvider.GetCurrencyName());
            }
            else
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_command_sellvehicle_args");
        }
    }
}
