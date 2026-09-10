using Domain.Enum;

namespace Domain.Contracts.Events.Base
{
    public abstract class LancamentoEventBase
    {
        public Guid LancamentoId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public ETipoLancamento TipoLancamento { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public long Versao { get; set; }

    }
}
