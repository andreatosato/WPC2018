using DurableApp.OrdineCliente;
using DurableApp.OrdineCliente.Model;
using Microsoft.Azure.WebJobs;
using SendGrid.Helpers.Mail;
using Serilog;
using System;
using System.Linq;

namespace DurableApp.InviaMail
{
    public static class InviaMailFunction
    {
        [FunctionName(Workflow.InviaMailOrdineCliente)]
        public static string Run(
           [ActivityTrigger] OrdiniAcquistoModel ordiniAcquisto,
           [OrchestrationClient] DurableOrchestrationClient starter,
           [SendGrid(ApiKey = "SendGridApiKey")]
           out SendGridMessage message)
        {
            string currentInstance = Guid.NewGuid().ToString("N");
            Log.Information($"InviaMailOrdineCliente : {currentInstance}");

            string toMail = Utility.GetEnvironmentVariable("SendGridTo");
            string fromMail = Utility.GetEnvironmentVariable("SendGridFrom");
            Log.Information($"Invio ordine {ordiniAcquisto.IdOrdine} a {ordiniAcquisto.ClienteCorrente.NumeroTelefono}.");
            message = new SendGridMessage
            {
                Subject = $"WPC 2018"                
            };
            message.AddTo(toMail);
            Content content = new Content
            {
                Type = "text/html",
                Value = $@"L'ordine {ordiniAcquisto.IdOrdine} è stato preso in carico
                <br><a href='{Utility.GetEnvironmentVariable("PublicUrl")}/ApprovaOrdine?ordineId={ordiniAcquisto.IdOrdine}'>Conferma ordine</a>"
            };

            message.From = new EmailAddress(fromMail);
            message.AddContents(new [] { content }.ToList());
            return currentInstance;
        }
    }
}
