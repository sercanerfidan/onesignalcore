using OneSignal;
using OneSignal.Services;
using OneSignalCore.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneSignalCore
{
    public class App : IApp
    {
        private readonly IMultiTaskServiceBTC multiTaskService;
        private readonly IBotService botService;
        private readonly IScanService scanService;
        private readonly OneSignalCore.Helpers.IMarketBTCHelper marketHelperBTC;
        private readonly OneSignalCore.Helpers.IMarketUSDTHelper marketHelperUSDT;
        private readonly OneSignalCore.Helpers.ICoinHelper coinHelper;


        public App(IMultiTaskServiceBTC multiTaskService, IBotService botService, IScanService scanService, OneSignalCore.Helpers.IMarketBTCHelper marketHelperBTC, OneSignalCore.Helpers.ICoinHelper coinHelper, OneSignalCore.Helpers.IMarketUSDTHelper marketHelperUSDT)
        {
            this.multiTaskService = multiTaskService;
            this.botService = botService;
            this.scanService = scanService;
            this.marketHelperBTC = marketHelperBTC;
            this.marketHelperUSDT = marketHelperUSDT;
            this.coinHelper = coinHelper;
        }

        public void Run() {

            try
            {
                Log.Information("App run started");

                botService.SendTextMessage("Bot listening service Started!", 1232817668);
                Console.WriteLine("Bot Application Started!");
                Log.Information("Bot Application Started!");

                marketHelperUSDT.StartMarketService();
                marketHelperBTC.StartMarketService();
                //coinHelper.StartCoinService();

                botService.SendTextMessage("All services starting!", 1232817668);
                Console.WriteLine("All services starting!");

                Log.Information("App run end line");

            }
            catch (Exception ex)
            {

                Log.Error("Start Controller error: ", ex);
            }


            Log.Information("All services started!");
        }

    }
}
