using System;

namespace InvoicesManager
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSummary = new System.Windows.Forms.Label();
            this.btnAddInvoice = new System.Windows.Forms.Button();
            this.btnViewInvoices = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(70, 25);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(255, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Invoice Management";
            // 
            // lblSummary
            // 
            this.lblSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSummary.BackColor = System.Drawing.Color.White;
            this.lblSummary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblSummary.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblSummary.Location = new System.Drawing.Point(20, 100);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Padding = new System.Windows.Forms.Padding(10);
            this.lblSummary.Size = new System.Drawing.Size(444, 92);
            this.lblSummary.TabIndex = 1;
            this.lblSummary.Text = "Loading data...";
            // 
            // btnAddInvoice
            // 
            this.btnAddInvoice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnAddInvoice.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddInvoice.FlatAppearance.BorderSize = 0;
            this.btnAddInvoice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddInvoice.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddInvoice.ForeColor = System.Drawing.Color.White;
            this.btnAddInvoice.Image = global::InvoicesManager.Properties.Resources.icons8_invoice_64;
            this.btnAddInvoice.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAddInvoice.Location = new System.Drawing.Point(20, 17);
            this.btnAddInvoice.Name = "btnAddInvoice";
            this.btnAddInvoice.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnAddInvoice.Size = new System.Drawing.Size(200, 71);
            this.btnAddInvoice.TabIndex = 2;
            this.btnAddInvoice.Text = "  Add Invoice";
            this.btnAddInvoice.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAddInvoice.UseVisualStyleBackColor = false;
            // 
            // btnViewInvoices
            // 
            this.btnViewInvoices.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(125)))), ((int)(((byte)(50)))));
            this.btnViewInvoices.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewInvoices.FlatAppearance.BorderSize = 0;
            this.btnViewInvoices.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewInvoices.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnViewInvoices.ForeColor = System.Drawing.Color.White;
            this.btnViewInvoices.Image = global::InvoicesManager.Properties.Resources.icons8_verify_48;
            this.btnViewInvoices.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnViewInvoices.Location = new System.Drawing.Point(20, 94);
            this.btnViewInvoices.Name = "btnViewInvoices";
            this.btnViewInvoices.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnViewInvoices.Size = new System.Drawing.Size(200, 82);
            this.btnViewInvoices.TabIndex = 3;
            this.btnViewInvoices.Text = "  View Invoices";
            this.btnViewInvoices.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnViewInvoices.UseVisualStyleBackColor = false;
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(47)))), ((int)(((byte)(47)))));
            this.btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Image = global::InvoicesManager.Properties.Resources.icons8_exit_64;
            this.btnExit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExit.Location = new System.Drawing.Point(20, 182);
            this.btnExit.Name = "btnExit";
            this.btnExit.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnExit.Size = new System.Drawing.Size(200, 70);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "  Exit";
            this.btnExit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnExit.UseVisualStyleBackColor = false;
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.panelHeader.Controls.Add(this.pictureBoxLogo);
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(484, 80);
            this.panelHeader.TabIndex = 5;
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxLogo.Image = global::InvoicesManager.Properties.Resources.icons8_invoice_64;
            this.pictureBoxLogo.Location = new System.Drawing.Point(20, 20);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(40, 40);
            this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxLogo.TabIndex = 1;
            this.pictureBoxLogo.TabStop = false;
            // 
            // panelButtons
            // 
            this.panelButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panelButtons.Controls.Add(this.btnAddInvoice);
            this.panelButtons.Controls.Add(this.btnViewInvoices);
            this.panelButtons.Controls.Add(this.btnExit);
            this.panelButtons.Location = new System.Drawing.Point(20, 208);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(444, 243);
            this.panelButtons.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(484, 466);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.lblSummary);
            this.Name = "MainForm";
            this.Text = "Invoice Manager";
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Button btnAddInvoice;
        private System.Windows.Forms.Button btnViewInvoices;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private System.Windows.Forms.Panel panelButtons;
    }
}
