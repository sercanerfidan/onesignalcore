using Telegram.Bot;

namespace OneSignal.Services
{
    public interface ITelegramBotService
    {
        TelegramBotClient CreateTelegramServiceInstance();
    }
}