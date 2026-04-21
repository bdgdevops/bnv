// Services/PortfolioService.cs
using BVN.WinForms.Models;

namespace BVN.WinForms.Services;

public class PortfolioService
{
    public const double InitialCash = 100_000.0;

    public double Cash { get; private set; } = InitialCash;
    public List<Transaction> Transactions { get; } = [];
    private readonly Dictionary<string, Position> _positions = [];

    public IReadOnlyDictionary<string, Position> Positions => _positions;

    public (bool ok, string msg) Buy(StockModel stock, int qty)
    {
        double cost = stock.Price * qty;
        if (cost > Cash) return (false, $"Fondos insuficientes. Necesitas ${cost:N2} pero tienes ${Cash:N2}.");

        Cash -= cost;
        Transactions.Add(new Transaction
        {
            Type        = TransactionType.Buy,
            Ticker      = stock.Ticker,
            CompanyName = stock.Name,
            Quantity    = qty,
            UnitPrice   = stock.Price,
        });

        if (_positions.TryGetValue(stock.Ticker, out var pos))
        {
            double totalCost = pos.AvgPrice * pos.Quantity + stock.Price * qty;
            pos.Quantity += qty;
            pos.AvgPrice  = totalCost / pos.Quantity;
        }
        else
        {
            _positions[stock.Ticker] = new Position
            {
                Ticker      = stock.Ticker,
                CompanyName = stock.Name,
                Icon        = stock.Icon,
                Quantity    = qty,
                AvgPrice    = stock.Price,
                CurrentPrice = stock.Price,
            };
        }
        return (true, "OK");
    }

    public (bool ok, string msg) Sell(StockModel stock, int qty)
    {
        if (!_positions.TryGetValue(stock.Ticker, out var pos) || pos.Quantity < qty)
            return (false, $"No tienes {qty} acción(es) de {stock.Ticker} para vender.");

        Cash += stock.Price * qty;
        Transactions.Add(new Transaction
        {
            Type        = TransactionType.Sell,
            Ticker      = stock.Ticker,
            CompanyName = stock.Name,
            Quantity    = qty,
            UnitPrice   = stock.Price,
        });

        pos.Quantity -= qty;
        if (pos.Quantity == 0) _positions.Remove(stock.Ticker);
        return (true, "OK");
    }

    public void UpdatePrices(List<StockModel> stocks)
    {
        foreach (var s in stocks)
            if (_positions.TryGetValue(s.Ticker, out var pos))
                pos.CurrentPrice = s.Price;
    }

    public double TotalValue(List<StockModel> stocks)
    {
        UpdatePrices(stocks);
        return Cash + _positions.Values.Sum(p => p.CurrentValue);
    }

    public double PnL(List<StockModel> stocks) => TotalValue(stocks) - InitialCash;

    public int TodayTradeCount() =>
        Transactions.Count(t => t.Timestamp.Date == DateTime.Today);

    public List<Position> GetPositions() => [.. _positions.Values];

    public void ClearHistory() => Transactions.Clear();
}
