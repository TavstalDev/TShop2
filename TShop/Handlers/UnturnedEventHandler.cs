using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavstal.TLibrary.Compatibility.Economy;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Helpers;
using Tavstal.TShop.Compability;
using Tavstal.TShop.Managers;

namespace Tavstal.TShop.Handlers
{
    internal static class UnturnedEventHandler
    {
        private static bool _isAttached = false;

        public static void Attach()
        {
            if (_isAttached)
                return;

            _isAttached = true;
            EffectManager.onEffectTextCommitted += Event_OnInputFieldEdit;
            EffectManager.onEffectButtonClicked += Event_OnButtonClick;
            U.Events.OnPlayerConnected += Event_OnPlayerJoin;
        }

        public static void Unattach()
        {
            if (!_isAttached)
                return;

            _isAttached = false;
            EffectManager.onEffectTextCommitted -= Event_OnInputFieldEdit;
            EffectManager.onEffectButtonClicked -= Event_OnButtonClick;
            U.Events.OnPlayerConnected -= Event_OnPlayerJoin;
        }

        private static void Event_OnPlayerJoin(UnturnedPlayer player)
        {
            //EffectManager.sendUIEffect(Config.EffectID, (short)Config.EffectID, player.SteamPlayer().transportConnection, true);
        }

        private static void Event_OnInputFieldEdit(Player player, string button, string text)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
            TShopComponent comp = player.GetComponent<TShopComponent>();

            
        }

        private static void Event_OnButtonClick(Player player, string button)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
            TShopComponent comp = player.GetComponent<TShopComponent>();

            
        }
    }
}
