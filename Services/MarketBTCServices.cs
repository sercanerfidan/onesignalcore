using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Dynamic;
using System.Text;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using Serilog;

namespace OneSignal.Services
{
    public class MarketBTCServices : IMarketServiceBTC
    {
        private readonly IBotService botService;

        private static readonly HttpClient client = new HttpClient();
        private List<string> MessageList;
        private static readonly object locky = new object();

        public MarketBTCServices(IBotService botService)
        {
            this.botService = botService;
            MessageList = new List<string>();
        }

        public async Task GetOrderBook(IEnumerable<string> coinList)
        {

            try {
                Log.Information("BTC Market service working..(GetOrderBook)");
                Console.WriteLine("BTC Market service working..(GetOrderBook)");
                foreach (var item in coinList)
                {

                    StringBuilder url = new StringBuilder();
                    url.Append("https://api.binance.com/api/v1/depth?symbol=");
                    url.Append(item);
                    url.Append("BTC&limit=100");

                    HttpResponseMessage response = await client.GetAsync(url.ToString());
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    var jo = JObject.Parse(responseBody);
                    var bids = jo.Properties().Children().FirstOrDefault().Root["bids"];
                    var asks = jo.Properties().Children().FirstOrDefault().Root["asks"];

                    double price = 0;
                    double amount = 0;
                    List<int> totalList = new List<int>();
                    int count = 0;
                    foreach (var a in asks)
                    {

                        if (a.Next != null)
                        {

                            price = a.Next[0].ToObject<double>();      // 0.00008762
                            amount = a.Next[1].ToObject<double>();      // 439.0
                            double total = (double)(price * amount);                        // 0.03846957

                            int rounded;
                            double value = Math.Ceiling(total);
                            if (!int.TryParse(value.ToString(), out rounded))
                            {  
                                string errormessage = "GetOrderBook error ->price: " + price + " amount : " + amount + " Total: " + total + ", value : " + value + ", rounded : " + rounded;
                                Log.Error(errormessage);
                                return;
                            }

                            //int rounded = Convert.ToInt32(Math.Ceiling(total));
                            string log = "ask-> " + rounded + " BTC, price: " + price + " ,coin:" + item + ", count: " + count;
                            if (rounded >= 30)
                            {
                                Log.Information(log);
                                string messagge = "🤩 " + item + " Sell wall " + rounded + " BTC at " + a.Next[0].ToString();

                                lock (locky) {

                                    if (!MessageList.Contains(messagge))
                                    {
                                        botService.SendTextMessage(messagge, 1232817668);
                                        MessageList.Add(messagge);
                                    }
                                }
                            }
                            count++;
                        }
                    }

                    int count2 = 0;
                    foreach (var b in bids)
                    {
                        if (b.Next != null)
                        {

                            price = b.Next[0].ToObject<double>();  
                            amount = b.Next[1].ToObject<double>();
                            double total =(double)(price * amount);
                            int rounded = Convert.ToInt32(Math.Ceiling(total));
                            string log = "ask-> " + rounded + " BTC, price: " + price + " ,coin:" + item + ", count: " + count2;


                            if (rounded >= 30)
                            {
                                Log.Information(log);
                                string messagge = "🤑 "+item + " Buy wall " + rounded + " BTC at " + b.Next[0].ToString();


                                lock (locky)
                                {

                                    if (!MessageList.Contains(messagge))
                                    {
                                        botService.SendTextMessage(messagge, 1232817668);
                                        MessageList.Add(messagge);
                                    }
                                }
                                 

                            }
                            count2++;
                        }
                    }
                }
            }
            catch(Exception ex) {

                string errorMessage = "Error in market service error: " + ex;
                Log.Error(errorMessage);
            }

       
        }


    }
}
