using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavstal.TLibrary.Compatibility;
using Newtonsoft.Json;

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