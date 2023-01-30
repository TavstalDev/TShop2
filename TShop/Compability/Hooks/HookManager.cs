#region References
using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using UnityEngine;
using Logger = Tavstal.TShop.Helpers.LoggerHelper;
using Tavstal.TShop.Managers;
using Tavstal.TShop.Compability;
using SDG.Unturned;
using Steamworks;
using System.Collections;
using Rocket.Core.Permissions;
using Rocket.API.Serialisation;
using Rocket.Core.Commands;
using System.Text.RegularExpressions;
using Rocket.Unturned.Events;
using System.IO;
using UnityEngine.Networking;
using System.Net;
using Newtonsoft.Json;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;
#endregion

namespace Tavstal.TShop.Compability
{
    public class HookManager
    {
        private readonly Dictionary<string, Hook> _hooks = new Dictionary<string, Hook>();
        public IEnumerable<Hook> Hooks => _hooks.Values;

        internal HookManager() { }

        public void Load(Type type)
        {
            if (!typeof(Hook).IsAssignableFrom(type))
            {
                Logger.LogException("'{0}' is not a hook.");
                return;
            }

            if (type.IsAbstract)
            {
                Logger.LogException(string.Format("Cannot register {0} because it is abstract.", type.Name));
                return;
            }

            var hook = CreateInstance<Hook>(type);
            if (_hooks.ContainsKey(hook.Name))
            {
                Logger.LogException("Hook with '{0}' name already exists.");
                return;
            }

            _hooks.Add(hook.Name, hook);
            hook.Load();
        }

        public void LoadAll()
        {
            var assembly = GetType().Assembly;

            foreach (Type t in assembly.GetTypes().ToList().FindAll(x => !x.IsAbstract && typeof(Hook).IsAssignableFrom(x)))
            {
                /*if (t.IsAbstract)
                {
                    Logger.LogException(string.Format("Cannot register {0} because it is abstract.", t.Name));
                    continue;
                }*/

                var hook = CreateInstance<Hook>(t);
                try
                {
                    if (_hooks.ContainsKey(hook.Name))
                    {
                        Logger.LogException(string.Format("Hook with '{0}' name already exists.", hook.Name));
                        continue;
                    }

                    _hooks.Add(hook.Name, hook);
                    hook.Load();
                }
                catch (Exception ex)
                {
                    Logger.LogException(string.Format("Failed to add the '{0}' hook to the library.", hook.Name));
                    Logger.LogError(ex);
                }
            }
        }

        public void Unload(Type type)
        {
            if (type != typeof(Hook))
            {
                Logger.LogException(string.Format("'{0}' is not a hook.", type.Name));
                return;
            }


            if (!_hooks.Any(x => x.Value.GetType() == type))
            {

                Logger.LogException(string.Format("'{0}' is not loaded.", type.Name));
                return;
            }

            var hook = _hooks.Values.FirstOrDefault(x => x.GetType() == type);
            if (hook == null)
            {
                Logger.LogException(string.Format("'{0}' is not found.", type.Name));
                return;
            }

            hook.Unload();
            _hooks.Remove(hook.Name);
        }

        public void UnLoadAll()
        {
            foreach (var value in _hooks.Values)
            {
                value.Unload();
            }
            _hooks.Clear();
        }

        public bool IsHookFound<T>()
        {
            foreach (var hook in _hooks)
            {
                if (hook.Value.GetType() == typeof(T))
                    return true;
            }
            return false;
        }

        public bool IsHookLoadable<T>()
        {
            foreach (var hook in _hooks)
            {
                if (hook.Value.GetType() == typeof(T))
                    return hook.Value.CanBeLoaded();
            }
            return false;
        }

        public T GetHook<T>()
        {
            foreach (var hook in _hooks)
            {
                if (hook.Value.GetType() == typeof(T))
                    return (T)(object)hook.Value;
            }
            return default;
        }

        private T CreateInstance<T>(Type type)
        {
            try
            {
                return (T)Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                Logger.LogException(string.Format("Failed to create instance for '{0}' hook.", type.Name));
                Logger.LogError(ex);
                return default;
            }

        }
    }
}
