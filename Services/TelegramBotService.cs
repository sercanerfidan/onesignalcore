using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace OneSignal.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly string  botToken;

        public TelegramBotService(string botToken)
        {
            this.botToken = botToken;
        }
        public TelegramBotClient CreateTelegramServiceInstance()
        {
            TelegramBotClient bot = new TelegramBotClient(botToken);
            //bot.StartReceiving();
            //bot.StopReceiving();
            return bot;
        }
    }
}
