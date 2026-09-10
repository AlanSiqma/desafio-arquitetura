using Domain.Contracts.Events;
using Domain.Entities;
using MassTransit;
using MicroService.ControleLancamentos.Extession;
using MicroService.ControleLancamentos.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
namespace MicroService.ControleLancamentos.Test.Services;

public class LancamentoServiceTests
{
    [Fact]
    public async Task DeveObterLancamentoPorId()
    {
        // Arrange
        var id = Guid.NewGuid();

        var lancamentos = new List<Lancamento>
        {
            new()
            {
                Id = id,
                Descricao = "Salário",
                Valor = 5000m,
                Data = new DateTime(2026, 9, 9),
                TipoLancamento =
                    Domain.Enum.ETipoLancamento.Receita,
                Categoria = "Salário",
                Versao = 1
            }
        };

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .Options;

        var db = new Mock<AppDbContext>(options);


        db.Setup(x => x.Lancamentos)
            .ReturnsDbSet(lancamentos);

        var publishEndpoint =
            new Mock<IPublishEndpoint>();

        var logger =
            new Mock<ILogger<LancamentoService>>();

        var service = new LancamentoService(
            db.Object,
            publishEndpoint.Object,
            logger.Object);

        // Act
        var resultado = await service.ObterPorIdAsync(id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(id, resultado.Id);
        Assert.Equal("Salário", resultado.Descricao);
        Assert.Equal(5000m, resultado.Valor);
    }

    [Fact]
    public async Task DeveCriarLancamentoEPublicarEvento()
    {
        // Arrange
        var lancamentos = new List<Lancamento>();
        var idempotencyKeys = new List<IdempotencyKey>();

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .Options;

        var db = new Mock<AppDbContext>(options);

        db.Setup(x => x.Lancamentos)
            .ReturnsDbSet(lancamentos);

        db.Setup(x => x.IdempotencyKeys)
            .ReturnsDbSet(idempotencyKeys);

        var publishEndpoint =
            new Mock<IPublishEndpoint>();

        var logger =
            new Mock<ILogger<LancamentoService>>();

        var databaseFacade =
             new Mock<DatabaseFacade>(db.Object);

        var transaction =
            new Mock<IDbContextTransaction>();

        db.Setup(x => x.Database)
            .Returns(databaseFacade.Object);

        db.Setup(x => x.SaveChangesAsync(
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

        databaseFacade
            .Setup(x => x.BeginTransactionAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);

        transaction
            .Setup(x => x.CommitAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new LancamentoService(
            db.Object,
            publishEndpoint.Object,
            logger.Object);

        var lancamento = new Lancamento
        {
            Descricao = "Salário",
            Valor = 5000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário"
        };

        // Act
        var resultado = await service.CriarAsync(
            lancamento,
            "chave-123");

        // Assert
        Assert.False(resultado.Conflito);
        Assert.NotNull(resultado.Lancamento);

        Assert.NotEqual(Guid.Empty, resultado.Lancamento!.Id);
        Assert.Equal(1, resultado.Lancamento.Versao);

        publishEndpoint.Verify(
            x => x.Publish(
                It.Is<LancamentoCriadoEvent>(evento =>
                    evento.LancamentoId == resultado.Lancamento.Id &&
                    evento.Descricao == lancamento.Descricao &&
                    evento.Valor == lancamento.Valor &&
                    evento.Data == lancamento.Data &&
                    evento.TipoLancamento == lancamento.TipoLancamento &&
                    evento.Categoria == lancamento.Categoria &&
                    evento.Versao == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);

        transaction.Verify(
            x => x.CommitAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task DeveRetornarLancamentoExistenteQuandoIdempotencyKeyForReutilizadaComMesmoPayload()
    {
        // Arrange
        var id = Guid.NewGuid();

        var lancamentoExistente = new Lancamento
        {
            Id = id,
            Descricao = "Salário",
            Valor = 5000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário",
            Versao = 1
        };

        var lancamentoRequest = new Lancamento
        {
            Descricao = "Salário",
            Valor = 5000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário"
        };

        var requestHash =
            lancamentoRequest.CalculateRequestHash();

        var idempotencyKey = new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            Key = "chave-123",
            RequestHash = requestHash,
            LancamentoId = id,
            CreatedAt = DateTime.UtcNow
        };

        var lancamentos = new List<Lancamento>
    {
        lancamentoExistente
    };

        var idempotencyKeys = new List<IdempotencyKey>
    {
        idempotencyKey
    };

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .Options;

        var db = new Mock<AppDbContext>(options);

        db.Setup(x => x.Lancamentos)
            .ReturnsDbSet(lancamentos);

        db.Setup(x => x.IdempotencyKeys)
            .ReturnsDbSet(idempotencyKeys);

        var publishEndpoint =
            new Mock<IPublishEndpoint>();

        var logger =
            new Mock<ILogger<LancamentoService>>();

        var service = new LancamentoService(
            db.Object,
            publishEndpoint.Object,
            logger.Object);

        // Act
        var resultado = await service.CriarAsync(
            lancamentoRequest,
            "chave-123");

        // Assert
        Assert.False(resultado.Conflito);
        Assert.NotNull(resultado.Lancamento);

        Assert.Equal(id, resultado.Lancamento!.Id);
        Assert.Equal(
            lancamentoExistente.Descricao,
            resultado.Lancamento.Descricao);
        Assert.Equal(
            lancamentoExistente.Valor,
            resultado.Lancamento.Valor);

        publishEndpoint.Verify(
            x => x.Publish(
                It.IsAny<LancamentoCriadoEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task DeveRetornarConflitoQuandoIdempotencyKeyForReutilizadaComPayloadDiferente()
    {
        // Arrange
        var lancamentoExistente = new Lancamento
        {
            Id = Guid.NewGuid(),
            Descricao = "Salário",
            Valor = 5000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário",
            Versao = 1
        };

        var lancamentoRequest = new Lancamento
        {
            Descricao = "Salário diferente",
            Valor = 7000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário"
        };

        var idempotencyKey = new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            Key = "chave-123",
            RequestHash = lancamentoExistente.CalculateRequestHash(),
            LancamentoId = lancamentoExistente.Id,
            CreatedAt = DateTime.UtcNow
        };

        var lancamentos = new List<Lancamento>
    {
        lancamentoExistente
    };

        var idempotencyKeys = new List<IdempotencyKey>
    {
        idempotencyKey
    };

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .Options;

        var db = new Mock<AppDbContext>(options);

        db.Setup(x => x.Lancamentos)
            .ReturnsDbSet(lancamentos);

        db.Setup(x => x.IdempotencyKeys)
            .ReturnsDbSet(idempotencyKeys);

        var publishEndpoint =
            new Mock<IPublishEndpoint>();

        var logger =
            new Mock<ILogger<LancamentoService>>();

        var service = new LancamentoService(
            db.Object,
            publishEndpoint.Object,
            logger.Object);

        // Act
        var resultado = await service.CriarAsync(
            lancamentoRequest,
            "chave-123");

        // Assert
        Assert.True(resultado.Conflito);
        Assert.Null(resultado.Lancamento);

        publishEndpoint.Verify(
            x => x.Publish(
                It.IsAny<LancamentoCriadoEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task DeveAtualizarLancamentoEPublicarEvento()
    {
        // Arrange
        var id = Guid.NewGuid();

        var lancamento = new Lancamento
        {
            Id = id,
            Descricao = "Salário",
            Valor = 5000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário",
            Versao = 1
        };

        var atualizacao = new Lancamento
        {
            Descricao = "Salário atualizado",
            Valor = 6000m,
            Data = new DateTime(2026, 9, 10),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário"
        };

        var lancamentos = new List<Lancamento>
    {
        lancamento
    };

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .Options;

        var db = new Mock<AppDbContext>(options);

        db.Setup(x => x.Lancamentos)
            .ReturnsDbSet(lancamentos);

        db.Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var publishEndpoint =
            new Mock<IPublishEndpoint>();

        var logger =
            new Mock<ILogger<LancamentoService>>();

        var service = new LancamentoService(
            db.Object,
            publishEndpoint.Object,
            logger.Object);

        // Act
        var resultado = await service.AtualizarAsync(
            id,
            atualizacao);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(id, resultado.Id);
        Assert.Equal("Salário atualizado", resultado.Descricao);
        Assert.Equal(6000m, resultado.Valor);
        Assert.Equal(new DateTime(2026, 9, 10), resultado.Data);
        Assert.Equal(2, resultado.Versao);

        publishEndpoint.Verify(
            x => x.Publish(
                It.Is<LancamentoAlteradoEvent>(evento =>
                    evento.LancamentoId == id &&
                    evento.Descricao == "Salário atualizado" &&
                    evento.Valor == 6000m &&
                    evento.Versao == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);

        db.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeveRetornarNullQuandoLancamentoNaoExistir()
    {
        // Arrange
        var id = Guid.NewGuid();

        var lancamentos = new List<Lancamento>();

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .Options;

        var db = new Mock<AppDbContext>(options);

        db.Setup(x => x.Lancamentos)
            .ReturnsDbSet(lancamentos);

        var publishEndpoint =
            new Mock<IPublishEndpoint>();

        var logger =
            new Mock<ILogger<LancamentoService>>();

        var service = new LancamentoService(
            db.Object,
            publishEndpoint.Object,
            logger.Object);

        var atualizacao = new Lancamento
        {
            Descricao = "Salário atualizado",
            Valor = 6000m,
            Data = new DateTime(2026, 9, 10),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário"
        };

        // Act
        var resultado = await service.AtualizarAsync(
            id,
            atualizacao);

        // Assert
        Assert.Null(resultado);

        publishEndpoint.Verify(
            x => x.Publish(
                It.IsAny<LancamentoAlteradoEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task DeveExcluirLancamentoEPublicarEvento()
    {
        // Arrange
        var id = Guid.NewGuid();

        var lancamento = new Lancamento
        {
            Id = id,
            Descricao = "Salário",
            Valor = 5000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário",
            Versao = 1
        };

        var lancamentos = new List<Lancamento>
    {
        lancamento
    };

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .Options;

        var db = new Mock<AppDbContext>(options);

        db.Setup(x => x.Lancamentos)
            .ReturnsDbSet(lancamentos);

        db.Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var publishEndpoint =
            new Mock<IPublishEndpoint>();

        var logger =
            new Mock<ILogger<LancamentoService>>();

        var service = new LancamentoService(
            db.Object,
            publishEndpoint.Object,
            logger.Object);

        // Act
        var resultado = await service.ExcluirAsync(id);

        // Assert
        Assert.True(resultado);
        Assert.Equal(2, lancamento.Versao);

        publishEndpoint.Verify(
            x => x.Publish(
                It.Is<LancamentoExcluidoEvent>(evento =>
                    evento.LancamentoId == id &&
                    evento.Versao == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);

        db.Verify(
            x => x.Lancamentos.Remove(lancamento),
            Times.Once);

        db.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task DeveListarLancamentos()
    {
        // Arrange
        var lancamentos = new List<Lancamento>
    {
        new()
        {
            Id = Guid.NewGuid(),
            Descricao = "Salário",
            Valor = 5000m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Receita,
            Categoria = "Salário",
            Versao = 1
        },
        new()
        {
            Id = Guid.NewGuid(),
            Descricao = "Aluguel",
            Valor = 1500m,
            Data = new DateTime(2026, 9, 9),
            TipoLancamento =
                Domain.Enum.ETipoLancamento.Despesa,
            Categoria = "Moradia",
            Versao = 1
        }
    };

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .Options;

        var db = new Mock<AppDbContext>(options);

        db.Setup(x => x.Lancamentos)
            .ReturnsDbSet(lancamentos);

        var publishEndpoint =
            new Mock<IPublishEndpoint>();

        var logger =
            new Mock<ILogger<LancamentoService>>();

        var service = new LancamentoService(
            db.Object,
            publishEndpoint.Object,
            logger.Object);

        // Act
        var resultado = await service.ListarAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
        Assert.Contains(
            resultado,
            x => x.Descricao == "Salário");
        Assert.Contains(
            resultado,
            x => x.Descricao == "Aluguel");
    }
}