using DurableApp.OrdineCliente.Model;
using Microsoft.Azure.WebJobs;
using Serilog;
using System;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace DurableApp.NotificaSms
{
    public static class NotificaSmsOrdineCliente
    {
        [FunctionName(Workflow.NotificaSmsOrdineCliente)]
        public static async Task<string> Run(
           [ActivityTrigger] OrdiniAcquistoModel ordiniAcquisto,
           [TwilioSms(AccountSidSetting = "TwilioAccountSid",
                      AuthTokenSetting = "TwilioAuthToken")]
            IAsyncCollector<CreateMessageOptions> message,
            Microsoft.Extensions.Logging.ILogger logger)
        {
            string currentInstance = Guid.NewGuid().ToString("N");
            await message.AddAsync(new CreateMessageOptions(new PhoneNumber(ordiniAcquisto.ClienteCorrente.NumeroTelefono))
            {
                Body = $"L'ordine {ordiniAcquisto.IdOrdine} è preso in carico. Conferma la mail",
                From = new PhoneNumber("+447533003163")
            });
            await message.FlushAsync();

            return currentInstance;
        }
    }
}
