using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace AppNotification.Services;

public class TelegramBotService : IHostedService
{
    private readonly IOptions<BotConfiguration> _botConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TelegramBotClient _botClient;

    public TelegramBotService(IOptions<BotConfiguration> botConfig, IHttpClientFactory httpClientFactory)
    {
        _botConfig = botConfig;
        _httpClientFactory = httpClientFactory;
        var options = new TelegramBotClientOptions(_botConfig.Value.BotToken);
        _botClient = new TelegramBotClient(options, _httpClientFactory.CreateClient("telegram_bot_client"));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //var options = new TelegramBotClientOptions(_botConfig.Value.BotToken);
        //var _botClient = new TelegramBotClient(options, _httpClientFactory.CreateClient("telegram_bot_client"));
        var U = 6189079292;
        var N = 5952881968;
        var m = "This is Notification";
        await SendMessage(U, m);
        var message = "Subscribe";

        await SendReplyKeyboard(U, message, cancellationToken);


    }

    public async Task SendMessage(long U, string m)
    {
        await _botClient.SendTextMessageAsync(chatId: U, text: m);
    }



    public async Task<Message> SendReplyKeyboard(long chatId, string message, CancellationToken cancellationToken)
    {

        InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                new[] { InlineKeyboardButton.WithCallbackData("Subscribe", "request_contact_callback") }
                });


        return await _botClient.SendTextMessageAsync(
             chatId: chatId,
             text: message,
             replyMarkup: inlineKeyboard,
             cancellationToken: cancellationToken
         );
    }

    //public async Task<Message> SendReplyKeyboard(long chatId, string message, CancellationToken cancellationToken)
    //{
    //    InlineKeyboardMarkup inlineKeyboard = new(
    // new[]
    // {
    //    new[] { new InlineKeyboardButton("Subscribe") }
    // });

    //    return await _botClient.SendTextMessageAsync(
    //        chatId: chatId,
    //        text: message,
    //        replyMarkup: inlineKeyboard,
    //        cancellationToken: cancellationToken);
    //}

    public async Task StopAsync(CancellationToken cancellationToken)
    {

    }
}

