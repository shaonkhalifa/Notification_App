using AppNotification.Context;
using AppNotification.Enitiy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Numerics;
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
    private readonly UserInfoService _service;
   
    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, UserInfoService service)
    {
        _botClient = botClient;
        _logger = logger;
        _service = service;
        
       
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {

        var handler = update switch
        {

            { Message: { } message } => BotContactSharedReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            { PollAnswer: { } pollAnswer } => BotPollAnswerReceived(pollAnswer, cancellationToken),
            { MyChatMember: { } chatmember } => HandelBlock(chatmember, cancellationToken),

            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }


    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        var a = callbackQuery.From.Id;
        var b = callbackQuery.From.Username;
        var c = callbackQuery.From.FirstName;
        var d = callbackQuery.From.LastName;
        var f = callbackQuery.Message!.Chat.Id;
        var g = callbackQuery.Message!.Contact;

        if (callbackQuery.Data == "request_contact_callback")
        {
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(new[] { new InlineKeyboardButton[0] });
            await _botClient.EditMessageReplyMarkupAsync(chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId, replyMarkup: keyboard);
        }


        KeyboardButton requestPhoneButton = KeyboardButton.WithRequestContact("Share Contact");


        ReplyKeyboardMarkup replyKeyboard = new ReplyKeyboardMarkup(requestPhoneButton);
        replyKeyboard.Keyboard = new[] { new[] { requestPhoneButton } };
        replyKeyboard.OneTimeKeyboard = true;
        replyKeyboard.ResizeKeyboard = true;
        replyKeyboard.InputFieldPlaceholder = " ";


        await _botClient.SendTextMessageAsync(
           chatId: callbackQuery.Message.Chat.Id,
           text: "Please share your contact information:",
           replyMarkup: replyKeyboard,
           cancellationToken: cancellationToken);



    }



    public async Task BotContactSharedReceived(Message message, CancellationToken cancellationToken)
    {

        if (message.Contact != null)
        {

            string a = "+8801745427269";
            string b = "01721415244";

            string phoneNumber = message.Contact.PhoneNumber;
            long? userId = message.Contact.UserId;
            string userName = message.Contact.FirstName + message.Contact.LastName;
            long chatId = message.Chat.Id;


            if (phoneNumber != a)
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
                   isAnonymous: false,
                   cancellationToken: cancellationToken
                   );
            }
            else
            {

                long c = message.Chat.Id;
                long? u = message.Contact.UserId;

                await _service.UserInfoUpdateAsync(phoneNumber, c, u);
               // await UserInfoUpdateAsync(phoneNumber, c, u);


                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Thank you for sharing your phone number: {phoneNumber}  You will get regular Update",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);



                //DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow; // Example
                //DateTime dateTime = dateTimeOffset.DateTime;

                //await _botClient.RestrictChatMemberAsync(
                //    chatId: message.Chat.Id,
                //    userId: message.From.Id, // Restrict the specific user who shared the contact
                //    permissions: new ChatPermissions
                //    {
                //        CanSendMessages = false,
                //    },
                //    untilDate: dateTime.AddMinutes(5),
                //    cancellationToken: cancellationToken);


            }


        }
        else
        {
            if (message.Text == "/start")
            {
                InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                     new[] { InlineKeyboardButton.WithCallbackData("Subscribe", "request_contact_callback") }
                });

                await _botClient.SendTextMessageAsync(
                     chatId: message.Chat.Id,
                     text: "Please Subscribe",
                     replyMarkup: inlineKeyboard,
                     cancellationToken: cancellationToken
                 );


                //    ReplyKeyboardMarkup replyKeyboardMarkup = new(
                //   new[]
                //   {
                //            new KeyboardButton[] { "subscribe" },

                //   })
                //    {
                //        ResizeKeyboard = true,
                //        OneTimeKeyboard = true,
                //        InputFieldPlaceholder = " "
                //};

                //    await _botClient.SendTextMessageAsync(
                //       chatId: message.Chat.Id,
                //       text: "Pleace subscribe",
                //       replyMarkup: replyKeyboardMarkup,
                //       cancellationToken: cancellationToken);

            }

            //else if (message.Text == "subscribe")
            //{

            //    KeyboardButton requestPhoneButton = KeyboardButton.WithRequestContact("Share Contact");


            //    ReplyKeyboardMarkup replyKeyboard = new ReplyKeyboardMarkup(requestPhoneButton);
            //    replyKeyboard.Keyboard = new[] { new[] { requestPhoneButton } };
            //    replyKeyboard.OneTimeKeyboard = true;
            //    replyKeyboard.ResizeKeyboard = true;
            //    replyKeyboard.InputFieldPlaceholder = " ";


            //    await _botClient.SendTextMessageAsync(
            //       chatId: message.Chat.Id,
            //       text: "Please share your contact information:",
            //       replyMarkup: replyKeyboard,

            //    cancellationToken: cancellationToken);

            //}

            else
            {
                await _botClient.DeleteMessageAsync(
                 chatId: message.Chat.Id,
                 messageId: message.MessageId,
                 cancellationToken: cancellationToken
                 );
            }

        }

    }


    private async Task BotPollAnswerReceived(PollAnswer pollAnswer, CancellationToken cancellationToken)
    {
        var pollId = pollAnswer.PollId;
        var userId = pollAnswer.User.Id;
        var chosenOptionId = pollAnswer.OptionIds;


        await _botClient.SendTextMessageAsync(
            chatId: pollAnswer.User.Id,
            text: $" You will get regular Update",
            cancellationToken: cancellationToken
        );


    }

    //HandelBlock

    private async Task HandelBlock(ChatMemberUpdated chatMember, CancellationToken cancellationToken)
    {
        var date = chatMember.Date;
        var a = chatMember.NewChatMember.Status;
        var b = chatMember.OldChatMember.Status;


        await _botClient.SendTextMessageAsync(
            chatId: chatMember.Chat.Id,
            text: $"You block the Notification App",
            cancellationToken: cancellationToken
        );
    }
    

   




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


        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

    }

    //public async Task UserInfoUpdateAsync(string phone, long chatId, long? userId)
    //{

    //    var data = await _context.UserInfo.Where(a => a.PhoneNumber == phone).FirstOrDefaultAsync();

    //    if (data != null)
    //    {
    //        data.TelegramUserId = userId;
    //        data.TelegramNotificationId = chatId;
    //        _context.UserInfo.Update(data);
    //        await _context.SaveChangesAsync();

    //    }


    //}

}

