using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using Tavstal.TLibrary.Models.Economy;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Model.Classes;
using Tavstal.TShop.Model.Components;

namespace Tavstal.TShop.Commands
{
    public class CommandBuyVehicle : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "buyvehicle";
        public string Help => "Buys a specific vehicle";
        public string Syntax => "[vehicleID]";
        public List<string> Aliases => new List<string> { "buyv" };
        public List<string> Permissions => new List<string> { "tshop.buy.vehicle", "tshop.commands.buy.vehicle" };

        public async void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            TShopComponent comp = callerPlayer.GetComponent<TShopComponent>();

            if (args.Length == 1)
            {
                ushort id = 0;
                int amount = 1;
                try
                {
                    ushort.TryParse(args[0], out id);
                }
                catch { /* ignore */ }

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
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_vehicle_not_exists", args[0]);
                    return;
                }

                Product item = await TShop.Database.FindVehicleAsync(id);
                if (item == null)
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_vehicle_not_added", args[0]);
                    return;
                }

                decimal cost = item.GetBuyCost(amount);

                if (await TShop.EconomyProvider.GetBalanceAsync(callerPlayer.CSteamID) < cost)
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_balance_not_enough", cost.ToString("0.00"), TShop.EconomyProvider.GetCurrencyName());
                    return;
                }

                if (cost == 0)
                {
                    TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_vehicle_buy_error");
                    return;
                }

                await TShop.EconomyProvider.WithdrawAsync(callerPlayer.CSteamID, cost);
                VehicleManager.spawnLockedVehicleForPlayerV2(id, callerPlayer.Position + new UnityEngine.Vector3(0, 0, 5), callerPlayer.Player.transform.rotation, callerPlayer.Player);
                if (TShop.EconomyProvider.HasTransactionSystem())
                    await TShop.EconomyProvider.AddTransactionAsync(callerPlayer.CSteamID, new Transaction(Guid.NewGuid(), ETransaction.PURCHASE, comp.PaymentMethod, TShop.Instance.Localize(true, "ui_shopname"), callerPlayer.CSteamID.m_SteamID, 0, cost, DateTime.Now));
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "success_vehicle_buy", asset.vehicleName, amount, cost, TShop.EconomyProvider.GetCurrencyName());
            }
            else
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(),  "error_command_buyvehicle_args");
        }
    }
}
