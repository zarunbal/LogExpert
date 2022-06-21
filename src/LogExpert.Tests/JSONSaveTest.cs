using System.IO;
using LogExpert.Config;
using Newtonsoft.Json;
using NUnit.Framework;

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
            Assert.NotNull(settings);
            Assert.True(settings.alwaysOnTop);

            ConfigManager.Settings.alwaysOnTop = false;
            ConfigManager.Save(SettingsFlags.All);
            
            settings = null;
            Assert.DoesNotThrow(CastSettings);
            Assert.NotNull(settings);
            Assert.False(settings.alwaysOnTop);


            void CastSettings()
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFile));
            }
        }
    }
}
