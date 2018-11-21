using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DurableApp.OrdineCliente.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Serilog;

namespace DurableApp.OrdineCliente
{
    public static class OrdineClienteManager
    {
        [FunctionName("OrdineClienteManager")]
        [return: Table("OrdiniCliente")]
        public static async Task<OrdiniAcquistoTable> Run(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            OrdiniAcquistoModel ordineAcquisto = context.GetInput<OrdiniAcquistoModel>();
            // Se è un nuovo tentativo, imposto l'IdOrdine
            ordineAcquisto.IdOrdine = context.InstanceId;
            
            // Utilizzo l'orario di start della funzione
            DateTime startManagerDatetime = context.CurrentUtcDateTime;
            
            // TODO: Salva l'ordine in un DB.
            string mailInstance;
            string smsInstance = "";

            // Invia notifica ordine via SMS
            await context.CallActivityAsync<string>(
                Workflow.NotificaSmsOrdineCliente,
                ordineAcquisto);

            // Invia notifica ordine via Mail
            mailInstance = await context.CallActivityWithRetryAsync<string>(Workflow.InviaMailOrdineCliente, new RetryOptions(TimeSpan.FromSeconds(5), 10), ordineAcquisto);
            Log.Information($"OrdineClienteManager: MailInstance {mailInstance}");
                
            //TODO: abilitare Human Interaction
            if (!string.IsNullOrEmpty(mailInstance))
            {
                await context.CallSubOrchestratorAsync(Workflow.AttendiOrdineCliente, ordineAcquisto.IdOrdine, ordineAcquisto.IdOrdine);
            }

            return new OrdiniAcquistoTable
            {
                PartitionKey = ordineAcquisto.IdOrdine,
                RowKey = $"{smsInstance}-{mailInstance}",
                Ordine = ordineAcquisto,
                NotificaSmsOrdineCliente = smsInstance,
                InviaMailOrdineCliente = mailInstance,
                Elaborazione = DateTimeOffset.UtcNow
            };
        }
    }
}
