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

public class LancamentoCriadoConsumerTests
{
    [Fact]
    public async Task DeveCriarLancamentoNoMongoQuandoEventoForRecebido()
    {
        // Arrange
        var collection =
            new Mock<IMongoCollection<LancamentoDocument>>();

        var logger =
            new Mock<ILogger<LancamentoCriadoConsumer>>();

        var context =
            new Mock<ConsumeContext<LancamentoCriadoEvent>>();

        var cursor =
            new Mock<IAsyncCursor<LancamentoDocument>>();

        var evento = new LancamentoCriadoEvent
        {
            LancamentoId = Guid.NewGuid(),
            Descricao = "Salário",
            Valor = 5000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário",
            Versao = 1
        };

        context
            .Setup(x => x.Message)
            .Returns(evento);

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
                It.IsAny<
                    FindOptions<
                        LancamentoDocument,
                        LancamentoDocument>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);

        var consumer = new LancamentoCriadoConsumer(
            collection.Object,
            logger.Object);

        // Act
        await consumer.Consume(context.Object);

        // Assert
        collection.Verify(
            x => x.InsertOneAsync(
                It.Is<LancamentoDocument>(documento =>
                    documento.Id == evento.LancamentoId &&
                    documento.Descricao == evento.Descricao &&
                    documento.Valor == evento.Valor &&
                    documento.Data == evento.Data &&
                    documento.TipoLancamento == evento.TipoLancamento &&
                    documento.Categoria == evento.Categoria &&
                    documento.Versao == evento.Versao &&
                    documento.Excluido == false),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeveIgnorarEventoQuandoVersaoExistenteForIgualOuMaior()
    {
        // Arrange
        var collection =
            new Mock<IMongoCollection<LancamentoDocument>>();

        var logger =
            new Mock<ILogger<LancamentoCriadoConsumer>>();

        var context =
            new Mock<ConsumeContext<LancamentoCriadoEvent>>();

        var cursor =
            new Mock<IAsyncCursor<LancamentoDocument>>();

        var lancamentoId = Guid.NewGuid();

        var evento = new LancamentoCriadoEvent
        {
            LancamentoId = lancamentoId,
            Descricao = "Salário atualizado",
            Valor = 6000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário",
            Versao = 2
        };

        var existente = new LancamentoDocument
        {
            Id = lancamentoId,
            Descricao = "Salário",
            Valor = 5000m,
            Data = evento.Data,
            TipoLancamento = evento.TipoLancamento,
            Categoria = "Salário",
            Versao = 2,
            Excluido = false
        };

        context
            .Setup(x => x.Message)
            .Returns(evento);

        cursor
            .SetupSequence(x =>
                x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);

        cursor
            .SetupSequence(x =>
                x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        cursor
            .SetupGet(x => x.Current)
            .Returns([existente]);

        collection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.IsAny<FindOptions<LancamentoDocument, LancamentoDocument>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);

        var consumer = new LancamentoCriadoConsumer(
            collection.Object,
            logger.Object);

        // Act
        await consumer.Consume(context.Object);

        // Assert
        collection.Verify(
            x => x.InsertOneAsync(
                It.IsAny<LancamentoDocument>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        collection.Verify(
            x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.IsAny<LancamentoDocument>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeveAtualizarLancamentoQuandoEventoPossuirVersaoMaisNova()
    {
        // Arrange
        var collection =
            new Mock<IMongoCollection<LancamentoDocument>>();

        var logger =
            new Mock<ILogger<LancamentoCriadoConsumer>>();

        var context =
            new Mock<ConsumeContext<LancamentoCriadoEvent>>();

        var cursor =
            new Mock<IAsyncCursor<LancamentoDocument>>();

        var lancamentoId = Guid.NewGuid();

        var evento = new LancamentoCriadoEvent
        {
            LancamentoId = lancamentoId,
            Descricao = "Salário atualizado",
            Valor = 6000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário",
            Versao = 2
        };

        var existente = new LancamentoDocument
        {
            Id = lancamentoId,
            Descricao = "Salário",
            Valor = 5000m,
            Data = evento.Data,
            TipoLancamento = evento.TipoLancamento,
            Categoria = "Salário",
            Versao = 1,
            Excluido = false
        };

        context
            .Setup(x => x.Message)
            .Returns(evento);

        cursor
            .SetupSequence(x =>
                x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);

        cursor
            .SetupSequence(x =>
                x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        cursor
            .SetupGet(x => x.Current)
            .Returns([existente]);

        collection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.IsAny<FindOptions<LancamentoDocument, LancamentoDocument>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);

        var consumer = new LancamentoCriadoConsumer(
            collection.Object,
            logger.Object);

        // Act
        await consumer.Consume(context.Object);

        // Assert
        collection.Verify(
            x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.Is<LancamentoDocument>(documento =>
                    documento.Id == evento.LancamentoId &&
                    documento.Descricao == evento.Descricao &&
                    documento.Valor == evento.Valor &&
                    documento.Versao == evento.Versao &&
                    documento.Excluido == false),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        collection.Verify(
            x => x.InsertOneAsync(
                It.IsAny<LancamentoDocument>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}