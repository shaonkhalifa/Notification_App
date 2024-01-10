using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace AppNotification.Services;

public class TelegramBotService : IHostedService
{
    private readonly IOptions<BotConfiguration> _botConfig;
    private readonly IHttpClientFactory _httpClientFactory;

    public TelegramBotService(IOptions<BotConfiguration> botConfig, IHttpClientFactory httpClientFactory)
    {
        _botConfig = botConfig;
        _httpClientFactory = httpClientFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = new TelegramBotClientOptions(_botConfig.Value.BotToken);
        var client = new TelegramBotClient(options, _httpClientFactory.CreateClient("telegram_bot_client"));


    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {

    }
}

