using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneSignal.Services
{
    public class CoinService : ICoinService
    {
        private readonly IBotService botService;
        private static readonly HttpClient client = new HttpClient();

        public CoinService(IBotService botService)
        {
            this.botService = botService;
        }
        public async Task<string> GetCoinMarketInfo(string coinName, int depth)
        {

            Log.Information("Coin service working..(GetCoinMarketInfo)");
            Console.WriteLine("Coin service working..(GetCoinMarketInfo)");

            try
            {
                StringBuilder url = new StringBuilder();
                url.Append("https://api.binance.com/api/v1/depth?symbol=");
                url.Append(coinName);
                url.Append("USDT&limit=");
                url.Append(depth.ToString());

                HttpResponseMessage response = await client.GetAsync(url.ToString());
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject<dynamic>(responseBody);

                var jo = JObject.Parse(responseBody);
                var bids = jo.Properties().Children().FirstOrDefault().Root["bids"];
                var asks = jo.Properties().Children().FirstOrDefault().Root["asks"];

                double price = 0;
                double amount = 0;
                List<string> tradeDecisions = new List<string>();
                int totalAsks = 0;
                foreach (var a in asks)
                {
                    if (a.Next != null)
                    {
                        price = a.Next[0].ToObject<double>();
                        amount = a.Next[1].ToObject<double>();
                        double total = (double)(price * amount);
                        int rounded = Convert.ToInt32(Math.Ceiling(total));
                        totalAsks += rounded;
                    }
                }

                int totalBids = 0;
                foreach (var b in bids)
                {
                    if (b.Next != null)
                    {
                        price = b.Next[0].ToObject<double>();
                        amount = b.Next[1].ToObject<double>();
                        double total = (double)(price * amount);
                        int rounded = Convert.ToInt32(Math.Ceiling(total));
                        totalBids += rounded;
                    }
                }

                int allAmount = totalAsks + totalBids;
                int askRate = 100 * totalAsks / allAmount;
                int bidRate = 100 - askRate;

                Enum desicion = Trade.NEUTRAL;

                if ((askRate > bidRate) && (askRate >= 65))
                {
                    desicion = Trade.STRONGSELL;
                }

                if ((askRate > bidRate) && (askRate >= 55) && (askRate < 65))
                {
                    desicion = Trade.SELL;
                }

                if ((askRate < bidRate) && (bidRate >= 65))
                {
                    desicion = Trade.STRONGBUY;
                }

                if ((askRate < bidRate) && (bidRate >= 55) && (bidRate < 65))
                {
                    desicion = Trade.BUY;
                }


                return desicion.ToString();
            }
            catch (Exception ex) {

                string errorMessage = "Error in coin service error: " + ex;
                Log.Error(errorMessage);
                return "ServiceFailedCheckLogs";
                
            }


        }


        public async Task<string> TradeDesicionSerivice(string coinName, int depth)
        {
            botService.SendTextMessage($"Coin orderbook scanning started with {depth} depth !", 1232817668);
            List<string> depthTradeList = new List<string>();

            Trade tradeDesicion = Trade.NEUTRAL;
            await Task.Run(async () =>
            {
                while (true)
                {
                    string desicion = await GetCoinMarketInfo(coinName, depth);
                    if (depthTradeList.Count == 5) {

                        int buyCount = depthTradeList.Where(p => p == Trade.BUY.ToString()).Count();
                        int sBuyCount = depthTradeList.Where(p => p == Trade.STRONGBUY.ToString()).Count();
                        int sellCount = depthTradeList.Where(p => p == Trade.SELL.ToString()).Count();
                        int sSellCount = depthTradeList.Where(p => p == Trade.STRONGSELL.ToString()).Count();
                        int noneCount = depthTradeList.Where(p => p == Trade.NEUTRAL.ToString()).Count();

                        if (buyCount >= 3) {
                            tradeDesicion = Trade.BUY;
                        }

                        if (sBuyCount >= 3)
                        {
                            tradeDesicion = Trade.STRONGBUY;
                        }

                        if (buyCount >= 3 && sBuyCount >= 1 )
                        {
                            tradeDesicion = Trade.STRONGBUY;
                        }


                        if (sellCount >= 3)
                        {
                            tradeDesicion = Trade.SELL;
                        }

                        if (sSellCount >= 3 && sSellCount >= 1)
                        {
                            tradeDesicion = Trade.STRONGSELL;
                        }

                        if (noneCount >= 3)
                        {
                            tradeDesicion = Trade.NEUTRAL;
                        }

                        if ((sellCount == 2 && buyCount == 2) ||  (sSellCount == 2 && sBuyCount == 2)) {
                            tradeDesicion = Trade.NEUTRAL;
                        }


                        if (noneCount == 2 && buyCount == 2 && sBuyCount == 1)
                        {
                            tradeDesicion = Trade.BUY;
                        }

                        if (noneCount == 2 && sellCount == 2 && sSellCount == 1)
                        {
                            tradeDesicion = Trade.SELL;
                        }


                        if (noneCount == 2 && buyCount == 1 && sBuyCount == 2)
                        {
                            tradeDesicion = Trade.BUY;
                        }

                        if (noneCount == 2 && sellCount == 1 && sSellCount == 2)
                        {
                            tradeDesicion = Trade.SELL;
                        }

                        if (noneCount == 2 && buyCount == 1 && sSellCount == 2)
                        {
                            tradeDesicion = Trade.NEUTRAL;
                        }

                        if (noneCount == 2 && sellCount == 1 && sBuyCount == 2)
                        {
                            tradeDesicion = Trade.NEUTRAL;
                        }


                        if (noneCount == 1 && sellCount == 2 && sSellCount == 2)
                        {
                            tradeDesicion = Trade.SELL;
                        }

                        if (noneCount == 1 && buyCount == 2 && sBuyCount == 2)
                        {
                            tradeDesicion = Trade.BUY;
                        }

                        if (noneCount == 1 && sellCount == 2 && sBuyCount == 2)
                        {
                            tradeDesicion = Trade.BUY;
                        }

                        if (noneCount == 1 && buyCount == 2 && sSellCount == 2)
                        {
                            tradeDesicion = Trade.SELL;
                        }

                        break;
                    }
                    depthTradeList.Add(desicion);
                    Thread.Sleep(7000);
                }
            });

            return tradeDesicion.ToString();


            //Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        string desicion = await GetCoinMarketInfo(coinName, 500);
            //        if (depth500TradeList.Count == 5)
            //        {
            //            break;
            //        }
            //        depth100TradeList.Add(desicion);
            //        Thread.Sleep(10000);
            //    }
            //});
        }

      
    }
}
