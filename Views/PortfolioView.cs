// Views/PortfolioView.cs — Vista del portafolio
using System.Drawing;
using BVN.WinForms.Controls;
using BVN.WinForms.Models;
using BVN.WinForms.Services;
using BVN.WinForms.Theme;

namespace BVN.WinForms.Views;

public class PortfolioView : DbUserControl
{
    private readonly PortfolioService _port;
    private readonly DataGridView     _grid;

    public PortfolioView(PortfolioService port)
    {
        _port     = port;
        Dock      = DockStyle.Fill;
        BackColor = AppTheme.BgBase;
        Padding   = new Padding(16);

        var card = new DbPanel
        {
            Dock      = DockStyle.Fill,
            BackColor = AppTheme.BgCard,
        };
        Controls.Add(card);

        var layout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            RowCount    = 2,
            ColumnCount = 1,
            BackColor   = Color.Transparent,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        card.Controls.Add(layout);

        // Header
        var header = new DbPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        header.Controls.Add(new Label
        {
            Text      = "💼 Mi Portafolio",
            ForeColor = AppTheme.TextPrimary,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            Location  = new Point(16, 0),
            AutoSize  = false,
            Size      = new Size(300, 50),
            TextAlign = ContentAlignment.MiddleLeft,
        });
        header.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(18, 79, 156, 249));
            e.Graphics.DrawLine(pen, 0, 49, header.Width, 49);
        };
        layout.Controls.Add(header, 0, 0);

        // DataGridView estilizado
        _grid = new DataGridView
        {
            Dock                   = DockStyle.Fill,
            BackgroundColor        = AppTheme.BgCard,
            GridColor              = Color.FromArgb(25, 45, 65),
            ForeColor              = AppTheme.TextPrimary,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.BgInput,
                ForeColor = AppTheme.TextMuted,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                SelectionBackColor = AppTheme.BgInput,
                SelectionForeColor = AppTheme.TextMuted,
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor          = AppTheme.BgCard,
                ForeColor          = AppTheme.TextPrimary,
                Font               = AppTheme.FontBase,
                SelectionBackColor = Color.FromArgb(30, AppTheme.AccentBlue),
                SelectionForeColor = AppTheme.TextPrimary,
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(10, 79, 156, 249),
            },
            BorderStyle      = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            RowHeadersVisible  = false,
            SelectionMode      = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly           = true,
            RowTemplate        = { Height = 44 },
            ColumnHeadersHeight = 36,
        };

        _grid.Columns.Add("Icon",    "");
        _grid.Columns.Add("Ticker",  "TICKER");
        _grid.Columns.Add("Company", "EMPRESA");
        _grid.Columns.Add("Qty",     "CANT.");
        _grid.Columns.Add("Avg",     "P. PROM.");
        _grid.Columns.Add("Current", "PRECIO ACT.");
        _grid.Columns.Add("Value",   "VALOR");
        _grid.Columns.Add("PnL",     "G/P");
        _grid.Columns.Add("PnLPct",  "VAR %");

        _grid.Columns["Icon"].FillWeight    = 30;
        _grid.Columns["Ticker"].FillWeight  = 60;
        _grid.Columns["Company"].FillWeight = 120;
        _grid.Columns["Qty"].FillWeight     = 50;
        _grid.Columns["PnLPct"].FillWeight  = 70;

        _grid.CellFormatting += GridCellFormatting;

        //layout.Controls.Add(_grid, 0, 1);

        Refresh();
    }

    public void Refresh()
    {
        _grid.Rows.Clear();
        var positions = _port.GetPositions();

        if (positions.Count == 0)
        {
            // vacío
            return;
        }

        foreach (var p in positions)
        {
            _grid.Rows.Add(
                p.Icon,
                p.Ticker,
                p.CompanyName,
                p.Quantity,
                $"${p.AvgPrice:N2}",
                $"${p.CurrentPrice:N2}",
                $"${p.CurrentValue:N2}",
                $"{(p.GainLoss >= 0 ? "+" : "-")}${Math.Abs(p.GainLoss):N2}",
                $"{(p.GainLossPct >= 0 ? "+" : "")}{p.GainLossPct:N2}%"
            );
            // Tag the row with the Position for cell formatting
            _grid.Rows[_grid.Rows.Count - 1].Tag = p;
        }

        base.Refresh();
    }

    private void GridCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_grid.Rows[e.RowIndex].Tag is not Position pos) return;

        if (_grid.Columns[e.ColumnIndex].Name is "PnL" or "PnLPct")
        {
            e.CellStyle.ForeColor = pos.IsPositive ? AppTheme.GreenLight : AppTheme.RedLight;
        }
        else if (_grid.Columns[e.ColumnIndex].Name == "Ticker")
        {
            e.CellStyle.ForeColor = AppTheme.AccentBlue;
            e.CellStyle.Font      = AppTheme.FontMonoBold;
        }
    }

    public new void Refresh(List<Position> positions)
    {
        if (!IsHandleCreated) return;
        BeginInvoke(Refresh);
    }
}
