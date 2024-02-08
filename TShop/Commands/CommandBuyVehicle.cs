using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Compability;

namespace Tavstal.TShop
{
    public class CommandBuyVehicle : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "buyvehicle";
        public string Help => "Buys a specific vehicle";
        public string Syntax => "[vehicleID]";
        public List<string> Aliases => new List<string> { "buyv" };
        public List<string> Permissions => new List<string> { "tshop.buy.vehicle" };

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
                catch { }

                VehicleAsset asset = null;

                if (id > 0)
                    asset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);
                else
                    asset = UAssetHelper.FindVehicleAsset(args[0]);

                if (asset == null)
                {
                    UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "error_vehicle_not_exists", args[0]);
                    return;
                }
                id = asset.id;

                Product item = await TShop.Database.FindVehicle(id);
                if (item == null)
                {
                    UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "error_vehicle_not_added", args[0]);
                    return;
                }

                decimal cost = item.GetBuyCost(amount);

                if (TShop.EconomyProvider.GetBalance(callerPlayer.CSteamID) < cost)
                {
                    UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "error_balance_not_enough");
                    return;
                }

                if (cost == 0)
                {
                    UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "error_vehicle_buy_error");
                    return;
                }

                TShop.EconomyProvider.Withdraw(callerPlayer.CSteamID, cost);
                VehicleManager.spawnLockedVehicleForPlayerV2(asset.id, callerPlayer.Position + new UnityEngine.Vector3(0, 0, 5), callerPlayer.Player.transform.rotation, callerPlayer.Player);
                TShop.EconomyProvider.AddTransaction(callerPlayer.CSteamID, new Transaction(Guid.NewGuid(), ETransaction.PURCHASE, comp.PaymentMethod, TShop.Instance.Localize(true, "ui_shopname"), callerPlayer.CSteamID.m_SteamID, 0, cost, DateTime.Now));
                UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "success_vehicle_buy", asset.vehicleName, amount, cost, TShop.EconomyProvider.GetConfigValue<string>("MoneySymbol"));
            }
            else
                UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "error_command_buyvehicle_args");
        }
    }
}
