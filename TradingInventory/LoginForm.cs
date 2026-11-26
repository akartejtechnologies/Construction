namespace TradingInventory;
class LoginForm : Form {
    readonly TextBox u = new() { PlaceholderText = "Username", Font = Ui.F }, p = new() { PlaceholderText = "Password", UseSystemPasswordChar = true, Font = Ui.F };
    readonly Label msg = new() { ForeColor = Ui.Danger, AutoSize = true, Font = Ui.F };
    int fails; DateTime lockUntil;
    public LoginForm() {
        Text = "Sign in"; Size = new Size(380, 400); StartPosition = FormStartPosition.CenterScreen; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; BackColor = Color.White;
        var f = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(40, 30, 40, 0) };
        f.Controls.Add(new Label { Text = "Trading & Inventory", Font = new Font("Segoe UI Semibold", 17f), ForeColor = Ui.Navy, AutoSize = true });
        f.Controls.Add(new Label { Text = "Sign in to continue", ForeColor = Color.Gray, AutoSize = true, Margin = new Padding(3, 0, 0, 24), Font = Ui.F });
        u.Width = p.Width = 290; u.Margin = new Padding(3, 3, 3, 14);
        var b = Ui.Btn("Sign in", (s, e) => Go()); b.Width = 290; b.Margin = new Padding(3, 10, 3, 10);
        f.Controls.AddRange(new Control[] { u, p, b, msg }); Controls.Add(f); AcceptButton = b;
    }
    void Go() {
        try {
            if (DateTime.Now < lockUntil) { msg.Text = "Too many attempts. Try again later."; return; }
            var t = Db.Table("SELECT Username,PasswordHash,Salt,Role FROM Users WHERE Username=@u AND IsActive=1", "@u", u.Text.Trim());
            if (t.Rows.Count == 1 && Security.Verify(p.Text, (string)t.Rows[0]["Salt"], (string)t.Rows[0]["PasswordHash"])) {
                Session.User = (string)t.Rows[0]["Username"]; Session.Role = (string)t.Rows[0]["Role"]; DialogResult = DialogResult.OK; return;
            }
            if (++fails >= 5) { lockUntil = DateTime.Now.AddMinutes(2); fails = 0; }
            msg.Text = "Invalid username or password."; p.Clear();
        } catch (Exception e) { Ui.Err(e); }
    }
}
