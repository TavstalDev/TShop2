using Newtonsoft.Json;
using System;
using Tavstal.TLibrary.Models.Database;

namespace Tavstal.TShop.Model.Classes
{
    [Serializable]
    public class DatabaseData : DatabaseSettingsBase
    {
        [JsonProperty(Order = 7)]
        public string DatabaseTable_Products { get; set; }
    }
}