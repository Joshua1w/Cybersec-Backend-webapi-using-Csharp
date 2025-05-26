using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlogBackend.Services
{
    public class AuditLogger
    {
        private readonly string _logFilePath;
        private readonly object _lock = new object(); // for thread safety

        public AuditLogger()
        {
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audit.log");
        }

        public async Task LogAsync(string action, string performedBy, string targetUser, string details = null)
        {
            string previousHash = await GetLastHashAsync();
            string logEntry = $"{DateTime.UtcNow:O} | Action: {action} | By: {performedBy} | Target: {targetUser} | Details: {details ?? "-"}";
            string entryToHash = previousHash + logEntry;
            string hash = ComputeSha256Hash(entryToHash);
            string chainedEntry = $"{logEntry} | PrevHash: {previousHash} | Hash: {hash}";

            // Strict append-only: never overwrite/truncate
            lock (_lock)
            {
                using (var stream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(chainedEntry);
                }
            }
        }

        private async Task<string> GetLastHashAsync()
        {
            if (!File.Exists(_logFilePath))
                return "GENESIS";

            string lastLine = null;
            // Read last line efficiently
            using (var stream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    lastLine = await reader.ReadLineAsync();
                }
            }
            if (lastLine == null)
                return "GENESIS";
            var lastHash = ExtractHash(lastLine);
            return string.IsNullOrEmpty(lastHash) ? "GENESIS" : lastHash;
        }

        private string ExtractHash(string logLine)
        {
            var idx = logLine.LastIndexOf("| Hash: ");
            if (idx == -1) return null;
            return logLine.Substring(idx + 8).Trim();
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                var sb = new StringBuilder();
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
