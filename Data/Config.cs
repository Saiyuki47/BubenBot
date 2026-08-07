using System.Threading.Tasks;
using BubenBot.Data;

namespace BubenBot;

public class Config
{
    public static ConfigModel ConfigProperties { get; private set; } = new();

    private readonly ConfigStore _store = new();

    public async Task InitializeConfigData()
    {
        ConfigProperties = await _store.LoadAsync();
    }
}
