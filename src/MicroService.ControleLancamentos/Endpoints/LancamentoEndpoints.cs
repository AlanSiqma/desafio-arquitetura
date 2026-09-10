using Domain.Entities;
using MicroService.ControleLancamentos.Services;

namespace MicroService.ControleLancamentos.Endpoints;

public static class LancamentoEndpoints
{
    public static void MapLancamentoEndpoints(this WebApplication app)
    {
        app.MapPost("/lancamentos", Criar)
            .RequireAuthorization("LancamentosWrite");

        app.MapGet("/lancamentos/{id:guid}", ObterPorId)
            .RequireAuthorization("LancamentosRead");

        app.MapGet("/lancamentos", Listar)
            .RequireAuthorization("LancamentosRead");

        app.MapPut("/lancamentos/{id:guid}", Atualizar)
            .RequireAuthorization("LancamentosWrite");

        app.MapDelete("/lancamentos/{id:guid}", Excluir)
            .RequireAuthorization("LancamentosWrite");
    }

    private static async Task<IResult> Criar(
        HttpRequest request,
        Lancamento lancamento,
        LancamentoService service)
    {
        if (!request.Headers.TryGetValue(
                "Idempotency-Key",
                out var idempotencyKeyHeader))
        {
            return Results.BadRequest(
                "O header Idempotency-Key é obrigatório.");
        }

        var key = idempotencyKeyHeader.ToString().Trim();

        if (string.IsNullOrWhiteSpace(key))
        {
            return Results.BadRequest(
                "O header Idempotency-Key não pode ser vazio.");
        }

        if (key.Length > 100)
        {
            return Results.BadRequest(
                "O header Idempotency-Key deve ter no máximo 100 caracteres.");
        }

        var resultado = await service.CriarAsync(
            lancamento,
            key);

        if (resultado.Conflito)
        {
            return Results.Conflict(
                "A Idempotency-Key já foi utilizada para outra requisição.");
        }

        return Results.Created(
            $"/lancamentos/{resultado.Lancamento!.Id}",
            resultado.Lancamento);
    }

    private static async Task<IResult> ObterPorId(
        Guid id,
        LancamentoService service)
    {
        var lancamento = await service.ObterPorIdAsync(id);

        return lancamento is null
            ? Results.NotFound()
            : Results.Ok(lancamento);
    }

    private static async Task<IResult> Listar(
        LancamentoService service)
    {
        var lancamentos = await service.ListarAsync();

        return Results.Ok(lancamentos);
    }

    private static async Task<IResult> Atualizar(
        Guid id,
        Lancamento lancamentoAtualizado,
        LancamentoService service)
    {
        var lancamento = await service.AtualizarAsync(
            id,
            lancamentoAtualizado);

        return lancamento is null
            ? Results.NotFound()
            : Results.Ok(lancamento);
    }

    private static async Task<IResult> Excluir(
        Guid id,
        LancamentoService service)
    {
        var excluido = await service.ExcluirAsync(id);

        return excluido
            ? Results.NoContent()
            : Results.NotFound();
    }
}