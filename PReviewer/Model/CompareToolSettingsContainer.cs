using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PReviewer.Domain;

namespace PReviewer.Model
{
    [Serializable]
    public class CompareToolSettingsContainer
    {
        public int CurrentActiveIndex { get; set; }
        public List<SerializableCompareToolSettings> CompareToolSettingses { get; set; }

        public CompareToolSettingsContainer()
        {
            CompareToolSettingses = new List<SerializableCompareToolSettings>();
            CurrentActiveIndex = -1;
        }

        public CompareToolSettingsContainer(IEnumerable<CompareToolSettings> toolSettinges, int activeIndex)
        {
            CompareToolSettingses = new List<SerializableCompareToolSettings>(toolSettinges.Select(x => x.ToSerializable()));
            CurrentActiveIndex = activeIndex;
        }

        public IEnumerable<CompareToolSettings> GetCompareTools()
        {
            return CompareToolSettingses.Select(x => new CompareToolSettings(x));
        }
    }
}
