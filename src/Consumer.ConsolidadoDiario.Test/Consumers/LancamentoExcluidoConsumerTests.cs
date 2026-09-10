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

public class LancamentoExcluidoConsumerTests
{
    [Fact]
    public async Task DeveMarcarLancamentoComoExcluidoQuandoEventoPossuirVersaoMaisNova()
    {
        // Arrange
        var collection =
            new Mock<IMongoCollection<LancamentoDocument>>();

        var logger =
            new Mock<ILogger<LancamentoExcluidoConsumer>>();

        var context =
            new Mock<ConsumeContext<LancamentoExcluidoEvent>>();

        var evento = new LancamentoExcluidoEvent
        {
            LancamentoId = Guid.NewGuid(),
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

        var consumer = new LancamentoExcluidoConsumer(
            collection.Object,
            logger.Object);

        // Act
        await consumer.Consume(context.Object);

        // Assert
        collection.Verify(
            x => x.UpdateOneAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.Is<UpdateDefinition<LancamentoDocument>>(
                    _ => true),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeveIgnorarEventoQuandoLancamentoNaoExistir()
    {
        // Arrange
        var collection =
            new Mock<IMongoCollection<LancamentoDocument>>();

        var logger =
            new Mock<ILogger<LancamentoExcluidoConsumer>>();

        var context =
            new Mock<ConsumeContext<LancamentoExcluidoEvent>>();

        var cursor =
            new Mock<IAsyncCursor<LancamentoDocument>>();

        var evento = new LancamentoExcluidoEvent
        {
            LancamentoId = Guid.NewGuid(),
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

        cursor
            .SetupSequence(x =>
                x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(false);

        cursor
            .SetupSequence(x =>
                x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        cursor
            .SetupGet(x => x.Current)
            .Returns([]);

        collection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.IsAny<FindOptions<LancamentoDocument, LancamentoDocument>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);

        var consumer = new LancamentoExcluidoConsumer(
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

        collection.Verify(
            x => x.FindAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.IsAny<FindOptions<LancamentoDocument, LancamentoDocument>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}