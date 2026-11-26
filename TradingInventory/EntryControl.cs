using System.Data;
namespace TradingInventory;
// Purchase Entry (stock in) when sale=false; Supply/Sales Entry (stock out + invoice) when sale=true.
class EntryControl : UserControl {
    readonly bool sale; readonly ComboBox party, mat; readonly DateTimePicker dt = new() { Format = DateTimePickerFormat.Short };
    readonly NumericUpDown qty = Num(3), rate = Num(2); readonly TextBox note = new() { MaxLength = 200 };
    readonly Label amt = new() { Text = "0.00", Font = new Font("Segoe UI Semibold", 11f), Height = 26 }, stk = new() { Text = "-", Font = Ui.F, Height = 26 };
    readonly DataGridView g = Ui.Grid();
    const string MatSql = "SELECT MaterialId Id,Name,PurchaseRate,SaleRate,Stock FROM Materials ORDER BY Name";
    static NumericUpDown Num(int d) => new() { DecimalPlaces = d, Maximum = 99999999 };
    public EntryControl(bool isSale) {
        sale = isSale; BackColor = Ui.Bg;
        party = Ui.Combo(sale ? "SELECT CustomerId Id,Name FROM Customers ORDER BY Name" : "SELECT SupplierId Id,Name FROM Suppliers ORDER BY Name");
        mat = Ui.Combo(MatSql);
        var bar = Ui.Bar();
        bar.Controls.AddRange(new Control[] { Ui.Field("Date", dt, 120), Ui.Field(sale ? "Customer" : "Supplier", party, 200), Ui.Field("Material", mat, 200), Ui.Field("Available stock", stk, 110),
            Ui.Field("Quantity", qty, 100), Ui.Field("Rate", rate, 100), Ui.Field("Amount", amt, 110), Ui.Field(sale ? "Delivery details" : "Bill / remarks", note, 240), Ui.Btn("Save", Save) });
        if (sale) bar.Controls.Add(Ui.Btn("Print invoice", Print, Color.FromArgb(22, 101, 52)));
        bar.Controls.Add(Ui.Btn("Export Excel", (s, e) => Ui.Csv(g), Color.Gray));
        mat.SelectedIndexChanged += (s, e) => { if (mat.SelectedItem is DataRowView r) { rate.Value = Convert.ToDecimal(r[sale ? "SaleRate" : "PurchaseRate"]); stk.Text = Convert.ToDecimal(r["Stock"]).ToString("N3"); } };
        qty.ValueChanged += (s, e) => amt.Text = (qty.Value * rate.Value).ToString("N2"); rate.ValueChanged += (s, e) => amt.Text = (qty.Value * rate.Value).ToString("N2");
        Controls.Add(g); Controls.Add(bar); Load();
    }
    void Load() {
        g.DataSource = Db.Table(sale
            ? "SELECT TOP 200 s.SaleId ID,s.InvoiceNo Invoice,s.SaleDate [Date],c.Name Customer,m.Name Material,s.Qty,s.Rate,s.Amount,s.Delivery FROM Sales s JOIN Customers c ON c.CustomerId=s.CustomerId JOIN Materials m ON m.MaterialId=s.MaterialId ORDER BY s.SaleId DESC"
            : "SELECT TOP 200 p.PurchaseId ID,p.PurchaseDate [Date],s.Name Supplier,m.Name Material,p.Qty,p.Rate,p.Amount,p.Remarks FROM Purchases p JOIN Suppliers s ON s.SupplierId=p.SupplierId JOIN Materials m ON m.MaterialId=p.MaterialId ORDER BY p.PurchaseId DESC");
        var i = mat.SelectedIndex; mat.DataSource = Db.Table(MatSql); if (i >= 0 && i < mat.Items.Count) mat.SelectedIndex = i;
    }
    void Save(object? s, EventArgs e) {
        try {
            if (party.SelectedValue == null || mat.SelectedValue == null || qty.Value <= 0 || rate.Value <= 0) throw new InvalidOperationException("Select party and material; quantity and rate must be greater than zero.");
            int p = Convert.ToInt32(party.SelectedValue), m = Convert.ToInt32(mat.SelectedValue); decimal q = qty.Value, r = rate.Value, a = q * r; var d = dt.Value.Date; var n = note.Text.Trim();
            Db.Tx((c, t) => {
                if (sale) {
                    if (Db.Run(c, t, "UPDATE Materials SET Stock=Stock-@q WHERE MaterialId=@m AND Stock>=@q", "@q", q, "@m", m) == 0) throw new InvalidOperationException("Insufficient stock.");
                    Db.Run(c, t, "INSERT Sales(SaleDate,CustomerId,MaterialId,Qty,Rate,Amount,Delivery,CreatedBy) VALUES(@d,@p,@m,@q,@r,@a,@n,@u)", "@d", d, "@p", p, "@m", m, "@q", q, "@r", r, "@a", a, "@n", n, "@u", Session.User);
                } else {
                    Db.Run(c, t, "UPDATE Materials SET Stock=Stock+@q,PurchaseRate=@r WHERE MaterialId=@m", "@q", q, "@r", r, "@m", m);
                    Db.Run(c, t, "INSERT Purchases(PurchaseDate,SupplierId,MaterialId,Qty,Rate,Amount,Remarks,CreatedBy) VALUES(@d,@p,@m,@q,@r,@a,@n,@u)", "@d", d, "@p", p, "@m", m, "@q", q, "@r", r, "@a", a, "@n", n, "@u", Session.User);
                }
            });
            qty.Value = 0; note.Clear(); Load();
        } catch (Exception x) { Ui.Err(x); }
    }
    void Print(object? s, EventArgs e) {
        try {
            if (g.CurrentRow == null) return;
            var r = Db.Table("SELECT s.InvoiceNo,s.SaleDate,c.Name Customer,c.Address,m.Name Material,s.Qty,s.Rate,s.Amount,s.Delivery FROM Sales s JOIN Customers c ON c.CustomerId=s.CustomerId JOIN Materials m ON m.MaterialId=s.MaterialId WHERE s.SaleId=@i", "@i", Convert.ToInt32(g.CurrentRow.Cells[0].Value)).Rows[0];
            var pd = new System.Drawing.Printing.PrintDocument();
            pd.PrintPage += (o, a) => {
                var gx = a.Graphics!; float y = 60; using var h = new Font("Segoe UI", 20, FontStyle.Bold); using var n = new Font("Segoe UI", 12);
                gx.DrawString("INVOICE", h, Brushes.Black, 60, y); y += 60;
                foreach (var l in new[] { $"Invoice No : {r["InvoiceNo"]}", $"Date : {Convert.ToDateTime(r["SaleDate"]):dd-MMM-yyyy}", $"Customer : {r["Customer"]}", $"Address : {r["Address"]}", "", $"Material : {r["Material"]}", $"Quantity : {r["Qty"]}", $"Rate : {r["Rate"]:N2}", $"Delivery : {r["Delivery"]}", "", $"Invoice amount : {Convert.ToDecimal(r["Amount"]):N2}" }) { gx.DrawString(l, n, Brushes.Black, 60, y); y += 30; }
            };
            using var pv = new PrintPreviewDialog { Document = pd, WindowState = FormWindowState.Maximized }; pv.ShowDialog();
        } catch (Exception x) { Ui.Err(x); }
    }
}
