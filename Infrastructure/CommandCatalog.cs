using System.Collections.Generic;

namespace BubenBot.Infrastructure;

public static class CommandCatalog
{
    // Base commands always shown in !help
    public static IReadOnlyList<string> BaseCommands { get; } = new[]
    {
        "help",
        "ping",
        "ban",
        "hs",
        "age",
        "echo",
        "meme",
        "lsuser",
        "votekick"
    };

    // Commands only shown when the target user is on the server
    public static IReadOnlyList<string> TargetUserCommands { get; } = new[]
    {
        "madi"
    };
}
