// Controls/NavButton.cs — Botón de navegación lateral personalizado
using System.Drawing;
using System.Drawing.Drawing2D;
using BVN.WinForms.Theme;

namespace BVN.WinForms.Controls;

public class NavButton : DbPanel
{
    private string   _titleText= "";
    private string   _icon     = "";
    private bool     _active;
    private bool     _hover;
    private Color    _accentColor = AppTheme.AccentBlue;

    public string TitleText { get => _titleText; set { _titleText = value; Invalidate(); } }
    public string Icon  { get => _icon;  set { _icon  = value; Invalidate(); } }

    public bool   Active
    {
        get => _active;
        set { _active = value; Invalidate(); }
    }

    public Color AccentColor
    {
        get => _accentColor;
        set { _accentColor = value; Invalidate(); }
    }

    public event EventHandler? Clicked;

    public NavButton()
    {
        Height  = 42;
        Cursor  = Cursors.Hand;
        Margin  = new Padding(8, 3, 8, 3);
        Click  += (_, _) => Clicked?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bounds = new Rectangle(0, 0, Width, Height);

        // Active / hover background
        if (_active || _hover)
        {
            var alpha = _active ? 28 : 14;
            using var bg = new SolidBrush(Color.FromArgb(alpha, AppTheme.AccentBlue));
            AppTheme.FillRoundRect(g, bg, bounds, 8);
        }

        // Active indicator bar (left)
        if (_active)
        {
            using var bar = new SolidBrush(_accentColor);
            g.FillRectangle(bar, 0, 8, 3, Height - 16);
        }

        // Icon
        var iconBrush = _active
            ? new SolidBrush(_accentColor)
            : new SolidBrush(_hover ? AppTheme.TextPrimary : AppTheme.TextSecondary);

        g.DrawString(_icon, new Font("Segoe UI Emoji", 12f), iconBrush,
            new RectangleF(14, 0, 30, Height),
            new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });

        // Label
        var labelBrush = _active
            ? new SolidBrush(_accentColor)
            : new SolidBrush(_hover ? AppTheme.TextPrimary : AppTheme.TextSecondary);

        g.DrawString(_titleText, AppTheme.FontBold, labelBrush,
            new RectangleF(46, 0, Width - 50, Height),
            new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });

        iconBrush.Dispose();
        labelBrush.Dispose();
    }

    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hover = true; Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = false; Invalidate(); }
}
