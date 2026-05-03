// Views/GitDemoView.cs — Vista Git Demo con terminal interactivo
using System.Drawing;
using System.Drawing.Drawing2D;
using BVN.WinForms.Controls;
using BVN.WinForms.Theme;

namespace BVN.WinForms.Views;

public class GitDemoView : DbUserControl
{
    private readonly GitSimulator _git = new();
    private readonly Panel        _termOutput;
    private readonly TextBox      _termInput;
    private int                   _termY;

    private readonly string[] _suggestions =
    [
        "git status", "git log --oneline", "git branch",
        "git checkout -b feature/cotizador",
        "git add .", "git commit -m \"feat: agrega reportes\"",
        "git push origin main", "git merge feature/cotizador",
        "git stash", "git tag v2.1.0", "help"
    ];

    public GitDemoView()
    {
        Dock      = DockStyle.Fill;
        BackColor = AppTheme.BgBase;

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        Controls.Add(scroll);

        var root = new FlowLayoutPanel
        {
            Dock         = DockStyle.Top,
            AutoSize     = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding      = new Padding(16, 14, 16, 16),
            BackColor    = Color.Transparent,
        };
        scroll.Controls.Add(root);
        root.Width = 1200;

        // ── HERO ─────────────────────────────────────────────
        root.Controls.Add(BuildHero());

        // ── FILA 1: Conceptos | GitFlow ───────────────────────
        var row1 = new TableLayoutPanel
        {
            ColumnCount = 2, RowCount = 1, AutoSize = true,
            BackColor   = Color.Transparent,
            Margin      = new Padding(0, 0, 0, 14),
        };
        row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        row1.Width = root.Width - 32;
        root.Controls.Add(row1);

        row1.Controls.Add(BuildConceptsCard(), 0, 0);
        row1.Controls.Add(BuildGitFlowCard(),  1, 0);

        // ── FILA 2: Comandos | Convenciones ───────────────────
        var row2 = new TableLayoutPanel
        {
            ColumnCount = 2, RowCount = 1, AutoSize = true,
            BackColor   = Color.Transparent,
            Margin      = new Padding(0, 0, 0, 14),
        };
        row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        row2.Width = root.Width - 32;
        root.Controls.Add(row2);

        row2.Controls.Add(BuildCommandsCard(),     0, 0);
        row2.Controls.Add(BuildConventionsCard(),  1, 0);

        // ── TERMINAL ──────────────────────────────────────────
        var termCard = CreateCard(root.Width - 32);
        root.Controls.Add(termCard);

        var termLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1,
            BackColor = Color.Transparent, AutoSize = true,
        };
        termLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));   // header
        termLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 240));  // output
        termLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));   // input
        termLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // suggestions
        termCard.Controls.Add(termLayout);
        termCard.Height = 378;

        // Terminal header
        var tHeader = new DbPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        tHeader.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(18, 79, 156, 249));
            e.Graphics.DrawLine(pen, 0, tHeader.Height - 1, tHeader.Width, tHeader.Height - 1);
        };
        tHeader.Controls.Add(new Label
        {
            Text      = "🖥  Terminal Git Interactivo — bvn-trading-system",
            ForeColor = AppTheme.TextPrimary,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(16, 0, 0, 0),
        });
        termLayout.Controls.Add(tHeader, 0, 0);

        // Terminal output
        _termOutput = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.FromArgb(255, 4, 8, 16),
            Padding   = new Padding(12, 8, 12, 8),
            AutoScroll = true,
        };
        termLayout.Controls.Add(_termOutput, 0, 1);

        // Terminal input row
        var inputRow = new DbPanel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.FromArgb(255, 4, 8, 16),
        };
        inputRow.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(28, 79, 156, 249));
            e.Graphics.DrawLine(pen, 0, 0, inputRow.Width, 0);
        };

        var prompt = new Label
        {
            Text      = "$ ",
            ForeColor = AppTheme.GreenLight,
            Font      = AppTheme.FontMonoBold,
            Location  = new Point(12, 0),
            AutoSize  = false,
            Size      = new Size(22, 44),
            TextAlign = ContentAlignment.MiddleLeft,
        };
        inputRow.Controls.Add(prompt);

        _termInput = new TextBox
        {
            BackColor   = Color.FromArgb(255, 4, 8, 16),
            ForeColor   = AppTheme.AccentCyan,
            Font        = new Font("Cascadia Code", 11f),
            BorderStyle = BorderStyle.None,
            Location    = new Point(36, 12),
            Width       = termCard.Width - 60,
            Anchor      = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
        };
        _termInput.KeyDown += TerminalKeyDown;
        inputRow.Controls.Add(_termInput);
        termLayout.Controls.Add(inputRow, 0, 2);

        // Sugerencias
        var sugPanel = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            BackColor     = Color.FromArgb(255, 8, 14, 22),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = false,
            Padding       = new Padding(8, 8, 8, 8),
        };
        var sugLabel = new Label
        {
            Text      = "Sugerencias:",
            ForeColor = AppTheme.TextMuted,
            Font      = AppTheme.FontSmall,
            AutoSize  = true,
            Margin    = new Padding(4, 4, 8, 0),
        };
        sugPanel.Controls.Add(sugLabel);

        foreach (var sug in _suggestions)
        {
            var capture = sug;
            var btn = new Button
            {
                Text      = capture,
                BackColor = Color.FromArgb(20, AppTheme.AccentBlue),
                ForeColor = AppTheme.AccentCyan,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Cascadia Code", 8f),
                AutoSize  = true,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(3, 0, 3, 0),
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(50, AppTheme.AccentBlue);
            btn.Click += (_, _) => { _termInput.Text = capture; RunCommand(capture); };
            sugPanel.Controls.Add(btn);
        }
        termLayout.Controls.Add(sugPanel, 0, 3);

        // Mensaje de bienvenida
        PrintLine("# Bienvenido al terminal Git del sistema BVN Trading", "comment");
        PrintLine("# Escribe 'help' para ver todos los comandos disponibles", "comment");
        PrintLine("", "out");
        PrintLine("En rama: main | Repositorio: bvn-trading-system", "out");
    }

    // ── Terminal ──────────────────────────────────────────────

    private void TerminalKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        var cmd = _termInput.Text.Trim();
        _termInput.Text = "";
        e.Handled = e.SuppressKeyPress = true;
        if (!string.IsNullOrEmpty(cmd)) RunCommand(cmd);
    }

    private void RunCommand(string cmd)
    {
        PrintPrompt(cmd);
        var output = _git.Execute(cmd);
        foreach (var (text, style) in output)
            PrintLine(text, style);
        PrintLine("", "out");
        _termOutput.ScrollControlIntoView(_termOutput.Controls.Count > 0
            ? _termOutput.Controls[^1] : _termOutput);
    }

    private void PrintPrompt(string cmd)
    {
        var lbl = new Label
        {
            AutoSize  = true,
            Font      = new Font("Cascadia Code", 9.5f),
            Location  = new Point(0, _termY),
            Padding   = new Padding(0),
        };
        lbl.Text = $"bvn@dev:~/bvn-trading-system$ {cmd}";
        lbl.ForeColor = AppTheme.AccentCyan;
        _termOutput.Controls.Add(lbl);
        _termY += 18;
    }

    private void PrintLine(string text, string style)
    {
        var color = style switch
        {
            "success" => AppTheme.GreenLight,
            "error"   => AppTheme.RedLight,
            "comment" => AppTheme.TextMuted,
            "warn"    => AppTheme.Yellow,
            _         => AppTheme.TextSecondary,
        };
        var lbl = new Label
        {
            Text      = string.IsNullOrEmpty(text) ? " " : text,
            ForeColor = color,
            Font      = style == "comment"
                ? new Font("Cascadia Code", 9f, FontStyle.Italic)
                : AppTheme.FontMono,
            AutoSize  = true,
            Location  = new Point(0, _termY),
            Padding   = new Padding(0),
            MaximumSize = new Size(_termOutput.Width - 20, 0),
        };
        _termOutput.Controls.Add(lbl);
        _termY += 17;
    }

    // ── Cards constructores ───────────────────────────────────

    private static DbPanel CreateCard(int width, int height = 0)
    {
        var p = new DbPanel
        {
            BackColor = AppTheme.BgCard,
            Width     = width,
        };
        if (height > 0) p.Height = height;
        return p;
    }

    private Panel BuildHero()
    {
        var p = new DbPanel
        {
            Width     = 1168,
            Height    = 80,
            BackColor = Color.Transparent,
            Margin    = new Padding(0, 0, 0, 16),
        };
        var lbl = new Label
        {
            Text      = "🌿  Git para Equipos de Desarrollo — BVN Trading System",
            ForeColor = AppTheme.AccentBlue,
            Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
            AutoSize  = true,
            Location  = new Point(0, 10),
        };
        var sub = new Label
        {
            Text      = "Flujo de trabajo profesional para el equipo BVN · Conventional Commits · GitFlow",
            ForeColor = AppTheme.TextSecondary,
            Font      = new Font("Segoe UI", 10f),
            AutoSize  = true,
            Location  = new Point(0, 46),
        };
        p.Controls.AddRange([lbl, sub]);
        return p;
    }

    private Panel BuildConceptsCard()
    {
        var card = new DbPanel { Dock = DockStyle.Fill, BackColor = AppTheme.BgCard, Height = 320, Margin = new Padding(0, 0, 8, 0) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 6, ColumnCount = 1, BackColor = Color.Transparent };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        for (int i = 0; i < 5; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        card.Controls.Add(layout);

        layout.Controls.Add(BuildCardHeader("🔑 Conceptos Clave de Git"), 0, 0);
        var concepts = new[]
        {
            ("📁", "Repository (Repo)", "Historial completo del proyecto BVN con todos sus cambios desde el inicio."),
            ("🌿", "Branch (Rama)",     "Línea independiente de desarrollo. 'main' = producción, 'feature/' = desarrollo."),
            ("📸", "Commit",            "Snapshot del proyecto. Cada commit tiene un hash único e inmutable."),
            ("🔀", "Pull Request",      "Solicitud de integración de cambios con revisión de código del equipo."),
            ("🏷️","Tag / Release",    "Marcador de versión estable: v1.0.0, v2.3.1 para releases en producción."),
        };
        for (int i = 0; i < concepts.Length; i++)
        {
            var (icon, title, desc) = concepts[i];
            layout.Controls.Add(BuildConceptRow(icon, title, desc), 0, i + 1);
        }
        return card;
    }

    private Panel BuildGitFlowCard()
    {
        var card = new DbPanel { Dock = DockStyle.Fill, BackColor = AppTheme.BgCard, Height = 320, Margin = new Padding(8, 0, 0, 0) };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = Color.Transparent,
            RowCount = 2, ColumnCount = 1,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        card.Controls.Add(layout);

        layout.Controls.Add(BuildCardHeader("🔄 Flujo GitFlow — BVN Trading System"), 0, 0);

        var flowPanel = new DbPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        flowPanel.Paint += PaintGitFlow;
        layout.Controls.Add(flowPanel, 0, 1);
        return card;
    }

    private void PaintGitFlow(object? sender, PaintEventArgs e)
    {
        if (sender is not Control p) return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var branches = new[]
        {
            ("main (producción)",    AppTheme.GreenLight,  4, new[] {120, 270, 420, 560}),
            ("develop",              AppTheme.AccentBlue,  60, new[] {90, 200, 380}),
            ("feature/order-book",   AppTheme.AccentPurple,110, new[] {150, 300}),
            ("hotfix/price-feed",    AppTheme.RedLight,   155, new[] {490}),
        };

        int y = 40;
        foreach (var (name, color, offsetX, dotXs) in branches)
        {
            // Lines between dots
            for (int i = 0; i < dotXs.Length - 1; i++)
            {
                using var linePen = new Pen(Color.FromArgb(60, color), 2);
                g.DrawLine(linePen, dotXs[i] + offsetX, y, dotXs[i + 1] + offsetX, y);
            }

            // Dots
            for (int i = 0; i < dotXs.Length; i++)
            {
                bool isHead = i == dotXs.Length - 1;
                float r = isHead ? 7f : 5f;
                using var dotFill = new SolidBrush(Color.FromArgb(isHead ? 180 : 60, color));
                using var dotBrd  = new Pen(color, isHead ? 2 : 1.5f);
                g.FillEllipse(dotFill, dotXs[i] + offsetX - r, y - r, r * 2, r * 2);
                g.DrawEllipse(dotBrd,  dotXs[i] + offsetX - r, y - r, r * 2, r * 2);
            }

            // Label badge
            var nameLbl = name;
            using var labelBg   = new SolidBrush(Color.FromArgb(28, color));
            using var labelBrush = new SolidBrush(color);
            var   sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            var   lbRect = new Rectangle(4, y - 12, 165, 24);
            AppTheme.FillRoundRect(g, labelBg, lbRect, 5);
            g.DrawString(nameLbl, AppTheme.FontMono, labelBrush, lbRect, sf);

            y += 52;
        }

        // Tip
        using var tipBg = new SolidBrush(Color.FromArgb(20, AppTheme.AccentBlue));
        var tipRect = new Rectangle(8, p.Height - 68, p.Width - 16, 56);
        AppTheme.FillRoundRect(g, tipBg, tipRect, 8);
        using var tipBrush = new SolidBrush(AppTheme.AccentBlue);
        g.DrawString("💡 Cada feature se desarrolla en rama separada y se integra\n" +
                     "   a 'develop' vía Pull Request con revisión de equipo.",
            AppTheme.FontSmall, tipBrush, new Rectangle(tipRect.X + 10, tipRect.Y + 8, tipRect.Width - 20, tipRect.Height - 10));
    }

    private Panel BuildCommandsCard()
    {
        var card = new DbPanel { Dock = DockStyle.Fill, BackColor = AppTheme.BgCard, Height = 380, Margin = new Padding(0, 0, 8, 0) };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = Color.Transparent,
            RowCount = 2, ColumnCount = 1,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        card.Controls.Add(layout);

        layout.Controls.Add(BuildCardHeader("💻 Comandos Esenciales"), 0, 0);

        var list = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, BackColor = Color.Transparent,
            Padding = new Padding(10, 5, 10, 5), AutoScroll = true,
        };
        layout.Controls.Add(list, 0, 1);

        var commands = new[]
        {
            ("INICIAR / CLONAR", "", ""),
            ("", "git init",                         "Inicializa un repositorio nuevo"),
            ("", "git clone <url>",                   "Clona el repositorio BVN remoto"),
            ("GUARDAR CAMBIOS", "", ""),
            ("", "git status",                        "Ver qué archivos han cambiado"),
            ("", "git add .",                         "Preparar todos los cambios"),
            ("", "git commit -m \"feat: texto\"",    "Guardar snapshot con mensaje"),
            ("RAMAS (BRANCHES)", "", ""),
            ("", "git checkout -b feature/reporte",  "Crear y cambiarse a nueva rama"),
            ("", "git merge feature/reporte",        "Integrar la rama a develop"),
            ("SINCRONIZAR", "", ""),
            ("", "git pull origin develop",          "Descargar cambios del equipo"),
            ("", "git push origin feature/reporte",  "Subir rama al repositorio remoto"),
        };

        foreach (var (grp, cmd, desc) in commands)
        {
            if (!string.IsNullOrEmpty(grp))
            {
                list.Controls.Add(new Label
                {
                    Text      = grp,
                    ForeColor = AppTheme.TextMuted,
                    Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    AutoSize  = true,
                    Margin    = new Padding(4, 10, 0, 2),
                });
            }
            else
            {
                var row = new DbPanel { Height = 30, AutoSize = false, Width = 540 };
                row.Controls.Add(new Label
                {
                    Text      = cmd,
                    ForeColor = AppTheme.AccentCyan,
                    Font      = AppTheme.FontMono,
                    Location  = new Point(0, 5),
                    Width     = 230,
                    AutoSize  = false,
                });
                row.Controls.Add(new Label
                {
                    Text      = desc,
                    ForeColor = AppTheme.TextSecondary,
                    Font      = AppTheme.FontSmall,
                    Location  = new Point(235, 7),
                    Width     = 300,
                    AutoSize  = false,
                });
                list.Controls.Add(row);
            }
        }

        return card;
    }

    private Panel BuildConventionsCard()
    {
        var card = new DbPanel { Dock = DockStyle.Fill, BackColor = AppTheme.BgCard, Height = 380, Margin = new Padding(8, 0, 0, 0) };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = Color.Transparent,
            RowCount = 2, ColumnCount = 1,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        card.Controls.Add(layout);

        layout.Controls.Add(BuildCardHeader("📝 Conventional Commits — BVN"), 0, 0);

        var list = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, BackColor = Color.Transparent,
            Padding = new Padding(12, 8, 12, 8),
        };
        layout.Controls.Add(list, 0, 1);

        var types = new[]
        {
            ("feat",     "#4F9CF9", "Nueva funcionalidad — feat: agrega gráfica de velas"),
            ("fix",      "#F87171", "Corrección — fix: precio incorrecto en ticker PETMX"),
            ("docs",     "#F59E0B", "Documentación — docs: actualiza README del módulo"),
            ("refactor", "#A78BFA", "Mejora interna — refactor: extrae lógica comisiones"),
            ("test",     "#34D399", "Pruebas — test: unit tests del motor de precios"),
            ("chore",    "#94A3B8", "Mantenimiento — chore: actualiza dependencias NuGet"),
        };

        foreach (var (badge, hex, desc) in types)
        {
            var row  = new DbPanel { Height = 32, Width = 550, AutoSize = false };
            var clr  = ColorTranslator.FromHtml(hex);
            var badgePnl = new DbPanel
            {
                Size      = new Size(65, 22),
                Location  = new Point(0, 5),
                BackColor = Color.FromArgb(35, clr),
            };
            badgePnl.Controls.Add(new Label
            {
                Text      = badge,
                ForeColor = clr,
                Font      = AppTheme.FontMonoBold,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
            });
            row.Controls.Add(badgePnl);
            row.Controls.Add(new Label
            {
                Text      = desc,
                ForeColor = AppTheme.TextSecondary,
                Font      = AppTheme.FontSmall,
                Location  = new Point(74, 8),
                Width     = 470,
                AutoSize  = false,
            });
            list.Controls.Add(row);
        }

        // Tip
        var tipPanel = new DbPanel
        {
            Width     = 550,
            Height    = 52,
            BackColor = Color.FromArgb(20, AppTheme.Yellow),
            Margin    = new Padding(0, 8, 0, 0),
        };
        tipPanel.Controls.Add(new Label
        {
            Text      = "💡 Un buen mensaje de commit permite auditar el sistema\n   de trading sin necesidad de abrir el código fuente.",
            ForeColor = AppTheme.Yellow,
            Font      = AppTheme.FontSmall,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(10, 0, 0, 0),
        });
        list.Controls.Add(tipPanel);

        return card;
    }

    private static Panel BuildCardHeader(string title)
    {
        var p = new DbPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        p.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(18, 79, 156, 249));
            e.Graphics.DrawLine(pen, 0, p.Height - 1, p.Width, p.Height - 1);
        };
        p.Controls.Add(new Label
        {
            Text      = title,
            ForeColor = AppTheme.TextPrimary,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(16, 0, 0, 0),
        });
        return p;
    }

    private static Panel BuildConceptRow(string icon, string title, string desc)
    {
        var p = new DbPanel { Dock = DockStyle.Fill, BackColor = AppTheme.BgInput, Margin = new Padding(8, 3, 8, 3) };
        p.Controls.Add(new Label
        {
            Text      = icon,
            Font      = new Font("Segoe UI Emoji", 13f),
            Location  = new Point(8, 12),
            AutoSize  = true,
        });
        p.Controls.Add(new Label
        {
            Text      = title,
            ForeColor = AppTheme.TextPrimary,
            Font      = AppTheme.FontBold,
            Location  = new Point(36, 8),
            AutoSize  = true,
        });
        p.Controls.Add(new Label
        {
            Text      = desc,
            ForeColor = AppTheme.TextSecondary,
            Font      = AppTheme.FontSmall,
            Location  = new Point(36, 26),
            Width     = 450,
            AutoSize  = false,
        });
        return p;
    }
}

// ── Git Simulator ─────────────────────────────────────────────────────────────

public class GitSimulator
{
    private readonly Random _rng = new();
    private string _branch = "main";
    private readonly List<string> _branches = ["main", "develop", "feature/order-book", "release/v2.0"];
    private readonly List<string> _staged   = [];
    private List<string> _modified = ["src/market/PriceEngine.cs", "src/api/TradingController.cs", "README.md"];

    record Commit(string Hash, string Author, string Date, string Msg);
    private readonly List<Commit> _log =
    [
        new("a3f8d21", "María González", "2026-04-18", "feat: agrega módulo de órdenes"),
        new("b7c1e09", "Carlos López",   "2026-04-17", "fix: corrige cálculo de comisiones"),
        new("c2d4f81", "Ana Ramírez",    "2026-04-16", "refactor: extrae validación de órdenes"),
        new("e9a2b44", "Luis Torres",    "2026-04-15", "feat: integra feed de precios en tiempo real"),
        new("f3c7d19", "María González", "2026-04-14", "docs: actualiza documentación API REST"),
        new("1a8e22c", "Carlos López",   "2026-04-13", "chore: actualiza NuGet packages LTS"),
    ];

    private string RndHash() => _rng.Next(0x1000000, 0xFFFFFFF).ToString("x7");

    public List<(string text, string style)> Execute(string raw)
    {
        if (raw == "help")    return Help();
        if (raw is "clear" or "cls") return [];

        var parts = raw.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return [];
        if (parts[0] != "git")
            return [($"bash: {parts[0]}: comando no reconocido. Escribe 'help'", "error")];
        if (parts.Length < 2) return [("git: esperando subcomando", "error")];

        return parts[1] switch
        {
            "status"   => Status(),
            "log"      => Log(parts),
            "branch"   => Branch(parts),
            "checkout" => Checkout(parts),
            "add"      => Add(parts),
            "commit"   => DoCommit(parts),
            "push"     => Push(parts),
            "pull"     => Pull(),
            "merge"    => Merge(parts),
            "diff"     => Diff(),
            "stash"    => Stash(parts),
            "tag"      => Tag(parts),
            "remote"   => [("origin\thttps://github.com/bvn/trading-system.git (fetch)", "out"),
                           ("origin\thttps://github.com/bvn/trading-system.git (push)", "out")],
            "fetch"    => [("Desde https://github.com/bvn/trading-system", "out"),
                           ($"   a3f8d21..{RndHash()}  main → origin/main", "success")],
            "init"     => [("Repositorio Git ya inicializado en .git/", "success")],
            "clone"    => [("Clonando en 'bvn-trading-system'...", "out"),
                           ("recibiendo objetos: 100% (847/847), hecho.", "success")],
            _ => [($"git: '{parts[1]}' no es un comando git válido en esta demo.", "error"),
                  ("Escribe 'help' para ver los comandos disponibles.", "out")]
        };
    }

    List<(string, string)> Status() =>
    [
        ($"En rama {_branch}", "out"),
        ("Tu rama está actualizada con origin/" + _branch + ".", "out"),
        ("", "out"),
        .. (_staged.Count > 0
            ? new[] { ("Cambios en staging:", "success") }
                .Concat(_staged.Select(f => ($"  modificado:   {f}", "success")))
            : Array.Empty<(string, string)>()),
        .. (_modified.Count > 0
            ? new[] { ("Cambios sin preparar:", "error") }
                .Concat(_modified.Select(f => ($"  modificado:   {f}", "error")))
            : new[] { ("nada que hacer — árbol limpio", "out") })
    ];

    List<(string, string)> Log(string[] parts)
    {
        bool oneline = parts.Contains("--oneline");
        int  n       = parts.Contains("-n") ? int.Parse(parts[Array.IndexOf(parts, "-n") + 1]) : _log.Count;
        var  r       = new List<(string, string)>();
        foreach (var c in _log.Take(n))
        {
            if (oneline) r.Add(($"{c.Hash} {c.Msg}", "out"));
            else
            {
                r.Add(($"commit {c.Hash}", "warn"));
                r.Add(($"Author: {c.Author}", "out"));
                r.Add(($"Date:   {c.Date}", "out"));
                r.Add(($"    {c.Msg}", "out"));
                r.Add(("", "out"));
            }
        }
        return r;
    }

    List<(string, string)> Branch(string[] p)
    {
        if (p.Length == 2)
            return _branches.Select(b => b == _branch ? ($"* {b}", "success") : ($"  {b}", "out")).ToList();
        if (p.Length >= 3 && p[2] == "-d")
            return [($"Rama '{p[3]}' eliminada.", "success")];
        _branches.Add(p[2]);
        return [($"Rama '{p[2]}' creada.", "success")];
    }

    List<(string, string)> Checkout(string[] p)
    {
        bool isNew = p.Contains("-b");
        var  name  = isNew ? (p.Length > 3 ? p[3] : "") : (p.Length > 2 ? p[2] : "");
        if (string.IsNullOrEmpty(name)) return [("error: especifica una rama", "error")];
        if (isNew) { _branches.Add(name); _branch = name; return [($"Cambiado a nueva rama '{name}'", "success")]; }
        if (!_branches.Contains(name)) return [($"error: '{name}' no existe.", "error")];
        _branch = name;
        return [($"Cambiado a rama '{name}'", "success")];
    }

    List<(string, string)> Add(string[] p)
    {
        var t = p.Length > 2 ? p[2] : ".";
        if (t is "." or "-A") { _staged.AddRange(_modified); _modified.Clear(); return [("Cambios preparados.", "success")]; }
        if (_modified.Remove(t)) { _staged.Add(t); return [($"'{t}' en staging.", "success")]; }
        return [($"fatal: '{t}' no reconocido.", "error")];
    }

    List<(string, string)> DoCommit(string[] p)
    {
        if (_staged.Count == 0) return [("nada que commit — usa 'git add'", "error")];
        var mIdx = Array.IndexOf(p, "-m");
        var msg  = mIdx >= 0 ? string.Join(" ", p[(mIdx + 1)..]).Trim('"') : "WIP";
        var hash = RndHash();
        _log.Insert(0, new(hash, "Tú", DateTime.Now.ToString("yyyy-MM-dd"), msg));
        var n = _staged.Count; _staged.Clear();
        return [($"[{_branch} {hash}] {msg}", "success"),
                ($" {n} archivo(s) cambiado(s)", "out")];
    }

    List<(string, string)> Push(string[] p) =>
    [
        ("Enumerando objetos, hecho.", "out"),
        ("Escribiendo objetos: 100%, hecho.", "out"),
        ("To https://github.com/bvn/trading-system", "success"),
        ($"   a3f8d21..{_log[0].Hash}  {_branch} → origin/{_branch}", "success"),
    ];

    List<(string, string)> Pull() => _rng.NextDouble() > 0.5
        ? [("Desde origin", "out"), ($"Fast-forward: {_rng.Next(1, 5)} archivos", "success")]
        : [("Ya está actualizado.", "success")];

    List<(string, string)> Merge(string[] p)
    {
        var t = p.Length > 2 ? p[2] : "";
        if (!_branches.Contains(t)) return [($"merge: {t} — no se puede fusionar", "error")];
        return [($"Actualizando a3f8d21..{RndHash()}", "success"), ("Merge realizado con estrategia 'ort'.", "success")];
    }

    List<(string, string)> Diff() =>
    [
        ("diff --git a/src/market/PriceEngine.cs b/src/market/PriceEngine.cs", "warn"),
        ("-    var price = lastTrade.Price * (1 + drift);", "error"),
        ("+    var price = lastTrade.Price * (1 + drift * volatilityFactor);", "success"),
    ];

    List<(string, string)> Stash(string[] p) => p.Length > 2 && p[2] == "pop"
        ? [("Cambios restaurados del stash.", "success")]
        : [($"Guardado en stash WIP on {_branch}.", "success")];

    List<(string, string)> Tag(string[] p) => p.Length == 2
        ? [("v1.0.0", "out"), ("v1.1.0", "out"), ("v2.0.0", "out")]
        : [($"Tag '{p[2]}' creado.", "success")];

    List<(string, string)> Help() =>
    [
        ("═══════════════════════════════════════════════", "comment"),
        ("  Terminal Git BVN — Comandos disponibles", "warn"),
        ("═══════════════════════════════════════════════", "comment"),
        ("", "out"),
        ("  git status                   Ver archivos modificados / staged", "out"),
        ("  git log / git log --oneline  Ver historial de commits", "out"),
        ("  git branch                   Listar ramas", "out"),
        ("  git checkout -b feature/X    Crear y cambiarse a rama", "out"),
        ("  git add .                    Preparar todos los cambios", "out"),
        ("  git commit -m \"mensaje\"    Confirmar cambios", "out"),
        ("  git push origin main         Subir cambios al remoto", "out"),
        ("  git pull origin develop      Descargar cambios del equipo", "out"),
        ("  git merge feature/X          Fusionar rama", "out"),
        ("  git diff                     Ver diferencias", "out"),
        ("  git stash / stash pop        Guardar/restaurar cambios", "out"),
        ("  git tag / git tag v2.0.0     Listar/crear tags", "out"),
    ];
}
