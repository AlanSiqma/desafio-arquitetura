using Data.MongoDb.Context;
using Domain.Documents;
using Domain.Enum;
using MongoDB.Driver;

namespace MicroService.ConsolidadoDiario.Services;

public class ConsolidadoDiarioService
{
    private readonly IMongoCollection<LancamentoDocument> _lancamentos;
    private readonly IMongoCollection<ConsolidadoDiarioDocument> _consolidados;

    public ConsolidadoDiarioService(
       IMongoCollection<LancamentoDocument> lancamentos,
       IMongoCollection<ConsolidadoDiarioDocument> consolidados)
    {
        _lancamentos = lancamentos;
        _consolidados = consolidados;
    }

    public async Task<ConsolidadoDiarioDocument> GerarAsync(DateTime data)
    {
        var inicio = data.Date;
        var fim = inicio.AddDays(1);

        var filtroLancamentos = Builders<LancamentoDocument>.Filter.And(
            Builders<LancamentoDocument>.Filter.Gte(x => x.Data, inicio),
            Builders<LancamentoDocument>.Filter.Lt(x => x.Data, fim),
            Builders<LancamentoDocument>.Filter.Eq(x => x.Excluido, false)
        );

        var lancamentos = await _lancamentos
            .Find(filtroLancamentos)
            .ToListAsync();

        var totalReceitas = lancamentos
            .Where(x => x.TipoLancamento == ETipoLancamento.Receita)
            .Sum(x => x.Valor);

        var totalDespesas = lancamentos
            .Where(x => x.TipoLancamento == ETipoLancamento.Despesa)
            .Sum(x => x.Valor);

        var quantidadeReceitas = lancamentos
            .Count(x => x.TipoLancamento == ETipoLancamento.Receita);

        var quantidadeDespesas = lancamentos
            .Count(x => x.TipoLancamento == ETipoLancamento.Despesa);

        var filtroConsolidado =
            Builders<ConsolidadoDiarioDocument>.Filter.Eq(
                x => x.Data,
                inicio);

        var existente = await _consolidados
            .Find(filtroConsolidado)
            .FirstOrDefaultAsync();

        var consolidado = new ConsolidadoDiarioDocument
        {
            Id = existente?.Id ?? Guid.NewGuid(),
            Data = inicio,
            TotalReceitas = totalReceitas,
            TotalDespesas = totalDespesas,
            Saldo = totalReceitas - totalDespesas,
            QuantidadeReceitas = quantidadeReceitas,
            QuantidadeDespesas = quantidadeDespesas,
            AtualizadoEm = DateTime.UtcNow
        };

        await _consolidados.ReplaceOneAsync(
            filtroConsolidado,
            consolidado,
            new ReplaceOptions
            {
                IsUpsert = true
            });

        return consolidado;
    }

    public async Task<ConsolidadoDiarioDocument> ObterAsync(DateTime data)
    {
        var inicio = data.Date;

        var filtro = Builders<ConsolidadoDiarioDocument>
            .Filter
            .Eq(x => x.Data, inicio);

        var consolidado = await _consolidados
            .Find(filtro)
            .FirstOrDefaultAsync();

        return consolidado ?? await GerarAsync(inicio);
    }
}