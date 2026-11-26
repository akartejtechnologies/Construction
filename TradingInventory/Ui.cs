using System.Text;
namespace TradingInventory;
static class Ui {
    public static readonly Color Navy = Color.FromArgb(15, 23, 42), Accent = Color.FromArgb(37, 99, 235), Bg = Color.FromArgb(244, 246, 250), Danger = Color.FromArgb(220, 38, 38);
    public static readonly Font F = new("Segoe UI", 10f);
    public static Button Btn(string t, EventHandler h, Color? c = null) {
        var b = new Button { Text = t, AutoSize = true, MinimumSize = new Size(90, 34), FlatStyle = FlatStyle.Flat, BackColor = c ?? Accent, ForeColor = Color.White, Font = F, Margin = new Padding(0, 20, 8, 0), Cursor = Cursors.Hand };
        b.FlatAppearance.BorderSize = 0; b.Click += h; return b;
    }
    public static Control Field(string label, Control c, int w = 170) {
        c.Width = w; c.Font = F; c.Margin = Padding.Empty;
        var p = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false, Margin = new Padding(0, 0, 14, 8) };
        p.Controls.Add(new Label { Text = label, AutoSize = true, ForeColor = Color.FromArgb(75, 85, 99), Font = F });
        p.Controls.Add(c); return p;
    }
    public static FlowLayoutPanel Bar() => new() { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(16, 12, 16, 4), BackColor = Color.White };
    public static DataGridView Grid() {
        var g = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, EnableHeadersVisualStyles = false, Font = F, RowTemplate = { Height = 32 }, ColumnHeadersHeight = 38 };
        g.ColumnHeadersDefaultCellStyle.BackColor = Navy; g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252); g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254); g.DefaultCellStyle.SelectionForeColor = Color.Black;
        return g;
    }
    public static ComboBox Combo(string sql) { var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Name", ValueMember = "Id" }; cb.DataSource = Db.Table(sql); return cb; }
    public static void Err(Exception e) {
        try { File.AppendAllText("error.log", $"{DateTime.Now:s} {e}\n"); } catch { }
        MessageBox.Show(e is InvalidOperationException ? e.Message : "Operation failed. Details were written to error.log.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    public static void Csv(DataGridView g) {
        using var d = new SaveFileDialog { Filter = "Excel CSV|*.csv", FileName = "export.csv" }; if (d.ShowDialog() != DialogResult.OK) return;
        var sb = new StringBuilder(); sb.AppendLine(string.Join(",", g.Columns.Cast<DataGridViewColumn>().Select(c => Q(c.HeaderText))));
        foreach (DataGridViewRow r in g.Rows) sb.AppendLine(string.Join(",", r.Cells.Cast<DataGridViewCell>().Select(c => Q(c.Value?.ToString()))));
        File.WriteAllText(d.FileName, sb.ToString(), new UTF8Encoding(true));
    }
    static string Q(string? s) { s ??= ""; if (s.Length > 0 && "=+-@".Contains(s[0]) && !decimal.TryParse(s, out _)) s = "'" + s; return "\"" + s.Replace("\"", "\"\"") + "\""; } // blocks CSV formula injection
}
