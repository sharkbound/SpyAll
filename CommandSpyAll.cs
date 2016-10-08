using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace SpyAll
{
    class CommandSpyAll : IRocketCommand
    {
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return Rocket.API.AllowedCaller.Both; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            foreach (var steamplayer in Provider.Players)
            {
                steamplayer.Player.sendScreenshot((CSteamID)0);
            }
        }

        public string Help
        {
            get { return "Screenshots all players connected"; }
        }

        public string Name
        {
            get { return "spyall"; }
        }

        public List<string> Permissions
        {
            get { return new List<string> { "spyall" }; }
        }

        public string Syntax
        {
            get { return ""; }
        }
    }
}
