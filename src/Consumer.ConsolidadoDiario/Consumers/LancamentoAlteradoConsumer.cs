using Data.MongoDb.Context;
using Domain.Contracts.Events;
using Domain.Documents;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Consumer.ConsolidadoDiario.Consumers;

public class LancamentoAlteradoConsumer
    : IConsumer<LancamentoAlteradoEvent>
{
    private readonly IMongoCollection<LancamentoDocument> _lancamentos;
    private readonly ILogger<LancamentoAlteradoConsumer> _logger;

    public LancamentoAlteradoConsumer(
       IMongoCollection<LancamentoDocument> lancamentos,
       ILogger<LancamentoAlteradoConsumer> logger)
    {
        _lancamentos = lancamentos;
        _logger = logger;
    }

    public async Task Consume(
        ConsumeContext<LancamentoAlteradoEvent> context)
    {
        _logger.LogInformation(
           "Processando LancamentoAlteradoEvent. LancamentoId: {LancamentoId}",
           context.Message.LancamentoId);
        Console.WriteLine(
            $"Processando LancamentoAlteradoEvent. " +
            $"LancamentoId: {context.Message.LancamentoId}");
        var evento = context.Message;

        Console.WriteLine(
            $"Lançamento alterado: {evento.LancamentoId} " +
            $"Versão: {evento.Versao}");

        var filtro = Builders<LancamentoDocument>.Filter.And(
            Builders<LancamentoDocument>.Filter.Eq(
                x => x.Id,
                evento.LancamentoId),

            Builders<LancamentoDocument>.Filter.Lt(
                x => x.Versao,
                evento.Versao)
        );

        var atualizacao = Builders<LancamentoDocument>.Update
            .Set(x => x.Descricao, evento.Descricao)
            .Set(x => x.Valor, evento.Valor)
            .Set(x => x.Data, evento.Data)
            .Set(x => x.TipoLancamento, evento.TipoLancamento)
            .Set(x => x.Categoria, evento.Categoria)
            .Set(x => x.Versao, evento.Versao)
            .Set(x => x.Excluido, false);

        var resultado = await _lancamentos.UpdateOneAsync(
            filtro,
            atualizacao);

        if (resultado.MatchedCount == 0)
        {
            Console.WriteLine(
                $"Evento ignorado. Lançamento {evento.LancamentoId} " +
                $"não existe ou já possui versão igual/superior.");

            return;
        }

        Console.WriteLine(
            $"Lançamento {evento.LancamentoId} atualizado no Mongo.");

        _logger.LogInformation(
              "LancamentoAlteradoEvent processado com sucesso. LancamentoId: {LancamentoId}",
              context.Message.LancamentoId);
    }
}