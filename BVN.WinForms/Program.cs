// Program.cs
using BVN.WinForms;
using Velopack;

VelopackApp.Build().Run();

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