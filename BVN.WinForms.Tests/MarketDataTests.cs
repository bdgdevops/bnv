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

    [Fact(DisplayName = "GetStocks: todos los precios de apertura son mayores a 0")]
    public void GetStocks_AllOpenPricesArePositive()
    {
        var stocks = MarketData.GetStocks();
        Assert.All(stocks, s => Assert.True(s.Open > 0, $"{s.Ticker}: Open debe ser > 0 pero fue {s.Open}"));
    }

    [Fact(DisplayName = "GetStocks: los Tickers son únicos (sin duplicados)")]
    public void GetStocks_TickersAreUnique()
    {
        var stocks  = MarketData.GetStocks();
        var tickers = stocks.Select(s => s.Ticker).ToList();
        Assert.Equal(tickers.Count, tickers.Distinct().Count());
    }

    [Fact(DisplayName = "GetStocks: AMXL está en el catálogo con precio inicial correcto")]
    public void GetStocks_AmxlPresent_WithCorrectPrice()
    {
        var stocks = MarketData.GetStocks();
        var amxl   = stocks.FirstOrDefault(s => s.Ticker == "AMXL");

        Assert.NotNull(amxl);
        Assert.Equal(14.85, amxl.Open, precision: 2);
    }

    [Fact(DisplayName = "GetStocks: la volatilidad de todas las acciones está en rango razonable (0–0.05)")]
    public void GetStocks_VolatilityInExpectedRange()
    {
        var stocks = MarketData.GetStocks();
        Assert.All(stocks, s =>
        {
            Assert.True(s.Volatility > 0  , $"{s.Ticker}: Volatility > 0 esperado");
            Assert.True(s.Volatility < 0.05, $"{s.Ticker}: Volatility < 0.05 esperado, fue {s.Volatility}");
        });
    }

    // ─── GetHeadlines ─────────────────────────────────────────────────────────

    [Fact(DisplayName = "GetHeadlines: retorna al menos 5 titulares de noticias")]
    public void GetHeadlines_ReturnsAtLeastFiveItems()
    {
        var headlines = MarketData.GetHeadlines();
        Assert.True(headlines.Count >= 5, $"Se esperaban ≥ 5 titulares, se obtuvieron {headlines.Count}");
    }

    [Fact(DisplayName = "GetHeadlines: ningún titular es nulo o vacío")]
    public void GetHeadlines_NoEmptyHeadlines()
    {
        var headlines = MarketData.GetHeadlines();
        Assert.All(headlines, h => Assert.False(string.IsNullOrWhiteSpace(h)));
    }
}
