using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;

public class MercadoPagoService
{
    public async Task<Preference> CreatePaymentPreference(decimal amount, string title)
    {
        // Inicializa tu clave de acceso
        MercadoPagoConfig.AccessToken = "APP_USR-4896539832781906-091321-1c336f0ee0727e81185492f77d9d6207-2691597676";

        var request = new PreferenceRequest
        {
            Items = new List<PreferenceItemRequest>
            {
                new PreferenceItemRequest
                {
                    Title = title,
                    Quantity = 1,
                    CurrencyId = "ARS", // o "USD"
                    UnitPrice = amount
                }
            }
        };

        var client = new PreferenceClient();
        Preference preference = await client.CreateAsync(request);

        return preference;
    }
}
