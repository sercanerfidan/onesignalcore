using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneSignal.Services;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OneSignal
{
    public class ScanService : IScanService
    {
        List<Task> tasks;
        private static readonly HttpClient client = new HttpClient();
        private readonly IBotService botService;
        private ConcurrentDictionary<string, DateTime> MessageList;
        private static readonly object locky= new object();
        public ScanService(IBotService botService)
        {
            this.botService = botService;
            MessageList = new ConcurrentDictionary<string, DateTime>();
        }

        public async Task StartUp()
        {
            tasks = new List<Task>();

            List<string> coinList = Enum.GetNames(typeof(Coins)).ToList();

            var takeCount = coinList.Count / 5;

            List<IEnumerable<string>> listOfPartition = new List<IEnumerable<string>>();
            for (int i = 0; i < coinList.Count(); i += takeCount)
            {
                listOfPartition.Add(coinList.Skip(i).Take(takeCount));
            }

            foreach (var item in listOfPartition)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    await GetOrderBook(item);
                }));

            }

            await Task.WhenAll(tasks);


            //Parallel.ForEach(listOfPartition, new ParallelOptions() { MaxDegreeOfParallelism = 3 }, index =>
            //{
            //    GetOrderBook(index);
            //});


        }


        public async Task GetOrderBook(IEnumerable<string> coinList)
        {

            try
            {

                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromMinutes(1);

                //var timer = new System.Threading.Timer((e) =>
                //{
                //    botService.SendTextMessage("Test message", 1232817668);

                //}, null, startTimeSpan, periodTimeSpan);

                Console.WriteLine("Scan service working..(GetOrderBook)");
                Log.Information("Scan service working..(GetOrderBook)");

                foreach (var item in coinList)
                {

                    StringBuilder url = new StringBuilder();
                    url.Append("https://api.binance.com/api/v1/depth?symbol=");
                    url.Append(item);
                    url.Append("USDT&limit=100");

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


                    if ((bidRate > askRate) && (bidRate >= 60))
                    {
                        string message = string.Empty;
                        Guid id = Guid.NewGuid();
                        StringBuilder sb = new StringBuilder();
                        sb.Append("BUY signal in");
                        sb.Append(" " + item);
                        sb.Append(" - " + id.ToString());
                        message = sb.ToString();


                        if (!string.IsNullOrEmpty(message))
                        {
                            lock (locky) {
                                lock (MessageList)
                                {
                                    var queryb = MessageList.Where(x => x.Key.IndexOf(" " + item + " ") > -1).ToList();
                                    if (queryb.Count == 0)
                                    {
                                        
                                        if (MessageList.TryAdd(message, DateTime.Now)) {
                                            botService.SendTextMessage(message, 1232817668);
                                        }
                                    }
                                    else
                                    {

                                        foreach (KeyValuePair<string, DateTime> dict in MessageList.ToList())
                                        {
                                            var queryb2 = MessageList.Where(x => x.Key.IndexOf(" " + item + " ") > -1).ToList();
                                            if (queryb2.Count > 0)
                                            {
                                                TimeSpan ts = DateTime.Now - dict.Value;
                                                if (ts.TotalMinutes > 30)
                                                {
                                                    if (MessageList.TryAdd(message, DateTime.Now)) {

                                                        botService.SendTextMessage(message, 1232817668);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                               

                            }


                        }
                    }


                    if ((askRate > bidRate) && (askRate >= 60))
                    {
                        string message = string.Empty;
                        Guid id = Guid.NewGuid();
                        StringBuilder sb = new StringBuilder();
                        sb.Append("SELL signal in");
                        sb.Append(" " + item);
                        sb.Append(" - " + id.ToString());
                        message = sb.ToString();

                        if (!string.IsNullOrEmpty(message))
                        {
                            lock (locky)
                            {
                                lock (MessageList)
                                {
                                    var query = MessageList.Where(x => x.Key.IndexOf(" " + item + " ") > -1).ToList();
                                    if (query.Count == 0)
                                    {
                                       
                                        if (MessageList.TryAdd(message, DateTime.Now)) {
                                            botService.SendTextMessage(message, 1232817668);
                                        }
                                    }
                                    else
                                    {

                                        foreach (KeyValuePair<string, DateTime> dict in MessageList.ToList())
                                        {
                                            var query2 = MessageList.Where(x => x.Key.IndexOf(" " + item + " ") > -1).ToList();
                                            if (query2.Count > 0)
                                            {
                                                TimeSpan ts = DateTime.Now - dict.Value;
                                                if (ts.TotalMinutes > 30)
                                                {

                                                    //MessageList.TryRemove(message, out _);
                                                    if (MessageList.TryAdd(message, DateTime.Now)) {

                                                        botService.SendTextMessage(message, 1232817668);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                               
                            }


                        }
                    }
                }
            }
                  catch (Exception ex)
            {


                string errorMessage = "Error in scan service error: " + ex;
                Log.Error(errorMessage);
            }

        }

    }
}
