using Domain.Documents;
using Domain.Enum;
using MicroService.ConsolidadoDiario.Services;
using MongoDB.Driver;
using Moq;

namespace MicroService.ConsolidadoDiario.Test.Services;

public class ConsolidadoDiarioServiceTests
{
    [Fact]
    public async Task DeveGerarConsolidadoComReceitasDespesasESaldo()
    {
        // Arrange
        var data = new DateTime(2026, 9, 9);

        var lancamentos = new List<LancamentoDocument>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Descricao = "Salário",
                Valor = 5000m,
                Data = data.AddHours(8),
                TipoLancamento = ETipoLancamento.Receita,
                Categoria = "Salário",
                Versao = 1,
                Excluido = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Descricao = "Aluguel",
                Valor = 1500m,
                Data = data.AddHours(10),
                TipoLancamento = ETipoLancamento.Despesa,
                Categoria = "Moradia",
                Versao = 1,
                Excluido = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Descricao = "Alimentação",
                Valor = 500m,
                Data = data.AddHours(12),
                TipoLancamento = ETipoLancamento.Despesa,
                Categoria = "Alimentação",
                Versao = 1,
                Excluido = false
            }
        };

        var lancamentosCollection =
            new Mock<IMongoCollection<LancamentoDocument>>();

        var consolidadosCollection =
            new Mock<IMongoCollection<ConsolidadoDiarioDocument>>();

        var cursorLancamentos =
            new Mock<IAsyncCursor<LancamentoDocument>>();

        var cursorConsolidados =
            new Mock<IAsyncCursor<ConsolidadoDiarioDocument>>();

        cursorLancamentos
            .SetupSequence(x =>
                x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        cursorLancamentos
            .SetupGet(x => x.Current)
            .Returns(lancamentos);

        lancamentosCollection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.IsAny<FindOptions<LancamentoDocument, LancamentoDocument>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorLancamentos.Object);

        cursorConsolidados
            .SetupSequence(x =>
                x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        cursorConsolidados
            .SetupGet(x => x.Current)
            .Returns([]);

        consolidadosCollection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<ConsolidadoDiarioDocument>>(),
                It.IsAny<FindOptions<
                    ConsolidadoDiarioDocument,
                    ConsolidadoDiarioDocument>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorConsolidados.Object);

        var service = new ConsolidadoDiarioService(
            lancamentosCollection.Object,
            consolidadosCollection.Object);

        // Act
        var resultado = await service.GerarAsync(data);

        // Assert
        Assert.NotNull(resultado);

        Assert.Equal(data.Date, resultado.Data);
        Assert.Equal(5000m, resultado.TotalReceitas);
        Assert.Equal(2000m, resultado.TotalDespesas);
        Assert.Equal(3000m, resultado.Saldo);
        Assert.Equal(1, resultado.QuantidadeReceitas);
        Assert.Equal(2, resultado.QuantidadeDespesas);

        consolidadosCollection.Verify(
            x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<ConsolidadoDiarioDocument>>(),
                It.Is<ConsolidadoDiarioDocument>(consolidado =>
                    consolidado.Data == data.Date &&
                    consolidado.TotalReceitas == 5000m &&
                    consolidado.TotalDespesas == 2000m &&
                    consolidado.Saldo == 3000m &&
                    consolidado.QuantidadeReceitas == 1 &&
                    consolidado.QuantidadeDespesas == 2),
                It.Is<ReplaceOptions>(options =>
                    options.IsUpsert),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeveIgnorarLancamentosExcluidos()
    {
        // Arrange
        var data = new DateTime(2026, 9, 9);

        var lancamentos = new List<LancamentoDocument>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Descricao = "Salário",
                Valor = 5000m,
                Data = data.AddHours(8),
                TipoLancamento = ETipoLancamento.Receita,
                Categoria = "Salário",
                Versao = 1,
                Excluido = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Descricao = "Lançamento excluído",
                Valor = 10000m,
                Data = data.AddHours(10),
                TipoLancamento = ETipoLancamento.Receita,
                Categoria = "Teste",
                Versao = 2,
                Excluido = true
            }
        };

        var lancamentosCollection =
            new Mock<IMongoCollection<LancamentoDocument>>();

        var consolidadosCollection =
            new Mock<IMongoCollection<ConsolidadoDiarioDocument>>();

        var cursorLancamentos =
            new Mock<IAsyncCursor<LancamentoDocument>>();

        var cursorConsolidados =
            new Mock<IAsyncCursor<ConsolidadoDiarioDocument>>();

        var lancamentosEncontrados = lancamentos
            .Where(x => !x.Excluido)
            .ToList();

        cursorLancamentos
            .SetupSequence(x =>
                x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        cursorLancamentos
            .SetupGet(x => x.Current)
            .Returns(lancamentosEncontrados);

        lancamentosCollection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.IsAny<FindOptions<LancamentoDocument, LancamentoDocument>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorLancamentos.Object);

        cursorConsolidados
            .SetupSequence(x =>
                x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        cursorConsolidados
            .SetupGet(x => x.Current)
            .Returns([]);

        consolidadosCollection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<ConsolidadoDiarioDocument>>(),
                It.IsAny<FindOptions<
                    ConsolidadoDiarioDocument,
                    ConsolidadoDiarioDocument>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorConsolidados.Object);

        var service = new ConsolidadoDiarioService(
            lancamentosCollection.Object,
            consolidadosCollection.Object);

        // Act
        var resultado = await service.GerarAsync(data);

        // Assert
        Assert.Equal(5000m, resultado.TotalReceitas);
        Assert.Equal(0m, resultado.TotalDespesas);
        Assert.Equal(5000m, resultado.Saldo);
        Assert.Equal(1, resultado.QuantidadeReceitas);
        Assert.Equal(0, resultado.QuantidadeDespesas);
    }

    [Fact]
    public async Task DeveRetornarConsolidadoExistente()
    {
        // Arrange
        var data = new DateTime(2026, 9, 9);

        var consolidadoExistente = new ConsolidadoDiarioDocument
        {
            Id = Guid.NewGuid(),
            Data = data.Date,
            TotalReceitas = 5000m,
            TotalDespesas = 2000m,
            Saldo = 3000m,
            QuantidadeReceitas = 2,
            QuantidadeDespesas = 3,
            AtualizadoEm = DateTime.UtcNow
        };

        var lancamentosCollection =
            new Mock<IMongoCollection<LancamentoDocument>>();

        var consolidadosCollection =
            new Mock<IMongoCollection<ConsolidadoDiarioDocument>>();

        var cursorConsolidados =
            new Mock<IAsyncCursor<ConsolidadoDiarioDocument>>();

        cursorConsolidados
            .SetupSequence(x =>
                x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        cursorConsolidados
            .SetupGet(x => x.Current)
            .Returns([consolidadoExistente]);

        consolidadosCollection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<ConsolidadoDiarioDocument>>(),
                It.IsAny<FindOptions<
                    ConsolidadoDiarioDocument,
                    ConsolidadoDiarioDocument>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorConsolidados.Object);

        var service = new ConsolidadoDiarioService(
            lancamentosCollection.Object,
            consolidadosCollection.Object);

        // Act
        var resultado = await service.ObterAsync(data);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(consolidadoExistente.Id, resultado.Id);
        Assert.Equal(data.Date, resultado.Data);
        Assert.Equal(5000m, resultado.TotalReceitas);
        Assert.Equal(2000m, resultado.TotalDespesas);
        Assert.Equal(3000m, resultado.Saldo);
        Assert.Equal(2, resultado.QuantidadeReceitas);
        Assert.Equal(3, resultado.QuantidadeDespesas);

        consolidadosCollection.Verify(
            x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<ConsolidadoDiarioDocument>>(),
                It.IsAny<ConsolidadoDiarioDocument>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeveGerarConsolidadoQuandoNaoExistir()
    {
        // Arrange
        var data = new DateTime(2026, 9, 9);

        var lancamentos = new List<LancamentoDocument>
    {
        new()
        {
            Id = Guid.NewGuid(),
            Descricao = "Salário",
            Valor = 5000m,
            Data = data.AddHours(8),
            TipoLancamento = ETipoLancamento.Receita,
            Categoria = "Salário",
            Versao = 1,
            Excluido = false
        },
        new()
        {
            Id = Guid.NewGuid(),
            Descricao = "Aluguel",
            Valor = 1500m,
            Data = data.AddHours(10),
            TipoLancamento = ETipoLancamento.Despesa,
            Categoria = "Moradia",
            Versao = 1,
            Excluido = false
        }
    };

        var lancamentosCollection =
            new Mock<IMongoCollection<LancamentoDocument>>();

        var consolidadosCollection =
            new Mock<IMongoCollection<ConsolidadoDiarioDocument>>();

        var cursorLancamentos =
            new Mock<IAsyncCursor<LancamentoDocument>>();

        cursorLancamentos
            .SetupSequence(x =>
                x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);

        cursorLancamentos
            .SetupSequence(x =>
                x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        cursorLancamentos
            .SetupGet(x => x.Current)
            .Returns(lancamentos);

        lancamentosCollection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<LancamentoDocument>>(),
                It.IsAny<FindOptions<LancamentoDocument, LancamentoDocument>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorLancamentos.Object);

        consolidadosCollection
            .Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<ConsolidadoDiarioDocument>>(),
                It.IsAny<FindOptions<
                    ConsolidadoDiarioDocument,
                    ConsolidadoDiarioDocument>>(),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                var cursor =
                    new Mock<IAsyncCursor<ConsolidadoDiarioDocument>>();

                cursor
                    .SetupGet(x => x.Current)
                    .Returns([]);

                cursor
                    .Setup(x =>
                        x.MoveNext(It.IsAny<CancellationToken>()))
                    .Returns(false);

                cursor
                    .Setup(x =>
                        x.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

                return Task.FromResult(cursor.Object);
            });

        var service = new ConsolidadoDiarioService(
            lancamentosCollection.Object,
            consolidadosCollection.Object);

        // Act
        var resultado = await service.ObterAsync(data);

        // Assert
        Assert.NotNull(resultado);

        Assert.Equal(data.Date, resultado.Data);
        Assert.Equal(5000m, resultado.TotalReceitas);
        Assert.Equal(1500m, resultado.TotalDespesas);
        Assert.Equal(3500m, resultado.Saldo);
        Assert.Equal(1, resultado.QuantidadeReceitas);
        Assert.Equal(1, resultado.QuantidadeDespesas);

        consolidadosCollection.Verify(
            x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<ConsolidadoDiarioDocument>>(),
                It.Is<ConsolidadoDiarioDocument>(consolidado =>
                    consolidado.Data == data.Date &&
                    consolidado.TotalReceitas == 5000m &&
                    consolidado.TotalDespesas == 1500m &&
                    consolidado.Saldo == 3500m &&
                    consolidado.QuantidadeReceitas == 1 &&
                    consolidado.QuantidadeDespesas == 1),
                It.Is<ReplaceOptions>(options =>
                    options.IsUpsert),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}