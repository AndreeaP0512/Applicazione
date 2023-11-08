
namespace PentruIon
{
  partial class FrmInvoiceEdit
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
      this.components = new System.ComponentModel.Container();
      this.InvoiceEditTable = new DevExpress.XtraGrid.GridControl();
      this.InvoiceEdit = new DevExpress.XtraGrid.Views.Grid.GridView();
      this.colProductName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colQuantity = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colPrice = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colValue = new DevExpress.XtraGrid.Columns.GridColumn();
      this.btnOK = new DevExpress.XtraEditors.SimpleButton();
      this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
      this.txtInvoiceNumber = new DevExpress.XtraEditors.TextEdit();
      this.txtClientName = new DevExpress.XtraEditors.TextEdit();
      this.lookUpCurrency = new DevExpress.XtraEditors.LookUpEdit();
      this.currencyBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.invoiceDS = new PentruIon.InvoiceDS();
      this.lookUpDueDate = new DevExpress.XtraEditors.DateEdit();
      this.lookUpIssueDate = new DevExpress.XtraEditors.DateEdit();
      this.INLabel = new DevExpress.XtraEditors.LabelControl();
      this.CNLabel = new DevExpress.XtraEditors.LabelControl();
      this.CurLabel = new DevExpress.XtraEditors.LabelControl();
      this.DDLabel = new DevExpress.XtraEditors.LabelControl();
      this.IDLabel = new DevExpress.XtraEditors.LabelControl();
      this.invoiceEditBS = new System.Windows.Forms.BindingSource(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.InvoiceEditTable)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.InvoiceEdit)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.txtInvoiceNumber.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.txtClientName.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpCurrency.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.currencyBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.invoiceDS)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpDueDate.Properties.CalendarTimeProperties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpDueDate.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpIssueDate.Properties.CalendarTimeProperties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpIssueDate.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.invoiceEditBS)).BeginInit();
      this.SuspendLayout();
      // 
      // InvoiceEditTable
      // 
      this.InvoiceEditTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.InvoiceEditTable.Location = new System.Drawing.Point(12, 129);
      this.InvoiceEditTable.MainView = this.InvoiceEdit;
      this.InvoiceEditTable.Name = "InvoiceEditTable";
      this.InvoiceEditTable.Size = new System.Drawing.Size(774, 144);
      this.InvoiceEditTable.TabIndex = 0;
      this.InvoiceEditTable.TabStop = false;
      this.InvoiceEditTable.UseDirectXPaint = DevExpress.Utils.DefaultBoolean.False;
      this.InvoiceEditTable.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.InvoiceEdit});
      this.InvoiceEditTable.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InvoiceEditTable_KeyDown);
      // 
      // InvoiceEdit
      // 
      this.InvoiceEdit.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colProductName,
            this.colQuantity,
            this.colPrice,
            this.colValue});
      this.InvoiceEdit.GridControl = this.InvoiceEditTable;
      this.InvoiceEdit.Name = "InvoiceEdit";
      this.InvoiceEdit.OptionsSelection.MultiSelect = true;
      this.InvoiceEdit.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Top;
      this.InvoiceEdit.InitNewRow += new DevExpress.XtraGrid.Views.Grid.InitNewRowEventHandler(this.InvoiceEdit_InitNewRow);
      this.InvoiceEdit.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.InvoiceEdit_CustomUnboundColumnData);
      // 
      // colProductName
      // 
      this.colProductName.Caption = "ProductName";
      this.colProductName.FieldName = "ProductName";
      this.colProductName.Name = "colProductName";
      this.colProductName.Visible = true;
      this.colProductName.VisibleIndex = 0;
      // 
      // colQuantity
      // 
      this.colQuantity.Caption = "Quantity";
      this.colQuantity.FieldName = "Quantity";
      this.colQuantity.Name = "colQuantity";
      this.colQuantity.Visible = true;
      this.colQuantity.VisibleIndex = 1;
      // 
      // colPrice
      // 
      this.colPrice.Caption = "Price";
      this.colPrice.FieldName = "Price";
      this.colPrice.Name = "colPrice";
      this.colPrice.Visible = true;
      this.colPrice.VisibleIndex = 2;
      // 
      // colValue
      // 
      this.colValue.Caption = "Value";
      this.colValue.FieldName = "colValue";
      this.colValue.Name = "colValue";
      this.colValue.OptionsColumn.AllowEdit = false;
      this.colValue.OptionsColumn.ReadOnly = true;
      this.colValue.UnboundDataType = typeof(decimal);
      this.colValue.Visible = true;
      this.colValue.VisibleIndex = 3;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(630, 299);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.TabIndex = 1;
      this.btnOK.Text = "OK";
      this.btnOK.Click += new System.EventHandler(this.OKButton_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(711, 299);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.Click += new System.EventHandler(this.CancelButton_Click);
      // 
      // txtInvoiceNumber
      // 
      this.txtInvoiceNumber.Location = new System.Drawing.Point(97, 23);
      this.txtInvoiceNumber.Name = "txtInvoiceNumber";
      this.txtInvoiceNumber.Size = new System.Drawing.Size(100, 20);
      this.txtInvoiceNumber.TabIndex = 3;
      // 
      // txtClientName
      // 
      this.txtClientName.Location = new System.Drawing.Point(97, 67);
      this.txtClientName.Name = "txtClientName";
      this.txtClientName.Size = new System.Drawing.Size(100, 20);
      this.txtClientName.TabIndex = 4;
      // 
      // lookUpCurrency
      // 
      this.lookUpCurrency.Location = new System.Drawing.Point(474, 23);
      this.lookUpCurrency.Name = "lookUpCurrency";
      this.lookUpCurrency.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.lookUpCurrency.Properties.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("ShortName", "Currency")});
      this.lookUpCurrency.Properties.DisplayMember = "ShortName";
      this.lookUpCurrency.Properties.NullText = "";
      this.lookUpCurrency.Properties.ShowHeader = false;
      this.lookUpCurrency.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
      this.lookUpCurrency.Properties.ValueMember = "Id";
      this.lookUpCurrency.Size = new System.Drawing.Size(100, 20);
      this.lookUpCurrency.TabIndex = 5;
      // 
      // currencyBindingSource
      // 
      this.currencyBindingSource.DataMember = "Currency";
      this.currencyBindingSource.DataSource = this.invoiceDS;
      // 
      // invoiceDS
      // 
      this.invoiceDS.DataSetName = "InvoiceDS";
      this.invoiceDS.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
      // 
      // lookUpDueDate
      // 
      this.lookUpDueDate.EditValue = null;
      this.lookUpDueDate.Location = new System.Drawing.Point(279, 67);
      this.lookUpDueDate.Name = "lookUpDueDate";
      this.lookUpDueDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.lookUpDueDate.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.lookUpDueDate.Properties.DisplayFormat.FormatString = "{0:dd/MM/yyyy}";
      this.lookUpDueDate.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
      this.lookUpDueDate.Properties.EditFormat.FormatString = "{0:dd/MM/yyyy}";
      this.lookUpDueDate.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Custom;
      this.lookUpDueDate.Properties.MaskSettings.Set("mask", "dd/MM/yyyy");
      this.lookUpDueDate.Properties.UseMaskAsDisplayFormat = true;
      this.lookUpDueDate.Size = new System.Drawing.Size(102, 20);
      this.lookUpDueDate.TabIndex = 6;
      // 
      // lookUpIssueDate
      // 
      this.lookUpIssueDate.EditValue = null;
      this.lookUpIssueDate.Location = new System.Drawing.Point(279, 23);
      this.lookUpIssueDate.Name = "lookUpIssueDate";
      this.lookUpIssueDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.lookUpIssueDate.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.lookUpIssueDate.Properties.DisplayFormat.FormatString = "{0:dd/MM/yyyy}";
      this.lookUpIssueDate.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
      this.lookUpIssueDate.Properties.EditFormat.FormatString = "{0:dd/MM/yyyy}";
      this.lookUpIssueDate.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Custom;
      this.lookUpIssueDate.Properties.MaskSettings.Set("mask", "dd/MM/yyyy");
      this.lookUpIssueDate.Properties.UseMaskAsDisplayFormat = true;
      this.lookUpIssueDate.Size = new System.Drawing.Size(102, 20);
      this.lookUpIssueDate.TabIndex = 7;
      // 
      // INLabel
      // 
      this.INLabel.Location = new System.Drawing.Point(12, 26);
      this.INLabel.Name = "INLabel";
      this.INLabel.Size = new System.Drawing.Size(79, 13);
      this.INLabel.TabIndex = 8;
      this.INLabel.Text = "Invoice Number:";
      // 
      // CNLabel
      // 
      this.CNLabel.Location = new System.Drawing.Point(12, 70);
      this.CNLabel.Name = "CNLabel";
      this.CNLabel.Size = new System.Drawing.Size(61, 13);
      this.CNLabel.TabIndex = 9;
      this.CNLabel.Text = "Client Name:";
      // 
      // CurLabel
      // 
      this.CurLabel.Location = new System.Drawing.Point(405, 26);
      this.CurLabel.Name = "CurLabel";
      this.CurLabel.Size = new System.Drawing.Size(48, 13);
      this.CurLabel.TabIndex = 10;
      this.CurLabel.Text = "Currency:";
      // 
      // DDLabel
      // 
      this.DDLabel.Location = new System.Drawing.Point(210, 70);
      this.DDLabel.Name = "DDLabel";
      this.DDLabel.Size = new System.Drawing.Size(49, 13);
      this.DDLabel.TabIndex = 11;
      this.DDLabel.Text = "Due Date:";
      // 
      // IDLabel
      // 
      this.IDLabel.Location = new System.Drawing.Point(210, 26);
      this.IDLabel.Name = "IDLabel";
      this.IDLabel.Size = new System.Drawing.Size(56, 13);
      this.IDLabel.TabIndex = 12;
      this.IDLabel.Text = "Issue Date:";
      // 
      // FrmInvoiceEdit
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(798, 334);
      this.Controls.Add(this.IDLabel);
      this.Controls.Add(this.DDLabel);
      this.Controls.Add(this.CurLabel);
      this.Controls.Add(this.CNLabel);
      this.Controls.Add(this.INLabel);
      this.Controls.Add(this.lookUpIssueDate);
      this.Controls.Add(this.lookUpDueDate);
      this.Controls.Add(this.lookUpCurrency);
      this.Controls.Add(this.txtClientName);
      this.Controls.Add(this.txtInvoiceNumber);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.InvoiceEditTable);
      this.MinimumSize = new System.Drawing.Size(800, 366);
      this.Name = "FrmInvoiceEdit";
      this.ShowInTaskbar = false;
      this.Text = "Invoice Edit";
      ((System.ComponentModel.ISupportInitialize)(this.InvoiceEditTable)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.InvoiceEdit)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.txtInvoiceNumber.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.txtClientName.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpCurrency.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.currencyBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.invoiceDS)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpDueDate.Properties.CalendarTimeProperties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpDueDate.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpIssueDate.Properties.CalendarTimeProperties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpIssueDate.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.invoiceEditBS)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DevExpress.XtraGrid.GridControl InvoiceEditTable;
    private DevExpress.XtraGrid.Views.Grid.GridView InvoiceEdit;
    private DevExpress.XtraEditors.SimpleButton btnOK;
    private DevExpress.XtraEditors.SimpleButton btnCancel;
    private DevExpress.XtraEditors.TextEdit txtInvoiceNumber;
    private DevExpress.XtraEditors.TextEdit txtClientName;
    private DevExpress.XtraEditors.LookUpEdit lookUpCurrency;
    private DevExpress.XtraEditors.DateEdit lookUpDueDate;
    private DevExpress.XtraEditors.DateEdit lookUpIssueDate;
    private DevExpress.XtraEditors.LabelControl INLabel;
    private DevExpress.XtraEditors.LabelControl CNLabel;
    private DevExpress.XtraEditors.LabelControl CurLabel;
    private DevExpress.XtraEditors.LabelControl DDLabel;
    private DevExpress.XtraEditors.LabelControl IDLabel;
    private DevExpress.XtraGrid.Columns.GridColumn colProductName;
    private DevExpress.XtraGrid.Columns.GridColumn colQuantity;
    private DevExpress.XtraGrid.Columns.GridColumn colPrice;
    private DevExpress.XtraGrid.Columns.GridColumn colValue;
    private System.Windows.Forms.BindingSource currencyBindingSource;
    private InvoiceDS invoiceDS;
    private System.Windows.Forms.BindingSource invoiceEditBS;
  }
}