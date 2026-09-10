namespace Domain.Contracts.Events
{
    public class LancamentoExcluidoEvent
    {
        public Guid LancamentoId { get; set; }
        public long Versao { get; set; }

    }
}
