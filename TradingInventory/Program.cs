namespace TradingInventory;
static class Program {
    [STAThread]
    static void Main(string[] a) {
        // License generator (vendor use): TradingInventory.exe --genlicense <MachineId> <yyyy-MM-dd>
        if (a.Length == 3 && a[0] == "--genlicense") { File.WriteAllText("license.lic", Security.MakeLicense(a[1], a[2])); return; }
        ApplicationConfiguration.Initialize();
        var (ok, msg) =Security.CheckLicense();
        if (!ok) { Clipboard.SetText(Security.MachineId()); MessageBox.Show($"{msg}\n\nMachine ID (copied to clipboard):\n{Security.MachineId()}", "License", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        try { Security.EnsureAdmin(); } catch (Exception e) { MessageBox.Show("Cannot connect to database.\n" + e.Message); return; }
        using (var l = new LoginForm()) if (l.ShowDialog() != DialogResult.OK) return;
        Application.Run(new MainForm());
    }
}
