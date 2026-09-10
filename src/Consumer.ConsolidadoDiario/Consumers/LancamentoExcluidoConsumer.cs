using Data.MongoDb.Context;
using Domain.Contracts.Events;
using Domain.Documents;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Consumer.ConsolidadoDiario.Consumers
{
    public class LancamentoExcluidoConsumer
        : IConsumer<LancamentoExcluidoEvent>
    {
        private readonly IMongoCollection<LancamentoDocument> _lancamentos;
        private readonly ILogger<LancamentoExcluidoConsumer> _logger;

        public LancamentoExcluidoConsumer(
      IMongoCollection<LancamentoDocument> lancamentos,
      ILogger<LancamentoExcluidoConsumer> logger)
        {
            _lancamentos = lancamentos;
            _logger = logger;
        }

        public async Task Consume(
            ConsumeContext<LancamentoExcluidoEvent> context)
        {
            _logger.LogInformation(
           "Processando LancamentoExcluidoEvent. LancamentoId: {LancamentoId}",
           context.Message.LancamentoId);

            var evento = context.Message;

            Console.WriteLine(
                $"Lançamento excluído: {evento.LancamentoId} " +
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
                .Set(x => x.Excluido, true)
                .Set(x => x.Versao, evento.Versao);

            var resultado = await _lancamentos.UpdateOneAsync(
                filtro,
                atualizacao);

            if (resultado.MatchedCount == 0)
            {
                var documento = await _lancamentos
                    .Find(
                        Builders<LancamentoDocument>.Filter.Eq(
                            x => x.Id,
                            evento.LancamentoId))
                    .FirstOrDefaultAsync();

                if (documento is null)
                {
                    Console.WriteLine(
                        $"Lançamento {evento.LancamentoId} não encontrado no Mongo.");

                    return;
                }

                Console.WriteLine(
                    $"Exclusão ignorada. " +
                    $"Mongo versão {documento.Versao}, " +
                    $"evento versão {evento.Versao}.");

                return;
            }

            Console.WriteLine(
                $"Lançamento {evento.LancamentoId} marcado como excluído no Mongo.");


            _logger.LogInformation(
                  "LancamentoExcluidoEvent processado com sucesso. LancamentoId: {LancamentoId}",
                  context.Message.LancamentoId);
        }
    }
}