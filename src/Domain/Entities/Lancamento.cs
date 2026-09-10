using Domain.Enum;

namespace Domain.Entities
{
    public class Lancamento
    {
        public Lancamento(ETipoLancamento tipoLancamento)
        {
            this.Id = Guid.NewGuid();
            this.TipoLancamento = tipoLancamento;
            Data = DateTime.UtcNow;


        }
        public Lancamento()
        {
            this.Id = Guid.NewGuid();
            Data = DateTime.UtcNow;

        }
        public Guid Id { get; set; }
        public long Versao { get; set; } = 1;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public ETipoLancamento TipoLancamento { get; set; }
        public string Categoria { get; set; } = string.Empty;
    }
  
}