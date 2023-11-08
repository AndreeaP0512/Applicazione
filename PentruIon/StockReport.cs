using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace PentruIon
{
  public partial class StockReport : DevExpress.XtraReports.UI.XtraReport
  {
    
    public StockReport(string ProductName)
    {
      InitializeComponent();
      Title.Text = ProductName + " Stock Modfication";
    }

    
  }
}
