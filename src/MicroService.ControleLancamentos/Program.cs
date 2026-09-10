using MicroService.ControleLancamentos.Endpoints;
using MicroService.ControleLancamentos.Extensions;
using MicroService.ControleLancamentos.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationSecurity(builder.Configuration);
builder.Services.AddOpenApi();

builder.Services.AddApplicationHealthChecks();
builder.Services.AddObservability(builder.Configuration);
builder.Services.AddApplicationDatabase(builder.Configuration);
builder.Services.AddApplicationJson();
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddScoped<LancamentoService>();

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync();

app.MapHealthChecks("/health");

app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapLancamentoEndpoints();

app.Run();