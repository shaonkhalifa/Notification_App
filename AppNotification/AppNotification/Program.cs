using AppNotification.Services;
using Telegram.Bot;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {

        services.Configure<BotConfiguration>(
            context.Configuration.GetSection(BotConfiguration.Configuration));

        services.AddHttpClient("telegram_bot_client")
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    BotConfiguration? botConfig = sp.GetConfiguration<BotConfiguration>();
                    TelegramBotClientOptions options = new(botConfig.BotToken);
                    return new TelegramBotClient(options, httpClient);
                });

        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();
    })
    .Build();

await host.RunAsync();

#pragma warning disable CA1050 // Declare types in namespaces
#pragma warning disable RCS1110 // Declare type inside namespace.
public class BotConfiguration
#pragma warning restore RCS1110 // Declare type inside namespace.
#pragma warning restore CA1050 // Declare types in namespaces
{
    public static readonly string Configuration = "BotConfiguration";

    public string BotToken { get; set; } = "";
}


//using AppNotification.Enitiy;
//using Microsoft.Extensions.Options;
//using Telegram.Bot;

//var builder = WebApplication.CreateBuilder(args);

////builder.Host.ConfigureServices(services =>
////{
////    services.AddHttpClient("telegram_bot_client");
////    services.AddSingleton<ITelegramBotClient>(sp =>
////    {
////        var botConfig = sp.GetService<IOptions<BotConfiguration>>().Value;
////        var options = new TelegramBotClientOptions(botConfig.Token);
////        return new TelegramBotClient(options, sp.GetRequiredService<IHttpClientFactory>().CreateClient("telegram_bot_client"));
////    });
////});

//builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
//builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection("TelegramBot"));

//builder.Services.AddHttpClient("telegram_bot_client")
//   .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
//   {
//       BotConfiguration botConfiguration = sp.GetService<IOptions<BotConfiguration>>().Value;
//       var options = new TelegramBotClientOptions(botConfiguration.Token);
//       return new TelegramBotClient(options, sp.GetRequiredService<IHttpClientFactory>().CreateClient("telegram_bot_client"));

//   });
////builder.Services.AddSingleton<ITelegramBotClient>(sp =>
////{
////    var botConfig = sp.GetService<IOptions<BotConfiguration>>().Value;
////    var options = new TelegramBotClientOptions(botConfig.Token);
////    return new TelegramBotClient(options, sp.GetRequiredService<IHttpClientFactory>());
////});


//builder.Services.AddControllers();



//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();


//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

////#pragma warning disable CA1050 // Declare types in namespaces
////#pragma warning disable RCS1110 // Declare type inside namespace.
////public class BotConfiguration
////#pragma warning restore RCS1110 // Declare type inside namespace.
////#pragma warning restore CA1050 // Declare types in namespaces
////{
////    public static readonly string Configuration = "BotConfiguration";

////    public string BotToken { get; set; } = "";
////}

