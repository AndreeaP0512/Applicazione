
namespace PentruIon
{
  partial class FrmStockChanges
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmStockChanges));
      this.btnOK = new DevExpress.XtraEditors.SimpleButton();
      this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
      this.SDLabel = new DevExpress.XtraEditors.LabelControl();
      this.EDLabel = new DevExpress.XtraEditors.LabelControl();
      this.PNLabel = new DevExpress.XtraEditors.LabelControl();
      this.lookUpStartDate = new DevExpress.XtraEditors.DateEdit();
      this.lookUpEndDate = new DevExpress.XtraEditors.DateEdit();
      this.lookUpProductName = new DevExpress.XtraEditors.LookUpEdit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpStartDate.Properties.CalendarTimeProperties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpStartDate.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpEndDate.Properties.CalendarTimeProperties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpEndDate.Properties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpProductName.Properties)).BeginInit();
      this.SuspendLayout();
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(297, 65);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(70, 30);
      this.btnOK.TabIndex = 0;
      this.btnOK.Text = "OK";
      this.btnOK.Click += new System.EventHandler(this.OKButton_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(373, 65);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(70, 30);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.Click += new System.EventHandler(this.CancelButton_Click);
      // 
      // SDLabel
      // 
      this.SDLabel.Location = new System.Drawing.Point(48, 19);
      this.SDLabel.Name = "SDLabel";
      this.SDLabel.Size = new System.Drawing.Size(54, 13);
      this.SDLabel.TabIndex = 2;
      this.SDLabel.Text = "Start Date:";
      // 
      // EDLabel
      // 
      this.EDLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.EDLabel.Location = new System.Drawing.Point(289, 19);
      this.EDLabel.Name = "EDLabel";
      this.EDLabel.Size = new System.Drawing.Size(48, 13);
      this.EDLabel.TabIndex = 3;
      this.EDLabel.Text = "End Date:";
      // 
      // PNLabel
      // 
      this.PNLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.PNLabel.Location = new System.Drawing.Point(48, 73);
      this.PNLabel.Name = "PNLabel";
      this.PNLabel.Size = new System.Drawing.Size(71, 13);
      this.PNLabel.TabIndex = 4;
      this.PNLabel.Text = "Product Name:";
      // 
      // lookUpStartDate
      // 
      this.lookUpStartDate.EditValue = null;
      this.lookUpStartDate.Location = new System.Drawing.Point(125, 16);
      this.lookUpStartDate.Name = "lookUpStartDate";
      this.lookUpStartDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.lookUpStartDate.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.lookUpStartDate.Properties.DisplayFormat.FormatString = "{0:dd/MM/yyyy}";
      this.lookUpStartDate.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
      this.lookUpStartDate.Properties.EditFormat.FormatString = "{0:dd/MM/yyyy}";
      this.lookUpStartDate.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Custom;
      this.lookUpStartDate.Properties.MaskSettings.Set("mask", "dd/MM/yyyy");
      this.lookUpStartDate.Properties.UseMaskAsDisplayFormat = true;
      this.lookUpStartDate.Size = new System.Drawing.Size(100, 20);
      this.lookUpStartDate.TabIndex = 5;
      // 
      // lookUpEndDate
      // 
      this.lookUpEndDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.lookUpEndDate.EditValue = null;
      this.lookUpEndDate.Location = new System.Drawing.Point(343, 16);
      this.lookUpEndDate.Name = "lookUpEndDate";
      this.lookUpEndDate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.lookUpEndDate.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.lookUpEndDate.Properties.DisplayFormat.FormatString = "{0:dd/MM/yyyy}";
      this.lookUpEndDate.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
      this.lookUpEndDate.Properties.EditFormat.FormatString = "{0:dd/MM/yyyy}";
      this.lookUpEndDate.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Custom;
      this.lookUpEndDate.Properties.MaskSettings.Set("mask", "dd/MM/yyyy");
      this.lookUpEndDate.Properties.UseMaskAsDisplayFormat = true;
      this.lookUpEndDate.Size = new System.Drawing.Size(100, 20);
      this.lookUpEndDate.TabIndex = 6;
      // 
      // lookUpProductName
      // 
      this.lookUpProductName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lookUpProductName.Location = new System.Drawing.Point(125, 70);
      this.lookUpProductName.Name = "lookUpProductName";
      this.lookUpProductName.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
      this.lookUpProductName.Properties.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("ProductName", "ProductName")});
      this.lookUpProductName.Properties.DisplayMember = "ProductName";
      this.lookUpProductName.Properties.NullText = "";
      this.lookUpProductName.Properties.ValueMember = "ProductName";
      this.lookUpProductName.Size = new System.Drawing.Size(100, 20);
      this.lookUpProductName.TabIndex = 7;
      // 
      // FrmStockChanges
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(455, 107);
      this.Controls.Add(this.lookUpProductName);
      this.Controls.Add(this.lookUpEndDate);
      this.Controls.Add(this.lookUpStartDate);
      this.Controls.Add(this.PNLabel);
      this.Controls.Add(this.EDLabel);
      this.Controls.Add(this.SDLabel);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.IconOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("FrmStockChanges.IconOptions.LargeImage")));
      this.MaximumSize = new System.Drawing.Size(457, 139);
      this.MinimumSize = new System.Drawing.Size(457, 139);
      this.Name = "FrmStockChanges";
      this.Text = "FrmStockChanges";
      ((System.ComponentModel.ISupportInitialize)(this.lookUpStartDate.Properties.CalendarTimeProperties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpStartDate.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpEndDate.Properties.CalendarTimeProperties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpEndDate.Properties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.lookUpProductName.Properties)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private DevExpress.XtraEditors.SimpleButton btnOK;
    private DevExpress.XtraEditors.SimpleButton btnCancel;
    private DevExpress.XtraEditors.LabelControl SDLabel;
    private DevExpress.XtraEditors.LabelControl EDLabel;
    private DevExpress.XtraEditors.LabelControl PNLabel;
    private DevExpress.XtraEditors.DateEdit lookUpStartDate;
    private DevExpress.XtraEditors.DateEdit lookUpEndDate;
    private DevExpress.XtraEditors.LookUpEdit lookUpProductName;
  }
}