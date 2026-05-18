using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace InvoicesManager
{
    //CRUD Operation => Create, Read
    public static class DatabaseHelper
    {
        private const string DatabaseFileName = "invoices.db";
        private static readonly string databasePath = ResolveDatabasePath();
        private static readonly string connectionString = GetConnectionString();

        private static string GetConnectionString()
        {
            return $"Data Source={databasePath};Version=3;";
        }

        public static string GetConnectionStringPublic()
        {
            return connectionString;
        }

        public static string GetDatabasePath()
        {
            return databasePath;
        }

        public static void InitializeDatabase()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS Invoices (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                ClientName TEXT,
                                InvoiceNumber TEXT,
                                Date TEXT,
                                Amount REAL,
                                Details TEXT,
                                BarcodeValue TEXT
                            )";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                EnsureBarcodeColumn(conn);
                BackfillMissingBarcodes(conn);
            }
        }

        public static void AddInvoice(Invoice invoice)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string barcodeValue = string.IsNullOrWhiteSpace(invoice.BarcodeValue)
                    ? InvoiceEnhancementService.GenerateRandomBarcodeValue()
                    : invoice.BarcodeValue;
                invoice.BarcodeValue = barcodeValue;

                string sql = "INSERT INTO Invoices (ClientName, InvoiceNumber, Date, Amount, Details, BarcodeValue) VALUES (@ClientName, @InvoiceNumber, @Date, @Amount, @Details, @BarcodeValue)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientName", invoice.ClientName);
                    cmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
                    cmd.Parameters.AddWithValue("@Date", invoice.Date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@Amount", invoice.Amount);
                    cmd.Parameters.AddWithValue("@Details", invoice.Details);
                    cmd.Parameters.AddWithValue("@BarcodeValue", barcodeValue);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<Invoice> GetAllInvoices()
        {
            var invoices = new List<Invoice>();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Invoices ORDER BY Id";
                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        invoices.Add(
                            new Invoice(
                                Convert.ToInt32(reader["Id"]),
                                reader["ClientName"].ToString(),
                                reader["InvoiceNumber"].ToString(),
                                DateTime.Parse(reader["Date"].ToString()),
                                Convert.ToDecimal(reader["Amount"]),
                                reader["Details"].ToString(),
                                ReaderHasColumn(reader, "BarcodeValue") ? Convert.ToString(reader["BarcodeValue"]) : null
                        ));
                    }
                }
            }
            return invoices;
        }

        public static void NormalizeInvoiceIds()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    var currentIds = new List<int>();
                    const string selectSql = "SELECT Id FROM Invoices ORDER BY Id";
                    using (var selectCmd = new SQLiteCommand(selectSql, conn, transaction))
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            currentIds.Add(Convert.ToInt32(reader["Id"]));
                        }
                    }

                    for (int i = 0; i < currentIds.Count; i++)
                    {
                        int oldId = currentIds[i];
                        int temporaryId = -(i + 1);

                        using (var tempUpdateCmd = new SQLiteCommand("UPDATE Invoices SET Id = @TempId WHERE Id = @OldId", conn, transaction))
                        {
                            tempUpdateCmd.Parameters.AddWithValue("@TempId", temporaryId);
                            tempUpdateCmd.Parameters.AddWithValue("@OldId", oldId);
                            tempUpdateCmd.ExecuteNonQuery();
                        }
                    }

                    using (var finalizeCmd = new SQLiteCommand("UPDATE Invoices SET Id = -Id WHERE Id < 0", conn, transaction))
                    {
                        finalizeCmd.ExecuteNonQuery();
                    }

                    SyncInvoiceSequence(conn, transaction);
                    transaction.Commit();
                }
            }
        }

        public static void SetInvoiceBarcode(int invoiceId, string barcodeValue)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                const string sql = "UPDATE Invoices SET BarcodeValue = @BarcodeValue WHERE Id = @Id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@BarcodeValue", barcodeValue);
                    cmd.Parameters.AddWithValue("@Id", invoiceId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteInvoice(int invoiceId)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    const string deleteSql = "DELETE FROM Invoices WHERE Id = @Id";
                    int deletedRows;
                    using (var deleteCmd = new SQLiteCommand(deleteSql, conn, transaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@Id", invoiceId);
                        deletedRows = deleteCmd.ExecuteNonQuery();
                    }

                    if (deletedRows > 0)
                    {
                        // Avoid PK conflicts while shifting IDs down by using a temporary negative range.
                        const string markSql = "UPDATE Invoices SET Id = -Id WHERE Id > @DeletedId";
                        using (var markCmd = new SQLiteCommand(markSql, conn, transaction))
                        {
                            markCmd.Parameters.AddWithValue("@DeletedId", invoiceId);
                            markCmd.ExecuteNonQuery();
                        }

                        const string normalizeSql = "UPDATE Invoices SET Id = ABS(Id) - 1 WHERE Id < 0";
                        using (var normalizeCmd = new SQLiteCommand(normalizeSql, conn, transaction))
                        {
                            normalizeCmd.ExecuteNonQuery();
                        }

                        SyncInvoiceSequence(conn, transaction);
                    }

                    transaction.Commit();
                }
            }
        }

        public static void UpdateInvoice(Invoice invoice)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string barcodeValue = string.IsNullOrWhiteSpace(invoice.BarcodeValue)
                    ? InvoiceEnhancementService.GenerateRandomBarcodeValue()
                    : invoice.BarcodeValue;
                invoice.BarcodeValue = barcodeValue;

                const string sql = @"UPDATE Invoices
                                     SET ClientName = @ClientName,
                                         InvoiceNumber = @InvoiceNumber,
                                         Date = @Date,
                                         Amount = @Amount,
                                         Details = @Details,
                                         BarcodeValue = @BarcodeValue
                                     WHERE Id = @Id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientName", invoice.ClientName);
                    cmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
                    cmd.Parameters.AddWithValue("@Date", invoice.Date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@Amount", invoice.Amount);
                    cmd.Parameters.AddWithValue("@Details", invoice.Details);
                    cmd.Parameters.AddWithValue("@BarcodeValue", barcodeValue);
                    cmd.Parameters.AddWithValue("@Id", invoice.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void EnsureBarcodeColumn(SQLiteConnection conn)
        {
            if (ColumnExists(conn, "Invoices", "BarcodeValue"))
            {
                return;
            }

            using (var cmd = new SQLiteCommand("ALTER TABLE Invoices ADD COLUMN BarcodeValue TEXT", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void BackfillMissingBarcodes(SQLiteConnection conn)
        {
            const string selectSql = "SELECT Id FROM Invoices WHERE BarcodeValue IS NULL OR TRIM(BarcodeValue) = ''";
            var missingIds = new List<int>();

            using (var selectCmd = new SQLiteCommand(selectSql, conn))
            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    missingIds.Add(Convert.ToInt32(reader["Id"]));
                }
            }

            foreach (int invoiceId in missingIds)
            {
                string barcodeValue = InvoiceEnhancementService.GenerateRandomBarcodeValue();

                using (var updateCmd = new SQLiteCommand("UPDATE Invoices SET BarcodeValue = @BarcodeValue WHERE Id = @Id", conn))
                {
                    updateCmd.Parameters.AddWithValue("@BarcodeValue", barcodeValue);
                    updateCmd.Parameters.AddWithValue("@Id", invoiceId);
                    updateCmd.ExecuteNonQuery();
                }
            }
        }

        private static bool ColumnExists(SQLiteConnection conn, string tableName, string columnName)
        {
            using (var cmd = new SQLiteCommand($"PRAGMA table_info({tableName})", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string currentColumnName = Convert.ToString(reader["name"]);
                    if (string.Equals(currentColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ReaderHasColumn(SQLiteDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void SyncInvoiceSequence(SQLiteConnection conn, SQLiteTransaction transaction)
        {
            const string sqliteSequenceExistsSql = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = 'sqlite_sequence' LIMIT 1";
            using (var checkCmd = new SQLiteCommand(sqliteSequenceExistsSql, conn, transaction))
            {
                if (checkCmd.ExecuteScalar() == null)
                {
                    return;
                }
            }

            long maxId = 0;
            using (var maxCmd = new SQLiteCommand("SELECT COALESCE(MAX(Id), 0) FROM Invoices", conn, transaction))
            {
                object maxResult = maxCmd.ExecuteScalar();
                if (maxResult != null && maxResult != DBNull.Value)
                {
                    maxId = Convert.ToInt64(maxResult);
                }
            }

            const string updateSeqSql = "UPDATE sqlite_sequence SET seq = @Seq WHERE name = 'Invoices'";
            using (var updateSeqCmd = new SQLiteCommand(updateSeqSql, conn, transaction))
            {
                updateSeqCmd.Parameters.AddWithValue("@Seq", maxId);
                int updatedRows = updateSeqCmd.ExecuteNonQuery();
                if (updatedRows > 0)
                {
                    return;
                }
            }

            const string insertSeqSql = "INSERT INTO sqlite_sequence (name, seq) VALUES ('Invoices', @Seq)";
            using (var insertSeqCmd = new SQLiteCommand(insertSeqSql, conn, transaction))
            {
                insertSeqCmd.Parameters.AddWithValue("@Seq", maxId);
                insertSeqCmd.ExecuteNonQuery();
            }
        }

        private static string ResolveDatabasePath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (!LooksLikeBuildOutputDirectory(baseDirectory))
            {
                return Path.Combine(baseDirectory, DatabaseFileName);
            }

            string projectDatabasePath = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\invoices.db"));
            string runtimeDatabasePath = Path.Combine(baseDirectory, DatabaseFileName);
            string solutionDatabasePath = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\invoices.db"));
            string bestSourcePath = SelectBestExistingDatabasePath(
                projectDatabasePath,
                solutionDatabasePath,
                runtimeDatabasePath);

            PromoteDatabaseToProjectPath(bestSourcePath, projectDatabasePath);
            return projectDatabasePath;
        }

        private static bool LooksLikeBuildOutputDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            string normalizedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string configurationFolderName = Path.GetFileName(normalizedPath);
            string parentFolder = Path.GetDirectoryName(normalizedPath);
            string parentFolderName = Path.GetFileName(parentFolder);

            bool isBuildConfigurationFolder =
                string.Equals(configurationFolderName, "Debug", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(configurationFolderName, "Release", StringComparison.OrdinalIgnoreCase);

            return isBuildConfigurationFolder &&
                   string.Equals(parentFolderName, "bin", StringComparison.OrdinalIgnoreCase);
        }

        private static string SelectBestExistingDatabasePath(params string[] candidatePaths)
        {
            if (candidatePaths == null || candidatePaths.Length == 0)
            {
                return null;
            }

            string bestPath = null;
            long bestInvoiceCount = long.MinValue;
            DateTime bestLastWrite = DateTime.MinValue;

            foreach (string candidatePath in candidatePaths)
            {
                if (string.IsNullOrWhiteSpace(candidatePath) || !File.Exists(candidatePath))
                {
                    continue;
                }

                long invoiceCount = GetInvoiceCountSafe(candidatePath);
                DateTime lastWrite = File.GetLastWriteTimeUtc(candidatePath);

                bool shouldReplaceBest =
                    invoiceCount > bestInvoiceCount ||
                    (invoiceCount == bestInvoiceCount && lastWrite > bestLastWrite);

                if (!shouldReplaceBest)
                {
                    continue;
                }

                bestPath = candidatePath;
                bestInvoiceCount = invoiceCount;
                bestLastWrite = lastWrite;
            }

            return bestPath;
        }

        private static long GetInvoiceCountSafe(string databasePath)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;Read Only=True;"))
                {
                    connection.Open();

                    using (var tableCheck = new SQLiteCommand("SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Invoices'", connection))
                    {
                        long tableExists = Convert.ToInt64(tableCheck.ExecuteScalar());
                        if (tableExists == 0)
                        {
                            return 0;
                        }
                    }

                    using (var countCommand = new SQLiteCommand("SELECT COUNT(*) FROM Invoices", connection))
                    {
                        return Convert.ToInt64(countCommand.ExecuteScalar());
                    }
                }
            }
            catch
            {
                return -1;
            }
        }

        private static void PromoteDatabaseToProjectPath(string sourcePath, string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(destinationPath))
            {
                return;
            }

            string destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                return;
            }

            if (string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!File.Exists(destinationPath))
            {
                File.Copy(sourcePath, destinationPath, false);
                return;
            }

            long sourceInvoices = GetInvoiceCountSafe(sourcePath);
            long destinationInvoices = GetInvoiceCountSafe(destinationPath);
            DateTime sourceLastWrite = File.GetLastWriteTimeUtc(sourcePath);
            DateTime destinationLastWrite = File.GetLastWriteTimeUtc(destinationPath);

            bool shouldPromote =
                sourceInvoices > destinationInvoices ||
                (sourceInvoices == destinationInvoices && sourceLastWrite > destinationLastWrite);

            if (!shouldPromote)
            {
                return;
            }

            string backupPath = destinationPath + ".backup";
            File.Copy(destinationPath, backupPath, true);
            File.Copy(sourcePath, destinationPath, true);
        }
    }
}
