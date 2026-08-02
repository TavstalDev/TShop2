using System;
using System.Collections.Concurrent;
using Rocket.Unturned.Player;
using Tavstal.TLibrary.Extensions;
using Tavstal.TLibrary.Models.Logging;
using Tavstal.TShop.Components;

namespace Tavstal.TShop.Utils.Managers
{
    public static class ComponentManager
    {
        private static readonly ConcurrentDictionary<string, ShopComponent> _components = new ConcurrentDictionary<string, ShopComponent>();
        private static TLogger Logger => TShop.Logger;

        public static ShopComponent Get(UnturnedPlayer player) => _components.GetOrAdd(player.Id, player.GetComponent<ShopComponent>());

        public static void Invalidate(string id)
        {
            try
            {
                _components.TryRemove(id, out _);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected occured while invalidating {id}'s component.", ex);
            }
        }
    }
}