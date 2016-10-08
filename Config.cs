using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.API;

namespace SpyAll
{
    public class Config : IRocketPluginConfiguration
    {
        public int MinDelayBetweenScreenshots;
        public bool SaveSpamspyScreenshotsIndividually;

        public void LoadDefaults()
        {
            MinDelayBetweenScreenshots = 100;
            SaveSpamspyScreenshotsIndividually = false;
        }
    }
}
