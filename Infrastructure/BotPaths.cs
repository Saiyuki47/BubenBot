using System;
using System.IO;

namespace BubenBot.Infrastructure;

public static class BotPaths
{
    public static string BaseDirectory => AppContext.BaseDirectory;

    public static string ConfigFilePath => Path.Combine(BaseDirectory, "config.json");

    public static string VoiceLinesDirectory => Path.Combine(BaseDirectory, "VoiceLines");

    public static string MemesDirectory => Path.Combine(BaseDirectory, "memes");
}
