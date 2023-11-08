using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using Pyramid.Framework;
using Pyramid.Framework.BaseGUI;
using Pyramid.Framework.Localization;
using Pyramid.Framework.Utils;

using Pyramid.Helpers.Currency;

using Pyramid.Ra.Material.Data;
using Pyramid.Ra.Material.Interface;
using Pyramid.Ra.Material.Properties;

using Pyramid.Tools.Conversion;
using Pyramid.Tools.Utils;
using Pyramid.Ra.Geometry.Workspace;
using Pyramid.Ra.Material.Accessors;
using Pyramid.Ra.Material.PriceCatalog;
using Pyramid.Authentication;
using Pyramid.Ra.Material.Import;
using Pyramid.Helpers.Currency.Interface;
using Pyramid.Framework.BaseGUI.Forms;

namespace Pyramid.Ra.Material.Forms
{
  /// <summary>
  /// Forma de editare sectiuni.
  /// </summary>
  [System.Reflection.Obfuscation(Exclude = true)]
  public partial class FrmSection : BaseForm
  {
    #region Constants

    /// <summary>
    /// Cheia pentru salvarea filtrului utilizat la citirea datelor idn baza de date.
    /// </summary>
    private const string StateSectionSeries = "StateSectionSeries";

    /// <summary>
    /// Cheia pentru salvarea filtrului utilizat la citirea datelor idn baza de date.
    /// </summary>
    private const string StateContainsComponent = "StateContainsSection";

    /// <summary>
    /// Cheia pentru salvarea filtrului utilizat la citirea datelor idn baza de date.
    /// </summary>
    private const string StateSupplier = "StateSupplier";

    /// <summary>
    /// Cheia pentru salvarea filtrului utilizat la citirea datelor din baza de date
    /// </summary>
    private const string StateContainsReinforcement = "StateContainsReinforcement";

    /// <summary>
    /// Cheia pentru salvarea filtrului utilizat la citirea datelor din baza de date
    /// </summary>
    private const string StateContainsTolerance = "StateContainsTolerance";

    /// <summary>
    /// Cheia pentru salvarea filtrului utilizat la citirea datelor din baza de date
    /// </summary>
    private const string StateShowOnlyActive = "StateShowOnlyActive";
    /// <summary>
    /// Cheia ce identifica in fisierul de stare calea catre fisierul exportat.
    /// </summary>
    private const string StatePath = "StatePath";

    #endregion Constants|

    #region Private Members

    /// <summary>
    /// Interfata de acces la materiale
    /// </summary>
    private IMaterialDataModule material;
    /// <summary>
    /// Interfata de acces la valute
    /// </summary>
    private ICurrencyDataModule currency;
    /// <summary>
    /// Setul de date cu imagini;
    /// </summary>
    private ImageDataSet imageDS;
    /// <summary>
    /// Obiectul utilizat pentru schimb valutar.
    /// </summary>
    private CurrencyExchange currencyExchange;
    /// <summary>
    /// Modul de deschidere al formei
    /// </summary>
    private FormOpeningMode openingMode;
    /// <summary>
    /// DataView ce contine inregistrarile ramase in urma aplicarii filtrelor de pe coloanele gridului
    /// </summary>
    private DataView dvComponents;
    /// <summary>
    /// Flag ce indica daca datele au fost modificate
    /// </summary>
    private bool isDataChanged;
    /// <summary>
    /// Folderul de unde se vor incarca imaginile
    /// </summary>
    private FolderBrowserDialog folderBrowserDialogImages;
    /// <summary>
    /// Incarcare date la deschiderea formei.
    /// </summary>
    private bool firstTime;
    /// <summary>
    /// Refresh valori din lookupPriceCatalog.
    /// </summary>
    private bool updatesEnabled;
    /// <summary>
    /// Lista de preturi implicita.
    /// </summary>
    private Guid defaultPriceCatalogGuid;
    /// <summary>
    /// Flag ce indica daca ultilizatorul este administrator
    /// </summary>
    private bool isAdmin;
    /// <summary>
    /// Calea folderului unde se salveaza exportul.
    /// </summary>
    private string exportFolderPath;

    #endregion Private Members

    #region Constructor

    /// <summary>
    /// Creeaza forma de editare profile
    /// </summary>
    /// <param name="material">Interfata de acces la materiale</param>
    /// <param name="currency">Interfata de acces la valute</param>
    /// <param name="openingMode">Modul de deschidere al formei</param>
    /// <param name="defaultPriceCatalogGuid"> Lista e preturi implicita.</param> 
    public FrmSection(IMaterialDataModule material, ICurrencyDataModule currency, FormOpeningMode openingMode, Guid defaultPriceCatalogGuid)
    {
      InitializeComponent();
      LocalizeLookUpHeader();
      InitializeUnits();
      this.AttachBestFitEvents();

      this.material = material;
      this.currency = currency;
      this.openingMode = openingMode;
      this.isDataChanged = false;
      this.currencyExchange = new CurrencyExchange(currency, DateTime.Today);
      this.dvComponents = new DataView();
      this.folderBrowserDialogImages = new FolderBrowserDialog();
      this.firstTime = true;
      this.updatesEnabled = false;
      this.defaultPriceCatalogGuid = defaultPriceCatalogGuid;
      this.isAdmin = Session.CurrentSession.OptionsValidator.AccessInactiveRecords;
      if (isAdmin)
      {
        chkShowOnlyActive.Checked = true;
      }
    }

    #endregion Constructor

    #region Public Properties

    /// <summary>
    /// Flag ce indica daca datele au fost modificate.
    /// Proprietatea poate fi doar citita.
    /// </summary>
    public bool IsDataChanged
    {
      get { return isDataChanged; }
    }

    #endregion Public Properties

    #region Form Overrides

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      using (new WaitCursor())
      {
        this.Icon = Icon.FromHandle(Properties.Resources.ImgMaterialsSection.GetHicon());

        UpdateReadOnlyState();

        RefreshData(false);

        SetEditorsPriceCatalogReadOnly();

        barManager.SetPopupContextMenu(gridSection, popupMenu);

        InitializeHints();
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (DialogResult != DialogResult.Cancel)
      {
        viewSection.CloseEditor();
        if (!viewSection.UpdateCurrentRow())
        {
          e.Cancel = true;
          return;
        }
        if (!ApplyChanges(false))
        {
          e.Cancel = true;
          return;
        }
        if (openingMode == FormOpeningMode.Locked && IsDataChanged)
        {
          Debug.Assert(false, "Datele au fost modificate! Baza de date este blocata!");
        }
      }

      folderBrowserDialogImages.Dispose();
      base.OnClosing(e);
    }

    #endregion Form Overrides

    #region Private Methods

    /// <summary>
    /// Seteaza hint-urile pt Serie, Profil Component si Furnizor
    /// </summary>
    private void InitializeHints()
    {
      barEditSeries.Hint = repLookUpSeries.GetDisplayText(barEditSeries.EditValue);
      barEditSupplier.Hint = repLookUpSupplier.GetDisplayText(barEditSupplier.EditValue);
      barEditContainsSection.Hint = repLookUpContainsSection.GetDisplayText(barEditContainsSection.EditValue);
      barEditContainsReinforcement.Hint = repLookUpContainsReinforcement.GetDisplayText(barEditContainsReinforcement.EditValue);
      barEditContainsTolerance.Hint = repLookUpContainsTolerance.GetDisplayText(barEditContainsTolerance.EditValue);
      barEditPriceCatalog.Hint = repLookUpPriceCatalog.GetDisplayText(barEditPriceCatalog.EditValue);
    }

    /// <summary>
    /// Salveaza modificarile in baza de date
    /// </summary>
    /// <param name="refreshData">Specifica daca dupa salvare se vor reincarca datele</param>
    /// <returns>TRUE daca salvarea s-a incheiat cu succes, FALSE daca au aparut erori</returns>
    private bool ApplyChanges(bool refreshData)
    {
      try
      {
        using (new WaitCursor())
        {
          int? idPriceCatalog = (int?)barEditPriceCatalog.EditValue;

          // Se salveaza inregistrarea aflata in editare
          viewSection.CloseEditor();
          if (!viewSection.UpdateCurrentRow())
          {
            return false;
          }

          // Aflam datele pe care vrem sa le actualizam
          if (sectionDS.HasChanges() || imageDS.HasChanges())
          {
            SectionDataSet changesDS = sectionDS.GetChanges() as SectionDataSet;
            ImageDataSet imageChangesDS = imageDS.GetChanges() as ImageDataSet;
            // Salvam modificarile
            material.WriteSectionData(changesDS, new MaterialPriceWriteContext(idPriceCatalog), CultureInfo.CurrentCulture.Name, Session.CurrentSession.ConnectorInfo);
            //MaterialPoolCache.CurrentMaterialPoolCache.ImageManager.WriteImageDS(imageChangesDS);
            MaterialPool.MaterialPoolInstance.WriteAndMergeImageData(material, imageChangesDS);
            isDataChanged = true;

            if (refreshData)
            {
              RefreshData(false);
            }
          }
        }
      }
      catch (System.ServiceModel.FaultException ex)
      {
        FrameworkApplication.TraceSource.TraceData(TraceEventType.Error, 0, new object[] { ex.Message });
        if (XtraMessageBox.Show(this,
              Properties.Resources.MsgExceptionWriteDataSet,
              Properties.Resources.CaptionAttentionMsgBox,
              MessageBoxButtons.YesNo,
              MessageBoxIcon.Error) == DialogResult.No)
        {
          if (refreshData)
          {
            RefreshData(false);
          }
          return true;
        }
        else
        {
          return false;
        }
      }
      catch (Pyramid.Tools.AppException.CommitChangesException ex)
      {
        FrameworkApplication.TraceSource.TraceData(TraceEventType.Error, 0, new object[] { ex.Message });
        if (XtraMessageBox.Show(this,
              Properties.Resources.MsgExceptionWriteDataSet,
              Properties.Resources.CaptionAttentionMsgBox,
              MessageBoxButtons.YesNo,
              MessageBoxIcon.Error) == DialogResult.No)
        {
          if (refreshData)
          {
            RefreshData(false);
          }
          return true;
        }
        else
        {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Reincarca datele
    /// </summary>
    /// <param name="queryChanges">TRUE pentru a se verifica daca sunt modificari, FALSE pentru a face doar refresh</param>
    private void RefreshData(bool queryChanges)
    {
      // Se salveaza inregistrarea aflata in editare
      viewSection.CloseEditor();
      if (!viewSection.UpdateCurrentRow())
      {
        return;
      }

      if (queryChanges && sectionDS.HasChanges())
      {
        DialogResult result = XtraMessageBox.Show(this,
                                                  Properties.Resources.MsgSaveSectionQuestion,
                                                  Properties.Resources.CaptionAttentionMsgBox,
                                                  MessageBoxButtons.YesNoCancel,
                                                  MessageBoxIcon.Question);
        if (result == DialogResult.Cancel)
        {
          return;
        }
        else if (result == DialogResult.Yes)
        {
          if (!ApplyChanges(false))
          {
            return;
          }
        }
      }

      using (new WaitCursor())
      {
        gridUtils.SaveFocusAndSelection();
        updatesEnabled = false;

        currencyExchange.RefreshData(DateTime.Today);
        currencyDS = currencyExchange.CurrencyDS;
        costDS = material.ReadCostData(null, CultureInfo.CurrentCulture.Name, Session.CurrentSession.ConnectorInfo);
        colorListDS = material.ReadColorListData(new ColorListFilter(null, ShowOnlyActive), CultureInfo.CurrentCulture.Name, Session.CurrentSession.ConnectorInfo);
        consumeGroupDS = material.ReadConsumeGroupData(CultureInfo.CurrentCulture.Name, Session.CurrentSession.ConnectorInfo);
        colorDS = material.ReadColorData(new ColorCombinationFilter(ShowOnlyActive), CultureInfo.CurrentCulture.Name, Session.CurrentSession.ConnectorInfo);
        materialSpeciesDS = material.ReadMaterialSpeciesData(new MaterialSpeciesFilter(), CultureInfo.CurrentCulture.Name, Session.CurrentSession.ConnectorInfo);
        seriesDS = material.ReadSeriesData(new SeriesFilter(null, ShowOnlyActive), CultureInfo.CurrentCulture.Name, Session.CurrentSession.ConnectorInfo);
        priceCatalogDS = material.ReadPriceCatalogData(CultureInfo.CurrentCulture.Name, Session.CurrentSession.ConnectorInfo);

        imageDS = MaterialPool.ImageDS;

        int? idPriceCatalog = null;
        ///Datele se filtreaza in functie de catalog de preturi, dupa valoarea din barEditPriceCatalog.
        DataView priceCatalogView = priceCatalogDS.PriceCatalog.GetPriceCatalogView(ApplicationRoles.GrantedCostGroupLevelRoles);
        if (firstTime && priceCatalogView.Count > 0)
        {
          firstTime = false;
          PriceCatalogDataSet.PriceCatalogRow priceCatalogRow = priceCatalogDS.PriceCatalog.FindByGuid(defaultPriceCatalogGuid);
          if (priceCatalogView != null)
          {
            priceCatalogView.Sort = priceCatalogDS.PriceCatalog.IdColumn.ColumnName;

            //daca catalogul din optiuni nu exista in bd se seteaza catalogul principal
            if (priceCatalogRow == null)
            {
              for (int i = 0; i < priceCatalogView.Count; i++)
              {
                if ((bool)priceCatalogView[i]["IsMainPriceCatalog"])
                {
                  idPriceCatalog = (int)priceCatalogView[i]["Id"];
                }
              }
              if (idPriceCatalog == null)
              {
                idPriceCatalog = (int)priceCatalogView[0]["Id"];
              }
            }
            else if (priceCatalogView.Find(priceCatalogRow.Id) != -1)
            {
              idPriceCatalog = priceCatalogRow.Id;
            }
            else
            {
              idPriceCatalog = (int)priceCatalogView[0]["Id"];
            }
          }
        }
        else
        {
          idPriceCatalog = (int?)barEditPriceCatalog.EditValue;
        }
        barEditPriceCatalog.EditValue = idPriceCatalog;

        sectionDS = material.ReadSectionData(new SectionFilter(null, idPriceCatalog, ShowOnlyActive), CultureInfo.CurrentCulture.Name, Session.CurrentSession.ConnectorInfo);
        gridSection.DataSource = sectionDS.Section;

        sectionBS.DataSource = new DataView(sectionDS.Section);
        sectionBS.Filter = string.Format("{0} = '{1}'",
                    sectionDS.Section.SectionTypeColumn.ColumnName,
                    SectionType.Reinforcement.ToString());
        sectionBS.Sort = sectionDS.Section.CodeColumn.ColumnName;

        rawSectionBS.DataSource = new DataView(sectionDS.Section);
        rawSectionBS.Filter = string.Format("{0} = '{1}'",
                    sectionDS.Section.SectionTypeColumn.ColumnName,
                    SectionType.RawSection.ToString());
        rawSectionBS.Sort = sectionDS.Section.CodeColumn.ColumnName;

        dvComponents.Table = sectionDS.Section;
        // in View-ul de profile componente se pastreaza acele inregistrari au copii
        // in tabela CustomSectionItem mergand pe relatia profilelor componente (IdSection)
        dvComponents.RowFilter = string.Format("Max(Child(FK_Section_CustomSectionItem).{0}) <> 0", sectionDS.CustomSectionItem.IdColumn.ColumnName);
        repLookUpContainsSection.DataSource = dvComponents;


        toleranceBS.DataSource = new DataView(sectionDS.Section);
        toleranceBS.Filter = string.Format("Max(Child(FK_Section_SectionTolerance2).{0}) <> 0", sectionDS.SectionTolerance.IdColumn.ColumnName);
        toleranceBS.Sort = sectionDS.Section.CodeColumn.ColumnName;

        repLookUpSectionConsumeGroup.DataSource = consumeGroupDS.ConsumeGroup;

        ArrayList sectionType = EnumTypeLocalizer.Localize(typeof(SectionType));
        sectionType.Sort();
        repLookUpSectionType.DataSource = sectionType;

        repLookUpPriceCatalog.DataSource = priceCatalogView;
        repGridLookUpSectionReinforcement.DataSource = sectionBS;
        repLookUpSectionCurrency.DataSource = currencyDS.Currency;
        repLookUpSectionMaterialType.DataSource = EnumTypeLocalizer.Localize(typeof(MaterialType));
        repLookUpPriceComputeMode.DataSource = EnumTypeLocalizer.Localize(typeof(PriceCalculationType));
        repLookUpSeries.DataSource = seriesDS.Series;
        repLookUpContainsReinforcement.DataSource = sectionBS;
        repLookUpSectionFixingMode.DataSource = EnumTypeLocalizer.Localize(typeof(SurfaceFixingMode));
        repLookUpCuttingType.DataSource = EnumTypeLocalizer.Localize<CuttingType>();
        repLookUpCurvingMode.DataSource = EnumTypeLocalizer.Localize<CurvingMode>();
        repLookUpCoversInnerTemplates.DataSource = EnumTypeLocalizer.Localize<ViewSide>();
        repLookUpSupplier.DataSource = seriesDS.Series.Suppliers();
        repFlagsKnownSide.AddFlagsEnum(typeof(KnownSide),
          new KnownSide[] { KnownSide.Top, KnownSide.Left, KnownSide.Bottom, KnownSide.Right, KnownSide.Vertical, KnownSide.Horizontal, KnownSide.All });
        repLookUpSectionExtendingMode.DataSource = EnumTypeLocalizer.Localize(typeof(ExtendingMode));

        repLookUpCornerCuttingType.DataSource = EnumTypeLocalizer.Localize(typeof(CornerCuttingType));
        repLookUpAltersInnerGeometry.DataSource = BoolTypeLocalizer.Localize();
        repLookUpRawSectionType.DataSource = EnumTypeLocalizer.Localize(typeof(RawSectionType));

        viewSection.RefreshData();

        gridUtils.RestoreFocusAndSelection();
        updatesEnabled = true;

      }
    }

    /// <summary>
    /// Actualizeaza starea editoarelor in functie de modul de deschidere al formei
    /// </summary>
    private void UpdateReadOnlyState()
    {
      bool readOnly = openingMode != FormOpeningMode.Normal;


      foreach (GridColumn column in viewSection.Columns)
      {
        if (openingMode == FormOpeningMode.Locked)
        {

          column.OptionsColumn.ReadOnly = true;
          column.OptionsColumn.AllowEdit = false;
          continue;
        }

        // tolerantele si setarile de optimizare se pot modifica indiferent de mod de deschidere.
        // pretul si valuta se pot modifica in modul limited.
        if (column == colCuttingTolerance || column == colBindingTolerance ||
            column == colProcessingTolerance || column == colCurvingAddition || column == colFillingTolerance ||
            column == colIsForOptimization || column == colUseDoubleCut ||
            column == colBinSize || column == colUseInventory || column == colOptimizationMinLimitLength ||
            column == colOptimizationMinInventoryLength || column == colOptimizationMaxLimitLength ||
            column == colOptimizationTargetInventory || column == colOptimizationMaxInventory ||
            (openingMode == FormOpeningMode.LimitedReadOnly &&
             (column == colCurrency || column == colUnitBasePrice || column == colPriceCalculationType)))
        {
          column.OptionsColumn.ReadOnly = false;
          column.OptionsColumn.AllowEdit = true;
        }
        else
        {
          column.OptionsColumn.ReadOnly = readOnly;
          column.OptionsColumn.AllowEdit = !readOnly;
        }
      }

      if (openingMode == FormOpeningMode.Locked)
      {
        bbSave.Enabled = false;
        bbCut.Enabled = false;
        bbCopy.Enabled = false;
        bbPaste.Enabled = false;
        bbDelete.Enabled = false;
        bbDeleteProfileSystem.Enabled = false;
        bbNewItem.Enabled = false;
        bbModifyPriceCatalog.Enabled = false;
        bbResetPriceCatalog.Enabled = false;
        bbImages.Enabled = false;
        bbActivateSelection.Enabled = false;
        bbDeactivateSelection.Enabled = false;
        bbImportPrices.Enabled = false;
      }
      else
      {
        bbSave.Enabled = true;
        bbCut.Enabled = !readOnly;
        bbCopy.Enabled = !readOnly;
        bbPaste.Enabled = !readOnly;
        bbDelete.Enabled = !readOnly;
        bbDeleteProfileSystem.Enabled = !readOnly;
        bbNewItem.Enabled = !readOnly;
        bbModifyPriceCatalog.Enabled = !readOnly;
        bbResetPriceCatalog.Enabled = !readOnly;
        bbActivateSelection.Enabled = !readOnly;
        bbDeactivateSelection.Enabled = !readOnly;
      }

      if (openingMode == FormOpeningMode.LimitedReadOnly)
      {
        bbSave.Enabled = true;
        colPriceCalculationType.OptionsColumn.ReadOnly = false;
        colCurrency.OptionsColumn.ReadOnly = false;

        colPriceCalculationType.OptionsColumn.AllowEdit = true;
        colCurrency.OptionsColumn.AllowEdit = true;

        bbModifyPriceCatalog.Enabled = true;
        bbResetPriceCatalog.Enabled = true;
      }

      if (!isAdmin)
      {
        colIsActive.OptionsColumn.ReadOnly = true;
        chkShowOnlyActive.Checked = true;
        chkShowOnlyActive.Enabled = false;

        bbActivateSelection.Enabled = false;
        bbDeactivateSelection.Enabled = false;
      }
    }

    /// <summary>
    /// Setare controale catalog de preturi in fct de catalogul principal.
    /// </summary>
    private void SetEditorsPriceCatalogReadOnly()
    {
      bool readOnly = openingMode != FormOpeningMode.Normal;
      int? idPriceCatalog = (int?)barEditPriceCatalog.EditValue;
      if (!idPriceCatalog.HasValue)
      {
        return;
      }
      if (!readOnly)
      {
        PriceCatalogDataSet.PriceCatalogRow priceCatalogRow = priceCatalogDS.PriceCatalog.FindById(idPriceCatalog.Value);
        if (priceCatalogRow != null)
        {
          bbResetPriceCatalog.Enabled = !priceCatalogRow.IsMainPriceCatalog;
        }
      }
    }

    /// <summary>
    /// Importa imaginile dintr-un fisier ales
    /// </summary>
    /// <param name="path">Calea fisierului ales</param>
    private void ImportImages(string path)
    {
      IEnumerable<string> folderImages;
      Image profileImage;
      bool imageSizeOk, imageExists;
      int countOkImages = 0, countNoImages = 0, countImagesTooBig = 0;

      this.Cursor = Cursors.WaitCursor;
      imageExists = false;
      for (int i = 0; i < viewSection.DataRowCount; i++)
      {
        SectionDataSet.SectionRow dataRow = (SectionDataSet.SectionRow)viewSection.GetDataRow(i);
        if (imageDS.Image.FindByGuid(dataRow.Guid) != null)
        {
          imageExists = true;
          break;
        }
      }

      if (imageExists)
      {
        this.Cursor = Cursors.Default;
        if (XtraMessageBox.Show(this, Properties.Resources.ProfileAlreadyHasImages
                              , Properties.Resources.CaptionQuestionMsgBox
                              , MessageBoxButtons.YesNo
                              , MessageBoxIcon.Question) == DialogResult.No)
        {
          return;
        }
      }

      this.Cursor = Cursors.WaitCursor;
      for (int i = 0; i < viewSection.DataRowCount; i++)
      {
        SectionDataSet.SectionRow dataRow = (SectionDataSet.SectionRow)viewSection.GetDataRow(i);
        imageSizeOk = false;
        imageExists = false;
        try
        {
          folderImages = Directory.GetFiles(path, String.Format(Properties.Settings.Default.ImageDefaultFormat, dataRow.Code), SearchOption.TopDirectoryOnly);
          foreach (string imagePath in folderImages)
          {
            try
            {
              profileImage = Image.FromFile(imagePath);
              if (profileImage != null)
              {
                imageExists = true;
                using (MemoryStream mStream = new MemoryStream())
                {
                  profileImage.Save(mStream, ImageFormat.Png);
                  byte[] ret = mStream.ToArray();
                  if ((ret.Length / 1024) < Properties.Settings.Default.ImageMaximumSize)
                  {
                    imageDS.Image.SetImage(dataRow, ret);
                    imageSizeOk = true;
                    break;
                  }
                }
              }
            }
            catch { }
          }
        }
        catch { }
        if (!imageExists)
        {
          countNoImages++;
        }
        else if (!imageSizeOk)
        {
          countImagesTooBig++;
        }
        else
        {
          countOkImages++;
        }
      }
      this.Cursor = Cursors.Default;
      XtraMessageBox.Show(this, String.Format(Properties.Resources.ProfileImageUpload, countOkImages, countNoImages, countImagesTooBig)
                              , Properties.Resources.CaptionAttentionMsgBox
                              , MessageBoxButtons.OK
                              , MessageBoxIcon.Information);
    }
    public DialogResult ShowSectionEditDialog(Guid sectionGuid)
    {
      if (sectionGuid == Guid.Empty)
      {
        return DialogResult.Cancel;
      }
      viewSection.CloseEditor();
      if (!viewSection.UpdateCurrentRow())
      {
        return DialogResult.Cancel;
      }
      gridUtils.SaveFocusAndSelection();
      persistStateComponent.AutomaticMode = false;
      SectionDataSet sectionEditDS = sectionDS.Copy() as SectionDataSet;
      ImageDataSet imageEditDS = imageDS.Copy() as ImageDataSet;
      SectionDataSet.SectionRow sectionEditRow = sectionEditDS.Section.FindByGuid(sectionGuid);

      bool isActive = true;

      if (sectionEditRow != null)
        isActive = sectionEditRow.IsActive;
      FrmSectionEdit frm = null;
      try
      {
        frm = new FrmSectionEdit(sectionEditRow, sectionEditDS, imageEditDS, seriesDS, costDS, colorListDS, consumeGroupDS,
                                                currencyDS, colorDS, materialSpeciesDS, currencyExchange, openingMode, ShowOnlyActive);
        if (frm.ShowDisposableDialog(this) == DialogResult.OK)
        {
          if (frm.sectionDS.HasChanges())
          {
            using (new WaitCursor())
            {
              if (sectionEditRow != null && isActive != sectionEditRow.IsActive)
              {
                DialogResult? lastDialogResult = null;
                ValidateSectionRow(sectionEditRow, sectionEditRow.IsActive, ref lastDialogResult, MessageBoxButtons.YesNoCancel);

                if (lastDialogResult.HasValue && (lastDialogResult.Value == DialogResult.Cancel))
                  return DialogResult.Cancel;
              }

              gridSection.DataSource = null;
              sectionDS = frm.sectionDS.Copy() as SectionDataSet;
              imageDS = imageEditDS.Copy() as ImageDataSet;
              gridSection.DataSource = sectionDS.Section;
            }
          }
          else
          {
            return DialogResult.Cancel;
          }
          gridUtils.RestoreFocusAndSelection();
        }
        else
        {
          gridUtils.RestoreFocusAndSelection();
          return DialogResult.Cancel;
        }
      }
      catch
      {
        Debug.Assert(false);
        return DialogResult.Cancel;
      }
      finally
      {
        frm.Dispose();
        sectionEditDS.Dispose();
        imageDS.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
      }

      gridUtils.RestoreFocusAndSelection();
      return DialogResult.OK;
    }
    /// <summary>
    /// Afiseaza fereastra de editare a proprietatilor unui profil
    /// </summary>
    /// <param name="sectionRow">Randul de tip Profil care se editeaza sau NULL daca e unul nou</param>
    private void ShowSectionEditDialog(SectionDataSet.SectionRow sectionRow)
    {
      viewSection.CloseEditor();
      if (!viewSection.UpdateCurrentRow())
      {
        return;
      }

      if (consumeGroupDS.ConsumeGroup.Count == 0)
      {
        XtraMessageBox.Show(this,
                            Properties.Resources.MsgErrorConsumeGroupNull,
                            Properties.Resources.CaptionAttentionMsgBox,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
        return;
      }

      gridUtils.SaveFocusAndSelection();

      SectionDataSet sectionEditDS = sectionDS.Copy() as SectionDataSet;
      ImageDataSet imageEditDS = imageDS.Copy() as ImageDataSet;

      SectionDataSet.SectionRow sectionEditRow = null;
      if (sectionRow != null)
      {
        sectionEditRow = sectionEditDS.Section.FindById(sectionRow.Id);
      }

      bool isActive = true;

      if (sectionEditRow != null)
        isActive = sectionEditRow.IsActive;

      FrmSectionEdit frm = null;

      try
      {
        frm = new FrmSectionEdit(sectionEditRow, sectionEditDS, imageEditDS, seriesDS, costDS, colorListDS, consumeGroupDS,
                                                currencyDS, colorDS, materialSpeciesDS, currencyExchange, openingMode, ShowOnlyActive);
        if (frm.ShowDisposableDialog(this, false) == DialogResult.OK)
        {
          if (frm.sectionDS.HasChanges())
          {
            using (new WaitCursor())
            {
              if (sectionEditRow != null && isActive != sectionEditRow.IsActive)
              {
                DialogResult? lastDialogResult = null;
                ValidateSectionRow(sectionEditRow, sectionEditRow.IsActive, ref lastDialogResult, MessageBoxButtons.YesNoCancel);

                if (lastDialogResult.HasValue && (lastDialogResult.Value == DialogResult.Cancel))
                  return;
              }

              gridSection.DataSource = null;
              sectionDS = frm.sectionDS.Copy() as SectionDataSet;
              imageDS = imageEditDS.Copy() as ImageDataSet;
              gridSection.DataSource = sectionDS.Section;
            }
          }
        }
      }
      catch
      {
        Debug.Assert(false);
      }
      finally
      {
        frm.Dispose();
        sectionEditDS.Dispose();
        imageDS.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
      }

      gridUtils.RestoreFocusAndSelection();
    }

    /// <summary>
    /// Metoda ce localizeaza caption-urile coloanelor afisate in lookup-urile de pe forma.
    /// </summary>
    /// <remarks>
    /// Deoarece captionul nu se poate localiza cu mecanismul standard, localizarea 
    /// se face manual in aceasta metoda.
    /// </remarks>
    private void LocalizeLookUpHeader()
    {
      LocalizeLookUpHeader(repLookUpSeries, Resources.ResourceManager);
      LocalizeLookUpHeader(repLookUpContainsSection, Resources.ResourceManager);
      LocalizeLookUpHeader(repLookUpSectionCurrency, Resources.ResourceManager);
      LocalizeLookUpHeader(repLookUpContainsReinforcement, Resources.ResourceManager);
      LocalizeLookUpHeader(repLookUpContainsTolerance, Resources.ResourceManager);
      LocalizeLookUpHeader(repLookUpPriceCatalog, Resources.ResourceManager);

    }

    /// <summary>
    /// Initializeaza unitatile de masura de pe forma.
    /// </summary>
    private void InitializeUnits()
    {
      // Controale de editare.
      repUnitLength.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      repUnitNullableLength.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      repUnitSurface.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.SectionSurface;
      repUnitMass.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.Mass;
      repUnitHeatTransferCoefficient.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.ThermalTransmittance;
      repUnitNullableMomentOfInertia.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MomentOfInertia;

      // Label-uri si Caption-uri
      colUnitWeight.Caption = String.Format(colUnitWeight.Caption, MeasurableProperties.DefaultMeasurableProperties.Length.DisplayUnitShortName);
      colUnitBasePrice.Caption = String.Format(colUnitBasePrice.Caption, MeasurableProperties.DefaultMeasurableProperties.Mass.DisplayUnitShortName);
    }

    /// <summary>
    /// Valideaza datele pentru profilele compuse in functie de activ/inactiv
    /// </summary>
    /// <param name="sectionRow"></param>
    /// <returns></returns>
    private bool? ValidateSectionCustomIsActiveOld(SectionDataSet.SectionRow sectionRow)
    {
      if (!sectionRow.IsActive)
      {
        string filter = string.Format("{0} = {1}",
                                       sectionDS.CustomSectionItem.IdSectionColumn.ColumnName,
                                       sectionRow.Id);
        if (sectionDS.CustomSectionItem.Select(filter).Length > 0)
        {
          SectionDataSet.SectionRow parentSectionRow = null;

          bool changeData = false;

          foreach (SectionDataSet.CustomSectionItemRow customSectionItemRow in sectionDS.CustomSectionItem.Select(filter))
          {
            parentSectionRow = sectionDS.Section.FindById(customSectionItemRow.IdParentSection);

            if (changeData && parentSectionRow != null)
              parentSectionRow.IsActive = false;

            if (parentSectionRow != null && parentSectionRow.IsActive)
            {
              DialogResult result = XtraMessageBox.Show(this,
                                   Properties.Resources.MsgErrorSectionCustomChildDeactivate,
                                   Properties.Resources.CaptionAttentionMsgBox,
                                   MessageBoxButtons.YesNoCancel,
                                   MessageBoxIcon.Exclamation);
              if (result == DialogResult.Yes)
              {
                changeData = true;
                parentSectionRow.IsActive = false;
              }
              else if (result == DialogResult.No)
              {
                return false;
              }
              else if (result == DialogResult.Cancel)
              {
                return null;
              }
            }
          }
        }
      }

      if (sectionRow.IsActive)
      {
        string filter = string.Format("{0} = {1}",
                                       sectionDS.CustomSectionItem.IdSectionColumn.ColumnName,
                                       sectionRow.Id);
        if (sectionDS.CustomSectionItem.Select(filter).Length > 0)
        {
          SectionDataSet.SectionRow parentSectionRow = null;

          bool changeData = false;

          foreach (SectionDataSet.CustomSectionItemRow customSectionItemRow in sectionDS.CustomSectionItem.Select(filter))
          {
            int inactiveChildren = 0;
            parentSectionRow = sectionDS.Section.FindById(customSectionItemRow.IdParentSection);

            string secondFilter = string.Format("{0} = {1}",
                                      sectionDS.CustomSectionItem.IdParentSectionColumn.ColumnName,
                                      parentSectionRow.Id);

            SectionDataSet.SectionRow childSectionRow = null;

            foreach (SectionDataSet.CustomSectionItemRow parentCustomSectionItemRow in sectionDS.CustomSectionItem.Select(secondFilter))
            {
              childSectionRow = sectionDS.Section.FindById(customSectionItemRow.IdSection);
              if (!childSectionRow.IsActive)
                inactiveChildren++;
            }

            if (changeData && parentSectionRow != null && inactiveChildren == 0)
              parentSectionRow.IsActive = true;

            if (parentSectionRow != null && !parentSectionRow.IsActive && inactiveChildren == 0)
            {
              DialogResult result = XtraMessageBox.Show(this,
                                   Properties.Resources.MsgErrorSectionCustomChildActivate,
                                   Properties.Resources.CaptionAttentionMsgBox,
                                   MessageBoxButtons.YesNoCancel,
                                   MessageBoxIcon.Exclamation);
              if (result == DialogResult.Yes)
              {
                changeData = true;
                parentSectionRow.IsActive = true;
              }
              else if (result == DialogResult.No)
              {
                return false;
              }
              else if (result == DialogResult.Cancel)
              {
                return null;
              }
            }
          }
        }


        string parentFilter = string.Format("{0} = {1}",
                                       sectionDS.CustomSectionItem.IdParentSectionColumn.ColumnName,
                                       sectionRow.Id);
        if (sectionDS.CustomSectionItem.Select(parentFilter).Length > 0)
        {
          bool isForChange = false;

          foreach (SectionDataSet.CustomSectionItemRow customSectionItemRow in sectionDS.CustomSectionItem.Select(parentFilter))
          {
            SectionDataSet.SectionRow childSectionRow = sectionDS.Section.FindById(customSectionItemRow.IdSection);
            if (!childSectionRow.IsActive)
            {
              isForChange = true;
              break;
            }
          }

          if (isForChange)
          {
            DialogResult result = XtraMessageBox.Show(this,
                                   Properties.Resources.MsgErrorSectionCustomParentActivate,
                                   Properties.Resources.CaptionAttentionMsgBox,
                                   MessageBoxButtons.YesNoCancel,
                                   MessageBoxIcon.Exclamation);
            if (result == DialogResult.Yes)
            {
              foreach (SectionDataSet.CustomSectionItemRow customSectionItemRow in sectionDS.CustomSectionItem.Select(parentFilter))
              {
                SectionDataSet.SectionRow childSectionRow = sectionDS.Section.FindById(customSectionItemRow.IdSection);
                childSectionRow.IsActive = true;
              }
            }
            else if (result == DialogResult.No)
            {
              sectionRow.IsActive = false;
              return false;
            }
            else if (result == DialogResult.Cancel)
            {
              return null;
            }
          }

        }
      }

      return true;
    }

    /// <summary>
    /// Valideaza datele pentru profile in functie de activ/inactiv
    /// </summary>
    /// <param name="accessoryRow">Randul verificat.</param>
    /// <returns>Adevarat daca validarea e corecta, fals altfel.</returns>
    private bool? ValidateSectionIsActive(SectionDataSet.SectionRow sectionRow, bool isActive)
    {
      DialogResult? lastDialogResult = null;
      ValidateSectionRow(sectionRow, isActive, ref lastDialogResult, MessageBoxButtons.YesNoCancel);

      if (lastDialogResult.HasValue && (lastDialogResult.Value == DialogResult.Cancel))
      {
        return null;
      }

      if (lastDialogResult.HasValue && (lastDialogResult.Value == DialogResult.No || lastDialogResult.Value == DialogResult.OK))
      {
        return false;
      }

      return true;
    }


    /// <summary>
    /// Valideaza datele pentru profile in functie de activ/inactiv.
    /// </summary>
    /// <param name="isActive">Flag ce indica daca datele au fost activate/dezactivate.</param>
    /// <param name="selectedRowHandles">Randurile selectate.</param>
    private void ValidateSections(int[] selectedRowHandles, bool isActive)
    {
      if (selectedRowHandles == null || selectedRowHandles.Length < 1)
      {
        return;
      }

      try
      {
        gridSection.BeginUpdate();
        IEnumerable<SectionDataSet.SectionRow> sectionRows = selectedRowHandles.Select(rowHandle => (SectionDataSet.SectionRow)viewSection.GetDataRow(rowHandle)).ToArray();
        foreach (SectionDataSet.SectionRow sectionRow in sectionRows)
        {
          sectionRow.IsActive = isActive;
        }

        DialogResult? lastDialogResult = null;
        foreach (SectionDataSet.SectionRow sectionRow in sectionRows)
        {
          ValidateSectionRow(sectionRow, isActive, ref lastDialogResult, MessageBoxButtons.YesNo);
        }
      }
      finally
      {
        gridSection.EndUpdate();
      }
    }

    /// <summary>
    /// Valideaza datele pentru profile in functie de activ/inactiv.
    /// </summary>
    /// <param name="isActive">Flag ce indica daca datele au fost activate/dezactivate.</param>
    /// <param name="selectedRowHandles">Randurile selectate.</param>
    /// <param name="lastDialogResult">Mesajul ce urmeaza a fi afisat utilizatorului.</param>
    /// <param name="messageBoxButtons">Butoanele mesajului.</param>
    public void ValidateSectionRow(SectionDataSet.SectionRow sectionRow, bool isActive, ref DialogResult? lastDialogResult, MessageBoxButtons messageBoxButtons)
    {
      if (!isActive)
      {
        foreach (SectionDataSet.SectionRow parentSectionRow in sectionDS.Section)
        {
          if (parentSectionRow.RowState == DataRowState.Deleted)
            continue;

          if (!parentSectionRow.IsIdReinforcementNull() && parentSectionRow.IsActive && parentSectionRow.IdReinforcement == sectionRow.Id)
          {
            if (!lastDialogResult.HasValue)
            {
              lastDialogResult = XtraMessageBox.Show(this,
                                  Properties.Resources.MsgErrorInactiveReinforcement,
                                  Properties.Resources.CaptionAttentionMsgBox,
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Exclamation);
            }
            sectionRow.IsActive = true;
            return;
          }

          if (!parentSectionRow.IsIdRawSectionNull() && parentSectionRow.IsActive && parentSectionRow.IdRawSection == sectionRow.Id)
          {
            if (!lastDialogResult.HasValue)
            {
              lastDialogResult = XtraMessageBox.Show(this,
                                  Properties.Resources.MsgErrorInactiveSectionRaw,
                                  Properties.Resources.CaptionAttentionMsgBox,
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Exclamation);
            }
            sectionRow.IsActive = true;
            return;
          }

          if (!parentSectionRow.IsIdArcRawSectionNull() && parentSectionRow.IsActive && parentSectionRow.IdArcRawSection == sectionRow.Id)
          {
            if (!lastDialogResult.HasValue)
            {
              lastDialogResult = XtraMessageBox.Show(this,
                                  Properties.Resources.MsgErrorInactiveSectionRaw,
                                  Properties.Resources.CaptionAttentionMsgBox,
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Exclamation);
            }
            sectionRow.IsActive = true;
            return;
          }
        }

        foreach (SectionDataSet.SectionColorListRow parentSectionColorListRow in sectionDS.SectionColorList)
        {
          if (parentSectionColorListRow.RowState == DataRowState.Deleted)
            continue;

          if (!parentSectionColorListRow.IsIdReinforcementNull() && parentSectionColorListRow.IdReinforcement == sectionRow.Id)
          {
            if (!lastDialogResult.HasValue)
            {
              lastDialogResult = XtraMessageBox.Show(this,
                                  Properties.Resources.MsgErrorInactiveReinforcement,
                                  Properties.Resources.CaptionAttentionMsgBox,
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Exclamation);
            }
            sectionRow.IsActive = true;
            return;
          }

          foreach (SectionDataSet.SectionColorListItemRow parentSectionColorListItemRow in sectionDS.SectionColorListItem)
          {
            if (!parentSectionColorListItemRow.IsIdReinforcementNull() && parentSectionColorListItemRow.IdReinforcement == sectionRow.Id)
            {
              if (!lastDialogResult.HasValue)
              {
                lastDialogResult = XtraMessageBox.Show(this,
                                    Properties.Resources.MsgErrorInactiveReinforcement,
                                    Properties.Resources.CaptionAttentionMsgBox,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);
              }
              sectionRow.IsActive = true;
              return;
            }
          }
        }

        foreach (SectionDataSet.SectionRow parentSectionRow in sectionRow.ParentSectionRows)
        {
          if (parentSectionRow.RowState == DataRowState.Deleted || !parentSectionRow.IsActive)
          {
            continue;
          }

          bool? parentSectionRowIsValid = ValidateSectionIsActive(parentSectionRow, false);
          if (parentSectionRowIsValid == null || parentSectionRowIsValid == false)
          {
            sectionRow.IsActive = true;
            return;
          }
        }

        foreach (SectionDataSet.SectionRow parentSectionRow in sectionRow.ParentSectionRows)
        {
          if (!parentSectionRow.IsActive)
          {
            continue;
          }

          if (!lastDialogResult.HasValue)
          {
            lastDialogResult = XtraMessageBox.Show(this,
                                                    Properties.Resources.MsgErrorSectionCustomChildDeactivate,
                                                    Properties.Resources.CaptionQuestionMsgBox,
                                                    messageBoxButtons,
                                                    MessageBoxIcon.Question);
          }

          if (lastDialogResult.Value == DialogResult.Cancel)
          {
            return;
          }
          else if (lastDialogResult.Value == DialogResult.No)
          {
            sectionRow.IsActive = true;
            return;
          }
          else if (lastDialogResult.Value == DialogResult.Yes)
          {
            parentSectionRow.IsActive = false;
          }
        }
      }

      if (isActive)
      {
        foreach (SectionDataSet.SectionRow childSectionRow in sectionDS.Section)
        {
          if (childSectionRow.RowState == DataRowState.Deleted)
            continue;

          if ((!sectionRow.IsIdReinforcementNull() && childSectionRow.Id == sectionRow.IdReinforcement && !childSectionRow.IsActive) ||
              (!sectionRow.IsIdRawSectionNull() && childSectionRow.Id == sectionRow.IdRawSection && !childSectionRow.IsActive) ||
              (!sectionRow.IsIdArcRawSectionNull() && childSectionRow.Id == sectionRow.IdArcRawSection && !childSectionRow.IsActive))
          {
            if (!lastDialogResult.HasValue || lastDialogResult.Value == System.Windows.Forms.DialogResult.OK)
            {
              lastDialogResult = XtraMessageBox.Show(this,
                                                    Properties.Resources.MsgErrorActiveRelationships,
                                                    Properties.Resources.CaptionAttentionMsgBox,
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Exclamation);
            }

            if (lastDialogResult.Value == DialogResult.Yes)
            {
              childSectionRow.IsActive = true;
            }
          }
        }

        // La activarea parintelui fie se reactiveaza toate componentele fie se renunta la activarea initiala.
        foreach (SectionDataSet.SectionRow childSectionRow in sectionRow.ChildSectionRows)
        {
          if (childSectionRow.RowState == DataRowState.Deleted || childSectionRow.IsActive)
            continue;

          if (!lastDialogResult.HasValue || lastDialogResult.Value == System.Windows.Forms.DialogResult.OK)
          {
            lastDialogResult = XtraMessageBox.Show(this,
                                                    Properties.Resources.MsgErrorSectionCustomParentActivate,
                                                    Properties.Resources.CaptionQuestionMsgBox,
                                                    messageBoxButtons,
                                                    MessageBoxIcon.Question);
          }

          if (lastDialogResult.Value == DialogResult.Cancel)
          {
            return;
          }

          if (lastDialogResult.Value == DialogResult.Yes)
          {
            childSectionRow.IsActive = true;
            continue;
          }

          sectionRow.IsActive = false;
        }

        // La activarea unei componente utilizatorul poate opta si pentru activarea parintelul daca toti fratii componentei active sunt activi. 
        DialogResult? reactivateParentDialogResult = null;
        foreach (SectionDataSet.SectionRow parentSectionRow in sectionRow.ParentSectionRows)
        {
          // Se verifica daca parintele este activ.
          if (parentSectionRow.RowState == DataRowState.Deleted || parentSectionRow.IsActive)
            continue;

          // Se verifica daca exista cel putin un frate inactiv.
          bool allSiblingsActive = true;
          foreach (SectionDataSet.SectionRow siblingSectionRow in parentSectionRow.ChildSectionRows)
          {
            if (!siblingSectionRow.IsActive)
            {
              allSiblingsActive = false;
              break;
            }
          }
          if (!allSiblingsActive)
            continue;

          // Se cere confirmarea utilizatorului pentru activarea parintelui
          if (!reactivateParentDialogResult.HasValue)
          {
            reactivateParentDialogResult = XtraMessageBox.Show(this,
                                                    Properties.Resources.MsgErrorSectionCustomChildActivate,
                                                    Properties.Resources.CaptionQuestionMsgBox,
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question);
          }

          if (reactivateParentDialogResult.Value == DialogResult.Yes)
          {
            parentSectionRow.IsActive = true;
          }
        }
      }
    }


    /// <summary>
    /// Verifica tipul profilului.
    /// </summary>
    private bool ValidateSectionType(SectionDataSet.SectionRow sectionRow)
    {
      SectionType sectionType = sectionRow.ConvertedSectionType;
      bool profileUsedAsReinforcement = false;

      // Daca profilul nu este armatura ne asiguram ca nu avem profile armate cu el.
      // Situatia este posibila daca intial acest profil era armatura
      if (sectionType != SectionType.Reinforcement)
      {
        //filtru daca se gaseste in lista de armaturi a unui profil
        string filterSectionReinforcementRows = string.Format("{0} = {1}",
               sectionDS.SectionReinforcement.IdSectionReinforcementColumn.ColumnName,
               sectionRow.Id);


        SectionDataSet.SectionReinforcementRow[] sectionReinforcementRows = (SectionDataSet.SectionReinforcementRow[])sectionDS.SectionReinforcement.Select(filterSectionReinforcementRows);

        if (sectionReinforcementRows.Length > 0)
        {
          profileUsedAsReinforcement = true;
          DialogResult result = XtraMessageBox.Show(this,
                                   Properties.Resources.MsgErrorSectionReinforcementUsed,
                                   Properties.Resources.CaptionAttentionMsgBox,
                                   MessageBoxButtons.YesNo,
                                   MessageBoxIcon.Exclamation);
          if (result == DialogResult.No)
          {
            return false;
          }

          //filtru daca e armatura implicita
          string filterSectionRows = string.Format("{0} = {1}",
                 sectionDS.Section.IdReinforcementColumn.ColumnName,
                 sectionRow.Id);
          //filtru daca se gaseste in armaturile din listele de culori
          string filterSectionColorListRows = string.Format("{0} = {1}",
                 sectionDS.SectionColorList.IdReinforcementColumn.ColumnName,
                 sectionRow.Id);
          string filterSectionColorListItemRows = string.Format("{0} = {1}",
                 sectionDS.SectionColorListItem.IdReinforcementColumn.ColumnName,
                 sectionRow.Id);

          SectionDataSet.SectionRow[] defaultReinforcementRows = (SectionDataSet.SectionRow[])sectionDS.Section.Select(filterSectionRows);
          SectionDataSet.SectionColorListRow[] sectionColorListRows = (SectionDataSet.SectionColorListRow[])sectionDS.SectionColorList.Select(filterSectionColorListRows);
          SectionDataSet.SectionColorListItemRow[] sectionColorListItemRows = (SectionDataSet.SectionColorListItemRow[])sectionDS.SectionColorListItem.Select(filterSectionColorListItemRows);

          //se sterge din lista de armaturi a profilelor
          foreach (SectionDataSet.SectionReinforcementRow reinforcedSection in sectionReinforcementRows)
          {
            reinforcedSection.Delete();
          }

          //se seteaza armatura implicita nula
          foreach (SectionDataSet.SectionRow reinforcedSection in defaultReinforcementRows)
          {
            reinforcedSection.SetIdReinforcementNull();
          }

          //se sterge din lista de culori
          foreach (SectionDataSet.SectionColorListRow reinforcedSection in sectionColorListRows)
          {
            reinforcedSection.SetIdReinforcementNull();
          }

          foreach (SectionDataSet.SectionColorListItemRow reinforcedSection in sectionColorListItemRows)
          {
            reinforcedSection.SetIdReinforcementNull();
          }
        }
      }

      // Daca profilul nu este profil brut ne asiguram ca nici un alt profil nu-l foloseste cu acest rol.
      // Situatia este posibila daca inainte de modificarea randului acesta era rofil brut
      if (sectionType != SectionType.RawSection)
      {
        string filter = string.Format("[{0}] = {2} OR [{1}] = {2}",
              sectionDS.Section.IdRawSectionColumn.ColumnName,
              sectionDS.Section.IdArcRawSectionColumn.ColumnName,
              sectionRow.Id);
        if (sectionDS.Section.Select(filter).Length > 0)
        {
          Debug.Assert(!profileUsedAsReinforcement, "The profile was used as reinforcement an as raw profile!");
          if (!profileUsedAsReinforcement)
          {
            DialogResult result = XtraMessageBox.Show(this,
                                    Properties.Resources.MsgErrorRawSectionUsed,
                                    Properties.Resources.CaptionAttentionMsgBox,
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
            {
              return false;
            }
          }

          foreach (SectionDataSet.SectionRow section in sectionDS.Section.Select(filter))
          {
            section.SetIdRawSectionNull();
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Randurile cu profile vizibile in grid ce vor avea preturile exportate.
    /// </summary>
    /// <returns></returns>
    private List<SectionDataSet.SectionRow> GetViewSectionRows()
    {
      List<SectionDataSet.SectionRow> sectionRows = new List<SectionDataSet.SectionRow>();

      for (int i = 0; i < viewSection.DataRowCount; i++)
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(i) as SectionDataSet.SectionRow;
        if (sectionRow != null && sectionRow.ChildSectionRows.Count() > 0)
          continue;

        sectionRows.Add(sectionRow);
      }

      return sectionRows;
    }

    #endregion Private Methods

    #region Event Handlers

    private void viewSection_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
    {
      GridView view = sender as GridView;
      bool enableRowEditTools = false;
      int[] selectedRowsHandles = view.GetSelectedRows();
      if (selectedRowsHandles != null && selectedRowsHandles.Length > 0)
      {
        enableRowEditTools = true;
      }
      bbCut.Enabled = enableRowEditTools && openingMode == FormOpeningMode.Normal;
      bbCopy.Enabled = enableRowEditTools && openingMode == FormOpeningMode.Normal;
      if (view.IsNewItemRow(view.FocusedRowHandle))
      {
        bbEditItem.Enabled = false;
      }
      else
      {
        bbEditItem.Enabled = enableRowEditTools;
      }
      bbNewItem.Enabled = openingMode == FormOpeningMode.Normal;
      bbDelete.Enabled = enableRowEditTools && openingMode == FormOpeningMode.Normal;
      bbDeleteProfileSystem.Enabled = enableRowEditTools && openingMode == FormOpeningMode.Normal;

      foreach (BarItemLink link in popupMenu.ItemLinks)
      {
        if (link.Item == bbNewItem)
        {
          if (view.IsNewItemRow(view.FocusedRowHandle))
          {
            link.Visible = true;
          }
          else
          {
            link.Visible = false;
          }
        }
        if (link.Item == bbEditItem)
        {
          if (view.IsNewItemRow(view.FocusedRowHandle))
          {
            link.Visible = false;
          }
          else
          {
            link.Visible = true;
          }
        }
      }
    }

    private void repLookUpSeries_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      if (e.Button.Kind == ButtonPredefines.Delete)
      {
        // se face cast la BaseEdit pentru a putea afisa NullText in control (altfel nu merge)
        BaseEdit editor = sender as BaseEdit;
        editor.EditValue = null;
        // se seteaza EditValue pe null pe BarEditItem pentru ca altfel nu se intra in evenimentul de EditValueChanged
        barEditSeries.EditValue = null;
        editor.DoValidate();
      }
    }

    private void repLookUpContainsSection_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      if (e.Button.Kind == ButtonPredefines.Delete)
      {
        // se face cast la BaseEdit pentru a putea afisa NullText in control (altfel nu merge)
        BaseEdit editor = sender as BaseEdit;
        editor.EditValue = null;
        // se seteaza EditValue pe null pe BarEditItem pentru ca altfel nu se intra in evenimentul de EditValueChanged
        barEditContainsSection.EditValue = null;
        editor.DoValidate();
      }
    }

    private void repLookUpContainsTolerance_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      if (e.Button.Kind == ButtonPredefines.Delete)
      {
        // se face cast la BaseEdit pentru a putea afisa NullText in control (altfel nu merge)
        BaseEdit editor = sender as BaseEdit;
        editor.EditValue = null;
        // se seteaza EditValue pe null pe BarEditItem pentru ca altfel nu se intra in evenimentul de EditValueChanged
        barEditContainsTolerance.EditValue = null;
        editor.DoValidate();
      }
    }

    private void repLookUpContainsReinforcement_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      if (e.Button.Kind == ButtonPredefines.Delete)
      {
        //se face cast la BaseEdit pentru a puteaa afisa NullText in control (altfel nu merge)
        BaseEdit editor = sender as BaseEdit;
        editor.EditValue = null;
        // se seteaza EditValue pe null pe BarEditItem ca altfel nu se intra in evenimentul de EditValueChanged
        barEditContainsReinforcement.EditValue = null;
        editor.DoValidate();
      }
    }

    private void barEditFilter_EditValueChanged(object sender, EventArgs e)
    {
      using (new WaitCursor())
      {
        BarEditItem barEditItem = sender as BarEditItem;
        if (barEditItem == null)
          return;

        viewSection.RefreshData();
        barEditItem.Hint = barEditItem.Edit.GetDisplayText(barEditItem.EditValue);
      }
    }

    private void barEditPriceCatalogFilter_EditValueChanged(object sender, EventArgs e)
    {
      if (updatesEnabled)
      {
        RefreshData(false);
      }

      SetEditorsPriceCatalogReadOnly();
    }

    private void repLookUpPriceCatalog_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      if (e.Button.Kind == ButtonPredefines.Delete)
      {
        // se face cast la BaseEdit pentru a putea afisa NullText in control (altfel nu merge)
        BaseEdit editor = sender as BaseEdit;
        editor.EditValue = null;
        // se seteaza EditValue pe null pe BarEditItem pentru ca altfel nu se intra in evenimentul de EditValueChanged
        barEditPriceCatalog.EditValue = null;
        editor.DoValidate();
      }
    }

    private void repLookUpPriceCatalog_EditValueChanging(object sender, ChangingEventArgs e)
    {
      viewSection.CloseEditor();
      if (!viewSection.UpdateCurrentRow())
      {
        e.Cancel = true;
        return;
      }

      if (!sectionDS.HasChanges())
      {
        return;
      }

      DialogResult result = XtraMessageBox.Show(this,
                                                Properties.Resources.MsgSaveSectionQuestion,
                                                Properties.Resources.CaptionAttentionMsgBox,
                                                MessageBoxButtons.YesNoCancel,
                                                MessageBoxIcon.Question);
      if (result == DialogResult.Cancel)
      {
        e.Cancel = true;
        return;
      }

      if (result == DialogResult.Yes)
      {
        if (!ApplyChanges(false))
        {
          e.Cancel = true;
          return;
        }
      }
      // 26.10. 2016 BiancaB - am setat valoarea din lookup pt ca pe result = NO nu se schimba valoarea
      //din lookup
      barEditPriceCatalog.EditValue = e.NewValue;
    }

    private void repImage_EditValueChanging(object sender, ChangingEventArgs e)
    {
      Image img = e.NewValue as Image;
      if (img != null)
      {
        MemoryStream mStream = new MemoryStream();
        img.Save(mStream, ImageFormat.Jpeg);
        byte[] ret = mStream.ToArray();
        mStream.Close();
        e.NewValue = ret;
      }
    }

    private void repBaseEdit_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      if (e.Button.Kind == ButtonPredefines.Delete)
      {
        PopupBaseEdit popupEdit = sender as PopupBaseEdit;
        if (popupEdit != null)
          popupEdit.ClosePopup();

        BaseEdit baseEdit = sender as BaseEdit;
        baseEdit.EditValue = null;
      }
    }

    private void repImage_ButtonPressed(object sender, ButtonPressedEventArgs e)
    {
      ImageEdit imgEdit = sender as ImageEdit;
      if (e.Button.Kind == ButtonPredefines.Ellipsis)
      {
        using (OpenFileDialog dlg = new OpenFileDialog())
        {
          dlg.Multiselect = false;
          dlg.CheckFileExists = true;
          dlg.CheckPathExists = true;
          dlg.Filter = string.Format(Properties.Resources.FormatOpenDialogFilterImages, Settings.Default.ImageFilter);
          dlg.FilterIndex = 0;
          dlg.ValidateNames = true;
          if (dlg.ShowDialog(this) == DialogResult.OK)
          {
            try
            {
              Stream imageStream = dlg.OpenFile();

              int length = Convert.ToInt32(imageStream.Length);
              byte[] imageBuffer = new byte[length];
              if ((imageBuffer.Length / 1024) > Properties.Settings.Default.ImageMaximumSize)
              {
                XtraMessageBox.Show(this, String.Format(Properties.Resources.ProfileImageTooBig)
                                  , Properties.Resources.CaptionAttentionMsgBox
                                  , MessageBoxButtons.OK
                                  , MessageBoxIcon.Warning);
                return;
              }
              imageStream.Read(imageBuffer, 0, length);
              imgEdit.EditValue = imageBuffer;
            }
            catch
            {
              imgEdit.EditValue = null;
            }
          }
        }
      }
      if (e.Button.Kind == ButtonPredefines.Delete)
      {
        if (XtraMessageBox.Show(this, Properties.Resources.MsgDeleteImageQuestion,
          Properties.Resources.CaptionAttentionMsgBox,
          MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
          imgEdit.Image = null;
      }
    }

    private void viewSection_DoubleClick(object sender, EventArgs e)
    {
      GridView view = sender as GridView;
      if (view == null)
      {
        return;
      }
      GridHitInfo hitInfo = view.CalcHitInfo(view.GridControl.PointToClient(Cursor.Position));

      if (hitInfo.InRow || hitInfo.InRowCell)
      {
        SectionDataSet.SectionRow sectionRow = view.GetDataRow(view.FocusedRowHandle) as SectionDataSet.SectionRow;
        if (sectionRow != null)
        {
          ShowSectionEditDialog(sectionRow);
        }
      }
    }

    private void repText_Leave(object sender, EventArgs e)
    {
      TextEdit editor = sender as TextEdit;
      try
      {
        if (viewSection.FocusedColumn == colCode)
        {
          if (viewSection.FocusedRowHandle == GridControl.NewItemRowHandle)
          {
            return;
          }
          object value = viewSection.GetRowCellValue(viewSection.FocusedRowHandle, colDesignation);
          if (value == null)
          {
            return;
          }
          if (string.IsNullOrEmpty(value.ToString()))
          {
            string code = viewSection.GetRowCellValue(viewSection.FocusedRowHandle, colCode).ToString();
            viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colDesignation, code);
          };
        }
      }
      catch { }
    }

    private void viewSection_InvalidRowException(object sender, InvalidRowExceptionEventArgs e)
    {
      if (e.Exception is WarningException && String.IsNullOrEmpty(e.ErrorText))
      {
        e.ExceptionMode = ExceptionMode.Ignore;
        return;
      }
      if (e.ErrorText == Properties.Resources.MsgErrorActiveInactiveDataReverted)
      {
        e.ExceptionMode = ExceptionMode.NoAction;
        return;
      }

      e.ExceptionMode = gridUtils.TreatInvalidRowException((GridView)sender, e.Exception);
    }

    private void gridUtils_BeforeCopying(object sender, BeforeCopyingEventArgs e)
    {
      // In cazul unui profil ce face parte dintr-unul compus, nu copiez 
      // informatiile din CustomSectionItem pentru ca noul profil (cel copiat)
      // ar deveni si el componenta a acelui profil compus.
      if (e.Row != null && e.Row.GetType() == typeof(SectionDataSet.CustomSectionItemRow))
      {
        if (e.Relation.ChildColumns[0].ColumnName == sectionDS.CustomSectionItem.IdSectionColumn.ColumnName)
        {
          e.Cancel = true;
        }
      }

      if (e.Row != null && e.Row.GetType() == typeof(SectionDataSet.SectionReinforcementRow))
      {
        if (e.Relation.ChildColumns[0].ColumnName == sectionDS.SectionReinforcement.IdSectionReinforcementColumn.ColumnName)
        {
          e.Cancel = true;
        }
      }

      if (e.Row != null && e.Row.GetType() == typeof(SectionDataSet.SectionCoverRow))
      {
        if (e.Relation.ChildColumns[0].ColumnName == sectionDS.SectionCover.IdSectionCoverColumn.ColumnName)
        {
          e.Cancel = true;
        }
      }

      if (e.Row != null && e.Row.GetType() == typeof(SectionDataSet.SectionToleranceRow))
      {
        if (e.Relation.ChildColumns[0].ColumnName == sectionDS.SectionTolerance.IdSection2Column.ColumnName)
        {
          e.Cancel = true;
        }
      }

      if (e.Row != null && e.Row.GetType() == typeof(SectionDataSet.SectionHeatTransferCoefficientRow))
      {
        if (e.Relation.ChildColumns[0].ColumnName == sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName ||
            e.Relation.ChildColumns[0].ColumnName == sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName)
        {
          e.Cancel = true;
        }
      }

      // Fii unui profil brut si ai unei armaturi nu se copiaza
      if (e.Relation != null && (e.Relation.ChildColumns[0].ColumnName == sectionDS.Section.IdReinforcementColumn.ColumnName ||
                                  e.Relation.ChildColumns[0].ColumnName == sectionDS.Section.IdRawSectionColumn.ColumnName ||
                                  e.Relation.ChildColumns[0].ColumnName == sectionDS.Section.IdArcRawSectionColumn.ColumnName))
      {
        e.Cancel = true;
      }
    }

    private void gridUtils_BeforePasting(object sender, BeforePastingEventArgs e)
    {
      /*In cazul unei armaturi ce face parte dintr-un profil, nu fac paste cu
       informatiile din sectionReinforcementRow pentru ca noua armatura (cea copiata)
       ar deveni si ea componenta a acelui profil.
       In originalRow.Table.DataSet.Section avem randurile ce au fost copiate initial
       nu sunt alte randuri deorece adancimea cautarii este de 1, iar in gridUtils a fost
       restrictionata cautarea copiilor de pe o relatie ce are ca parinte si copil aceasi tabela
       Modificarile au fost facute pentru a face copierea multipla sa fie la fel ca si copierea individuala. */
      if (e.Row is SectionDataSet.SectionReinforcementRow)
      {
        SectionDataSet.SectionReinforcementRow sectionReinforcementRow = e.Row as SectionDataSet.SectionReinforcementRow;
        SectionDataSet.SectionReinforcementRow originalRow = e.OriginalRow as SectionDataSet.SectionReinforcementRow;
        if (((SectionDataSet)originalRow.Table.DataSet).Section.FindById(sectionReinforcementRow.IdSection) != null)
        {
          e.Cancel = true;
        }
      }
      if (e.Row is SectionDataSet.CustomSectionItemRow)
      {
        SectionDataSet.CustomSectionItemRow sectionCustomRow = e.Row as SectionDataSet.CustomSectionItemRow;
        SectionDataSet.CustomSectionItemRow originalRow = e.OriginalRow as SectionDataSet.CustomSectionItemRow;
        if (((SectionDataSet)originalRow.Table.DataSet).Section.FindById(sectionCustomRow.IdParentSection) != null)
        {
          e.Cancel = true;
        }
      }
      if (e.Row is SectionDataSet.SectionToleranceRow)
      {
        SectionDataSet.SectionToleranceRow sectionToleranceRow = e.Row as SectionDataSet.SectionToleranceRow;
        SectionDataSet.SectionToleranceRow originalRow = e.OriginalRow as SectionDataSet.SectionToleranceRow;
        if (((SectionDataSet)originalRow.Table.DataSet).Section.FindById(sectionToleranceRow.IdSection1) != null)
        {
          e.Cancel = true;
        }
      }
      if (e.Row is SectionDataSet.SeriesSectionRow)
      {
        SectionDataSet.SeriesSectionRow seriesSectionRow = e.Row as SectionDataSet.SeriesSectionRow;
        SectionDataSet.SeriesSectionRow originalRow = e.OriginalRow as SectionDataSet.SeriesSectionRow;
        if (((SectionDataSet)originalRow.Table.DataSet).Section.FindById(seriesSectionRow.IdSection) != null)
        {
          e.Cancel = true;
        }
      }

      if (e.Row is SectionDataSet.SectionRow)
      {
        SectionDataSet.SectionRow sectionRow = e.Row as SectionDataSet.SectionRow;
        SectionDataSet.SectionRow originalSectionRow = e.OriginalRow as SectionDataSet.SectionRow;
        sectionRow.Code = gridUtils.GetNext_Code(sectionRow.Code, sectionDS.Section.CodeColumn);
        imageDS.Image.SetImage(sectionRow, imageDS.Image.GetImage(originalSectionRow.Guid));
      }
      else if (e.Row is SectionDataSet.SectionColorListItemRow)
      {
        SectionDataSet.SectionColorListItemRow sectionColorListItemRow = e.Row as SectionDataSet.SectionColorListItemRow;
        sectionColorListItemRow.Code = sectionColorListItemRow.SectionColorListRow.SectionRow.CreateCodeWithColorCombination(colorDS.ColorCombination.FindById(sectionColorListItemRow.IdColorCombination), colorListDS);
        sectionColorListItemRow.GenerateUniqueCode();
      }
    }

    private void gridUtils_Deleting(object sender, DeletingEventArgs e)
    {
      SectionDataSet.SectionRow sectionRow = e.Row as SectionDataSet.SectionRow;

      string filter = string.Format("{0} = {1}",
                                     sectionDS.CustomSectionItem.IdSectionColumn.ColumnName,
                                     sectionRow.Id);
      if (sectionDS.CustomSectionItem.Select(filter).Length > 0)
      {
        e.Explanation = Properties.Resources.MsgDeleteCustomSectionItemError;
        e.Cancel = true;
        return;
      }

      if (sectionRow.ConvertedSectionType == SectionType.Reinforcement)
      {
        filter = string.Format("{0} = {1}",
                        sectionDS.SectionReinforcement.IdSectionReinforcementColumn.ColumnName,
                        sectionRow.Id);
        DataRow[] sectionReinforcementRows = sectionDS.SectionReinforcement.Select(filter).ToArray();
        if (sectionReinforcementRows.Length > 0)
        {
          DialogResult result = XtraMessageBox.Show(this,
                                    Properties.Resources.MsgErrorSectionReinforcementUsed,
                                    Properties.Resources.CaptionAttentionMsgBox,
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Exclamation);
          if (result == DialogResult.Cancel)
          {
            e.Cancel = true;
            return;
          }
          else
          {
            foreach (SectionDataSet.SectionReinforcementRow reinforcedSection in sectionDS.SectionReinforcement.Select(filter))
            {
              reinforcedSection.Delete();
            }
          }
        }
      }

      string filterSHTItems = string.Format("{0} = {1} OR {0} = {2}",
            sectionRow.Id,
            sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName,
            sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName);

      if (sectionDS.SectionHeatTransferCoefficient.Select(filterSHTItems).Length > 0)
      {
        foreach (SectionDataSet.SectionHeatTransferCoefficientRow SHTrow in
          sectionDS.SectionHeatTransferCoefficient.Select(filterSHTItems))
        {
          SHTrow.Delete();
        }
      }

      string filterTolerance = string.Format("{0} = {1} OR {0} = {2}",
      sectionRow.Id,
      sectionDS.SectionTolerance.IdSection1Column.ColumnName,
      sectionDS.SectionTolerance.IdSection2Column.ColumnName);

      if (sectionDS.SectionTolerance.Select(filterTolerance).Length > 0)
      {
        foreach (SectionDataSet.SectionToleranceRow toleranceRow in
          sectionDS.SectionTolerance.Select(filterTolerance))
        {
          toleranceRow.Delete();
        }
      }
    }

    private void barManager_QueryShowPopupMenu(object sender, QueryShowPopupMenuEventArgs e)
    {
      GridControl grid = e.Control as GridControl;
      if (grid == null)
        return;
      GridView view = grid.FocusedView as GridView;
      if (view == null)
        return;
      GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(e.Position));
      if (!hitInfo.InRow)
      {
        e.Cancel = true;
      }
    }

    /// <summary>
    /// Eveniment ce dezactiveaza functiile de Cut, Copy, Paste, Delete 
    /// cand se editeaza un control (ptr a folosi functiile locale)
    /// </summary>
    private void barManager_ShortcutItemClick(object sender, ShortcutItemClickEventArgs e)
    {
      Control control = GetFocusedControl(this, typeof(BaseEdit));
      if (control == null)
      {
        return;
      }
      BaseEdit editor = control as BaseEdit;
      if (editor == null)
      {
        return;
      }
      if (editor.EditorContainsFocus && editor.IsNeededKey(new KeyEventArgs(e.Shortcut.Key)))
      {
        e.Cancel = true;
      }
    }

    private void viewSection_ShowingEditor(object sender, CancelEventArgs e)
    {
      string materialTypeName = viewSection.GetRowCellValue(viewSection.FocusedRowHandle, colMaterialType) as string;
      string sectionTypeName = viewSection.GetRowCellValue(viewSection.FocusedRowHandle, colSectionType) as string;
      string priceComputeModeName = viewSection.GetRowCellValue(viewSection.FocusedRowHandle, colPriceCalculationType) as string;
      string rawSectionTypeName = viewSection.GetRowCellValue(viewSection.FocusedRowHandle, colRawSectionType) as string;

      MaterialType? materialType = null;
      SectionType? sectionType = null;
      PriceCalculationType? priceComputeMode = null;
      RawSectionType? rawSectionType = null;

      if (!string.IsNullOrEmpty(materialTypeName))
      {
        materialType = EnumConvert<MaterialType>.ToEnum(materialTypeName);
      }
      if (!string.IsNullOrEmpty(priceComputeModeName))
      {
        priceComputeMode = EnumConvert<PriceCalculationType>.ToEnum(priceComputeModeName);
      }
      if (!string.IsNullOrEmpty(sectionTypeName))
      {
        sectionType = EnumConvert<SectionType>.ToEnum(sectionTypeName);
      }
      if (!string.IsNullOrEmpty(rawSectionTypeName))
      {
        rawSectionType = EnumConvert<RawSectionType>.ToEnum(rawSectionTypeName);
      }

      if (viewSection.FocusedColumn == colHasThermalBreak &&
          materialType.HasValue &&
         (materialType.Value != MaterialType.Aluminium &&
          materialType.Value != MaterialType.Steel))
      {
        // coloana Are Bariera termica este editabila doar ptr aluminiu si otel
        e.Cancel = true;
        return;
      }

      if (sectionType.HasValue)
      {
        // Numarul de sine se alege doar pentru toc ferereastra glisanta
        if (viewSection.FocusedColumn == colTrackNumber)
        {
          if (sectionType.Value != SectionType.Track && sectionType.Value != SectionType.MullionWithTrack && sectionType != SectionType.TrackInsectScreen)
          {
            e.Cancel = true;
            return;
          }
        }
      }

      if (sectionType.HasValue && materialType.HasValue)
      {
        // alegerea armaturii e posibila doar pentru profilele de tip PVC care nu sunt armaturi
        if (viewSection.FocusedColumn == colIdReinforcement)
        {
          SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(viewSection.FocusedRowHandle) as SectionDataSet.SectionRow;
          if (sectionRow.GetSectionReinforcementRows().Length == 0 ||
              materialType.Value != MaterialType.PVC ||
              sectionType.Value == SectionType.Reinforcement)
          {
            e.Cancel = true;
            return;
          }
        }
      }

      if (sectionType.HasValue && materialType.HasValue && rawSectionType.HasValue)
      {
        // alegerea profilului brut e posibila doar pentru profilele de tip Lemn care nu sunt profile brute,
        // iar acestea nu sunt compuse.
        if (viewSection.FocusedColumn == colIdRawSection)
        {
          if (materialType.Value != MaterialType.Wood || sectionType.Value == SectionType.RawSection || rawSectionType != RawSectionType.Defined)
          {
            e.Cancel = true;
            return;
          }

          SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(viewSection.FocusedRowHandle) as SectionDataSet.SectionRow;
          List<SectionDataSet.CustomSectionItemRow> customSectionItemRows = sectionDS.CustomSectionItem.Where(row => row.IdParentSection == sectionRow.Id).ToList();
          if (customSectionItemRows.Count != 0)
          {
            if (XtraMessageBox.Show(this,
                              Properties.Resources.MsgErrorSectionIsCustomSection,
                              Properties.Resources.CaptionAttentionMsgBox,
                              MessageBoxButtons.YesNo,
                              MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
              foreach (SectionDataSet.CustomSectionItemRow customSectionItemRow in customSectionItemRows)
                customSectionItemRow.Delete();
            }
            else
            {
              e.Cancel = true;
              return;
            }
          }
        }
        if ((rawSectionType == RawSectionType.None && (viewSection.FocusedColumn == colRawSectionTolerance ||
              viewSection.FocusedColumn == colRawH || viewSection.FocusedColumn == colRawW || viewSection.FocusedColumn == colUsePriceOnRawSection)) ||
            (rawSectionType == RawSectionType.Defined && (viewSection.FocusedColumn == colRawH || viewSection.FocusedColumn == colRawW ||
              viewSection.FocusedColumn == colWInputTolerance || viewSection.FocusedColumn == colMaxW ||
              viewSection.FocusedColumn == colVariableW || viewSection.FocusedColumn == colUsePriceOnRawSection)))
        {
          e.Cancel = true;
          return;
        }

        object cellUseVariableW = viewSection.GetRowCellValue(viewSection.FocusedRowHandle, colVariableW);
        if (cellUseVariableW != null)
        {
          bool useVariableW = (bool)cellUseVariableW;
          if (!useVariableW && (viewSection.FocusedColumn == colWInputTolerance || viewSection.FocusedColumn == colMaxW))
          {
            e.Cancel = true;
            return;
          }
        }
      }

      object cellValue = viewSection.GetRowCellValue(viewSection.FocusedRowHandle, colIsForOptimization);
      if (cellValue != null)
      {
        bool isForOptimization = (bool)cellValue;
        if (!isForOptimization)
        {
          // daca nu se optimizeaza nu se pot edita urmatoarele coloane.
          if (viewSection.FocusedColumn == colUseDoubleCut || viewSection.FocusedColumn == colBinSize ||
              viewSection.FocusedColumn == colUseInventory || viewSection.FocusedColumn == colOptimizationMinLimitLength ||
              viewSection.FocusedColumn == colOptimizationMinInventoryLength || viewSection.FocusedColumn == colOptimizationMaxLimitLength ||
              viewSection.FocusedColumn == colOptimizationTargetInventory || viewSection.FocusedColumn == colOptimizationMaxInventory)
          {
            e.Cancel = true;
            return;
          }
        }
      }

      cellValue = viewSection.GetRowCellValue(viewSection.FocusedRowHandle, colUseInventory);
      if (cellValue != null)
      {
        bool useInventory = (bool)cellValue;
        if (!useInventory)
        {
          // daca nu se optimizeaza din stoc nu se pot edita urmatoarele coloane.
          if (viewSection.FocusedColumn == colOptimizationMinLimitLength ||
              viewSection.FocusedColumn == colOptimizationMinInventoryLength || viewSection.FocusedColumn == colOptimizationMaxLimitLength ||
              viewSection.FocusedColumn == colOptimizationTargetInventory || viewSection.FocusedColumn == colOptimizationMaxInventory)
          {
            e.Cancel = true;
            return;
          }
        }
      }

      cellValue = viewSection.GetRowCellValue(viewSection.FocusedRowHandle, colExtendingMode);
      if (cellValue != null)
      {
        if (cellValue.ToString() == ExtendingMode.None.ToString())
        {
          if (viewSection.FocusedColumn == colMinSegmentLength || viewSection.FocusedColumn == colMaxSegmentLength ||
            viewSection.FocusedColumn == colMaxSegmentedLength)
          {
            e.Cancel = true;
            return;
          }
        }
      }
    }

    private void repLookUpSectionMaterialType_EditValueChanging(object sender, ChangingEventArgs e)
    {
      if (e.NewValue == null || viewSection.IsFilterRow(viewSection.FocusedRowHandle))
        return;

      MaterialType materialType = EnumConvert<MaterialType>.ToEnum(e.NewValue.ToString());
      MaterialType oldMaterialType = EnumConvert<MaterialType>.ToEnum(e.OldValue.ToString());

      int rowHandle = viewSection.FocusedRowHandle;
      SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(rowHandle) as SectionDataSet.SectionRow;
      if (sectionRow == null)
        return;

      if (oldMaterialType == MaterialType.Wood && sectionRow.ConvertedSectionType == SectionType.RawSection)
      {
        XtraMessageBox.Show(this,
           Properties.Resources.MsgErrorSectionIsNotWood,
           Properties.Resources.CaptionAttentionMsgBox,
           MessageBoxButtons.OK,
           MessageBoxIcon.Exclamation);
        e.Cancel = true;
        return;
      }
      if (materialType != oldMaterialType &&
       (sectionRow.GetSectionMaterialSpeciesRows().FirstOrDefault(row => row.RowState != DataRowState.Deleted) != null ||
        sectionRow.GetSectionHeatTransferCoefficientRowsByFkSectionSectionsHeatTransferCoefficient1().FirstOrDefault(row => row.RowState != DataRowState.Deleted && !row.IsIdMaterialSpeciesNull()) != null ||
        sectionRow.GetSectionHeatTransferCoefficientRowsByFkSectionSectionsHeatTransferCoefficient2().FirstOrDefault(row => row.RowState != DataRowState.Deleted && !row.IsIdMaterialSpeciesNull()) != null))
      {
        if (XtraMessageBox.Show(this,
             Properties.Resources.MsgWarningSectionMaterialSpecies,
             Properties.Resources.CaptionAttentionMsgBox,
             MessageBoxButtons.OKCancel,
             MessageBoxIcon.Exclamation) == DialogResult.Cancel)
        {
          e.Cancel = true;
          return;
        }
        else
        {
          foreach (SectionDataSet.SectionMaterialSpeciesRow row in sectionRow.GetSectionMaterialSpeciesRows())
            row.Delete();

          List<SectionDataSet.SectionHeatTransferCoefficientRow> sectionHeatTransfer1Rows = sectionRow.GetSectionHeatTransferCoefficientRowsByFkSectionSectionsHeatTransferCoefficient1().Where(row => !row.IsIdMaterialSpeciesNull()).ToList();
          foreach (SectionDataSet.SectionHeatTransferCoefficientRow row in sectionHeatTransfer1Rows)
            row.Delete();

          List<SectionDataSet.SectionHeatTransferCoefficientRow> sectionHeatTransfer2Rows = sectionRow.GetSectionHeatTransferCoefficientRowsByFkSectionSectionsHeatTransferCoefficient2().Where(row => !row.IsIdMaterialSpeciesNull()).ToList();
          foreach (SectionDataSet.SectionHeatTransferCoefficientRow row in sectionHeatTransfer2Rows)
            row.Delete();
        }
      }

      switch (materialType)
      {
        case MaterialType.Aluminium:
          {
            //viewSection.SetRowCellValue(rowHandle, colIdReinforcement, null);
            viewSection.SetRowCellValue(rowHandle, colIdRawSection, null);
            if (sectionRow.PriceCalculationType != PriceCalculationType.PerWeight.ToString())
            {
              viewSection.SetRowCellValue(rowHandle, colPriceCalculationType, PriceCalculationType.PerWeight);
            }
            break;
          }

        case MaterialType.PVC:
          {
            viewSection.SetRowCellValue(rowHandle, colIdRawSection, null);
            if (sectionRow.PriceCalculationType != PriceCalculationType.PerLength.ToString())
            {
              viewSection.SetRowCellValue(rowHandle, colPriceCalculationType, PriceCalculationType.PerLength);
            }
            break;
          }

        case MaterialType.Steel:
          {
            viewSection.SetRowCellValue(rowHandle, colIdReinforcement, null);
            viewSection.SetRowCellValue(rowHandle, colIdRawSection, null);
            if (sectionRow.PriceCalculationType != PriceCalculationType.PerLength.ToString())
            {
              viewSection.SetRowCellValue(rowHandle, colPriceCalculationType, PriceCalculationType.PerLength);
            }
            break;
          }

        case MaterialType.Wood:
          {
            viewSection.SetRowCellValue(rowHandle, colIdReinforcement, null);
            if (sectionRow.PriceCalculationType != PriceCalculationType.PerLength.ToString())
            {
              viewSection.SetRowCellValue(rowHandle, colPriceCalculationType, PriceCalculationType.PerLength);
            }
            break;
          }
      }
    }

    private void repLookUpSectionType_EditValueChanged(object sender, EventArgs e)
    {
      if (viewSection.IsFilterRow(viewSection.FocusedRowHandle))
        return;

      LookUpEdit editBase = sender as LookUpEdit;

      SectionType sectionType = EnumConvert<SectionType>.ToEnum(editBase.EditValue.ToString());

      if (sectionType == SectionType.Track || sectionType == SectionType.MullionWithTrack || sectionType == SectionType.TrackInsectScreen)
      {
        int trackNumber = Convert.ToInt32(viewSection.GetRowCellValue(viewSection.FocusedRowHandle, colTrackNumber));
        if (trackNumber == 0)
        {
          viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colTrackNumber, Properties.Settings.Default.SectionDefaultTrackNumber);
        }
      }

      // Setare default Mod Fixare Suprafete.
      switch (sectionType)
      {
        case SectionType.Frame:
        case SectionType.SashWindowInt:
        case SectionType.SashWindowExt:
        case SectionType.SashDoorInt:
        case SectionType.SashDoorExt:
        case SectionType.Mullion:
        case SectionType.BottomRail:
          viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colFixingMode, SurfaceFixingMode.Bead);
          break;
        default:
          viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colFixingMode, SurfaceFixingMode.None);
          break;
      }
    }

    private void repLookUpRawSectionType_EditValueChanged(object sender, EventArgs e)
    {
      LookUpEdit editBase = sender as LookUpEdit;

      RawSectionType rawSectionType = EnumConvert<RawSectionType>.ToEnum(editBase.EditValue.ToString());

      if (rawSectionType == RawSectionType.Defined)
      {
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colVariableW, false);
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colWInputTolerance, 0);
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colMaxW, 0);
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colRawH, 0);
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colRawW, 0);
      }
      else if (rawSectionType == RawSectionType.None)
      {
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colRawSectionTolerance, 0);
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colRawH, 0);
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colRawW, 0);
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colIdRawSection, null);
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colUsePriceOnRawSection, false);
      }
      else if (rawSectionType == RawSectionType.Dynamic)
      {
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colIdRawSection, null);
      }
    }

    /// <summary>
    /// Verifica daca profilul de armare selectat este inclus in toate seriile in care apare si profilul editat
    /// </summary>
    private void repGridLookUpSectionReinforcement_EditValueChanging(object sender, ChangingEventArgs e)
    {
      if (e.NewValue == null || viewSection.IsFilterRow(viewSection.FocusedRowHandle))
        return;

      int idReinforcement = (int)e.NewValue;
      SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(viewSection.FocusedRowHandle) as SectionDataSet.SectionRow;
      if (sectionRow == null)
        return;

      // cautare in toate seriile ce contin profilul curent
      string filterSeries = string.Format("{0} = {1}",
                              sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                              sectionRow.Id);

      bool validReinforcement = true;
      // se verifica daca fiecare serie in care se afla profilul curent, contine
      // armatura selectata
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        string filterSeriesSection = string.Format("{0} = {1} AND {2} = {3}",
                                              sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                                              seriesSectionRow.IdSeries,
                                              sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                                              idReinforcement);
        if (sectionDS.SeriesSection.Select(filterSeriesSection).Length == 0)
        {
          validReinforcement = false;
        }
      }

      if (!validReinforcement)
      {
        DialogResult result = XtraMessageBox.Show(this,
                        Properties.Resources.MsgErrorSectionReinforcementInvalid,
                        Properties.Resources.CaptionAttentionMsgBox,
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Exclamation);
        if (result == DialogResult.Cancel)
        {
          e.Cancel = true;
          return;
        }

        // Adaugare armaturi necesare
        foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
        {
          string filterSeriesSection = string.Format("{0} = {1} AND {2} = {3}",
                                                sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                                                seriesSectionRow.IdSeries,
                                                sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                                                idReinforcement);
          // daca armatura aleasa nu exista in aceasta serie, o adaug
          if (sectionDS.SeriesSection.Select(filterSeriesSection).Length == 0)
          {
            SectionDataSet.SeriesSectionRow newSeriesSectionRow = sectionDS.SeriesSection.NewSeriesSectionRow();
            newSeriesSectionRow.IdSeries = seriesSectionRow.IdSeries;
            newSeriesSectionRow.IdSection = idReinforcement;
            sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
          }
        }
      }
    }

    /// <summary>
    /// Verifica daca profilul brut selectat este inclus in toate seriile in care apare si profilul editat
    /// </summary>
    private void repLookUpIdRawSection_EditValueChanging(object sender, ChangingEventArgs e)
    {
      if (e.NewValue == null || viewSection.IsFilterRow(viewSection.FocusedRowHandle))
        return;

      int idRawSection = (int)e.NewValue;
      SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(viewSection.FocusedRowHandle) as SectionDataSet.SectionRow;
      if (sectionRow == null)
        return;

      // cautare in toate seriile ce contin profilul curent
      string filterSeries = string.Format("{0} = {1}",
                              sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                              sectionRow.Id);

      bool validRawSection = true;
      // se verifica daca fiecare serie in care se afla profilul curent, contine
      // profilul brut selectat
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        string filterSeriesSection = string.Format("{0} = {1} AND {2} = {3}",
                                              sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                                              seriesSectionRow.IdSeries,
                                              sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                                              idRawSection);
        if (sectionDS.SeriesSection.Select(filterSeriesSection).Length == 0)
        {
          validRawSection = false;
        }
      }

      if (!validRawSection)
      {
        DialogResult result = XtraMessageBox.Show(this,
                        Properties.Resources.MsgErrorSectionRawSectionInvalid,
                        Properties.Resources.CaptionAttentionMsgBox,
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Exclamation);
        if (result == DialogResult.Cancel)
        {
          e.Cancel = true;
          return;
        }

        // Adaugare profil brut
        foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
        {
          string filterSeriesSection = string.Format("{0} = {1} AND {2} = {3}",
                                                sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                                                seriesSectionRow.IdSeries,
                                                sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                                                idRawSection);
          // daca profilul brut aleas nu exista in aceasta serie, il adaug
          if (sectionDS.SeriesSection.Select(filterSeriesSection).Length == 0)
          {
            SectionDataSet.SeriesSectionRow newSeriesSectionRow = sectionDS.SeriesSection.NewSeriesSectionRow();
            newSeriesSectionRow.IdSeries = seriesSectionRow.IdSeries;
            newSeriesSectionRow.IdSection = idRawSection;
            sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
          }
        }
      }

      if (e.OldValue == DBNull.Value && viewSection.FocusedColumn == colIdRawSection)
      {
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colRawSectionTolerance, Properties.Settings.Default.SectionDefaultRawSectionTolerance);
      }
      else if (e.OldValue == DBNull.Value && viewSection.FocusedColumn == colIdArcRawSection)
      {
        viewSection.SetRowCellValue(viewSection.FocusedRowHandle, colArcRawSectionTolerance, Properties.Settings.Default.SectionDefaultRawSectionTolerance);
      }
    }

    private void viewSection_ValidateRow(object sender, ValidateRowEventArgs e)
    {
      SectionDataSet.SectionRow currentSectionRow = viewSection.GetDataRow(e.RowHandle) as SectionDataSet.SectionRow;
      string code = currentSectionRow[sectionDS.Section.CodeColumn.ColumnName] as string;

      if (string.IsNullOrEmpty(code))
      {
        e.ErrorText = string.Format(Properties.Resources.MsgErrorRowFieldEmpty, colCode.Caption);
        e.Valid = false;
        return;
      }

      SectionDataSet.SectionRow sameCodeSectionRow = sectionDS.Section.FirstOrDefault(row => row.RowState != DataRowState.Deleted && row.Code == code && row != currentSectionRow);
      if (sameCodeSectionRow != null)
      {
        e.ErrorText = string.Format(Properties.Resources.MsgErrorRowCodeConstraint, colCode.Caption);
        e.Valid = false;
        return;
      }

      string designation = currentSectionRow[sectionDS.Section.DesignationColumn.ColumnName] as string;
      if (string.IsNullOrEmpty(designation))
      {
        e.ErrorText = string.Format(Properties.Resources.MsgErrorRowFieldEmpty, colDesignation.Caption);
        e.Valid = false;
        return;
      }

      decimal minSegmentLength = Convert.ToDecimal(currentSectionRow[sectionDS.Section.MinSegmentLengthColumn.ColumnName]);
      decimal maxSegmentLength = Convert.ToDecimal(currentSectionRow[sectionDS.Section.MaxSegmentLengthColumn.ColumnName]);
      decimal maxSegmentedLength = Convert.ToDecimal(currentSectionRow[sectionDS.Section.MaxSegmentedLengthColumn.ColumnName]);

      if (minSegmentLength < 0 || maxSegmentedLength < maxSegmentLength || maxSegmentLength < 2 * minSegmentLength)
      {
        e.ErrorText = string.Format(Properties.Resources.MsgErrorSegmentLengthInvalid, colMinSegmentLength.Caption, colMaxSegmentLength);
        e.Valid = false;
        return;
      }

      if (currentSectionRow[sectionDS.Section.SectionTypeColumn.ColumnName] != DBNull.Value)
      {
        SectionType sectionType = currentSectionRow.ConvertedSectionType;
        if (sectionType == SectionType.Reinforcement)
        {
          SectionDataSet.SectionReinforcementRow[] sectionReinforcementRows = currentSectionRow.GetSectionReinforcementRows();
          if (sectionReinforcementRows != null && sectionReinforcementRows.Length > 0)
          {
            e.ErrorText = Properties.Resources.MsgErrorSectionCannotBecomeReinforcement;
            e.Valid = false;
            return;
          }
        }

        if (sectionType == SectionType.RawSection)
        {
          // Un profil ce are profil brut nu poate deveni profil brut.
          object obj = currentSectionRow[sectionDS.Section.IdRawSectionColumn.ColumnName];
          obj = obj == DBNull.Value ? currentSectionRow[sectionDS.Section.IdArcRawSectionColumn.ColumnName] : obj;
          if (obj != DBNull.Value)
          {
            e.ErrorText = Properties.Resources.MsgErrorSectionCannotBecomeRawSection;
            e.Valid = false;
            return;
          }

          // Un profil trebuie sa fie din lemn pentru a putea deveni profil brut.
          obj = currentSectionRow[sectionDS.Section.MaterialTypeColumn.ColumnName];
          if (obj == DBNull.Value || (string)obj != MaterialType.Wood.ToString())
          {
            e.ErrorText = Properties.Resources.MsgErrorSectionIsNotWood;
            e.Valid = false;
            return;
          }

          // Un profil compus nu poate deveni profil brut.
          SectionDataSet.CustomSectionItemRow[] customSectionItemRows = currentSectionRow.GetFkParentCustomSectionItemRows();
          if (customSectionItemRows != null && customSectionItemRows.Length > 0)
          {
            e.ErrorText = Properties.Resources.MsgErrorIsCustomSection;
            e.Valid = false;
            return;
          }
        }
      }

      if (!ValidateSectionType(currentSectionRow))
      {
        e.ErrorText = String.Empty;
        e.Valid = false;
        return;
      }

      bool? currentSectionRowIsValid = ValidateSectionIsActive(currentSectionRow, currentSectionRow.IsActive);
      if (currentSectionRowIsValid == null)
      {
        e.ErrorText = Properties.Resources.MsgErrorActiveInactiveDataReverted;
        e.Valid = false;
        return;
      }
      else if (currentSectionRowIsValid == false)
      {
        e.ErrorText = String.Empty;
        e.Valid = false;
        return;
      }


      //if (currentSectionRow.HasVersion(DataRowVersion.Proposed) &&
      //    currentSectionRow[sectionDS.Section.CodeColumn.ColumnName, DataRowVersion.Current].ToString() != currentSectionRow[sectionDS.Section.CodeColumn.ColumnName, DataRowVersion.Proposed].ToString() &&
      //           XtraMessageBox.Show(this, Properties.Resources.MsgRegenerateCodesQuestion,
      //               Properties.Resources.CaptionAttentionMsgBox,
      //               MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
      //{
      //  currentSectionRow.RegenerateSectionColorListItemCodes(colorDS, colorListDS, currentSectionRow[sectionDS.Section.CodeColumn.ColumnName, DataRowVersion.Proposed].ToString(), currentSectionRow[sectionDS.Section.CodeColumn.ColumnName, DataRowVersion.Current].ToString());
      //}

    }

    private void viewSection_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
    {
      int rowHandle = viewSection.GetRowHandle(e.ListSourceRowIndex);

      SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(rowHandle) as SectionDataSet.SectionRow;
      if (sectionRow == null)
      {
        return;
      }
      if (e.IsGetData)
      {
        if (e.Column == colUnitWeight)
        {
          e.Value = MeasurableProperties.DefaultMeasurableProperties.Mass.ConvertInvInternalToDisplay(sectionRow.UnitWeight);
        }
        if (e.Column == colUnitBasePrice)
        {
          e.Value = MeasurableProperties.DefaultMeasurableProperties.Length.ConvertInvInternalToDisplay(sectionRow.UnitBasePrice);
        }
        if (e.Column == colFlagsKnownSide)
        {
          e.Value = sectionRow.ConvertedKnownSide;
        }
        if (e.Column == colImage)
        {
          e.Value = imageDS.Image.GetImage(sectionRow.Guid);
        }
        if (e.Column == colDxf)
        {
          if (!sectionRow.IsDxfNull())
          {
            Bitmap bmp = new Bitmap(1, 1);
            ImageConverter converter = new ImageConverter();
            e.Value = (byte[])converter.ConvertTo(bmp, typeof(byte[]));
          }

        }
      }
      else
      {
        if (e.Column == colUnitWeight)
        {
          sectionRow.UnitWeight = MeasurableProperties.DefaultMeasurableProperties.Mass.ConvertInvDisplayToInternal(Convert.ToDecimal(e.Value));
        }
        if (e.Column == colUnitBasePrice)
        {
          sectionRow.UnitBasePrice = MeasurableProperties.DefaultMeasurableProperties.Length.ConvertInvDisplayToInternal(Convert.ToDecimal(e.Value));
        }
        if (e.Column == colFlagsKnownSide)
        {
          sectionRow.ConvertedKnownSide = (KnownSide)e.Value;
        }
        if (e.Column == colImage)
        {
          imageDS.Image.SetImage(sectionRow, e.Value as byte[]);
        }
      }
    }

    private void ViewSection_CellValueChanged(object sender, CellValueChangedEventArgs e)
    {
      if (e.Column == colH1 ||
          e.Column == colH2 ||
          e.Column == colH3)
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(e.RowHandle) as SectionDataSet.SectionRow;
        if (sectionRow == null)
        {
          return;
        }

        decimal h1 = sectionRow.H1;
        decimal h2 = sectionRow.H2;
        decimal h3 = sectionRow.H3;
        decimal h = sectionRow.H;

        if (e.Column == colH1)
        {
          h1 = Convert.ToDecimal(e.Value);
          sectionRow.H1 = h1;
        }
        if (e.Column == colH2)
        {
          h2 = Convert.ToDecimal(e.Value);
          sectionRow.H2 = h2;
        }
        if (e.Column == colH3)
        {
          h3 = Convert.ToDecimal(e.Value);
          sectionRow.H3 = h3;
        }
        if (h < h1 + h2 + h3)
        {
          sectionRow.H = h1 + h2 + h3;
        }

        return;
      }

      if (e.Column == colW1)
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(e.RowHandle) as SectionDataSet.SectionRow;
        if (sectionRow == null)
        {
          return;
        }

        decimal w1 = sectionRow.W1;
        decimal w = sectionRow.W;

        if (w < w1)
        {
          w1 = Convert.ToDecimal(e.Value);
          sectionRow.W1 = w1;
          sectionRow.W = w1;
        }

        return;
      }
    }

    private void repTxtLength_EditValueChanged(object sender, EventArgs e)
    {
      //if (viewSection.FocusedColumn == colH1 ||
      //    viewSection.FocusedColumn == colH2 ||
      //    viewSection.FocusedColumn == colH3)
      //{
      //  SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(viewSection.FocusedRowHandle) as SectionDataSet.SectionRow;
      //  if (sectionRow == null)
      //  {
      //    return;
      //  }

      //  decimal h1 = sectionRow.H1;
      //  decimal h2 = sectionRow.H2;
      //  decimal h3 = sectionRow.H3;
      //  decimal h = sectionRow.H;

      //  BaseEdit editor = sender as BaseEdit;
      //  if (h == h1 + h2 + h3)
      //  {
      //    if (viewSection.FocusedColumn == colH1)
      //    {
      //      h1 = Convert.ToDecimal(editor.EditValue);
      //      viewSection.SetFocusedRowCellValue(colH1, h1);
      //    }
      //    if (viewSection.FocusedColumn == colH2)
      //    {
      //      h2 = Convert.ToDecimal(editor.EditValue);
      //      viewSection.SetFocusedRowCellValue(colH2, h2);
      //    }
      //    if (viewSection.FocusedColumn == colH3)
      //    {
      //      h3 = Convert.ToDecimal(editor.EditValue);
      //      viewSection.SetFocusedRowCellValue(colH3, h3);
      //    }

      //    h = h1 + h2 + h3;
      //    viewSection.SetFocusedRowCellValue(colH, h1 + h2 + h3);
      //  }

      //  return;
      //}

      //if (viewSection.FocusedColumn == colW1)
      //{
      //  SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(viewSection.FocusedRowHandle) as SectionDataSet.SectionRow;
      //  if (sectionRow == null)
      //  {
      //    return;
      //  }

      //  decimal w1 = sectionRow.W1;
      //  decimal w = sectionRow.W;

      //  BaseEdit editor = sender as BaseEdit;
      //  if (w == w1)
      //  {
      //    w1 = Convert.ToDecimal(editor.EditValue);
      //    viewSection.SetFocusedRowCellValue(colW1, w1);

      //    w = w1;
      //    viewSection.SetFocusedRowCellValue(colW, w);
      //  }

      //  return;
      //}
    }

    private void gridUtils_CustomRowFilter(object sender, RowFilterEventArgs e)
    {
      // Daca nu este activa nici o filtrare se merge pe comportamentul standard
      int? idSeries = (int?)barEditSeries.EditValue;
      int? idComponentSection = (int?)barEditContainsSection.EditValue;
      string supplier = (barEditSupplier.EditValue == null) ? string.Empty : barEditSupplier.EditValue.ToString();
      int? idComponentReinforcement = (int?)barEditContainsReinforcement.EditValue;
      int? idComponentTolerance = (int?)barEditContainsTolerance.EditValue;

      // Se verifica daca exista filtrare custom
      if (idSeries == null && idComponentSection == null && supplier == string.Empty && idComponentReinforcement == null && idComponentTolerance == null)
      {
        return;
      }

      if (idSeries.HasValue && seriesDS.Series.FindById(idSeries.Value) == null)
      {
        barEditSeries.EditValue = null;
        idSeries = null;
      }

      if (idComponentSection.HasValue && sectionDS.Section.FindById(idComponentSection.Value) == null)
      {
        barEditContainsSection.EditValue = null;
        idComponentSection = null;
      }

      if (supplier != string.Empty && seriesDS.Series.SeriesBySupplier(supplier).Count == 0)
      {
        barEditSupplier.EditValue = null;
        supplier = string.Empty;
      }

      if (idComponentReinforcement.HasValue && sectionDS.Section.FindById(idComponentReinforcement.Value) == null)
      {
        barEditContainsReinforcement.EditValue = null;
        idComponentReinforcement = null;
      }

      if (idComponentTolerance.HasValue && sectionDS.Section.FindById(idComponentTolerance.Value) == null)
      {
        barEditContainsTolerance.EditValue = null;
        idComponentTolerance = null;
      }

      DataView viewSectionDataSource = viewSection.DataSource as System.Data.DataView;
      if (viewSectionDataSource == null)
      {
        Debug.Assert(false, "Sursa de date a view-ului nu este de tip DataView");
        return;
      }

      DataRowView sectionRowView = viewSectionDataSource[e.ListSourceRow];
      SectionDataSet.SectionRow sectionRow = sectionRowView.Row as SectionDataSet.SectionRow;
      if (sectionRow == null || sectionRow.RowState == DataRowState.Deleted)
      {
        Debug.Assert(false, "Sursa de date a view-ului nu poate contine randuri sterse");
        return;
      }

      // Se filtraza dupa serie
      if (idSeries.HasValue)
      {
        bool visible = false;
        foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionRow.GetSeriesSectionRows())
        {
          if (seriesSectionRow.IdSeries == idSeries.Value)
          {
            visible = true;
            break;
          }
        }

        if (!visible)
        {
          e.Visible = false;
          e.Handled = true;
          return;
        }
      }

      if (idComponentSection.HasValue)
      {
        bool visible = false;
        // Se filtreaza dupa profilul component
        foreach (SectionDataSet.CustomSectionItemRow componentSectionItemRow in sectionRow.GetFkParentCustomSectionItemRows())
        {
          if (componentSectionItemRow.IdSection == idComponentSection.Value)
          {
            visible = true;
            break;
          }
        }

        if (!visible)
        {
          e.Visible = false;
          e.Handled = true;
          return;
        }
      }


      if (idComponentReinforcement.HasValue)
      {
        bool visible = false;
        //se filtreaza dupa armatura
        foreach (SectionDataSet.SectionReinforcementRow componentReinforcementItemRow in sectionRow.GetSectionReinforcementRows())
        {
          if (componentReinforcementItemRow.IdSectionReinforcement == idComponentReinforcement.Value)
          {
            visible = true;
            break;
          }
        }
        if (!visible)
        {
          e.Visible = false;
          e.Handled = true;
          return;
        }

      }

      //se filtreaza dupa supplier
      if (supplier != string.Empty)
      {
        bool visible = false;
        List<int> seriesFromSupplier = seriesDS.Series.SeriesBySupplier(supplier);
        foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionRow.GetSeriesSectionRows())
        {
          if (seriesFromSupplier.Contains(seriesSectionRow.IdSeries))
          {
            visible = true;
            break;
          }
        }

        if (!visible)
        {
          e.Visible = false;
          e.Handled = true;
          return;
        }
      }

      //se filtreaza dupa tolerante
      if (idComponentTolerance.HasValue)
      {
        bool visible = false;
        // Se filtreaza dupa toleranta componenta
        foreach (SectionDataSet.SectionToleranceRow sectionToleranceRow in sectionRow.GetSectionToleranceRowsByFk_Section_SectionTolerance1())
        {
          if (sectionToleranceRow.IdSection2 == idComponentTolerance.Value)
          {
            visible = true;
            break;
          }
        }

        if (!visible)
        {
          e.Visible = false;
          e.Handled = true;
          return;
        }
      }

    }

    private void viewSection_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
    {
      if (e.Column != colUnitBasePrice || e.Value == null || e.Value == DBNull.Value)
        return;

      int rowHandle = viewSection.GetRowHandle(e.ListSourceRowIndex);

      try
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(rowHandle) as SectionDataSet.SectionRow;
        if (sectionRow == null)
          return;
        e.DisplayText = CurrencyExchange.CurrentCurrencyExchange.GetDisplayValue((decimal)e.Value, sectionRow.Currency);
      }
      catch (Exception exception)
      {
        FrameworkApplication.TreatException(exception);
      }
    }

    private void repLookUpSectionCurrency_EditValueChanged(object sender, EventArgs e)
    {
      // Codul a fost comentat deoarece metoda CloseEditor duce la erori cand se incearca filtrearea elementelor din lookup

      //try
      //{
      //  viewSection.CloseEditor();
      //  // Inchiderea editorului invalideaza tot randul ceea ce face ca invalidarea celulei din colUnitWeightPrice
      //  // sa fie redundanta. Pentru a ne asigura ca si pe viitor comportamentul gridului nu se modifica
      //  // am lasat si invalidarea celulei.
      //  viewSection.InvalidateRowCell(viewSection.FocusedRowHandle, colUnitBasePrice);
      //}
      //catch (Exception exception)
      //{
      //  FrameworkApplication.TreatException(exception);
      //}
    }

    private void repNullableEditor_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      BaseEdit editNullable = sender as BaseEdit;
      if (editNullable == null || e.Button.Kind != ButtonPredefines.Delete)
        return;

      editNullable.EditValue = null;
    }

    private void viewSection_HiddenEditor(object sender, EventArgs e)
    {
      barManager.SetPopupContextMenu(gridSection, popupMenu);
    }

    private void viewSection_ShownEditor(object sender, EventArgs e)
    {
      barManager.SetPopupContextMenu(gridSection, null);
    }

    private void repLookUpSupplier_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      if (e.Button.Kind == ButtonPredefines.Delete)
      {
        // se face cast la BaseEdit pentru a putea afisa NullText in control (altfel nu merge)
        BaseEdit editor = sender as BaseEdit;
        editor.EditValue = null;
        // se seteaza EditValue pe null pe BarEditItem pentru ca altfel nu se intra in evenimentul de EditValueChanged
        barEditSupplier.EditValue = null;
        editor.DoValidate();
      }
    }

    private void repGridLookUpSectionReinforcement_Popup(object sender, EventArgs e)
    {
      bool hasImage = false;
      GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
      if (gridLookUpEdit == null)
        return;

      GridColumn colGridLookUpImage = gridLookUpEdit.Properties.View.Columns[colGridLookUpSectionReinforcementImage.FieldName];

      for (int i = 0; i < gridLookUpEdit.Properties.View.DataRowCount; i++)
      {
        if (gridLookUpEdit.Properties.View.GetRowCellValue(i, colGridLookUpImage) != null)
        {
          hasImage = true;
          break;
        }
      }
      if (!hasImage)
      {
        colGridLookUpImage.Visible = false;
      }
      else
      {
        colGridLookUpImage.Visible = true;
      }
    }


    private void repGridLookUpSectionReinforcementView_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
    {
      if (e.IsSetData || e.Column.FieldName != colGridLookUpSectionReinforcementImage.FieldName)
        return;

      GridView senderGridView = sender as GridView;
      if (senderGridView == null)
        return;

      int rowHandle = senderGridView.GetRowHandle(e.ListSourceRowIndex);

      SectionDataSet.SectionRow row = (SectionDataSet.SectionRow)senderGridView.GetDataRow(rowHandle);
      if (row != null)
      {
        e.Value = imageDS.Image.GetImage(row.Guid);
      }
    }

    private void repGridLookUpSectionReinforcementView_CalcRowHeight(object sender, RowHeightEventArgs e)
    {
      GridView senderGridView = sender as GridView;
      if (senderGridView == null)
        return;

      GridColumn colGridLookUpImage = senderGridView.Columns[colGridLookUpSectionReinforcementImage.FieldName];

      if (senderGridView.GetRowCellValue(e.RowHandle, colGridLookUpImage) != null)
      {
        e.RowHeight = 3 * e.RowHeight;
      }
    }

    #region State Event Handlers

    private void persistStateComponent_UserStateLoad(object sender, Framework.State.UserStateOperationEventArgs e)
    {
      try
      {
        if (e.UserState.ContainsKey(StateSectionSeries))
          barEditSeries.EditValue = e.UserState[StateSectionSeries];

        if (e.UserState.ContainsKey(StateContainsComponent))
          barEditContainsSection.EditValue = e.UserState[StateContainsComponent];

        if (e.UserState.ContainsKey(StateSupplier))
          barEditSupplier.EditValue = e.UserState[StateSupplier];

        if (e.UserState.ContainsKey(StateContainsReinforcement))
          barEditContainsReinforcement.EditValue = e.UserState[StateContainsReinforcement];

        if (e.UserState.ContainsKey(StateContainsTolerance))
          barEditContainsTolerance.EditValue = e.UserState[StateContainsTolerance];

        if (e.UserState.ContainsKey(StateShowOnlyActive) && isAdmin)
          chkShowOnlyActive.Checked = (bool)e.UserState[StateShowOnlyActive];
        else
          chkShowOnlyActive.Checked = true;

        if (e.UserState.ContainsKey(StatePath))
          exportFolderPath = e.UserState[StatePath].ToString();
      }
      catch (Exception exception)
      {
        FrameworkApplication.TreatException(exception);
      }
    }

    private void persistStateComponent_UserStateSave(object sender, Framework.State.UserStateOperationEventArgs e)
    {
      if (barEditSeries.EditValue != null)
        e.UserState[StateSectionSeries] = barEditSeries.EditValue;

      if (barEditContainsSection.EditValue != null)
        e.UserState[StateContainsComponent] = barEditContainsSection.EditValue;

      if (barEditSupplier.EditValue != null)
        e.UserState[StateSupplier] = barEditSupplier.EditValue;

      if (barEditContainsReinforcement.EditValue != null)
        e.UserState[StateContainsReinforcement] = barEditContainsReinforcement.EditValue;

      if (barEditContainsTolerance.EditValue != null)
        e.UserState[StateContainsTolerance] = barEditContainsTolerance.EditValue;

      if (isAdmin)
        e.UserState[StateShowOnlyActive] = chkShowOnlyActive.Checked;

      e.UserState[StatePath] = String.Format("{0}\\", exportFolderPath);
    }

    #endregion State Event Handlers

    #endregion Event Handlers

    #region Toolbar

    private void bbSave_ItemClick(object sender, ItemClickEventArgs e)
    {
      ApplyChanges(true);
    }

    private void bbRefresh_ItemClick(object sender, ItemClickEventArgs e)
    {
      RefreshData(true);
    }

    private void bbCut_ItemClick(object sender, ItemClickEventArgs e)
    {
      gridUtils.Cut();
    }

    private void bbCopy_ItemClick(object sender, ItemClickEventArgs e)
    {
      gridUtils.Copy();
    }

    private void bbPaste_ItemClick(object sender, ItemClickEventArgs e)
    {
      gridUtils.Paste();
    }

    private void bbNewItem_ItemClick(object sender, ItemClickEventArgs e)
    {
      ShowSectionEditDialog(null);
    }

    private void bbEditItem_ItemClick(object sender, ItemClickEventArgs e)
    {
      SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(viewSection.FocusedRowHandle) as SectionDataSet.SectionRow;
      if (sectionRow != null)
      {
        ShowSectionEditDialog(sectionRow);
      }
    }

    private void bbDelete_ItemClick(object sender, ItemClickEventArgs e)
    {
      gridUtils.Delete();
    }

    private void bbPrintPreview_ItemClick(object sender, ItemClickEventArgs e)
    {
      gridUtils.PrintPreview();
    }

    private void bbPrint_ItemClick(object sender, ItemClickEventArgs e)
    {
      gridUtils.Print();
    }

    private void bbExport_ItemClick(object sender, ItemClickEventArgs e)
    {
      try
      {
        gridUtils.Export();
      }
      catch (System.IO.IOException)
      {
        XtraMessageBox.Show(this,
                            Properties.Resources.MsgExportIOException,
                            Properties.Resources.CaptionAttentionMsgBox,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
      }
    }

    /// <summary>
    /// Export profile prices to excel.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void bbExportPrices_ItemClick(object sender, ItemClickEventArgs e)
    {
      try
      {
        List<SectionDataSet.SectionRow> sectionRows = GetViewSectionRows();
        SectionPricesImportExport sectionPricesImportExport = new SectionPricesImportExport(sectionRows, materialSpeciesDS, colorListDS, colorDS, null, null, exportFolderPath);

        sectionPricesImportExport.ExportSectionPrices();
      }
      catch (FileNotFoundException exception)
      {
        XtraMessageBox.Show(this,
          exception.Message,
          Properties.Resources.CaptionErrorMsgBox,
          MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
      catch (InvalidOperationException exception)
      {
        XtraMessageBox.Show(this,
          exception.Message,
          Properties.Resources.CaptionErrorMsgBox,
          MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
      catch (Exception exception)
      {
        FrameworkApplication.TraceSource.TraceData(TraceEventType.Error, 0, new object[] { exception.Message });

        XtraMessageBox.Show(this,
          Properties.Resources.MsgExportIOException,
          Properties.Resources.CaptionErrorMsgBox,
          MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
    }

    /// <summary>
    /// Import profile prices from excel.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void bbImportPrices_ItemClick(object sender, ItemClickEventArgs e)
    {
      String filePath;
      if ((filePath = ImportUtils.BrowseExcelFile(exportFolderPath)) != null)
      {
        try
        {
          SectionPricesImportExport sectionPricesImportExport = new SectionPricesImportExport(null, materialSpeciesDS, colorListDS, colorDS, sectionDS, currencyDS, filePath);
          sectionPricesImportExport.ImportPriceRows(filePath);

          XtraMessageBox.Show(this,
                  Properties.Resources.MsgImportSuccessful,
                  Properties.Resources.CaptionInformationMsgBox,
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Information);
        }
        catch (FileNotFoundException exception)
        {
          XtraMessageBox.Show(this,
            exception.Message,
            Properties.Resources.CaptionErrorMsgBox,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        }
        catch (InvalidOperationException exception)
        {
          XtraMessageBox.Show(this,
            exception.Message,
            Properties.Resources.CaptionErrorMsgBox,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        }
        catch (Exception exception)
        {
          FrameworkApplication.TraceSource.TraceData(TraceEventType.Error, 0, new object[] { exception.Message });

          XtraMessageBox.Show(this,
            Properties.Resources.MsgExportIOException,
            Properties.Resources.CaptionErrorMsgBox,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        }
      }
    }

    private void bbImages_ItemClick(object sender, ItemClickEventArgs e)
    {
      folderBrowserDialogImages.Description = "Selectati folderul cu imagini...";
      folderBrowserDialogImages.ShowNewFolderButton = false;
      if (folderBrowserDialogImages.ShowDialog(this) == DialogResult.OK)
      {
        ImportImages(folderBrowserDialogImages.SelectedPath);
      }
    }

    private void bbPasteColorLists_ItemClick(object sender, ItemClickEventArgs e)
    {
      IDataObject dataObject = ClipboardEx.GetDataObject();
      if (dataObject == null)
        return;

      object clipboardObject = dataObject.GetData(sectionDS.GetType());
      DataSet clipboardDataSet = clipboardObject as DataSet;
      if (clipboardDataSet == null || !clipboardDataSet.Tables.Contains(sectionDS.Section.TableName))
        return;
      DataTable clipboardSectionTable = clipboardDataSet.Tables[sectionDS.Section.TableName];

      if (clipboardSectionTable.Rows.Count != 1)
        return;

      // Indecsii randurilor selectate pentru a se face paste.
      int[] sectionRowHandles = viewSection.GetSelectedRows();

      // Profilul din care se copiaza listele.
      SectionDataSet.SectionRow clipboardSectionRow = clipboardSectionTable.Rows[0] as SectionDataSet.SectionRow;

      SectionDataSet.SectionColorListRow[] clipboardSectionColorListRows = clipboardSectionRow.GetSectionColorListRows();

      if (clipboardSectionColorListRows.Length == 0)
      {
        XtraMessageBox.Show(this,
                       Properties.Resources.MsgInvalidPasteSectionColorLists,
                       Properties.Resources.CaptionAttentionMsgBox,
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Warning);
        return;
      }

      if (XtraMessageBox.Show(this,
                      Properties.Resources.MsgPasteSectionColorLists,
                      Properties.Resources.CaptionAttentionMsgBox,
                      MessageBoxButtons.YesNo,
                      MessageBoxIcon.Warning) == DialogResult.No)
        return;

      foreach (int sectionRowHandle in sectionRowHandles)
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(sectionRowHandle) as SectionDataSet.SectionRow;
        foreach (SectionDataSet.SectionColorListRow sectionColorListRow in sectionRow.GetSectionColorListRows())
          sectionColorListRow.Delete();

        // Pentru fiecare lista, se creaza o copie a listei si se adauga in DataSet.
        foreach (SectionDataSet.SectionColorListRow clipboardSectionColorListRow in clipboardSectionColorListRows)
        {
          SectionDataSet.SectionColorListRow newSectionColorListRow = sectionDS.SectionColorList.NewSectionColorListRow();
          newSectionColorListRow.IdColorList = clipboardSectionColorListRow.IdColorList;
          newSectionColorListRow.IdSection = sectionRow.Id;
          newSectionColorListRow.PriceCalculationType = clipboardSectionColorListRow.PriceCalculationType;
          newSectionColorListRow.Price = 0;
          newSectionColorListRow.ConvertedEditingMode = clipboardSectionColorListRow.ConvertedEditingMode;
          if (!clipboardSectionColorListRow.IsIdCostGroupNull())
            newSectionColorListRow.IdCostGroup = clipboardSectionColorListRow.IdCostGroup;
          if (!clipboardSectionColorListRow.IsBarLengthNull())
            newSectionColorListRow.BarLength = clipboardSectionColorListRow.BarLength;
          if (!clipboardSectionColorListRow.IsIdReinforcementNull() && !sectionRow.IsIdReinforcementNull())
            newSectionColorListRow.IdReinforcement = clipboardSectionColorListRow.IdReinforcement;
          sectionDS.SectionColorList.AddSectionColorListRow(newSectionColorListRow);

          // Pentru fiecare element din lista, se creaza o copie a elementului si se adauga in DataSet.
          foreach (SectionDataSet.SectionColorListItemRow clipboardSectionColorListItemRow in clipboardSectionColorListRow.GetSectionColorListItemRows())
          {
            SectionDataSet.SectionColorListItemRow newSectionColorListItemRow = sectionDS.SectionColorListItem.NewSectionColorListItemRow();
            newSectionColorListItemRow.Price = 0;
            newSectionColorListItemRow.IdColorCombination = clipboardSectionColorListItemRow.IdColorCombination;
            newSectionColorListItemRow.IdSectionColorList = newSectionColorListRow.Id;
            newSectionColorListItemRow.Code = sectionRow.CreateCodeWithColorCombination(colorDS.ColorCombination.FindById(clipboardSectionColorListItemRow.IdColorCombination), colorListDS);
            newSectionColorListItemRow.GenerateUniqueCode();
            if (!clipboardSectionColorListItemRow.IsBarLengthNull())
              newSectionColorListItemRow.BarLength = clipboardSectionColorListItemRow.BarLength;
            if (!clipboardSectionColorListItemRow.IsIdReinforcementNull() && !sectionRow.IsIdReinforcementNull())
              newSectionColorListItemRow.IdReinforcement = clipboardSectionColorListItemRow.IdReinforcement;
            if (!clipboardSectionColorListItemRow.IsIdCostGroupNull())
              newSectionColorListItemRow.IdCostGroup = clipboardSectionColorListItemRow.IdCostGroup;

            sectionDS.SectionColorListItem.AddSectionColorListItemRow(newSectionColorListItemRow);
          }
        }
      }
    }

    private void bbPasteMaterialSpecies_ItemClick(object sender, ItemClickEventArgs e)
    {
      IDataObject dataObject = ClipboardEx.GetDataObject();
      if (dataObject == null)
        return;

      object clipboardObject = dataObject.GetData(sectionDS.GetType());
      DataSet clipboardDataSet = clipboardObject as DataSet;
      if (clipboardDataSet == null || !clipboardDataSet.Tables.Contains(sectionDS.Section.TableName))
        return;
      DataTable clipboardSectionTable = clipboardDataSet.Tables[sectionDS.Section.TableName];

      if (clipboardSectionTable.Rows.Count != 1)
        return;

      // Indecsii randurilor selectate pentru a se face paste.
      int[] sectionRowHandles = viewSection.GetSelectedRows();

      // Profilul din care se copiaza listele.
      SectionDataSet.SectionRow clipboardSectionRow = clipboardSectionTable.Rows[0] as SectionDataSet.SectionRow;

      SectionDataSet.SectionMaterialSpeciesRow[] clipboardSectionMaterialSpeciesRows = clipboardSectionRow.GetSectionMaterialSpeciesRows();

      if (clipboardSectionMaterialSpeciesRows.Length == 0)
      {
        XtraMessageBox.Show(this,
                       Properties.Resources.MsgInvalidPasteSectionMaterialSpecies,
                       Properties.Resources.CaptionAttentionMsgBox,
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Warning);
        return;
      }

      if (XtraMessageBox.Show(this,
                      Properties.Resources.MsgPasteSectionMaterialSpecies,
                      Properties.Resources.CaptionAttentionMsgBox,
                      MessageBoxButtons.YesNo,
                      MessageBoxIcon.Warning) == DialogResult.No)
        return;

      foreach (int sectionRowHandle in sectionRowHandles)
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(sectionRowHandle) as SectionDataSet.SectionRow;
        if (sectionRow.MaterialType != clipboardSectionRow.MaterialType)
          continue;

        foreach (SectionDataSet.SectionMaterialSpeciesRow sectionMaterialSpeciesRow in sectionRow.GetSectionMaterialSpeciesRows())
          sectionMaterialSpeciesRow.Delete();

        // Pentru fiecare tip de material, se creaza o copie tipului de material si se adauga in DataSet.
        foreach (SectionDataSet.SectionMaterialSpeciesRow clipboardSectionMaterialSpeciesRow in clipboardSectionMaterialSpeciesRows)
        {
          SectionDataSet.SectionMaterialSpeciesRow newSectionMaterialSpeciesRow = sectionDS.SectionMaterialSpecies.NewSectionMaterialSpeciesRow();
          newSectionMaterialSpeciesRow.IdMaterialSpecies = clipboardSectionMaterialSpeciesRow.IdMaterialSpecies;
          newSectionMaterialSpeciesRow.IdSection = sectionRow.Id;
          if (!clipboardSectionMaterialSpeciesRow.IsIxNull())
            newSectionMaterialSpeciesRow.Ix = clipboardSectionMaterialSpeciesRow.Ix;
          if (!clipboardSectionMaterialSpeciesRow.IsIyNull())
            newSectionMaterialSpeciesRow.Iy = clipboardSectionMaterialSpeciesRow.Iy;
          if (!clipboardSectionMaterialSpeciesRow.IsPriceNull())
            newSectionMaterialSpeciesRow.Price = clipboardSectionMaterialSpeciesRow.Price;
          if (!clipboardSectionMaterialSpeciesRow.IsUnitWeightNull())
            newSectionMaterialSpeciesRow.UnitWeight = clipboardSectionMaterialSpeciesRow.UnitWeight;
          if (!clipboardSectionMaterialSpeciesRow.IsHeatTransferCoefficientNull())
            newSectionMaterialSpeciesRow.HeatTransferCoefficient = clipboardSectionMaterialSpeciesRow.HeatTransferCoefficient;
          newSectionMaterialSpeciesRow.ConvertedMaterialSpeciesUsage = clipboardSectionMaterialSpeciesRow.ConvertedMaterialSpeciesUsage;
          sectionDS.SectionMaterialSpecies.AddSectionMaterialSpeciesRow(newSectionMaterialSpeciesRow);
        }
      }
    }

    private void bbRegenerateColorCodes_ItemClick(object sender, ItemClickEventArgs e)
    {
      try
      {
        sectionDS.EnforceConstraints = false;

        List<SectionDataSet.SectionColorListItemRow> sectionColorListItemRows = new List<SectionDataSet.SectionColorListItemRow>();
        foreach (int selectedRowHandle in viewSection.GetSelectedRows())
        {
          SectionDataSet.SectionRow selectedSectionRow = viewSection.GetDataRow(selectedRowHandle) as SectionDataSet.SectionRow;

          foreach (SectionDataSet.SectionColorListRow sectionColorListRow in selectedSectionRow.GetSectionColorListRows())
          {
            if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListColorCombinations)
              sectionColorListItemRows.AddRange(sectionColorListRow.GetSectionColorListItemRows());
          }
          foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListItemRows)
          {
            string code = sectionColorListItemRow.SectionColorListRow.SectionRow.CreateCodeWithColorCombination(colorDS.ColorCombination.SingleOrDefault(row => row.Id == sectionColorListItemRow.IdColorCombination), colorListDS);
            sectionColorListItemRow.Code = code;
          }
          foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListItemRows)
          {
            sectionColorListItemRow.GenerateUniqueCode();
          }
          sectionColorListItemRows.Clear();
        }
      }
      finally
      {
        sectionDS.EnforceConstraints = true;
      }
    }

    private void bbActivateSelection_ItemClick(object sender, ItemClickEventArgs e)
    {
      if (!isAdmin)
        return;

      int[] sectionRowHandles = viewSection.GetSelectedRows();

      ValidateSections(sectionRowHandles, true);

    }

    private void bbDeactivateSelection_ItemClick(object sender, ItemClickEventArgs e)
    {
      if (!isAdmin)
        return;

      int[] sectionRowHandles = viewSection.GetSelectedRows();

      ValidateSections(sectionRowHandles, false);
    }

    #endregion Toolbar

    private void viewSection_CustomRowCellEditForEditing(object sender, CustomRowCellEditEventArgs e)
    {
      if (e.Column != colIdReinforcement)
        return;

      SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(e.RowHandle) as SectionDataSet.SectionRow;
      if (sectionRow == null)
        return;

      DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit repGridLookUpSectionReinforcementClone = repGridLookUpSectionReinforcement.Clone() as DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit;
      //List<int> sectionReinforcementList = sectionDS.SectionReinforcement.Where(row => row.IdSection == sectionRow.Id).Select(row => row.IdSectionReinforcement).ToList();
      sectionReinforcementBS.DataSource = sectionRow.GetSectionReinforcements();//sectionDS.Section.Where(row => sectionReinforcementList.Contains(row.Id));
      repGridLookUpSectionReinforcementClone.DataSource = sectionReinforcementBS;
      e.RepositoryItem = repGridLookUpSectionReinforcementClone;
    }

    private void bbPasteReinforcement_ItemClick(object sender, ItemClickEventArgs e)
    {
      IDataObject dataObject = ClipboardEx.GetDataObject();
      if (dataObject == null)
        return;

      object clipboardObject = dataObject.GetData(sectionDS.GetType());
      DataSet clipboardDataSet = clipboardObject as DataSet;
      if (clipboardDataSet == null || !clipboardDataSet.Tables.Contains(sectionDS.Section.TableName))
        return;

      DataTable clipboardSectionTable = clipboardDataSet.Tables[sectionDS.Section.TableName];

      if (clipboardSectionTable.Rows.Count != 1)
        return;

      // Indecsii randurilor selectate pentru a se face paste.
      int[] sectionRowHandles = viewSection.GetSelectedRows();

      //Profilul din care se copiaza listele
      SectionDataSet.SectionRow clipboardSectionRow = clipboardSectionTable.Rows[0] as SectionDataSet.SectionRow;
      SectionDataSet.SectionReinforcementRow[] clipboardSectionReinforcementRows = clipboardSectionRow.GetSectionReinforcementRows();

      if (clipboardSectionReinforcementRows.Length == 0)
      {
        XtraMessageBox.Show(this,
                            Properties.Resources.MsgInvalidPasteSectionReinforcement,
                            Properties.Resources.CaptionAttentionMsgBox,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
        return;
      }

      if (XtraMessageBox.Show(this,
                             Properties.Resources.MsgPasteSectionReinforcement,
                             Properties.Resources.CaptionAttentionMsgBox,
                             MessageBoxButtons.YesNo,
                             MessageBoxIcon.Question) == DialogResult.No)
      {
        return;
      }


      foreach (int sectionRowHandle in sectionRowHandles)
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(sectionRowHandle) as SectionDataSet.SectionRow;
        if (sectionRow.Id == clipboardSectionRow.Id)
          continue;

        if (clipboardSectionRow.IsIdReinforcementNull())
        {
          sectionRow.SetIdReinforcementNull();
        }
        else
        {
          sectionRow.IdReinforcement = clipboardSectionRow.IdReinforcement;
        }
        foreach (SectionDataSet.SectionReinforcementRow sectionReinforcemntRow in sectionRow.GetSectionReinforcementRows())
          sectionReinforcemntRow.Delete();

        //Pentru fiecare Armatura, se creeaza o copie si se adauga in DataSet
        foreach (SectionDataSet.SectionReinforcementRow clipboardSectionReinforcemntRow in clipboardSectionReinforcementRows)
        {
          SectionDataSet.SectionReinforcementRow newSectionReinforcementRow = sectionDS.SectionReinforcement.NewSectionReinforcementRow();
          newSectionReinforcementRow.SectionRowByFkSectionSectionReinforcementParent = sectionRow;
          newSectionReinforcementRow.Offset = clipboardSectionReinforcemntRow.Offset;
          newSectionReinforcementRow.OffsetVCut = clipboardSectionReinforcemntRow.OffsetVCut;
          newSectionReinforcementRow.CuttingType = clipboardSectionReinforcemntRow.CuttingType;

          newSectionReinforcementRow.IdSectionReinforcement = clipboardSectionReinforcemntRow.IdSectionReinforcement;
          sectionDS.SectionReinforcement.AddSectionReinforcementRow(newSectionReinforcementRow);
        }
        AddRequiredSections(sectionRow.Id);
      }
    }

    /// <summary>
    /// Adauga, daca este cazul, armatura / componentele din cadrul profilului editat
    /// in seriile din care acesta face parte
    /// </summary>
    /// <param name="idSection">Id-ul profilului pentru care se adauga dependentele</param>
    private void AddRequiredSections(int idSection)
    {
      string filterSeries = string.Format("{0} = {1}",
                  sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                  idSection);
      SectionDataSet.SectionRow aSectionRow = sectionDS.Section.FindById(idSection);

      #region Adaugare armaturi necesare
      // I) Adaugare armaturi necesare
      string filterReinforcementItems = string.Format("{0} = {1}",
                                    idSection,
                                    sectionDS.SectionReinforcement.IdSectionColumn.ColumnName);
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        foreach (SectionDataSet.SectionReinforcementRow reinforcementRow in sectionDS.SectionReinforcement.Select(filterReinforcementItems))
        {
          string filterSeriesReinforcement = string.Format("{0} = {1} AND {2} = {3}",
                        sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                        seriesSectionRow.IdSeries,
                        sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                        reinforcementRow.IdSectionReinforcement);
          if (sectionDS.SeriesSection.Select(filterSeriesReinforcement).Length == 0)
          {
            SectionDataSet.SeriesSectionRow newSeriesSectionRow = sectionDS.SeriesSection.NewSeriesSectionRow();
            newSeriesSectionRow.IdSeries = seriesSectionRow.IdSeries;
            newSeriesSectionRow.IdSection = reinforcementRow.IdSectionReinforcement;
            sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
            AddRequiredSections(reinforcementRow.IdSectionReinforcement);
          }
        }
      }
      #endregion Adaugare armaturi necesare

      #region Adaugare componente necesare
      // III Adaugare componente necesare
      string filterCustomItems = string.Format("{0} = {1}",
                    sectionDS.CustomSectionItem.IdParentSectionColumn.ColumnName,
                    idSection);
      // pentru fiecare serie din care face parte profilul curent
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        // pentru fiecare componenta care face parte din profilul curent
        foreach (SectionDataSet.CustomSectionItemRow customItemRow in sectionDS.CustomSectionItem.Select(filterCustomItems))
        {
          string filterSeriesCustomItems = string.Format("{0} = {1} AND {2} = {3}",
                        sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                        seriesSectionRow.IdSeries,
                        sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                        customItemRow.IdSection);
          // daca componenta nu exista in seria curenta o adaug
          if (sectionDS.SeriesSection.Select(filterSeriesCustomItems).Length == 0)
          {
            SectionDataSet.SeriesSectionRow newSeriesSectionRow = sectionDS.SeriesSection.NewSeriesSectionRow();
            newSeriesSectionRow.IdSeries = seriesSectionRow.IdSeries;
            newSeriesSectionRow.IdSection = customItemRow.IdSection;
            sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
            AddRequiredSections(customItemRow.IdSection);
          }
        }
      }
      #endregion Adaugare componente necesare

      #region Adaugare Serii Tolerante

      // VI Adauga profilele folosite in combinatiile de tolerante in seriile profilului curent
      string filterToleranceItems = string.Format("{0} = {1}",
                                    idSection,
                                    sectionDS.SectionTolerance.IdSection1Column.ColumnName);
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        foreach (SectionDataSet.SectionToleranceRow toleranceRow in sectionDS.SectionTolerance.Select(filterToleranceItems))
        {
          string filterSeriesTolerance = string.Format("{0} = {1} AND {2} = {3}",
                        sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                        seriesSectionRow.IdSeries,
                        sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                        toleranceRow.IdSection2);
          if (sectionDS.SeriesSection.Select(filterSeriesTolerance).Length == 0)
          {
            SectionDataSet.SeriesSectionRow newSeriesSectionRow = sectionDS.SeriesSection.NewSeriesSectionRow();
            newSeriesSectionRow.IdSeries = seriesSectionRow.IdSeries;
            newSeriesSectionRow.IdSection = toleranceRow.IdSection2;
            sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
            AddRequiredSections(toleranceRow.IdSection2);
          }
        }
      }

      #endregion
    }

    private void BbTolerance_ItemClick(object sender, ItemClickEventArgs e)
    {
      IDataObject dataObject = ClipboardEx.GetDataObject();
      if (dataObject == null)
        return;

      object clipboardObject = dataObject.GetData(sectionDS.GetType());
      DataSet clipboardDataSet = clipboardObject as DataSet;
      if (clipboardDataSet == null || !clipboardDataSet.Tables.Contains(sectionDS.Section.TableName))
        return;

      DataTable clipboardSectionTable = clipboardDataSet.Tables[sectionDS.Section.TableName];

      if (clipboardSectionTable.Rows.Count != 1)
        return;

      // Indecsii randurilor selectate pentru a se face paste.
      int[] sectionRowHandles = viewSection.GetSelectedRows();

      //Profilul din care se copiaza listele
      SectionDataSet.SectionRow clipboardSectionRow = clipboardSectionTable.Rows[0] as SectionDataSet.SectionRow;
      SectionDataSet.SectionToleranceRow[] clipboardSectionToleranceRows = clipboardSectionRow.GetSectionToleranceRows1();

      if (clipboardSectionToleranceRows.Length == 0)
      {
        XtraMessageBox.Show(this,
                            Properties.Resources.MsgInvalidPasteSectionTolerance,
                            Properties.Resources.CaptionAttentionMsgBox,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
        return;
      }

      if (XtraMessageBox.Show(this,
                             Properties.Resources.MsgPasteSectionTolerance,
                             Properties.Resources.CaptionAttentionMsgBox,
                             MessageBoxButtons.YesNo,
                             MessageBoxIcon.Question) == DialogResult.No)
      {
        return;
      }


      foreach (int sectionRowHandle in sectionRowHandles)
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(sectionRowHandle) as SectionDataSet.SectionRow;
        if (sectionRow == null || sectionRow.Id == clipboardSectionRow.Id)
          continue;

        sectionRow.CuttingTolerance = clipboardSectionRow.CuttingTolerance;
        sectionRow.BindingTolerance = clipboardSectionRow.BindingTolerance;
        sectionRow.ProcessingTolerance = clipboardSectionRow.ProcessingTolerance;
        if (clipboardSectionRow.IsSashToleranceNull())
        {
          sectionRow.SetSashToleranceNull();
        }
        else
        {
          sectionRow.SashTolerance = clipboardSectionRow.SashTolerance;
        }
        if (clipboardSectionRow.IsNoThresholdToleranceNull())
        {
          sectionRow.SetNoThresholdToleranceNull();
        }
        else
        {
          sectionRow.NoThresholdTolerance = clipboardSectionRow.NoThresholdTolerance;
        }
        if (clipboardSectionRow.IsFillingToleranceNull())
        {
          sectionRow.SetFillingToleranceNull();
        }
        else
        {
          sectionRow.FillingTolerance = clipboardSectionRow.FillingTolerance;
        }
        if (clipboardSectionRow.IsFoldingSash2SashToleranceNull())
        {
          sectionRow.SetFoldingSash2SashToleranceNull();
        }
        else
        {
          sectionRow.FoldingSash2SashTolerance = clipboardSectionRow.FoldingSash2SashTolerance;
        }
        if (clipboardSectionRow.IsSlideSwingToleranceNull())
        {
          sectionRow.SetSlideSwingToleranceNull();
        }
        else
        {
          sectionRow.SlideSwingTolerance = clipboardSectionRow.SlideSwingTolerance;
        }
        sectionRow.DowelTolerance = clipboardSectionRow.DowelTolerance;
        sectionRow.TenonTolerance = clipboardSectionRow.TenonTolerance;
        sectionRow.MullionDowelTolerance = clipboardSectionRow.MullionDowelTolerance;
        sectionRow.MullionTenonTolerance = clipboardSectionRow.MullionTenonTolerance;
        sectionRow.AdapterTolerance = clipboardSectionRow.AdapterTolerance;
        sectionRow.ExtraSashDimension = clipboardSectionRow.ExtraSashDimension;


        foreach (SectionDataSet.SectionToleranceRow sectionToleranceRow in sectionRow.GetSectionToleranceRows1())
          sectionToleranceRow.Delete();

        //Pentru fiecare Toleranta, se creeaza o copie si se adauga in DataSet
        foreach (SectionDataSet.SectionToleranceRow clipboardSectionToleranceRow in clipboardSectionToleranceRows)
        {
          SectionDataSet.SectionToleranceRow newSectionToleranceRow = sectionDS.SectionTolerance.NewSectionToleranceRow();
          newSectionToleranceRow.SectionRowByFk_Section_SectionTolerance1 = sectionRow;
          newSectionToleranceRow.IdSection2 = clipboardSectionToleranceRow.IdSection2;
          newSectionToleranceRow.Tolerance = clipboardSectionToleranceRow.Tolerance;
          sectionDS.SectionTolerance.AddSectionToleranceRow(newSectionToleranceRow);
        }
        AddRequiredSections(sectionRow.Id);

      }
    }

    private void bbPasteImage_ItemClick(object sender, ItemClickEventArgs e)
    {
      IDataObject dataObject = ClipboardEx.GetDataObject();
      if (dataObject == null)
        return;

      object clipboardObject = dataObject.GetData(sectionDS.GetType());
      DataSet clipboardDataSet = clipboardObject as DataSet;
      if (clipboardDataSet == null || !clipboardDataSet.Tables.Contains(sectionDS.Section.TableName))
        return;
      DataTable clipboardSectionTable = clipboardDataSet.Tables[sectionDS.Section.TableName];

      if (clipboardSectionTable.Rows.Count != 1)
        return;

      // Indecsii randurilor selectate pentru a se face paste.
      int[] sectionRowHandles = viewSection.GetSelectedRows();

      // Profilul din care se copiaza listele.
      SectionDataSet.SectionRow clipboardSectionRow = clipboardSectionTable.Rows[0] as SectionDataSet.SectionRow;
      byte[] clipboardSectionImage = imageDS.Image.GetImage(clipboardSectionRow.Guid);

      foreach (int sectionRowHandle in sectionRowHandles)
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(sectionRowHandle) as SectionDataSet.SectionRow;
        if (sectionRow.Id == clipboardSectionRow.Id)
          continue;

        imageDS.Image.SetImage(sectionRow, clipboardSectionImage);
      }

    }

    private void bbPasteSeries_ItemClick(object sender, ItemClickEventArgs e)
    {
      IDataObject dataObject = ClipboardEx.GetDataObject();
      if (dataObject == null)
        return;

      object clipboardObject = dataObject.GetData(sectionDS.GetType());
      DataSet clipboardDataSet = clipboardObject as DataSet;
      if (clipboardDataSet == null || !clipboardDataSet.Tables.Contains(sectionDS.Section.TableName))
        return;

      DataTable clipboardSectionTable = clipboardDataSet.Tables[sectionDS.Section.TableName];

      if (clipboardSectionTable.Rows.Count != 1)
        return;

      // Indecsii randurilor selectate pentru a se face paste.
      int[] sectionRowHandles = viewSection.GetSelectedRows();

      //Profilul din care se copiaza listele
      SectionDataSet.SectionRow clipboardSectionRow = clipboardSectionTable.Rows[0] as SectionDataSet.SectionRow;
      SectionDataSet.SeriesSectionRow[] clipboardSeriesSectionRows = clipboardSectionRow.GetSeriesSectionRows();

      if (clipboardSeriesSectionRows.Length == 0)
      {
        XtraMessageBox.Show(this,
                            Properties.Resources.MsgInvalidPasteSeriesSection,
                            Properties.Resources.CaptionAttentionMsgBox,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
        return;
      }

      if (XtraMessageBox.Show(this,
                             Properties.Resources.MsgPasteSeriesSection,
                             Properties.Resources.CaptionAttentionMsgBox,
                             MessageBoxButtons.YesNo,
                             MessageBoxIcon.Question) == DialogResult.No)
      {
        return;
      }


      foreach (int sectionRowHandle in sectionRowHandles)
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(sectionRowHandle) as SectionDataSet.SectionRow;
        if (sectionRow.Id == clipboardSectionRow.Id)
          continue;
        foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionRow.GetSeriesSectionRows())
          seriesSectionRow.Delete();

        //Pentru fiecare Serie, se creeaza o copie si se adauga in DataSet
        foreach (SectionDataSet.SeriesSectionRow clipboardSeriesSectionRow in clipboardSeriesSectionRows)
        {
          SectionDataSet.SeriesSectionRow newSeriesSectionRow = sectionDS.SeriesSection.NewSeriesSectionRow();
          newSeriesSectionRow.IdSeries = clipboardSeriesSectionRow.IdSeries;
          newSeriesSectionRow.IdSection = sectionRow.Id;

          if (clipboardSeriesSectionRow.IsPriorityNull())
          {
            newSeriesSectionRow.SetPriorityNull();
          }
          else
          {
            newSeriesSectionRow.Priority = clipboardSeriesSectionRow.Priority;
          }
          if (clipboardSeriesSectionRow.IsIsSeriesActiveNull())
          {
            newSeriesSectionRow.SetIsSeriesActiveNull();
          }
          else
          {
            newSeriesSectionRow.IsSeriesActive = clipboardSeriesSectionRow.IsSeriesActive;
          }
          sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);

        }

      }
    }

    private void bbModifyPriceCatalog_ItemClick(object sender, ItemClickEventArgs e)
    {
      //se salveaza datele iniante de orice modificare
      DialogResult result = XtraMessageBox.Show(this,
                                              Properties.Resources.MsgSaveInformation,
                                              Properties.Resources.CaptionAttentionMsgBox,
                                              MessageBoxButtons.OKCancel,
                                              MessageBoxIcon.Question);
      if (result == DialogResult.Cancel)
      {
        return;
      }

      if (!ApplyChanges(false))
      {
        return;
      }


      // Indecsii randurilor selectate pentru a se modifica preturile.
      int[] sectionRowHandles = viewSection.GetSelectedRows();
      int? idPriceCatalog = (int?)barEditPriceCatalog.EditValue;

      List<SectionDataSet.SectionRow> destSectionRows = new List<SectionDataSet.SectionRow>();
      List<SectionDataSet.SectionColorListRow> destSectionColorListRows = new List<SectionDataSet.SectionColorListRow>();
      List<SectionDataSet.SectionColorListItemRow> destSectionColorListItemRows = new List<SectionDataSet.SectionColorListItemRow>();
      List<SectionDataSet.SectionMaterialSpeciesRow> destSectionMaterialsSpeciesRows = new List<SectionDataSet.SectionMaterialSpeciesRow>();
      foreach (int sectionRowHandle in sectionRowHandles)
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(sectionRowHandle) as SectionDataSet.SectionRow;
        destSectionRows.Add(sectionRow);
        destSectionColorListRows.AddRange(sectionRow.GetSectionColorListRows());
        foreach (SectionDataSet.SectionColorListRow sectionColorListRow in destSectionColorListRows)
        {
          destSectionColorListItemRows.AddRange(sectionColorListRow.GetSectionColorListItemRows());
        }
        destSectionMaterialsSpeciesRows.AddRange(sectionRow.GetSectionMaterialSpeciesRows());
      }

      FrmPriceCatalogEdit frm = new FrmPriceCatalogEdit(material, currency, idPriceCatalog);
      if (frm.ShowDisposableDialog(this, false) == DialogResult.OK)
      {
        PriceCatalogCopyOptions priceCatalogCopyOptions = frm.PriceCatalogCopyOptions;
        PriceCatalogCopy priceCatalogCopy = new PriceCatalogCopy(priceCatalogCopyOptions, material);
        SectionDataSet srcSectionDS = material.ReadSectionData(new SectionFilter(null, priceCatalogCopyOptions.IdPriceCatalogSource, false), CultureInfo.CurrentCulture.Name, Session.CurrentSession.ConnectorInfo);
        priceCatalogCopy.CopyToSection(srcSectionDS, destSectionRows.ToArray());
        priceCatalogCopy.CopyToSectionColorList(srcSectionDS, destSectionColorListRows.ToArray());
        priceCatalogCopy.CopyToSectionColorListItem(srcSectionDS, destSectionColorListItemRows.ToArray());
        priceCatalogCopy.CopyToSectionMaterialSpecies(srcSectionDS, destSectionMaterialsSpeciesRows.ToArray());

      }

      frm.Dispose();
    }

    private void bbResetPriceCatalog_ItemClick(object sender, ItemClickEventArgs e)
    {
      if (viewSection.IsNewItemRow(viewSection.FocusedRowHandle))
        return;

      // Indecsii randurilor selectate pentru a se modifica preturile.
      int[] sectionRowHandles = viewSection.GetSelectedRows();
      if (sectionRowHandles.Length == 0)
        return;

      //se salveaza datele iniante de orice modificare
      DialogResult result = XtraMessageBox.Show(this,
                                              Properties.Resources.MsgSaveInformation,
                                              Properties.Resources.CaptionAttentionMsgBox,
                                              MessageBoxButtons.OKCancel,
                                              MessageBoxIcon.Question);
      if (result == DialogResult.Cancel)
      {
        return;
      }

      if (!ApplyChanges(false))
      {
        return;
      }

      List<SectionDataSet.SectionRow> destSectionRows = new List<SectionDataSet.SectionRow>();
      List<SectionDataSet.SectionColorListRow> destSectionColorListRows = new List<SectionDataSet.SectionColorListRow>();
      List<SectionDataSet.SectionColorListItemRow> destSectionColorListItemRows = new List<SectionDataSet.SectionColorListItemRow>();
      List<SectionDataSet.SectionMaterialSpeciesRow> destSectionMaterialsSpeciesRows = new List<SectionDataSet.SectionMaterialSpeciesRow>();
      foreach (int sectionRowHandle in sectionRowHandles)
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(sectionRowHandle) as SectionDataSet.SectionRow;
        destSectionRows.Add(sectionRow);
        destSectionColorListRows.AddRange(sectionRow.GetSectionColorListRows());
        foreach (SectionDataSet.SectionColorListRow sectionColorListRow in destSectionColorListRows)
        {
          destSectionColorListItemRows.AddRange(sectionColorListRow.GetSectionColorListItemRows());
        }
        destSectionMaterialsSpeciesRows.AddRange(sectionRow.GetSectionMaterialSpeciesRows());
      }

      foreach (SectionDataSet.SectionRow destSectionRow in destSectionRows)
      {
        destSectionRow.IsDefaultPrice = true;
      }


      foreach (SectionDataSet.SectionColorListRow destSectionColorListRow in destSectionColorListRows)
      {
        destSectionColorListRow.IsDefaultPrice = true;
      }

      foreach (SectionDataSet.SectionColorListItemRow destSectionColorListItemRow in destSectionColorListItemRows)
      {
        destSectionColorListItemRow.IsDefaultPrice = true;
      }

      foreach (SectionDataSet.SectionMaterialSpeciesRow destSectionMaterialsSpeciesRow in destSectionMaterialsSpeciesRows)
      {
        destSectionMaterialsSpeciesRow.IsDefaultPrice = true;
      }

      //se salveaza datele 
      ApplyChanges(true);
    }

    /// <summary>
    /// Tratare eveniment declansat pentru formatarea coloanei Pret din grid.
    /// </summary>
    private void viewSection_RowCellStyle(object sender, RowCellStyleEventArgs e)
    {
      try
      {
        GridView gridView = (GridView)sender;

        SectionDataSet.SectionRow sectionRow = gridView.GetDataRow(e.RowHandle) as SectionDataSet.SectionRow;

        if (sectionRow == null)
        {
          return;
        }

        if (e.Column == colUnitBasePrice)
        {
          if (!sectionRow.IsDefaultPrice)
          {
            e.Appearance.ForeColor = MaterialUtils.ColorPriceModify;
            e.Appearance.BackColor = Color.White;
          }
        }
      }
      catch (Exception exception)
      {
        Debug.Assert(false, exception.ToString());
        FrameworkApplication.TreatException(exception);
      }
    }

    private void bbDeleteProfileSystem_ItemClick(object sender, ItemClickEventArgs e)
    {
      // Indecsii randurilor selectate pentru a se face stergerea seriilor de pe profile.
      int[] sectionRowHandles = viewSection.GetSelectedRows();
      List<SectionDataSet.SeriesSectionRow> seriesSectionRows = new List<SectionDataSet.SeriesSectionRow>();
      foreach (int sectionRowHandle in sectionRowHandles)
      {
        SectionDataSet.SectionRow sectionRow = viewSection.GetDataRow(sectionRowHandle) as SectionDataSet.SectionRow;
        seriesSectionRows.AddRange(sectionRow.GetSeriesSectionRows().ToArray());
      }

      if (seriesSectionRows.Count == 0)
      {
        XtraMessageBox.Show(this,
                            Properties.Resources.MsgInvalidPasteSeriesSection,
                            Properties.Resources.CaptionAttentionMsgBox,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
        return;
      }
      else
      {
        if (XtraMessageBox.Show(this,
                                Properties.Resources.MsgQuestionDeleteSeriesSection,
                                Properties.Resources.CaptionAttentionMsgBox,
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.Yes)
        {
          foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in seriesSectionRows)
            seriesSectionRow.Delete();
        }
      }
    }

    private void chkShowOnlyActive_CheckedChanged(object sender, ItemClickEventArgs e)
    {
      RefreshData(true);
    }

    /// <summary>
    /// Afisare doar active
    /// </summary>
    [Browsable(false)]
    public bool ShowOnlyActive
    {
      set { chkShowOnlyActive.Checked = value; }
      get
      {
        if (!isAdmin)
          return true;

        return chkShowOnlyActive.Checked;
      }
    }

    private void viewSection_RowStyle(object sender, RowStyleEventArgs e)
    {
      GridView view = sender as GridView;
      if (e.RowHandle < 0 || view.GetRowCellValue(e.RowHandle, colIsActive) == null)
        return;

      bool isActive = (bool)view.GetRowCellValue(e.RowHandle, colIsActive);
      if (!isActive)
      {
        e.Appearance.ForeColor = SystemColors.GrayText;
      }

      if (!isActive && (view.IsRowSelected(e.RowHandle) || view.FocusedRowHandle == e.RowHandle))
      {
        e.Appearance.ForeColor = Color.LightSlateGray;
        e.Appearance.BackColor = view.Appearance.SelectedRow.BackColor;
        e.HighPriority = true;
      }
    }

    private void gridSection_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.F &&
    ((e.Modifiers & Keys.Control) == Keys.Control) &&
    ((e.Modifiers & Keys.Shift) == Keys.Shift))
      {
        using (FrmFindAndReplace frmFindAndReplace = new FrmFindAndReplace(this.viewSection,
                 new GridColumn[] { colDesignation, colCode }))
        {
          frmFindAndReplace.ShowDisposableDialog();
        }
        e.Handled = true;
      }
    }

    private void bbPasteCopiedCell_ItemClick(object sender, ItemClickEventArgs e)
    {
      List<string> ignoredColumns = new List<string>() { colCode.FieldName, colSectionType.FieldName, colMaterialType.FieldName };
      gridUtils.PasteDataColumn(ignoredColumns, sectionDS.Section.TableName, typeof(SectionDataSet));
    }

    private void repLookUpRawSectionType_EditValueChanging(object sender, ChangingEventArgs e)
    {
      if (e.NewValue == null)
      {
        return;
      }
      SectionDataSet.SectionRow sectionRow = viewSection.GetFocusedDataRow() as SectionDataSet.SectionRow;
      if (sectionRow == null)
      {
        return;
      }

      RawSectionType rawSectionType = EnumConvert<RawSectionType>.ToEnum(e.NewValue.ToString());
      RawSectionType? oldRawSectionType = null;
      if (e.OldValue != null)
      {
        oldRawSectionType = EnumConvert<RawSectionType>.ToEnum(e.OldValue.ToString());
      }

      if (oldRawSectionType.HasValue && oldRawSectionType == RawSectionType.Defined && oldRawSectionType != rawSectionType && sectionRow.UseRawSectionInfo)
      {
        SectionDataSet.SectionRow rawSection = null;
        if (!sectionRow.IsIdRawSectionNull())
          rawSection = sectionDS.Section.FindById(sectionRow.IdRawSection);

        if (rawSection != null)
        {
          foreach (SectionDataSet.SectionMaterialSpeciesRow rawSectionMaterialSpeciesRow in rawSection.GetSectionMaterialSpeciesRows())
          {
            SectionDataSet.SectionMaterialSpeciesRow newSectionMaterialSpeciesRow = sectionDS.SectionMaterialSpecies.NewSectionMaterialSpeciesRow();
            newSectionMaterialSpeciesRow.IdMaterialSpecies = rawSectionMaterialSpeciesRow.IdMaterialSpecies;
            newSectionMaterialSpeciesRow.IdSection = sectionRow.Id;
            newSectionMaterialSpeciesRow.MaterialSpeciesUsage = rawSectionMaterialSpeciesRow.MaterialSpeciesUsage;
            if (!rawSectionMaterialSpeciesRow.IsUnitWeightNull())
              newSectionMaterialSpeciesRow.UnitWeight = rawSectionMaterialSpeciesRow.UnitWeight;
            if (!rawSectionMaterialSpeciesRow.IsIxNull())
              newSectionMaterialSpeciesRow.Ix = rawSectionMaterialSpeciesRow.Ix;
            if (!rawSectionMaterialSpeciesRow.IsIyNull())
              newSectionMaterialSpeciesRow.Iy = rawSectionMaterialSpeciesRow.Iy;
            if (!rawSectionMaterialSpeciesRow.IsPriceNull())
              newSectionMaterialSpeciesRow.Price = rawSectionMaterialSpeciesRow.Price;
            if (!rawSectionMaterialSpeciesRow.IsHeatTransferCoefficientNull())
              newSectionMaterialSpeciesRow.HeatTransferCoefficient = rawSectionMaterialSpeciesRow.HeatTransferCoefficient;
            sectionDS.SectionMaterialSpecies.AddSectionMaterialSpeciesRow(newSectionMaterialSpeciesRow);
          }
        }
        sectionRow.UseRawSectionInfo = false;
      }
    }

    private void repDxfImage_ButtonPressed(object sender, ButtonPressedEventArgs e)
    {
      SectionDataSet.SectionRow sectionRow = viewSection.GetFocusedDataRow() as SectionDataSet.SectionRow;
      if (sectionRow == null)
      {
        return;
      }

      ImageEdit imgEdit = sender as ImageEdit;
      ImageConverter converter = new ImageConverter();
      if (e.Button.Kind == ButtonPredefines.Ellipsis)
      {
        using (OpenFileDialog dlg = new OpenFileDialog())
        {
          dlg.Multiselect = false;
          dlg.CheckFileExists = true;
          dlg.CheckPathExists = true;
          dlg.Filter = Settings.Default.DxfFilter;
          dlg.FilterIndex = 0;
          dlg.ValidateNames = true;
          if (dlg.ShowDialog(this) == DialogResult.OK)
          {
            try
            {
              string asciiDxf = System.IO.File.ReadAllText(dlg.FileName);
              //netDxf.DxfDocument dxf = MaterialInterfaceUtils.PruneDxf(asciiDxf);
              Bitmap bitmap = MaterialInterfaceUtils.GetDxfImage(asciiDxf);
              if (bitmap.Width == 1 || bitmap.Height == 1)
              {
                XtraMessageBox.Show(this,
                                    Properties.Resources.MsgInvalidDxf,
                                    Properties.Resources.CaptionAttentionMsgBox,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);

                imgEdit.EditValue = null;
              }
              else
              {
                sectionRow.Dxf = asciiDxf;
                imgEdit.EditValue = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
              }
            }
            catch
            {
              imgEdit.EditValue = null;
            }
          }
        }
      }
      if (e.Button.Kind == ButtonPredefines.Delete)
      {
        if (XtraMessageBox.Show(this, Properties.Resources.MsgDeleteImageQuestion,
          Properties.Resources.CaptionAttentionMsgBox,
          MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
        {
          sectionRow.Dxf = null;
          imgEdit.Image = null;
        }
      }
      else if (e.Button.Kind == ButtonPredefines.Combo)
      {
        if (!sectionRow.IsDxfNull())
        {
          Bitmap bitmap = Pyramid.Ra.Material.Interface.MaterialInterfaceUtils.GetDxfImage(sectionRow.Dxf);
          imgEdit.EditValue = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
        }
      }
    }

    private void repDxfImage_EditValueChanging(object sender, ChangingEventArgs e)
    {
      Image img = e.NewValue as Image;
      if (img != null)
      {
        MemoryStream mStream = new MemoryStream();
        img.Save(mStream, ImageFormat.Bmp);
        byte[] ret = mStream.ToArray();
        mStream.Close();
        e.NewValue = ret;
      }
    }
  }
}
