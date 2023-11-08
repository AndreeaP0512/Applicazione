using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.Data;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.Utils.Menu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace PentruIon
{
  public partial class FrmMain : XtraForm
  {

    public FrmMain()
    {
      InitializeComponent();
      barManager.SetPopupContextMenu(gridInvoices, popupMenu);
    }

    private void bbAdd_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    {
      InvoiceDS invoiceEditDS = invoiceDS.Copy() as InvoiceDS;
      using (FrmInvoiceEdit frmInvoiceEdit = new FrmInvoiceEdit(null, invoiceEditDS))
      {
        if (frmInvoiceEdit.ShowDialog() == DialogResult.OK)
        {
          if (invoiceEditDS.HasChanges())
          {
            gridInvoices.DataSource = null;
            invoiceDS = invoiceEditDS.Copy() as InvoiceDS;
            gridInvoices.DataSource = invoiceDS.Invoice;
          }
        }
      }


    }

    private void bbEdit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    {
      InvoiceDS invoiceEditDS = invoiceDS.Copy() as InvoiceDS;
      InvoiceDS.InvoiceRow invoiceEditRow = null;
      InvoiceDS.InvoiceRow invoiceRow = viewInvoices.GetDataRow(viewInvoices.FocusedRowHandle) as InvoiceDS.InvoiceRow;
      if (invoiceRow != null)
      {
        invoiceEditRow = invoiceEditDS.Invoice.FindById(invoiceRow.Id);
      }
      if (invoiceEditRow == null)
      {
        return;
      }

      ShowInvoiceEdit(invoiceEditRow, invoiceEditDS);
    }

    private void ShowInvoiceEdit(InvoiceDS.InvoiceRow invoiceRow, InvoiceDS invoiceDSCopy)
    {
      using (FrmInvoiceEdit frmInvoiceEdit = new FrmInvoiceEdit(invoiceRow, invoiceDSCopy))
      {

        if (frmInvoiceEdit.ShowDialog() == DialogResult.OK)
        {

          if (invoiceDSCopy.HasChanges())
          {
            gridInvoices.DataSource = null;
            invoiceDS = invoiceDSCopy.Copy() as InvoiceDS;
            gridInvoices.DataSource = invoiceDS.Invoice;
          }
        }
      }

    }

    private void bbStock_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    {
      using (FrmStockChanges frmStockChanges = new FrmStockChanges(invoiceDS))
      {
        if (frmStockChanges.ShowDialog() == DialogResult.OK)
        {

        }
      }
    }

    private void bbExit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    {
      this.Close();
    }

    private void bbLoad_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    {
      InvoiceDataAccess invoiceDataAccess = new InvoiceDataAccess();

      invoiceDS = invoiceDataAccess.ReadInvoiceData();
      gridInvoices.DataSource = invoiceDS.Invoice;

      bbAdd.Enabled = true;
      bbStock.Enabled = true;
      bbEdit.Enabled = true;
      bbSave.Enabled = true;
      bbDelete.Enabled = true;

      
    }

    private void viewInvoices_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {

      if ((e.Column == colValueSum))
      {
        decimal val = 0;
        DataRowView row = e.Row as DataRowView;
        InvoiceDS.InvoiceRow invoiceRow = row.Row as InvoiceDS.InvoiceRow;
        foreach (InvoiceDS.InvoiceItemRow invoiceItemRow in invoiceRow.GetInvoiceItemRows())
        {
          val += invoiceItemRow.Value;
        }
        e.Value = val;
      }
      if (e.Column == colCurrency)
      {
        DataRowView row = e.Row as DataRowView;
        InvoiceDS.InvoiceRow invoiceRow = row.Row as InvoiceDS.InvoiceRow;
        e.Value = invoiceRow.CurrencyRow.Symbol;
      }
      if (e.Column == colValueInRef)
      {
        decimal val = 0;
        DataRowView row = e.Row as DataRowView;
        InvoiceDS.InvoiceRow invoiceRow = row.Row as InvoiceDS.InvoiceRow;
        InvoiceDS.CurrencyRateRow exchangeRow = null;
        foreach (InvoiceDS.InvoiceItemRow invoiceItemRow in invoiceRow.GetInvoiceItemRows())
        {
          val += invoiceItemRow.Value;
        }
        e.Value = val;
        foreach (InvoiceDS.CurrencyRateRow currencyRate in invoiceDS.CurrencyRate)
        {
          if (exchangeRow == null)
          {
            if (currencyRate.ExchangeDate.Date <= invoiceRow.IssueDate.Date && currencyRate.IdCurrency == invoiceRow.IdCurrency)
            {
              exchangeRow = currencyRate;
            }
          }
          else if (currencyRate.ExchangeDate.Date <= invoiceRow.IssueDate.Date && currencyRate.IdCurrency == invoiceRow.IdCurrency && currencyRate.ExchangeDate.Date >= exchangeRow.ExchangeDate.Date)
          {
            exchangeRow = currencyRate;
          }
        }
        if (exchangeRow == null)
        {
          foreach (InvoiceDS.CurrencyRateRow currencyRate in invoiceDS.CurrencyRate)
          {
            if (exchangeRow == null)
            {
              if (currencyRate.ExchangeDate.Date >= invoiceRow.IssueDate.Date && currencyRate.IdCurrency == invoiceRow.IdCurrency)
              {
                exchangeRow = currencyRate;
              }
            }
            else if (currencyRate.ExchangeDate.Date >= invoiceRow.IssueDate.Date && currencyRate.IdCurrency == invoiceRow.IdCurrency && currencyRate.ExchangeDate.Date <= exchangeRow.ExchangeDate.Date)
            {
              exchangeRow = currencyRate;
            }
          }
        }
        e.Value = String.Format("{0:0.00}", val * exchangeRow.ExchangeRate);

      }
    }

    private void bbDelete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    {
      int[] selectedHandles = viewInvoices.GetSelectedRows();
      List<DataRow> toBeDeletedRows = new List<DataRow>();
      foreach (int handle in selectedHandles)
      {

        DataRow row = viewInvoices.GetDataRow(handle);
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

    }

    private void bbSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    {

      if (invoiceDS.HasChanges())
      {
        InvoiceDS changesDS = invoiceDS.GetChanges() as InvoiceDS;
        InvoiceDataAccess invoiceDataAccess = new InvoiceDataAccess();
        invoiceDataAccess.WriteInvoiceData(changesDS);
        invoiceDS.AcceptChanges();
        invoiceDS = invoiceDataAccess.ReadInvoiceData();
        gridInvoices.DataSource = invoiceDS.Invoice;
      }
    }

    private void viewInvoices_DoubleClick(object sender, EventArgs e)
    {
      DevExpress.Utils.DXMouseEventArgs ev = e as DevExpress.Utils.DXMouseEventArgs;
      if (ev.Button == MouseButtons.Left)
      {

        GridView view = sender as GridView;
        if (view == null)
        {
          return;
        }
        GridHitInfo hitInfo = view.CalcHitInfo(view.GridControl.PointToClient(Cursor.Position));
        if (hitInfo.InRow || hitInfo.InRowCell)
        {
          InvoiceDS.InvoiceRow invoiceRow = view.GetDataRow(view.FocusedRowHandle) as InvoiceDS.InvoiceRow;
          InvoiceDS invoiceCopy = invoiceDS.Copy() as InvoiceDS;
          if (invoiceRow != null)
          {
            invoiceRow = invoiceCopy.Invoice.FindById(invoiceRow.Id);
            ShowInvoiceEdit(invoiceRow, invoiceCopy);
          }
        }
      }
    }

    private void DeleteRowButton_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    {
      InvoiceDS.InvoiceRow invoiceRow = viewInvoices.GetDataRow(viewInvoices.FocusedRowHandle) as InvoiceDS.InvoiceRow;

      if (invoiceRow != null)
      {
        invoiceRow.CancelEdit();
        invoiceRow.Delete();
      }
    }

    private void EditRowButton_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    {
      InvoiceDS invoiceEditDS = invoiceDS.Copy() as InvoiceDS;
      InvoiceDS.InvoiceRow invoiceEditRow = viewInvoices.GetDataRow(viewInvoices.FocusedRowHandle) as InvoiceDS.InvoiceRow;
      ShowInvoiceEdit(invoiceEditRow, invoiceEditDS);
    }

    private void barManager_QueryShowPopupMenu(object sender, DevExpress.XtraBars.QueryShowPopupMenuEventArgs e)
    {
      GridControl grid = e.Control as GridControl;
      if (grid == null)
        return;

      GridView view = grid.FocusedView as GridView;
      GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(e.Position));
      if (!hitInfo.InRow)
      {
        e.Cancel = true;
      }
    }

    private void viewInvoices_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
    {
      viewInvoices.Focus();
    }
  }
}
