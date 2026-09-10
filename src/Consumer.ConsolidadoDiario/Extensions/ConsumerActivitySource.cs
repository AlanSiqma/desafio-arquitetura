using System.Diagnostics;

namespace Consumer.ConsolidadoDiario.Observability;

public static class ConsumerActivitySource
{
    public static readonly ActivitySource Source =
        new("Consumer.ConsolidadoDiario");
}