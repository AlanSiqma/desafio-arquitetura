using Domain.Enum;

namespace Domain.Documents
{
    public class LancamentoDocument
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public ETipoLancamento TipoLancamento { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public long Versao { get; set; }
        public bool Excluido { get; set; } = false;
    }
}
