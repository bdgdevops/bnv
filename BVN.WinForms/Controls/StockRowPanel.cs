// Controls/StockRowPanel.cs — Fila custom-painted para la tabla de mercado
using System.Drawing;
using System.Drawing.Drawing2D;
using BVN.WinForms.Models;
using BVN.WinForms.Theme;

namespace BVN.WinForms.Controls;

public class StockRowPanel : DbPanel
{
    private StockModel _stock;
    private SparklinePanel _sparkline;
    private bool _isSelected;
    private Color _flashColor = Color.Transparent;

    public StockModel Stock => _stock;

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; Invalidate(); }
    }

    public event EventHandler<StockModel>? StockClicked;

    public StockRowPanel(StockModel stock)
    {
        _stock  = stock;
        Height  = 52;
        Dock    = DockStyle.Top;
        Cursor  = Cursors.Hand;

        _sparkline = new SparklinePanel
        {
            Data       = [.. stock.PriceHistory],
            Width      = 80,
            Height     = 34,
            ShowFill   = true,
            LineColor  = ColorTranslator.FromHtml(stock.ColorHex),
            Visible    = false,
        };

        Controls.Add(_sparkline);
        Click += (_, _) => StockClicked?.Invoke(this, _stock);
    }

    public void Update(StockModel updated)
    {
        bool priceUp   = updated.Price > _stock.Price;
        bool priceDown = updated.Price < _stock.Price;
        _stock = updated;

        _sparkline.Data = [.. updated.PriceHistory];

        if (priceUp)
        {
            FlashAsync(Color.FromArgb(40, AppTheme.Green));
        }
        else if (priceDown)
        {
            FlashAsync(Color.FromArgb(40, AppTheme.Red));
        }

        Invalidate();
    }

    private async void FlashAsync(Color c)
    {
        _flashColor = c;
        Invalidate();
        await Task.Delay(600);
        _flashColor = Color.Transparent;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g   = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = ClientRectangle;

        // Row background
        Color bg = _isSelected
            ? Color.FromArgb(35, AppTheme.AccentBlue)
            : _flashColor;
        if (!bg.IsEmpty && bg.A > 0)
            using (var br = new SolidBrush(bg))
                g.FillRectangle(br, bounds);

        // Bottom separator
        using (var p = new Pen(Color.FromArgb(18, 79, 156, 249)))
            g.DrawLine(p, 0, Height - 1, Width, Height - 1);

        // Hover effect (tracked separately in OnMouseEnter/Leave)
        int x = 14;
        int cy = Height / 2;

        // ── Ícono badge ──────────────────────────────────────
        var iconRect = new Rectangle(x, cy - 18, 36, 36);
        var accent   = ColorTranslator.FromHtml(_stock.ColorHex);
        using var iconBg = new SolidBrush(Color.FromArgb(28, accent));
        AppTheme.FillRoundRect(g, iconBg, iconRect, 8);

        g.DrawString(_stock.Icon, new Font("Segoe UI Emoji", 13f), Brushes.White,
            iconRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

        x += 46;

        // ── Ticker + Nombre ───────────────────────────────────
        using (var tickerBrush = new SolidBrush(AppTheme.AccentBlue))
            g.DrawString(_stock.Ticker, AppTheme.FontMonoBold, tickerBrush, x, cy - 14);

        using (var nameBrush = new SolidBrush(AppTheme.TextSecondary))
            g.DrawString(_stock.Name, AppTheme.FontSmall, nameBrush, x, cy);

        x = 250;

        // ── Precio ────────────────────────────────────────────
        using (var priceBrush = new SolidBrush(AppTheme.TextPrimary))
            g.DrawString($"${_stock.Price:N2}", AppTheme.FontMonoBold, priceBrush, x, cy - 8);

        x = 360;

        // ── Cambio absoluto ───────────────────────────────────
        var changeColor = _stock.Change >= 0 ? AppTheme.GreenLight : AppTheme.RedLight;
        string changeStr = $"{(_stock.Change >= 0 ? "+" : "")}{_stock.Change:N2}";
        using (var changeBrush = new SolidBrush(changeColor))
            g.DrawString(changeStr, AppTheme.FontMono, changeBrush, x, cy - 8);

        x = 450;

        // ── % badge ───────────────────────────────────────────
        string pctStr = $"{(_stock.ChangePct >= 0 ? "+" : "")}{_stock.ChangePct:N2}%";
        var badgeRect = new Rectangle(x, cy - 11, 80, 22);
        var badgeColor = _stock.ChangePct >= 0
            ? Color.FromArgb(30, AppTheme.Green)
            : Color.FromArgb(30, AppTheme.Red);
        using var badgeBrush = new SolidBrush(badgeColor);
        AppTheme.FillRoundRect(g, badgeBrush, badgeRect, 6);

        using var pctBrush = new SolidBrush(changeColor);
        g.DrawString(pctStr, AppTheme.FontMono, pctBrush, badgeRect,
            new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

        x = 545;

        // ── Volumen ───────────────────────────────────────────
        string volStr = _stock.Volume >= 1_000_000
            ? $"{_stock.Volume / 1_000_000.0:N1}M"
            : $"{_stock.Volume / 1_000:N0}K";
        using var volBrush = new SolidBrush(AppTheme.TextMuted);
        g.DrawString(volStr, AppTheme.FontMono, volBrush, x, cy - 8);

        // ── Sparkline (se posiciona a la derecha) ─────────────
        int sparkX = Math.Max(0, Width - 100);
        _sparkline.Location = new Point(sparkX, cy - 17);
        _sparkline.Visible  = true;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (!_isSelected) BackColor = Color.FromArgb(10, 79, 156, 249);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        BackColor = Color.Transparent;
    }
}
