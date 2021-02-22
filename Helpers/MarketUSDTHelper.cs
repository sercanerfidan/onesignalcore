using OneSignal;
using OneSignal.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OneSignalCore.Helpers
{
    public class MarketUSDTHelper : IMarketUSDTHelper
    {
        private readonly IMultiTaskServiceUSDT multiTaskService;
        private readonly IBotService botService;
        public MarketUSDTHelper(IMultiTaskServiceUSDT multiTaskService, IBotService botService)
        {
            this.multiTaskService = multiTaskService;
            this.botService = botService;
        }

        public async Task StartMarketService()
        {
            botService.SendTextMessage("USDT Wall monitor scanning started!", 1232817668);
            Console.WriteLine("USDT Wall monitor scanning started!");

            while (true)
            {
                await multiTaskService.StartUp();
                await Task.Delay(7000);
            }
        }
    }
}
