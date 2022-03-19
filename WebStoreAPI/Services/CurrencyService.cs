using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using WebStoreAPI.Models;
using Microsoft.Extensions.Caching.Memory;

namespace WebStoreAPI.Services
{
    public class CurrencyService : ICurrencyService
    {
        private CbrDataResponse _cbrData;
        private const string CbrDateKey = "cbrDate";
        private readonly IMemoryCache _cache;
        private const int ResponseStorageHours = 1;


        public CurrencyService(IMemoryCache cache)
        {
            _cache = cache;
        }

        private void SetCbrData()
        {
            if (_cache.TryGetValue(CbrDateKey, out _cbrData))
                return;

            var httpClient = new HttpClient();
            //get exchange rates against the ruble
            string jsonResponse = httpClient
                .GetAsync("https://www.cbr-xml-daily.ru/daily_json.js").Result.Content.ReadAsStringAsync().Result;

            _cbrData = JsonConvert.DeserializeObject<CbrDataResponse>(jsonResponse);
            _cache.Set(CbrDateKey, _cbrData,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(ResponseStorageHours)));
        }

        public decimal ConvertCurrency(decimal value, AvailableCurrencies to)
        {
            SetCbrData();
            decimal result;

            switch (to)
            {
                case AvailableCurrencies.Eur:
                    result = value / _cbrData.Valute["EUR"].Value;
                    break;

                case AvailableCurrencies.Usd:
                    result = value / _cbrData.Valute["USD"].Value;
                    break;

                default:
                    result = value;
                    break;
            }

            return result;
        }
    }
}