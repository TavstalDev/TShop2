using System;
using Logger = Tavstal.TShop.Helpers.LoggerHelper;
using System.Reflection;

namespace Tavstal.TShop.Compability
{

    public abstract class Hook
    {

        public string Name { get; }
        public bool IsLoaded { get; private set; }

        protected Hook(string name)
        {
            Name = name;
        }

        internal void Load()
        {
            if (!CanBeLoaded())
            {
                return;
            }

            IsLoaded = true;

            try
            {
                OnLoad();
            }
            catch (Exception ex)
            {
                IsLoaded = false;
                Logger.LogError($"Failed to load '{Name}' hook.");
                Logger.LogException(ex.ToString());
            }
        }

        internal void Unload()
        {
            IsLoaded = false;

            try
            {
                OnUnload();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to unload '{Name}' hook.");
                Logger.LogException(ex.ToString());
            }
        }

        public abstract void OnLoad();

        public abstract void OnUnload();

        public abstract bool CanBeLoaded();

    }
}