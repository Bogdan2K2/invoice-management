using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System.Runtime.InteropServices;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace InvoicesManager
{
    public partial class InvoicesListForm : Form
    {
        private const string EditColumnName = "EditInvoice";
        private const string DeleteColumnName = "DeleteInvoice";

        public InvoicesListForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            ApplyCustomStyle();
            
            UiStyle.AddHoverEffect(btnExportExcel);
            UiStyle.AddHoverEffect(btnGenerateWord);
            
            btnExportExcel.Click += btnExportExcel_Click;
            btnGenerateWord.Click += btnGenerateWord_Click;
            dgv.CellContentClick += dgv_CellContentClick;
            
            LoadData();
        }

        private void ApplyCustomStyle()
        {
            BackColor = UiStyle.PageBackground;

            panelHeader.BackColor = UiStyle.Header;
            lblTitle.ForeColor = UiStyle.HeaderText;
            lblTitle.Font = UiStyle.TitleFont;
            UiStyle.NormalizePictureIcon(pictureBoxLogo);

            panelContent.BackColor = UiStyle.Surface;
            panelButtons.BackColor = UiStyle.SurfaceMuted;

            UiStyle.StyleActionButton(btnExportExcel, UiStyle.Accent);
            UiStyle.StyleActionButton(btnGenerateWord, UiStyle.AccentAlt);

            UiStyle.NormalizeButtonIcon(btnExportExcel);
            UiStyle.NormalizeButtonIcon(btnGenerateWord);

            DataGridViewCellStyle headerStyle = dgv.ColumnHeadersDefaultCellStyle;
            headerStyle.BackColor = UiStyle.Header;
            headerStyle.ForeColor = UiStyle.HeaderText;
            headerStyle.SelectionBackColor = UiStyle.Header;
            headerStyle.SelectionForeColor = UiStyle.HeaderText;
            headerStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle = headerStyle;

            DataGridViewCellStyle rowsStyle = dgv.DefaultCellStyle;
            rowsStyle.BackColor = UiStyle.Surface;
            rowsStyle.ForeColor = UiStyle.TextPrimary;
            rowsStyle.SelectionBackColor = UiStyle.Accent;
            rowsStyle.SelectionForeColor = Color.White;
            rowsStyle.Font = UiStyle.BodyFont;
            dgv.DefaultCellStyle = rowsStyle;

            dgv.GridColor = Color.FromArgb(203, 213, 225);
            dgv.RowTemplate.Height = 32;
            dgv.BackgroundColor = UiStyle.Surface;
        }

        private void LoadData()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DatabaseHelper.GetConnectionStringPublic()))
                {
                    connection.Open();
                    string query = @"
                        SELECT
                            Id,
                            ClientName,
                            InvoiceNumber,
                            COALESCE(strftime('%Y-%m-%d', Date), Date) AS Date,
                            Amount,
                            Details,
                            BarcodeValue
                        FROM Invoices
                        ORDER BY Id";
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    if (!dataTable.Columns.Contains("DisplayId"))
                    {
                        DataColumn displayIdColumn = new DataColumn("DisplayId", typeof(int));
                        dataTable.Columns.Add(displayIdColumn);
                        displayIdColumn.SetOrdinal(0);

                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            dataTable.Rows[i]["DisplayId"] = i + 1;
                        }
                    }

                    dgv.DataSource = dataTable;
                    
                    // format grid 
                    if (dgv.Columns.Contains("Amount"))
                    {
                        dgv.Columns["Amount"].DefaultCellStyle.Format = "C2";
                    }
                    if (dgv.Columns.Contains("Date"))
                    {
                        dgv.Columns["Date"].DefaultCellStyle.Format = "d";
                    }

                    // column headers
                    if (dgv.Columns.Contains("DisplayId"))
                        dgv.Columns["DisplayId"].HeaderText = "ID";
                    if (dgv.Columns.Contains("Id"))
                        dgv.Columns["Id"].Visible = false;
                    if (dgv.Columns.Contains("ClientName"))
                        dgv.Columns["ClientName"].HeaderText = "Client Name";
                    if (dgv.Columns.Contains("InvoiceNumber"))
                        dgv.Columns["InvoiceNumber"].HeaderText = "Invoice Number";
                    if (dgv.Columns.Contains("Date"))
                        dgv.Columns["Date"].HeaderText = "Date";
                    if (dgv.Columns.Contains("Amount"))
                        dgv.Columns["Amount"].HeaderText = "Amount";
                    if (dgv.Columns.Contains("Details"))
                        dgv.Columns["Details"].HeaderText = "Details";
                    if (dgv.Columns.Contains("BarcodeValue"))
                        dgv.Columns["BarcodeValue"].Visible = false;

                    EnsureActionColumns();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseHelper.NormalizeInvoiceIds();
                LoadData();

                var invoices = DatabaseHelper.GetAllInvoices();
                // warning if no invoices are available
                if (invoices.Count == 0)
                {
                    MessageBox.Show("There are no invoices to export.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string baseFileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Invoices");
                string excelFilePath = baseFileName + ".xlsx";
                string assetsDirectory = InvoiceEnhancementService.GetAssetsDirectory();

                var excelApp = new Excel.Application();
                var workbook = excelApp.Workbooks.Add();
                Excel._Worksheet worksheet = (Excel._Worksheet)workbook.ActiveSheet;

                worksheet.Cells[1, 1] = "Id";
                worksheet.Cells[1, 2] = "Client";
                worksheet.Cells[1, 3] = "Invoice Number";
                worksheet.Cells[1, 4] = "Date";
                worksheet.Cells[1, 5] = "Amount";
                worksheet.Cells[1, 6] = "Details";
                worksheet.Cells[1, 7] = "Logo";

                int row = 2;
                foreach (var inv in invoices)
                {
                    worksheet.Cells[row, 1] = inv.Id;
                    worksheet.Cells[row, 2] = inv.ClientName;
                    worksheet.Cells[row, 3] = inv.InvoiceNumber;
                    worksheet.Cells[row, 4] = inv.Date.ToString("MM/dd/yyyy");
                    worksheet.Cells[row, 5] = inv.Amount;
                    worksheet.Cells[row, 6] = inv.Details;
                    worksheet.Cells[row, 7] = inv.ClientName;
                    InsertClientLogoInExcelRow(worksheet, row, inv.ClientName, assetsDirectory);
                    row++;
                }

                var headerRange = worksheet.get_Range("A1", "G1");
                headerRange.Font.Bold = true;
                headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);

                worksheet.Columns.AutoFit();
                ((Excel.Range)worksheet.Columns[7]).ColumnWidth = 18;

                workbook.SaveAs(excelFilePath);
                workbook.Close();
                excelApp.Quit();

                System.Diagnostics.Process.Start(excelFilePath);
                MessageBox.Show("Excel file has been generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating Excel file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InsertClientLogoInExcelRow(Excel._Worksheet worksheet, int rowIndex, string clientName, string assetsDirectory)
        {
            string logoPath = InvoiceEnhancementService.GetClientLogoPath(clientName, assetsDirectory);
            if (string.IsNullOrWhiteSpace(logoPath) || !File.Exists(logoPath))
            {
                return;
            }

            Excel.Range logoCell = (Excel.Range)worksheet.Cells[rowIndex, 7];
            float left = (float)(double)logoCell.Left + 4f;
            float top = (float)(double)logoCell.Top + 2f;
            const float width = 30f;
            const float height = 30f;

            ((Excel.Range)worksheet.Rows[rowIndex]).RowHeight = 34;
            dynamic shapes = worksheet.Shapes;
            shapes.AddPicture(
                logoPath,
                0,
                -1,
                left,
                top,
                width,
                height);
        }

        private void btnGenerateWord_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an invoice to generate Word document.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Invoice selectedInvoice = GetSelectedInvoice();
                string baseFileName = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    $"Invoice_{selectedInvoice.InvoiceNumber}");
                string wordFilePath = baseFileName + ".docx";

                GenerateDetailedWordInvoice(wordFilePath, selectedInvoice);
                System.Diagnostics.Process.Start(wordFilePath);
                MessageBox.Show("Word document has been generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating Word document: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Invoice GetSelectedInvoice()
        {
            DataGridViewRow selectedRow = dgv.SelectedRows[0];
            int id = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            string clientName = Convert.ToString(selectedRow.Cells["ClientName"].Value);
            string invoiceNumber = Convert.ToString(selectedRow.Cells["InvoiceNumber"].Value);
            DateTime date = Convert.ToDateTime(selectedRow.Cells["Date"].Value);
            decimal amount = Convert.ToDecimal(selectedRow.Cells["Amount"].Value);
            string details = Convert.ToString(selectedRow.Cells["Details"].Value);
            string barcodeValue = dgv.Columns.Contains("BarcodeValue")
                ? Convert.ToString(selectedRow.Cells["BarcodeValue"].Value)
                : null;

            return new Invoice(id, clientName, invoiceNumber, date, amount, details, barcodeValue);
        }

        private void GenerateDetailedWordInvoice(string wordFilePath, Invoice invoice)
        {
            string assetsDirectory = InvoiceEnhancementService.GetAssetsDirectory();
            string logoPath = InvoiceEnhancementService.GetClientLogoPath(invoice.ClientName, assetsDirectory);
            string barcodeValue = invoice.BarcodeValue;
            if (string.IsNullOrWhiteSpace(barcodeValue))
            {
                barcodeValue = InvoiceEnhancementService.GenerateRandomBarcodeValue();
                DatabaseHelper.SetInvoiceBarcode(invoice.Id, barcodeValue);
            }

            string barcodeImagePath = InvoiceEnhancementService.GenerateBarcodeImage(barcodeValue, assetsDirectory);

            Word._Application wordApp = null;
            Word.Document document = null;

            try
            {
                wordApp = new Word.Application();
                document = wordApp.Documents.Add();
                document.PageSetup.PaperSize = Word.WdPaperSize.wdPaperA4;
                document.PageSetup.TopMargin = wordApp.CentimetersToPoints(1.8f);
                document.PageSetup.BottomMargin = wordApp.CentimetersToPoints(1.8f);
                document.PageSetup.LeftMargin = wordApp.CentimetersToPoints(2.0f);
                document.PageSetup.RightMargin = wordApp.CentimetersToPoints(2.0f);

                AddBrandHeader(document, logoPath, invoice.ClientName);
                AddInvoiceTitle(document);
                AddInvoiceMetaTable(document, invoice);
                AddDetailsBlock(document, invoice.Details);
                AddBarcodeBlock(document, barcodeImagePath, barcodeValue);

                Word.Range footerRange = document.Sections[1].Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                footerRange.Text = $"Generated on {DateTime.Now:MMMM dd, yyyy}";
                footerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;

                document.SaveAs(wordFilePath);
            }
            finally
            {
                if (document != null)
                {
                    document.Close();
                    Marshal.FinalReleaseComObject(document);
                }

                if (wordApp != null)
                {
                    wordApp.Quit();
                    Marshal.FinalReleaseComObject(wordApp);
                }
            }
        }

        private void AddBrandHeader(Word.Document document, string logoPath, string clientName)
        {
            if (string.IsNullOrWhiteSpace(logoPath) || !File.Exists(logoPath))
            {
                return;
            }

            try
            {
                Word.Section firstSection = document.Sections[1];
                Word.HeaderFooter header = firstSection.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                Word.Range anchorRange = header.Range;

                object linkToFile = false;
                object saveWithDocument = true;
                object left = 0f;
                object top = 0f;
                object width = 56f;
                object height = 56f;
                object anchor = anchorRange;

                Word.Shape logoShape = header.Shapes.AddPicture(
                    logoPath,
                    ref linkToFile,
                    ref saveWithDocument,
                    ref left,
                    ref top,
                    ref width,
                    ref height,
                    ref anchor);

                logoShape.Left = document.PageSetup.PageWidth - document.PageSetup.RightMargin - 62f;
                logoShape.Top = 4f;
            }
            catch
            {
                Word.Range fallbackRange = document.Sections[1].Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                fallbackRange.Text = $"Client: {clientName}";
                fallbackRange.Font.Name = "Segoe UI";
                fallbackRange.Font.Bold = 1;
                fallbackRange.Font.Size = 12;
                fallbackRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
            }
        }

        private void AddInvoiceTitle(Word.Document document)
        {
            Word.Paragraph titleParagraph = document.Content.Paragraphs.Add();
            titleParagraph.Range.Text = "INVOICE";
            titleParagraph.Range.Font.Name = "Segoe UI";
            titleParagraph.Range.Font.Bold = 1;
            titleParagraph.Range.Font.Size = 24;
            titleParagraph.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            titleParagraph.Format.SpaceAfter = 14;
            titleParagraph.Range.InsertParagraphAfter();
        }

        private void AddInvoiceMetaTable(Word.Document document, Invoice invoice)
        {
            decimal vat = Math.Round(invoice.Amount * 0.19m, 2);
            decimal total = invoice.Amount + vat;

            Word.Range tableRange = document.Bookmarks.get_Item("\\endofdoc").Range;
            Word.Table invoiceTable = document.Tables.Add(tableRange, 7, 2);
            invoiceTable.Borders.Enable = 1;
            invoiceTable.Range.Font.Name = "Segoe UI";
            invoiceTable.Range.Font.Size = 11;
            invoiceTable.Rows.Alignment = Word.WdRowAlignment.wdAlignRowLeft;
            invoiceTable.Columns[1].Width = 180f;
            invoiceTable.Columns[2].Width = 290f;

            SetTableRow(invoiceTable, 1, "Invoice ID", invoice.Id.ToString());
            SetTableRow(invoiceTable, 2, "Invoice Number", invoice.InvoiceNumber);
            SetTableRow(invoiceTable, 3, "Client", invoice.ClientName);
            SetTableRow(invoiceTable, 4, "Date", invoice.Date.ToString("MM/dd/yyyy"));
            SetTableRow(invoiceTable, 5, "Amount (Base)", invoice.Amount.ToString("C2"));
            SetTableRow(invoiceTable, 6, "VAT (19%)", vat.ToString("C2"));
            SetTableRow(invoiceTable, 7, "Total", total.ToString("C2"));

            invoiceTable.Range.InsertParagraphAfter();
            invoiceTable.Range.InsertParagraphAfter();
        }

        private void SetTableRow(Word.Table table, int rowIndex, string label, string value)
        {
            table.Cell(rowIndex, 1).Range.Text = label;
            table.Cell(rowIndex, 1).Range.Bold = 1;
            table.Cell(rowIndex, 2).Range.Text = value;
            table.Cell(rowIndex, 2).Range.Bold = 0;
        }

        private void AddDetailsBlock(Word.Document document, string details)
        {
            Word.Range detailsTitle = document.Bookmarks.get_Item("\\endofdoc").Range;
            detailsTitle.Text = "Details";
            detailsTitle.Font.Name = "Segoe UI";
            detailsTitle.Font.Bold = 1;
            detailsTitle.Font.Size = 14;
            detailsTitle.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            detailsTitle.InsertParagraphAfter();

            Word.Range detailsBody = document.Bookmarks.get_Item("\\endofdoc").Range;
            detailsBody.Text = string.IsNullOrWhiteSpace(details) ? "No details provided." : details;
            detailsBody.Font.Name = "Segoe UI";
            detailsBody.Font.Bold = 0;
            detailsBody.Font.Size = 11;
            detailsBody.InsertParagraphAfter();
            detailsBody.InsertParagraphAfter();
        }

        private void AddBarcodeBlock(Word.Document document, string barcodeImagePath, string barcodeValue)
        {
            Word.Range barcodeTitle = document.Bookmarks.get_Item("\\endofdoc").Range;
            barcodeTitle.Text = "Invoice Barcode";
            barcodeTitle.Font.Name = "Segoe UI";
            barcodeTitle.Font.Bold = 1;
            barcodeTitle.Font.Size = 12;
            barcodeTitle.InsertParagraphAfter();

            if (!string.IsNullOrWhiteSpace(barcodeImagePath) && File.Exists(barcodeImagePath))
            {
                Word.Range barcodeImageRange = document.Bookmarks.get_Item("\\endofdoc").Range;
                Word.InlineShape barcodeImage = barcodeImageRange.InlineShapes.AddPicture(barcodeImagePath, false, true);
                barcodeImage.Width = 330f;
                barcodeImageRange.InsertParagraphAfter();
            }

            Word.Range barcodeText = document.Bookmarks.get_Item("\\endofdoc").Range;
            barcodeText.Text = barcodeValue;
            barcodeText.Font.Name = "Consolas";
            barcodeText.Font.Size = 10;
            barcodeText.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            barcodeText.InsertParagraphAfter();
        }

        private void EnsureDeleteColumn()
        {
            if (dgv.Columns.Contains(DeleteColumnName))
            {
                return;
            }

            var deleteColumn = new DataGridViewButtonColumn
            {
                Name = DeleteColumnName,
                HeaderText = "Delete",
                Text = "-",
                Width = 78,
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat
            };

            deleteColumn.DefaultCellStyle.BackColor = Color.FromArgb(220, 38, 38);
            deleteColumn.DefaultCellStyle.ForeColor = Color.White;
            deleteColumn.DefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            deleteColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            deleteColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgv.Columns.Add(deleteColumn);
        }

        private void EnsureEditColumn()
        {
            if (dgv.Columns.Contains(EditColumnName))
            {
                return;
            }

            var editColumn = new DataGridViewButtonColumn
            {
                Name = EditColumnName,
                HeaderText = "Edit",
                Text = "✎",
                Width = 68,
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat
            };

            editColumn.DefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
            editColumn.DefaultCellStyle.ForeColor = Color.White;
            editColumn.DefaultCellStyle.Font = new Font("Segoe UI Symbol", 11F, FontStyle.Bold);
            editColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            editColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgv.Columns.Add(editColumn);
        }

        private void EnsureActionColumns()
        {
            EnsureEditColumn();
            EnsureDeleteColumn();

            int firstActionIndex = dgv.Columns.Count - 2;
            dgv.Columns[EditColumnName].DisplayIndex = firstActionIndex;
            dgv.Columns[DeleteColumnName].DisplayIndex = firstActionIndex + 1;
        }

        private Invoice GetInvoiceFromRow(DataGridViewRow row)
        {
            int id = Convert.ToInt32(row.Cells["Id"].Value);
            string clientName = Convert.ToString(row.Cells["ClientName"].Value);
            string invoiceNumber = Convert.ToString(row.Cells["InvoiceNumber"].Value);
            DateTime date = Convert.ToDateTime(row.Cells["Date"].Value);
            decimal amount = Convert.ToDecimal(row.Cells["Amount"].Value);
            string details = Convert.ToString(row.Cells["Details"].Value);
            string barcodeValue = dgv.Columns.Contains("BarcodeValue")
                ? Convert.ToString(row.Cells["BarcodeValue"].Value)
                : null;

            return new Invoice(id, clientName, invoiceNumber, date, amount, details, barcodeValue);
        }

        private void dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string selectedColumn = dgv.Columns[e.ColumnIndex].Name;
            if (string.Equals(selectedColumn, EditColumnName, StringComparison.Ordinal))
            {
                Invoice invoice = GetInvoiceFromRow(dgv.Rows[e.RowIndex]);
                using (var editForm = new AddInvoiceForm(invoice))
                {
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadData();
                    }
                }

                return;
            }

            if (!string.Equals(selectedColumn, DeleteColumnName, StringComparison.Ordinal))
            {
                return;
            }

            Invoice invoiceToDelete = GetInvoiceFromRow(dgv.Rows[e.RowIndex]);

            DialogResult confirmDelete = MessageBox.Show(
                $"Delete invoice {invoiceToDelete.InvoiceNumber}?",
                "Delete Invoice",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmDelete != DialogResult.Yes)
            {
                return;
            }

            DatabaseHelper.DeleteInvoice(invoiceToDelete.Id);
            LoadData();
        }
    }
}


