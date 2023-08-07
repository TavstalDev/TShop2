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

        public void Execute(IRocketPlayer caller, string[] args)
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
                    asset = UnturnedHelper.FindVehicleAsset(args[0]);

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

                decimal cost = item.GetBuyCost(amount);

                if (TShop.economyProvider.GetBalance(callerPlayer) < cost)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_balance_not_enough"));
                    return;
                }

                if (cost == 0)
                {
                    UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_vehicle_buy_error"));
                    return;
                }

                TShop.economyProvider.Withdraw(callerPlayer, cost);
                VehicleManager.spawnLockedVehicleForPlayerV2(asset.id, callerPlayer.Position + new UnityEngine.Vector3(0, 0, 5), callerPlayer.Player.transform.rotation, callerPlayer.Player);
                TShop.economyProvider.AddTransaction(callerPlayer, new Transaction(ETransaction.PURCHASE, comp.PaymentMethod.ToCurrency(), TShop.Instance.Localize(true, "ui_shopname"), callerPlayer.CSteamID.m_SteamID, 0, cost, DateTime.Now));
                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "success_vehicle_buy", asset.vehicleName, amount, cost, TShop.economyProvider.GetConfigValue("MoneySymbol")));
            }
            else
                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_buyvehicle_args"));
        }
    }
}
