// MainForm.cs — Ventana principal de BVN Stock Simulator
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using BVN.WinForms.Controls;
using BVN.WinForms.Services;
using BVN.WinForms.Theme;
using BVN.WinForms.Views;
using Velopack;

namespace BVN.WinForms;

public class MainForm : Form
{
    // ── Servicios ─────────────────────────────────────────────
    private readonly MarketSimulator _sim;
    private readonly PortfolioService _port;

    // ── Vistas ────────────────────────────────────────────────
    private readonly MarketView _viewMarket;
    private readonly PortfolioView _viewPortfolio;
    private readonly HistoryView _viewHistory;
    private readonly GitDemoView _viewGit;
    private Control _currentView;

    // ── Sidebar controles ─────────────────────────────────────
    private readonly NavButton _navMarket;
    private readonly NavButton _navPortfolio;
    private readonly NavButton _navHistory;
    private readonly NavButton _navGit;

    // ── KPIs ─────────────────────────────────────────────────
    private readonly KpiCard _kpiPortfolio;
    private readonly KpiCard _kpiPnL;
    private readonly KpiCard _kpiIndex;
    private readonly KpiCard _kpiTrades;

    // ── Header ────────────────────────────────────────────────
    private readonly Label _lblClock;
    private readonly Label _lblMarketStatus;
    private readonly Label _lblCash;
    private readonly System.Windows.Forms.Timer _clockTimer;

    // ── Toast ─────────────────────────────────────────────────
    private Panel? _toastPanel;

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var updatePath = @"\\SERVIDOR\carpeta-compartida\releases";
            var mgr = new UpdateManager(updatePath);
            if (!mgr.IsInstalled) return; // no verificar si no está instalado con Velopack
            var update = await mgr.CheckForUpdatesAsync();
            if (update == null) return; // ya está en la última versión
                                        // Preguntar al usuario
            var result = MessageBox.Show(
                $"Nueva versión {update.TargetFullRelease.Version} disponible.\n¿Desea actualizar ahora?",
                "Actualización disponible",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                await mgr.DownloadUpdatesAsync(update);
                mgr.ApplyUpdatesAndRestart(update);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al verificar actualizaciones: {ex.Message}");
        }
    }

    public MainForm()
    {
        // ── Configuración de ventana ───────────────────────────
        Text = "BVN · Bolsa de Valores Nacional";
        Size = new Size(1380, 840);
        MinimumSize = new Size(1120, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppTheme.BgBase;
        ForeColor = AppTheme.TextPrimary;
        Font = AppTheme.FontBase;
        DoubleBuffered = true;

        // Aplicar DWM dark title bar
        Load += async (_, _) =>
        {
            AppTheme.ApplyDarkTitleBar(this);
            AppTheme.RoundCorners(this);
            await CheckForUpdatesAsync();
        };

        // ── Servicios ─────────────────────────────────────────
        _sim = new MarketSimulator();
        _port = new PortfolioService();

        // ── Raíz: Header + Cuerpo ─────────────────────────────
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = AppTheme.BgBase,
            Padding = new Padding(0),
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        // ── Header ────────────────────────────────────────────
        var header = new DbPanel { Dock = DockStyle.Fill, BackColor = AppTheme.BgSurface };
        header.Paint += PaintHeaderBorder;
        root.Controls.Add(header, 0, 0);

        // Logo
        var logo = new Label
        {
            Text = "📈  BVN",
            ForeColor = AppTheme.AccentBlue,
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            Location = new Point(18, 0),
            AutoSize = false,
            Size = new Size(120, 56),
            TextAlign = ContentAlignment.MiddleLeft,
        };
        var logosub = new Label
        {
            Text = "Bolsa de Valores Nacional",
            ForeColor = AppTheme.TextMuted,
            Font = AppTheme.FontSmall,
            Location = new Point(84, 18),
            AutoSize = true,
        };
        header.Controls.AddRange([logo, logosub]);

        // KPIs in header (right side)
        _lblClock = new Label
        {
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Cascadia Code", 10f),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleRight,
        };
        _lblMarketStatus = new Label
        {
            Text = "● ABIERTO",
            ForeColor = AppTheme.GreenLight,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            AutoSize = true,
        };
        _lblCash = new Label
        {
            ForeColor = AppTheme.GreenLight,
            Font = new Font("Cascadia Code", 12f, FontStyle.Bold),
            AutoSize = true,
        };
        header.Controls.AddRange([_lblClock, _lblMarketStatus, _lblCash]);
        header.Resize += (_, _) => LayoutHeaderRight(header);

        // ── Cuerpo: Sidebar + Contenido ───────────────────────
        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 215));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.Controls.Add(body, 0, 1);

        // ── Sidebar ───────────────────────────────────────────
        var sidebar = new DbPanel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.BgSurface,
        };
        sidebar.Paint += PaintSidebarBorder;
        body.Controls.Add(sidebar, 0, 0);

        // Nav section label
        AddSidebarLabel(sidebar, "NAVEGACIÓN", 16, 18);

        _navMarket = new NavButton { TitleText = "Mercado", Icon = "📊", Active = true, Dock = DockStyle.Top, Location = new Point(0, 38) };
        _navPortfolio = new NavButton { TitleText = "Portafolio", Icon = "💼", Dock = DockStyle.Top };
        _navHistory = new NavButton { TitleText = "Historial", Icon = "📋", Dock = DockStyle.Top };

        var navSep = new DbPanel { Height = 1, Dock = DockStyle.Top, BackColor = Color.FromArgb(18, 79, 156, 249) };

        AddSidebarLabel2(sidebar, "HERRAMIENTAS");

        _navGit = new NavButton { TitleText = "Git Demo", Icon = "🌿", AccentColor = AppTheme.AccentPurple, Dock = DockStyle.Top };

        sidebar.Controls.Add(_navGit);
        sidebar.Controls.Add(navSep);
        sidebar.Controls.Add(_navHistory);
        sidebar.Controls.Add(_navPortfolio);
        sidebar.Controls.Add(_navMarket);

        // KPI cards en sidebar
        var kpiPanel = new DbPanel
        {
            Dock = DockStyle.Bottom,
            Height = 360,
            BackColor = Color.Transparent,
            Padding = new Padding(8, 8, 8, 8),
        };
        sidebar.Controls.Add(kpiPanel);

        var kpiLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 5,
            ColumnCount = 1,
            BackColor = Color.Transparent,
        };
        kpiLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
        kpiLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        kpiLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        kpiLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        kpiLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        kpiPanel.Controls.Add(kpiLayout);

        var sectionLbl = new Label
        {
            Text = "RESUMEN",
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 7f, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            Padding = new Padding(4, 0, 0, 0),
        };
        kpiLayout.Controls.Add(sectionLbl, 0, 0);

        _kpiPortfolio = new KpiCard { TitleText = "Portafolio Total", Value = "$100,000.00", Dock = DockStyle.Fill };
        _kpiPnL = new KpiCard { TitleText = "Ganancia / Pérdida", Value = "$0.00", Dock = DockStyle.Fill };
        _kpiIndex = new KpiCard { TitleText = "Índice BVN General", Value = "—", Dock = DockStyle.Fill };
        _kpiTrades = new KpiCard { TitleText = "Operaciones Hoy", Value = "0", Dock = DockStyle.Fill };

        kpiLayout.Controls.Add(_kpiPortfolio, 0, 1);
        kpiLayout.Controls.Add(_kpiPnL, 0, 2);
        kpiLayout.Controls.Add(_kpiIndex, 0, 3);
        kpiLayout.Controls.Add(_kpiTrades, 0, 4);

        // ── Contenido principal ───────────────────────────────
        var content = new DbPanel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.BgBase,
            Padding = new Padding(14, 14, 14, 14),
        };
        body.Controls.Add(content, 1, 0);

        // Vistas
        _viewMarket = new MarketView(_sim, _port);
        _viewPortfolio = new PortfolioView(_port);
        _viewHistory = new HistoryView(_port);
        _viewGit = new GitDemoView();

        _viewMarket.ToastRequested += ShowToast;

        content.Controls.AddRange([_viewGit, _viewHistory, _viewPortfolio, _viewMarket]);

        _viewPortfolio.Visible = false;
        _viewHistory.Visible = false;
        _viewGit.Visible = false;
        _currentView = _viewMarket;

        // ── Navegación ────────────────────────────────────────
        _navMarket.Clicked += (_, _) => ShowView(_viewMarket, _navMarket);
        _navPortfolio.Clicked += (_, _) => ShowView(_viewPortfolio, _navPortfolio);
        _navHistory.Clicked += (_, _) => ShowView(_viewHistory, _navHistory);
        _navGit.Clicked += (_, _) => ShowView(_viewGit, _navGit);

        // ── Reloj ─────────────────────────────────────────────
        _clockTimer = new System.Windows.Forms.Timer { Interval = 1000, Enabled = true };
        _clockTimer.Tick += (_, _) => UpdateClock(header);
        UpdateClock(header);

        // ── Iniciar simulación ────────────────────────────────
        _sim.OnTick += OnMarketTick;
        _sim.Start(2200);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _sim.Dispose();
        base.OnFormClosed(e);
    }

    // ── Market tick ───────────────────────────────────────────
    private void OnMarketTick(List<BVN.WinForms.Models.StockModel> stocks)
    {
        if (!IsHandleCreated) return;
        BeginInvoke(() =>
        {
            _port.UpdatePrices(stocks);
            _viewMarket.UpdateFromTick(stocks);

            var total = _port.TotalValue(stocks);
            var pnl = total - PortfolioService.InitialCash;
            var pnlPct = (pnl / PortfolioService.InitialCash) * 100;

            _kpiPortfolio.Value = $"${total:N2}";
            _kpiPortfolio.Invalidate();

            _kpiPnL.Value = $"{(pnl >= 0 ? "+" : "-")}${Math.Abs(pnl):N2}";
            _kpiPnL.Sub = $"{(pnlPct >= 0 ? "+" : "")}{pnlPct:N2}%";
            _kpiPnL.SubColor = pnl >= 0 ? AppTheme.GreenLight : AppTheme.RedLight;
            _kpiPnL.Invalidate();

            _kpiIndex.Value = $"{_sim.IndexValue:N2}";
            var idxPct = ((_sim.IndexValue - _sim.IndexOpen) / _sim.IndexOpen) * 100;
            _kpiIndex.Sub = $"{(idxPct >= 0 ? "+" : "")}{idxPct:N2}%";
            _kpiIndex.SubColor = idxPct >= 0 ? AppTheme.GreenLight : AppTheme.RedLight;
            _kpiIndex.Invalidate();

            _kpiTrades.Value = _port.TodayTradeCount().ToString();
            _kpiTrades.Invalidate();

            _lblCash.Text = $"  ${_port.Cash:N2}";

            if (_viewPortfolio.Visible) _viewPortfolio.Refresh();
        });
    }

    // ── Navegación ────────────────────────────────────────────
    private void ShowView(Control view, NavButton btn)
    {
        _currentView.Visible = false;
        view.Visible = true;
        _currentView = view;

        _navMarket.Active = btn == _navMarket;
        _navPortfolio.Active = btn == _navPortfolio;
        _navHistory.Active = btn == _navHistory;
        _navGit.Active = btn == _navGit;

        if (view == _viewPortfolio) _viewPortfolio.Refresh();
        if (view == _viewHistory) _viewHistory.Refresh();
    }

    // ── Reloj ─────────────────────────────────────────────────
    private void UpdateClock(Control header)
    {
        _lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
        var day = (int)DateTime.Now.DayOfWeek;
        var hour = DateTime.Now.Hour;
        bool open = day >= 1 && day <= 5 && hour >= 8 && hour < 17;
        _lblMarketStatus.Text = open ? "● ABIERTO" : "● CERRADO";
        _lblMarketStatus.ForeColor = open ? AppTheme.GreenLight : AppTheme.RedLight;
        LayoutHeaderRight(header);
    }

    private void LayoutHeaderRight(Control header)
    {
        int x = header.Width - 20;

        Action<Control> place = ctrl =>
        {
            x -= ctrl.Width + 12;
            ctrl.Location = new Point(x, (56 - ctrl.Height) / 2);
        };

        // Right-to-left
        _lblCash.Text = $"  ${_port.Cash:N2}  ";
        _lblCash.Size = _lblCash.GetPreferredSize(new Size(int.MaxValue, 56));

        place(_lblCash);
        place(_lblMarketStatus);
        place(_lblClock);
    }

    // ── Toast ─────────────────────────────────────────────────
    private void ShowToast(string msg, bool isError)
    {
        if (_toastPanel != null && !_toastPanel.IsDisposed)
        {
            Controls.Remove(_toastPanel);
            _toastPanel.Dispose();
        }

        _toastPanel = new Panel
        {
            BackColor = isError
                ? Color.FromArgb(210, 20, 6, 6)
                : Color.FromArgb(210, 4, 24, 16),
            Size = new Size(380, 66),
            Padding = new Padding(14, 10, 14, 10),
        };
        _toastPanel.Paint += (_, e) =>
        {
            var col = isError ? AppTheme.RedLight : AppTheme.GreenLight;
            using var pen = new Pen(Color.FromArgb(80, col), 1);
            using var path = AppTheme.RoundRect(new Rectangle(0, 0, _toastPanel.Width - 1, _toastPanel.Height - 1), 10);
            e.Graphics.DrawPath(pen, path);
        };
        _toastPanel.Controls.Add(new Label
        {
            Text = msg,
            ForeColor = AppTheme.TextPrimary,
            Font = AppTheme.FontBold,
            Location = new Point(14, 8),
            AutoSize = false,
            Size = new Size(352, 50),
        });

        // Position bottom-right of content area
        _toastPanel.Location = new Point(
            Width - _toastPanel.Width - 24,
            Height - _toastPanel.Height - 52);

        Controls.Add(_toastPanel);
        _toastPanel.BringToFront();

        // Auto-dismiss after 3.5s
        var t = new System.Windows.Forms.Timer { Interval = 3500 };
        t.Tick += (_, _) =>
        {
            t.Stop(); t.Dispose();
            if (_toastPanel != null && !_toastPanel.IsDisposed)
            {
                Controls.Remove(_toastPanel);
                _toastPanel.Dispose();
                _toastPanel = null;
            }
        };
        t.Start();
    }

    // ── Pintar líneas de borde ────────────────────────────────
    private static void PaintHeaderBorder(object? sender, PaintEventArgs e)
    {
        if (sender is not Control c) return;
        using var pen = new Pen(Color.FromArgb(22, 79, 156, 249));
        e.Graphics.DrawLine(pen, 0, c.Height - 1, c.Width, c.Height - 1);
    }

    private static void PaintSidebarBorder(object? sender, PaintEventArgs e)
    {
        if (sender is not Control c) return;
        using var pen = new Pen(Color.FromArgb(22, 79, 156, 249));
        e.Graphics.DrawLine(pen, c.Width - 1, 0, c.Width - 1, c.Height);
    }

    // ── Helpers sidebar ───────────────────────────────────────
    private static void AddSidebarLabel(Control sidebar, string text, int x, int y)
    {
        sidebar.Controls.Add(new Label
        {
            Text = text,
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 7f, FontStyle.Bold),
            Location = new Point(x, y),
            AutoSize = true,
        });
    }

    private static void AddSidebarLabel2(Control sidebar, string text)
    {
        sidebar.Controls.Add(new Label
        {
            Text = text,
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 7f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.BottomLeft,
            Padding = new Padding(18, 0, 0, 4),
        });
    }
}
