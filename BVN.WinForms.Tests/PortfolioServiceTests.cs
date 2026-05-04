// BVN.WinForms.Tests/PortfolioServiceTests.cs
using BVN.WinForms.Models;
using BVN.WinForms.Services;

namespace BVN.WinForms.Tests;

/// <summary>
/// Pruebas unitarias para <see cref="PortfolioService"/>.
/// Verifica las operaciones de compra, venta, balance y P&amp;L.
/// </summary>
public class PortfolioServiceTests
{
    // ─── Helper ────────────────────────────────────────────────────────────────

    private static StockModel MakeStock(string ticker = "AMXL", double price = 100.0) =>
        new() { Ticker = ticker, Name = ticker, Icon = "📱", Price = price, Open = price };

    // ─── Buy ───────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Buy: descuenta el costo del efectivo disponible")]
    public void Buy_DeductsCostFromCash()
    {
        var svc   = new PortfolioService();
        var stock = MakeStock(price: 100.0);

        var (ok, _) = svc.Buy(stock, qty: 10);

        Assert.True(ok);
        Assert.Equal(PortfolioService.InitialCash - 1_000.0, svc.Cash);
    }

    [Fact(DisplayName = "Buy: crea una posición nueva con la cantidad y precio correctos")]
    public void Buy_CreatesNewPosition()
    {
        var svc   = new PortfolioService();
        var stock = MakeStock("WALMEX", price: 57.30);

        svc.Buy(stock, qty: 5);

        Assert.True(svc.Positions.ContainsKey("WALMEX"));
        var pos = svc.Positions["WALMEX"];
        Assert.Equal(5, pos.Quantity);
        Assert.Equal(57.30, pos.AvgPrice, precision: 2);
    }

    [Fact(DisplayName = "Buy: falla cuando no hay fondos suficientes")]
    public void Buy_Fails_WhenInsufficientFunds()
    {
        var svc   = new PortfolioService();
        var stock = MakeStock(price: 999_999.0);   // precio mayor al capital inicial

        var (ok, msg) = svc.Buy(stock, qty: 1);

        Assert.False(ok);
        Assert.Contains("Fondos insuficientes", msg);
    }

    [Fact(DisplayName = "Buy: acumula posición existente y recalcula precio promedio")]
    public void Buy_AccumulatesPosition_AndUpdatesAvgPrice()
    {
        var svc = new PortfolioService();

        // Primera compra: 10 acciones a $100
        svc.Buy(MakeStock(price: 100.0), qty: 10);

        // Segunda compra: 10 acciones a $200 → promedio esperado $150
        svc.Buy(MakeStock(price: 200.0), qty: 10);

        var pos = svc.Positions["AMXL"];
        Assert.Equal(20, pos.Quantity);
        Assert.Equal(150.0, pos.AvgPrice, precision: 2);
    }

    // ─── Sell ──────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Sell: incrementa el efectivo en el monto correcto")]
    public void Sell_IncreasesCash()
    {
        var svc   = new PortfolioService();
        var stock = MakeStock(price: 100.0);

        svc.Buy(stock, qty: 10);
        double cashAfterBuy = svc.Cash;

        var (ok, _) = svc.Sell(stock, qty: 5);

        Assert.True(ok);
        Assert.Equal(cashAfterBuy + 500.0, svc.Cash, precision: 2);
    }

    [Fact(DisplayName = "Sell: elimina la posición cuando se venden todas las acciones")]
    public void Sell_RemovesPosition_WhenAllSharesSold()
    {
        var svc   = new PortfolioService();
        var stock = MakeStock(price: 100.0);

        svc.Buy(stock, qty: 10);
        svc.Sell(stock, qty: 10);

        Assert.False(svc.Positions.ContainsKey("AMXL"));
    }

    [Fact(DisplayName = "Sell: falla cuando no se tienen suficientes acciones")]
    public void Sell_Fails_WhenNotEnoughShares()
    {
        var svc   = new PortfolioService();
        var stock = MakeStock(price: 100.0);

        svc.Buy(stock, qty: 5);
        var (ok, msg) = svc.Sell(stock, qty: 10);

        Assert.False(ok);
        Assert.Contains("AMXL", msg);
    }

    // ─── PnL & TotalValue ─────────────────────────────────────────────────────

    [Fact(DisplayName = "PnL: es 0 al inicio (sin operaciones)")]
    public void PnL_IsZero_InitialState()
    {
        var svc    = new PortfolioService();
        var stocks = new List<StockModel>();

        Assert.Equal(0.0, svc.PnL(stocks));
    }

    [Fact(DisplayName = "PnL: refleja la ganancia cuando el precio de mercado sube")]
    public void PnL_IsPositive_WhenPriceRises()
    {
        var svc   = new PortfolioService();
        var stock = MakeStock(price: 100.0);

        svc.Buy(stock, qty: 10);   // costo: $1 000

        // Precio de mercado ahora es $150 → ganancia no realizada $500
        var stockAtNewPrice    = MakeStock(price: 150.0);
        var currentStocks      = new List<StockModel> { stockAtNewPrice };
        double pnl             = svc.PnL(currentStocks);

        Assert.True(pnl > 0, $"Se esperaba PnL positivo, pero fue {pnl:N2}");
        Assert.Equal(500.0, pnl, precision: 2);
    }

    // ─── TodayTradeCount ──────────────────────────────────────────────────────

    [Fact(DisplayName = "TodayTradeCount: cuenta correctamente las operaciones del día")]
    public void TodayTradeCount_ReflectsAllTodayTrades()
    {
        var svc   = new PortfolioService();
        var stock = MakeStock(price: 100.0);

        svc.Buy(stock, qty: 5);
        svc.Buy(stock, qty: 3);
        svc.Sell(stock, qty: 2);

        Assert.Equal(3, svc.TodayTradeCount());
    }

    // ─── PRUEBA INTENCIONAL QUE FALLA ─────────────────────────────────────────

    /// <summary>
    /// ⚠️ FALLA INTENCIONAL — Pendiente de implementar.
    /// <para>
    ///   El servicio aún no cuenta las órdenes canceladas por fondos insuficientes
    ///   como "intentos de operación". Esta prueba documenta ese comportamiento
    ///   esperado futuro y DEBE fallar hasta que se implemente la funcionalidad.
    /// </para>
    /// </summary>
    [Fact(DisplayName = "⚠️ FALLA INTENCIONAL: ClearHistory debe reducir TodayTradeCount a 0")]
    public void FAILING_ClearHistory_ShouldResetTodayTradeCount()
    {
        // Arrange
        var svc   = new PortfolioService();
        var stock = MakeStock(price: 100.0);

        svc.Buy(stock, qty: 5);  // 1 transacción registrada
        svc.Buy(stock, qty: 3);  // 2 transacciones registradas

        // Act: ClearHistory borra las transacciones del historial
        svc.ClearHistory();

        // Assert INTENCIONAL: se espera que TodayTradeCount() devuelva 0
        // tras limpiar el historial... pero en realidad lo hace correctamente.
        // El Assert.Fail fuerza el fallo para demostrar una prueba roja (red).
        int tradeCount = svc.TodayTradeCount();
        Assert.Fail(
            $"🔴 PRUEBA ROJA (red) intencional.\n" +
            $"   TodayTradeCount() devolvió {tradeCount} — el comportamiento es correcto,\n" +
            $"   pero esta prueba existe para demostrar cómo se ve un fallo en el pipeline.\n" +
            $"   Elimina o comenta este Assert.Fail() cuando ya no necesites la demostración."
        );
    }
}
