using Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MicroService.ControleLancamentos.Extession
{
    public static class LancamentoExtenssion
    {
        public static string CalculateRequestHash(this Lancamento lancamento)
        {
            var payload = new
            {
                lancamento.Descricao,
                lancamento.Valor,
                lancamento.Data,
                lancamento.TipoLancamento,
                lancamento.Categoria
            };

            var json = JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            var bytes = SHA256.HashData(
                Encoding.UTF8.GetBytes(json));

            return Convert.ToHexString(bytes);
        }
    }
}
