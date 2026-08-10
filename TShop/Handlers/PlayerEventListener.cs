using Tavstal.RocketFlow.Attributes;
using Tavstal.RocketFlow.Core;
using Tavstal.RocketFlow.Events.Player;
using Tavstal.RocketFlow.Events.Player.Inventory;
using Tavstal.TShop.Components;
using Tavstal.TShop.Utils.Managers;
// ReSharper disable UnusedMember.Global

namespace Tavstal.TShop.Handlers
{
    public class PlayerEventListener : EventListener
    {
        [EventHandler]
        public void OnPlayerJoin(PlayerConnectEvent e)
        {
            UIManager.Init(e.Player);
        }

        [EventHandler]
        public void OnPlayerLeave(PlayerDisconnectEvent e)
        {
            ComponentManager.Invalidate(e.Player.Id);
        }

        [EventHandler]
        public void OnPlayerEquip(PlayerEquipEvent e)
        {
            ShopComponent? comp = ComponentManager.Get(e.Player);
            if (comp == null)
                return;
            
            // Prevent equipping if a transaction is currently in progress.
            if (comp.IsUIOpened)
               e.ShouldAllow = false;
        }
    }
}