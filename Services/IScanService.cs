using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneSignal
{
    public interface IScanService
    {
        Task StartUp();
        Task  GetOrderBook(IEnumerable<string> coinList);
    }
}