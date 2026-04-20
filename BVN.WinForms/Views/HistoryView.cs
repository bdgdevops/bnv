// Views/HistoryView.cs — Vista del historial de transacciones
using System.Drawing;
using BVN.WinForms.Controls;
using BVN.WinForms.Models;
using BVN.WinForms.Services;
using BVN.WinForms.Theme;

namespace BVN.WinForms.Views;

public class HistoryView : DbUserControl
{
    private readonly PortfolioService _port;
    private readonly DataGridView     _grid;
    private readonly Button           _btnClear;

    public HistoryView(PortfolioService port)
    {
        _port     = port;
        Dock      = DockStyle.Fill;
        BackColor = AppTheme.BgBase;
        Padding   = new Padding(16);

        var card = new DbPanel { Dock = DockStyle.Fill, BackColor = AppTheme.BgCard };
        Controls.Add(card);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1,
            BackColor = Color.Transparent,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        card.Controls.Add(layout);

        // Header con botón limpiar
        var header = new DbPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        header.Controls.Add(new Label
        {
            Text      = "📋 Historial de Operaciones",
            ForeColor = AppTheme.TextPrimary,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            Location  = new Point(16, 0),
            AutoSize  = false,
            Size      = new Size(300, 50),
            TextAlign = ContentAlignment.MiddleLeft,
        });

        _btnClear = new Button
        {
            Text      = "Limpiar historial",
            BackColor = Color.Transparent,
            ForeColor = AppTheme.TextMuted,
            FlatStyle = FlatStyle.Flat,
            Font      = AppTheme.FontSmall,
            Size      = new Size(130, 28),
            Anchor    = AnchorStyles.Top | AnchorStyles.Right,
            Cursor    = Cursors.Hand,
        };
        _btnClear.FlatAppearance.BorderColor = Color.FromArgb(50, AppTheme.AccentBlue);
        _btnClear.Click += (_, _) => { _port.ClearHistory(); Refresh(); };
        _btnClear.Location = new Point(header.Width - 146, 11);

        header.Controls.Add(_btnClear);
        header.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(18, 79, 156, 249));
            e.Graphics.DrawLine(pen, 0, 49, header.Width, 49);
            // reposicionar botón limpiar
            _btnClear.Location = new Point(header.Width - 146, 11);
        };
        layout.Controls.Add(header, 0, 0);

        // Grid
        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = AppTheme.BgCard,
            GridColor       = Color.FromArgb(25, 45, 65),
            BorderStyle     = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            RowHeadersVisible  = false,
            SelectionMode      = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly           = true,
            RowTemplate        = { Height = 40 },
            ColumnHeadersHeight = 36,
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
        };

        _grid.Columns.Add("Time",    "FECHA/HORA");
        _grid.Columns.Add("Type",    "TIPO");
        _grid.Columns.Add("Ticker",  "TICKER");
        _grid.Columns.Add("Company", "EMPRESA");
        _grid.Columns.Add("Qty",     "CANT.");
        _grid.Columns.Add("Price",   "PRECIO");
        _grid.Columns.Add("Total",   "TOTAL");

        _grid.Columns["Time"].FillWeight    = 110;
        _grid.Columns["Type"].FillWeight    = 55;
        _grid.Columns["Ticker"].FillWeight  = 60;
        _grid.Columns["Company"].FillWeight = 130;

        _grid.CellFormatting += GridFormatting;
        layout.Controls.Add(_grid, 0, 1);

        Refresh();
    }

    public void Refresh()
    {
        _grid.Rows.Clear();
        foreach (var tx in Enumerable.Reverse(_port.Transactions))
        {
            _grid.Rows.Add(
                tx.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                tx.TypeLabel,
                tx.Ticker,
                tx.CompanyName,
                tx.Quantity,
                $"${tx.UnitPrice:N2}",
                $"${tx.Total:N2}"
            );
            _grid.Rows[_grid.Rows.Count - 1].Tag = tx;
        }
        base.Refresh();
    }

    private void GridFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_grid.Rows[e.RowIndex].Tag is not Transaction tx) return;

        if (_grid.Columns[e.ColumnIndex].Name == "Type")
        {
            e.CellStyle.ForeColor = tx.Type == TransactionType.Buy
                ? AppTheme.GreenLight : AppTheme.RedLight;
            e.CellStyle.Font = AppTheme.FontMonoBold;
        }
        else if (_grid.Columns[e.ColumnIndex].Name is "Ticker" or "Total")
        {
            e.CellStyle.ForeColor = tx.Type == TransactionType.Buy
                ? AppTheme.GreenLight : AppTheme.RedLight;
        }
    }

    public void OnNewTransaction()
    {
        if (!IsHandleCreated) return;
        BeginInvoke(Refresh);
    }
}
