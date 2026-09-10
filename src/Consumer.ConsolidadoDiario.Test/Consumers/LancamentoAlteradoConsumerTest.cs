using Consumer.ConsolidadoDiario.Consumers;
using Domain.Contracts.Events;
using Domain.Documents;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Consumer.ConsolidadoDiario.Test.Consumers;

public class LancamentoAlteradoConsumerTests
{
    [Fact]
    public async Task DeveAtualizarLancamentoQuandoEventoPossuirVersaoMaisNova()
    {
        // Arrange
        var collection =
            new Mock<IMongoCollection<LancamentoDocument>>();

        var logger =
            new Mock<ILogger<LancamentoAlteradoConsumer>>();

        var context =
            new Mock<ConsumeContext<LancamentoAlteradoEvent>>();

        var evento = new LancamentoAlteradoEvent
        {
            LancamentoId = Guid.NewGuid(),
            Descricao = "Salário atualizado",
            Valor = 6000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário",
            Versao = 2
        };

        var resultado = new Mock<UpdateResult>();

        resultado
            .SetupGet(x => x.MatchedCount)
            .Returns(1);

        context
            .Setup(x => x.Message)
            .Returns(evento);

        collection
            .Setup(x => x.UpdateOneAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.IsAny<UpdateDefinition<LancamentoDocument>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultado.Object);

        var consumer = new LancamentoAlteradoConsumer(
            collection.Object,
            logger.Object);

        // Act
        await consumer.Consume(context.Object);

        // Assert
        collection.Verify(
            x => x.UpdateOneAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.IsAny<UpdateDefinition<LancamentoDocument>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeveIgnorarEventoQuandoLancamentoNaoExistirOuVersaoNaoForMaisNova()
    {
        // Arrange
        var collection =
            new Mock<IMongoCollection<LancamentoDocument>>();

        var logger =
            new Mock<ILogger<LancamentoAlteradoConsumer>>();

        var context =
            new Mock<ConsumeContext<LancamentoAlteradoEvent>>();

        var evento = new LancamentoAlteradoEvent
        {
            LancamentoId = Guid.NewGuid(),
            Descricao = "Salário atualizado",
            Valor = 6000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário",
            Versao = 2
        };

        var resultado = new Mock<UpdateResult>();

        resultado
            .SetupGet(x => x.MatchedCount)
            .Returns(0);

        context
            .Setup(x => x.Message)
            .Returns(evento);

        collection
            .Setup(x => x.UpdateOneAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.IsAny<UpdateDefinition<LancamentoDocument>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultado.Object);

        var consumer = new LancamentoAlteradoConsumer(
            collection.Object,
            logger.Object);

        // Act
        await consumer.Consume(context.Object);

        // Assert
        collection.Verify(
            x => x.UpdateOneAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.IsAny<UpdateDefinition<LancamentoDocument>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}