using Discord;
using System.Threading.Tasks;

namespace BubenBot
{
    public static class EmbedHandler
    {
        public static Task<Embed> CreateBasicEmbed(string title, string description, Color color)
        {
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .WithCurrentTimestamp()
                .Build();

            return Task.FromResult(embed);
        }

        public static Task<Embed> CreateErrorEmbed(string source, string error)
        {
            var embed = new EmbedBuilder()
                .WithTitle($"ERROR OCCURED FROM - {source}")
                .WithDescription($"**Error Details**: \n{error}")
                .WithColor(Color.DarkRed)
                .WithCurrentTimestamp()
                .Build();

            return Task.FromResult(embed);
        }
    }
}
