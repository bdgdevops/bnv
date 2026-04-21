// Controls/SparklinePanel.cs — Gráfica de línea miniatura dibujada en GDI+
using System.Drawing;
using System.Drawing.Drawing2D;
using BVN.WinForms.Theme;

namespace BVN.WinForms.Controls;

public class SparklinePanel : DbPanel
{
    private List<double> _data = [];
    private Color _lineColor   = AppTheme.AccentBlue;
    private bool  _showFill    = true;

    public List<double> Data
    {
        get => _data;
        set { _data = value; Invalidate(); }
    }

    public Color LineColor
    {
        get => _lineColor;
        set { _lineColor = value; Invalidate(); }
    }

    public bool ShowFill
    {
        get => _showFill;
        set { _showFill = value; Invalidate(); }
    }

    public SparklinePanel()
    {
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_data.Count < 2) return;

        var g   = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        float w   = Width;
        float h   = Height;
        float pad = 4f;

        double min = _data.Min();
        double max = _data.Max();
        double rng = max - min;
        if (rng == 0) rng = 1;

        var pts = new PointF[_data.Count];
        for (int i = 0; i < _data.Count; i++)
        {
            float x = i * (w - 1) / (_data.Count - 1);
            float y = pad + (float)((1 - (_data[i] - min) / rng) * (h - pad * 2));
            pts[i]  = new PointF(x, y);
        }

        // Fill area
        if (_showFill && pts.Length > 1)
        {
            var fillPts = new PointF[pts.Length + 2];
            fillPts[0] = new PointF(0, h);
            pts.CopyTo(fillPts, 1);
            fillPts[^1] = new PointF(pts[^1].X, h);

            using var fillBrush = new SolidBrush(Color.FromArgb(40, _lineColor));
            g.FillPolygon(fillBrush, fillPts);
        }

        // Line
        bool isPositive = _data.Count >= 2 && _data[^1] >= _data[0];
        var lineColor  = _showFill ? _lineColor : (isPositive ? AppTheme.GreenLight : AppTheme.RedLight);

        using var pen = new Pen(lineColor, 1.5f);
        g.DrawLines(pen, pts);

        // Last point dot
        var last = pts[^1];
        using var dotBrush = new SolidBrush(lineColor);
        g.FillEllipse(dotBrush, last.X - 2.5f, last.Y - 2.5f, 5, 5);
    }
}
