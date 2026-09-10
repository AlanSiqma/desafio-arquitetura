using Data.MongoDb.Context;
using Domain.Contracts.Events;
using Domain.Documents;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Diagnostics;
using Consumer.ConsolidadoDiario.Observability;

namespace Consumer.ConsolidadoDiario.Consumers;

public class LancamentoCriadoConsumer
    : IConsumer<LancamentoCriadoEvent>
{
    private readonly IMongoCollection<LancamentoDocument> _lancamentos;
    private readonly ILogger<LancamentoCriadoConsumer> _logger;

    public LancamentoCriadoConsumer(
     IMongoCollection<LancamentoDocument> lancamentos,
     ILogger<LancamentoCriadoConsumer> logger)
    {
        _lancamentos = lancamentos;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LancamentoCriadoEvent> context)
    {
        using var activity =
         ConsumerActivitySource.Source.StartActivity(
             "LancamentoCriadoEvent.Process");

        _logger.LogInformation(
            "Processando LancamentoCriadoEvent. LancamentoId: {LancamentoId}",
            context.Message.LancamentoId);

        var evento = context.Message;
       
        Console.WriteLine(
            $"Lançamento recebido: {evento.LancamentoId} " +
            $"Versão: {evento.Versao}");

        var documento = new LancamentoDocument
        {
            Id = evento.LancamentoId,
            Descricao = evento.Descricao,
            Valor = evento.Valor,
            Data = evento.Data,
            TipoLancamento = evento.TipoLancamento,
            Categoria = evento.Categoria,
            Excluido = false,
            Versao = evento.Versao
        };

        var filtro = Builders<LancamentoDocument>.Filter.Eq(
            x => x.Id,
            evento.LancamentoId);

        var existente = await _lancamentos
            .Find(filtro)
            .FirstOrDefaultAsync();

        if (existente is not null)
        {
            if (existente.Versao >= evento.Versao)
            {
                Console.WriteLine(
                    $"Evento ignorado. Lançamento {evento.LancamentoId} " +
                    $"já possui versão {existente.Versao}.");

                return;
            }

            await _lancamentos.ReplaceOneAsync(
                filtro,
                documento);

            Console.WriteLine(
                $"Lançamento {evento.LancamentoId} atualizado no Mongo.");

            return;
        }

        await _lancamentos.InsertOneAsync(documento);
            
        Console.WriteLine(
            $"Lançamento {evento.LancamentoId} criado no Mongo.");

        _logger.LogInformation(
           "LancamentoCriadoEvent processado com sucesso. LancamentoId: {LancamentoId}",
           context.Message.LancamentoId);
    }
}