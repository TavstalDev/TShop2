using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TShop.Compability;

namespace Tavstal.TShop
{
    public class CommandCostVehicle : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "costvehicle";
        public string Help => "Checks the cost of a specific vehicle.";
        public string Syntax => "[vehicleID]";
        public List<string> Aliases => new List<string> { "costv" };
        public List<string> Permissions => new List<string> { "tshop.cost.vehicle", "tshop.commands.cost.vehicle" };

        public async void Execute(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;

            if (args.Length == 1)
            {
                ushort id = 0;
                int amount = 1;
                try
                {
                    ushort.TryParse(args[0], out id);
                }
                catch { }

                VehicleAsset asset;
                if (id > 0)
                    asset = UAssetHelper.FindVehicleAsset(id);
                else
                    asset = UAssetHelper.FindVehicleAsset(args[0]);

                if (asset == null)
                {
                    UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "error_vehicle_not_exists", args[0]);
                    return;
                }
                id = asset.id;

                Product item = await TShop.Database.FindVehicleAsync(id);
                if (item == null)
                {
                    UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "error_vehicle_not_added", args[0]);
                    return;
                }

                UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "success_vehicle_cost", asset.vehicleName, amount, item.GetBuyCost(), item.GetSellCost(), TShop.EconomyProvider.GetCurrencyName());
            }
            else
                UChatHelper.SendCommandReply(TShop.Instance,callerPlayer.SteamPlayer(),  "error_command_costvehicle_args");
        }
    }
}
