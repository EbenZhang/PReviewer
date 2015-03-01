using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
