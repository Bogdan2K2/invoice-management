using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;
using System.Windows.Forms;

namespace ExcelAddInInvoicesChart
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            try
            {

                if (Application != null)
                {

                    Application.WorkbookOpen += Application_WorkbookOpen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la inițializarea add-in-ului: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Application_WorkbookOpen(Excel.Workbook Wb)
        {

        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            if (Application != null)
            {
                Application.WorkbookOpen -= Application_WorkbookOpen;
            }
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
