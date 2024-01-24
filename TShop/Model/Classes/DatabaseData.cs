using Newtonsoft.Json;
using System;
using Tavstal.TLibrary.Compatibility;

namespace Tavstal.TShop.Compability
{
    [Serializable]
    public class DatabaseData : DatabaseSettingsBase
    {
        [JsonProperty(Order = 7)]
        public string DatabaseTable_Products { get; set; }

        public DatabaseData() { }
    }
}