﻿using OneSignal.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneSignal
{
    public class MultiTaskServiceBTC : IMultiTaskServiceBTC
    {
        List<Task> tasks;
        private readonly IMarketServiceBTC marketService;

        public MultiTaskServiceBTC(IMarketServiceBTC marketService)
        {
            this.marketService = marketService;
        }

        public async Task StartUp() {

            try {
                tasks = new List<Task>();

                List<string> coinList = Enum.GetNames(typeof(Coins)).Where(x => x.Contains("BNB") || x.Contains("ADA") || x.Contains("XMR")).ToList();

                var takeCount = coinList.Count / 3;

                List<IEnumerable<string>> listOfPartition = new List<IEnumerable<string>>();
                for (int i = 0; i < coinList.Count(); i += takeCount)
                {
                    listOfPartition.Add(coinList.Skip(i).Take(takeCount));
                }


                //Parallel.ForEach(listOfPartition, new ParallelOptions() { MaxDegreeOfParallelism = 3 }, index =>
                //{
                //    marketService.GetOrderBook(index);
                //});

                foreach (var item in listOfPartition)
                {
                    await Task.Delay(3000);
                    tasks.Add(Task.Run(async () =>
                    {
                        await marketService.GetOrderBook(item);

                    }));

                }
                await Task.WhenAll(tasks);

            } catch (Exception ex) {
                Log.Error("Error on MultiTaskService error: ", ex);
            }

          


        }

    }
}
