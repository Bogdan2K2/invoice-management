using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InvoicesManager
{
    public partial class AddInvoiceForm : Form
    {
        private readonly int? editInvoiceId;
        private string editBarcodeValue;

        public AddInvoiceForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            ApplyCustomStyle();
            
            UiStyle.AddHoverEffect(btnSave);
        }

        public AddInvoiceForm(Invoice invoice) : this()
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            editInvoiceId = invoice.Id;
            editBarcodeValue = invoice.BarcodeValue;

            txtClientName.Text = invoice.ClientName;
            txtInvoiceNumber.Text = invoice.InvoiceNumber;
            dtpDate.Value = invoice.Date;
            txtAmount.Text = invoice.Amount.ToString("0.##");
            txtDetails.Text = invoice.Details;

            lblTitle.Text = "Edit Invoice";
            Text = "Edit Invoice";
            btnSave.Text = "Update";
        }

        private bool IsEditMode => editInvoiceId.HasValue;

        private void ApplyCustomStyle()
        {
            BackColor = UiStyle.PageBackground;

            panelHeader.BackColor = UiStyle.Header;
            lblTitle.ForeColor = UiStyle.HeaderText;
            lblTitle.Font = UiStyle.TitleFont;
            UiStyle.NormalizePictureIcon(pictureBoxLogo);

            panelContent.BackColor = UiStyle.Surface;

            Label[] labels = { lblClientName, lblInvoiceNumber, lblDate, lblAmount, lblDetails };
            foreach (Label label in labels)
            {
                label.ForeColor = UiStyle.TextPrimary;
                label.Font = UiStyle.BodyFont;
            }

            TextBox[] textInputs = { txtClientName, txtInvoiceNumber, txtAmount, txtDetails };
            foreach (TextBox textInput in textInputs)
            {
                textInput.Font = UiStyle.BodyFont;
                textInput.BackColor = UiStyle.Surface;
                textInput.ForeColor = UiStyle.TextPrimary;
            }

            dtpDate.Font = UiStyle.BodyFont;
            dtpDate.CalendarForeColor = UiStyle.TextPrimary;

            UiStyle.StyleActionButton(btnSave, UiStyle.Accent);
            UiStyle.NormalizeButtonIcon(btnSave);
            btnSave.Size = new Size(160, 58);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // validate inputs
            if (string.IsNullOrWhiteSpace(txtClientName.Text))
            {
                MessageBox.Show("Please enter client name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtClientName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtInvoiceNumber.Text))
            {
                MessageBox.Show("Please enter invoice number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtInvoiceNumber.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAmount.Text) || !decimal.TryParse(txtAmount.Text, out decimal amount))
            {
                MessageBox.Show("Please enter a valid amount.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAmount.Focus();
                return;
            }

            try
            {
                var invoice = new Invoice(
                    editInvoiceId ?? 0,
                    txtClientName.Text.Trim(),
                    txtInvoiceNumber.Text.Trim(),
                    dtpDate.Value.Date,
                    amount,
                    txtDetails.Text.Trim(),
                    IsEditMode ? editBarcodeValue : InvoiceEnhancementService.GenerateRandomBarcodeValue());

                if (IsEditMode)
                {
                    DatabaseHelper.UpdateInvoice(invoice);
                    MessageBox.Show("Invoice updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    DatabaseHelper.AddInvoice(invoice);
                    MessageBox.Show("Invoice saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving invoice: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            txtClientName.Text = string.Empty;
            txtInvoiceNumber.Text = string.Empty;
            dtpDate.Value = DateTime.Now;
            txtAmount.Text = string.Empty;
            txtDetails.Text = string.Empty;
            txtClientName.Focus();
        }
    }
}
