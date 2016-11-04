using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using System.Threading;
using Steamworks;
using SDG.Unturned;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;

namespace SpyAll
{
    public class Plugin : RocketPlugin<Config>
    {
        public static List<Thread> ThreadsList = new List<Thread>();
        public static Dictionary<CSteamID, Thread> PerUserThreads = new Dictionary<CSteamID, Thread>();
        public static Plugin Instance;

        protected override void Load()
        {
            Instance = this;
            Logger.Log("SpyAll has Loaded!");
        }

        protected override void Unload()
        {
            Logger.Log("SpyAll has Unloaded!");
        }

        public override Rocket.API.Collections.TranslationList DefaultTranslations
        {
            get
            {
                return new Rocket.API.Collections.TranslationList
                {
                    {"finish_spamspy", "Finished taking {0} screenshots of {1}"},
                    {"start_spamspy", "starting taking screenshots..."},
                    {"under_minimum_delay", "{0} is under the minimum delay between screenshots!"},
                    {"player_not_found", "A player by the name of '{0}' could not be found!"},
                    {"invalid_amount", "Invalid value for amount of screenshots, enter a valid amount!"},
                    {"invalid_delay", "Invalid value for delay, enter a valid delay!"},
                    {"wrong_usage", "Invalid command usage, correct usage: {0}"},
                    {"stopped_spies","Stopped {0} spamspies!"}
                };
            }
        }

        public static void SpamSpy(SteamPlayer steamPlayer, int delay, int times, IRocketPlayer caller, Thread t)
        {
            int screenshotsTaken = 0;
            UnturnedPlayer uCaller = null;
            UnturnedPlayer spyedPlayer = null;
            bool callerIsConsole = false;
            bool OSIsWindows = false;
            string moveToDir = "";
            string spyFolderDir = "";
            
            if (Environment.OSVersion.ToString().ToLower().Contains("microsoft"))
                OSIsWindows = true;

            if (Plugin.Instance.Configuration.Instance.SaveSpamspyScreenshotsIndividually)
            {
                moveToDir = Environment.CurrentDirectory;
                if (OSIsWindows)
                {
                    moveToDir = moveToDir.Remove(moveToDir.LastIndexOf(@"\Rocket"), 7);
                    spyFolderDir = moveToDir + @"\Spy\";
                    moveToDir += @"\Spy\" + steamPlayer.playerID.steamID.ToString();
                }
                else
                {
                    moveToDir = moveToDir.Remove(moveToDir.LastIndexOf("/Rocket"), 7);
                    spyFolderDir = moveToDir + @"/Spy/";
                    moveToDir += @"/Spy/" + steamPlayer.playerID.steamID.ToString();
                }

                if (!System.IO.Directory.Exists(moveToDir))
                {
                    System.IO.Directory.CreateDirectory(moveToDir);
                }
            }


            if (caller is ConsolePlayer) callerIsConsole = true;

            sendMessage(caller, Instance.Translate("start_spamspy"));

            while (screenshotsTaken < times)
            {
                if (steamPlayer == null) break;
                if (uCaller == null && !callerIsConsole) uCaller = (UnturnedPlayer)caller;
                if (spyedPlayer == null) spyedPlayer = UnturnedPlayer.FromSteamPlayer(steamPlayer);

                if (caller is ConsolePlayer)
                    steamPlayer.player.sendScreenshot((CSteamID)0);
                else
                    steamPlayer.player.sendScreenshot(uCaller.CSteamID);

                screenshotsTaken++;
                Thread.Sleep(delay);

                CopyScreenshotToFolder(screenshotsTaken, spyedPlayer.CSteamID.ToString(), moveToDir, spyFolderDir, OSIsWindows);
            }

            sendMessage(caller, Instance.Translate("finish_spamspy", times.ToString(), spyedPlayer.DisplayName));
            if (Plugin.ThreadsList.Contains(t))
            {
                Plugin.ThreadsList.Remove(t);
            }

            if (!callerIsConsole)
            {
                if (Plugin.PerUserThreads.ContainsKey(uCaller.CSteamID))
                {
                    Plugin.PerUserThreads.Remove(uCaller.CSteamID);
                }
            }
        }

        static void sendMessage(IRocketPlayer caller, string msg)
        {
            if (caller is ConsolePlayer)
            {
                Logger.Log(msg);
            }
            else
            {
                UnturnedChat.Say(caller, msg);
            }
        }

        public static int GetMinDelayFromConfig()
        {
            return Instance.Configuration.Instance.MinDelayBetweenScreenshots;
        }

        public static void CopyScreenshotToFolder(int count, string CSteamId, string moveToDir, string spyFolderDir, bool OSIsWindows)
        {
            if (!Plugin.Instance.Configuration.Instance.SaveSpamspyScreenshotsIndividually) return;

            string outputDir = "";

            if (OSIsWindows) outputDir = moveToDir + @"\" + CSteamId + "-" + count.ToString() + ".jpg";
            else outputDir = moveToDir + "/" + CSteamId + "-" + count.ToString() + ".jpg";

            if (!System.IO.File.Exists(outputDir))
            {
                try
                {
                    System.IO.File.Copy(spyFolderDir + CSteamId + ".jpg", outputDir);
                }
                catch { }
            }
            else
            {
                try
                {
                    System.IO.File.Delete(outputDir);
                    System.IO.File.Copy(spyFolderDir + CSteamId + ".jpg", outputDir);
                }
                catch { }
            }
        }
    }

}
