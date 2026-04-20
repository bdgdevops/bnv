// LoginForm.cs — Pantalla de inicio de sesión sencilla
using System.Drawing;
using BVN.WinForms.Controls;
using BVN.WinForms.Theme;

namespace BVN.WinForms;

public class LoginForm : Form
{
    public LoginForm()
    {
        // ── Configuración de la ventana ──────────────────────────
        Text = "BVN · Iniciar Sesión";
        Size = new Size(400, 520);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppTheme.BgBase;
        ForeColor = AppTheme.TextPrimary;
        Font = AppTheme.FontBase;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = true;

        Load += (_, _) =>
        {
            AppTheme.ApplyDarkTitleBar(this);
            AppTheme.RoundCorners(this);
        };

        // ── Card central ─────────────────────────────────────────
        var card = new DbPanel
        {
            Size = new Size(340, 420),
            Location = new Point((Width - 340) / 2 - 8, (Height - 420) / 2 - 20),
            BackColor = AppTheme.BgCard,
            Anchor = AnchorStyles.None,
        };
        Controls.Add(card);

        // Logo
        var logo = new Label
        {
            Text = "📈 BVN",
            ForeColor = AppTheme.AccentBlue,
            Font = new Font("Segoe UI", 24f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 30),
        };
        card.Controls.Add(logo);

        var subtitle = new Label
        {
            Text = "Bolsa de Valores Nacional",
            ForeColor = AppTheme.TextMuted,
            Font = AppTheme.FontSmall,
            AutoSize = true,
            Location = new Point(26, 75),
        };
        card.Controls.Add(subtitle);

        var instruction = new Label
        {
            Text = "Ingresa a la plataforma de simulación",
            ForeColor = AppTheme.TextSecondary,
            Font = AppTheme.FontBase,
            AutoSize = true,
            Location = new Point(24, 130),
        };
        card.Controls.Add(instruction);

        // ── Panel Usuario ────────────────────────────────────────
        var userLbl = new Label
        {
            Text = "USUARIO",
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(24, 180),
        };
        card.Controls.Add(userLbl);

        var userBg = new DbPanel
        {
            Size = new Size(290, 42),
            Location = new Point(26, 202),
            BackColor = AppTheme.BgInput,
        };
        card.Controls.Add(userBg);

        var userTxt = new TextBox
        {
            Text = "admin",
            Location = new Point(12, 11),
            Width = 266,
            BackColor = AppTheme.BgInput,
            ForeColor = AppTheme.TextPrimary,
            BorderStyle = BorderStyle.None,
            Font = AppTheme.FontBase,
        };
        userBg.Controls.Add(userTxt);

        // ── Panel Contraseña ─────────────────────────────────────
        var passLbl = new Label
        {
            Text = "CONTRASEÑA",
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(24, 260),
        };
        card.Controls.Add(passLbl);

        var passBg = new DbPanel
        {
            Size = new Size(290, 42),
            Location = new Point(26, 282),
            BackColor = AppTheme.BgInput,
        };
        card.Controls.Add(passBg);

        var passTxt = new TextBox
        {
            Text = "••••••••",
            UseSystemPasswordChar = true,
            Location = new Point(12, 11),
            Width = 266,
            BackColor = AppTheme.BgInput,
            ForeColor = AppTheme.TextPrimary,
            BorderStyle = BorderStyle.None,
            Font = AppTheme.FontBase,
        };
        passBg.Controls.Add(passTxt);

        // ── Botón Login ──────────────────────────────────────────
        var btnLogin = new Button
        {
            Text = "Iniciar Sesión",
            Size = new Size(290, 44),
            Location = new Point(26, 350),
            BackColor = AppTheme.AccentBlue,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor = Cursors.Hand,
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Click += (_, _) =>
        {
            // Simple: No valída nada y da éxito
            DialogResult = DialogResult.OK;
            Close();
        };
        card.Controls.Add(btnLogin);

        // Enter submits the form
        AcceptButton = btnLogin;

    }
}
