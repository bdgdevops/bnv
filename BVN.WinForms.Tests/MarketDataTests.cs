// BVN.WinForms.Tests/MarketDataTests.cs
using BVN.WinForms.Services;

namespace BVN.WinForms.Tests;

/// <summary>
/// Pruebas unitarias para <see cref="MarketData"/>.
/// Verifica que el catálogo de acciones y los titulares de noticias sean consistentes.
/// </summary>
public class MarketDataTests
{
    // ─── GetStocks ─────────────────────────────────────────────────────────────

    [Fact(DisplayName = "GetStocks: retorna exactamente 10 acciones")]
    public void GetStocks_Returns10Stocks()
    {
        var stocks = MarketData.GetStocks();
        Assert.Equal(10, stocks.Count);
    }

    [Fact(DisplayName = "GetStocks: ninguna acción tiene Ticker vacío")]
    public void GetStocks_NoEmptyTickers()
    {
        var stocks = MarketData.GetStocks();
        Assert.All(stocks, s => Assert.False(string.IsNullOrWhiteSpace(s.Ticker)));
    }

    [Fact(DisplayName = "GetStocks: ninguna acción tiene nombre vacío")]
    public void GetStocks_NoEmptyNames()
    {
        var stocks = MarketData.GetStocks();
        Assert.All(stocks, s => Assert.False(string.IsNullOrWhiteSpace(s.Name)));
    }

}
