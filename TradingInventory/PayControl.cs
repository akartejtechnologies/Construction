namespace TradingInventory;
// Customer Receipt (receipt=true) or Supplier Payment (receipt=false)
class PayControl : UserControl {
    readonly bool rec; readonly string tbl, pc, idc, ptbl; readonly ComboBox party, mode = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    readonly DateTimePicker dt = new() { Format = DateTimePickerFormat.Short }; readonly NumericUpDown amt = new() { DecimalPlaces = 2, Maximum = 99999999 }; readonly TextBox note = new() { MaxLength = 200 }; readonly DataGridView g = Ui.Grid();
    public PayControl(bool receipt) {
        rec = receipt; tbl = rec ? "Receipts" : "Payments"; pc = rec ? "CustomerId" : "SupplierId"; idc = rec ? "ReceiptId" : "PaymentId"; ptbl = rec ? "Customers" : "Suppliers"; BackColor = Ui.Bg;
        party = Ui.Combo($"SELECT {pc} Id,Name FROM {ptbl} ORDER BY Name"); mode.Items.AddRange(new object[] { "Cash", "Bank", "UPI", "Cheque" }); mode.SelectedIndex = 0;
        var bar = Ui.Bar();
        bar.Controls.AddRange(new Control[] { Ui.Field("Date", dt, 120), Ui.Field(rec ? "Customer" : "Supplier", party, 220), Ui.Field("Amount", amt, 120), Ui.Field("Mode", mode, 100), Ui.Field("Remarks", note, 240), Ui.Btn("Save", Save), Ui.Btn("Export Excel", (s, e) => Ui.Csv(g), Color.Gray) });
        Controls.Add(g); Controls.Add(bar); Load();
    }
    void Load() => g.DataSource = Db.Table($"SELECT TOP 200 x.{idc} ID,x.PayDate [Date],p.Name Party,x.Amount,x.Mode,x.Remarks FROM {tbl} x JOIN {ptbl} p ON p.{pc}=x.{pc} ORDER BY x.{idc} DESC");
    void Save(object? s, EventArgs e) {
        try {
            if (party.SelectedValue == null || amt.Value <= 0) throw new InvalidOperationException("Select a party and enter an amount greater than zero.");
            Db.Exec($"INSERT {tbl}(PayDate,{pc},Amount,Mode,Remarks,CreatedBy) VALUES(@d,@p,@a,@m,@n,@u)", "@d", dt.Value.Date, "@p", Convert.ToInt32(party.SelectedValue), "@a", amt.Value, "@m", mode.Text, "@n", note.Text.Trim(), "@u", Session.User);
            amt.Value = 0; note.Clear(); Load();
        } catch (Exception x) { Ui.Err(x); }
    }
}
