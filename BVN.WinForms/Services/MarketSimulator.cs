// Services/MarketSimulator.cs
using BVN.WinForms.Models;

namespace BVN.WinForms.Services;

public class MarketSimulator : IDisposable
{
    private readonly Random _rng = new();
    private System.Threading.Timer? _timer;

    public List<StockModel> Stocks { get; } = MarketData.GetStocks();
    public double IndexOpen  { get; private set; }
    public double IndexValue { get; private set; }

    // Fired on the thread-pool — subscribers must marshal to UI thread
    public event Action<List<StockModel>>? OnTick;

    public MarketSimulator()
    {
        // Seed 30 price history points per stock
        foreach (var s in Stocks)
        {
            var seed = s.Price;
            for (int i = 0; i < 30; i++)
            {
                seed *= 1 + (_rng.NextDouble() * 2 - 1) * s.Volatility;
                s.PriceHistory.Add(Math.Round(seed, 2));
            }
            s.Price = Math.Round(seed, 2);
            s.Open  = s.Price;
        }

        IndexOpen  = Stocks.Average(s => s.Price);
        IndexValue = IndexOpen;
    }

    public void Start(int intervalMs = 2200)
    {
        _timer = new System.Threading.Timer(_ => Tick(), null, 1000, intervalMs);
    }

    private void Tick()
    {
        foreach (var s in Stocks)
        {
            double drift = (_rng.NextDouble() * 2 - 1) * s.Volatility;
            double news  = _rng.NextDouble() < 0.06 ? (_rng.NextDouble() * 2 - 1) * 0.015 : 0;
            s.Price = Math.Max(0.01, Math.Round(s.Price * (1 + drift + news), 2));
            s.Volume = _rng.NextInt64(500_000, 5_000_000);

            s.PriceHistory.Add(s.Price);
            if (s.PriceHistory.Count > 60) s.PriceHistory.RemoveAt(0);
        }

        IndexValue = Math.Round(Stocks.Average(s => s.Price), 2);
        OnTick?.Invoke(Stocks);
    }

    public void Dispose() => _timer?.Dispose();
}
