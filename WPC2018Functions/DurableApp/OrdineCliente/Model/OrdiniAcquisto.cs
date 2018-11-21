using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableApp.OrdineCliente.Model
{
    public class OrdiniAcquistoModel
    {
        public string IdConfirmation { get; set; }
        public string IdOrdine { get; set; }
        public Cliente ClienteCorrente { get; set; }
        public IEnumerable<Articolo> Articoli { get; set; }
        public decimal TotaleAcquisti => Articoli.Sum(x => x.PrezzoAcquisto);

        public class Articolo
        {
            public int IdArticolo { get; set; }
            public decimal PrezzoAcquisto { get; set; }
        }

        public class Cliente
        {
            public int IdCliente { get; set; }
            public string NumeroTelefono { get; set; }
        }
    }

    public class OrdiniAcquistoTable : TableEntity, ITableEntity
    {
        public OrdiniAcquistoModel Ordine { get; set; }
        public string InviaMailOrdineCliente { get; set; }
        public string NotificaSmsOrdineCliente { get; set; }
        public DateTimeOffset Elaborazione { get; set; }
    }
}
