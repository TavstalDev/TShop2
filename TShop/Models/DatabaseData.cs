using System;
using Newtonsoft.Json;
using Tavstal.TLibrary.Models.Database;

namespace Tavstal.TShop.Models
{
    [Serializable]
    public class DatabaseData : DatabaseSettingsBase
    {
        [JsonProperty(Order = 7)]
        public string ProductsTable { get; set; }
    }
}