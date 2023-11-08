using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace PentruIon
{
  public partial class FrmInvoiceEdit : XtraForm
  {
    private InvoiceDS.InvoiceRow invoiceRow;

    
    public FrmInvoiceEdit(InvoiceDS.InvoiceRow invoiceRow, InvoiceDS invoiceDS)
    {
      InitializeComponent();

      if(invoiceRow == null)
      {
        invoiceRow = invoiceDS.Invoice.NewInvoiceRow();
        invoiceRow.ClientName = "";
        invoiceRow.InvoiceNumber = "";
        invoiceRow.IssueDate = DateTime.Now;
        invoiceRow.DueDate = DateTime.Now;
        invoiceRow.IdCurrency = 1;
        invoiceDS.Invoice.AddInvoiceRow(invoiceRow);

      }
      this.invoiceRow = invoiceRow;
      
      this.invoiceDS = invoiceDS;
     
      

      invoiceEditBS.DataSource = invoiceDS.InvoiceItem;
      invoiceEditBS.Filter = string.Format("{0} = {1}",
                                            invoiceDS.InvoiceItem.IdInvoiceColumn,
                                            invoiceRow.Id);
      InvoiceEditTable.DataSource = invoiceEditBS;

      if (!invoiceRow.IsInvoiceNumberNull()) 
      { 
        txtInvoiceNumber.Text = invoiceRow.InvoiceNumber; 
      }
      if (!invoiceRow.IsIssueDateNull()) { lookUpIssueDate.DateTime = invoiceRow.IssueDate; }
      if (!invoiceRow.IsDueDateNull()) { lookUpDueDate.DateTime = invoiceRow.DueDate; }
      if (!invoiceRow.IsClientNameNull()) { txtClientName.Text = invoiceRow.ClientName; }

      lookUpCurrency.Properties.DataSource = invoiceDS.Currency;
      if (!invoiceRow.IsIdCurrencyNull()) { lookUpCurrency.EditValue = invoiceRow.CurrencyRow.Id; }
      else { lookUpCurrency.EditValue = 1; }
      

    }
    

    private void CancelButton_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void OKButton_Click(object sender, EventArgs e)
    {
      invoiceRow.InvoiceNumber = txtInvoiceNumber.Text;
      invoiceRow.IssueDate = lookUpIssueDate.DateTime;
      invoiceRow.DueDate=lookUpDueDate.DateTime;
      invoiceRow.ClientName= txtClientName.Text;
      invoiceRow.IdCurrency = (int) lookUpCurrency.EditValue;
      if (invoiceRow.InvoiceNumber == null)
      {
        XtraMessageBox.Show(this, "Please insert an Invoice Number");
        this.DialogResult = DialogResult.None;
        return;
      }
      else if (invoiceRow.ClientName == null)
      {
        XtraMessageBox.Show(this, "Please insert a Client Name");
        this.DialogResult = DialogResult.None;
        return;
      }
      this.Close();
    }


    private void InvoiceEdit_InitNewRow(object sender, InitNewRowEventArgs e)
    {
      InvoiceDS.InvoiceItemRow newRow = InvoiceEdit.GetDataRow(e.RowHandle) as InvoiceDS.InvoiceItemRow;
      newRow.IdInvoice = invoiceRow.Id;
      
    }


    private void InvoiceEditTable_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Delete)
      {
        int[] selectedHandles = InvoiceEdit.GetSelectedRows();
        List<DataRow> toBeDeletedRows = new List<DataRow>();
        foreach (int handle in selectedHandles)
        {

          DataRow row = InvoiceEdit.GetDataRow(handle);
          if (row == null)
          {
            continue;
          }
          toBeDeletedRows.Add(row);
        }
        foreach (DataRow row in toBeDeletedRows)
        {
          row.CancelEdit();
          row.Delete();
        }
        e.Handled = true;
      }
    }

    private void InvoiceEdit_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {
      if (e.Column == colValue)
      {
        DataRowView row = e.Row as DataRowView;
        InvoiceDS.InvoiceItemRow Row = row.Row as InvoiceDS.InvoiceItemRow;
        if(Row!= null && !Row.IsQuantityNull() && !Row.IsPriceNull())
        {
          e.Value = Row.Quantity * Row.Price;
          Row.Value = (decimal)e.Value;
        }

      }
    }
  }
}
