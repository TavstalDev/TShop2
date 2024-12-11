using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavstal.TLibrary.Helpers.Unturned;
using Tavstal.TLibrary.Models.Commands;
using Tavstal.TLibrary.Models.Plugin;
using Tavstal.TShop.Models;

namespace Tavstal.TShop.Commands
{
    public class CommandCostVehicle : CommandBase
    {
        protected override IPlugin Plugin => TShop.Instance;
        public override AllowedCaller AllowedCaller => AllowedCaller.Player;
        public override string Name => "costvehicle";
        public override string Help => "Checks the cost of a specific vehicle.";
        public override string Syntax => "[vehicleID]";
        public override List<string> Aliases => new List<string> { "costv" };
        public override List<string> Permissions => new List<string> { "tshop.cost.vehicle", "tshop.commands.cost.vehicle" };
        protected override List<SubCommand> SubCommands => new List<SubCommand>();

        protected override async Task<bool> ExecutionRequested(IRocketPlayer caller, string[] args)
        {
            UnturnedPlayer callerPlayer = (UnturnedPlayer)caller;

            if (args.Length != 1)
            {
                TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "error_command_costvehicle_args");
                return true;
            }

            ushort id = 0;
            int amount = 1;
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

            TShop.Instance.SendCommandReply(callerPlayer.SteamPlayer(), "success_vehicle_cost", asset.vehicleName,
                amount, item.GetBuyCost(), item.GetSellCost(), TShop.EconomyProvider.GetCurrencyName());

            return true;
        }
    }
}
