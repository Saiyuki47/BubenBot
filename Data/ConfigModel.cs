using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace BubenBot.Data
{
    public class ConfigModel
    {
        //Token of the Bot
        public string Token { get; set; }
        //Prefix for the Bot commands
        public string Prefix { get; set; }
        //1:Playing 2:Listens 3:Wachting
        public ActivityType Activity { get; set; }
        //Comes after Activity
        public string Status { get; set; }
        //The Name of the Target for example "Tim32"
        public string TargetUserName { get; set; } = "defaultuser";
        //Path of the Folder with the Voiceline
        public string VoiceLinePath { get; set; }

    }
}
