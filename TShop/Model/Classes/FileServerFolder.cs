using Newtonsoft.Json;

namespace Tavstal.TShop.Model.Classes
{
    public class FileServerFolder
    {
        [JsonProperty("DispalyName")]
        public string DisplayName { get; set; }
        [JsonProperty("FolderLink")]
        public string FolderLink { get; set; }
        [JsonProperty("MinItemId")]
        public int MinItemID { get; set; }
        [JsonProperty("MaxItemId")]
        public int MaxItemID { get; set; }

        public FileServerFolder(string displayName, string folderLink, int minItemID, int maxItemID)
        {
            DisplayName = displayName;
            FolderLink = folderLink;
            MinItemID = minItemID;
            MaxItemID = maxItemID;
        }
    }
}