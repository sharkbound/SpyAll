using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.API;
using Rocket.Unturned.Player;
using Steamworks;
using SDG.Unturned;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using System.Threading;

namespace SpyAll
{
    class CommandSpamSpy : IRocketCommand
    {
        public void Execute(IRocketPlayer caller, string[] command)
        {

            if (command.Length == 1)
            {
                if (command[0].ToLower() == "stop")
                {

                    int amountStopped = clearAndStopAllSpyThreads(ref Plugin.ThreadsList);
                    clearAndStopAllSpyThreads(ref Plugin.PerUserThreads);

                    sendMessage(caller, Plugin.Instance.Translate("stopped_spies", amountStopped));

                    return;
                }
            }

            int enteredDelay = 500;
            int enteredAmount = 2;

            if (command.Length != 3)
            {
                sendMessage(caller, Plugin.Instance.Translate("wrong_usage", Syntax));
                return;
            }

            UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[0]);
            if (otherPlayer == null)
            {
                sendMessage(caller, Plugin.Instance.Translate("player_not_found", command[0]));
                return;
            }
            
            if (!int.TryParse(command[1], out enteredAmount))
            {
                sendMessage(caller, Plugin.Instance.Translate("invalid_amount"));
                return;
            }

            if (!int.TryParse(command[2], out enteredDelay))
            {
                sendMessage(caller, Plugin.Instance.Translate("invalid_delay"));
                return;
            }

            if (Plugin.GetMinDelayFromConfig() != 0)
            {
                if (enteredDelay < Plugin.GetMinDelayFromConfig())
                {
                    UnturnedChat.Say(caller, Plugin.Instance.Translate("under_minimum_delay", enteredDelay));
                    return;
                }
            }

            SteamPlayer steamP = null;
            foreach (var p in Provider.clients)
            {
                if (p.playerID.steamID == otherPlayer.CSteamID)
                {
                    steamP = p;
                    break;
                }
            }

            if (steamP == null) return;

            if (!(caller is ConsolePlayer)) clearAndStopSpecificUserThreads(ref Plugin.PerUserThreads, ((UnturnedPlayer)caller).CSteamID);

            Thread t = null;
            t = new Thread(() =>
            {
                Plugin.SpamSpy(steamP, enteredDelay, enteredAmount, caller, t);
            });
            t.Start();
            Plugin.ThreadsList.Add(t);
            if (!(caller is ConsolePlayer)) Plugin.PerUserThreads.Add(((UnturnedPlayer)caller).CSteamID, t);
        }

        public List<string> Aliases
        {
            get { return new List<string> { "ss" }; }
        }

        public AllowedCaller AllowedCaller
        {
            get { return Rocket.API.AllowedCaller.Both; }
        }

        public string Help
        {
            get { return "Rapidly takes screenshots of a player"; }
        }

        public string Name
        {
            get { return "spamspy"; }
        }

        public List<string> Permissions
        {
            get { return new List<string> { "spamspy" }; }
        }

        public string Syntax
        {
            get { return "<player> <amountOfScreenshots> <delayBetweenScreenshots>"; }
        }

        void sendMessage(IRocketPlayer caller, string msg)
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

        int clearAndStopAllSpyThreads(ref List<Thread> list)
        {
            int amountStopped = 0;
            for (int jj = list.Count - 1; jj >= 0; jj--)
            {
                list[jj].Abort();
                list.Remove(list[jj]);
                amountStopped++;
            }

            return amountStopped;
        }

        void clearAndStopSpecificUserThreads(ref List<Thread> list, Thread t)
        {
            if (list.Contains(t))
            {
                int index = list.IndexOf(t);
                list[index].Abort();
                list.Remove(t);
            }
        }

        int clearAndStopAllSpyThreads(ref Dictionary<CSteamID, Thread> dictionary)
        {
            List<CSteamID> toRemove = new List<CSteamID>();
            int amountStopped = 0;

            foreach (var pair in dictionary)
            {
                pair.Value.Abort();
                toRemove.Add(pair.Key);
                amountStopped++;
            }

            foreach (var steamID in toRemove)
            {
                dictionary.Remove(steamID);
            }
            toRemove = new List<CSteamID>();

            return amountStopped;
        }

        void clearAndStopSpecificUserThreads(ref Dictionary<CSteamID, Thread> dictionary, CSteamID ID)
        {
            Thread t = null;

            if (dictionary.ContainsKey(ID))
            {
                t = dictionary[ID];
                dictionary[ID].Abort();
                dictionary.Remove(ID);

                if (t != null)
                {
                    clearAndStopSpecificUserThreads(ref Plugin.ThreadsList, t);
                }
            }

            
        }

    }
}
