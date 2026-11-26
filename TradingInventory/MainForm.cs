using Microsoft.Data.SqlClient;
namespace TradingInventory;
class MainForm : Form {
    readonly Panel host = new() { Dock = DockStyle.Fill };
    public MainForm() {
        Text = "Trading & Inventory Manager"; WindowState = FormWindowState.Maximized; MinimumSize = new Size(1100, 680); BackColor = Ui.Bg; Font = Ui.F;
        var side = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 260, BackColor = Ui.Navy, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        side.Controls.Add(new Label { Text = "  Software main menu", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 13f), Height = 70, Width = 260, TextAlign = ContentAlignment.MiddleLeft });
        Item(side, "Dashboard", () => new DashboardControl());
        Item(side, "Masters", Masters);
        Item(side, "Purchase Entry", () => new EntryControl(false));
        Item(side, "Supply / Sales Entry", () => new EntryControl(true));
        Item(side, "Receipt & Payment", () => { var t = new TabControl { Font = Ui.F }; foreach (var r in new[] { true, false }) { var pg = new TabPage(r ? "Customer Receipt" : "Supplier Payment"); var c = new PayControl(r) { Dock = DockStyle.Fill }; pg.Controls.Add(c); t.TabPages.Add(pg); } return t; });
        Item(side, "Reports", () => new ReportsControl());
        Item(side, "Backup database", Backup, Ui.Navy, Session.IsAdmin);
        Item(side, $"Logout ({Session.User})", () => { Application.Restart(); Environment.Exit(0); return null!; }, Ui.Navy);
        Controls.Add(host); Controls.Add(side); Show(() => new DashboardControl());
    }
    void Item(FlowLayoutPanel side, string text, Func<Control> make, Color? c = null, bool enabled = true) {
        var b = new Button { Text = "   " + text, Width = 260, Height = 52, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = c ?? Ui.Navy, TextAlign = ContentAlignment.MiddleLeft, Font = Ui.F, Margin = Padding.Empty, Enabled = enabled, Cursor = Cursors.Hand };
        b.FlatAppearance.BorderSize = 0; b.FlatAppearance.MouseOverBackColor = Ui.Accent; b.Click += (s, e) => { try { Show(make); } catch (Exception x) { Ui.Err(x); } }; side.Controls.Add(b);
    }
    void Show(Func<Control> make) {
        var c = make(); if (c == null) return; foreach (Control o in host.Controls) o.Dispose(); host.Controls.Clear(); c.Dock = DockStyle.Fill; host.Controls.Add(c);
    }
    static Control Masters() {
        var t = new TabControl { Font = Ui.F };
        void Add(string n, Control c) { var p = new TabPage(n); c.Dock = DockStyle.Fill; p.Controls.Add(c); t.TabPages.Add(p); }
        Add("Unit Master", new MasterControl("Units", "UnitId", new Fld("UnitName", "Unit name")));
        Add("Material Master", new MasterControl("Materials", "MaterialId", new Fld("Name", "Material name"), new Fld("UnitId", "Unit", 'u'), new Fld("PurchaseRate", "Purchase rate", 'n'), new Fld("SaleRate", "Sale rate", 'n'), new Fld("MinStock", "Low-stock level", 'n')));
        Add("Supplier Master", new MasterControl("Suppliers", "SupplierId", new Fld("Name", "Supplier name"), new Fld("Phone", "Phone"), new Fld("Address", "Address")));
        Add("Customer Master", new MasterControl("Customers", "CustomerId", new Fld("Name", "Customer name"), new Fld("Phone", "Phone"), new Fld("Address", "Address")));
        return t;
    }
    Control Backup() {
        using var d = new SaveFileDialog { Filter = "SQL backup|*.bak", FileName = $"{Db.DbName}_{DateTime.Now:yyyyMMdd_HHmm}.bak", Title = "Backup path (on the SQL Server machine)" };
        if (d.ShowDialog() != DialogResult.OK) return null!;
        using var c = new SqlConnection(Db.Cs); c.Open();
        using var m = new SqlCommand($"BACKUP DATABASE [{Db.DbName.Replace("]", "]]")}] TO DISK=@p WITH INIT, COMPRESSION", c) { CommandTimeout = 600 };
        m.Parameters.AddWithValue("@p", d.FileName); m.ExecuteNonQuery(); MessageBox.Show("Backup completed."); return null!;
    }
}
