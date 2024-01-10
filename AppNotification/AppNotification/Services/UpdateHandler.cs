using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace AppNotification.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        
        var handler = update switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:

            { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            { InlineQuery: { } inlineQuery } => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
            { ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
            { Poll: { } pollAnswer } => BotPollAnswerReceived(pollAnswer, update, cancellationToken),



            //UpdateType.PollAnswer pollAnswer => HandlePollAnswerAsync(pollAnswer, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Contact != null)
        {
            await BotContactSharedReceived(message, cancellationToken);
        }
        if (message.Text is not { } messageText)
            return;


        var action = messageText.Split(' ')[0] switch
        {
            "/subscribe" => RequestForSubscribtions(_botClient, message, cancellationToken),
            "/inline_keyboard" => SendInlineKeyboard(_botClient, message, cancellationToken),
            "/keyboard" => SendReplyKeyboard(_botClient, message, cancellationToken),
            "/remove" => RemoveKeyboard(_botClient, message, cancellationToken),
            "/photo" => SendFile(_botClient, message, cancellationToken),
            "/request" => RequestContactAndLocation(_botClient, message, cancellationToken),
            "/request_contact" => RequestContact(_botClient, message, cancellationToken),
            "/inline_mode" => StartInlineQuery(_botClient, message, cancellationToken),
            "/throw" => FailingHandler(_botClient, message, cancellationToken),

            _ => Usage(_botClient, message, cancellationToken)
        };
        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);

        static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);

            await Task.Delay(500, cancellationToken);

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    },
                });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                })
            {
                ResizeKeyboard = true
            };

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Removing keyboard",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendFile(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                message.Chat.Id,
                ChatAction.UploadPhoto,
                cancellationToken: cancellationToken);

            const string filePath = "Files/tux.png";
            await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            return await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: new InputFileStream(fileStream, fileName),
                caption: "Nice Picture",
                cancellationToken: cancellationToken);
        }
        static async Task<Message> RequestContact(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var requestContactButton = new KeyboardButton("Share Contact")
            {
                RequestContact = true
            };

            var requestReplyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                 new[] { requestContactButton }
            });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Please share your contact information:",
                replyMarkup: requestReplyKeyboard,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> RequestContactAndLocation(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            KeyboardButton shareContactButton = new KeyboardButton("Share Contact");
            shareContactButton.RequestContact = true;


            ReplyKeyboardMarkup RequestReplyKeyboard = new(
             new[]
             {
                new[] { shareContactButton }
             });


            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Please share your contact information:",
                replyMarkup: RequestReplyKeyboard,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> RequestForSubscribtions(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var pollOptions = new[]
            {
                new PollOption(),
                new PollOption(),
                new PollOption(),
            };
            pollOptions[0].Text = "Option 1";
            pollOptions[1].Text = "Option 2";
            pollOptions[2].Text = "Option 3";

            var poll = new Poll
            {
                Question = "Choose your subscription preference:",
                Options = pollOptions,
            };
            var stringOptions = pollOptions.Select(option => option.Text).ToArray();

            await botClient.SendPollAsync(
               chatId: message.Chat.Id,
               question: "Which is your Company?",
               options: stringOptions,
               cancellationToken: cancellationToken
           );
            return await botClient.SendTextMessageAsync(
                 chatId: message.Chat.Id,
                 text: "Please share your phone number to complete the subscription:",
                 replyMarkup: new ReplyKeyboardMarkup(
                     new[] { new KeyboardButton("Share Contact") }),
                 cancellationToken: cancellationToken
             );

            //return await botClient.SendTextMessageAsync(
            //    chatId: message.Chat.Id,
            //    text: "Waiting for your phone number...",
            //    cancellationToken: cancellationToken
            //);
        }



        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            const string usage = "Usage:\n" +
                                 "/subscribe  - send inline keyboard\n" +
                                 "/keyboard    - send custom keyboard\n" +
                                 "/remove      - remove custom keyboard\n" +
                                 "/photo       - send a photo\n" +
                                 "/request     - request location or contact\n" +
                                 "/inline_mode - send keyboard with Inline Query";

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> StartInlineQuery(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Inline Mode"));

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Press the button to start Inline Query",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Remove unused parameter
        static Task<Message> FailingHandler(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            throw new IndexOutOfRangeException();
        }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter.
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        await _botClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"Received {callbackQuery.Data}",
            cancellationToken: cancellationToken);

        await _botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: $"Received {callbackQuery.Data}",
            cancellationToken: cancellationToken);
    }
    public async Task BotContactSharedReceived(Message message, CancellationToken cancellationToken)
    {
        string a = "8801745427269";
        string b = "01721415244";

        string phoneNumber = message.Contact.PhoneNumber;
        long? userId = message.Contact.UserId;
        string userName = message.Contact.FirstName + message.Contact.LastName;
        long chatId = message.Chat.Id;
        if (phoneNumber == a)
        {
            var pollOptions = new[]
            {
                new PollOption(),
                new PollOption(),
                new PollOption(),
            };
            pollOptions[0].Text = "EYE";
            pollOptions[1].Text = "Google";
            pollOptions[2].Text = "Microsoft";

            var poll = new Poll
            {
                Question = "Choose your subscription preference:",
                Options = pollOptions,
            };
            var stringOptions = pollOptions.Select(option => option.Text).ToArray();

            await _botClient.SendPollAsync(
               chatId: message.Chat.Id,
               question: "Which is your Company?",
               options: stringOptions,
               cancellationToken: cancellationToken
           );
        }
        else
        {

            

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"Thank you for sharing your phone number: {phoneNumber}  You will get regular Update",
                cancellationToken: cancellationToken);

            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow; // Example
            DateTime dateTime = dateTimeOffset.DateTime;

            await _botClient.RestrictChatMemberAsync(
                chatId: message.Chat.Id,
                userId: message.From.Id, // Restrict the specific user who shared the contact
                permissions: new ChatPermissions
                {
                    CanSendMessages = false,
                },
                untilDate: dateTime.AddMinutes(5),
                cancellationToken: cancellationToken);


        }


    }

    private async Task BotPollAnswerReceived(Poll poll, Update update, CancellationToken cancellationToken)
    {
        var pollId = poll.Id;
        var userId = poll.Id;
        var chosenOptionId = update.Id;

        // var userId = BotPollAnswer(update,cancellationToken);

        Console.WriteLine($"User {userId} voted for option {chosenOptionId} in poll {pollId}");

        var chatId = update.CallbackQuery.Message.Chat.Id;

        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Thank you for voting in the poll!",
            cancellationToken: cancellationToken
        );
    }

    //private async Task<string> BotPollAnswer(Update update, CancellationToken cancellationToken)
    //{
    //    var poll = update.Poll;
    //    var pollAnswer = update.PollAnswer; 

    //    var chatId = pollAnswer.ch; 
    //    return chatId.ToString();

    //}

    #region Inline Mode

    private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "1",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent("hello"))
        };

        await _botClient.AnswerInlineQueryAsync(
            inlineQueryId: inlineQuery.Id,
            results: results,
            cacheTime: 0,
            isPersonal: true,
            cancellationToken: cancellationToken);
    }

    private async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);

        await _botClient.SendTextMessageAsync(
            chatId: chosenInlineResult.From.Id,
            text: $"You chose result with Id: {chosenInlineResult.ResultId}",
            cancellationToken: cancellationToken);
    }

    #endregion

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RCS1163 // Unused parameter.
    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
#pragma warning restore RCS1163 // Unused parameter.
#pragma warning restore IDE0060 // Remove unused parameter
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task SendMessage(long U,string m)
    {
        await _botClient.SendTextMessageAsync(chatId: U, text: m);
    }
}

