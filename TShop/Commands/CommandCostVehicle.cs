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
    public class CommandCostVehicle : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "costvehicle";
        public string Help => "Checks the cost of a specific vehicle.";
        public string Syntax => "[vehicleID]";
        public List<string> Aliases => new List<string> { "costv" };
        public List<string> Permissions => new List<string> { "tshop.cost.vehicle" };

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

                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "success_vehicle_cost", asset.vehicleName, amount, item.GetBuyCost(), item.GetSellCost(), TShop.economyProvider.GetConfigValue("MoneySymbol")));
            }
            else
                UnturnedHelper.SendChatMessage(callerPlayer.SteamPlayer(), TShop.Instance.Localize(true, "error_command_costvehicle_args"));
        }
    }
}
