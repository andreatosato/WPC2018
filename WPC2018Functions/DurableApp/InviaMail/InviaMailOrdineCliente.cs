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
           [SendGrid(ApiKey = "SendGridApiKey")] ICollector<SendGridMessage> messageCollector)
        {
            string toMail = Utility.GetEnvironmentVariable("SendGridTo");
            string fromMail = Utility.GetEnvironmentVariable("SendGridFrom");
            var message = new SendGridMessage { Subject = $"WPC 2018" };
            message.AddTo(toMail);
            Content content = new Content
            {
                Type = "text/html",
                Value = $@"L'ordine {ordiniAcquisto.IdOrdine} è stato preso in carico
                <br><a href='{Utility.GetEnvironmentVariable("PublicUrl")}/ApprovaOrdine?ordineId={ordiniAcquisto.IdConfirmation}'>Conferma ordine</a>"
            };

            message.From = new EmailAddress(fromMail);
            message.AddContents(new [] { content }.ToList());
            messageCollector.Add(message);
            
            return Guid.NewGuid().ToString("N");
        }
    }
}
