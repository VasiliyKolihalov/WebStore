using WebStoreAPI.Models;

namespace WebStoreAPI.Services
{
    public interface ICurrencyService
    {
        public decimal ConvertCurrency(decimal value, AvailableCurrencies to);
    }
}