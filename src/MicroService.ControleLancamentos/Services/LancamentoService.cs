using Domain.Contracts.Events;
using Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using MicroService.ControleLancamentos.Extession;

namespace MicroService.ControleLancamentos.Services;

public class LancamentoService
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<LancamentoService> _logger;

    public LancamentoService(
        AppDbContext db,
        IPublishEndpoint publishEndpoint,
         ILogger<LancamentoService> logger)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<(Lancamento? Lancamento, bool Conflito)> CriarAsync(
        Lancamento lancamento,
        string idempotencyKey)
    {
        _logger.LogInformation(
    "Iniciando criação de lançamento. IdempotencyKey: {IdempotencyKey}",
    idempotencyKey);

        var requestHash = lancamento.CalculateRequestHash();

        var existente = await _db.IdempotencyKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == idempotencyKey);

        if (existente is not null)
        {
            if (existente.RequestHash != requestHash)
            {
                return (null, true);
            }

            var lancamentoExistente = await _db.Lancamentos
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == existente.LancamentoId);

            if (lancamentoExistente is null)
            {
                throw new InvalidOperationException(
                    "A Idempotency-Key existe, mas o lançamento associado não foi encontrado.");
            }

            return (lancamentoExistente, false);
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        {
            lancamento.Id = Guid.NewGuid();
            lancamento.Versao = 1;

            _db.Lancamentos.Add(lancamento);

            _db.IdempotencyKeys.Add(new IdempotencyKey
            {
                Id = Guid.NewGuid(),
                Key = idempotencyKey,
                RequestHash = requestHash,
                LancamentoId = lancamento.Id,
                CreatedAt = DateTime.UtcNow
            });

            await _publishEndpoint.Publish(
                new LancamentoCriadoEvent
                {
                    LancamentoId = lancamento.Id,
                    Descricao = lancamento.Descricao,
                    Valor = lancamento.Valor,
                    Data = lancamento.Data,
                    TipoLancamento = lancamento.TipoLancamento,
                    Categoria = lancamento.Categoria,
                    Versao = lancamento.Versao
                });

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return (lancamento, false);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException postgresException
                  && postgresException.SqlState == "23505")
        {
            await transaction.RollbackAsync();

            var existenteConcorrente = await _db.IdempotencyKeys
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Key == idempotencyKey);

            if (existenteConcorrente is null)
            {
                throw;
            }

            if (existenteConcorrente.RequestHash != requestHash)
            {
                return (null, true);
            }

            var lancamentoExistente = await _db.Lancamentos
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == existenteConcorrente.LancamentoId);

            if (lancamentoExistente is null)
            {
                throw;
            }

            return (lancamentoExistente, false);
        }
        finally
        {
            _logger.LogInformation(
    "Lançamento criado com sucesso. LancamentoId: {LancamentoId}",
    lancamento.Id);
        }
    }

    public async Task<Lancamento?> ObterPorIdAsync(Guid id)
    {
        return await _db.Lancamentos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Lancamento>> ListarAsync()
    {
        return await _db.Lancamentos
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Lancamento?> AtualizarAsync(
        Guid id,
        Lancamento lancamentoAtualizado)
    {
        var lancamento = await _db.Lancamentos
            .FirstOrDefaultAsync(x => x.Id == id);

        if (lancamento is null)
        {
            return null;
        }

        lancamento.Descricao = lancamentoAtualizado.Descricao;
        lancamento.Valor = lancamentoAtualizado.Valor;
        lancamento.Data = lancamentoAtualizado.Data;
        lancamento.TipoLancamento = lancamentoAtualizado.TipoLancamento;
        lancamento.Categoria = lancamentoAtualizado.Categoria;

        lancamento.Versao++;

        await _publishEndpoint.Publish(
            new LancamentoAlteradoEvent
            {
                LancamentoId = lancamento.Id,
                Descricao = lancamento.Descricao,
                Valor = lancamento.Valor,
                Data = lancamento.Data,
                TipoLancamento = lancamento.TipoLancamento,
                Categoria = lancamento.Categoria,
                Versao = lancamento.Versao
            });

        await _db.SaveChangesAsync();

        return lancamento;
    }

    public async Task<bool> ExcluirAsync(Guid id)
    {
        var lancamento = await _db.Lancamentos
            .FirstOrDefaultAsync(x => x.Id == id);

        if (lancamento is null)
        {
            return false;
        }

        lancamento.Versao++;

        await _publishEndpoint.Publish(
            new LancamentoExcluidoEvent
            {
                LancamentoId = lancamento.Id,
                Versao = lancamento.Versao
            });

        _db.Lancamentos.Remove(lancamento);

        await _db.SaveChangesAsync();

        return true;
    }
}