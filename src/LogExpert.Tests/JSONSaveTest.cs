using LogExpert.Config;
using Newtonsoft.Json;
using NUnit.Framework;
using System.IO;

namespace LogExpert.Tests
{
    [TestFixture]
    public class JSONSaveTest
    {
        [Test(Author = "Hirogen", Description = "Save Options as JSON and Check if the written file can be cast again into the settings object")]
        public void SaveOptionsAsJSON()
        {
            ConfigManager.Settings.alwaysOnTop = true;
            ConfigManager.Save(SettingsFlags.All);
            string configDir = ConfigManager.ConfigDir;
            string settingsFile = configDir + "\\settings.json";

            Settings settings = null;
            
            Assert.DoesNotThrow(CastSettings);
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.alwaysOnTop, Is.True);

            ConfigManager.Settings.alwaysOnTop = false;
            ConfigManager.Save(SettingsFlags.All);
            
            settings = null;
            Assert.DoesNotThrow(CastSettings);

            Assert.That(settings, !Is.Null);
            Assert.That(settings.alwaysOnTop, Is.False);


            void CastSettings()
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFile));
            }
        }
    }
}
