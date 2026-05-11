using NetCord;
using NetCord.Gateway;

namespace SchemaBot;

class Program
{
    private static string _token = Environment.GetEnvironmentVariable("DISCORD_TOKEN")
                   ?? throw new InvalidOperationException("DISCORD_TOKEN environment variable not set");

    private static ulong _targetChannelId = ulong.Parse(Environment.GetEnvironmentVariable("TARGET_CHANNEL") 
                                                ?? throw new InvalidOperationException("TARGET_CHANNEL environment variable not set"));
    private static ulong _pinnedMessageId = ulong.Parse(Environment.GetEnvironmentVariable("PINNED_MESSAGE")
                                                ?? throw new InvalidOperationException("PINNED_MESSAGE environment variable not set"));
    private static string[] _triggers = (Environment.GetEnvironmentVariable("STRING_TRIGGERS")
                                         ?? throw new InvalidOperationException("STRING_TRIGGERS environment variable not set"))
                                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    private static GatewayClient _client = new(new BotToken(_token), new GatewayClientConfiguration()
    {
        Intents = GatewayIntents.GuildMessages
                  | GatewayIntents.MessageContent
                  | GatewayIntents.Guilds
    });
    
    static async Task Main(string[] args)
    {
        _client.MessageCreate += async (message) =>
        {
            if (message.Author.IsBot || !message.GuildId.HasValue) return; // Discard messages from bots, webhooks or direct
            if (message.ChannelId == _targetChannelId) return; // Prevent recursive loops by ignoring target channel

            bool containsTrigger = _triggers.Any(str =>
                message.Content.Contains(str, StringComparison.Ordinal));
            
            if (!containsTrigger) return;
            
            string pinnedMessageLink =
                $"https://discord.com/channels/{message.GuildId}/{_targetChannelId}/{_pinnedMessageId}";
            string userMention = $"<@{message.Author.Id}>";
            string responsePayload = $"{userMention}, var god läs schemat: {pinnedMessageLink}";

            await _client.Rest.SendMessageAsync(_targetChannelId, responsePayload);
        };

        await _client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }
}