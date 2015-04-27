using System;
namespace PReviewer.Model
{
    [Serializable]
    public class SerializableCompareToolSettings
    {
        public string Name { get; set; }
        public string Parameters { get; set; }
        public string ExePath { get; set; }
    }
}
