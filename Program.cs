// Program.cs
using BVN.WinForms;

Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);

using (var login = new LoginForm())
{
    if (login.ShowDialog() == DialogResult.OK)
    {
        Application.Run(new MainForm());
    }
}