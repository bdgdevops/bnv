// Controls/KpiCard.cs — Tarjeta de indicador clave (KPI)
using System.Drawing;
using System.Drawing.Drawing2D;
using BVN.WinForms.Theme;

namespace BVN.WinForms.Controls;

public class KpiCard : DbPanel
{
    private string _titleText   = "";
    private string _value   = "—";
    private string _sub     = "";
    private Color  _subColor = AppTheme.TextMuted;

    public string TitleText    { get => _titleText;    set { _titleText    = value; Invalidate(); } }
    public string Value    { get => _value;    set { _value    = value; Invalidate(); } }
    public string Sub      { get => _sub;      set { _sub      = value; Invalidate(); } }
    public Color  SubColor { get => _subColor; set { _subColor = value; Invalidate(); } }

    public KpiCard()
    {
        Size    = new Size(200, 80);
        Padding = new Padding(14, 10, 14, 10);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

        using var bg = new SolidBrush(AppTheme.BgCard);
        AppTheme.FillRoundRect(g, bg, bounds, 10);

        using var border = new Pen(Color.FromArgb(22, 79, 156, 249));
        AppTheme.DrawRoundRect(g, border, bounds, 10);

        using var labelBrush = new SolidBrush(AppTheme.TextMuted);
        g.DrawString(_titleText.ToUpper(), AppTheme.FontSmall, labelBrush, new Point(14, 10));

        using var valBrush = new SolidBrush(AppTheme.TextPrimary);
        g.DrawString(_value, AppTheme.FontMonoBold, valBrush, new Point(14, 30));

        if (!string.IsNullOrEmpty(_sub))
        {
            using var subBrush = new SolidBrush(_subColor);
            g.DrawString(_sub, AppTheme.FontSmall, subBrush, new Point(14, 56));
        }
    }
}
