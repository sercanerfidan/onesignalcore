using System.Threading.Tasks;
using Telegram.Bot.Args;

namespace OneSignal.Services
{
    public interface IBotService
    {
        void SendTextMessage(string message, long chatId);

        void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs);

        void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs);


    }
}