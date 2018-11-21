using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DurableApp.OrdineCliente.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace DurableApp.OrdineCliente
{
    public static class OrdineConfermato
    {

        [FunctionName(Workflow.OrdineConfermato)]
        public static void Run([ActivityTrigger] OrdiniAcquistoTable ordineAcquisto,
            [Table("OrdineConfermato")] CloudTable approvaOrdineTables,
            ILogger log)
        {
            TableOperation tableOperation = TableOperation.InsertOrReplace(ordineAcquisto);
            approvaOrdineTables.ExecuteAsync(tableOperation);
        }

    }
}