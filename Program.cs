using Microsoft.Extensions.DependencyInjection;
using OneSignal;
using OneSignal.Services;
using OneSignalCore.Helpers;
using Serilog;
using System;

namespace OneSignalCore
{
    class Program
    {
        public static readonly string BotToken = "1210081882:AAEQNMiMjkup0ZL_SNHNbrV11GM6D2Y3EVk";
        public static IServiceProvider _serviceProvider;
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
     
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            Console.ReadKey();
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {

            Log.Logger = new LoggerConfiguration()
.MinimumLevel.Debug()
.WriteTo.File("log.txt")
.CreateLogger();

            Log.Information("ConfigureServices start");

            Console.WriteLine("ConfigureServices start");

            try
            {
                services.AddSingleton<IScanService>(provider => new ScanService(provider.GetRequiredService<IBotService>()));
                services.AddSingleton<ICoinService>(provider => new CoinService(provider.GetRequiredService<IBotService>()));
                services.AddSingleton<ITelegramBotService>(provider => new TelegramBotService(BotToken));
                services.AddSingleton<IBotService>(provider => new BotService(provider.GetRequiredService<ITelegramBotService>()));
                services.AddSingleton<IMarketServiceUSDT>(provider => new MarketUSDTServices(provider.GetRequiredService<IBotService>()));
                services.AddSingleton<IMultiTaskServiceUSDT>(provider => new MultiTaskServiceUSDT(provider.GetRequiredService<IMarketServiceUSDT>()));
                services.AddSingleton<IMarketServiceBTC>(provider => new MarketBTCServices(provider.GetRequiredService<IBotService>()));
                services.AddSingleton<IMultiTaskServiceBTC>(provider => new MultiTaskServiceBTC(provider.GetRequiredService<IMarketServiceBTC>()));
                services.AddSingleton<OneSignalCore.Helpers.IMarketUSDTHelper>(provider => new OneSignalCore.Helpers.MarketUSDTHelper(provider.GetRequiredService<IMultiTaskServiceUSDT>(), provider.GetRequiredService<IBotService>()));
                services.AddSingleton<OneSignalCore.Helpers.IMarketBTCHelper>(provider => new OneSignalCore.Helpers.MarketBTCHelper(provider.GetRequiredService<IMultiTaskServiceBTC>(), provider.GetRequiredService<IBotService>()));
                services.AddSingleton<OneSignalCore.Helpers.ICoinHelper>(provider => new OneSignalCore.Helpers.CoinHelper(provider.GetRequiredService<IScanService>(), provider.GetRequiredService<IBotService>()));
                services.AddSingleton<IApp>(provider => new App(provider.GetRequiredService<IMultiTaskServiceBTC>(), provider.GetRequiredService<IBotService>(),
                  provider.GetRequiredService<IScanService>(), provider.GetRequiredService<IMarketBTCHelper>(), provider.GetRequiredService<ICoinHelper>(), provider.GetRequiredService<IMarketUSDTHelper>()));
                _serviceProvider = services.BuildServiceProvider();


                var bot = _serviceProvider.GetRequiredService<ITelegramBotService>().CreateTelegramServiceInstance();
                var botService = _serviceProvider.GetRequiredService<IBotService>();
                bot.OnMessage += botService.BotOnMessageReceived;
                bot.OnMessageEdited += botService.BotOnMessageReceived;
                bot.OnReceiveError += botService.BotOnReceiveError;
                bot.StartReceiving();

                _serviceProvider.GetRequiredService<IApp>().Run();

                Log.Information(" ConfigureService End");

                Console.WriteLine(" ConfigureService End");

            }
            catch (Exception ex)
            {

                Log.Error("ConfigureService Error : ", ex);
            }

              return services;
        }
    }
}
