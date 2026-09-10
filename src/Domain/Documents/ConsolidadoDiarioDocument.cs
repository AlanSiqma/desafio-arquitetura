using Domain.Enum;

namespace Domain.Documents
{
    public class ConsolidadoDiarioDocument
    {
        public Guid Id { get; set; }

        public DateTime Data { get; set; }

        public decimal TotalReceitas { get; set; }

        public decimal TotalDespesas { get; set; }

        public decimal Saldo { get; set; }

        public int QuantidadeReceitas { get; set; }

        public int QuantidadeDespesas { get; set; }

        public DateTime AtualizadoEm { get; set; }
    }
}