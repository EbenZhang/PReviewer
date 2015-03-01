using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ExtendedCL;
using Octokit;
using PReviewer.Domain;

namespace PReviewer.Model
{
    public class CompareToolSettingsPersist : ICompareToolSettingsPersist
    {
        private static readonly string CompareToolSettingsFile = Path.Combine(PathHelper.ProcessAppDir, "CompareTools.xml");
        public CompareToolSettingsContainer Load()
        {
            if (!File.Exists(CompareToolSettingsFile))
            {
                return new CompareToolSettingsContainer();
            }

            using (var stream = File.OpenRead(CompareToolSettingsFile))
            {
                var container = new XmlSerializer(typeof(CompareToolSettingsContainer)).Deserialize(stream) as CompareToolSettingsContainer;
                return container;
            }
        }

        public void Save(CompareToolSettingsContainer serializable)
        {
            if (File.Exists(CompareToolSettingsFile))
            {
                File.Delete(CompareToolSettingsFile);
            }
            using (var stream = File.OpenWrite(CompareToolSettingsFile))
            {
                new XmlSerializer(typeof(CompareToolSettingsContainer)).Serialize(stream, serializable);
            }
        }
    }
}
