namespace TradingInventory;
class ReportsControl : UserControl {
    static readonly Dictionary<string, string> R = new() {
        ["Stock Report"] = "SELECT m.Name Material,u.UnitName Unit,m.Stock,m.MinStock,m.Stock*m.PurchaseRate StockValue,CASE WHEN m.Stock<=m.MinStock THEN 'LOW' ELSE 'OK' END Status FROM Materials m JOIN Units u ON u.UnitId=m.UnitId ORDER BY m.Name",
        ["Material Ledger"] = "SELECT Dt [Date],Material,[Type],QtyIn,QtyOut FROM (SELECT p.PurchaseDate Dt,m.Name Material,'Purchase' [Type],p.Qty QtyIn,0 QtyOut FROM Purchases p JOIN Materials m ON m.MaterialId=p.MaterialId UNION ALL SELECT s.SaleDate,m.Name,'Supply',0,s.Qty FROM Sales s JOIN Materials m ON m.MaterialId=s.MaterialId) x WHERE Dt BETWEEN @f AND @t ORDER BY Material,Dt",
        ["Purchase Register"] = "SELECT p.PurchaseDate [Date],s.Name Supplier,m.Name Material,p.Qty,p.Rate,p.Amount FROM Purchases p JOIN Suppliers s ON s.SupplierId=p.SupplierId JOIN Materials m ON m.MaterialId=p.MaterialId WHERE p.PurchaseDate BETWEEN @f AND @t ORDER BY p.PurchaseDate",
        ["Sales Register"] = "SELECT s.InvoiceNo Invoice,s.SaleDate [Date],c.Name Customer,m.Name Material,s.Qty,s.Rate,s.Amount FROM Sales s JOIN Customers c ON c.CustomerId=s.CustomerId JOIN Materials m ON m.MaterialId=s.MaterialId WHERE s.SaleDate BETWEEN @f AND @t ORDER BY s.SaleDate",
        ["Customer Ledger"] = "SELECT Dt [Date],Customer,Particulars,Debit,Credit FROM (SELECT s.SaleDate Dt,c.Name Customer,s.InvoiceNo Particulars,s.Amount Debit,0 Credit FROM Sales s JOIN Customers c ON c.CustomerId=s.CustomerId UNION ALL SELECT r.PayDate,c.Name,'Receipt '+r.Mode,0,r.Amount FROM Receipts r JOIN Customers c ON c.CustomerId=r.CustomerId) x WHERE Dt BETWEEN @f AND @t ORDER BY Customer,Dt",
        ["Customer Outstanding"] = "SELECT c.Name Customer,ISNULL(s.A,0) Supplied,ISNULL(r.A,0) Received,ISNULL(s.A,0)-ISNULL(r.A,0) Pending FROM Customers c OUTER APPLY(SELECT SUM(Amount) A FROM Sales WHERE CustomerId=c.CustomerId) s OUTER APPLY(SELECT SUM(Amount) A FROM Receipts WHERE CustomerId=c.CustomerId) r ORDER BY Pending DESC",
        ["Supplier Outstanding"] = "SELECT c.Name Supplier,ISNULL(s.A,0) Purchased,ISNULL(r.A,0) Paid,ISNULL(s.A,0)-ISNULL(r.A,0) Payable FROM Suppliers c OUTER APPLY(SELECT SUM(Amount) A FROM Purchases WHERE SupplierId=c.SupplierId) s OUTER APPLY(SELECT SUM(Amount) A FROM Payments WHERE SupplierId=c.SupplierId) r ORDER BY Payable DESC",
        ["Profit Report (avg purchase cost)"] = "SELECT s.InvoiceNo Invoice,s.SaleDate [Date],m.Name Material,s.Amount Sales,s.Qty*ISNULL(a.R,m.PurchaseRate) Cost,s.Amount-s.Qty*ISNULL(a.R,m.PurchaseRate) Profit FROM Sales s JOIN Materials m ON m.MaterialId=s.MaterialId OUTER APPLY(SELECT SUM(Amount)/NULLIF(SUM(Qty),0) R FROM Purchases WHERE MaterialId=s.MaterialId) a WHERE s.SaleDate BETWEEN @f AND @t ORDER BY s.SaleDate"
    };
    readonly ComboBox rep = new() { DropDownStyle = ComboBoxStyle.DropDownList }; readonly DateTimePicker f = new() { Format = DateTimePickerFormat.Short }, t = new() { Format = DateTimePickerFormat.Short }; readonly DataGridView g = Ui.Grid();
    public ReportsControl() {
        BackColor = Ui.Bg; rep.Items.AddRange(R.Keys.ToArray()); rep.SelectedIndex = 0; f.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var bar = Ui.Bar(); bar.Controls.AddRange(new Control[] { Ui.Field("Report", rep, 260), Ui.Field("From", f, 120), Ui.Field("To", t, 120), Ui.Btn("Run", Run), Ui.Btn("Export Excel", (s, e) => Ui.Csv(g), Color.Gray) });
        Controls.Add(g); Controls.Add(bar); Run(null, EventArgs.Empty);
    }
    void Run(object? s, EventArgs e) { try { g.DataSource = Db.Table(R[rep.Text], "@f", f.Value.Date, "@t", t.Value.Date); } catch (Exception x) { Ui.Err(x); } }
}
