using OneSignal;
using OneSignal.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OneSignalCore.Helpers
{
    public class MarketBTCHelper : IMarketBTCHelper
    {
        private readonly IMultiTaskServiceBTC multiTaskService;
        private readonly IBotService botService;
        public  MarketBTCHelper(IMultiTaskServiceBTC multiTaskService, IBotService botService)
        {
            this.multiTaskService = multiTaskService;
            this.botService = botService;
        }

        public async Task StartMarketService()
        {
            botService.SendTextMessage("BTC Wall monitor scanning started!", 1232817668);
            Console.WriteLine("BTC Wall monitor scanning started!");

            while (true)
            {
                await multiTaskService.StartUp();
                await Task.Delay(7000);
            }
        }
    }
}
