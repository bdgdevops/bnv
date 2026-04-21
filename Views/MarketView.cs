// Views/MarketView.cs — Vista del mercado de valores
using System.Drawing;
using System.Drawing.Drawing2D;
using BVN.WinForms.Controls;
using BVN.WinForms.Models;
using BVN.WinForms.Services;
using BVN.WinForms.Theme;

namespace BVN.WinForms.Views;

public class MarketView : DbUserControl
{
    private readonly MarketSimulator  _sim;
    private readonly PortfolioService _port;

    private readonly List<StockRowPanel> _rows = [];
    private StockModel? _selected;

    // ── UI Panels ─────────────────────────────────────────────
    private readonly Panel         _tablePanel;
    private readonly DbPanel       _tradePanel;
    private readonly SparklinePanel _miniChart;
    private readonly Label         _lblSelectedTicker;
    private readonly Label         _lblSelectedName;
    private readonly Label         _lblSelectedPrice;
    private readonly Label         _lblTotal;
    private readonly NumericUpDown _numQty;
    private readonly Panel         _newsPanel;

    // ── Filtro activo ─────────────────────────────────────────
    private string _filter = "all";

    public event Action<string, bool>? ToastRequested; // (msg, isError)

    public MarketView(MarketSimulator sim, PortfolioService port)
    {
        _sim  = sim;
        _port = port;
        Dock  = DockStyle.Fill;
        BackColor = AppTheme.BgBase;

        // ── Layout raíz: tabla izquierda | panel derecho
        var root = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 2,
            RowCount    = 1,
            BackColor   = Color.Transparent,
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        Controls.Add(root);

        // ── ════ PANEL IZQUIERDO: TABLA ════ ──────────────────
        var leftCard = CreateCard();
        root.Controls.Add(leftCard, 0, 0);

        var leftLayout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            RowCount    = 3,
            ColumnCount = 1,
            BackColor   = Color.Transparent,
            Padding     = new Padding(0),
        };
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));  // Header
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));  // Col headers
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Rows
        leftCard.Controls.Add(leftLayout);

        // Header de tabla
        var tblHeader = BuildTableHeader();
        leftLayout.Controls.Add(tblHeader, 0, 0);

        // Columnas
        var colHeader = BuildColumnHeaders();
        leftLayout.Controls.Add(colHeader, 0, 1);

        // Filas de acciones (en un panel scrollable)
        _tablePanel = new Panel
        {
            Dock      = DockStyle.Fill,
            AutoScroll = true,
            BackColor  = Color.Transparent,
        };
        leftLayout.Controls.Add(_tablePanel, 0, 2);

        foreach (var stock in _sim.Stocks)
        {
            var row = new StockRowPanel(stock);
            row.StockClicked += OnStockClicked;
            _rows.Add(row);
        }
        LayoutRows(_sim.Stocks);

        // ── ════ PANEL DERECHO: OPERACIÓN + NOTICIAS ════ ─────
        var rightLayout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            RowCount    = 2,
            ColumnCount = 1,
            BackColor   = Color.Transparent,
            Padding     = new Padding(10, 0, 0, 0),
        };
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        root.Controls.Add(rightLayout, 1, 0);

        // Panel de operación
        _tradePanel = CreateCard();
        rightLayout.Controls.Add(_tradePanel, 0, 0);

        // ── Contenido del panel de operación
        var tradeLayout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            RowCount    = 7,
            ColumnCount = 1,
            BackColor   = Color.Transparent,
            Padding     = new Padding(0),
        };
        tradeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // Header
        tradeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));  // Info acción
        tradeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Mini chart
        tradeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));  // Label qty
        tradeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // Qty spinner
        tradeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));  // Total
        tradeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Botones
        _tradePanel.Controls.Add(tradeLayout);

        // 0: Header
        //var trHeader = BuildSectionHeader("Operar");
        //tradeLayout.Controls.Add(trHeader, 0, 0);

        // 1: Info
        var infoPanel = new DbPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        tradeLayout.Controls.Add(infoPanel, 0, 1);

        _lblSelectedTicker = new Label
        {
            AutoSize  = false, Text = "—",
            ForeColor = AppTheme.AccentBlue,
            Font      = new Font("Cascadia Code", 15f, FontStyle.Bold),
            Location  = new Point(16, 10),
            Size      = new Size(120, 24),
        };
        _lblSelectedName = new Label
        {
            AutoSize  = false, Text = "Selecciona una acción",
            ForeColor = AppTheme.TextSecondary,
            Font      = AppTheme.FontSmall,
            Location  = new Point(16, 34),
            Size      = new Size(200, 16),
        };
        _lblSelectedPrice = new Label
        {
            AutoSize  = false, Text = "—",
            ForeColor = AppTheme.TextPrimary,
            Font      = new Font("Cascadia Code", 16f, FontStyle.Bold),
            Location  = new Point(16, 50),
            Size      = new Size(180, 22),
        };
        infoPanel.Controls.AddRange([_lblSelectedTicker, _lblSelectedName, _lblSelectedPrice]);

        // 2: Mini chart
        _miniChart = new SparklinePanel
        {
            Dock    = DockStyle.Fill,
            ShowFill = true,
        };
        var chartCard = new DbPanel
        {
            Dock      = DockStyle.Fill,
            BackColor = AppTheme.BgInput,
            Padding   = new Padding(6),
        };
        chartCard.Controls.Add(_miniChart);
        tradeLayout.Controls.Add(chartCard, 0, 2);

        // 3: Label qty
        var lblQtyTitle = new Label
        {
            Text      = "CANTIDAD DE ACCIONES",
            ForeColor = AppTheme.TextMuted,
            Font      = AppTheme.FontSmall,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            Padding   = new Padding(16, 0, 0, 0),
        };
        tradeLayout.Controls.Add(lblQtyTitle, 0, 3);

        // 4: Qty spinner
        _numQty = new NumericUpDown
        {
            Minimum   = 1,
            Maximum   = 10000,
            Value     = 1,
            Increment = 1,
            BackColor = AppTheme.BgInput,
            ForeColor = AppTheme.TextPrimary,
            Font      = new Font("Cascadia Code", 12f, FontStyle.Bold),
            BorderStyle = BorderStyle.FixedSingle,
            Dock      = DockStyle.Fill,
        };
        _numQty.ValueChanged += (_, _) => UpdateTotal();
        tradeLayout.Controls.Add(_numQty, 0, 4);

        // 5: Total
        _lblTotal = new Label
        {
            Text      = "Total estimado: —",
            ForeColor = AppTheme.AccentCyan,
            Font      = AppTheme.FontBase,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = AppTheme.BgInput,
            Padding   = new Padding(16, 0, 0, 0),
        };
        tradeLayout.Controls.Add(_lblTotal, 0, 5);

        // 6: Botones
        var btnPanel = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 2,
            RowCount    = 1,
            BackColor   = Color.Transparent,
            Padding     = new Padding(10, 8, 10, 8),
        };
        btnPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        btnPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tradeLayout.Controls.Add(btnPanel, 0, 6);

        // Panel de noticias
        var newsCard = CreateCard();
        rightLayout.Controls.Add(newsCard, 0, 1);

        var newsLayout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            RowCount    = 2,
            ColumnCount = 1,
            BackColor   = Color.Transparent,
        };
        newsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        newsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        newsCard.Controls.Add(newsLayout);

        //newsLayout.Controls.Add(BuildSectionHeader("🗞 Noticias del Mercado"), 0, 0);

        _newsPanel = new Panel
        {
            Dock      = DockStyle.Fill,
            AutoScroll = true,
            BackColor  = Color.Transparent,
        };
        newsLayout.Controls.Add(_newsPanel, 0, 1);
        LoadNews();
    }

    // ── Construir helpers ─────────────────────────────────────

    private static DbPanel CreateCard() => new()
    {
        Dock        = DockStyle.Fill,
        BackColor   = AppTheme.BgCard,
        Margin      = new Padding(0, 0, 0, 10),
    };

    private Panel BuildTableHeader()
    {
        var p = new DbPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

        var title = new Label
        {
            Text      = "Instrumentos del Mercado",
            ForeColor = AppTheme.TextPrimary,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            Location  = new Point(18, 0),
            AutoSize  = false,
            Size      = new Size(280, 52),
            TextAlign = ContentAlignment.MiddleLeft,
        };
        p.Controls.Add(title);

        // Filtros
        string[] filters = ["Todos", "▲ Ganadoras", "▼ Perdedoras"];
        string[] tags    = ["all", "gainers", "losers"];
        int xBtn = p.Width - 258;

        for (int i = 0; i < filters.Length; i++)
        {
            var tag  = tags[i];
            var text = filters[i];
            var btn  = new Button
            {
                Text      = text,
                Tag       = tag,
                ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font      = AppTheme.FontSmall,
                Size      = new Size(80, 28),
                Location  = new Point(p.Width - (3 - i) * 84, 12),
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
            };
            btn.FlatAppearance.BorderColor  = Color.FromArgb(50, AppTheme.AccentBlue);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, AppTheme.AccentBlue);
            btn.Click += (s, _) =>
            {
                _filter = tag;
                LayoutRows(_sim.Stocks);
            };
            p.Controls.Add(btn);
        }

        return p;
    }

    private static Panel BuildColumnHeaders()
    {
        var p = new DbPanel { Dock = DockStyle.Fill, BackColor = AppTheme.BgInput };

        void AddLbl(string text, int x, int width)
        {
            p.Controls.Add(new Label
            {
                Text      = text,
                ForeColor = AppTheme.TextMuted,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                Location  = new Point(x, 0),
                Size      = new Size(width, 36),
                TextAlign = ContentAlignment.MiddleLeft,
            });
        }
        AddLbl("TICKER / EMPRESA", 60,  160);
        AddLbl("PRECIO",          250, 100);
        AddLbl("CAMBIO",          360,  90);
        AddLbl("VAR %",           450,  90);
        AddLbl("VOLUMEN",         545,  80);
        AddLbl("GRÁF.",           -1,    0); // sparkline, no label
        return p;
    }

    // private static Panel BuildSectionHeader(string title)
    // {
    //     var p = new DbPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

    //     p.Paint += (_, e) =>
    //     {
    //         using var pen = new Pen(Color.FromArgb(18, 79, 156, 249));
    //         e.Graphics.DrawLine(pen, 0, p.Height - 1, p.Width, p.Height - 1);
    //     };

    //     p.Controls.Add(new Label
    //     {
    //         Text      = title,
    //         ForeColor = AppTheme.TextPrimary,
    //         Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
    //         Location  = new Point(16, 0),
    //         AutoSize  = false,
    //         Size      = new Size(300, 44),
    //         TextAlign = ContentAlignment.MiddleLeft,
    //     });
    //     return p;
    // }

    private static Button CreateActionButton(string text, Color bg, Color light) => new()
    {
        Text      = text,
        BackColor = bg,
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
        Dock      = DockStyle.Fill,
        Margin    = new Padding(4),
        Cursor    = Cursors.Hand,
        Height    = 40,
    };

    // ── Lógica ────────────────────────────────────────────────

    private void LayoutRows(List<StockModel> stocks)
    {
        _tablePanel.SuspendLayout();
        _tablePanel.Controls.Clear();

        var filtered = _filter switch
        {
            "gainers" => stocks.Where(s => s.ChangePct >= 0).ToList(),
            "losers"  => stocks.Where(s => s.ChangePct < 0).ToList(),
            _         => stocks,
        };

        foreach (var stock in filtered)
        {
            var row = _rows.FirstOrDefault(r => r.Stock.Ticker == stock.Ticker)
                      ?? new StockRowPanel(stock);
            _tablePanel.Controls.Add(row);
        }

        _tablePanel.ResumeLayout();
    }

    private void OnStockClicked(object? sender, StockModel stock)
    {
        _selected = stock;
        foreach (var r in _rows) r.IsSelected = r.Stock.Ticker == stock.Ticker;

        _lblSelectedTicker.Text = stock.Ticker;
        _lblSelectedName.Text   = stock.Name;
        _lblSelectedPrice.Text  = $"${stock.Price:N2}";
        _miniChart.Data         = [.. stock.PriceHistory];
        _miniChart.LineColor    = ColorTranslator.FromHtml(stock.ColorHex);

        UpdateTotal();
    }

    private void UpdateTotal()
    {
        if (_selected == null) return;
        double total = _selected.Price * (double)_numQty.Value;
        _lblTotal.Text = $"Total estimado:  ${total:N2}";
    }

    private void OnBuyClick(object? sender, EventArgs e)
    {
        if (_selected == null) return;
        var stock = _sim.Stocks.First(s => s.Ticker == _selected.Ticker);
        var (ok, msg) = _port.Buy(stock, (int)_numQty.Value);
        ToastRequested?.Invoke(
            ok ? $"✅ Compra ejecutada — {(int)_numQty.Value}x {stock.Ticker} @ ${stock.Price:N2}" : $"❌ {msg}",
            !ok);
    }

    private void OnSellClick(object? sender, EventArgs e)
    {
        if (_selected == null) return;
        var stock = _sim.Stocks.First(s => s.Ticker == _selected.Ticker);
        var (ok, msg) = _port.Sell(stock, (int)_numQty.Value);
        ToastRequested?.Invoke(
            ok ? $"💰 Venta ejecutada — {(int)_numQty.Value}x {stock.Ticker} @ ${stock.Price:N2}" : $"❌ {msg}",
            !ok);
    }

    public void UpdateFromTick(List<StockModel> stocks)
    {
        if (!IsHandleCreated) return;
        BeginInvoke(() =>
        {
            foreach (var row in _rows)
            {
                var updated = stocks.FirstOrDefault(s => s.Ticker == row.Stock.Ticker);
                if (updated != null) row.Update(updated);
            }

            if (_selected != null)
            {
                var s = stocks.FirstOrDefault(s2 => s2.Ticker == _selected.Ticker);
                if (s != null)
                {
                    _selected = s;
                    _lblSelectedPrice.Text = $"${s.Price:N2}";
                    _miniChart.Data        = [.. s.PriceHistory];
                    UpdateTotal();
                }
            }
        });
    }

    private void LoadNews()
    {
        var headlines = MarketData.GetHeadlines().OrderBy(_ => Guid.NewGuid()).ToList();
        int y = 0;
        foreach (var h in headlines)
        {
            var lbl = new Label
            {
                Text      = h,
                ForeColor = AppTheme.TextSecondary,
                Font      = AppTheme.FontSmall,
                Location  = new Point(0, y),
                Width     = _newsPanel.Width,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoSize  = true,
                Padding   = new Padding(12, 8, 12, 8),
                MaximumSize = new Size(_newsPanel.Width > 0 ? _newsPanel.Width : 300, 0),
            };
            _newsPanel.Controls.Add(lbl);
            y += 44;
        }
    }
}
