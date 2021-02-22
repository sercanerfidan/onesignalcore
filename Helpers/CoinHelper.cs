using OneSignal;
using OneSignal.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OneSignalCore.Helpers
{
    public class CoinHelper : ICoinHelper
    {
        private readonly IScanService scanService;
        private readonly IBotService botService;
        public CoinHelper(IScanService scanService, IBotService botService)
        {
            this.scanService = scanService;
            this.botService = botService;
        }
        public async Task StartCoinService()
        {
            botService.SendTextMessage("Single coin signal scanning started!", 1232817668);
            Console.WriteLine("Single coin signal scanning started!");

            while (true)
            {
                await scanService.StartUp();
                await Task.Delay(20000);

            }
        }
    }
}
