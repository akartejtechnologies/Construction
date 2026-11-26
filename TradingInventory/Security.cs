using System.Security.Cryptography;
using System.Text;
namespace TradingInventory;
static class Session { public static string User = "", Role = ""; public static bool IsAdmin => Role == "Admin"; }
static class Security {
    // CHANGE this secret before shipping and keep the generator private (consider RSA signing + obfuscation for production).
    static readonly byte[] LicKey = Encoding.UTF8.GetBytes("CHANGE-ME-vendor-secret-2026-!x9Qz");
    public static string Hash(string pwd, byte[] salt) => Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(pwd, salt, 150_000, HashAlgorithmName.SHA256, 32));
    public static bool Verify(string pwd, string salt, string hash) =>
        CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(Hash(pwd, Convert.FromBase64String(salt))), Convert.FromBase64String(hash));
    public static void EnsureAdmin() {
        if ((int)Db.Scalar("SELECT COUNT(*) FROM Users")! > 0) return;
        var s = RandomNumberGenerator.GetBytes(16);
        Db.Exec("INSERT Users(Username,PasswordHash,Salt,Role) VALUES('admin',@h,@s,'Admin')", "@h", Hash("Admin@123", s), "@s", Convert.ToBase64String(s));
    }
    public static string MachineId() {
        var g = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography", "MachineGuid", "")?.ToString() ?? Environment.MachineName;
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(g)))[..16];
    }
    static string Mac(string data) => Convert.ToHexString(HMACSHA256.HashData(LicKey, Encoding.UTF8.GetBytes(data)));
    public static string MakeLicense(string machine, string expiry) => $"{machine}|{expiry}|{Mac(machine + "|" + expiry)}";
    public static (bool, string) CheckLicense() {
        //var f = Path.Combine(AppContext.BaseDirectory, "license.lic");
        //if (!File.Exists(f)) return (false, "License file (license.lic) not found.");
        //var p = File.ReadAllText(f).Trim().Split('|');
        //if (p.Length != 3) return (false, "Invalid license file.");
        //if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(Mac(p[0] + "|" + p[1])), Encoding.UTF8.GetBytes(p[2].ToUpperInvariant()))) return (false, "License signature is invalid.");
        //if (!p[0].Equals(MachineId(), StringComparison.OrdinalIgnoreCase)) return (false, "License is for a different computer.");
        //if (!DateOnly.TryParse(p[1], out var exp) || exp < DateOnly.FromDateTime(DateTime.Today)) return (false, "License has expired.");
        return (true, "");
    }
}
