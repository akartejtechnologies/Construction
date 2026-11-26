namespace TradingInventory;
class DashboardControl : UserControl {
    public DashboardControl() {
        BackColor = Ui.Bg;
        var r = Db.Table(@"SELECT (SELECT ISNULL(SUM(Stock*PurchaseRate),0) FROM Materials) V,
            (SELECT ISNULL(SUM(Amount),0) FROM Purchases WHERE PurchaseDate=CAST(GETDATE() AS DATE)) P,
            (SELECT ISNULL(SUM(Amount),0) FROM Sales WHERE SaleDate=CAST(GETDATE() AS DATE)) S,
            (SELECT ISNULL((SELECT SUM(Amount) FROM Sales),0)-ISNULL((SELECT SUM(Amount) FROM Receipts),0)) O,
            (SELECT COUNT(*) FROM Materials WHERE Stock<=MinStock) L").Rows[0];
        var cards = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(16) };
        void Card(string title, string val, Color c) {
            var p = new Panel { Width = 230, Height = 100, BackColor = Color.White, Margin = new Padding(0, 0, 16, 0) };
            p.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 5, BackColor = c });
            p.Controls.Add(new Label { Text = val, Font = new Font("Segoe UI Semibold", 18f), Location = new Point(20, 44), AutoSize = true });
            p.Controls.Add(new Label { Text = title, ForeColor = Color.Gray, Font = Ui.F, Location = new Point(20, 14), AutoSize = true }); cards.Controls.Add(p);
        }
        Card("Total stock value", Convert.ToDecimal(r["V"]).ToString("N2"), Ui.Accent); Card("Today's purchases", Convert.ToDecimal(r["P"]).ToString("N2"), Color.Teal);
        Card("Today's sales", Convert.ToDecimal(r["S"]).ToString("N2"), Color.SeaGreen); Card("Pending customer amount", Convert.ToDecimal(r["O"]).ToString("N2"), Color.DarkOrange); Card("Low-stock alerts", r["L"].ToString()!, Ui.Danger);
        var g = Ui.Grid(); g.DataSource = Db.Table("SELECT Name [Low-stock material],Stock,MinStock FROM Materials WHERE Stock<=MinStock ORDER BY Name");
        Controls.Add(g); Controls.Add(cards);
    }
}
