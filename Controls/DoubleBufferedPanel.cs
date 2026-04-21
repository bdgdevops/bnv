// Controls/DoubleBufferedPanel.cs — Panel con DoubleBuffer activado
namespace BVN.WinForms.Controls;

/// <summary>Panel base con DoubleBuffering activado para prevenir parpadeo.</summary>
public class DbPanel : Panel
{
    public DbPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint  |
                 ControlStyles.ResizeRedraw, true);
    }
}

/// <summary>UserControl base con DoubleBuffering activado.</summary>
public class DbUserControl : UserControl
{
    public DbUserControl()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint  |
                 ControlStyles.ResizeRedraw, true);
    }
}
