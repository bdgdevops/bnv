// Models/StockModel.cs
namespace BVN.WinForms.Models;

public class StockModel
{
    public string Ticker    { get; init; } = "";
    public string Name      { get; init; } = "";
    public string Icon      { get; init; } = "";
    public string ColorHex  { get; init; } = "#4F9CF9";
    public string Sector    { get; init; } = "";

    public double Price     { get; set; }
    public double Open      { get; set; }
    public double Change    => Price - Open;
    public double ChangePct => Open == 0 ? 0 : (Price - Open) / Open * 100;
    public long   Volume    { get; set; }
    public double Volatility { get; init; } = 0.012;

    public List<double> PriceHistory { get; } = [];
}

public enum TransactionType { Buy, Sell }

public class Transaction
{
    public DateTime       Timestamp   { get; init; } = DateTime.Now;
    public TransactionType Type       { get; init; }
    public string          Ticker     { get; init; } = "";
    public string          CompanyName { get; init; } = "";
    public int             Quantity   { get; init; }
    public double          UnitPrice  { get; init; }
    public double          Total      => UnitPrice * Quantity;
    public string          TypeLabel  => Type == TransactionType.Buy ? "COMPRA" : "VENTA";
}

public class Position
{
    public string Ticker      { get; init; } = "";
    public string CompanyName { get; init; } = "";
    public string Icon        { get; init; } = "";
    public int    Quantity    { get; set; }
    public double AvgPrice    { get; set; }
    public double CurrentPrice { get; set; }
    public double CurrentValue => CurrentPrice * Quantity;
    public double GainLoss     => (CurrentPrice - AvgPrice) * Quantity;
    public double GainLossPct  => AvgPrice == 0 ? 0 : (CurrentPrice - AvgPrice) / AvgPrice * 100;
    public bool   IsPositive   => GainLoss >= 0;
}
