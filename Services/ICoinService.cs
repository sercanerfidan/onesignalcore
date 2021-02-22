using System.Threading.Tasks;

namespace OneSignal.Services
{
    public interface ICoinService
    {
        Task<string> GetCoinMarketInfo(string coinName, int depth);

        Task<string> TradeDesicionSerivice(string coinName, int depth);
    }
}