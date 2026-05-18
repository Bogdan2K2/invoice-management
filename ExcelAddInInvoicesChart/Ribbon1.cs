using Microsoft.Office.Tools.Ribbon;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Office.Interop;

namespace ExcelAddInInvoicesChart
{
    public partial class Ribbon1
    {
        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
        }

        private void btnGenerateChart_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Application excelApp = Globals.ThisAddIn.Application;
                Microsoft.Office.Interop.Excel.Worksheet worksheet = excelApp.ActiveSheet as Microsoft.Office.Interop.Excel.Worksheet;
                Microsoft.Office.Interop.Excel.Range selectedRange = excelApp.Selection as Microsoft.Office.Interop.Excel.Range;

                if (selectedRange == null || selectedRange.Rows.Count < 2)
                {
                    MessageBox.Show("Please select a table with at least 2 rows (header + data).", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int amountColumn = -1;
                for (int col = 1; col <= selectedRange.Columns.Count; col++)
                {
                    string header = selectedRange.Cells[1, col].Text.ToString().Trim();
                    if (header.Equals("Amount", StringComparison.OrdinalIgnoreCase))
                    {
                        amountColumn = col;
                        break;
                    }
                }

                if (amountColumn == -1)
                {
                    MessageBox.Show("Amount column not found.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                double totalAmount = 0;
                for (int row = 2; row <= selectedRange.Rows.Count; row++)
                {
                    string cellValue = selectedRange.Cells[row, amountColumn].Text.ToString();
                    if (double.TryParse(cellValue, out double value))
                    {
                        totalAmount += value;
                    }
                }

                worksheet.Cells[1, selectedRange.Columns.Count + 2].Value = "Total Amount";
                worksheet.Cells[1, selectedRange.Columns.Count + 2].Font.Bold = true;
                worksheet.Cells[2, selectedRange.Columns.Count + 2].Value = totalAmount;
                worksheet.Cells[2, selectedRange.Columns.Count + 2].NumberFormat = "#,##0.00";
                worksheet.Columns[selectedRange.Columns.Count + 2].AutoFit();

                MessageBox.Show($"Total amount calculated: {totalAmount:N2}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating total: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}