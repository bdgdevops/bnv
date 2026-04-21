// Theme/AppTheme.cs — Sistema de diseño dark premium para WinForms
using System.Drawing;
using System.Runtime.InteropServices;

namespace BVN.WinForms.Theme;

public static class AppTheme
{
    // ── Colores base ──────────────────────────────────────────
    public static readonly Color BgBase    = ColorTranslator.FromHtml("#080D14");
    public static readonly Color BgSurface = ColorTranslator.FromHtml("#0E1623");
    public static readonly Color BgCard    = ColorTranslator.FromHtml("#111E2E");
    public static readonly Color BgInput   = ColorTranslator.FromHtml("#0C1520");
    public static readonly Color Border    = Color.FromArgb(30,  79, 156, 249);
    public static readonly Color BorderHi  = Color.FromArgb(70,  79, 156, 249);

    // ── Texto ─────────────────────────────────────────────────
    public static readonly Color TextPrimary   = ColorTranslator.FromHtml("#E8F0FE");
    public static readonly Color TextSecondary = ColorTranslator.FromHtml("#8BA3C4");
    public static readonly Color TextMuted     = ColorTranslator.FromHtml("#4D6585");

    // ── Acento ───────────────────────────────────────────────
    public static readonly Color AccentBlue   = ColorTranslator.FromHtml("#4F9CF9");
    public static readonly Color AccentCyan   = ColorTranslator.FromHtml("#22D3EE");
    public static readonly Color AccentPurple = ColorTranslator.FromHtml("#A78BFA");

    // ── Estado ────────────────────────────────────────────────
    public static readonly Color Green      = ColorTranslator.FromHtml("#10B981");
    public static readonly Color GreenLight = ColorTranslator.FromHtml("#34D399");
    public static readonly Color Red        = ColorTranslator.FromHtml("#EF4444");
    public static readonly Color RedLight   = ColorTranslator.FromHtml("#F87171");
    public static readonly Color Yellow     = ColorTranslator.FromHtml("#F59E0B");

    // ── Fuentes ───────────────────────────────────────────────
    public static readonly Font FontBase   = new("Segoe UI",    9.5f, FontStyle.Regular);
    public static readonly Font FontSmall  = new("Segoe UI",    8.5f, FontStyle.Regular);
    public static readonly Font FontBold   = new("Segoe UI",    9.5f, FontStyle.Bold);
    public static readonly Font FontTitle  = new("Segoe UI",   12f,  FontStyle.Bold);
    public static readonly Font FontHero   = new("Segoe UI",   18f,  FontStyle.Bold);
    public static readonly Font FontMono   = new("Cascadia Code", 9f, FontStyle.Regular);
    public static readonly Font FontMonoBold = new("Cascadia Code", 9f, FontStyle.Bold);
    public static readonly Font FontMonoLg = new("Cascadia Code", 11f, FontStyle.Regular);

    // ── Brushes utilitarios ───────────────────────────────────
    public static SolidBrush Br(Color c) => new(c);
    public static Pen Pn(Color c, float w = 1f) => new(c, w);

    // ── DWM: barra de título oscura ───────────────────────────
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public static void ApplyDarkTitleBar(Form form)
    {
        int dark = 1;
        DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
    }

    // ── Esquinas redondeadas (Win11) ──────────────────────────
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_ROUND = 2;

    public static void RoundCorners(Form form)
    {
        int rounded = DWMWCP_ROUND;
        DwmSetWindowAttribute(form.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref rounded, sizeof(int));
    }

    // ── Helper de gradiente ───────────────────────────────────
    public static void FillRoundRect(Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using var path = RoundRect(rect, radius);
        g.FillPath(brush, path);
    }

    public static void DrawRoundRect(Graphics g, Pen pen, Rectangle rect, int radius)
    {
        using var path = RoundRect(rect, radius);
        g.DrawPath(pen, path);
    }

    public static System.Drawing.Drawing2D.GraphicsPath RoundRect(Rectangle rect, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
