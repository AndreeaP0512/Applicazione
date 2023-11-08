using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PentruIon
{
  public partial class FrmStockChanges : XtraForm
  {
    struct ItemComparer : IEqualityComparer<InvoiceDS.InvoiceItemRow>
    {
      public bool Equals(InvoiceDS.InvoiceItemRow x, InvoiceDS.InvoiceItemRow y)
      {
        return x.ProductName == y.ProductName;
      }

      public int GetHashCode(InvoiceDS.InvoiceItemRow obj)
      {
        return obj.ProductName.GetHashCode();
      }
    }

    private InvoiceDS _invoiceDS;
    public FrmStockChanges(InvoiceDS invoiceDS)
    {
      InitializeComponent();
      _invoiceDS = invoiceDS;
      lookUpProductName.Properties.DataSource = invoiceDS.InvoiceItem.Distinct(new ItemComparer());
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void OKButton_Click(object sender, EventArgs e)
    {
      if (lookUpProductName.EditValue == null)
      {
        XtraMessageBox.Show(this,"Please insert a Product Name");
        this.DialogResult = DialogResult.None;
        return;
      }
      if (lookUpStartDate.DateTime.Date > lookUpEndDate.DateTime.Date)
      {
        XtraMessageBox.Show(this,"Start Date must be before End Date");
        this.DialogResult = DialogResult.None;
        return;
      }
      ReportDS reportDS = new ReportDS();
      List<DateTime> dates = new List<DateTime>();

      for (DateTime dt = lookUpStartDate.DateTime; dt <= lookUpEndDate.DateTime; dt = dt.AddDays(1))
      {
        dates.Add(dt);
        
      }
      ReportDS.ReportRow previousRow = null;
      foreach (DateTime date in dates)
      {
        ReportDS.ReportRow reportRow = reportDS.Report.NewReportRow();
        reportRow.Date = date;
        reportRow.InitialQuantity = 0;
        reportRow.CurrentQuantity = 0;
       
        if (previousRow == null)
        {
          foreach(InvoiceDS.InvoiceItemRow invoice in _invoiceDS.InvoiceItem)
          {
            if (invoice.ProductName == lookUpProductName.EditValue.ToString() && invoice.InvoiceRow.IssueDate.Date< date.Date)
            {
              reportRow.InitialQuantity += invoice.Quantity;
            }
            if (invoice.ProductName == lookUpProductName.EditValue.ToString() && invoice.InvoiceRow.IssueDate.Date == date.Date)
            {
              reportRow.CurrentQuantity += invoice.Quantity;
            }
          }
        }
        else
        {
          reportRow.InitialQuantity = previousRow.InitialQuantity + previousRow.CurrentQuantity;
          foreach (InvoiceDS.InvoiceItemRow invoice in _invoiceDS.InvoiceItem)
          {
            if (invoice.ProductName == lookUpProductName.EditValue.ToString() && invoice.InvoiceRow.IssueDate.Date == date.Date)
            {
              reportRow.CurrentQuantity += invoice.Quantity;
            }
          }
        }
        reportDS.Report.AddReportRow(reportRow);
        previousRow = reportRow;
      }
      StockReport stockReport = new StockReport(lookUpProductName.EditValue.ToString());
      stockReport.DataSource = reportDS;
      ReportPrintTool printTool = new ReportPrintTool(stockReport); 
      printTool.ShowRibbonPreviewDialog(UserLookAndFeel.Default);
      this.Close();
    }
  }
}
