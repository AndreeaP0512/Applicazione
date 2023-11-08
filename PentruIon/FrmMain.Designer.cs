
namespace PentruIon
{
    partial class FrmMain
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
      DevExpress.XtraGrid.GridLevelNode gridLevelNode1 = new DevExpress.XtraGrid.GridLevelNode();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
      this.viewInvoiceItems = new DevExpress.XtraGrid.Views.Grid.GridView();
      this.colProductName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colQuantity = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colPrice = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colValue = new DevExpress.XtraGrid.Columns.GridColumn();
      this.gridInvoices = new DevExpress.XtraGrid.GridControl();
      this.viewInvoices = new DevExpress.XtraGrid.Views.Grid.GridView();
      this.colInvoiceNumber = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colClientName = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colIssueDate = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colDueDate = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colValueSum = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colCurrency = new DevExpress.XtraGrid.Columns.GridColumn();
      this.colValueInRef = new DevExpress.XtraGrid.Columns.GridColumn();
      this.barManager = new DevExpress.XtraBars.BarManager(this.components);
      this.MenuBar = new DevExpress.XtraBars.Bar();
      this.MBDatabase = new DevExpress.XtraBars.BarSubItem();
      this.bbLoad = new DevExpress.XtraBars.BarButtonItem();
      this.bbSave = new DevExpress.XtraBars.BarButtonItem();
      this.bs1 = new DevExpress.XtraBars.BarStaticItem();
      this.bbAdd = new DevExpress.XtraBars.BarButtonItem();
      this.bbEdit = new DevExpress.XtraBars.BarButtonItem();
      this.bbDelete = new DevExpress.XtraBars.BarButtonItem();
      this.bs2 = new DevExpress.XtraBars.BarStaticItem();
      this.bbExit = new DevExpress.XtraBars.BarButtonItem();
      this.MBReports = new DevExpress.XtraBars.BarSubItem();
      this.bbStock = new DevExpress.XtraBars.BarButtonItem();
      this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
      this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
      this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
      this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
      this.barSubItem1 = new DevExpress.XtraBars.BarSubItem();
      this.barSubItem2 = new DevExpress.XtraBars.BarSubItem();
      this.EditRowButton = new DevExpress.XtraBars.BarButtonItem();
      this.DeleteRowButton = new DevExpress.XtraBars.BarButtonItem();
      this.invoiceDS = new PentruIon.InvoiceDS();
      this.ApplicazioneMenu = new DevExpress.XtraBars.Ribbon.ApplicationMenu(this.components);
      this.barSubItem4 = new DevExpress.XtraBars.BarSubItem();
      this.bar2 = new DevExpress.XtraBars.Bar();
      this.bar3 = new DevExpress.XtraBars.Bar();
      this.bar4 = new DevExpress.XtraBars.Bar();
      this.behaviorManager = new DevExpress.Utils.Behaviors.BehaviorManager(this.components);
      this.invoiceDSBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.popupMenu = new DevExpress.XtraBars.PopupMenu(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.viewInvoiceItems)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.gridInvoices)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.viewInvoices)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.barManager)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.invoiceDS)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.ApplicazioneMenu)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.behaviorManager)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.invoiceDSBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.popupMenu)).BeginInit();
      this.SuspendLayout();
      // 
      // viewInvoiceItems
      // 
      this.viewInvoiceItems.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colProductName,
            this.colQuantity,
            this.colPrice,
            this.colValue});
      this.viewInvoiceItems.GridControl = this.gridInvoices;
      this.viewInvoiceItems.Name = "viewInvoiceItems";
      this.viewInvoiceItems.OptionsBehavior.ReadOnly = true;
      this.viewInvoiceItems.OptionsDetail.EnableMasterViewMode = false;
      this.viewInvoiceItems.OptionsDetail.ShowDetailTabs = false;
      this.viewInvoiceItems.ViewCaption = "Invoice Items";
      // 
      // colProductName
      // 
      this.colProductName.Caption = "Product Name";
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
      this.colValue.FieldName = "Value";
      this.colValue.Name = "colValue";
      this.colValue.Visible = true;
      this.colValue.VisibleIndex = 3;
      // 
      // gridInvoices
      // 
      this.gridInvoices.Dock = System.Windows.Forms.DockStyle.Fill;
      gridLevelNode1.LevelTemplate = this.viewInvoiceItems;
      gridLevelNode1.RelationName = "FK_Invoice_InvoiceItem";
      this.gridInvoices.LevelTree.Nodes.AddRange(new DevExpress.XtraGrid.GridLevelNode[] {
            gridLevelNode1});
      this.gridInvoices.Location = new System.Drawing.Point(0, 20);
      this.gridInvoices.MainView = this.viewInvoices;
      this.gridInvoices.MenuManager = this.barManager;
      this.gridInvoices.MinimumSize = new System.Drawing.Size(800, 366);
      this.gridInvoices.Name = "gridInvoices";
      this.gridInvoices.Size = new System.Drawing.Size(800, 409);
      this.gridInvoices.TabIndex = 0;
      this.gridInvoices.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.viewInvoices,
            this.viewInvoiceItems});
      // 
      // viewInvoices
      // 
      this.viewInvoices.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colInvoiceNumber,
            this.colClientName,
            this.colIssueDate,
            this.colDueDate,
            this.colValueSum,
            this.colCurrency,
            this.colValueInRef});
      this.viewInvoices.GridControl = this.gridInvoices;
      this.viewInvoices.Name = "viewInvoices";
      this.viewInvoices.OptionsBehavior.ReadOnly = true;
      this.viewInvoices.OptionsSelection.MultiSelect = true;
      this.viewInvoices.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.viewInvoices_PopupMenuShowing);
      this.viewInvoices.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.viewInvoices_CustomUnboundColumnData);
      this.viewInvoices.DoubleClick += new System.EventHandler(this.viewInvoices_DoubleClick);
      // 
      // colInvoiceNumber
      // 
      this.colInvoiceNumber.Caption = "Invoice Number";
      this.colInvoiceNumber.FieldName = "InvoiceNumber";
      this.colInvoiceNumber.Name = "colInvoiceNumber";
      this.colInvoiceNumber.Visible = true;
      this.colInvoiceNumber.VisibleIndex = 0;
      // 
      // colClientName
      // 
      this.colClientName.Caption = "Client Name";
      this.colClientName.FieldName = "ClientName";
      this.colClientName.Name = "colClientName";
      this.colClientName.Visible = true;
      this.colClientName.VisibleIndex = 1;
      // 
      // colIssueDate
      // 
      this.colIssueDate.Caption = "Issue Date";
      this.colIssueDate.DisplayFormat.FormatString = "{0:dd/MM/yyyy}";
      this.colIssueDate.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
      this.colIssueDate.FieldName = "IssueDate";
      this.colIssueDate.Name = "colIssueDate";
      this.colIssueDate.Visible = true;
      this.colIssueDate.VisibleIndex = 2;
      // 
      // colDueDate
      // 
      this.colDueDate.Caption = "Due Date";
      this.colDueDate.DisplayFormat.FormatString = "{0:dd/MM/yyyy}";
      this.colDueDate.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
      this.colDueDate.FieldName = "DueDate";
      this.colDueDate.Name = "colDueDate";
      this.colDueDate.Visible = true;
      this.colDueDate.VisibleIndex = 3;
      // 
      // colValueSum
      // 
      this.colValueSum.Caption = "Value";
      this.colValueSum.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.colValueSum.FieldName = "Value";
      this.colValueSum.Name = "colValueSum";
      this.colValueSum.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
      this.colValueSum.Visible = true;
      this.colValueSum.VisibleIndex = 4;
      // 
      // colCurrency
      // 
      this.colCurrency.Caption = "Currency";
      this.colCurrency.FieldName = "Currency";
      this.colCurrency.Name = "colCurrency";
      this.colCurrency.UnboundType = DevExpress.Data.UnboundColumnType.String;
      this.colCurrency.Visible = true;
      this.colCurrency.VisibleIndex = 5;
      // 
      // colValueInRef
      // 
      this.colValueInRef.Caption = "Value In Reference Currency";
      this.colValueInRef.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.colValueInRef.FieldName = "ValueInRef";
      this.colValueInRef.Name = "colValueInRef";
      this.colValueInRef.UnboundType = DevExpress.Data.UnboundColumnType.String;
      this.colValueInRef.Visible = true;
      this.colValueInRef.VisibleIndex = 6;
      // 
      // barManager
      // 
      this.barManager.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.MenuBar});
      this.barManager.DockControls.Add(this.barDockControlTop);
      this.barManager.DockControls.Add(this.barDockControlBottom);
      this.barManager.DockControls.Add(this.barDockControlLeft);
      this.barManager.DockControls.Add(this.barDockControlRight);
      this.barManager.Form = this;
      this.barManager.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.barSubItem1,
            this.barSubItem2,
            this.MBDatabase,
            this.MBReports,
            this.bbLoad,
            this.bbSave,
            this.bs1,
            this.bbAdd,
            this.bbEdit,
            this.bbDelete,
            this.bs2,
            this.bbExit,
            this.bbStock,
            this.EditRowButton,
            this.DeleteRowButton});
      this.barManager.MaxItemId = 16;
      this.barManager.QueryShowPopupMenu += new DevExpress.XtraBars.QueryShowPopupMenuEventHandler(this.barManager_QueryShowPopupMenu);
      // 
      // MenuBar
      // 
      this.MenuBar.BarName = "MenuBar";
      this.MenuBar.DockCol = 0;
      this.MenuBar.DockRow = 0;
      this.MenuBar.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
      this.MenuBar.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.MBDatabase),
            new DevExpress.XtraBars.LinkPersistInfo(this.MBReports)});
      this.MenuBar.Text = "MenuBar";
      // 
      // MBDatabase
      // 
      this.MBDatabase.Caption = "Database";
      this.MBDatabase.Id = 2;
      this.MBDatabase.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("MBDatabase.ImageOptions.Image")));
      this.MBDatabase.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("MBDatabase.ImageOptions.LargeImage")));
      this.MBDatabase.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.bbLoad),
            new DevExpress.XtraBars.LinkPersistInfo(this.bbSave),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.bs1, DevExpress.XtraBars.BarItemPaintStyle.Caption),
            new DevExpress.XtraBars.LinkPersistInfo(this.bbAdd),
            new DevExpress.XtraBars.LinkPersistInfo(this.bbEdit),
            new DevExpress.XtraBars.LinkPersistInfo(this.bbDelete),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.bs2, DevExpress.XtraBars.BarItemPaintStyle.Caption),
            new DevExpress.XtraBars.LinkPersistInfo(this.bbExit)});
      this.MBDatabase.Name = "MBDatabase";
      // 
      // bbLoad
      // 
      this.bbLoad.Caption = "Load";
      this.bbLoad.Id = 5;
      this.bbLoad.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("bbLoad.ImageOptions.Image")));
      this.bbLoad.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("bbLoad.ImageOptions.LargeImage")));
      this.bbLoad.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L));
      this.bbLoad.Name = "bbLoad";
      this.bbLoad.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.bbLoad_ItemClick);
      // 
      // bbSave
      // 
      this.bbSave.Caption = "Save";
      this.bbSave.Enabled = false;
      this.bbSave.Id = 6;
      this.bbSave.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("bbSave.ImageOptions.Image")));
      this.bbSave.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("bbSave.ImageOptions.LargeImage")));
      this.bbSave.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S));
      this.bbSave.Name = "bbSave";
      this.bbSave.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.bbSave_ItemClick);
      // 
      // bs1
      // 
      this.bs1.Caption = "-";
      this.bs1.Id = 7;
      this.bs1.Name = "bs1";
      // 
      // bbAdd
      // 
      this.bbAdd.Caption = "Add";
      this.bbAdd.Enabled = false;
      this.bbAdd.Id = 8;
      this.bbAdd.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("bbAdd.ImageOptions.Image")));
      this.bbAdd.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("bbAdd.ImageOptions.LargeImage")));
      this.bbAdd.Name = "bbAdd";
      this.bbAdd.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.bbAdd_ItemClick);
      // 
      // bbEdit
      // 
      this.bbEdit.Caption = "Edit";
      this.bbEdit.Enabled = false;
      this.bbEdit.Id = 9;
      this.bbEdit.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("bbEdit.ImageOptions.Image")));
      this.bbEdit.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("bbEdit.ImageOptions.LargeImage")));
      this.bbEdit.Name = "bbEdit";
      this.bbEdit.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.bbEdit_ItemClick);
      // 
      // bbDelete
      // 
      this.bbDelete.Caption = "Delete";
      this.bbDelete.Enabled = false;
      this.bbDelete.Id = 10;
      this.bbDelete.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("bbDelete.ImageOptions.Image")));
      this.bbDelete.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("bbDelete.ImageOptions.LargeImage")));
      this.bbDelete.ItemShortcut = new DevExpress.XtraBars.BarShortcut(System.Windows.Forms.Keys.Delete);
      this.bbDelete.Name = "bbDelete";
      this.bbDelete.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.bbDelete_ItemClick);
      // 
      // bs2
      // 
      this.bs2.Caption = "-";
      this.bs2.Id = 11;
      this.bs2.Name = "bs2";
      // 
      // bbExit
      // 
      this.bbExit.Caption = "Exit";
      this.bbExit.Id = 12;
      this.bbExit.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("bbExit.ImageOptions.Image")));
      this.bbExit.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("bbExit.ImageOptions.LargeImage")));
      this.bbExit.Name = "bbExit";
      this.bbExit.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.bbExit_ItemClick);
      // 
      // MBReports
      // 
      this.MBReports.Caption = "Reports";
      this.MBReports.Id = 4;
      this.MBReports.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.bbStock)});
      this.MBReports.Name = "MBReports";
      // 
      // bbStock
      // 
      this.bbStock.Caption = "Stock Changes";
      this.bbStock.Enabled = false;
      this.bbStock.Id = 13;
      this.bbStock.Name = "bbStock";
      this.bbStock.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.bbStock_ItemClick);
      // 
      // barDockControlTop
      // 
      this.barDockControlTop.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.barDockControlTop.Appearance.Options.UseFont = true;
      this.barDockControlTop.CausesValidation = false;
      this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
      this.barDockControlTop.Manager = this.barManager;
      this.barDockControlTop.Size = new System.Drawing.Size(798, 20);
      // 
      // barDockControlBottom
      // 
      this.barDockControlBottom.CausesValidation = false;
      this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.barDockControlBottom.Location = new System.Drawing.Point(0, 429);
      this.barDockControlBottom.Manager = this.barManager;
      this.barDockControlBottom.Size = new System.Drawing.Size(798, 0);
      // 
      // barDockControlLeft
      // 
      this.barDockControlLeft.CausesValidation = false;
      this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
      this.barDockControlLeft.Location = new System.Drawing.Point(0, 20);
      this.barDockControlLeft.Manager = this.barManager;
      this.barDockControlLeft.Size = new System.Drawing.Size(0, 409);
      // 
      // barDockControlRight
      // 
      this.barDockControlRight.CausesValidation = false;
      this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
      this.barDockControlRight.Location = new System.Drawing.Point(798, 20);
      this.barDockControlRight.Manager = this.barManager;
      this.barDockControlRight.Size = new System.Drawing.Size(0, 409);
      // 
      // barSubItem1
      // 
      this.barSubItem1.Caption = "Database";
      this.barSubItem1.Id = 0;
      this.barSubItem1.Name = "barSubItem1";
      // 
      // barSubItem2
      // 
      this.barSubItem2.Caption = "Reports";
      this.barSubItem2.Id = 1;
      this.barSubItem2.Name = "barSubItem2";
      // 
      // EditRowButton
      // 
      this.EditRowButton.Caption = "Edit Row";
      this.EditRowButton.Id = 14;
      this.EditRowButton.Name = "EditRowButton";
      this.EditRowButton.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.EditRowButton_ItemClick);
      // 
      // DeleteRowButton
      // 
      this.DeleteRowButton.Caption = "Delete Row";
      this.DeleteRowButton.Id = 15;
      this.DeleteRowButton.Name = "DeleteRowButton";
      this.DeleteRowButton.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.DeleteRowButton_ItemClick);
      // 
      // invoiceDS
      // 
      this.invoiceDS.DataSetName = "InvoiceDS";
      this.invoiceDS.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
      // 
      // ApplicazioneMenu
      // 
      this.ApplicazioneMenu.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barSubItem1),
            new DevExpress.XtraBars.LinkPersistInfo(this.barSubItem2)});
      this.ApplicazioneMenu.Name = "ApplicazioneMenu";
      // 
      // barSubItem4
      // 
      this.barSubItem4.Caption = "Reports";
      this.barSubItem4.Id = 3;
      this.barSubItem4.Name = "barSubItem4";
      // 
      // bar2
      // 
      this.bar2.BarName = "Custom 3";
      this.bar2.DockCol = 0;
      this.bar2.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
      this.bar2.Text = "Custom 3";
      // 
      // bar3
      // 
      this.bar3.BarName = "Custom 4";
      this.bar3.DockCol = 0;
      this.bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
      this.bar3.Text = "Custom 4";
      // 
      // bar4
      // 
      this.bar4.BarName = "Custom 5";
      this.bar4.DockCol = 0;
      this.bar4.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
      this.bar4.Text = "Custom 5";
      // 
      // invoiceDSBindingSource
      // 
      this.invoiceDSBindingSource.DataSource = this.invoiceDS;
      this.invoiceDSBindingSource.Position = 0;
      // 
      // popupMenu
      // 
      this.popupMenu.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.EditRowButton),
            new DevExpress.XtraBars.LinkPersistInfo(this.DeleteRowButton)});
      this.popupMenu.Manager = this.barManager;
      this.popupMenu.Name = "popupMenu";
      // 
      // FrmMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(798, 429);
      this.Controls.Add(this.gridInvoices);
      this.Controls.Add(this.barDockControlLeft);
      this.Controls.Add(this.barDockControlRight);
      this.Controls.Add(this.barDockControlBottom);
      this.Controls.Add(this.barDockControlTop);
      this.MinimumSize = new System.Drawing.Size(800, 366);
      this.Name = "FrmMain";
      this.Text = "Applicazione";
      ((System.ComponentModel.ISupportInitialize)(this.viewInvoiceItems)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.gridInvoices)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.viewInvoices)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.barManager)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.invoiceDS)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.ApplicazioneMenu)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.behaviorManager)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.invoiceDSBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.popupMenu)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

    #endregion

    private DevExpress.XtraGrid.GridControl gridInvoices;
    private DevExpress.XtraGrid.Views.Grid.GridView viewInvoices;
    private DevExpress.XtraBars.Ribbon.ApplicationMenu ApplicazioneMenu;
    private DevExpress.XtraBars.BarSubItem barSubItem1;
    private DevExpress.XtraBars.BarSubItem barSubItem2;
    private DevExpress.XtraBars.BarManager barManager;
    private DevExpress.XtraBars.Bar MenuBar;
    private DevExpress.XtraBars.BarSubItem MBDatabase;
    private DevExpress.XtraBars.BarButtonItem bbLoad;
    private DevExpress.XtraBars.BarButtonItem bbSave;
    private DevExpress.XtraBars.BarSubItem MBReports;
    private DevExpress.XtraBars.BarDockControl barDockControlTop;
    private DevExpress.XtraBars.BarDockControl barDockControlBottom;
    private DevExpress.XtraBars.BarDockControl barDockControlLeft;
    private DevExpress.XtraBars.BarDockControl barDockControlRight;
    private DevExpress.XtraBars.BarSubItem barSubItem4;
    private DevExpress.XtraBars.Bar bar2;
    private DevExpress.XtraBars.Bar bar3;
    private DevExpress.XtraBars.Bar bar4;
    private DevExpress.XtraBars.BarStaticItem bs1;
    private DevExpress.XtraBars.BarButtonItem bbAdd;
    private DevExpress.XtraBars.BarButtonItem bbEdit;
    private DevExpress.XtraBars.BarButtonItem bbDelete;
    private DevExpress.XtraBars.BarStaticItem bs2;
    private DevExpress.XtraBars.BarButtonItem bbExit;
    private DevExpress.XtraBars.BarButtonItem bbStock;
    private DevExpress.XtraGrid.Columns.GridColumn colInvoiceNumber;
    private DevExpress.XtraGrid.Columns.GridColumn colClientName;
    private DevExpress.XtraGrid.Columns.GridColumn colIssueDate;
    private DevExpress.XtraGrid.Columns.GridColumn colDueDate;
    private DevExpress.XtraGrid.Columns.GridColumn colValueSum;
    private DevExpress.XtraGrid.Columns.GridColumn colCurrency;
    private DevExpress.XtraGrid.Columns.GridColumn colValueInRef;
    private InvoiceDS invoiceDS;
    private DevExpress.Utils.Behaviors.BehaviorManager behaviorManager;
    private System.Windows.Forms.BindingSource invoiceDSBindingSource;
    private DevExpress.XtraGrid.Views.Grid.GridView viewInvoiceItems;
    private DevExpress.XtraGrid.Columns.GridColumn colProductName;
    private DevExpress.XtraGrid.Columns.GridColumn colQuantity;
    private DevExpress.XtraGrid.Columns.GridColumn colPrice;
    private DevExpress.XtraGrid.Columns.GridColumn colValue;
    private DevExpress.XtraBars.PopupMenu popupMenu;
    private DevExpress.XtraBars.BarButtonItem EditRowButton;
    private DevExpress.XtraBars.BarButtonItem DeleteRowButton;
  }
}

