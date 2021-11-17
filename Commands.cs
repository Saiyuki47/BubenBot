using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using BubenBot;
using Victoria;

namespace BubenBot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        string printString = "";
        Random rand = new Random();
        //Get Target UserName from Config
        string TargetName = Config.ConfigProperties.TargetUserName;
        //Check if a Specific user is on the server
        private bool IsTargetOnServer => Context.Guild.Users.Any(x => x.Username.Equals(TargetName));

        public static string voiceLinesPath { get; set; } = Directory.GetCurrentDirectory() + "\\VoiceLines\\";


        public async Task InitializeVoiceLinesFolder()
        {
            if (!File.Exists(voiceLinesPath))
            {
                Directory.CreateDirectory(voiceLinesPath);
                await LoggingService.LogAsync(new Discord.LogMessage(LogSeverity.Error, "Source", "No VoiceLines Folder found! A new one was generated"));
            }
            else 
            {
                await LoggingService.LogAsync(new Discord.LogMessage(LogSeverity.Info, "Source", "VoiceLines Folder found :D"));
            }

        }

        List<string> befehle = new List<string>()
            {
                "help",
                "ping",
                "ban",
                "hs",
                "age",
                "echo",
            };

        [Command("help")]
        [Remarks("Help")]
        [Summary("Finds all the modules and prints out it's summary tag.")]
        public async Task Help()
        {
            printString = "";
            bool IsTargetOnServer = Context.Guild.Users.Any(x => x.Username.Equals(TargetName));
            printString += "Hier sind meine Befehle: ";
            foreach (string str in befehle)
            {
                printString += "\n-" + str;
            }

            if (IsTargetOnServer)
            {
                printString += "\n-madi";
            }

            await ReplyAsync(embed: await EmbedHandler.CreateBasicEmbed("Help",
            printString,
            Color.Green));
            

        }

        [Command("ping", RunMode = RunMode.Async)]
        public async Task Ping()
        {
     
            await ReplyAsync("pong");
            
        }

        [Command("ban")]
        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "Du hast kein Rechte, um Leute zu bannen lol")]
        public async Task BanMember(IGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
            {
                await ReplyAsync("Bitte gib einen User an");
                return;
            }
            if (reason == null) reason = "nicht angegeben";

            await Context.Guild.AddBanAsync(user, 0, reason);

            var EmbedBuilder = new EmbedBuilder()
                .WithDescription($":white_check_mark: {user.Mention} wurde gebannt\n**Grund** {reason}")
                .WithFooter(footer =>
                {
                    footer
                    .WithText("Lmao ez")
                    .WithIconUrl("https://i.imgur.com/6Bi17B3.png");
                });
            Embed embed = EmbedBuilder.Build();
            await ReplyAsync(embed: embed);

            ITextChannel logChannel = Context.Client.GetChannel(642698444431032330) as ITextChannel;
            var EmbedBuilderLog = new EmbedBuilder()
                .WithDescription($"{user.Mention} wurde gebannt\n**Grund** {reason}\n**Moderator** {Context.User.Mention}")
                .WithFooter(footer =>
                {
                    footer
                    .WithText("Lmao ez")
                    .WithIconUrl("https://i.imgur.com/6Bi17B3.png");
                });
            Embed embedLog = EmbedBuilderLog.Build();


        }

        #region Madi is a Bitch
        //Gets a random Audio from directory and plays it
        [Command("madi", RunMode = RunMode.Async)]
        public async Task JoinChannel([Remainder] IVoiceChannel channel = null)
        {
            // Get the audio channel
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await channel.ConnectAsync();

            string[] files = Directory.GetFiles(voiceLinesPath, "*.mp3");

            await SendAsync(audioClient, files[rand.Next(files.Length)]);
            //await Context.Channel.SendMessageAsync(files[rand.Next(files.Length)]);

            await channel.DisconnectAsync();
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
        }
        #endregion

        [Command("hs")]
        public async Task HS(string usr = null)
        {
            if (usr == null)
            {
                await ReplyAsync("Bitte gib einen User an (mit oder ohne @)");
                return;
            }

            int random = rand.Next(1,100);

            if(Context.User.Username.Equals(TargetName) && random == 1)
            {
                await ReplyAsync("Selber Huso!!!!",true);
                return;
            }

            await ReplyAsync(usr + " ist ein Hurensohn!",true);

        }

        [Command("age")]
        public async Task AGE(IGuildUser user = null)
        {
            string age = "";
            if (user==null)
            {
                age = "Dein Account wurde am " + Context.User.CreatedAt.ToString() + " erstellt.";
            }
            else
                age = "Der Account von "+ user.Username + " wurde am " + user.CreatedAt.ToString() + " erstellt.";

            await ReplyAsync(embed: await EmbedHandler.CreateBasicEmbed("Age", age, Color.Green));
        }

        /////////////////////////////Not Good but works
        [Command("meme")]
        public async Task MeMe(long index = 0)
        {
            DirectoryInfo d = new DirectoryInfo(Environment.CurrentDirectory + @"\memes");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles(); //Getting Text files

            if (index > Files.Count())
            {
                await ReplyAsync(embed: await EmbedHandler.CreateErrorEmbed("MEME Command",
                                            "Number exceeds the number of memes available"));
            }
            else if (index > 0)
            {
                for (int i = 0; i < index; i++)
                {
                    await Context.Channel.SendFileAsync(Files[rand.Next(0, Files.Count())].ToString());
                }
            }
            else
                await Context.Channel.SendFileAsync(Files[rand.Next(0, Files.Count())].ToString());

        }

        [Command("echo")]
        public Task SayAsync([Remainder] string echo)
        => ReplyAsync(echo);

        [Command("lsuser")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "Du hast kein Rechte, um Leute aufzulisten lol")]
        public async Task LSUserAsync()
        {
            foreach(IUser usr in Context.Guild.Users)
            {
                await ReplyAsync(usr.Username.ToString()+", "+usr.Id);
                if (usr.Username == "Sir Salafist")
                {
                    await ReplyAsync("Alter Madi ist ja hier");
                }
            }
            await ReplyAsync("done");
            return;
        }



    }
}
