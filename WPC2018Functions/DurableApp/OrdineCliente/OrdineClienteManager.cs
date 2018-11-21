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
        public static async Task Run(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            OrdiniAcquistoModel ordineAcquisto = context.GetInput<OrdiniAcquistoModel>();
            // Utilizzo un Id di esempio. 
            ordineAcquisto.IdOrdine = context.InstanceId;

            // Invia notifica ordine via SMS
            string smsInstance = await context.CallActivityAsync<string>(
                Workflow.NotificaSmsOrdineCliente,
                ordineAcquisto);
            
            // Il SubOrchestrator non è necessario, è un esempio
            await context.CallSubOrchestratorAsync(Workflow.AttendiOrdineCliente, ordineAcquisto);

            // Utilizzo l'orario di start della funzione
            DateTime startManagerDatetime = context.CurrentUtcDateTime;
            await context.CallActivityAsync(Workflow.OrdineConfermato, new OrdiniAcquistoTable
            {
                PartitionKey = ordineAcquisto.IdOrdine,
                RowKey = $"{smsInstance}-{context.InstanceId}",
                Ordine = ordineAcquisto,
                NotificaSmsOrdineCliente = smsInstance,
                InviaMailOrdineCliente = context.InstanceId,
                Elaborazione = startManagerDatetime
            });
        }
    }
}
