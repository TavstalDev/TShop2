using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Chat;
using Rocket.API;
using Tavstal.TShop.Compability;
using Tavstal.TShop.Managers;
using Tavstal.TLibrary.Compatibility;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers;

namespace Tavstal.TShop
{
    public class CommandSellVehicle : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "sellvehicle";
        public string Help => "Sells the current vehicle.";
        public string Syntax => "";
        public List<string> Aliases => new List<string> { "sellv" };
        public List<string> Permissions => new List<string> { "tshop.sell.vehicle" };

        public void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;
            TShopComponent comp = callerPlayer.GetComponent<TShopComponent>();

            if (args.Length == 0)
            {
                int amount = 1;

                VehicleAsset asset = null;
                InteractableVehicle vehicle = callerPlayer.CurrentVehicle;
                if (vehicle == null)
                {
                    UChatHelper.SendChatMessage(TShop.Instance,callerPlayer.SteamPlayer(),  "error_vehicle_sell_null");
                    return;
                }
                else if (vehicle.lockedOwner != callerPlayer.CSteamID || !vehicle.isLocked ||vehicle.isDead)
                {
                    UChatHelper.SendChatMessage(TShop.Instance,callerPlayer.SteamPlayer(),  "error_vehicle_sell_owner");
                    return;
                }

                ShopItem item = TShop.Database.FindVehicle(vehicle.id);
                if (item == null)
                {
                    UChatHelper.SendChatMessage(TShop.Instance,callerPlayer.SteamPlayer(),  "error_vehicle_not_added", args[0]);
                    return;
                }

                decimal cost = item.GetSellCost(amount);
                if (cost == 0)
                {
                    UChatHelper.SendChatMessage(TShop.Instance,callerPlayer.SteamPlayer(),  "error_vehicle_sell_error");
                    return;
                }

                TShop.economyProvider.Deposit(callerPlayer, cost);
                foreach (Passenger pas in vehicle.passengers)
                {
                    VehicleManager.forceRemovePlayer(vehicle, pas.player.playerID.steamID);
                }
                VehicleManager.askVehicleDestroy(vehicle);
                TShop.economyProvider.AddTransaction(callerPlayer, new Transaction(ETransaction.SALE, comp.PaymentMethod.ToCurrency(), TShop.Instance.Localize(true, "ui_shopname"), 0, callerPlayer.CSteamID.m_SteamID, cost, DateTime.Now));
                UChatHelper.SendChatMessage(TShop.Instance,callerPlayer.SteamPlayer(),  "success_vehicle_sell", asset.vehicleName, amount, cost, TShop.economyProvider.GetConfigValue<string>("MoneySymbol"));
            }
            else
                UChatHelper.SendChatMessage(TShop.Instance,callerPlayer.SteamPlayer(),  "error_command_sellvehicle_args");
        }
    }
}
