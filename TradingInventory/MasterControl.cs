namespace TradingInventory;
record Fld(string Col, string Label, char Kind = 't'); // t=text, n=number, u=unit dropdown
class MasterControl : UserControl {
    readonly string tbl, id; readonly Fld[] fs; readonly Control[] ins; readonly DataGridView g = Ui.Grid(); int cur;
    public MasterControl(string table, string idCol, params Fld[] fields) {
        tbl = table; id = idCol; fs = fields; BackColor = Ui.Bg;
        ins = fs.Select(Make).ToArray();
        var bar = Ui.Bar();
        for (int i = 0; i < fs.Length; i++) bar.Controls.Add(Ui.Field(fs[i].Label, ins[i], 200));
        var del = Ui.Btn("Delete", Del, Ui.Danger); del.Enabled = Session.IsAdmin;
        bar.Controls.AddRange(new Control[] { Ui.Btn("Save", Save), Ui.Btn("New", (s, e) => Clear(), Color.Gray), del });
        g.SelectionChanged += Pick; Controls.Add(g); Controls.Add(bar);
    }
    static Control Make(Fld f) {
        if (f.Kind == 'n') return new NumericUpDown { DecimalPlaces = 2, Maximum = 99999999 };
        if (f.Kind == 'u') return new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Name", ValueMember = "Id" };
        return new TextBox { MaxLength = 200 };
    }
    protected override void OnVisibleChanged(EventArgs e) { base.OnVisibleChanged(e); if (Visible) try { Reload(); } catch (Exception x) { Ui.Err(x); } }
    void Reload() {
        for (int i = 0; i < fs.Length; i++) if (fs[i].Kind == 'u') ((ComboBox)ins[i]).DataSource = Db.Table("SELECT UnitId Id,UnitName Name FROM Units ORDER BY UnitName");
        g.DataSource = Db.Table($"SELECT * FROM {tbl} ORDER BY {id} DESC");
    }
    void Clear() { cur = 0; foreach (var c in ins) { if (c is TextBox t) t.Clear(); else if (c is NumericUpDown n) n.Value = 0; } ins[0].Focus(); }
    void Pick(object? s, EventArgs e) {
        if (g.CurrentRow == null) return; cur = Convert.ToInt32(g.CurrentRow.Cells[0].Value);
        var r = Db.Table($"SELECT * FROM {tbl} WHERE {id}=@i", "@i", cur); if (r.Rows.Count == 0) return; var row = r.Rows[0];
        for (int i = 0; i < fs.Length; i++) {
            var v = row[fs[i].Col];
            if (fs[i].Kind == 'n') ((NumericUpDown)ins[i]).Value = Convert.ToDecimal(v); else if (fs[i].Kind == 'u') ((ComboBox)ins[i]).SelectedValue = v; else ins[i].Text = v.ToString();
        }
    }
    void Save(object? s, EventArgs e) {
        try {
            var p = new List<object>();
            for (int i = 0; i < fs.Length; i++) { object? v; var c = ins[i]; if (fs[i].Kind == 'n') v = ((NumericUpDown)c).Value; else if (fs[i].Kind == 'u') v = ((ComboBox)c).SelectedValue; else v = c.Text.Trim(); p.Add("@p" + i); p.Add(v!); }
            if (p[1] is string n && n.Length == 0) throw new InvalidOperationException("The first field is required.");
            if (cur == 0) Db.Exec($"INSERT INTO {tbl}({string.Join(",", fs.Select(f => f.Col))}) VALUES({string.Join(",", fs.Select((f, i) => "@p" + i))})", p.ToArray());
            else { p.Add("@id"); p.Add(cur); Db.Exec($"UPDATE {tbl} SET {string.Join(",", fs.Select((f, i) => $"{f.Col}=@p{i}"))} WHERE {id}=@id", p.ToArray()); }
            Clear(); Reload();
        } catch (Microsoft.Data.SqlClient.SqlException x) when (x.Number is 2601 or 2627) { Ui.Err(new InvalidOperationException("This name already exists.")); } catch (Exception x) { Ui.Err(x); }
    }
    void Del(object? s, EventArgs e) {
        if (cur == 0 || MessageBox.Show("Delete this record?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        try { Db.Exec($"DELETE FROM {tbl} WHERE {id}=@i", "@i", cur); Clear(); Reload(); }
        catch (Microsoft.Data.SqlClient.SqlException x) when (x.Number == 547) { Ui.Err(new InvalidOperationException("Record is in use and cannot be deleted.")); }
    }
}
