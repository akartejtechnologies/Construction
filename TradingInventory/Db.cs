using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
namespace TradingInventory;
// All SQL is parameterised: pass parameters as name,value pairs.
static class Db {
    public static readonly string Cs = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json"))).RootElement.GetProperty("ConnectionString").GetString()!;
    public static SqlCommand Cmd(SqlConnection c, SqlTransaction? t, string sql, object[] p) {
        var m = new SqlCommand(sql, c, t);
        for (int i = 0; i + 1 < p.Length; i += 2) m.Parameters.AddWithValue((string)p[i], p[i + 1] ?? DBNull.Value);
        return m;
    }
    public static DataTable Table(string sql, params object[] p) {
        using var c = new SqlConnection(Cs); using var da = new SqlDataAdapter(Cmd(c, null, sql, p)); var d = new DataTable(); da.Fill(d); return d;
    }
    public static int Exec(string sql, params object[] p) { using var c = new SqlConnection(Cs); c.Open(); return Cmd(c, null, sql, p).ExecuteNonQuery(); }
    public static object? Scalar(string sql, params object[] p) { using var c = new SqlConnection(Cs); c.Open(); return Cmd(c, null, sql, p).ExecuteScalar(); }
    public static int Run(SqlConnection c, SqlTransaction t, string sql, params object[] p) => Cmd(c, t, sql, p).ExecuteNonQuery();
    public static void Tx(Action<SqlConnection, SqlTransaction> a) {
        using var c = new SqlConnection(Cs); c.Open(); using var t = c.BeginTransaction();
        try { a(c, t); t.Commit(); } catch { t.Rollback(); throw; }
    }
    public static string DbName => new SqlConnectionStringBuilder(Cs).InitialCatalog;
}
