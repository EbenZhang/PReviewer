using System;
using System.IO;
using System.Xml.Serialization;
using ExtendedCL;

namespace PReviewer.Model
{
    [Serializable]
    public class UserPreferences
    {
        private static readonly string PreferencePath = Path.Combine(PathHelper.ProcessAppDir, "Preferences.xml");

        public bool ShowWhiteSpaces = false;

        protected UserPreferences() { }

        public static UserPreferences Instance;

        static UserPreferences()
        {
            Instance = Load();
        }

        public static UserPreferences Load()
        {
            if (!File.Exists(PreferencePath)) return new UserPreferences();
            using (var stream = File.OpenRead(PreferencePath))
            {
                return new XmlSerializer(typeof(UserPreferences)).Deserialize(stream) as UserPreferences;
            }
        }

        public void Save()
        {
            if (File.Exists(PreferencePath))
            {
                File.Delete(PreferencePath);
            }

            using (var stream = File.OpenWrite(PreferencePath))
            {
                new XmlSerializer(typeof(UserPreferences)).Serialize(stream, this);
            }
        }
    }
}
