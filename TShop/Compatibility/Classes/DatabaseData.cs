using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavstal.TLibrary.Compatibility;
using YamlDotNet;
using YamlDotNet.Serialization;

namespace Tavstal.TShop.Compability
{
    [Serializable]
    public class DatabaseData : DatabaseSettingsBase
    {
        [YamlMember(Order = 7, Description = "The name of the table that contains the items")]
        public string DatabaseTable_Items { get; set; }
        [YamlMember(Order = 8, Description = "The name of the table that contains the vehicles")]
        public string DatabaseTable_Vehicles { get; set; }

        public DatabaseData() { }
    }
}