using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace InvoicesManager
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            ApplyCustomStyle();

            // database init sqlite
            DatabaseHelper.InitializeDatabase();
            
            UiStyle.AddHoverEffect(btnAddInvoice);
            UiStyle.AddHoverEffect(btnViewInvoices);
            UiStyle.AddHoverEffect(btnExit);
            
            btnAddInvoice.Click += btnAddInvoice_Click;
            btnViewInvoices.Click += btnViewInvoices_Click;
            btnExit.Click += btnExit_Click;
            
            UpdateSummary();
        }

        private void ApplyCustomStyle()
        {
            BackColor = UiStyle.PageBackground;

            panelHeader.BackColor = UiStyle.Header;
            lblTitle.ForeColor = UiStyle.HeaderText;
            lblTitle.Font = UiStyle.TitleFont;
            UiStyle.NormalizePictureIcon(pictureBoxLogo);

            lblSummary.BackColor = UiStyle.Surface;
            lblSummary.ForeColor = UiStyle.TextMuted;
            lblSummary.Font = UiStyle.BodyFont;

            panelButtons.BackColor = UiStyle.SurfaceMuted;

            UiStyle.StyleActionButton(btnAddInvoice, UiStyle.Accent);
            UiStyle.StyleActionButton(btnViewInvoices, UiStyle.AccentAlt);
            UiStyle.StyleActionButton(btnExit, UiStyle.Danger);

            UiStyle.NormalizeButtonIcon(btnAddInvoice);
            UiStyle.NormalizeButtonIcon(btnViewInvoices);
            UiStyle.NormalizeButtonIcon(btnExit);

            lblSummary.Padding = new Padding(16, 12, 16, 12);
            panelButtons.Padding = new Padding(16);

            const int buttonHeight = 58;
            const int buttonGap = 12;
            int buttonWidth = panelButtons.Width - (panelButtons.Padding.Horizontal);
            int startX = panelButtons.Padding.Left;

            btnAddInvoice.Size = new Size(buttonWidth, buttonHeight);
            btnViewInvoices.Size = new Size(buttonWidth, buttonHeight);
            btnExit.Size = new Size(buttonWidth, buttonHeight);

            btnAddInvoice.Location = new Point(startX, 18);
            btnViewInvoices.Location = new Point(startX, btnAddInvoice.Bottom + buttonGap);
            btnExit.Location = new Point(startX, btnViewInvoices.Bottom + buttonGap);
        }

        private void UpdateSummary()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DatabaseHelper.GetConnectionStringPublic()))
                {
                    connection.Open();

                    string countQuery = "SELECT COUNT(*) FROM Invoices";
                    SQLiteCommand countCommand = new SQLiteCommand(countQuery, connection);
                    int invoiceCount = Convert.ToInt32(countCommand.ExecuteScalar());

                    // sum ammount of invoices
                    string sumQuery = "SELECT SUM(Amount) FROM Invoices";
                    SQLiteCommand sumCommand = new SQLiteCommand(sumQuery, connection);
                    object result = sumCommand.ExecuteScalar();
                    decimal totalAmount = (result != DBNull.Value) ? Convert.ToDecimal(result) : 0;

                    lblSummary.Text = $"Total Invoices: {invoiceCount}\nTotal Amount: {totalAmount:C2}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating summary: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddInvoice_Click(object sender, EventArgs e)
        {
            AddInvoiceForm addInvoiceForm = new AddInvoiceForm();
            if (addInvoiceForm.ShowDialog() == DialogResult.OK)
            {
                UpdateSummary();
            }
        }

        private void btnViewInvoices_Click(object sender, EventArgs e)
        {
            InvoicesListForm invoicesListForm = new InvoicesListForm();
            invoicesListForm.ShowDialog();
            UpdateSummary();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
