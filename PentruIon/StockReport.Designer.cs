
namespace PentruIon
{
  partial class StockReport
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

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
      this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
      this.Detail = new DevExpress.XtraReports.UI.DetailBand();
      this.xrTable1 = new DevExpress.XtraReports.UI.XRTable();
      this.xrTableRow1 = new DevExpress.XtraReports.UI.XRTableRow();
      this.xrTableCellDate = new DevExpress.XtraReports.UI.XRTableCell();
      this.xrTableCellIQ = new DevExpress.XtraReports.UI.XRTableCell();
      this.xrTableCellCQ = new DevExpress.XtraReports.UI.XRTableCell();
      this.reportDS = new PentruIon.ReportDS();
      this.ReportHeader = new DevExpress.XtraReports.UI.ReportHeaderBand();
      this.Title = new DevExpress.XtraReports.UI.XRLabel();
      this.PageHeader = new DevExpress.XtraReports.UI.PageHeaderBand();
      this.xrTable2 = new DevExpress.XtraReports.UI.XRTable();
      this.xrTableRow2 = new DevExpress.XtraReports.UI.XRTableRow();
      this.xrHeaderCellDate = new DevExpress.XtraReports.UI.XRTableCell();
      this.xrHeaderCellIQ = new DevExpress.XtraReports.UI.XRTableCell();
      this.xrHeaderCellCQ = new DevExpress.XtraReports.UI.XRTableCell();
      ((System.ComponentModel.ISupportInitialize)(this.xrTable1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.reportDS)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.xrTable2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
      // 
      // TopMargin
      // 
      this.TopMargin.HeightF = 48.95833F;
      this.TopMargin.Name = "TopMargin";
      // 
      // BottomMargin
      // 
      this.BottomMargin.HeightF = 0F;
      this.BottomMargin.Name = "BottomMargin";
      // 
      // Detail
      // 
      this.Detail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrTable1});
      this.Detail.HeightF = 25F;
      this.Detail.Name = "Detail";
      // 
      // xrTable1
      // 
      this.xrTable1.LocationFloat = new DevExpress.Utils.PointFloat(10.00001F, 0F);
      this.xrTable1.Name = "xrTable1";
      this.xrTable1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 96F);
      this.xrTable1.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] {
            this.xrTableRow1});
      this.xrTable1.SizeF = new System.Drawing.SizeF(630F, 25F);
      // 
      // xrTableRow1
      // 
      this.xrTableRow1.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.xrTableCellDate,
            this.xrTableCellIQ,
            this.xrTableCellCQ});
      this.xrTableRow1.Name = "xrTableRow1";
      this.xrTableRow1.Weight = 1D;
      // 
      // xrTableCellDate
      // 
      this.xrTableCellDate.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
      this.xrTableCellDate.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Date]")});
      this.xrTableCellDate.Multiline = true;
      this.xrTableCellDate.Name = "xrTableCellDate";
      this.xrTableCellDate.StylePriority.UseBorders = false;
      this.xrTableCellDate.StylePriority.UseTextAlignment = false;
      this.xrTableCellDate.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
      this.xrTableCellDate.TextFormatString = "{0:dd-MMM-yy}";
      this.xrTableCellDate.Weight = 1D;
      // 
      // xrTableCellIQ
      // 
      this.xrTableCellIQ.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
      this.xrTableCellIQ.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[InitialQuantity]")});
      this.xrTableCellIQ.Multiline = true;
      this.xrTableCellIQ.Name = "xrTableCellIQ";
      this.xrTableCellIQ.StylePriority.UseBorders = false;
      this.xrTableCellIQ.StylePriority.UseTextAlignment = false;
      this.xrTableCellIQ.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
      this.xrTableCellIQ.Weight = 1D;
      // 
      // xrTableCellCQ
      // 
      this.xrTableCellCQ.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
      this.xrTableCellCQ.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[CurrentQuantity]")});
      this.xrTableCellCQ.Multiline = true;
      this.xrTableCellCQ.Name = "xrTableCellCQ";
      this.xrTableCellCQ.StylePriority.UseBorders = false;
      this.xrTableCellCQ.StylePriority.UseTextAlignment = false;
      this.xrTableCellCQ.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
      this.xrTableCellCQ.Weight = 1D;
      // 
      // reportDS
      // 
      this.reportDS.DataSetName = "ReportDS";
      this.reportDS.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
      // 
      // ReportHeader
      // 
      this.ReportHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.Title});
      this.ReportHeader.Name = "ReportHeader";
      // 
      // Title
      // 
      this.Title.BackColor = System.Drawing.Color.LawnGreen;
      this.Title.Font = new DevExpress.Drawing.DXFont("MingLiU_HKSCS-ExtB", 27.75F, DevExpress.Drawing.DXFontStyle.Italic, DevExpress.Drawing.DXGraphicsUnit.Point, new DevExpress.Drawing.DXFontAdditionalProperty[] {new DevExpress.Drawing.DXFontAdditionalProperty("GdiCharSet", ((byte)(0)))});
      this.Title.ForeColor = System.Drawing.Color.Tomato;
      this.Title.LocationFloat = new DevExpress.Utils.PointFloat(10.00001F, 12.5F);
      this.Title.Multiline = true;
      this.Title.Name = "Title";
      this.Title.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
      this.Title.SizeF = new System.Drawing.SizeF(630F, 65.70835F);
      this.Title.StylePriority.UseBackColor = false;
      this.Title.StylePriority.UseFont = false;
      this.Title.StylePriority.UseForeColor = false;
      this.Title.StylePriority.UseTextAlignment = false;
      this.Title.Text = "Title";
      this.Title.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
      // 
      // PageHeader
      // 
      this.PageHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrTable2});
      this.PageHeader.HeightF = 35.00001F;
      this.PageHeader.Name = "PageHeader";
      // 
      // xrTable2
      // 
      this.xrTable2.LocationFloat = new DevExpress.Utils.PointFloat(10.00001F, 10.00001F);
      this.xrTable2.Name = "xrTable2";
      this.xrTable2.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 96F);
      this.xrTable2.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] {
            this.xrTableRow2});
      this.xrTable2.SizeF = new System.Drawing.SizeF(630F, 25F);
      // 
      // xrTableRow2
      // 
      this.xrTableRow2.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.xrHeaderCellDate,
            this.xrHeaderCellIQ,
            this.xrHeaderCellCQ});
      this.xrTableRow2.Name = "xrTableRow2";
      this.xrTableRow2.Weight = 1D;
      // 
      // xrHeaderCellDate
      // 
      this.xrHeaderCellDate.Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
      this.xrHeaderCellDate.Multiline = true;
      this.xrHeaderCellDate.Name = "xrHeaderCellDate";
      this.xrHeaderCellDate.StylePriority.UseBorders = false;
      this.xrHeaderCellDate.StylePriority.UseTextAlignment = false;
      this.xrHeaderCellDate.Text = "Date";
      this.xrHeaderCellDate.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
      this.xrHeaderCellDate.Weight = 1D;
      // 
      // xrHeaderCellIQ
      // 
      this.xrHeaderCellIQ.Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
      this.xrHeaderCellIQ.Multiline = true;
      this.xrHeaderCellIQ.Name = "xrHeaderCellIQ";
      this.xrHeaderCellIQ.StylePriority.UseBorders = false;
      this.xrHeaderCellIQ.StylePriority.UseTextAlignment = false;
      this.xrHeaderCellIQ.Text = "Initial Quantity";
      this.xrHeaderCellIQ.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
      this.xrHeaderCellIQ.Weight = 1D;
      // 
      // xrHeaderCellCQ
      // 
      this.xrHeaderCellCQ.Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
      this.xrHeaderCellCQ.Multiline = true;
      this.xrHeaderCellCQ.Name = "xrHeaderCellCQ";
      this.xrHeaderCellCQ.StylePriority.UseBorders = false;
      this.xrHeaderCellCQ.StylePriority.UseTextAlignment = false;
      this.xrHeaderCellCQ.Text = "Current Quantity";
      this.xrHeaderCellCQ.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
      this.xrHeaderCellCQ.Weight = 1D;
      // 
      // StockReport
      // 
      this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.BottomMargin,
            this.Detail,
            this.ReportHeader,
            this.PageHeader});
      this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.reportDS});
      this.DataMember = "Report";
      this.DataSource = this.reportDS;
      this.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
      this.Margins = new DevExpress.Drawing.DXMargins(100, 100, 49, 0);
      this.Version = "21.2";
      ((System.ComponentModel.ISupportInitialize)(this.xrTable1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.reportDS)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.xrTable2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

    }

    #endregion

    private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
    private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
    private DevExpress.XtraReports.UI.DetailBand Detail;
    private DevExpress.XtraReports.UI.XRTable xrTable1;
    private DevExpress.XtraReports.UI.XRTableRow xrTableRow1;
    private DevExpress.XtraReports.UI.XRTableCell xrTableCellDate;
    private DevExpress.XtraReports.UI.XRTableCell xrTableCellIQ;
    private DevExpress.XtraReports.UI.XRTableCell xrTableCellCQ;
    private ReportDS reportDS;
    private DevExpress.XtraReports.UI.ReportHeaderBand ReportHeader;
    private DevExpress.XtraReports.UI.XRLabel Title;
    private DevExpress.XtraReports.UI.PageHeaderBand PageHeader;
    private DevExpress.XtraReports.UI.XRTable xrTable2;
    private DevExpress.XtraReports.UI.XRTableRow xrTableRow2;
    private DevExpress.XtraReports.UI.XRTableCell xrHeaderCellDate;
    private DevExpress.XtraReports.UI.XRTableCell xrHeaderCellIQ;
    private DevExpress.XtraReports.UI.XRTableCell xrHeaderCellCQ;
  }
}
