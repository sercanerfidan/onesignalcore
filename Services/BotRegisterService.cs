using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace OneSignal.Services
{
    public class BotRegisterService
    {
        private readonly ITelegramBotService telegramBotService;
        private readonly IBotService botService;
        private readonly ITelegramBotClient bot;
        public BotRegisterService(IBotService botService, ITelegramBotService telegramBotService)
        {
            this.botService = botService;
            this.telegramBotService = telegramBotService;
            this.bot = telegramBotService.CreateTelegramServiceInstance();
        }

        public void StartUp() {


        }
    }
}
