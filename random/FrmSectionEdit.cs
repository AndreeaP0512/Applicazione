using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using DevExpress.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;

using Pyramid.Framework.BaseGUI;
using Pyramid.Framework.Localization;
using Pyramid.Framework.Utils;

using Pyramid.Helpers.Currency;
using Pyramid.Helpers.Currency.Data;

using Pyramid.Ra.Material.Data;
using Pyramid.Ra.Material.Interface;
using Pyramid.Ra.Material.Properties;
using Pyramid.Ra.Geometry.Workspace;
using DevExpress.XtraBars;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraEditors.Repository;
using Pyramid.Framework;
using Pyramid.Authentication;
using Pyramid.Ra.Geometry.Primitives;
using Pyramid.Ra.Geometry;
using Pyramid.Helpers.Unit;

namespace Pyramid.Ra.Material.Forms
{

  /// <summary>
  /// Forma de editare individuala sectiuni.
  /// </summary>
  [System.Reflection.Obfuscation(Exclude = true)]
  public partial class FrmSectionEdit : BaseForm
  {
    #region Private Members

    /// <summary>
    /// Randul corespunzator profilului ce s-a deschis pentru editare
    /// </summary>
    private SectionDataSet.SectionRow sectionRow;
    /// <summary>
    /// Setul de date cu imagini.
    /// </summary>
    private ImageDataSet imageDS;
    private bool modified;
    private CurrencyExchange currencyExchange;
    /// <summary>
    /// Flag ce stabileste daca se vor doar seriile active
    /// </summary>
    bool onlyActive;
    /// <summary>
    /// Modul de deschidere al formei
    /// </summary>
    private FormOpeningMode openingMode;
    /// <summary>
    /// Flag ce indica daca ultilizatorul este administrator
    /// </summary>
    private bool isAdmin;
    private bool isRounding = false;

    #endregion Private Members

    #region Constructor

    /// <summary>
    /// Constructorul formei de editare al unui profil
    /// </summary>
    /// <param name="sectionRow">Randul corespunzator profilului ce s-a deschis pentru editare</param>
    /// <param name="sectionDS">Setul de date cu profile</param>
    /// <param name="imageDS">Setul de date cu imagini.</param>
    /// <param name="seriesDS">Setul de date cu serii de profile</param>
    /// <param name="costDS">Setul de date cu costuri</param>
    /// <param name="consumeGroupDS">Setul de date cu categorii de consum</param>
    /// <param name="currencyDS">Setul de date cu valute</param>
    /// <param name="colorDS">Setul de date cu culori</param>
    /// <param name="materialSpeciesDS">Setul de date cu tipuri de materiale</param>
    /// <param name="currencyExchange">Obiectul de conversii valutare</param>
    /// <param name="openingMode">Modul de deschidere al formei</param>
    public FrmSectionEdit(SectionDataSet.SectionRow sectionRow,
                          SectionDataSet sectionDS,
                          ImageDataSet imageDS,
                          SeriesDataSet seriesDS,
                          CostDataSet costDS,
                          ColorListDataSet colorListDS,
                          ConsumeGroupDataSet consumeGroupDS,
                          CurrencyDataSet currencyDS,
                          ColorDataSet colorDS,
                          MaterialSpeciesDataSet materialSpeciesDS,
                          CurrencyExchange currencyExchange,
                          FormOpeningMode openingMode,
                          bool onlyActive)
    {
      InitializeComponent();
      LocalizeLookUpHeader();
      InitializeUnits();
      this.AttachBestFitEvents();

      this.sectionRow = sectionRow;
      this.sectionDS = sectionDS;
      this.imageDS = imageDS;
      this.seriesDS = seriesDS;
      this.costDS = costDS;
      this.colorListDS = colorListDS;
      this.consumeGroupDS = consumeGroupDS;
      this.currencyDS = currencyDS;
      this.colorDS = colorDS;
      this.materialSpeciesDS = materialSpeciesDS;
      this.currencyExchange = currencyExchange;
      this.openingMode = openingMode;
      this.onlyActive = onlyActive;
      this.isAdmin = Session.CurrentSession.OptionsValidator.AccessInactiveRecords;

      this.Icon = Icon.FromHandle(Properties.Resources.ImgMaterialsSection.GetHicon());

      this.repLookUpCostGroupId.DataSource = costDS.CostGroup.GetCostGroupView(
          CostGroupType.Material, ApplicationRoles.GrantedCostGroupLevelRoles);

      this.repLookUpCostGroup.DataSource = costDS.CostGroup.GetCostGroupView(
          CostGroupType.Material, ApplicationRoles.GrantedCostGroupLevelRoles);
    }


    #endregion Constructor

    #region Public Properties

    /// <summary>
    /// Randul corespunzator profilului ce s-a deschis pentru editare
    /// </summary>
    public SectionDataSet.SectionRow SectionRow
    {
      get { return sectionRow; }
    }

    #endregion Public Properties

    #region Form Overrides

    protected override void OnLoad(EventArgs e)
    {
      try
      {
        using (new WaitCursor())
        {
          // se ataseaza handlerul specificat pentru tratarea modificarilor in componentele interne.
          FrameworkUtils.AttachHandlerOnModified(this, Component_Modified);

          base.OnLoad(e);

          // setare mod ReadOnly daca este cazul
          UpdateReadOnlyState();

          #region Section

          if (sectionRow == null)
          {
            // se genereaza un rand nou.
            sectionRow = GenerateDefaultRow(sectionDS, consumeGroupDS, currencyDS);
            sectionDS.Section.AddSectionRow(sectionRow);
          }

          lookUpCurrency.Properties.DataSource = currencyDS.Currency;
          lookUpPriceComputeMode.Properties.DataSource = EnumTypeLocalizer.Localize<PriceCalculationType>(new PriceCalculationType[] { PriceCalculationType.PerLength, PriceCalculationType.PerWeight, PriceCalculationType.PerVolume });
          ArrayList sectionType = EnumTypeLocalizer.Localize(typeof(SectionType));
          sectionType.Sort();
          lookUpSectionType.Properties.DataSource = sectionType;
          flagsKnownSide.Properties.AddFlagsEnum(typeof(KnownSide),
            new KnownSide[] { KnownSide.Top, KnownSide.Left, KnownSide.Bottom, KnownSide.Right, KnownSide.Vertical, KnownSide.Horizontal, KnownSide.All });

          lookUpMaterialType.Properties.DataSource = EnumTypeLocalizer.Localize(typeof(MaterialType));
          lookUpConsumeGroup.Properties.DataSource = consumeGroupDS.ConsumeGroup;

          sectionBS.DataSource = new DataView(sectionDS.Section);
          sectionBS.Filter = onlyActive ?
            string.Format("{0} = '{1}' AND {2} <> {3} AND {4} = {5}",
                      sectionDS.Section.SectionTypeColumn.ColumnName,
                      SectionType.Reinforcement,
                      sectionDS.Section.IdColumn.ColumnName,
                      sectionRow.Id,
                      sectionDS.Section.IsActiveColumn.ColumnName,
                      Boolean.TrueString) :
            string.Format("{0} = '{1}' AND {2} <> {3}",
                      sectionDS.Section.SectionTypeColumn.ColumnName,
                      SectionType.Reinforcement,
                      sectionDS.Section.IdColumn.ColumnName,
                      sectionRow.Id);
          sectionBS.Sort = sectionDS.Section.CodeColumn.ColumnName;
          repGridLookUpColorReinforcement.DataSource = sectionBS;
          repGridLookUpColorListReinforcement.DataSource = sectionBS;
          repGridLookUpSectionReinforcementCode.DataSource = sectionBS;

          rawSectionBS.DataSource = new DataView(sectionDS.Section);
          rawSectionBS.Filter = onlyActive ?
            string.Format("{0} = '{1}' AND {2} <> {3} AND {4} = {5}",
                      sectionDS.Section.SectionTypeColumn.ColumnName,
                      SectionType.RawSection,
                      sectionDS.Section.IdColumn.ColumnName,
                      sectionRow.Id,
                      sectionDS.Section.IsActiveColumn.ColumnName,
                      Boolean.TrueString) :
            string.Format("{0} = '{1}' AND {2} <> {3}",
                      sectionDS.Section.SectionTypeColumn.ColumnName,
                      SectionType.RawSection,
                      sectionDS.Section.IdColumn.ColumnName,
                      sectionRow.Id);
          lookUpRawSectionType.Properties.DataSource = EnumTypeLocalizer.Localize(typeof(RawSectionType));
          lookUpRawSection.Properties.DataSource = rawSectionBS;
          lookUpArcRawSection.Properties.DataSource = rawSectionBS;

          lookUpFixingMode.Properties.DataSource = EnumTypeLocalizer.Localize(typeof(SurfaceFixingMode));
          lookUpCuttingType.Properties.DataSource = EnumTypeLocalizer.Localize<CuttingType>();
          lookUpCurvingMode.Properties.DataSource = EnumTypeLocalizer.Localize<CurvingMode>();
          lookUpCoversInnerTemplates.Properties.DataSource = EnumTypeLocalizer.Localize<ViewSide>();
          lookUpCornerCuttingType.Properties.DataSource = EnumTypeLocalizer.Localize(typeof(CornerCuttingType));
          lookUpExtendingMode.Properties.DataSource = EnumTypeLocalizer.Localize(typeof(ExtendingMode));
          lookUpAltersInnerGeometry.Properties.DataSource = BoolTypeLocalizer.Localize();
          lookUpMaterialSpeciesUsage.Properties.DataSource = EnumTypeLocalizer.Localize<MaterialSpeciesUsage>(
          new MaterialSpeciesUsage[] { MaterialSpeciesUsage.Main, MaterialSpeciesUsage.Secondary });

          sectionReinforcementBS.DataSource = sectionDS.SectionReinforcement;
          sectionReinforcementBS.Filter = string.Format("{0} = {1}", sectionDS.SectionReinforcement.IdSectionColumn.ColumnName, sectionRow.Id);
          gridReinforcements.DataSource = sectionReinforcementBS;
          repLookUpSectionReinforcementCuttingType.DataSource = EnumTypeLocalizer.Localize(typeof(ComponentCuttingType));

          bool hasImage = false;
          foreach (SectionDataSet.SectionRow row in sectionDS.Section)
          {
            if (row.RowState != DataRowState.Deleted)
            {
              if (imageDS.Image.GetImage(row.Guid) != null)
              {
                hasImage = true;
                break;
              }
            }
          }
          if (!hasImage)
          {
            colGridLookUpDefaultReinforcementViewImage.Visible = false;
            colGridLookUpColorListReinforcementViewImage.Visible = false;
            colGridLookUpColorReinforcementViewImage.Visible = false;
            colGridLookUpHeatTransferSectionImage.Visible = false;
            colGridLookUpSectionReinforcementImage.Visible = false;
            colLookUpCustomSectionItemImage.Visible = false;
            colLookUpSectionCoverSectionImage.Visible = false;
            colLookUpToleranceImage.Visible = false;
          }
          else
          {
            colGridLookUpDefaultReinforcementViewImage.Visible = true;
            colGridLookUpColorListReinforcementViewImage.Visible = true;
            colGridLookUpColorReinforcementViewImage.Visible = true;
            colGridLookUpHeatTransferSectionImage.Visible = true;
            colGridLookUpSectionReinforcementImage.Visible = true;
            colLookUpCustomSectionItemImage.Visible = true;
            colLookUpSectionCoverSectionImage.Visible = true;
            colLookUpToleranceImage.Visible = true;
          }

          #endregion Section

          #region Series

          seriesBS.DataSource = sectionDS.SeriesSection;
          seriesBS.Filter = onlyActive ?
            string.Format("{0} = {1} AND {2} = {3}", sectionDS.SeriesSection.IdSectionColumn.ColumnName, sectionRow.Id, sectionDS.SeriesSection.IsSeriesActiveColumn.ColumnName, Boolean.TrueString) :
            string.Format("{0} = {1}", sectionDS.SeriesSection.IdSectionColumn.ColumnName, sectionRow.Id);
          gridSeries.DataSource = seriesBS;

          repLookUpSeriesIdSeries.DataSource = seriesDS.Series;
          repLookUpSeriesMaterialType.DataSource = EnumTypeLocalizer.Localize(typeof(MaterialType), MaterialUtils.SupportedMaterialTypes.ToArray());

          #endregion Series

          #region Colors

          string filterSectionColorList = string.Format("{0} = {1}",
                                                        sectionDS.SectionColorList.IdSectionColumn.ColumnName,
                                                        sectionRow.Id);
          sectionColorListBS.DataSource = sectionDS.SectionColorList;
          sectionColorListBS.Filter = filterSectionColorList;

          gridSectionColorList.DataSource = sectionColorListBS;
          colorListBS.DataSource = colorListDS.ColorList;

          viewColorListItem.OptionsBehavior.Editable = false;

          colorCombinationBS.DataSource = colorDS.ColorCombination;
          costGroupBS.DataSource = costDS.CostGroup;
          repLookUpPriceCalculationType.DataSource = EnumTypeLocalizer.Localize<PriceCalculationType>(new PriceCalculationType[] { PriceCalculationType.PerLength, PriceCalculationType.PerSurface, PriceCalculationType.PerWeight, PriceCalculationType.PerVolume });
          repLookUpEditingMode.DataSource = EnumTypeLocalizer.Localize<ColorListEditingMode>(new ColorListEditingMode[] { ColorListEditingMode.None, ColorListEditingMode.EditColorListPrice, ColorListEditingMode.EditColorListColorCombinations });

          ColorListDataSet.ColorListRow[] colorListRows = onlyActive ?
            colorListDS.ColorList.Where(row => row.ConvertedPriceCalculationType != PriceCalculationType.None && row.IsActive)
                                                                               .OrderBy(row => row.Code)
                                                                               .ToArray() :
            colorListDS.ColorList.Where(row => row.ConvertedPriceCalculationType != PriceCalculationType.None)
                                                                               .OrderBy(row => row.Code)
                                                                               .ToArray();
          repLookUpColorListId.DataSource = colorListRows;
          repLookUpColorCombinationCode.DataSource = colorCombinationBS;
          repLookUpColorCombinationDesignation.DataSource = colorCombinationBS;

          SetValueFormat();

          barManager.SetPopupContextMenu(gridSectionColorList, popupMenu);
          barManager.SetPopupContextMenu(gridColorListItem, popupMenu);

          if (sectionReinforcementBS.Count == 0)
          {
            UpdateIdReinforcementColumn(false);
          }

          #endregion Colors

          #region MaterialSpecies

          SetGridMaterialSpeciesDataSource();

          #endregion MaterialSpecies

          #region CustomSectionItem


          customSectionItemBS.DataSource = sectionDS.CustomSectionItem;
          customSectionItemBS.Filter = string.Format("{0} = {1}",
                                                      sectionDS.CustomSectionItem.IdParentSectionColumn.ColumnName,
                                                      sectionRow.Id);
          gridCustomSectionItem.DataSource = customSectionItemBS;

          DataView custemSectionDataSourceView = new DataView(sectionDS.Section);
          custemSectionDataSourceView.RowFilter = string.Format("{0} <> {1}",
            sectionDS.Section.IdColumn.ColumnName, sectionRow.Id);
          repLookUpCustomSectionCuttingType.DataSource = EnumTypeLocalizer.Localize(typeof(ComponentCuttingType));
          repGridLookUpCustomSectionItemCode.DataSource = custemSectionDataSourceView;//sectionDS.Section;
          repLookUpEditMaterialSpeciesUsageCustomSectionItem.DataSource = EnumTypeLocalizer.Localize<MaterialSpeciesUsage>(
          new MaterialSpeciesUsage[] { MaterialSpeciesUsage.Main, MaterialSpeciesUsage.Secondary });

          colMaterialSpeciesUsageCustomSectionItem.Visible = sectionRow.MaterialType == MaterialType.Wood.ToString();

          #endregion CustomSectionItem

          #region SectionCover

          DataView sectionCoverDV = new DataView(sectionDS.SectionCover);
          sectionCoverDV.RowFilter = string.Format("{0} = {1}",
                                                   sectionDS.SectionCover.IdSectionColumn.ColumnName,
                                                   sectionRow.Id);
          gridSectionCover.DataSource = sectionCoverDV;

          repGridLookUpSectionCoverSection.DataSource = sectionDS.Section.Select(string.Format("{0} = 'Cover'",
                                                                                               sectionDS.Section.SectionTypeColumn.ColumnName));

          #endregion SectionCover

          #region Optimization

          lookUpOptimizationInventoryUseType.Properties.DataSource = EnumTypeLocalizer.Localize(typeof(OptimizationInventoryUseType));

          #endregion Optimization

          #region HeatTransfer

          sectionHeatTransferCoefficientBS.DataSource = sectionDS.SectionHeatTransferCoefficient;
          sectionHeatTransferCoefficientBS.Filter = string.Format("{0} = {1} OR {0} = {2}",
            sectionRow.Id,
            sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName,
            sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName);
          gridSectionHeatTransferCoefficient.DataSource = sectionHeatTransferCoefficientBS;

          DataView sectionsHeatTransferDataSourceView = new DataView(sectionDS.Section);
          repGridLookUpHeatTransferSection.DataSource = sectionDS.Section;
          repLookUpSHTSectionType.DataSource = EnumTypeLocalizer.Localize(typeof(SectionType));

          #endregion HeatTransfer

          #region Tolerance

          sectionToleranceBS.DataSource = sectionDS.SectionTolerance;
          sectionToleranceBS.Filter = string.Format("{0} = {1}",
            sectionRow.Id,
            sectionDS.SectionTolerance.IdSection1Column.ColumnName);
          gridTolerance.DataSource = sectionToleranceBS;

          DataView sectionToleranceDataSourceView = new DataView(sectionDS.Section);
          repGridLookUpToleranceSection.DataSource = sectionDS.Section;
          repLookUpToleranceSectionType.DataSource = EnumTypeLocalizer.Localize(typeof(SectionType));

          #endregion Tolerance

          // incarcare informatii in controale.
          WriteControls();

          //actualizare stare controle arc
          UpdateArchControls();

          //actualizare stare controale de segment
          UpdateSegmentControls();

          // actualizare stare controale de optimizare.
          UpdateOptimizationControls();

          // valideaza controlul cu focusul ca si acesta sa declanseze evenimentul de modificare.
          Validate();

          this.modified = false;
        }
      }
      catch (Exception exception)
      {
        Debug.Assert(false, exception.ToString());
        throw;
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (DialogResult == DialogResult.OK)
      {
        if ((!ReadControls() || !viewSeries.UpdateCurrentRow() ||
            !viewSectionColorList.UpdateCurrentRow() ||
            !viewColorListItem.UpdateCurrentRow() ||
            !viewCustomSectionItem.UpdateCurrentRow() ||
            !viewSectionHeatTransferCoefficient.UpdateCurrentRow()))
        {
          e.Cancel = true;
          return;
        }
        if (openingMode == FormOpeningMode.Locked && modified)
        {
          Debug.Assert(false, "Datele au fost modificate! Baza de date este blocata!");
        }
      }

      base.OnClosing(e);
    }

    #endregion Form Overrides

    #region Event Handlers

    #region Section

    /// <summary>
    /// Actualizeaza imaginea sectiunii atunci cand se schimba tipul ei
    /// </summary>
    private void lookUpSectionType_EditValueChanging(object sender, ChangingEventArgs e)
    {
      if (e.NewValue == null)
      {
        imgSchema.Image = null;
        return;
      }

      SectionType sectionType = (SectionType)e.NewValue;

      if (lookUpMaterialType.EditValue != null)
      {
        Image img = GetImage(sectionType, (MaterialType)lookUpMaterialType.EditValue);
        imgSchema.Image = img;
      }

      switch (sectionType)
      {
        case SectionType.Track:
        case SectionType.TrackJambVerticalSlide:
        case SectionType.TrackInsectScreen:
        case SectionType.MullionWithTrack:
        case SectionType.Compensation:
        case SectionType.TrackSillVerticalSlide:
        case SectionType.TrackHeadVerticalSlide:
          txtCouplingAngle.Properties.ReadOnly = true;
          chkCouplingAngleFixed.Enabled = false;
          txtTrackNumber.Properties.ReadOnly = false;
          lookUpAltersInnerGeometry.Properties.ReadOnly = true;
          break;
        case SectionType.FrameExpansion:
        case SectionType.LedgeInt:
        case SectionType.LedgeExt:
        case SectionType.SubSill:
        case SectionType.Custom:
        case SectionType.HousingRollerShutter:
          txtCouplingAngle.Properties.ReadOnly = true;
          chkCouplingAngleFixed.Enabled = false;
          txtTrackNumber.Properties.ReadOnly = true;
          lookUpAltersInnerGeometry.Properties.ReadOnly = false;
          break;
        case SectionType.Joining:
        case SectionType.Corner:
          txtCouplingAngle.Properties.ReadOnly = false;
          chkCouplingAngleFixed.Enabled = true;
          txtTrackNumber.Properties.ReadOnly = true;
          lookUpAltersInnerGeometry.Properties.ReadOnly = false;
          break;
        default:
          txtCouplingAngle.Properties.ReadOnly = true;
          chkCouplingAngleFixed.Enabled = false;
          txtTrackNumber.Properties.ReadOnly = true;
          lookUpAltersInnerGeometry.Properties.ReadOnly = true;
          break;
      }

      txtTrackNumber.Update();
      lookUpAltersInnerGeometry.Update();
    }

    private void lookUpSectionType_Validating(object sender, CancelEventArgs e)
    {
      SectionType sectionType = (SectionType)lookUpSectionType.EditValue;

      SectionType oldSectionType = sectionRow.ConvertedSectionType;
      if (oldSectionType == (SectionType)lookUpSectionType.EditValue)
        return;

      // daca profilul este armatura si i-am schimbat tipul in altceva, 
      // verific daca exista vreun profil armat cu acesta, si daca da
      // se avertizeaza, si se deazarmeaza toate profilele ce-l folosesc
      if (oldSectionType == SectionType.Reinforcement)
      {
        string filter = string.Format("[{0}] = {1}",
             sectionDS.Section.IdReinforcementColumn.ColumnName,
             sectionRow.Id);
        if (sectionDS.Section.Select(filter).Length > 0)
        {
          DialogResult result = XtraMessageBox.Show(this,
                                 Properties.Resources.MsgErrorSectionReinforcementUsed,
                                  Properties.Resources.CaptionAttentionMsgBox,
                                  MessageBoxButtons.OKCancel,
                                  MessageBoxIcon.Exclamation);
          if (result == DialogResult.Cancel)
          {
            lookUpSectionType.EditValue = oldSectionType;
            return;
          }
          else
          {
            foreach (SectionDataSet.SectionRow reinforcedSection in sectionDS.Section.Select(filter))
            {
              reinforcedSection.SetIdReinforcementNull();
            }
          }
        }

        // daca profilul este brut si i-am schimbat tipul in altceva, 
        // verific daca exista vreun profil ce il foloseste pe acesta
        // ca profil brut, si daca da se avertizeaza si se trec toate 
        // profilele ce-l folosesc la profil brut null.
        if (oldSectionType == SectionType.RawSection)
        {
          filter = string.Format("[{0}] = {1}",
               sectionDS.Section.IdRawSectionColumn.ColumnName,
               sectionRow.Id);
          if (sectionDS.Section.Select(filter).Length > 0)
          {
            DialogResult result = XtraMessageBox.Show(this,
                                   Properties.Resources.MsgErrorRawSectionUsed,
                                    Properties.Resources.CaptionAttentionMsgBox,
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Exclamation);
            if (result == DialogResult.Cancel)
            {
              lookUpSectionType.EditValue = oldSectionType;
              return;
            }
            else
            {
              foreach (SectionDataSet.SectionRow section in sectionDS.Section.Select(filter))
              {
                section.SetIdRawSectionNull();
              }
            }
          }
        }
      }

      if (sectionType == SectionType.Reinforcement)
      {
        if (sectionReinforcementBS.Count > 0)
        {
          XtraMessageBox.Show(this,
                              Properties.Resources.MsgErrorSectionCannotBecomeReinforcement,
                              Properties.Resources.CaptionAttentionMsgBox,
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Exclamation);
          lookUpSectionType.EditValue = sectionRow.ConvertedSectionType;
          return;
        }
        else
        {
          tabReinforcements.PageVisible = false;
          viewCustomSectionItem.OptionsBehavior.Editable = false;
        }
      }
      else if (sectionType == SectionType.RawSection)
      {
        // Un profil ce are profil brut nu poate deveni profil brut.
        if ((RawSectionType)lookUpRawSectionType.EditValue != RawSectionType.None)
        {
          XtraMessageBox.Show(this,
                              Properties.Resources.MsgErrorSectionCannotBecomeRawSection,
                              Properties.Resources.CaptionAttentionMsgBox,
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Exclamation);
          lookUpSectionType.EditValue = sectionRow.ConvertedSectionType;
          return;
        }
        // Un profil trebuie sa fie din lemn pentru a putea deveni profil brut.
        if (lookUpMaterialType.EditValue != null && (MaterialType)lookUpMaterialType.EditValue != MaterialType.Wood)
        {
          XtraMessageBox.Show(this,
                              Properties.Resources.MsgErrorSectionIsNotWood,
                              Properties.Resources.CaptionAttentionMsgBox,
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Exclamation);
          lookUpSectionType.EditValue = sectionRow.ConvertedSectionType;
          return;
        }
        // Un profil compus nu poate deveni profil brut.
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
            lookUpSectionType.EditValue = sectionRow.ConvertedSectionType;
            return;
          }
        }
        groupRawSection.Enabled = false;
        groupArcRawSection.Enabled = false;
        viewCustomSectionItem.OptionsBehavior.Editable = false;
      }
      else
      {
        if (lookUpMaterialType.EditValue != null && (MaterialType)lookUpMaterialType.EditValue == MaterialType.Wood)
        {
          groupRawSection.Enabled = openingMode == FormOpeningMode.Normal;
          groupArcRawSection.Enabled = openingMode == FormOpeningMode.Normal;
        }

        if (lookUpMaterialType.EditValue != null && (MaterialType)lookUpMaterialType.EditValue == MaterialType.PVC)
        {
          tabReinforcements.PageVisible = openingMode == FormOpeningMode.Normal;
        }
        viewCustomSectionItem.OptionsBehavior.Editable = openingMode == FormOpeningMode.Normal;
      }

      if (sectionType == SectionType.Track || sectionType == SectionType.TrackInsectScreen || sectionType == SectionType.MullionWithTrack || sectionType == SectionType.TrackHeadVerticalSlide
        || sectionType == SectionType.TrackJambVerticalSlide || sectionType == SectionType.TrackMullionRollerShutter || sectionType == SectionType.TrackRollerInsectScreen
        || sectionType == SectionType.TrackRollerShutter || sectionType == SectionType.TrackSillVerticalSlide)
      {
        txtTrackNumber.Enabled = openingMode == FormOpeningMode.Normal;
        int trackNumber = Convert.ToInt32(txtTrackNumber.EditValue);
        if (trackNumber == 0)
        {
          if (sectionType == SectionType.TrackInsectScreen)
          {
            txtTrackNumber.EditValue = Properties.Settings.Default.SectionDefaultInsectScreenTrackNumber;
          }
          else
          {
            txtTrackNumber.EditValue = Properties.Settings.Default.SectionDefaultTrackNumber;
          }
        }
      }
      else
      {
        txtTrackNumber.Enabled = false;
        txtTrackNumber.EditValue = 0;
      }

      if (sectionType == SectionType.Frame && ((ViewSide)lookUpCoversInnerTemplates.EditValue) == ViewSide.Unknown)
      {
        lookUpCoversInnerTemplates.EditValue = ViewSide.Outside;
      }


      // Setare default Mod Fixare Suprafete.
      switch (sectionType)
      {
        case SectionType.Frame:
          lookUpCoversInnerTemplates.EditValue = ViewSide.Outside;
          break;
        case SectionType.SashWindowInt:
        case SectionType.SashWindowExt:
        case SectionType.SashDoorInt:
        case SectionType.SashDoorExt:
        case SectionType.Mullion:
        case SectionType.BottomRail:
          lookUpFixingMode.EditValue = SurfaceFixingMode.Bead;
          lookUpCoversInnerTemplates.EditValue = ViewSide.Unknown;
          break;
        default:
          lookUpFixingMode.EditValue = SurfaceFixingMode.None;
          lookUpCoversInnerTemplates.EditValue = ViewSide.Unknown;
          break;
      }

      Image img = null;
      if (lookUpMaterialType.EditValue != null)
      {
        img = GetImage(sectionType, (MaterialType)lookUpMaterialType.EditValue);
      }

      switch (sectionType)
      {
        case SectionType.Mullion:
        case SectionType.MullionInsectScreen:
          {
            lookUpCuttingType.EditValue = CuttingType.StraightCut;
            lookUpCuttingType.Enabled = true;
            break;
          }
        default:
          {
            lookUpCuttingType.EditValue = CuttingType.StraightCut;
            lookUpCuttingType.Enabled = false;
            break;
          }
      }

      imgSchema.Image = img;

      if (sectionRow.ConvertedSectionType != sectionType)
      {
        sectionRow.ConvertedSectionType = sectionType;
      }
    }

    private void lookUpMaterialType_Validating(object sender, CancelEventArgs e)
    {
      if ((SectionType)lookUpSectionType.EditValue == SectionType.RawSection &&
           (MaterialType)lookUpMaterialType.EditValue != sectionRow.ConvertedMaterialType)
      {
        XtraMessageBox.Show(this,
             Properties.Resources.MsgErrorSectionIsNotWood,
             Properties.Resources.CaptionErrorMsgBox,
             MessageBoxButtons.OK,
             MessageBoxIcon.Exclamation);
        lookUpMaterialType.EditValue = sectionRow.ConvertedMaterialType;
        return;
      }

      if ((MaterialType)lookUpMaterialType.EditValue != sectionRow.ConvertedMaterialType &&
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
          lookUpMaterialType.EditValue = sectionRow.ConvertedMaterialType;
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

      if ((MaterialType)lookUpMaterialType.EditValue != sectionRow.ConvertedMaterialType &&
         (MaterialType)lookUpMaterialType.EditValue != MaterialType.PVC)
      {
        SectionDataSet.SectionReinforcementRow[] sectionReinforcementRows = sectionDS.SectionReinforcement.Where(row => row.IdSection == sectionRow.Id && row.RowState != DataRowState.Deleted).ToArray();
        foreach (SectionDataSet.SectionReinforcementRow row in sectionReinforcementRows)
        {
          row.Delete();
        }
      }

      if (sectionRow.ConvertedMaterialType != (MaterialType)lookUpMaterialType.EditValue)
      {
        sectionRow.ConvertedMaterialType = (MaterialType)lookUpMaterialType.EditValue;
      }
    }

    private void cmdOpenImage_Click(object sender, EventArgs e)
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
            imgImage.EditValue = imageBuffer;
          }
          catch
          {
            imgImage.EditValue = null;
          }
        }
      }
    }

    private void cmdDeleteImage_Click(object sender, System.EventArgs e)
    {
      if (XtraMessageBox.Show(this, Properties.Resources.MsgDeleteImageQuestion,
        Properties.Resources.CaptionAttentionMsgBox,
        MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
        imgImage.Image = null;
    }

    private void txtCode_Leave(object sender, System.EventArgs e)
    {
      if (string.IsNullOrEmpty(txtDesignation.Text))
      {
        txtDesignation.Text = txtCode.Text;
      }


      if (openingMode != FormOpeningMode.Locked && txtCode.Text != sectionRow.Code)
      {
        foreach (SectionDataSet.SectionRow existingSectionRow in sectionDS.Section)
        {
          if (sectionRow != existingSectionRow && txtCode.Text == existingSectionRow.Code)
          {
            XtraMessageBox.Show(this, string.Format(Properties.Resources.MsgErrorCodeNotUnique, txtCode.Text),
                     Properties.Resources.CaptionAttentionMsgBox,
                     MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtCode.Text = sectionRow.Code;
            return;
          }
        }

        //foreach (SectionDataSet.SectionColorListRow sectionColorListRow in sectionRow.GetSectionColorListRows())
        //{
        //  if (sectionColorListRow.EditingMode == ColorListEditingMode.EditColorListColorCombinations.ToString() &&
        //             sectionColorListRow.GetSectionColorListItemRows().Length != 0)
        //  {
        //    if (XtraMessageBox.Show(this, Properties.Resources.MsgRegenerateCodesQuestion,
        //         Properties.Resources.CaptionAttentionMsgBox,
        //         MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        //    {
        //      sectionRow.RegenerateSectionColorListItemCodes(colorDS, colorListDS, txtCode.Text, sectionRow.Code);
        //    }

        //    break;
        //  }
        //}

        if (sectionRow.Code != txtCode.Text)
        {
          sectionRow.Code = txtCode.Text;
        }
        SetGridColorItemDataSource();
      }
    }

    private void lookUpMaterialType_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
    {
      if (e.NewValue == null)
      {
        return;
      }

      MaterialType materialType = (MaterialType)e.NewValue;
      MaterialType? oldMaterialType = null;
      if (e.OldValue != null)
      {
        oldMaterialType = (MaterialType)e.OldValue;
      }

      // daca valorile actuale ale tolerantelor si lungimii sunt default-urile pentru 
      // vechiul tip de material, se pun tolerantele default ale noului tip de material
      decimal cuttingTolerance = Convert.ToDecimal(unitCuttingTolerance.EditValue);
      decimal bindingTolerance = Convert.ToDecimal(unitBindingTolerance.EditValue);
      decimal processingTolerance = Convert.ToDecimal(unitProcessingTolerance.EditValue);
      decimal curvingAddition = Convert.ToDecimal(unitCurvingAddition.EditValue);
      decimal barLength = Convert.ToDecimal(unitBarLength.EditValue);

      bool setDefaultCuttingTolerance = false;
      bool setDefaultBindingTolerance = false;
      bool setDefaultProcessingTolerance = false;
      bool setDefaultCurvingAddition = false;
      bool setDefaultBarLength = false;

      if (oldMaterialType.HasValue)
      {
        switch (oldMaterialType.Value)
        {
          case MaterialType.Aluminium:
            {
              setDefaultCuttingTolerance = cuttingTolerance == Properties.Settings.Default.SectionDefaultCuttingToleranceAluminium;
              setDefaultBindingTolerance = bindingTolerance == Properties.Settings.Default.SectionDefaultBindingToleranceAluminium;
              setDefaultProcessingTolerance = processingTolerance == Properties.Settings.Default.SectionDefaultProcessingToleranceAluminium;
              setDefaultCurvingAddition = curvingAddition == Properties.Settings.Default.SectionDefaultCurvingAdditionAluminium;
              setDefaultBarLength = barLength == Properties.Settings.Default.SectionDefaultBarLengthAluminium;
              break;
            }
          case MaterialType.PVC:
            {
              setDefaultCuttingTolerance = cuttingTolerance == Properties.Settings.Default.SectionDefaultCuttingTolerancePVC;
              setDefaultBindingTolerance = bindingTolerance == Properties.Settings.Default.SectionDefaultBindingTolerancePVC;
              setDefaultProcessingTolerance = processingTolerance == Properties.Settings.Default.SectionDefaultProcessingTolerancePVC;
              setDefaultCurvingAddition = curvingAddition == Properties.Settings.Default.SectionDefaultCurvingAdditionPVC;
              setDefaultBarLength = barLength == Properties.Settings.Default.SectionDefaultBarLengthPVC;
              break;
            }
          case MaterialType.Wood:
            {
              setDefaultCuttingTolerance = cuttingTolerance == Properties.Settings.Default.SectionDefaultCuttingToleranceWood;
              setDefaultBindingTolerance = bindingTolerance == Properties.Settings.Default.SectionDefaultBindingToleranceWood;
              setDefaultProcessingTolerance = processingTolerance == Properties.Settings.Default.SectionDefaultProcessingToleranceWood;
              setDefaultCurvingAddition = curvingAddition == Properties.Settings.Default.SectionDefaultCurvingAdditionWood;
              setDefaultBarLength = barLength == Properties.Settings.Default.SectionDefaultBarLengthWood;
              break;
            }
          case MaterialType.Steel:
            {
              setDefaultCuttingTolerance = cuttingTolerance == Properties.Settings.Default.SectionDefaultCuttingToleranceSteel;
              setDefaultBindingTolerance = bindingTolerance == Properties.Settings.Default.SectionDefaultBindingToleranceSteel;
              setDefaultProcessingTolerance = processingTolerance == Properties.Settings.Default.SectionDefaultProcessingToleranceSteel;
              setDefaultCurvingAddition = curvingAddition == Properties.Settings.Default.SectionDefaultCurvingAdditionSteel;
              setDefaultBarLength = barLength == Properties.Settings.Default.SectionDefaultBarLengthSteel;
              break;
            }
          default:
            {
              Debug.Assert(false, "Tip necunoscut de material");
              break;
            }
        }
      }

      switch (materialType)
      {
        case MaterialType.Aluminium:
          {
            #region Aluminium
            tabMaterialSpecies.PageVisible = false;
            if (setDefaultBarLength)
            {
              unitBarLength.EditValue = Properties.Settings.Default.SectionDefaultBarLengthAluminium;
            }
            if (setDefaultBindingTolerance)
            {
              unitBindingTolerance.EditValue = Properties.Settings.Default.SectionDefaultBindingToleranceAluminium;
            }
            if (setDefaultCuttingTolerance)
            {
              unitCuttingTolerance.EditValue = Properties.Settings.Default.SectionDefaultCuttingToleranceAluminium;
            }
            if (setDefaultProcessingTolerance)
            {
              unitProcessingTolerance.EditValue = Properties.Settings.Default.SectionDefaultProcessingToleranceAluminium;
            }
            if (setDefaultCurvingAddition)
            {
              unitCurvingAddition.EditValue = Properties.Settings.Default.SectionDefaultCurvingAdditionAluminium;
            }
            if (lookUpPriceComputeMode.EditValue != null && ((PriceCalculationType)lookUpPriceComputeMode.EditValue != PriceCalculationType.PerWeight))
            {
              lookUpPriceComputeMode.EditValue = PriceCalculationType.PerWeight;
            }

            chkHasThermalBreak.Enabled = true;

            tabReinforcements.PageVisible = lookUpSectionType.EditValue == null || (SectionType)lookUpSectionType.EditValue != SectionType.Reinforcement;
            //gridLookUpDefaultReinforcement.EditValue = null;
            groupRawSection.Enabled = false;
            groupArcRawSection.Enabled = false;
            lookUpRawSection.EditValue = null;
            lookUpArcRawSection.EditValue = null;
            lookUpRawSectionType.EditValue = RawSectionType.None;
            chkArcRawSection.EditValue = false;
            lblRawSectionTolerance.Enabled = false;
            lblArcRawSectionTolerance.Enabled = false;
            #endregion Aluminium
            break;
          }
        case MaterialType.PVC:
          {
            #region PVC
            tabMaterialSpecies.PageVisible = false;
            if (setDefaultBarLength)
            {
              unitBarLength.EditValue = Properties.Settings.Default.SectionDefaultBarLengthPVC;
            }
            if (setDefaultBindingTolerance)
            {
              unitBindingTolerance.EditValue = Properties.Settings.Default.SectionDefaultBindingTolerancePVC;
            }
            if (setDefaultCuttingTolerance)
            {
              unitCuttingTolerance.EditValue = Properties.Settings.Default.SectionDefaultCuttingTolerancePVC;
            }
            if (setDefaultProcessingTolerance)
            {
              unitProcessingTolerance.EditValue = Properties.Settings.Default.SectionDefaultProcessingTolerancePVC;
            }
            if (setDefaultCurvingAddition)
            {
              unitCurvingAddition.EditValue = Properties.Settings.Default.SectionDefaultCurvingAdditionPVC;
            }

            if (lookUpPriceComputeMode.EditValue != null && ((PriceCalculationType)lookUpPriceComputeMode.EditValue != PriceCalculationType.PerLength))
            {
              lookUpPriceComputeMode.EditValue = PriceCalculationType.PerLength;
            }

            groupRawSection.Enabled = false;
            groupArcRawSection.Enabled = false;
            lookUpRawSection.EditValue = null;
            lookUpArcRawSection.EditValue = null;
            lookUpRawSectionType.EditValue = RawSectionType.None;
            chkArcRawSection.EditValue = false;
            chkHasThermalBreak.Enabled = false;
            tabReinforcements.PageVisible = lookUpSectionType.EditValue == null || (SectionType)lookUpSectionType.EditValue != SectionType.Reinforcement;

            lblRawSectionTolerance.Enabled = false;
            lblArcRawSectionTolerance.Enabled = false;
            #endregion PVC
            break;
          }
        case MaterialType.Wood:
          {
            #region Wood
            tabMaterialSpecies.PageVisible = true;
            if (setDefaultBarLength)
            {
              unitBarLength.EditValue = Properties.Settings.Default.SectionDefaultBarLengthWood;
            }
            if (setDefaultBindingTolerance)
            {
              unitBindingTolerance.EditValue = Properties.Settings.Default.SectionDefaultBindingToleranceWood;
            }
            if (setDefaultCuttingTolerance)
            {
              unitCuttingTolerance.EditValue = Properties.Settings.Default.SectionDefaultCuttingToleranceWood;
            }
            if (setDefaultProcessingTolerance)
            {
              unitProcessingTolerance.EditValue = Properties.Settings.Default.SectionDefaultProcessingToleranceWood;
            }
            if (setDefaultCurvingAddition)
            {
              unitCurvingAddition.EditValue = Properties.Settings.Default.SectionDefaultCurvingAdditionWood;
            }

            chkHasThermalBreak.Enabled = false;
            tabReinforcements.PageVisible = false;
            gridLookUpDefaultReinforcement.EditValue = null;
            lblRawSectionTolerance.Enabled = true;
            lblArcRawSectionTolerance.Enabled = true;
            groupRawSection.Enabled = lookUpSectionType.EditValue == null || (SectionType)lookUpSectionType.EditValue != SectionType.RawSection;
            groupArcRawSection.Enabled = lookUpSectionType.EditValue == null || (SectionType)lookUpSectionType.EditValue != SectionType.RawSection;
            #endregion Wood
            break;
          }
        case MaterialType.Steel:
          {
            #region Steel
            tabMaterialSpecies.PageVisible = false;
            if (setDefaultBarLength)
            {
              unitBarLength.EditValue = Properties.Settings.Default.SectionDefaultBarLengthSteel;
            }
            if (setDefaultBindingTolerance)
            {
              unitBindingTolerance.EditValue = Properties.Settings.Default.SectionDefaultBindingToleranceSteel;
            }
            if (setDefaultCuttingTolerance)
            {
              unitCuttingTolerance.EditValue = Properties.Settings.Default.SectionDefaultCuttingToleranceSteel;
            }
            if (setDefaultProcessingTolerance)
            {
              unitProcessingTolerance.EditValue = Properties.Settings.Default.SectionDefaultProcessingToleranceSteel;
            }
            if (setDefaultCurvingAddition)
            {
              unitCurvingAddition.EditValue = Properties.Settings.Default.SectionDefaultCurvingAdditionSteel;
            }
            if (lookUpPriceComputeMode.EditValue != null && ((PriceCalculationType)lookUpPriceComputeMode.EditValue != PriceCalculationType.PerLength))
            {
              lookUpPriceComputeMode.EditValue = PriceCalculationType.PerLength;
            }

            chkHasThermalBreak.Enabled = true;
            tabReinforcements.PageVisible = false;
            gridLookUpDefaultReinforcement.EditValue = null;

            groupRawSection.Enabled = false;
            groupArcRawSection.Enabled = false;
            lookUpRawSection.EditValue = null;
            lookUpArcRawSection.EditValue = null;
            lookUpRawSectionType.EditValue = RawSectionType.None;
            chkArcRawSection.EditValue = false;
            lblRawSectionTolerance.Enabled = false;
            lblArcRawSectionTolerance.Enabled = false;
            #endregion Steel
            break;
          }
        default:
          {
            tabMaterialSpecies.PageVisible = false;
            Debug.Assert(false, "Tip necunoscut de material");
            break;
          }
      }

      string filter = string.Format("{0} = '{1}'",
                      materialSpeciesDS.MaterialSpecies.MaterialTypeColumn.ColumnName,
                      materialType);
      materialSpeciesBS.DataSource = materialSpeciesDS.MaterialSpecies;
      materialSpeciesBS.Filter = filter;

      Image img = null;
      if (lookUpSectionType.EditValue != null)
      {
        img = GetImage((SectionType)lookUpSectionType.EditValue, materialType);
      }

      imgSchema.Image = img;
    }

    private void imgImage_ImageChanged(object sender, EventArgs e)
    {
      if (imgImage.EditValue != null)
      {
        if (imgImage.Image.Width < imgImage.Width && imgImage.Image.Height < imgImage.Height)
        {
          imgImage.Properties.SizeMode = PictureSizeMode.Clip;
        }
        else
        {
          imgImage.Properties.SizeMode = PictureSizeMode.Zoom;
        }
      }
    }

    #region Parametrii H si W

    private void unitH123_Enter(object sender, EventArgs e)
    {
      BaseEdit editor = sender as BaseEdit;
      if (editor != null)
        editor.SelectAll();
    }


    /// <summary>
    /// Initial sumarizarea era realizata pe evenimentul de Leave, insa a fost modificat deoarece nu se salva valoarea sumarizata
    /// atunci cand se schimbau unitatile de masura
    /// </summary>
    private void unitH123_Validated(object sender, EventArgs e)
    {
      decimal h1 = Convert.ToDecimal(unitH1.EditValue);
      decimal h2 = Convert.ToDecimal(unitH2.EditValue);
      decimal h3 = Convert.ToDecimal(unitH3.EditValue);
      decimal h = Convert.ToDecimal(unitH.EditValue);
      if (h < h1 + h2 + h3)
      {
        unitH.EditValue = h1 + h2 + h3;
      }
    }

    private void unitW1_Enter(object sender, EventArgs e)
    {
      BaseEdit editor = sender as BaseEdit;
      if (editor != null)
        editor.SelectAll();
    }

    private void unitW1_Leave(object sender, EventArgs e)
    {
      decimal w1 = Convert.ToDecimal(unitW1.EditValue);
      decimal w = Convert.ToDecimal(unitW.EditValue);
      if (w < w1)
      {
        unitW.EditValue = w1;
      }
    }

    private void unitH_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      decimal h1 = Convert.ToDecimal(unitH1.EditValue);
      decimal h2 = Convert.ToDecimal(unitH2.EditValue);
      decimal h3 = Convert.ToDecimal(unitH3.EditValue);

      //Se seteaze focusul pe alt control apoi se revine la cel de summarizare
      //pt ca altfel nu se face calculul corect
      this.cmdOK.Focus();
      unitH.EditValue = h1 + h2 + h3;
      unitH.Focus();
      //Punem modified pe true doarece valoarea controlului nu se salveaza
      this.modified = true;
    }

    private void unitW_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      decimal w1 = Convert.ToDecimal(unitW1.EditValue);
      this.cmdOK.Focus();
      unitW.EditValue = w1;
      unitW.Focus();
      //Punem modified pe true doarece valoarea controlului nu se salveaza
      this.modified = true;
    }


    #endregion Parametrii H si W

    private void lookUpRawSectionType_EditValueChanging(object sender, ChangingEventArgs e)
    {
      if (e.NewValue == null)
      {
        return;
      }

      RawSectionType rawSectionType = (RawSectionType)e.NewValue;
      RawSectionType? oldRawSectionType = null;
      if (e.OldValue != null)
      {
        oldRawSectionType = (RawSectionType)e.OldValue;
      }

      if (oldRawSectionType.HasValue && oldRawSectionType == RawSectionType.Defined && oldRawSectionType != rawSectionType && chkUseRawSectionMaterialSpecies.Checked)
      {
        SectionDataSet.SectionRow rawSection = null;
        if (lookUpRawSection.EditValue != null)
          rawSection = sectionDS.Section.FindById((int)lookUpRawSection.EditValue);

        if (rawSection != null)
        {
          AddSectionMaterialSpeciesRows(rawSection);
        }
      }
    }

    private void lookUpRawSectionType_EditValueChanged(object sender, EventArgs e)
    {
      RawSectionType rawSectionType = (RawSectionType)lookUpRawSectionType.EditValue;
      if (rawSectionType == RawSectionType.Defined)
      {
        lookUpRawSection.Enabled = openingMode == FormOpeningMode.Normal && rawSectionType == RawSectionType.Defined;
        unitRawSectionTolerance.Enabled = openingMode == FormOpeningMode.Normal;
        unitDynamicRawSectionH.Enabled = false;
        unitDynamicRawSectionH.EditValue = 0;
        unitDynamicRawSectionW.Enabled = false;
        unitDynamicRawSectionW.EditValue = 0;
        chkUsePriceOnRawSection.Enabled = false;
        chkUsePriceOnRawSection.Checked = false;
        chkHasVariableW.Enabled = false;
        chkHasVariableW.Checked = false;
        unitMaxW.Enabled = false;
        unitWInputTolerance.Enabled = false;
        if (lookUpRawSection.EditValue == null)
        {
          unitRawSectionTolerance.EditValue = Properties.Settings.Default.SectionDefaultRawSectionTolerance;
        }
        chkUseRawSectionMaterialSpecies.Enabled = true;
        UpdateMaterialSpeciesRawSectionColumn(true, chkArcRawSection.Checked);
      }
      else if (rawSectionType == RawSectionType.Dynamic)
      {
        lookUpRawSection.Enabled = false;
        lookUpRawSection.EditValue = null;
        unitRawSectionTolerance.Enabled = openingMode == FormOpeningMode.Normal;
        unitDynamicRawSectionH.Enabled = openingMode == FormOpeningMode.Normal;
        unitDynamicRawSectionW.Enabled = openingMode == FormOpeningMode.Normal;
        chkUsePriceOnRawSection.Enabled = openingMode == FormOpeningMode.Normal;
        chkHasVariableW.Enabled = openingMode == FormOpeningMode.Normal;
        chkHasVariableW.Checked = false;
        unitMaxW.Enabled = false;
        unitWInputTolerance.Enabled = false;
        chkUseRawSectionMaterialSpecies.Checked = false;
        chkUseRawSectionMaterialSpecies.Enabled = false;
        UpdateMaterialSpeciesRawSectionColumn(false, chkArcRawSection.Checked);
        RoundValues(true);
        RoundValues(false);
      }
      else if (rawSectionType == RawSectionType.None)
      {
        UpdateMaterialSpeciesRawSectionColumn(false, chkArcRawSection.Checked);
        lookUpRawSection.EditValue = null;
        lookUpRawSection.Enabled = false;
        unitRawSectionTolerance.Enabled = false;
        unitDynamicRawSectionH.Enabled = false;
        unitDynamicRawSectionH.EditValue = 0;
        unitDynamicRawSectionW.Enabled = false;
        unitDynamicRawSectionW.EditValue = 0;
        chkUseRawSectionMaterialSpecies.Checked = false;
        chkUseRawSectionMaterialSpecies.Enabled = false;
        chkUsePriceOnRawSection.Enabled = false;
        chkUsePriceOnRawSection.Checked = false;
        chkHasVariableW.Enabled = openingMode == FormOpeningMode.Normal;
        chkHasVariableW.Checked = false;
        unitMaxW.Enabled = false;
        unitWInputTolerance.Enabled = false;
      }
    }

    private void chkArcRawSection_EditValueChanged(object sender, EventArgs e)
    {
      bool allowRawSection = (RawSectionType)lookUpRawSectionType.EditValue == RawSectionType.Defined;
      if (chkArcRawSection.Checked)
      {
        lookUpArcRawSection.Enabled = openingMode == FormOpeningMode.Normal;
        unitArcRawSectionTolerance.Enabled = openingMode == FormOpeningMode.Normal;

        if (lookUpArcRawSection.EditValue == null)
        {
          if (sectionRow.IsIdArcRawSectionNull())
          {
            using (DataView arcRawSectionView = new DataView(sectionDS.Section))
            {
              arcRawSectionView.RowFilter = string.Format("{0} = '{1}' AND {2} <> {3}",
                        sectionDS.Section.SectionTypeColumn.ColumnName,
                        SectionType.RawSection,
                        sectionDS.Section.IdColumn.ColumnName,
                        sectionRow.Id);
              arcRawSectionView.Sort = sectionDS.Section.CodeColumn.ColumnName;
              if (arcRawSectionView.Count > 0)
              {
                lookUpArcRawSection.EditValue = ((SectionDataSet.SectionRow)arcRawSectionView[0].Row).Id;
              }
            }
          }
          else
          {
            lookUpArcRawSection.EditValue = sectionRow.IdArcRawSection;
          }
          unitArcRawSectionTolerance.EditValue = Properties.Settings.Default.SectionDefaultRawSectionTolerance;
          UpdateMaterialSpeciesRawSectionColumn(allowRawSection, true);
        }
      }
      else
      {
        UpdateMaterialSpeciesRawSectionColumn(allowRawSection, false);
        lookUpArcRawSection.EditValue = null;
        lookUpArcRawSection.Enabled = false;
        unitArcRawSectionTolerance.Enabled = false;
      }
    }

    private void unitArea_EditValueChanged(object sender, EventArgs e)
    {
      TextEdit editor = sender as TextEdit;
      if (sectionRow.Area != (decimal)editor.EditValue)
        sectionRow.Area = (decimal)editor.EditValue;
    }

    private void unitPerimter_EditValueChanged(object sender, EventArgs e)
    {
      TextEdit editor = sender as TextEdit;
      if (sectionRow.Perimeter != (decimal)editor.EditValue)
        sectionRow.Perimeter = (decimal)editor.EditValue;
    }

    private void unitUnitWeight_EditValueChanged(object sender, EventArgs e)
    {
      TextEdit editor = sender as TextEdit;
      if (sectionRow.UnitWeight != (decimal)editor.EditValue)
        sectionRow.UnitWeight = (decimal)editor.EditValue;
    }

    private void lookUpPriceComputeMode_EditValueChanged(object sender, EventArgs e)
    {
      TextEdit editor = sender as TextEdit;
      //if (sectionRow.PriceCalculationType != editor.EditValue.ToString())
      //  sectionRow.PriceCalculationType = editor.EditValue.ToString();

      if ((PriceCalculationType)lookUpPriceComputeMode.EditValue == PriceCalculationType.PerLength)
        lblUnitBasePrice.Text = String.Format(Properties.Resources.LblUnitBasePriceFormat, MeasurableProperties.DefaultMeasurableProperties.Length.DisplayUnitShortName);
      else if ((PriceCalculationType)lookUpPriceComputeMode.EditValue == PriceCalculationType.PerWeight)
        lblUnitBasePrice.Text = String.Format(Properties.Resources.LblUnitBasePriceFormat, MeasurableProperties.DefaultMeasurableProperties.Mass.DisplayUnitShortName);
      else if ((PriceCalculationType)lookUpPriceComputeMode.EditValue == PriceCalculationType.PerVolume)
        lblUnitBasePrice.Text = String.Format(Properties.Resources.LblUnitBasePriceFormat, MeasurableProperties.DefaultMeasurableProperties.Volume.DisplayUnitShortName);
    }

    private void txtUnitWeightPrice_EditValueChanged(object sender, EventArgs e)
    {
      TextEdit editor = sender as TextEdit;
      if (sectionRow.DisplayUnitBasePrice != (decimal)editor.EditValue)
        sectionRow.DisplayUnitBasePrice = (decimal)editor.EditValue;
    }

    #endregion Section

    #region Series

    /// <summary>
    /// Seteaza noua inregistrarea la profilul deschid pentru editare
    /// </summary>
    private void viewSeries_InitNewRow(object sender, InitNewRowEventArgs e)
    {
      SectionDataSet.SeriesSectionRow row = viewSeries.GetDataRow(e.RowHandle) as SectionDataSet.SeriesSectionRow;
      row.IdSection = sectionRow.Id;

      decimal max = 1;
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection)
      {
        if (seriesSectionRow.RowState == DataRowState.Deleted)
          continue;

        if (!seriesSectionRow.IsPriorityNull() && seriesSectionRow.Priority > max)
          max = seriesSectionRow.Priority;
      }

      row.Priority = max;
    }

    private void viewSeries_KeyUp(object sender, KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        case Keys.Delete:
          {
            // se verifica daca alte profile depind pe seriile ce se vor sterge de profilul curent.
            if (!CheckViewSeriesForDelete())
            {
              return;
            }

            DeleteSeriesSection();
            //Se face acum sergerea automata a elementelor selectate in metoda DeleteSeriesSection
            //GridUtils.Delete(viewSeries);

            break;
          }
      }
    }

    /// <summary>
    /// Populeaza coloana Tipul Materialului care e nelegata la sursa de date
    /// </summary>
    private void viewSeries_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {
      int rowHandle = viewSeries.GetRowHandle(e.ListSourceRowIndex);

      if (viewSeries.FocusedRowHandle == rowHandle && viewSeries.FocusedColumn == colSeriesId)
      {
        LookUpEdit lookUp = viewSeries.ActiveEditor as LookUpEdit;
        if (lookUp != null && !lookUp.IsPopupOpen)
        {
          viewSeries.CloseEditor();
        }
      }
      SectionDataSet.SeriesSectionRow seriesSectionRow = viewSeries.GetDataRow(rowHandle) as SectionDataSet.SeriesSectionRow;
      if (seriesSectionRow == null)
      {
        return;
      }

      SeriesDataSet.SeriesRow seriesRow = null;
      if (seriesSectionRow[sectionDS.SeriesSection.IdSeriesColumn.ColumnName] != DBNull.Value)
      {
        seriesRow = seriesDS.Series.FindById(seriesSectionRow.IdSeries);
        //seriesSectionRow.IsSeriesActive = seriesDS.Series.FindById(seriesSectionRow.IdSeries).IsActive;
      }
      if (seriesRow == null)
      {
        return;
      }

      if (e.Column == colSeriesMaterialType)
      {
        e.Value = seriesRow.MaterialType;
      }
      else if (e.Column == colSeriesDesignation)
      {
        e.Value = seriesRow.Designation;
      }
    }

    private void ViewSeries_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
    {
      DataRowView seriesSectionRowView = e.Row as DataRowView;
      if (e.Row == null)
      {
        return;
      }

      SectionDataSet.SeriesSectionRow seriesSectionRow = seriesSectionRowView.Row as SectionDataSet.SeriesSectionRow;
      if (seriesSectionRow == null)
      {
        return;
      }

      SeriesDataSet.SeriesRow seriesRow = seriesDS.Series.FindById(seriesSectionRow.IdSeries);
      seriesSectionRow.IsSeriesActive = seriesRow.IsActive;
    }

    private void ViewSeries_RowStyle(object sender, RowStyleEventArgs e)
    {
      GridView view = sender as GridView;
      if (e.RowHandle < 0)
        return;

      SectionDataSet.SeriesSectionRow seriesSectionRow = viewSeries.GetDataRow(e.RowHandle) as SectionDataSet.SeriesSectionRow;
      if (seriesSectionRow == null)
      {
        return;
      }

      if (!seriesSectionRow.IsIsSeriesActiveNull() && !seriesSectionRow.IsSeriesActive)
      {
        e.Appearance.ForeColor = SystemColors.GrayText;
      }

      if (!seriesSectionRow.IsIsSeriesActiveNull() && !seriesSectionRow.IsSeriesActive && (view.IsRowSelected(e.RowHandle) || view.FocusedRowHandle == e.RowHandle))
      {
        e.Appearance.ForeColor = Color.LightSlateGray;
        e.Appearance.BackColor = view.Appearance.SelectedRow.BackColor;
        e.HighPriority = true;
      }
    }

    private void repLookUpSeriesIdSeries_EditValueChanged(object sender, EventArgs e)
    {
      //viewSeries.CloseEditor();
      LookUpEdit lookUp = sender as LookUpEdit;
      if (lookUp.EditValue != null && !lookUp.IsPopupOpen)
      {
        int idSeries = (int)lookUp.EditValue;

        viewSeries.SetRowCellValue(viewSeries.FocusedRowHandle, colSeriesDesignation, idSeries);
      }
    }

    #endregion Series

    #region Colors

    /// <summary>
    /// Cand se intra in tabul de Culori, se provoaca repopularea datelor din grid cu modificarile efectuate
    /// in tabul de Detalii
    /// </summary>
    private void tabControlSectionProperties_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
    {
      if (e.PrevPage == tabDetails)
      {
        if (sectionRow == null)
          return;

        decimal newUnitWeight = MeasurableProperties.DefaultMeasurableProperties.Length.ConvertInvDisplayToInternal(Convert.ToDecimal(unitUnitWeight.EditValue));
        if (sectionRow.UnitWeight != newUnitWeight)
          sectionRow.UnitWeight = newUnitWeight;

        PriceCalculationType newPriceCalcylationType = (PriceCalculationType)lookUpPriceComputeMode.EditValue;
        if (newPriceCalcylationType != sectionRow.ConvertedPriceCalculationType)
          sectionRow.ConvertedPriceCalculationType = newPriceCalcylationType;

        decimal newUnitBasePrice = Convert.ToDecimal(txtUnitWeightPrice.EditValue);
        if (sectionRow.UnitWeight != newUnitBasePrice)
          sectionRow.DisplayUnitBasePrice = newUnitBasePrice;
      }
      if (e.Page == tabColors)
      {
        colColorListItemPricePerLength.Caption = String.Format(Properties.Resources.LblUnitBasePriceFormat, MeasurableProperties.DefaultMeasurableProperties.Length.DisplayUnitShortName);
        colColorListItemPricePerLength.Visible = true;
        if ((PriceCalculationType)lookUpPriceComputeMode.EditValue == PriceCalculationType.PerLength)
        {
          colColorListItemPrice.Caption = String.Format(Properties.Resources.LblUnitBasePriceFormat, MeasurableProperties.DefaultMeasurableProperties.Length.DisplayUnitShortName);
          colColorListItemPricePerLength.Visible = false;
        }
        else if ((PriceCalculationType)lookUpPriceComputeMode.EditValue == PriceCalculationType.PerWeight)
          colColorListItemPrice.Caption = String.Format(Properties.Resources.LblUnitBasePriceFormat, MeasurableProperties.DefaultMeasurableProperties.Mass.DisplayUnitShortName);
        else if ((PriceCalculationType)lookUpPriceComputeMode.EditValue == PriceCalculationType.PerVolume)
          colColorListItemPrice.Caption = String.Format(Properties.Resources.LblUnitBasePriceFormat, MeasurableProperties.DefaultMeasurableProperties.Volume.DisplayUnitShortName);

        SetGridColorItemDataSource();
        viewSectionColorList.Focus();
      }
      else if (e.Page == tabMaterialSpecies)
      {
        colMaterialSpeciesPrice.Caption = String.Format(Properties.Resources.LblUnitBasePriceFormat, MeasurableProperties.DefaultMeasurableProperties.Length.DisplayUnitShortName);
      }
      else if (e.Page == tabComponents)
      {
        viewCustomSectionItem.Focus();
      }
      else if (e.Page == tabReinforcements)
      {
        SetDefaultReinforcementDataSource();
      }
      else if (e.Page == tabSeries)
      {
        viewSeries.Focus();
      }
    }

    private void viewColorListItem_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
    {
      if ((e.Column != colColorListItemPrice && e.Column != colColorListItemPricePerLength) || e.Value == null || e.Value == DBNull.Value)
        return;

      try
      {
        int rowHandle = viewColorListItem.GetRowHandle(e.ListSourceRowIndex);

        SectionDataSet.SectionColorListItemRow sectionColorListItemRow = viewColorListItem.GetDataRow(rowHandle) as SectionDataSet.SectionColorListItemRow;
        if (sectionColorListItemRow == null)
          return;
        e.DisplayText = currencyExchange.GetDisplayValueWithSymbol((decimal)e.Value, (string)lookUpCurrency.EditValue);
      }
      catch (Exception exception)
      {
        FrameworkApplication.TreatException(exception);
      }
    }

    private void viewSectionColorList_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {
      int rowHandle = viewSectionColorList.GetRowHandle(e.ListSourceRowIndex);

      SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(rowHandle) as SectionDataSet.SectionColorListRow;
      if(sectionColorListRow == null || sectionColorListRow[sectionDS.SectionColorList.IdColorListColumn.ColumnName] == DBNull.Value)
      {
        return;
      }

      ColorListDataSet.ColorListRow colorListRow = colorListDS.ColorList.FindById(sectionColorListRow.IdColorList);

      if(colorListRow == null)
      {
        return;
      }

      if (e.IsGetData)
      {
        if (e.Column == colPriceCalculationType)
        {
          if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None)
            e.Value = colorListRow.PriceCalculationType;
          else
            e.Value = sectionColorListRow.PriceCalculationType;
        }

        if (e.Column == colEditingMode)
        {
          e.Value = sectionColorListRow.EditingMode;
        }

        if (e.Column == colPrice)
        {
          if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None)
          {
            if (lookUpCurrency.EditValue != null && !colorListRow.IsCurrencyNull())
              e.Value = sectionColorListRow.SectionRow.HasThermalBreak ?
                          CurrencyExchange.CurrentCurrencyExchange.Exchange(colorListRow.DisplayUnitPriceTB, colorListRow.Currency, lookUpCurrency.EditValue.ToString()) :
                          CurrencyExchange.CurrentCurrencyExchange.Exchange(colorListRow.DisplayUnitPrice, colorListRow.Currency, lookUpCurrency.EditValue.ToString());
          }
          else if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListColorCombinations)
          {
            SectionDataSet.SectionColorListItemRow[] sectionColorListItemRows = sectionColorListRow.GetSectionColorListItemRows();
            foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListItemRows)
            {
              if (sectionColorListItemRows[0].Price != sectionColorListItemRow.Price)
              {
                e.Value = null;
                return;
              }
            }
            if (sectionColorListItemRows.Length > 0)
              e.Value = sectionColorListRow.DisplayPrice;
            else
              e.Value = chkHasThermalBreak.Checked ?
                colorListRow.DisplayUnitPriceTB :
                colorListRow.DisplayUnitPrice;
          }
          else
          {
            e.Value = sectionColorListRow.DisplayPrice;
          }
        }
        if (e.Column == colListIdCostGroup)
        {
          if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None)
            e.Value = colorListRow.IsIdCostGroupNull() ? (int?)null : colorListRow.IdCostGroup;

          else if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListColorCombinations)
          {
            SectionDataSet.SectionColorListItemRow[] sectionColorListItemRows = sectionColorListRow.GetSectionColorListItemRows();
            foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListItemRows)
            {
              if (sectionColorListItemRow.IsIdCostGroupNull() || sectionColorListItemRows[0].IdCostGroup != sectionColorListItemRow.IdCostGroup)
              {
                e.Value = null;
                return;
              }
            }
            e.Value = sectionColorListItemRows.Length > 0 ? sectionColorListItemRows[0].IdCostGroup : (int?)null;
          }
          else
          {
            if (sectionColorListRow.IsIdCostGroupNull())
            {
              if (!colorListRow.IsIdCostGroupNull())
              {
                e.Value = colorListRow.IdCostGroup;
              }
              else
              {
                e.Value = DBNull.Value;
              }
            }
            else
            {
              e.Value = sectionColorListRow.IdCostGroup;
            }
            // e.Value = sectionColorListRow.IsIdCostGroupNull() ? colorListRow.IdCostGroup : sectionColorListRow.IdCostGroup;
          }
        }

        if (e.Column == colListIdReinforcement)
        {
          if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None)
          {
            e.Value = null;

            //Am comentat secv de cod pt a nu afisa nimic in coloana LungimeBara si Armatura - 5/06/2015(Bianca) -> Culori si preturi - neafisare lungime bara si cod armatura
            //if (sectionColorListRow.IsIdReinforcementNull())
            //  e.Value = null;
            //else
            //  if (gridLookUpDefaultReinforcement.EditValue != null)
            //    e.Value = sectionRow.IdReinforcement;
          }
          //else if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListColorCombinations)
          //{
          //  SectionDataSet.SectionColorListItemRow[] sectionColorListItemRows = sectionColorListRow.GetSectionColorListItemRows();
          //  foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListItemRows)
          //  {
          //    if (sectionColorListItemRows[0].IsIdReinforcementNull() ||
          //        sectionColorListItemRow.IsIdReinforcementNull() || sectionColorListItemRows[0].IdReinforcement != sectionColorListItemRow.IdReinforcement)
          //    {
          //      e.Value = null;
          //      return;

          //    }
          //  }
          //  e.Value = sectionColorListItemRows.Length > 0 ? sectionColorListItemRows[0].IdReinforcement : (int?)null;
          //}
          else
          {
            e.Value = sectionColorListRow.IsIdReinforcementNull() ? (decimal?)null : sectionColorListRow.IdReinforcement;
          }
        }

        if (e.Column == colListBarLength)
        {
          if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None)
          {
            //Am comentat secv de cod pt a nu afisa nimic in coloana LungimeBara si Armatura - 5/06/2015(Bianca) -> Culori si preturi - neafisare lungime bara si cod armatura

            //if (sectionColorListRow.IsBarLengthNull())
            //  e.Value = null;
            //else
            //  if (unitBarLength.EditValue != null)
            //    e.Value = sectionRow.BarLength;
            e.Value = null;
          }
          //Am comentat secv de cod pt a nu afisa nimic in coloana LungimeBara si Armatura - 5/06/2015(Bianca) -> Culori si preturi - neafisare lungime bara si cod armatura

          //else if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListColorCombinations)
          //{
          //  SectionDataSet.SectionColorListItemRow[] sectionColorListItemRows = sectionColorListRow.GetSectionColorListItemRows();
          //  foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListItemRows)
          //  {
          //    if (sectionColorListItemRows[0].IsBarLengthNull() ||
          //        sectionColorListItemRow.IsBarLengthNull() || sectionColorListItemRows[0].BarLength != sectionColorListItemRow.BarLength)
          //    {
          //      e.Value = null;
          //      return;
          //    }
          //  }
          //  e.Value = sectionColorListItemRows.Length > 0 ? sectionColorListItemRows[0].BarLength : (decimal?)null;
          //}
          else
          {
            e.Value = sectionColorListRow.IsBarLengthNull() ? (decimal?)null : sectionColorListRow.BarLength;
          }
        }
      }

      if (e.IsSetData)
      {
        if (e.Column == colPriceCalculationType)
        {
          sectionColorListRow.PriceCalculationType = e.Value.ToString();
        }
        if (e.Column == colPrice)
        {
          sectionColorListRow.DisplayPrice = (decimal)e.Value;
        }
        if (e.Column == colEditingMode)
        {
          sectionColorListRow.EditingMode = e.Value.ToString();
        }
        if (e.Column == colListIdCostGroup)
        {
          if (e.Value != null && e.Value.ToString() != string.Empty)
            sectionColorListRow.IdCostGroup = (int)e.Value;
          else
            sectionColorListRow.SetIdCostGroupNull();
        }
        if (e.Column == colListIdReinforcement)
        {
          if (e.Value != null && e.Value.ToString() != string.Empty)
            sectionColorListRow.IdReinforcement = (int)e.Value;
          else
            sectionColorListRow.SetIdReinforcementNull();
        }
        if (e.Column == colListBarLength)
        {
          if (e.Value != null && e.Value.ToString() != string.Empty)
            sectionColorListRow.BarLength = (decimal)e.Value;
          else
            sectionColorListRow.SetBarLengthNull();
        }
      }

      viewSectionColorList.InvalidateRow(rowHandle);
    }

    private void viewSectionColorList_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
    {
      SectionDataSet.SectionColorListRow sectionColorListRow = e.Row as SectionDataSet.SectionColorListRow;
      if (sectionColorListRow == null)
      {
        DataRowView sectionColorListRowView = e.Row as DataRowView;
        sectionColorListRow = sectionColorListRowView.Row as SectionDataSet.SectionColorListRow;
      }

      if (sectionColorListRow != null && viewSectionColorList.IsNewItemRow(e.RowHandle) &&
        sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListColorCombinations)
      {
        foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in GetSectionColorListItemRows(sectionColorListRow))
        {
          SectionDataSet.SectionColorListItemRow newSectionColorListItemRow = CopySectionColorListItemRow(sectionColorListItemRow, sectionDS.SectionColorListItem);
          newSectionColorListItemRow.GenerateUniqueCode();
          newSectionColorListItemRow.Price = sectionColorListRow.Price;
          sectionDS.SectionColorListItem.AddSectionColorListItemRow(newSectionColorListItemRow);
        }
      }
    }

    private void ViewSectionColorList_RowStyle(object sender, RowStyleEventArgs e)
    {
      GridView view = sender as GridView;
      if (e.RowHandle < 0)
        return;

      SectionDataSet.SectionColorListRow currentRow = viewSectionColorList.GetDataRow(e.RowHandle) as SectionDataSet.SectionColorListRow;
      if (currentRow == null)
      {
        return;
      }

      ColorListDataSet.ColorListRow currentColorListRow = colorListDS.ColorList.SingleOrDefault(colorListRow => colorListRow.Id == currentRow.IdColorList);
      if (currentColorListRow == null)
      {
        return;
      }

      if (!currentColorListRow.IsActive)
      {
        e.Appearance.ForeColor = SystemColors.GrayText;
      }

      if (!currentColorListRow.IsActive && (view.IsRowSelected(e.RowHandle) || view.FocusedRowHandle == e.RowHandle))
      {
        e.Appearance.ForeColor = Color.LightSlateGray;
        e.Appearance.BackColor = view.Appearance.SelectedRow.BackColor;
        e.HighPriority = true;
      }
    }

    private void repLookUpColorListId_EditValueChanged(object sender, EventArgs e)
    {
      viewSectionColorList.InvalidateRow(viewSectionColorList.FocusedRowHandle);
    }

    private void lookUpReinforcement_EditValueChanged(object sender, EventArgs e)
    {
      TextEdit editor = sender as TextEdit;
      if (editor.EditValue != null)
      {
        if (sectionRow.IdReinforcement != (int)editor.EditValue)
          sectionRow.IdReinforcement = (int)editor.EditValue;
      }
      else
      {
        if (!sectionRow.IsIdReinforcementNull())
          sectionRow.SetIdReinforcementNull();
      }

      SetGridMaterialSpeciesDataSource();
    }

    private void unitBarLength_EditValueChanged(object sender, EventArgs e)
    {
      TextEdit editor = sender as TextEdit;
      if (sectionRow.BarLength != (decimal)editor.EditValue)
        sectionRow.BarLength = (decimal)editor.EditValue;
    }

    private void viewSectionColorList_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
    {
      if (e.Column != colPrice || e.Value == null || e.Value == DBNull.Value)
        return;

      try
      {
        int rowHandle = viewSectionColorList.GetRowHandle(e.ListSourceRowIndex);

        SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(rowHandle) as SectionDataSet.SectionColorListRow;
        if (sectionColorListRow == null)
          return;
        e.DisplayText = currencyExchange.GetDisplayValueWithSymbol((decimal)e.Value, (string)lookUpCurrency.EditValue);
      }
      catch (Exception exception)
      {
        FrameworkApplication.TreatException(exception);
      }
    }

    private void viewSectionColorList_InitNewRow(object sender, InitNewRowEventArgs e)
    {
      GridView senderView = (GridView)sender;
      SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(e.RowHandle) as SectionDataSet.SectionColorListRow;
      ColorListDataSet.ColorListRow colorListRow = colorListDS.ColorList.FindById((int)senderView.EditingValue);
      sectionColorListRow.IdColorList = colorListRow.Id;
      sectionColorListRow.IdSection = sectionRow.Id;
      sectionColorListRow.ConvertedEditingMode = ColorListEditingMode.None;
      sectionColorListRow.Price = colorListRow.IsCurrencyNull() ? colorListRow.Price :
                          CurrencyExchange.CurrentCurrencyExchange.Exchange(colorListRow.Price, colorListRow.Currency, lookUpCurrency.EditValue.ToString());
      sectionColorListRow.PriceCalculationType = colorListRow.PriceCalculationType;
      if (!colorListRow.IsIdCostGroupNull())
      {
        sectionColorListRow.IdCostGroup = colorListRow.IdCostGroup;
      }
      else
      {
        sectionColorListRow.SetIdCostGroupNull();
      }
      // if (!sectionRow.IsIdReinforcementNull())
      //  sectionColorListRow.IdReinforcement = sectionRow.IdReinforcement;

      sectionColorListRow.SetIdReinforcementNull();
      sectionColorListRow.SetBarLengthNull();
    }

    private void repLookUpPriceCalculationType_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      if (e.Button.Kind == ButtonPredefines.Delete)
      {
        SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(viewSectionColorList.FocusedRowHandle) as SectionDataSet.SectionColorListRow;
        BaseEdit editor = sender as BaseEdit;
        ColorListDataSet.ColorListRow colorListRow = colorListDS.ColorList.SingleOrDefault(row => row.Id == sectionColorListRow.IdColorList);
        editor.EditValue = colorListRow.PriceCalculationType;
      }
    }

    private void viewColorListItem_InitNewRow(object sender, InitNewRowEventArgs e)
    {
      SectionDataSet.SectionColorListItemRow sectionColorListItemRow = viewColorListItem.GetDataRow(e.RowHandle) as SectionDataSet.SectionColorListItemRow;
      SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(viewSectionColorList.FocusedRowHandle) as SectionDataSet.SectionColorListRow;
      ColorListDataSet.ColorListRow colorListRow = colorListDS.ColorList.SingleOrDefault(row => row.Id == sectionColorListRow.IdColorList);

      sectionColorListItemRow.IdColorCombination = (int)viewColorListItem.EditingValue;
      sectionColorListItemRow.Price = 0;
      sectionColorListItemRow.IdSectionColorList = sectionColorListRow.Id;
      sectionColorListItemRow.Code = sectionColorListRow.SectionRow.CreateCodeWithColorCombination(colorDS.ColorCombination.SingleOrDefault(row => row.Id == sectionColorListItemRow.IdColorCombination), colorListDS);
      sectionColorListItemRow.GenerateUniqueCode();

      if (!string.IsNullOrEmpty(viewSectionColorList.GetRowCellDisplayText(viewSectionColorList.FocusedRowHandle, colListIdCostGroup)))
      {
        sectionColorListItemRow.IdCostGroup = Convert.ToInt32(viewSectionColorList.GetRowCellValue(viewSectionColorList.FocusedRowHandle, colListIdCostGroup));
      }
      else if (!colorListRow.IsIdCostGroupNull())
      {
        ColorListDataSet.ColorListItemRow colorListItemRow = colorListRow.GetColorListItemRows().SingleOrDefault(row => row.IdColorCombination == sectionColorListItemRow.IdColorCombination);
        if (colorListItemRow != null)
        {
          int? idCostGroup = colorListItemRow.GetIdCostGroup();
          if (idCostGroup != null)
          {
            sectionColorListItemRow.IdCostGroup = idCostGroup.Value;
          }
          else
          {
            sectionColorListItemRow.SetIdCostGroupNull();
          }
        }
        else
        {
          sectionColorListItemRow.IdCostGroup = costDS.CostGroup[0].Id;
        }
      }
      else
      {
        sectionColorListItemRow.IdCostGroup = costDS.CostGroup[0].Id;
      }

      //Am comentat secv de cod pt a nu afisa nimic in coloana LungimeBara si Armatura - 5/06/2015(Bianca) -> Culori si preturi - neafisare lungime bara si cod armatura

      //if (!string.IsNullOrEmpty(viewSectionColorList.GetRowCellDisplayText(viewSectionColorList.FocusedRowHandle, colListIdReinforcement)))
      //  sectionColorListItemRow.IdReinforcement = Convert.ToInt32(viewSectionColorList.GetRowCellValue(viewSectionColorList.FocusedRowHandle, colListIdReinforcement));
      //else
      sectionColorListItemRow.SetIdReinforcementNull();

      //if (!string.IsNullOrEmpty(viewSectionColorList.GetRowCellDisplayText(viewSectionColorList.FocusedRowHandle, colListBarLength)))
      //  sectionColorListItemRow.BarLength = Convert.ToDecimal(viewSectionColorList.GetRowCellValue(viewSectionColorList.FocusedRowHandle, colListBarLength));
      //else
      sectionColorListItemRow.SetBarLengthNull();
    }

    private void viewSectionColorList_ShowingEditor(object sender, CancelEventArgs e)
    {
      SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(viewSectionColorList.FocusedRowHandle) as SectionDataSet.SectionColorListRow;

      if (sectionColorListRow == null)
        return;

      if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None
           && (viewSectionColorList.FocusedColumn == colPrice ||
              viewSectionColorList.FocusedColumn == colPriceCalculationType ||
              viewSectionColorList.FocusedColumn == colListIdCostGroup ||
              viewSectionColorList.FocusedColumn == colListIdReinforcement ||
              viewSectionColorList.FocusedColumn == colListBarLength ||
              viewSectionColorList.FocusedColumn == colListIdCostGroup))
        e.Cancel = true;
    }

    private void viewColorListItem_ShowingEditor(object sender, CancelEventArgs e)
    {
      SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(viewSectionColorList.FocusedRowHandle) as SectionDataSet.SectionColorListRow;
      if (viewColorListItem.IsNewItemRow(viewColorListItem.FocusedRowHandle) &&
            viewColorListItem.FocusedColumn.Name != colColorCombinationCode.Name)
        e.Cancel = true;
      if (sectionColorListRow != null &&
          (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None ||
            sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListPrice))
        e.Cancel = true;
    }

    private void repLookUpColorCombinationCode_EditValueChanged(object sender, EventArgs e)
    {
      LookUpEdit lookUp = sender as LookUpEdit;

      if (lookUp.EditValue != null && !lookUp.IsPopupOpen)
      {
        int idColorCombination = (int)lookUp.EditValue;
        viewColorListItem.SetRowCellValue(viewColorListItem.FocusedRowHandle, colColorCombinationDesignation, idColorCombination);
        SectionDataSet.SectionColorListItemRow sectionColorlistItemRow = viewColorListItem.GetDataRow(viewColorListItem.FocusedRowHandle) as SectionDataSet.SectionColorListItemRow;
        sectionColorlistItemRow.Code = sectionColorlistItemRow.SectionColorListRow.SectionRow.CreateCodeWithColorCombination(colorDS.ColorCombination.SingleOrDefault(row => row.Id == sectionColorlistItemRow.IdColorCombination), colorListDS);
        sectionColorlistItemRow.GenerateUniqueCode();
      }
    }

    private void viewSectionColorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      SetGridColorItemDataSource();
    }

    private void viewSectionColorList_RowCountChanged(object sender, EventArgs e)
    {
      viewSectionColorList.SelectRow(viewSectionColorList.FocusedRowHandle);
      SetGridColorItemDataSource();
    }

    private void viewColorListItem_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
    {
      viewColorListItem.Focus();
    }

    private void viewSectionColorList_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
    {
      viewSectionColorList.Focus();
    }

    private void viewColorListItem_KeyUp(object sender, KeyEventArgs e)
    {
      if (viewSectionColorList.GetSelectedRows().Length == 1)
      {
        SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetFocusedDataRow() as SectionDataSet.SectionColorListRow;
        if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListColorCombinations)
          switch (e.KeyCode)
          {
            case Keys.Delete:
              {
                GridUtils.Delete(viewColorListItem);
                break;
              }
          }
      }
    }

    private void viewSectionColorList_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
    {
      SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetFocusedDataRow() as SectionDataSet.SectionColorListRow;
      ColorListDataSet.ColorListRow colorListRow = colorListDS.ColorList.FindById(sectionColorListRow.IdColorList);
      if (sectionColorListRow == null)
        return;

      if (e.Column == colColorListId)
      {
        if (viewSectionColorList.GetFocusedRowCellValue(colEditingMode).ToString() == ColorListEditingMode.EditColorListColorCombinations.ToString())
        {
          if (sectionColorListRow.GetSectionColorListItemRows().Length == 0 ||
                XtraMessageBox.Show(this,
                Resources.MsgDeleteColorListItems,
                Resources.CaptionAttentionMsgBox,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.No)
          {
            foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListRow.GetSectionColorListItemRows())
              sectionColorListItemRow.Delete();

            if (!viewSectionColorList.IsNewItemRow(viewSectionColorList.FocusedRowHandle))
              foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in GetSectionColorListItemRows(sectionColorListRow))
                sectionDS.SectionColorListItem.AddSectionColorListItemRow(CopySectionColorListItemRow(sectionColorListItemRow, sectionDS.SectionColorListItem));
          }
        }

        if (!viewSectionColorList.IsNewItemRow(viewSectionColorList.FocusedRowHandle))
          SetGridColorItemDataSource();
      }

      if (e.Column == colPrice)
      {
        foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListRow.GetSectionColorListItemRows())
          sectionColorListItemRow.Price = sectionColorListRow.Price;
        SetGridColorItemDataSource();
      }

      //Am comentat secv de cod pt a nu afisa nimic in coloana LungimeBara si Armatura - 5/06/2015(Bianca) -> Culori si preturi - neafisare lungime bara si cod armatura
      if (e.Column == colListBarLength)
      {
        //foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListRow.GetSectionColorListItemRows())
        //{
        //  if (sectionColorListRow.IsBarLengthNull())
        //    sectionColorListItemRow.SetBarLengthNull();
        //  else
        //    sectionColorListItemRow.BarLength = sectionColorListRow.BarLength;
        //}
        //SetGridColorItemDataSource();
      }
      if (e.Column == colListIdReinforcement)
      {
        //foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListRow.GetSectionColorListItemRows())
        //  if (sectionColorListRow.IsIdReinforcementNull())
        //    sectionColorListItemRow.SetIdReinforcementNull();
        //  else
        //    sectionColorListItemRow.IdReinforcement = sectionColorListRow.IdReinforcement;
        //SetGridColorItemDataSource();
      }

      if (e.Column == colListIdCostGroup)
      {
        foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListRow.GetSectionColorListItemRows())
        {
          if (sectionColorListRow.IsIdCostGroupNull())
            sectionColorListItemRow.SetIdCostGroupNull();
          else
            sectionColorListItemRow.IdCostGroup = sectionColorListRow.IdCostGroup;
        }
        SetGridColorItemDataSource();
      }

      if (e.Column == colEditingMode)
      {
        SetGridColorItemDataSource();
        viewSectionColorList.InvalidateRow(viewSectionColorList.FocusedRowHandle);
      }
    }

    private void repLookUpCostGroupId_EditValueChanged(object sender, EventArgs e)
    {
      TextEdit textEdit = sender as TextEdit;
      SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetFocusedDataRow() as SectionDataSet.SectionColorListRow;

      foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListRow.GetSectionColorListItemRows())
      {
        if (textEdit.EditValue == null)
        {
          sectionColorListItemRow.SetIdCostGroupNull();
        }
        else
        {
          sectionColorListItemRow.IdCostGroup = (int)textEdit.EditValue;
        }
      }
    }

    private void viewColorListItem_ValidateRow(object sender, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)
    {
      // Verific sa nu existe combinatia de culori deja adaugata pe profil.
      GridView senderView = sender as GridView;
      SectionDataSet.SectionColorListItemRow sectionColorListItemRow = senderView.GetDataRow(e.RowHandle) as SectionDataSet.SectionColorListItemRow;
      if (!CanUseColorCombination(sectionColorListItemRow))
      {
        e.ErrorText = Properties.Resources.MsgErrorSectionColorCombinationUsed;
        e.Valid = false;
      }

      viewSectionColorList.RefreshData();
    }

    private void viewSectionColorList_ValidateRow(object sender, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)
    {
      // Verific sa nu existe o combinatie de culori din  lista de culori deja adaugata pe profil.
      SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(e.RowHandle) as SectionDataSet.SectionColorListRow;
      if (sectionColorListRow == null || sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListColorCombinations)
        return;

      //if (!colorListDS.ColorList.GeneratesDuplicateSectionCodes(sectionColorListRow))
      //{
      //  e.ErrorText = Resources.MsgInvalidSectionEditColorListItemCode;
      //  e.Valid = false;
      //}

      // Daca am un rand in view si randul editat nu este o inregistrare noua, sau daca nu am niciun rand in view
      // si adaug un rand nou => in tabela va fi o singura inregistrare, deci nu se mai face verificare de unicitate.
      if (viewSectionColorList.DataRowCount == 1 && !viewSectionColorList.IsNewItemRow(e.RowHandle) ||
          (viewSectionColorList.DataRowCount == 0 && viewSectionColorList.IsNewItemRow(e.RowHandle)))
        return;

      if (!CanUseColorList(sectionColorListRow))
      {
        e.ErrorText = Properties.Resources.MsgErrorSeriesColorCombinationUsed;
        e.Valid = false;
      }


    }

    private void viewColorListItem_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {
      int rowHandle = viewColorListItem.GetRowHandle(e.ListSourceRowIndex);

      SectionDataSet.SectionColorListItemRow sectionColorListItemRow = viewColorListItem.GetDataRow(rowHandle) as SectionDataSet.SectionColorListItemRow;
      if (sectionColorListItemRow == null)
      {
        return;
      }
      if (e.IsGetData)
      {
        if (e.Column == colColorListItemPrice)
        {
          int[] selectedColorListHandles = viewSectionColorList.GetSelectedRows();
          foreach (int selectedColorListHandle in selectedColorListHandles)
          {
            SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(selectedColorListHandle) as SectionDataSet.SectionColorListRow;
            if (sectionColorListRow != null && sectionColorListItemRow.IdSectionColorList == sectionColorListRow.Id)
            {
              if (sectionColorListRow.ConvertedEditingMode != ColorListEditingMode.EditColorListColorCombinations)
              {
                e.Value = sectionColorListItemRow.GetDisplayPrice(sectionRow.ConvertedPriceCalculationType);
                break;
              }
              else
              {
                e.Value = sectionColorListItemRow.DisplayPrice;
                break;
              }
            }
          }
        }
        if (e.Column == colColorListItemPricePerLength)
        {

          if (colColorListItemPricePerLength.Visible)
          {
            int[] selectedColorListHandles = viewSectionColorList.GetSelectedRows();
            foreach (int selectedColorListHandle in selectedColorListHandles)
            {
              SectionDataSet.SectionColorListRow sectionColorListRow =
                viewSectionColorList.GetDataRow(selectedColorListHandle) as SectionDataSet.SectionColorListRow;
              if (sectionColorListRow != null &&
                  sectionColorListItemRow.IdSectionColorList == sectionColorListRow.Id)
              {
                ColorListDataSet.ColorListRow colorListRow =
                  colorListDS.ColorList.FindById(sectionColorListRow.IdColorList);
                if (colorListRow != null)
                {
                  e.Value =
                    MeasurableProperties.DefaultMeasurableProperties.Length.ConvertInvInternalToDisplay(
                      sectionRow.GetUnitPrice(
                    colorDS.ColorCombination.FindById(sectionColorListItemRow.IdColorCombination), colorListRow, null, null, null));

                }
              }
            }
          }
        }
      }
      if (e.IsSetData)
      {
        if (e.Column == colColorListItemPrice)
        {
          if (sectionColorListItemRow.SectionColorListRow != null)
            sectionColorListItemRow.DisplayPrice = Convert.ToDecimal(e.Value);
          else
            sectionColorListItemRow.Price = Convert.ToDecimal(e.Value);
        }
      }
    }

    private void ViewColorListItem_RowStyle(object sender, RowStyleEventArgs e)
    {
      GridView view = sender as GridView;
      if (e.RowHandle < 0)
        return;

      SectionDataSet.SectionColorListItemRow currentRow = viewColorListItem.GetDataRow(e.RowHandle) as SectionDataSet.SectionColorListItemRow;
      if (currentRow == null)
      {
        return;
      }

      ColorDataSet.ColorCombinationRow currentColorCombinationRow = colorDS.ColorCombination.SingleOrDefault(colorCombinationRow => colorCombinationRow.Id == currentRow.IdColorCombination);
      if (currentColorCombinationRow == null)
      {
        return;
      }

      if (!currentColorCombinationRow.IsActive)
      {
        e.Appearance.ForeColor = SystemColors.GrayText;
      }

      if (!currentColorCombinationRow.IsActive && (view.IsRowSelected(e.RowHandle) || view.FocusedRowHandle == e.RowHandle))
      {
        e.Appearance.ForeColor = Color.LightSlateGray;
        e.Appearance.BackColor = view.Appearance.SelectedRow.BackColor;
        e.HighPriority = true;
      }
    }

    private void viewSectionColorList_HiddenEditor(object sender, EventArgs e)
    {
      if (sender == viewSectionColorList)
        barManager.SetPopupContextMenu(gridSectionColorList, popupMenu);
      else if (sender == viewColorListItem)
        barManager.SetPopupContextMenu(gridColorListItem, popupMenu);
    }

    private void viewSectionColorList_ShownEditor(object sender, EventArgs e)
    {
      if (sender == viewSectionColorList)
        barManager.SetPopupContextMenu(gridSectionColorList, null);
      else if (sender == viewColorListItem)
        barManager.SetPopupContextMenu(gridColorListItem, null);
    }

    #endregion Colors

    #region MaterialSpecies
    private void ViewMaterialSpecies_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {

      int rowHandle = viewMaterialSpecies.GetRowHandle(e.ListSourceRowIndex);

      if (viewMaterialSpecies.IsNewItemRow(rowHandle))
        return;

      SectionDataSet.SectionMaterialSpeciesRow sectionMaterialSpeciesRow = viewMaterialSpecies.GetDataRow(rowHandle) as SectionDataSet.SectionMaterialSpeciesRow;
      if (sectionMaterialSpeciesRow == null)
      {
        return;
      }

      if (e.IsGetData && !viewMaterialSpecies.IsNewItemRow(rowHandle))
      {
        if (e.Column == colMaterialSpeciesPrice)
        {
          if (chkUseMaterialSpeciesDefinition.Checked)
          {
            MaterialSpeciesDataSet.MaterialSpeciesRow materialSpeciesRow = materialSpeciesDS.MaterialSpecies.FindById(sectionMaterialSpeciesRow.IdMaterialSpecies);
            if (materialSpeciesRow != null)
            {
              decimal price = materialSpeciesRow.PricePerVolume;
              if (chkUsePriceOnRawSection.Checked)
              {
                price = price * MeasurableProperties.DefaultMeasurableProperties.Surface.ConvertFrom(Convert.ToDecimal(unitDynamicRawSectionH.EditValue) * Convert.ToDecimal(unitDynamicRawSectionW.EditValue), SurfaceDivisions.mm2);
              }
              else
              {
                price = price * Convert.ToDecimal(unitArea.EditValue);
              }

              e.Value = MeasurableProperties.DefaultMeasurableProperties.Length.ConvertInvInternalToDisplay(price);
            }
            else
            {
              e.Value = null;
            }
          }
          else
          {
            if (!sectionMaterialSpeciesRow.IsPriceNull())
            {
              e.Value = MeasurableProperties.DefaultMeasurableProperties.Length.ConvertInvInternalToDisplay(sectionMaterialSpeciesRow.Price);
            }
            else
            {
              e.Value = null;
            }
          }
        }
        if (e.Column == colMaterialSpeciesUsage)
        {
          e.Value = sectionMaterialSpeciesRow.MaterialSpeciesUsage;
        }
        if (e.Column == colMaterialSpeciesUnitWeight)
        {
          if (chkUseMaterialSpeciesDefinition.Checked)
          {
            MaterialSpeciesDataSet.MaterialSpeciesRow materialSpeciesRow = materialSpeciesDS.MaterialSpecies.FindById(sectionMaterialSpeciesRow.IdMaterialSpecies);
            if (materialSpeciesRow != null)
            {
              decimal mass = materialSpeciesRow.WeightPerVolume;
              if (chkUsePriceOnRawSection.Checked)
              {
                mass = mass * MeasurableProperties.DefaultMeasurableProperties.Surface.ConvertFrom(Convert.ToDecimal(unitDynamicRawSectionH.EditValue) * Convert.ToDecimal(unitDynamicRawSectionW.EditValue), SurfaceDivisions.mm2);
              }
              else
              {
                mass = mass * Convert.ToDecimal(unitArea.EditValue);
              }

              e.Value = MeasurableProperties.DefaultMeasurableProperties.Length.ConvertInvInternalToDisplay(mass);
            }
            else
            {
              e.Value = null;
            }
          }
          else
          {
            if (!sectionMaterialSpeciesRow.IsUnitWeightNull())
            {
              e.Value = MeasurableProperties.DefaultMeasurableProperties.Length.ConvertInvInternalToDisplay(sectionMaterialSpeciesRow.UnitWeight);
            }
            else
            {
              e.Value = null;
            }
          }
        }
      }
      if (e.IsSetData)
      {
        if (e.Column == colMaterialSpeciesPrice)
        {
          if (e.Value == null || e.Value == DBNull.Value)
          {
            sectionMaterialSpeciesRow.SetPriceNull();
          }
          else
          {
            sectionMaterialSpeciesRow.Price = MeasurableProperties.DefaultMeasurableProperties.Length.ConvertInvDisplayToInternal(Convert.ToDecimal(e.Value));
          }
        }
        if (e.Column == colMaterialSpeciesUnitWeight)
        {
          if (e.Value == null || e.Value == DBNull.Value)
          {
            sectionMaterialSpeciesRow.SetUnitWeightNull();
          }
          else
          {
            sectionMaterialSpeciesRow.UnitWeight = MeasurableProperties.DefaultMeasurableProperties.Length.ConvertInvDisplayToInternal(Convert.ToDecimal(e.Value));
          }
        }
        if (e.Column == colMaterialSpeciesUsage)
        {
          sectionMaterialSpeciesRow.MaterialSpeciesUsage = e.Value as String;
        }
      }
    }

    private void viewMaterialSpecies_InitNewRow(object sender, InitNewRowEventArgs e)
    {
      SectionDataSet.SectionMaterialSpeciesRow row = viewMaterialSpecies.GetDataRow(e.RowHandle) as SectionDataSet.SectionMaterialSpeciesRow;
      row.IdSection = sectionRow.Id;
      row.Price = 0;
      if (chkUseMaterialSpeciesDefinition.Checked)
      {
        MaterialSpeciesDataSet.MaterialSpeciesRow materialSpeciesRow = materialSpeciesDS.MaterialSpecies.FindById(row.IdMaterialSpecies);
        if (materialSpeciesRow != null)
        {
          row.Price = materialSpeciesRow.PricePerVolume;
          row.DefaultPrice = materialSpeciesRow.DefaultPricePerVolume;
          row.UnitWeight = materialSpeciesRow.WeightPerVolume;
        }
      }
      else
      {
        row.Price = 0;
        row.DefaultPrice = 0;
      }
      row.MaterialSpeciesUsage = sectionRow.MaterialSpeciesUsage;
    }

    private void SetDefinedMaterialSpeciesValues()
    {
      foreach (SectionDataSet.SectionMaterialSpeciesRow sectionSpeciesRow in sectionDS.SectionMaterialSpecies)
      {
        if (sectionSpeciesRow.RowState == DataRowState.Deleted)
        {
          continue;
        }

        if (sectionSpeciesRow.IdSection != sectionRow.Id)
        {
          continue;
        }

        sectionSpeciesRow.SetPriceNull();
        sectionSpeciesRow.SetDefaultPriceNull();
        sectionSpeciesRow.SetUnitWeightNull();
      }
    }

    private void viewMaterialSpecies_KeyUp(object sender, KeyEventArgs e)
    {
      if (chkUseRawSectionMaterialSpecies.Checked)
        return;

      switch (e.KeyCode)
      {
        case Keys.Delete:
          {
            GridUtils.Delete(viewMaterialSpecies);
            break;
          }
      }
    }

    private void chkUseRawSectionMaterialSpecies_EditValueChanging(object sender, ChangingEventArgs e)
    {
      if ((bool)e.NewValue && sectionRow.GetSectionMaterialSpeciesRows().Length != 0 &&
          XtraMessageBox.Show(this,
              Resources.MsgQuestionUseRawSectionMaterialSpecies,
              Resources.CaptionQuestionMsgBox,
              MessageBoxButtons.OKCancel,
              MessageBoxIcon.Question) == DialogResult.Cancel)
      {
        e.Cancel = true;
      }
    }

    private void chkUseRawSectionMaterialSpecies_EditValueChanged(object sender, EventArgs e)
    {
      SectionDataSet.SectionRow rawSection = null;
      if (lookUpRawSection.EditValue != null)
        rawSection = sectionDS.Section.FindById((int)lookUpRawSection.EditValue);

      if (chkUseRawSectionMaterialSpecies.Checked)
      {
        foreach (SectionDataSet.SectionMaterialSpeciesRow sectionMaterialSpeciesRow in sectionRow.GetSectionMaterialSpeciesRows())
          sectionMaterialSpeciesRow.Delete();
      }
      else if ((RawSectionType)lookUpRawSectionType.EditValue == RawSectionType.Defined &&
               rawSection.GetSectionMaterialSpeciesRows().Length != 0 &&
                XtraMessageBox.Show(this,
                Resources.MsgQuestionAddRawSectionMaterialSpecies,
                Resources.CaptionQuestionMsgBox,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
      {
        AddSectionMaterialSpeciesRows(rawSection);
      }

      SetGridMaterialSpeciesDataSource();
    }

    private void viewMaterialSpecies_ShowingEditor(object sender, CancelEventArgs e)
    {
      if (chkUseRawSectionMaterialSpecies.Checked || chkUseMaterialSpeciesDefinition.Checked)
      {
        e.Cancel = true;
        viewMaterialSpecies.RefreshData();
      }
    }

    #endregion MaterialSpecies

    #region CustomSectionItem

    private void viewCustomSectionItem_InitNewRow(object sender, DevExpress.XtraGrid.Views.Grid.InitNewRowEventArgs e)
    {
      SectionDataSet.CustomSectionItemRow row = viewCustomSectionItem.GetDataRow(e.RowHandle) as SectionDataSet.CustomSectionItemRow;
      row.IdParentSection = sectionRow.Id;
      row.ComponentOffset = 0;
      row.HasVariableW = false;
      if (sectionRow.MaterialType == MaterialType.Wood.ToString())
      {
        row.MaterialSpeciesUsage = lookUpMaterialSpeciesUsage.EditValue.ToString();
        if (row.SectionRowByFK_Section_CustomSectionItem != null)
        {
          SectionDataSet.SectionRow childSectionRow = sectionDS.Section.FindById(row.IdSection);
          if (childSectionRow != null)
          {
            row.HasVariableW = childSectionRow.HasVariableW;
          }
        }
      }
      row.ConvertedCuttingType = ComponentCuttingType.Default;
    }

    private void viewCustomSectionItem_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        case Keys.Delete:
          {
            GridUtils.Delete(viewCustomSectionItem);
            break;
          }
      }
    }

    private void viewCustomSectionItem_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {

      int rowHandle = viewCustomSectionItem.GetRowHandle(e.ListSourceRowIndex);

      if (viewCustomSectionItem.FocusedRowHandle == rowHandle &&
          viewCustomSectionItem.FocusedColumn == colIdSection)
      {
        LookUpEdit lookUp = viewCustomSectionItem.ActiveEditor as LookUpEdit;
        if (lookUp != null && !lookUp.IsPopupOpen)
        {
          viewCustomSectionItem.CloseEditor();
        }
      }
      SectionDataSet.CustomSectionItemRow customSectionItemRow = viewCustomSectionItem.GetDataRow(rowHandle) as SectionDataSet.CustomSectionItemRow;
      if (customSectionItemRow == null)
      {
        return;
      }
      SectionDataSet.SectionRow sectionItemRow = null;
      if (customSectionItemRow[sectionDS.CustomSectionItem.IdSectionColumn.ColumnName] != DBNull.Value)
      {
        sectionItemRow = sectionDS.Section.FindById(customSectionItemRow.IdSection);
      }
      if (sectionItemRow == null)
      {
        return;
      }
      if (e.IsGetData && e.Column == colDesignation)
      {
        e.Value = sectionItemRow.Designation;
      }

      if (e.Column == colMaterialSpeciesUsageCustomSectionItem)
      {
        if (e.IsGetData && customSectionItemRow.MaterialSpeciesUsage == null && e.Value != null)
        {
          e.Value = sectionItemRow.MaterialSpeciesUsage;
        }
        else if (e.IsGetData && customSectionItemRow.MaterialSpeciesUsage != null)
        {
          e.Value = customSectionItemRow.MaterialSpeciesUsage;
        }
        if (e.IsSetData)
        {
          customSectionItemRow.MaterialSpeciesUsage = e.Value as String;
        }
      }
    }

    #endregion CustomSectionItem

    #region SectionReinforcement

    private void repLookUpReinforcementCode_EditValueChanged(object sender, EventArgs e)
    {
      try
      {
        LookUpEdit lookUp = sender as LookUpEdit;
        if (!lookUp.IsPopupOpen)
        {
          int idSection = (int)lookUp.EditValue;
          viewReinforcements.SetRowCellValue(viewReinforcements.FocusedRowHandle, colReinforcementDesignation, idSection);
        }
      }
      catch { }
    }

    private void viewReinforcements_InitNewRow(object sender, InitNewRowEventArgs e)
    {
      SectionDataSet.SectionReinforcementRow row = viewReinforcements.GetDataRow(e.RowHandle) as SectionDataSet.SectionReinforcementRow;
      row.IdSection = sectionRow.Id;
      row.Offset = Properties.Settings.Default.SectionDefaultReinforcementOffset;
      row.OffsetVCut = Properties.Settings.Default.SectionDefaultReinforcementOffsetVCut;
      row.ConvertedCuttingType = ComponentCuttingType.StraightCut;
    }

    private void viewReinforcements_KeyUp(object sender, KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        case Keys.Delete:
          {
            GridUtils.Delete(viewReinforcements);
            break;
          }
      }
    }
    /// <summary>
    /// Eveniment ce seteaza stilul randurilor. Daca reinforcementul este inactiv, acesta apare cu culoarea gri
    /// </summary>
    private void ViewReinforcements_RowStyle(object sender, RowStyleEventArgs e)
    {
      GridView view = sender as GridView;
      if (e.RowHandle < 0)
        return;

      SectionDataSet.SectionReinforcementRow currentRow = viewReinforcements.GetDataRow(e.RowHandle) as SectionDataSet.SectionReinforcementRow;
      if (currentRow == null)
      {
        return;
      }
      SectionDataSet.SectionRow currentSectionRow = currentRow.SectionRowByFkSectionSectionReinforcementChild;
      if (currentSectionRow == null)
      {
        return;
      }

      if (!currentSectionRow.IsActive)
      {
        e.Appearance.ForeColor = SystemColors.GrayText;
      }

      if (!currentSectionRow.IsActive && (view.IsRowSelected(e.RowHandle) || view.FocusedRowHandle == e.RowHandle))
      {
        e.Appearance.ForeColor = Color.LightSlateGray;
        e.Appearance.BackColor = view.Appearance.SelectedRow.BackColor;
        e.HighPriority = true;
      }
    }

    private void viewReinforcements_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {

      int rowHandle = viewReinforcements.GetRowHandle(e.ListSourceRowIndex);

      SectionDataSet.SectionReinforcementRow sectionReinforcementRow = viewReinforcements.GetDataRow(rowHandle) as SectionDataSet.SectionReinforcementRow;
      if (sectionReinforcementRow == null)
        return;

      SectionDataSet.SectionRow sectionItemRow = null;
      if (sectionReinforcementRow[sectionDS.SectionReinforcement.IdSectionReinforcementColumn.ColumnName] != DBNull.Value)
      {
        sectionItemRow = sectionDS.Section.FindById(sectionReinforcementRow.IdSectionReinforcement);
      }
      if (sectionItemRow == null)
        return;

      e.Value = sectionItemRow.Designation;
    }

    #endregion SectionReinforcement

    #region SectionCover

    private void viewSectionCover_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {

      int rowHandle = viewSectionCover.GetRowHandle(e.ListSourceRowIndex);

      if (viewSectionCover.IsNewItemRow(rowHandle))
        return;

      if (e.IsSetData)
        return;

      if (e.Column != colSectionCoverDesignation)
        return;

      if (rowHandle == viewSectionCover.FocusedRowHandle)
        viewSectionCover.CloseEditor();

      SectionDataSet.SectionCoverRow sectionCoverRow = viewSectionCover.GetDataRow(rowHandle) as SectionDataSet.SectionCoverRow;
      if (sectionCoverRow == null)
        return;

      SectionDataSet.SectionRow sectionRow = sectionDS.Section.FindById(sectionCoverRow.IdSectionCover);
      if (sectionRow == null)
        return;

      e.Value = sectionRow.Designation;
    }

    private void viewSectionCover_InitNewRow(object sender, InitNewRowEventArgs e)
    {
      SectionDataSet.SectionCoverRow sectionCoverRow = viewSectionCover.GetDataRow(e.RowHandle) as SectionDataSet.SectionCoverRow;
      sectionCoverRow.IdSection = sectionRow.Id;
      sectionCoverRow.Priority = 2;
    }

    private void viewSectionCover_KeyUp(object sender, KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        case Keys.Delete:
          {
            GridUtils.Delete(viewSectionCover);
            break;
          }
      }
    }

    #endregion SectionCover

    #region SectionHeatTransferCoefficient

    private void viewSectionHeatTransferCoefficient_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {

      int rowHandle = viewSectionHeatTransferCoefficient.GetRowHandle(e.ListSourceRowIndex);

      SectionDataSet.SectionHeatTransferCoefficientRow heatTransferRow = viewSectionHeatTransferCoefficient.GetDataRow(rowHandle) as SectionDataSet.SectionHeatTransferCoefficientRow;
      if (heatTransferRow == null)
        return;

      if (e.Column == colSHTIdSection)
      {
        if (e.IsGetData)
        {
          e.Value = (heatTransferRow[sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName] != DBNull.Value &&
                    (int)heatTransferRow[sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName] == sectionRow.Id) ?
                      heatTransferRow[sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName] :
                      heatTransferRow[sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName];
        }

        if (e.IsSetData)
        {
          if (heatTransferRow.IdSection1 == sectionRow.Id)
            heatTransferRow.IdSection2 = (int)e.Value;
          else
            heatTransferRow.IdSection1 = (int)e.Value;
        }
      }
      if (e.Column == colSHTDesignation)
      {
        SectionDataSet.SectionRow sectionItemRow = null;

        if (heatTransferRow[sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName] != DBNull.Value &&
          (int)heatTransferRow[sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName] != sectionRow.Id)
        {
          sectionItemRow = sectionDS.Section.FindById(heatTransferRow.IdSection1);
        }
        else if (heatTransferRow[sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName] != DBNull.Value &&
          (int)heatTransferRow[sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName] != sectionRow.Id)
        {
          sectionItemRow = sectionDS.Section.FindById(heatTransferRow.IdSection2);
        }
        if (sectionItemRow == null)
        {
          return;
        }
        e.Value = sectionItemRow.Designation;
      }

      if (e.Column == colSHTSectionType)
      {
        SectionDataSet.SectionRow sectionItemRow = null;

        if (heatTransferRow[sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName] != DBNull.Value &&
          (int)heatTransferRow[sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName] != sectionRow.Id)
        {
          sectionItemRow = sectionDS.Section.FindById(heatTransferRow.IdSection1);
        }
        else if (heatTransferRow[sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName] != DBNull.Value &&
          (int)heatTransferRow[sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName] != sectionRow.Id)
        {
          sectionItemRow = sectionDS.Section.FindById(heatTransferRow.IdSection2);
        }

        if (sectionItemRow == null)
        {
          return;
        }
        e.Value = sectionItemRow.SectionType;
      }
    }

    private void viewSectionHeatTransferCoefficient_InitNewRow(object sender, InitNewRowEventArgs e)
    {
      SectionDataSet.SectionHeatTransferCoefficientRow heatTransferRow = viewSectionHeatTransferCoefficient.GetDataRow(e.RowHandle) as SectionDataSet.SectionHeatTransferCoefficientRow;
      heatTransferRow.IdSection1 = sectionRow.Id;
      heatTransferRow.HeatTransferCoefficient = 0;
      heatTransferRow.SetTrackIndexNull();
    }

    private void viewSectionHeatTransferCoefficient_KeyUp(object sender, KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        case Keys.Delete:
          {
            GridUtils.Delete(viewSectionHeatTransferCoefficient);
            break;
          }
      }
    }

    private void viewSectionHeatTransferCoefficient_ValidateRow(object sender, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)
    {
      SectionDataSet.SectionHeatTransferCoefficientRow validateRow = viewSectionHeatTransferCoefficient.GetDataRow(e.RowHandle) as SectionDataSet.SectionHeatTransferCoefficientRow;
      int? validateRowIdMaterialSpecies = validateRow[sectionDS.SectionHeatTransferCoefficient.IdMaterialSpeciesColumn.ColumnName] == DBNull.Value ? (int?)null : (int)validateRow[sectionDS.SectionHeatTransferCoefficient.IdMaterialSpeciesColumn.ColumnName];
      int? validateRowIdSection1 = validateRow[sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName] == DBNull.Value ? (int?)null : (int)validateRow[sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName];
      int? validateRowIdSection2 = validateRow[sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName] == DBNull.Value ? (int?)null : (int)validateRow[sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName];

      foreach (SectionDataSet.SectionHeatTransferCoefficientRow _shtRow in sectionDS.SectionHeatTransferCoefficient.Rows)
      {
        if (_shtRow == validateRow || _shtRow.RowState == DataRowState.Deleted)
          continue;

        int? shtRowIdSection1 = _shtRow[sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName] == DBNull.Value ? (int?)null : (int)_shtRow[sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName];
        int? shtRowIdSection2 = _shtRow[sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName] == DBNull.Value ? (int?)null : (int)_shtRow[sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName];
        int? shtRowIdMaterialSpecies = _shtRow[sectionDS.SectionHeatTransferCoefficient.IdMaterialSpeciesColumn.ColumnName] == DBNull.Value ? (int?)null : (int)_shtRow[sectionDS.SectionHeatTransferCoefficient.IdMaterialSpeciesColumn.ColumnName];

        if (shtRowIdSection1 != sectionRow.Id)
        {
          if (validateRowIdSection1 == shtRowIdSection1 &&
              validateRowIdMaterialSpecies == shtRowIdMaterialSpecies)
          {
            e.ErrorText = Properties.Resources.MsgErrorInvalidSHTrow;
            e.Valid = false;
          }
        }
        else
        {
          if (validateRowIdSection2.HasValue && validateRowIdSection2 == shtRowIdSection2 &&
              validateRowIdMaterialSpecies == shtRowIdMaterialSpecies)
          {
            e.ErrorText = Properties.Resources.MsgErrorInvalidSHTrow;
            e.Valid = false;
          }
        }

        if (validateRowIdSection2 == shtRowIdSection1 &&
            validateRowIdSection1 == shtRowIdSection2 &&
            validateRowIdMaterialSpecies == shtRowIdMaterialSpecies)
        {
          e.ErrorText = Properties.Resources.MsgErrorInvalidSHTrow;
          e.Valid = false;
        }
      }

      // Verific ca pentru a pune tip de material pe combinatie, ambele profile sunt din acelasi material.
      SectionDataSet.SectionRow combinationSectionRow;
      if (validateRowIdSection1 != sectionRow.Id)
      {
        combinationSectionRow = sectionDS.Section.FindById(validateRowIdSection1.Value);
      }
      else
      {
        combinationSectionRow = sectionDS.Section.FindById(validateRowIdSection2.Value);
      }

      if (validateRowIdMaterialSpecies.HasValue &&
         combinationSectionRow.ConvertedMaterialType != (MaterialType)lookUpMaterialType.EditValue)
      {
        e.ErrorText = Properties.Resources.MsgErrorDifferentShtSectionsMaterialType;
        e.Valid = false;
      }
    }

    private void repLookUpEditingMode_EditValueChanged(object sender, EventArgs e)
    {
      LookUpEdit lookUp = sender as LookUpEdit;
      SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(viewSectionColorList.FocusedRowHandle) as SectionDataSet.SectionColorListRow;
      if (sectionColorListRow == null || viewSectionColorList.IsNewItemRow(viewSectionColorList.FocusedRowHandle))
        return;

      ColorListDataSet.ColorListRow colorListRow = colorListDS.ColorList.FindById(sectionColorListRow.IdColorList);

      if (lookUp.EditValue.ToString() == ColorListEditingMode.None.ToString() ||
          lookUp.EditValue.ToString() == ColorListEditingMode.EditColorListPrice.ToString())
      {
        foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListRow.GetSectionColorListItemRows())
        {
          //sectionColorListRow.SetBarLengthNull();
          // sectionColorListRow.SetIdReinforcementNull();
          sectionColorListItemRow.Delete();
        }
      }

      if (lookUp.EditValue.ToString() == ColorListEditingMode.None.ToString())
      {
        if (!colorListRow.IsIdCostGroupNull())
          sectionColorListRow.IdCostGroup = colorListRow.IdCostGroup;
        else
          sectionColorListRow.IdCostGroup = costDS.CostGroup[0].Id;


        sectionColorListRow.PriceCalculationType = colorListRow.PriceCalculationType;
        sectionColorListRow.Price = sectionColorListRow.SectionRow.HasThermalBreak ?
                    CurrencyExchange.CurrentCurrencyExchange.Exchange(colorListRow.DisplayUnitPriceTB, colorListRow.Currency, lookUpCurrency.EditValue.ToString()) :
                    CurrencyExchange.CurrentCurrencyExchange.Exchange(colorListRow.DisplayUnitPrice, colorListRow.Currency, lookUpCurrency.EditValue.ToString());
      }

      // Daca se debifeaza utilizarea culorilor asociate listei, culorile acesteia se vor adauga in culorile profilului.

      if (colorListRow != null && lookUp.EditValue.ToString() == ColorListEditingMode.EditColorListColorCombinations.ToString())
      {
        foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in GetSectionColorListItemRows(sectionColorListRow))
        {
          SectionDataSet.SectionColorListItemRow newSectionColorListItemRow = CopySectionColorListItemRow(sectionColorListItemRow, sectionDS.SectionColorListItem);
          newSectionColorListItemRow.GenerateUniqueCode();
          sectionDS.SectionColorListItem.AddSectionColorListItemRow(newSectionColorListItemRow);
        }
      }

      sectionColorListRow.EditingMode = lookUp.EditValue.ToString();
      viewColorListItem.OptionsBehavior.Editable = lookUp.EditValue.ToString() == ColorListEditingMode.EditColorListColorCombinations.ToString();

      viewSectionColorList.CloseEditor();
    }

    private void repLookUpEditingMode_EditValueChanging(object sender, ChangingEventArgs e)
    {
      SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(viewSectionColorList.FocusedRowHandle) as SectionDataSet.SectionColorListRow;
      if (sectionColorListRow == null)
        return;

      if (viewSectionColorList.FocusedColumn.Name == colEditingMode.Name && e.NewValue.ToString() == ColorListEditingMode.None.ToString())
      {
        sectionColorListRow.SetBarLengthNull();
      }

      if (viewSectionColorList.FocusedColumn.Name == colEditingMode.Name &&
              (e.NewValue.ToString() == ColorListEditingMode.None.ToString() ||
               e.NewValue.ToString() == ColorListEditingMode.EditColorListPrice.ToString()))
      {
        if (!CanUseColorList(sectionColorListRow))
        {
          XtraMessageBox.Show(this,
              Resources.MsgErrorInvalidSectionColorList,
              Resources.CaptionAttentionMsgBox,
              MessageBoxButtons.OK,
              MessageBoxIcon.Exclamation);
          e.Cancel = true;
          return;
        }

        // Daca se trece la folosirea listei de culori, atunci se sterg itemurile adaugate de utilizator.
        SectionDataSet.SectionColorListItemRow[] sectionColorListItemRows = sectionColorListRow.GetSectionColorListItemRows();
        if (sectionColorListItemRows != null && sectionColorListItemRows.Length > 0)
        {
          if (XtraMessageBox.Show(this,
              Resources.MsgConfirmUseColorList,
              Resources.CaptionAttentionMsgBox,
              MessageBoxButtons.YesNo,
              MessageBoxIcon.Exclamation) == DialogResult.No)
          {
            e.Cancel = true;
            return;
          }
        }
      }
    }

    private void repLookUpPriceCalculationType_EditValueChanged(object sender, EventArgs e)
    {
      TextEdit editor = sender as TextEdit;
      SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(viewSectionColorList.FocusedRowHandle) as SectionDataSet.SectionColorListRow;
      sectionColorListRow.PriceCalculationType = editor.EditValue.ToString();
      SetGridColorItemDataSource();
    }

    #endregion SectionHeatTransferCoefficient

    #region SectionTolerance

    private void viewTolerance_KeyUp(object sender, KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        case Keys.Delete:
          {
            GridUtils.Delete(viewTolerance);
            this.modified = true;
            break;
          }
      }
    }

    private void viewTolerance_InitNewRow(object sender, InitNewRowEventArgs e)
    {
      SectionDataSet.SectionToleranceRow toleranceRow = viewTolerance.GetDataRow(e.RowHandle) as SectionDataSet.SectionToleranceRow;
      toleranceRow.IdSection1 = sectionRow.Id;
      toleranceRow.Tolerance = 0;
    }

    private void viewTolerance_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {

      int rowHandle = viewTolerance.GetRowHandle(e.ListSourceRowIndex);

      if (viewTolerance.IsNewItemRow(rowHandle))
        return;

      SectionDataSet.SectionToleranceRow toleranceRow = viewTolerance.GetDataRow(rowHandle) as SectionDataSet.SectionToleranceRow;
      if (toleranceRow == null)
        return;

      if (e.Column == colToleranceIdSection)
      {
        if (e.IsGetData)
        {
          e.Value = toleranceRow[sectionDS.SectionTolerance.IdSection2Column.ColumnName];
        }

        if (e.IsSetData)
        {
          toleranceRow.IdSection2 = (int)e.Value;
        }
      }

      if (e.Column == colToleranceDesignation)
      {
        SectionDataSet.SectionRow sectionItemRow = null;
        sectionItemRow = sectionDS.Section.FindById(toleranceRow.IdSection2);

        if (sectionItemRow == null)
        {
          return;
        }
        e.Value = sectionItemRow.Designation;
      }

      if (e.Column == colToleranceType)
      {
        SectionDataSet.SectionRow sectionItemRow = null;
        sectionItemRow = sectionDS.Section.FindById(toleranceRow.IdSection2);

        if (sectionItemRow == null)
        {
          return;
        }
        e.Value = sectionItemRow.SectionType;
      }
    }

    private void viewTolerance_ValidateRow(object sender, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)
    {
      SectionDataSet.SectionToleranceRow validateRow = viewTolerance.GetDataRow(e.RowHandle) as SectionDataSet.SectionToleranceRow;

      foreach (SectionDataSet.SectionToleranceRow sectionToleranceRow in sectionDS.SectionTolerance.Rows)
      {
        if (sectionToleranceRow == validateRow || sectionToleranceRow.RowState == DataRowState.Deleted)
          continue;

        if (sectionToleranceRow.IdSection1 != sectionRow.Id)
        {
          if (validateRow.IdSection1 == sectionToleranceRow.IdSection1)
          {
            e.ErrorText = Properties.Resources.MsgErrorInvalidSHTrow;
            e.Valid = false;
          }
        }
        else
        {
          if (validateRow[sectionDS.SectionTolerance.IdSection2Column.ColumnName] != DBNull.Value &&
            (int)validateRow[sectionDS.SectionTolerance.IdSection2Column.ColumnName] == sectionToleranceRow.IdSection2)
          {
            e.ErrorText = Properties.Resources.MsgErrorInvalidSHTrow;
            e.Valid = false;
          }
        }
      }
    }

    #endregion SectionTolerance

    #region Bar Manager

    private void bbCut_ItemClick(object sender, ItemClickEventArgs e)
    {
      GridUtils gridUtils = new GridUtils();
      gridUtils.View = viewSectionColorList;

      if (viewColorListItem.IsFocusedView)
      {
        int[] selectedListsRowHandles = viewSectionColorList.GetSelectedRows();
        if (selectedListsRowHandles.Length > 1)
          return;

        SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(selectedListsRowHandles[0]) as SectionDataSet.SectionColorListRow;
        if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None ||
              sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListPrice)
          return;

        gridUtils.View = viewColorListItem;
      }

      gridUtils.Cut();
      gridUtils.View = null;
    }

    private void bbPaste_ItemClick(object sender, ItemClickEventArgs e)
    {
      GridUtils gridUtils = new GridUtils();
      gridUtils.View = viewSectionColorList;

      if (viewColorListItem.IsFocusedView)
      {
        int[] selectedListsRowHandles = viewSectionColorList.GetSelectedRows();
        if (selectedListsRowHandles.Length > 1)
          return;

        SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(selectedListsRowHandles[0]) as SectionDataSet.SectionColorListRow;
        if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None ||
              sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListPrice)
          return;

        gridUtils.View = viewColorListItem;
      }

      gridUtils.Paste();
      gridUtils.View = null;
    }

    private void bbDelete_ItemClick(object sender, ItemClickEventArgs e)
    {
      if (viewColorListItem.IsFocusedView)
      {
        int[] selectedListsRowHandles = viewSectionColorList.GetSelectedRows();
        if (selectedListsRowHandles.Length > 1)
          return;

        SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(selectedListsRowHandles[0]) as SectionDataSet.SectionColorListRow;
        if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None ||
              sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListPrice)
          return;

        GridUtils.Delete(viewColorListItem);
        return;
      }
      GridUtils.Delete(viewSectionColorList);
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

      if (viewColorListItem.IsFocusedView)
      {
        int[] selectedRowHandles = view.GetSelectedRows();
        if (selectedRowHandles.Length > 1)
          return;
        bool enableRowEditTools = false;

        if (selectedRowHandles != null && selectedRowHandles.Length > 0)
        {
          enableRowEditTools = true;
        }
        bbCut.Enabled = enableRowEditTools && openingMode == FormOpeningMode.Normal;
        bbPaste.Enabled = enableRowEditTools && openingMode == FormOpeningMode.Normal;
        bbDelete.Enabled = enableRowEditTools && openingMode == FormOpeningMode.Normal;
        bbRegenerateCodes.Enabled = enableRowEditTools && openingMode == FormOpeningMode.Normal;
        bbAddMissingColors.Enabled = enableRowEditTools && openingMode == FormOpeningMode.Normal;

        if (viewSectionColorList.GetSelectedRows().Length > 1)
        {
          bbCut.Enabled = false;
          bbPaste.Enabled = false;
          bbDelete.Enabled = false;
          bbRegenerateCodes.Enabled = false;
          bbAddMissingColors.Enabled = false;
          return;
        }

        SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(viewSectionColorList.FocusedRowHandle) as SectionDataSet.SectionColorListRow;
        if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None ||
              sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListPrice)
        {
          bbCut.Enabled = false;
          bbPaste.Enabled = false;
          bbDelete.Enabled = false;
          bbRegenerateCodes.Enabled = false;
          bbAddMissingColors.Enabled = false;
          return;
        }

      }
      else if (viewSectionColorList.IsFocusedView)
      {
        bbCut.Enabled = false;
        bbPaste.Enabled = false;
        bbDelete.Enabled = true;
        bbRegenerateCodes.Enabled = true;
        bbAddMissingColors.Enabled = true;

        SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(viewSectionColorList.FocusedRowHandle) as SectionDataSet.SectionColorListRow;
        if (sectionColorListRow == null || sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None ||
              sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListPrice)
        {
          bbRegenerateCodes.Enabled = false;
          bbAddMissingColors.Enabled = false;
        }
      }
    }

    /// <summary>
    /// Eveniment ce dezactiveaza functiile de Cut, Copy, Paste, Delete 
    /// cand se editeaza un control (ptr a folosi functiile locale)
    /// </summary>
    private void barManager_ShortcutItemClick(object sender, ShortcutItemClickEventArgs e)
    {
      if (!gridSectionColorList.IsFocused)
      {
        e.Cancel = true;
        return;
      }
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

    private void bbRegenerateCodes_ItemClick(object sender, ItemClickEventArgs e)
    {
      bool areConstraints = sectionDS.EnforceConstraints;
      sectionDS.EnforceConstraints = false;
      try
      {
        List<SectionDataSet.SectionColorListItemRow> sectionColorListItemRows = new List<SectionDataSet.SectionColorListItemRow>();
        if (viewSectionColorList.IsFocusedView)
        {
          int[] selectedRowHandles = viewSectionColorList.GetSelectedRows();

          //Se formeaza o lista cu elementele ce trebuie regenerate.
          for (int i = 0; i < selectedRowHandles.Length; i++)
          {
            SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(selectedRowHandles[i]) as SectionDataSet.SectionColorListRow;
            if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListColorCombinations)
              sectionColorListItemRows.AddRange(sectionColorListRow.GetSectionColorListItemRows());
          }
          RegenerateCodes(sectionColorListItemRows);
        }
        else if (viewColorListItem.IsFocusedView)
        {
          int[] selectedRowHandles = viewColorListItem.GetSelectedRows();

          //Se formeaza o lista cu elementele ce trebuie regenerate.
          for (int i = 0; i < selectedRowHandles.Length; i++)
          {
            SectionDataSet.SectionColorListItemRow sectionColorListItemRow = viewColorListItem.GetDataRow(selectedRowHandles[i]) as SectionDataSet.SectionColorListItemRow;
            sectionColorListItemRows.Add(sectionColorListItemRow);
          }

          RegenerateCodes(sectionColorListItemRows);
        }

        SetGridColorItemDataSource();
      }
      finally
      {
        sectionDS.EnforceConstraints = areConstraints;
      }
    }

    private void bbAddMissingColors_ItemClick(object sender, ItemClickEventArgs e)
    {
      bool areConstraintsEnforced = sectionDS.EnforceConstraints;
      sectionDS.EnforceConstraints = false;
      try
      {
        SectionDataSet.SectionColorListRow sectionColorListRow = null;
        IEnumerable<SectionDataSet.SectionColorListItemRow> sectionColorListItemRows;
        IEnumerable<SectionDataSet.SectionColorListItemRow> usedSectionColorListItemRows;
        if (viewSectionColorList.IsFocusedView)
        {
          int[] selectedListRowHandles = viewSectionColorList.GetSelectedRows();
          if (selectedListRowHandles.Length > 1)
            return;

          sectionColorListRow = viewSectionColorList.GetDataRow(selectedListRowHandles[0]) as SectionDataSet.SectionColorListRow;
        }
        else if (viewColorListItem.IsFocusedView)
        {
          int[] selectedListRowHandles = viewSectionColorList.GetSelectedRows();
          if (selectedListRowHandles.Length > 1)
            return;

          sectionColorListRow = viewSectionColorList.GetDataRow(selectedListRowHandles[0]) as SectionDataSet.SectionColorListRow;
        }

        if (sectionColorListRow == null)
          return;
        sectionColorListItemRows = GetSectionColorListItemRows(sectionColorListRow);
        usedSectionColorListItemRows = sectionColorListRow.GetSectionColorListItemRows();

        foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListItemRows)
          if (usedSectionColorListItemRows.SingleOrDefault(usedSectionColorListItemRow => usedSectionColorListItemRow.IdColorCombination == sectionColorListItemRow.IdColorCombination) == null)
          {
            sectionColorListItemRow.GenerateUniqueCode();
            sectionDS.SectionColorListItem.AddSectionColorListItemRow(CopySectionColorListItemRow(sectionColorListItemRow, sectionDS.SectionColorListItem));
          }
        SetGridColorItemDataSource();
      }
      finally
      {
        sectionDS.EnforceConstraints = areConstraintsEnforced;
      }
    }

    #endregion Bar Manager

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

    private void view_InvalidRowException(object sender, DevExpress.XtraGrid.Views.Base.InvalidRowExceptionEventArgs e)
    {
      e.ExceptionMode = gridUtils.TreatInvalidRowException((GridView)sender, e.Exception);
    }

    private void repNumeric_Spin(object sender, SpinEventArgs e)
    {
      e.Handled = true;
    }

    /// <summary>
    /// Tratare eveniment declansat la 'mutarea' focusului intr-un control de tip editor.
    /// </summary>
    private void editor_Enter(object sender, EventArgs e)
    {
      BaseEdit editor = sender as BaseEdit;
      if (editor != null)
        editor.SelectAll();
    }

    /// <summary>
    /// Tratare eveniment declansat la modificarea valorii intr-un control de tip editor.
    /// </summary>
    private void Component_Modified(object sender, EventArgs e)
    {
      this.modified = true;
    }

    /// <summary>
    /// Tratare eveniment declansat pentru validarea valorii unui editor care opereaza pe valori pozitive.
    /// </summary>
    /// <param name="sender">Controlul care declanseaza evenimentul.</param>
    /// <param name="e">Argumentul evenimentului.</param>
    private void positiveValueEditor_Validating(object sender, CancelEventArgs e)
    {
      BaseEdit editor = sender as BaseEdit;
      if (editor != null)
      {
        if (Convert.ToDecimal(editor.EditValue) < 0)
          e.Cancel = true;
      }
    }

    /// <summary>
    /// Tratare eveniment declansat pentru tratarea cazului introducerii unei valori invalide intr-un editor.
    /// </summary>
    /// <param name="sender">Controlul care declanseaza evenimentul.</param>
    /// <param name="e">Argumentul evenimentului.</param>
    private void editor_InvalidValue(object sender, InvalidValueExceptionEventArgs e)
    {
      e.ErrorText = Resources.MsgInvalidValue;
    }

    /// <summary>
    /// Tratare eveniment declansat la modificarea bifei care specifica daca elementul intra in optimizare.
    /// </summary>
    /// <param name="sender">Controlul care declanseaza evenimentul.</param>
    /// <param name="e">Argumentul evenimentului.</param>
    private void chkIsForOptimization_CheckedChanged(object sender, EventArgs e)
    {
      UpdateOptimizationControls();
    }

    /// <summary>
    /// Tratare eveniment declansat la modificarea modului de depasire a barei.
    /// </summary>
    /// <param name="sender">Controlul care declanseaza evenimentul.</param>
    /// <param name="e">Argumentul evenimentului.</param>
    private void lookUpExtendingMode_EditValueChanged(object sender, EventArgs e)
    {
      UpdateSegmentControls();
    }

    /// <summary>
    /// Tratare eveniment declansat la apasarea butonului unui editor.
    /// </summary>
    /// <param name="sender">Controlul care declanseaza evenimentul.</param>
    /// <param name="e">Argumentul evenimentului.</param>
    private void editNullable_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      BaseEdit editNullable = sender as BaseEdit;
      if (editNullable != null && e.Button.Kind == ButtonPredefines.Delete)
        editNullable.EditValue = null;

      if (editNullable != null && e.Button.Kind == ButtonPredefines.Right && editNullable.EditValue != null)
      {
        int id = -1;

        int.TryParse(editNullable.EditValue.ToString(), out id);

        OpenSectionEdit(id);


        sectionBS.DataSource = new DataView(sectionDS.Section);
        sectionReinforcementBS.DataSource = sectionDS.SectionReinforcement;
        sectionReinforcementBS.Filter = string.Format("{0} = {1}", sectionDS.SectionReinforcement.IdSectionColumn.ColumnName, sectionRow.Id);
        gridReinforcements.DataSource = sectionReinforcementBS;
        repLookUpSectionReinforcementCuttingType.DataSource = EnumTypeLocalizer.Localize(typeof(ComponentCuttingType));
        //repGridLookUpSectionReinforcementCode.DataSource = sectionBS;
        gridReinforcements.RefreshDataSource();
        SetDefaultReinforcementDataSource();
      }

      SectionDataSet.CustomSectionItemRow customSection = viewCustomSectionItem.GetFocusedDataRow() as SectionDataSet.CustomSectionItemRow;
      if (customSection != null)
      {
        customSection.MaterialSpeciesUsage = null;
      }
      viewCustomSectionItem.CloseEditor();
    }

    private void lookUpRawSection_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      if (lookUpRawSection != null && e.Button.Kind == ButtonPredefines.Right && lookUpRawSection.EditValue != null)
      {
        int id = -1;

        int.TryParse(lookUpRawSection.EditValue.ToString(), out id);

        OpenSectionEdit(id);

        rawSectionBS.DataSource = new DataView(sectionDS.Section);
        rawSectionBS.Filter = sectionRow.IsActive ?
          string.Format("{0} = '{1}' AND {2} <> {3} AND {4} = {5}",
                    sectionDS.Section.SectionTypeColumn.ColumnName,
                    SectionType.RawSection,
                    sectionDS.Section.IdColumn.ColumnName,
                    sectionRow.Id,
                    sectionDS.Section.IsActiveColumn.ColumnName,
                    Boolean.TrueString) :
          string.Format("{0} = '{1}' AND {2} <> {3}",
                    sectionDS.Section.SectionTypeColumn.ColumnName,
                    SectionType.RawSection,
                    sectionDS.Section.IdColumn.ColumnName,
                    sectionRow.Id);
        lookUpRawSection.Properties.DataSource = rawSectionBS;
      }
    }

    private void lookUpArcRawSection_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      if (lookUpArcRawSection != null && e.Button.Kind == ButtonPredefines.Right && lookUpArcRawSection.EditValue != null)
      {
        int id = -1;

        int.TryParse(lookUpArcRawSection.EditValue.ToString(), out id);

        OpenSectionEdit(id);

        rawSectionBS.DataSource = new DataView(sectionDS.Section);
        rawSectionBS.Filter = sectionRow.IsActive ?
          string.Format("{0} = '{1}' AND {2} <> {3} AND {4} = {5}",
                    sectionDS.Section.SectionTypeColumn.ColumnName,
                    SectionType.RawSection,
                    sectionDS.Section.IdColumn.ColumnName,
                    sectionRow.Id,
                    sectionDS.Section.IsActiveColumn.ColumnName,
                    Boolean.TrueString) :
          string.Format("{0} = '{1}' AND {2} <> {3}",
                    sectionDS.Section.SectionTypeColumn.ColumnName,
                    SectionType.RawSection,
                    sectionDS.Section.IdColumn.ColumnName,
                    sectionRow.Id);
        lookUpArcRawSection.Properties.DataSource = rawSectionBS;
      }
    }

    private void repGridLookUpCustomSectionItemCode_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      BaseEdit baseEdit = sender as BaseEdit;
      bool isNewItemRow = viewCustomSectionItem.IsNewItemRow(viewCustomSectionItem.FocusedRowHandle);

      if (baseEdit != null && e.Button.Kind == ButtonPredefines.Right && baseEdit.EditValue != null && !isNewItemRow)
      {
        int id = -1;

        int.TryParse(baseEdit.EditValue.ToString(), out id);

        OpenSectionEdit(id);

        sectionBS.DataSource = new DataView(sectionDS.Section);
        customSectionItemBS.DataSource = sectionDS.CustomSectionItem;
        customSectionItemBS.Filter = string.Format("{0} = {1}",
                                                    sectionDS.CustomSectionItem.IdParentSectionColumn.ColumnName,
                                                    sectionRow.Id);
        gridCustomSectionItem.DataSource = customSectionItemBS;

        DataView custemSectionDataSourceView = new DataView(sectionDS.Section);
        custemSectionDataSourceView.RowFilter = string.Format("{0} <> {1}",
          sectionDS.Section.IdColumn.ColumnName, sectionRow.Id);
        repGridLookUpCustomSectionItemCode.DataSource = custemSectionDataSourceView;

        gridCustomSectionItem.RefreshDataSource();
      }
    }

    private void repGridLookUpSectionReinforcementCode_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      BaseEdit baseEdit = sender as BaseEdit;
      bool isNewItemRow = viewReinforcements.IsNewItemRow(viewReinforcements.FocusedRowHandle);
      if (baseEdit != null && e.Button.Kind == ButtonPredefines.Right && baseEdit.EditValue != null && !isNewItemRow)
      {
        int id = -1;

        int.TryParse(baseEdit.EditValue.ToString(), out id);

        OpenSectionEdit(id);

        sectionReinforcementBS.DataSource = sectionDS.SectionReinforcement;
        sectionReinforcementBS.Filter = string.Format("{0} = {1}", sectionDS.SectionReinforcement.IdSectionColumn.ColumnName, sectionRow.Id);
        gridReinforcements.DataSource = sectionReinforcementBS;
        repLookUpSectionReinforcementCuttingType.DataSource = EnumTypeLocalizer.Localize(typeof(ComponentCuttingType));
        gridReinforcements.RefreshDataSource();
        SetDefaultReinforcementDataSource();
      }
    }

    private void repGridLookUpToleranceSection_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      BaseEdit baseEdit = sender as BaseEdit;
      bool isNewItemRow = viewTolerance.IsNewItemRow(viewTolerance.FocusedRowHandle);
      if (baseEdit != null && e.Button.Kind == ButtonPredefines.Right && baseEdit.EditValue != null && !isNewItemRow)
      {
        int id = -1;

        int.TryParse(baseEdit.EditValue.ToString(), out id);

        OpenSectionEdit(id);

        repGridLookUpToleranceSection.DataSource = sectionDS.Section;

        sectionToleranceBS.DataSource = sectionDS.SectionTolerance;
        sectionToleranceBS.Filter = string.Format("{0} = {1}",
          sectionRow.Id,
          sectionDS.SectionTolerance.IdSection1Column.ColumnName);
        gridTolerance.DataSource = sectionToleranceBS;

        gridTolerance.RefreshDataSource();
      }
    }

    private void repGridLookUpSectionCoverSection_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      BaseEdit baseEdit = sender as BaseEdit;
      if (baseEdit != null && e.Button.Kind == ButtonPredefines.Right && baseEdit.EditValue != null)
      {
        int id = -1;

        int.TryParse(baseEdit.EditValue.ToString(), out id);

        OpenSectionEdit(id);

        DataView sectionCoverDV = new DataView(sectionDS.SectionCover);
        sectionCoverDV.RowFilter = string.Format("{0} = {1}",
                                                 sectionDS.SectionCover.IdSectionColumn.ColumnName,
                                                 sectionRow.Id);
        gridSectionCover.DataSource = sectionCoverDV;

        repGridLookUpSectionCoverSection.DataSource = sectionDS.Section.Select(string.Format("{0} = 'Cover'",
                                                                                             sectionDS.Section.SectionTypeColumn.ColumnName));

        gridSectionCover.RefreshDataSource();
      }
    }

    private void repGridLookUpHeatTransferSection_ButtonClick(object sender, ButtonPressedEventArgs e)
    {
      BaseEdit baseEdit = sender as BaseEdit;
      if (baseEdit != null && e.Button.Kind == ButtonPredefines.Right && baseEdit.EditValue != null)
      {
        int id = -1;

        int.TryParse(baseEdit.EditValue.ToString(), out id);

        OpenSectionEdit(id);

        sectionHeatTransferCoefficientBS.DataSource = sectionDS.SectionHeatTransferCoefficient;
        sectionHeatTransferCoefficientBS.Filter = string.Format("{0} = {1} OR {0} = {2}",
          sectionRow.Id,
          sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName,
          sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName);
        gridSectionHeatTransferCoefficient.DataSource = sectionHeatTransferCoefficientBS;

        repGridLookUpHeatTransferSection.DataSource = sectionDS.Section;

        gridSectionHeatTransferCoefficient.RefreshDataSource();
      }
    }

    private void OpenSectionEdit(int id)
    {
      SectionDataSet.SectionRow otherSectionRow = sectionDS.Section.FindById(id);

      if (otherSectionRow == null)
      {
        return;
      }

      SectionDataSet sectionDataSetCopy = sectionDS.Copy() as SectionDataSet;
      ImageDataSet imageDataSetCopy = imageDS.Copy() as ImageDataSet;

      otherSectionRow = sectionDataSetCopy.Section.FindById(otherSectionRow.Id);
      bool isActive = true;

      if (sectionRow != null)
      {
        isActive = sectionRow.IsActive;
      }
      FrmSectionEdit frm = null;
      try
      {
        frm = new FrmSectionEdit(otherSectionRow, sectionDataSetCopy, imageDataSetCopy, seriesDS, costDS, colorListDS, consumeGroupDS,
                                          currencyDS, colorDS, materialSpeciesDS, currencyExchange, openingMode, onlyActive);
        if (frm.ShowDisposableDialog(false) == DialogResult.OK)
        {
          if (sectionDataSetCopy.HasChanges())
          {
            using (new WaitCursor())
            {
              if (otherSectionRow != null && isActive != otherSectionRow.IsActive)
              {
                DialogResult? lastDialogResult = null;
                ValidateSectionRow(otherSectionRow, sectionRow.IsActive, ref lastDialogResult, MessageBoxButtons.YesNoCancel, sectionDataSetCopy);

                if (lastDialogResult.HasValue && (lastDialogResult.Value == DialogResult.Cancel))
                  return;
              }
              sectionDS = frm.sectionDS.Copy() as SectionDataSet;
              this.sectionRow = sectionDS.Section.FindById(sectionRow.Id);
              imageDS = imageDataSetCopy.Copy() as ImageDataSet;
              //otherSectionRow = otherSectionRowCopy;
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
        sectionDataSetCopy.Dispose();
        imageDataSetCopy.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
      }
    }

    /// <summary>
    /// Valideaza datele pentru profile in functie de activ/inactiv.
    /// </summary>
    /// <param name="isActive">Flag ce indica daca datele au fost activate/dezactivate.</param>
    /// <param name="selectedRowHandles">Randurile selectate.</param>
    /// <param name="lastDialogResult">Mesajul ce urmeaza a fi afisat utilizatorului.</param>
    /// <param name="messageBoxButtons">Butoanele mesajului.</param>
    public void ValidateSectionRow(SectionDataSet.SectionRow sectionRow, bool isActive, ref DialogResult? lastDialogResult, MessageBoxButtons messageBoxButtons, SectionDataSet sectionDSCopy)
    {
      if (!isActive)
      {
        foreach (SectionDataSet.SectionRow parentSectionRow in sectionDSCopy.Section)
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
          }
        }

        foreach (SectionDataSet.SectionColorListRow parentSectionColorListRow in sectionDSCopy.SectionColorList)
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
          }

          foreach (SectionDataSet.SectionColorListItemRow parentSectionColorListItemRow in sectionDSCopy.SectionColorListItem)
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
            }
          }
        }

        foreach (SectionDataSet.SectionRow parentSectionRow in sectionRow.ParentSectionRows)
        {
          if (parentSectionRow.RowState == DataRowState.Deleted || !parentSectionRow.IsActive)
          {
            continue;
          }

          if (!lastDialogResult.HasValue || lastDialogResult.Value == System.Windows.Forms.DialogResult.OK)
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

          if (lastDialogResult.Value == DialogResult.Yes)
          {
            parentSectionRow.IsActive = false;
            continue;
          }

          sectionRow.IsActive = true;
        }
      }

      if (isActive)
      {
        foreach (SectionDataSet.SectionRow childSectionRow in sectionDSCopy.Section)
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


    private void lookUpCurrency_EditValueChanged(object sender, EventArgs e)
    {
      SetValueFormat();
      gridSectionColorList.RefreshDataSource();
      gridColorListItem.RefreshDataSource();
    }


    #endregion Event Handlers

    #region Private Methods

    /// <summary>
    /// Adauga randuri de sectionMaterialSpeciesRow dupa un rawSection parinte.
    /// </summary>
    /// <param name="rawSectionMaterialSpeciesRow"></param>
    private void AddSectionMaterialSpeciesRows(SectionDataSet.SectionRow rawSection)
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

    /// <summary>
    /// Scrie in profil ceea ce se gaseste in controale
    /// </summary>
    /// <returns>TRUE daca citirea s-a incheiat cu succes, FALSE altfel</returns>
    private bool ReadControls()
    {
      if (!ValidateControls())
      {
        return false;
      }
      if (!modified)
      {
        return true;
      }

      // Informatii generale
      sectionRow.Code = txtCode.Text;
      sectionRow.Designation = txtDesignation.Text;
      sectionRow.SectionType = lookUpSectionType.EditValue.ToString();
      sectionRow.ConvertedKnownSide = (KnownSide)flagsKnownSide.EditValue;
      sectionRow.MaterialType = lookUpMaterialType.EditValue.ToString();
      sectionRow.Currency = lookUpCurrency.EditValue.ToString();
      sectionRow.IsActive = chkIsActive.Checked;

      // Parametrii geometriei
      sectionRow.H1 = Convert.ToDecimal(unitH1.EditValue);
      sectionRow.H2 = Convert.ToDecimal(unitH2.EditValue);
      sectionRow.H3 = Convert.ToDecimal(unitH3.EditValue);
      sectionRow.H = Convert.ToDecimal(unitH.EditValue);
      sectionRow.W = Convert.ToDecimal(unitW.EditValue);
      sectionRow.W1 = Convert.ToDecimal(unitW1.EditValue);
      sectionRow.Perimeter = Convert.ToDecimal(unitPerimter.EditValue);
      sectionRow.Area = Convert.ToDecimal(unitArea.EditValue);

      // Imagine profil
      imageDS.Image.SetImage(sectionRow, imgImage.EditValue as byte[]);

      // Informatii consume profil
      sectionRow.BarLength = Convert.ToDecimal(unitBarLength.EditValue);

      sectionRow.UnitWeight = MeasurableProperties.DefaultMeasurableProperties.Length.ConvertInvDisplayToInternal(Convert.ToDecimal(unitUnitWeight.EditValue));
      sectionRow.CuttingTolerance = Convert.ToDecimal(unitCuttingTolerance.EditValue);
      sectionRow.BindingTolerance = Convert.ToDecimal(unitBindingTolerance.EditValue);
      sectionRow.ProcessingTolerance = Convert.ToDecimal(unitProcessingTolerance.EditValue);
      sectionRow.TenonTolerance = Convert.ToDecimal(unitTenonTolerance.EditValue);
      sectionRow.DowelTolerance = Convert.ToDecimal(unitDowelTolerance.EditValue);
      sectionRow.MullionDowelTolerance = Convert.ToDecimal(unitMullionDowelTolerance.EditValue);
      sectionRow.MullionTenonTolerance = Convert.ToDecimal(unitMullionTenonTolerance.EditValue);
      sectionRow.AdapterTolerance = Convert.ToDecimal(unitAdapterTolerance.EditValue);
      sectionRow.ExtraSashDimension = Convert.ToDecimal(unitSashExtraDimension.EditValue);

      if (unitGlassTolerance.EditValue == null)
        sectionRow.SetFillingToleranceNull();
      else
        sectionRow.FillingTolerance = Convert.ToDecimal(unitGlassTolerance.EditValue);

      sectionRow.CurvingAddition = Convert.ToDecimal(unitCurvingAddition.EditValue);
      sectionRow.MinRadius = Convert.ToDecimal(unitMinRadius.EditValue);

      if (unitSashTolerance.EditValue == null)
        sectionRow.SetSashToleranceNull();
      else
        sectionRow.SashTolerance = Convert.ToDecimal(unitSashTolerance.EditValue);

      if (unitNoThresholdTolerance.EditValue == null)
        sectionRow.SetNoThresholdToleranceNull();
      else
        sectionRow.NoThresholdTolerance = Convert.ToDecimal(unitNoThresholdTolerance.EditValue);

      if (unitFoldingSash2SashTolerance.EditValue == null)
        sectionRow.SetFoldingSash2SashToleranceNull();
      else
        sectionRow.FoldingSash2SashTolerance = Convert.ToDecimal(unitFoldingSash2SashTolerance.EditValue);

      if (unitSlideAndSwingWingTolerance.EditValue == null)
        sectionRow.SetSlideSwingToleranceNull();
      else
        sectionRow.SlideSwingTolerance = Convert.ToDecimal(unitSlideAndSwingWingTolerance.EditValue);

      sectionRow.IdConsumeGroup = (int)lookUpConsumeGroup.EditValue;

      sectionRow.PriceCalculationType = lookUpPriceComputeMode.EditValue.ToString();
      sectionRow.DisplayUnitBasePrice = Convert.ToDecimal(txtUnitWeightPrice.EditValue);

      if (bbTxtDisplayOrder.EditValue != null)
      {
        sectionRow.DisplayOrder = Convert.ToInt32(bbTxtDisplayOrder.EditValue);
      }
      else
      {
        sectionRow.SetDisplayOrderNull();
      }

      // Informatii armatura
      if (gridLookUpDefaultReinforcement.EditValue != null)
      {
        sectionRow.IdReinforcement = (int)gridLookUpDefaultReinforcement.EditValue;
      }
      else
      {
        sectionRow.SetIdReinforcementNull();
      }

      // Informatii profil brut
      sectionRow.MaterialSpeciesUsage = lookUpMaterialSpeciesUsage.EditValue.ToString();

      if ((RawSectionType)lookUpRawSectionType.EditValue == RawSectionType.Defined && lookUpRawSection.EditValue != null)
      {
        sectionRow.IdRawSection = (int)lookUpRawSection.EditValue;
        sectionRow.RawSectionTolerance = Convert.ToDecimal(unitRawSectionTolerance.EditValue);
        sectionRow.UseRawSectionInfo = chkUseRawSectionMaterialSpecies.Checked;
        sectionRow.UsePriceOnRawSection = chkUsePriceOnRawSection.Checked;
      }
      else if ((RawSectionType)lookUpRawSectionType.EditValue == RawSectionType.Dynamic)
      {
        sectionRow.SetIdRawSectionNull();
        sectionRow.RawH = Convert.ToDecimal(unitDynamicRawSectionH.EditValue);
        sectionRow.RawW = Convert.ToDecimal(unitDynamicRawSectionW.EditValue);
        sectionRow.UsePriceOnRawSection = chkUsePriceOnRawSection.Checked;
        sectionRow.RawSectionTolerance = Convert.ToDecimal(unitRawSectionTolerance.EditValue);
        sectionRow.UseRawSectionInfo = chkUseRawSectionMaterialSpecies.Checked;
      }
      else
      {
        sectionRow.SetIdRawSectionNull();
        sectionRow.RawSectionTolerance = 0;
        sectionRow.UseRawSectionInfo = false;
      }

      sectionRow.UseMaterialSpeciesDefinition = chkUseMaterialSpeciesDefinition.Checked;
      bool hasVariableW = chkHasVariableW.Checked;
      sectionRow.HasVariableW = hasVariableW;
      if (hasVariableW)
      {
        sectionRow.MaxW = Convert.ToDecimal(unitMaxW.EditValue);
        sectionRow.WInputTolerance = Convert.ToDecimal(unitWInputTolerance.EditValue);
      }
      else
      {
        sectionRow.MaxW = 0;
        sectionRow.WInputTolerance = 0;
      }

      sectionRow.RawSectionType = lookUpRawSectionType.EditValue.ToString();
      if (chkArcRawSection.Checked && lookUpArcRawSection.EditValue != null)
      {
        sectionRow.IdArcRawSection = (int)lookUpArcRawSection.EditValue;
        if (unitArcRawSectionTolerance.EditValue != null)
          sectionRow.ArcRawSectionTolerance = Convert.ToDecimal(unitArcRawSectionTolerance.EditValue);
        else
          sectionRow.SetArcRawSectionToleranceNull();
      }
      else
      {
        sectionRow.SetIdArcRawSectionNull();
        sectionRow.ArcRawSectionTolerance = 0;
      }

      if (sectionRow.ConvertedSectionType == SectionType.Joining || sectionRow.ConvertedSectionType == SectionType.Corner && txtCouplingAngle.EditValue != null)
      {
        sectionRow.CouplingAngle = Convert.ToDecimal(txtCouplingAngle.EditValue);
      }

      // Informatii suplimentare
      sectionRow.FixingMode = lookUpFixingMode.EditValue.ToString();
      sectionRow.UseCompensation = chkUseCompensation.Checked;
      sectionRow.HasThermalBreak = chkHasThermalBreak.Checked;
      sectionRow.HasGasket = chkHasGasket.Checked;
      sectionRow.IsCouplingAngleFixed = chkCouplingAngleFixed.Checked;
      sectionRow.TrackNumber = Convert.ToInt32(txtTrackNumber.EditValue);
      sectionRow.GenerateConsume = chkGenerateConsume.Checked;
      sectionRow.IsExtra = chkIsExtra.Checked;
      sectionRow.CuttingType = lookUpCuttingType.EditValue.ToString();
      sectionRow.CurvingMode = lookUpCurvingMode.EditValue.ToString();
      sectionRow.CoversInnerTemplates = lookUpCoversInnerTemplates.EditValue.ToString();
      if (lookUpCornerCuttingType.EditValue == null)
      {
        sectionRow.SetCornerCuttingTypeNull();
      }
      else
      {
        sectionRow.CornerCuttingType = lookUpCornerCuttingType.EditValue.ToString();
      }
      if (lookUpAltersInnerGeometry.EditValue == null)
      {
        sectionRow.SetAltersInnerGeometryNull();
      }
      else
      {
        sectionRow.AltersInnerGeometry = Convert.ToBoolean(lookUpAltersInnerGeometry.EditValue);
      }

      //Informatii Segmente
      sectionRow.ExtendingMode = lookUpExtendingMode.EditValue.ToString();
      sectionRow.MinSegmentLength = Convert.ToDecimal(unitMinSegmentLength.EditValue);
      sectionRow.MaxSegmentLength = Convert.ToDecimal(unitMaxSegmentLength.EditValue);
      sectionRow.MaxSegmentedLength = Convert.ToDecimal(unitMaxSegmentedLength.EditValue);

      // Setari optimizare
      sectionRow.IsForOptimization = chkIsForOptimization.Checked;
      sectionRow.UseDoubleCut = chkUseDoubleCut.Checked;
      sectionRow.BinSize = Convert.ToUInt32(txtBinSize.EditValue);
      sectionRow.OptimizationInventoryUseType = lookUpOptimizationInventoryUseType.EditValue.ToString();
      sectionRow.OptimizationMinLimitLength = Convert.ToDecimal(unitMinLimitLength.EditValue);
      sectionRow.OptimizationMinInventoryLength = Convert.ToDecimal(unitMinInventoryLength.EditValue);
      sectionRow.OptimizationMaxLimitLength = Convert.ToDecimal(unitMaxLimitLength.EditValue);
      sectionRow.OptimizationTargetInventory = Convert.ToUInt32(txtTargetInventory.EditValue);
      sectionRow.OptimizationMaxInventory = Convert.ToUInt32(txtMaxInventory.EditValue);

      //Informatii Transfer Termic
      sectionRow.HeatTransferCoefficient = Convert.ToDecimal(unitHeatTransferCoefficient.EditValue);

      sectionRow.WOffset = Convert.ToDecimal(unitWOffset.EditValue);
      sectionRow.TrackDistance = Convert.ToDecimal(unitTrackDistance.EditValue);
      sectionRow.GlassOffset = Convert.ToDecimal(unitGlassOffset.EditValue);
      //Informatii Parametri Fizici
      if (unitIx.EditValue == null)
        sectionRow.SetIxNull();
      else
        sectionRow.Ix = Convert.ToDecimal(unitIx.EditValue);
      if (unitIy.EditValue == null)
        sectionRow.SetIyNull();
      else
        sectionRow.Iy = Convert.ToDecimal(unitIy.EditValue);

      return true;
    }

    /// <summary>
    /// Scrie in controale proprietatile profilului
    /// </summary>
    private void WriteControls()
    {
      if (sectionRow != null)
      {
        #region Non LookUp Controls

        // Informatii generale
        txtCode.EditValue = sectionRow.Code;
        txtDesignation.EditValue = sectionRow.Designation;
        chkIsActive.Checked = sectionRow.IsActive;

        // Parametrii geometriei
        unitH1.EditValue = sectionRow.H1;
        unitH2.EditValue = sectionRow.H2;
        unitH3.EditValue = sectionRow.H3;
        unitH.EditValue = sectionRow.H;
        unitW.EditValue = sectionRow.W;
        unitW1.EditValue = sectionRow.W1;
        unitPerimter.EditValue = sectionRow.Perimeter;
        unitArea.EditValue = sectionRow.Area;

        // Imagine profil
        imgImage.EditValue = imageDS.Image.GetImage(sectionRow.Guid);
        if (!sectionRow.IsDxfNull())
        {
          ImageConverter converter = new ImageConverter();
          Bitmap bitmap = Pyramid.Ra.Material.Interface.MaterialInterfaceUtils.GetDxfImage(sectionRow.Dxf);
          if (bitmap.Width == 1 || bitmap.Height == 1)
          {
            XtraMessageBox.Show(this,
                                Properties.Resources.MsgInvalidDxf,
                                Properties.Resources.CaptionAttentionMsgBox,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
          }
          imgDxf.EditValue = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
        }
        // Informatii consum profil
        unitBarLength.EditValue = sectionRow.BarLength;
        unitUnitWeight.EditValue = MeasurableProperties.DefaultMeasurableProperties.Length.ConvertInvInternalToDisplay(sectionRow.UnitWeight);
        txtUnitWeightPrice.EditValue = sectionRow.DisplayUnitBasePrice;

        unitCuttingTolerance.EditValue = sectionRow.CuttingTolerance;
        unitBindingTolerance.EditValue = sectionRow.BindingTolerance;
        unitProcessingTolerance.EditValue = sectionRow.ProcessingTolerance;
        unitTenonTolerance.EditValue = sectionRow.TenonTolerance;
        unitDowelTolerance.EditValue = sectionRow.DowelTolerance;
        unitMullionTenonTolerance.EditValue = sectionRow.MullionTenonTolerance;
        unitMullionDowelTolerance.EditValue = sectionRow.MullionDowelTolerance;
        unitAdapterTolerance.EditValue = sectionRow.AdapterTolerance;
        unitSashExtraDimension.EditValue = sectionRow.ExtraSashDimension;

        if (sectionRow.IsFillingToleranceNull())
          unitGlassTolerance.EditValue = null;
        else
          unitGlassTolerance.EditValue = sectionRow.FillingTolerance;

        unitCurvingAddition.EditValue = sectionRow.CurvingAddition;
        unitMinRadius.EditValue = sectionRow.MinRadius;

        if (sectionRow.IsSashToleranceNull())
          unitSashTolerance.EditValue = null;
        else
          unitSashTolerance.EditValue = sectionRow.SashTolerance;

        if (sectionRow.IsNoThresholdToleranceNull())
          unitNoThresholdTolerance.EditValue = null;
        else
          unitNoThresholdTolerance.EditValue = sectionRow.NoThresholdTolerance;

        if (sectionRow.IsFoldingSash2SashToleranceNull())
          unitFoldingSash2SashTolerance.EditValue = null;
        else
          unitFoldingSash2SashTolerance.EditValue = sectionRow.FoldingSash2SashTolerance;

        if (sectionRow.IsSlideSwingToleranceNull())
          unitSlideAndSwingWingTolerance.EditValue = null;
        else
          unitSlideAndSwingWingTolerance.EditValue = sectionRow.SlideSwingTolerance;

        if (!sectionRow.IsDisplayOrderNull())
          bbTxtDisplayOrder.EditValue = sectionRow.DisplayOrder;
        else
          bbTxtDisplayOrder.EditValue = null;

        // Informatii profil brut
        bool useDefinedRawSection = sectionRow.ConvertedRawSectionType == RawSectionType.Defined;
        bool useDynamicRawSection = sectionRow.ConvertedRawSectionType == RawSectionType.Dynamic;
        if (useDefinedRawSection && sectionRow.IsIdRawSectionNull())
        {
          lookUpRawSectionType.EditValue = RawSectionType.None;
          useDefinedRawSection = false;
          useDynamicRawSection = false;
        }
        else
        {
          lookUpRawSectionType.EditValue = sectionRow.ConvertedRawSectionType;
        }
        lookUpRawSectionType.Enabled = true;
        lookUpRawSection.EditValue = sectionRow.IsIdRawSectionNull() ? (int?)null : sectionRow.IdRawSection;
        unitRawSectionTolerance.EditValue = sectionRow.RawSectionTolerance;
        unitRawSectionTolerance.Enabled = useDefinedRawSection || useDynamicRawSection;
        unitDynamicRawSectionH.Enabled = useDynamicRawSection;
        unitDynamicRawSectionH.EditValue = sectionRow.RawH;
        unitDynamicRawSectionW.Enabled = useDynamicRawSection;
        unitDynamicRawSectionW.EditValue = sectionRow.RawW;
        chkUsePriceOnRawSection.Enabled = useDynamicRawSection;
        chkUsePriceOnRawSection.Checked = sectionRow.UsePriceOnRawSection;
        chkHasVariableW.Enabled = useDynamicRawSection || sectionRow.ConvertedRawSectionType == RawSectionType.None || (RawSectionType)lookUpRawSectionType.EditValue == RawSectionType.None;
        chkHasVariableW.Checked = sectionRow.HasVariableW;
        unitMaxW.Enabled = sectionRow.HasVariableW;
        unitMaxW.EditValue = sectionRow.MaxW;
        unitWInputTolerance.Enabled = sectionRow.HasVariableW;
        unitWInputTolerance.EditValue = sectionRow.WInputTolerance;


        chkArcRawSection.EditValue = !sectionRow.IsIdArcRawSectionNull();
        if (!sectionRow.IsArcRawSectionToleranceNull())
          unitArcRawSectionTolerance.EditValue = sectionRow.ArcRawSectionTolerance;
        else
          unitArcRawSectionTolerance.EditValue = null;
        unitArcRawSectionTolerance.Enabled = chkArcRawSection.Checked;

        chkUseRawSectionMaterialSpecies.Checked = useDefinedRawSection && sectionRow.UseRawSectionInfo;
        chkUseMaterialSpeciesDefinition.Checked = !chkUseMaterialSpeciesDefinition.Checked && sectionRow.UseMaterialSpeciesDefinition;
        chkUseRawSectionMaterialSpecies.Enabled = useDefinedRawSection;
        if (!useDefinedRawSection || !(bool)chkArcRawSection.EditValue)
        {
          UpdateMaterialSpeciesRawSectionColumn(useDefinedRawSection, (bool)chkArcRawSection.EditValue);
        }

        chkUseMaterialSpeciesDefinition.Checked = sectionRow.UseMaterialSpeciesDefinition;
        chkUseMaterialSpeciesDefinition.Enabled = !chkUseRawSectionMaterialSpecies.Checked;

        // Informatii suplimentare
        chkUseCompensation.EditValue = sectionRow.UseCompensation;
        chkHasThermalBreak.EditValue = sectionRow.HasThermalBreak;
        chkHasGasket.EditValue = sectionRow.HasGasket;
        txtTrackNumber.EditValue = sectionRow.TrackNumber;
        chkGenerateConsume.EditValue = sectionRow.GenerateConsume;
        chkIsExtra.EditValue = sectionRow.IsExtra;

        if (!sectionRow.IsCouplingAngleNull())
        {
          txtCouplingAngle.EditValue = sectionRow.CouplingAngle;
        }
        else if (sectionRow.ConvertedSectionType == SectionType.Corner)
        {
          txtCouplingAngle.EditValue = Convert.ToDecimal(Angle2D._90);
        }
        else if (SectionRow.ConvertedSectionType == SectionType.Joining)
        {
          txtCouplingAngle.EditValue = Convert.ToDecimal(Angle2D._180);
        }
        else
        {
          txtCouplingAngle.EditValue = null;
        }
        if (!sectionRow.IsIsCouplingAngleFixedNull())
        {
          chkCouplingAngleFixed.Checked = sectionRow.IsCouplingAngleFixed;
        }
        else
        {
          chkCouplingAngleFixed.Checked = true;
        }
        //Informatii segment
        unitMinSegmentLength.EditValue = sectionRow.MinSegmentLength;
        unitMaxSegmentLength.EditValue = sectionRow.MaxSegmentLength;
        unitMaxSegmentedLength.EditValue = sectionRow.MaxSegmentedLength;

        // Setari optimizare
        chkIsForOptimization.Checked = sectionRow.IsForOptimization;
        chkUseDoubleCut.Checked = sectionRow.UseDoubleCut;
        txtBinSize.EditValue = sectionRow.BinSize;
        unitMinLimitLength.EditValue = sectionRow.OptimizationMinLimitLength;
        unitMinInventoryLength.EditValue = sectionRow.OptimizationMinInventoryLength;
        unitMaxLimitLength.EditValue = sectionRow.OptimizationMaxLimitLength;
        txtTargetInventory.EditValue = sectionRow.OptimizationTargetInventory;
        txtMaxInventory.EditValue = sectionRow.OptimizationMaxInventory;

        //Informatii Transfer Termic
        unitHeatTransferCoefficient.EditValue = MeasurableProperties.DefaultMeasurableProperties.ThermalTransmittance.ConvertInternalToDisplay(sectionRow.HeatTransferCoefficient);

        //Informatii Parametri Fizici
        if (sectionRow.IsIxNull())
          unitIx.EditValue = null;
        else
          unitIx.EditValue = MeasurableProperties.DefaultMeasurableProperties.MomentOfInertia.ConvertInternalToDisplay(sectionRow.Ix);
        if (sectionRow.IsIyNull())
          unitIy.EditValue = null;
        else
          unitIy.EditValue = MeasurableProperties.DefaultMeasurableProperties.MomentOfInertia.ConvertInternalToDisplay(sectionRow.Iy);
        #endregion Non LookUp Controls

        #region LookUp Controls

        // Setarile de valori in look-up pot declansa modificari de valori in alte controale.
        lookUpSectionType.EditValue = sectionRow.ConvertedSectionType;
        lookUpMaterialType.EditValue = sectionRow.ConvertedMaterialType;
        flagsKnownSide.EditValue = sectionRow.ConvertedKnownSide;
        lookUpCurrency.EditValue = sectionRow.Currency;
        lookUpPriceComputeMode.EditValue = sectionRow.ConvertedPriceCalculationType;

        lookUpConsumeGroup.EditValue = sectionRow.IdConsumeGroup;

        gridLookUpDefaultReinforcement.EditValue = sectionRow.IsIdReinforcementNull() ? (int?)null : sectionRow.IdReinforcement;
        lookUpArcRawSection.EditValue = sectionRow.IsIdArcRawSectionNull() ? (int?)null : sectionRow.IdArcRawSection;
        lookUpFixingMode.EditValue = sectionRow.ConvertedFixingMode;
        lookUpExtendingMode.EditValue = sectionRow.ConvertedExtendingMode;
        lookUpCornerCuttingType.EditValue = sectionRow.ConvertedCornerCuttingType;
        lookUpAltersInnerGeometry.EditValue = sectionRow.IsAltersInnerGeometryNull() ? (bool?)null : sectionRow.AltersInnerGeometry;
        lookUpOptimizationInventoryUseType.EditValue = sectionRow.ConvertedOptimizationInventoryUseType;
        lookUpCuttingType.EditValue = sectionRow.ConvertedCuttingType;
        lookUpCurvingMode.EditValue = sectionRow.ConvertedCurvingMode;
        lookUpCoversInnerTemplates.EditValue = sectionRow.ConvertedCoversInnerTemplates;
        lookUpMaterialSpeciesUsage.EditValue = sectionRow.ConvertedMaterialSpeciesUsage;
        unitTrackDistance.EditValue = sectionRow.IsTrackDistanceNull() ? 0 : sectionRow.TrackDistance;
        unitWOffset.EditValue = sectionRow.IsWOffsetNull() ? 0 : sectionRow.WOffset;
        unitGlassOffset.EditValue = sectionRow.IsGlassOffsetNull() ? 0 : sectionRow.GlassOffset;

        #endregion LookUp Controls
      }
    }

    /// <summary>
    /// Genereaza un rand penru o noua sectiune pe baza DS-urilor specificate.
    /// </summary>
    /// <param name="sectionDS">Setul de date cu sectiuni.</param>
    /// <param name="consumeGroupDS">Setul de date cu grupuri de consum.</param>
    /// <param name="currencyDS">Setul de date cu valute.</param>
    private static SectionDataSet.SectionRow GenerateDefaultRow(SectionDataSet sectionDS,
                                                                ConsumeGroupDataSet consumeGroupDS,
                                                                CurrencyDataSet currencyDS)
    {
      SectionDataSet.SectionRow sectionRow = sectionDS.Section.NewSectionRow();
      sectionRow.Guid = Guid.NewGuid();
      sectionRow.Code = String.Empty;
      sectionRow.Designation = String.Empty;
      sectionRow.HeatTransferCoefficient = 0;
      sectionRow.ConvertedCurvingMode = CurvingMode.Bending;
      sectionRow.ConvertedCoversInnerTemplates = ViewSide.Unknown;
      sectionRow.UseRawSectionInfo = false;
      sectionRow.UseMaterialSpeciesDefinition = false;
      sectionRow.ConvertedRawSectionType = RawSectionType.None;
      sectionRow.RawH = 0;
      sectionRow.RawW = 0;
      sectionRow.UsePriceOnRawSection = false;
      sectionRow.HasVariableW = false;
      sectionRow.MaxW = 0;
      sectionRow.WInputTolerance = 0;
      sectionRow.IsActive = true;

      // in mod default se ia primul tip de profil in ordine alfabetica
      ArrayList sectionTypes = EnumTypeLocalizer.Localize(typeof(SectionType));
      sectionTypes.Sort();
      LocalizedPair pair = sectionTypes[0] as LocalizedPair;
      sectionRow.SectionType = pair.OriginalName;
      sectionRow.ConvertedKnownSide = KnownSide.All;

      MaterialType defaultMaterialType = Properties.Settings.Default.DefaultMaterialType;
      SetDefaultValuesOnMaterialType(sectionRow, defaultMaterialType);

      // se ia primul grup de consum gasit alfabetic ca default
      consumeGroupDS.ConsumeGroup.DefaultView.Sort = consumeGroupDS.ConsumeGroup.DesignationColumn.ColumnName;
      sectionRow.IdConsumeGroup = ((ConsumeGroupDataSet.ConsumeGroupRow)consumeGroupDS.ConsumeGroup.DefaultView[0].Row).Id;

      // valuta default e cea implicita
      currencyDS.Currency.DefaultView.RowFilter = string.Format("{0} = {1}",
                  currencyDS.Currency.IsDefaultColumn.ColumnName,
                  true);
      if (currencyDS.Currency.DefaultView.Count > 0)
      {
        sectionRow.Currency = ((CurrencyDataSet.CurrencyRow)currencyDS.Currency.DefaultView[0].Row).ShortName;
        sectionRow.DefaultCurrency = ((CurrencyDataSet.CurrencyRow)currencyDS.Currency.DefaultView[0].Row).ShortName;
      }
      else
      {
        sectionRow.Currency = currencyDS.Currency[0].ShortName;
      }
      currencyDS.Currency.DefaultView.RowFilter = string.Empty;

      sectionRow.UseCompensation = false;
      sectionRow.OptimizationInventoryUseType = OptimizationInventoryUseType.NoInventory.ToString();
      sectionRow.IsExtra = false;

      sectionRow.ConvertedCuttingType = CuttingType.StraightCut;
      sectionRow.ConvertedExtendingMode = ExtendingMode.None;
      sectionRow.MinSegmentLength = 0;
      sectionRow.MaxSegmentLength = sectionRow.BarLength - sectionRow.CuttingTolerance;
      sectionRow.MaxSegmentedLength = sectionRow.BarLength;

      sectionRow.MinRadius = 0;
      sectionRow.MaterialSpeciesUsage = MaterialSpeciesUsage.Main.ToString();
      return sectionRow;
    }

    /// <summary>
    /// Seteaza valorile default pe un profil in functie de tipul de material.
    /// </summary>
    /// <param name="sectionRow">Randul pe care se seteaza valorile default.</param>
    /// <param name="materialType">Tipul de material.</param>
    private static void SetDefaultValuesOnMaterialType(SectionDataSet.SectionRow sectionRow, MaterialType materialType)
    {
      sectionRow.MaterialType = materialType.ToString();

      switch (materialType)
      {
        case MaterialType.Aluminium:
          {
            sectionRow.BarLength = Properties.Settings.Default.SectionDefaultBarLengthAluminium;
            sectionRow.BindingTolerance = Properties.Settings.Default.SectionDefaultBindingToleranceAluminium;
            sectionRow.CuttingTolerance = Properties.Settings.Default.SectionDefaultCuttingToleranceAluminium;
            sectionRow.ProcessingTolerance = Properties.Settings.Default.SectionDefaultProcessingToleranceAluminium;
            sectionRow.CurvingAddition = Properties.Settings.Default.SectionDefaultCurvingAdditionAluminium;
            sectionRow.PriceCalculationType = PriceCalculationType.PerWeight.ToString();

            sectionRow.HasThermalBreak = true;

            break;
          }
        case MaterialType.PVC:
          {
            sectionRow.BarLength = Properties.Settings.Default.SectionDefaultBarLengthPVC;
            sectionRow.BindingTolerance = Properties.Settings.Default.SectionDefaultBindingTolerancePVC;
            sectionRow.CuttingTolerance = Properties.Settings.Default.SectionDefaultCuttingTolerancePVC;
            sectionRow.ProcessingTolerance = Properties.Settings.Default.SectionDefaultProcessingTolerancePVC;
            sectionRow.CurvingAddition = Properties.Settings.Default.SectionDefaultCurvingAdditionPVC;
            sectionRow.PriceCalculationType = PriceCalculationType.PerLength.ToString();

            sectionRow.HasThermalBreak = false;

            break;
          }
        case MaterialType.Wood:
          {
            sectionRow.BarLength = Properties.Settings.Default.SectionDefaultBarLengthWood;
            sectionRow.BindingTolerance = Properties.Settings.Default.SectionDefaultBindingToleranceWood;
            sectionRow.CuttingTolerance = Properties.Settings.Default.SectionDefaultCuttingToleranceWood;
            sectionRow.ProcessingTolerance = Properties.Settings.Default.SectionDefaultProcessingToleranceWood;
            sectionRow.CurvingAddition = Properties.Settings.Default.SectionDefaultCurvingAdditionWood;
            sectionRow.PriceCalculationType = PriceCalculationType.PerLength.ToString();
            sectionRow.CurvingMode = CurvingMode.Milling.ToString();
            sectionRow.HasThermalBreak = false;

            break;
          }
        case MaterialType.Steel:
          {
            sectionRow.BarLength = Properties.Settings.Default.SectionDefaultBarLengthSteel;
            sectionRow.BindingTolerance = Properties.Settings.Default.SectionDefaultBindingToleranceSteel;
            sectionRow.CuttingTolerance = Properties.Settings.Default.SectionDefaultCuttingToleranceSteel;
            sectionRow.ProcessingTolerance = Properties.Settings.Default.SectionDefaultProcessingToleranceSteel;
            sectionRow.CurvingAddition = Properties.Settings.Default.SectionDefaultCurvingAdditionSteel;
            sectionRow.PriceCalculationType = PriceCalculationType.PerLength.ToString();

            sectionRow.HasThermalBreak = true;

            break;
          }
        default:
          {
            Debug.Assert(false, "Tip necunoscut de material");
            break;
          }
      }
    }

    /// <summary>
    /// Verifica daca sunt valide datele din controalele referitoare la profil
    /// </summary>
    /// <returns>TRUE daca datele sunt valide, FALSE altfel</returns>
    private bool ValidateControls()
    {
      string message = string.Empty;
      MessageBoxButtons buttons = MessageBoxButtons.OK;

      if (string.IsNullOrEmpty(txtCode.Text))
      {
        message = Properties.Resources.MsgErrorCodeNull;
      }
      else if (sectionDS.Section.FindByCode(txtCode.Text) != null && txtCode.Text != sectionRow.Code)
      {
        message = string.Format(Properties.Resources.MsgErrorCodeNotUnique, txtCode.Text);
      }
      else if (string.IsNullOrEmpty(txtDesignation.Text))
      {
        message = Properties.Resources.MsgErrorDesignationNull;
      }
      else if (lookUpSectionType.EditValue == null)
      {
        message = Properties.Resources.MsgErrorSectionTypeNull;
      }
      else if (lookUpMaterialType.EditValue == null)
      {
        message = Properties.Resources.MsgErrorSeriesMaterialTypeNull;
      }
      else if (lookUpPriceComputeMode.EditValue == null)
      {
        message = Properties.Resources.MsgErrorPriceComputeModeTypeNull;
      }
      else if (lookUpCurrency.EditValue == null)
      {
        message = Properties.Resources.MsgErrorCurrencyNull;
      }
      else if (lookUpConsumeGroup.EditValue == null)
      {
        message = Properties.Resources.MsgErrorConsumeGroupNull;
      }
      else if (Convert.ToDecimal(unitMinLimitLength.EditValue) < 0 ||
        Convert.ToDecimal(unitMinInventoryLength.EditValue) < Convert.ToDecimal(unitMinLimitLength.EditValue) ||
        Convert.ToDecimal(unitMaxLimitLength.EditValue) < Convert.ToDecimal(unitMinInventoryLength.EditValue) ||
        Convert.ToDecimal(unitMaxLimitLength.EditValue) > Convert.ToDecimal(unitBarLength.EditValue))
      {
        message = Properties.Resources.MsgErrorOptimizationSegmentLengthInvalid;
      }
      else if (Convert.ToDecimal(unitMinSegmentLength.EditValue) < 0 ||
        Convert.ToDecimal(unitMaxSegmentLength.EditValue) > Convert.ToDecimal(unitMaxSegmentedLength.EditValue) ||
        Convert.ToDecimal(unitMaxSegmentLength.EditValue) < 2 * (Convert.ToDecimal(unitMinSegmentLength.EditValue)))
      {
        message = Properties.Resources.MsgErrorSegmentLengthInvalid;
      }
      else if (Convert.ToInt32(txtTargetInventory.EditValue) < 0 ||
        Convert.ToUInt32(txtMaxInventory.EditValue) < Convert.ToUInt32(txtTargetInventory.EditValue))
      {
        message = Properties.Resources.MsgErrorOptimizationInventoryLimitInvalid;
      }
      else if (!CheckReinforcementsValidity())
      {
        message = Properties.Resources.MsgErrorSectionReinforcementInvalid;
        buttons = MessageBoxButtons.YesNoCancel;
      }
      else if (!CheckRawSectionValidity())
      {
        message = Properties.Resources.MsgErrorSectionRawSectionInvalid;
        buttons = MessageBoxButtons.YesNoCancel;
      }
      else if (!CheckCustomItemsValidity())
      {
        message = Properties.Resources.MsgErrorSectionCustomItemInvalid;
        buttons = MessageBoxButtons.YesNoCancel;
      }
      //else if (!CheckSHTValidity())
      //{
      //  message = Properties.Resources.MsgErrorSectionSHTItemInvalid;
      //  buttons = MessageBoxButtons.YesNoCancel;
      //}
      //else if (!CheckToleranceValidity())
      //{
      //  message = Properties.Resources.MsgErrorSectionToleranceItemInvalid;
      //  buttons = MessageBoxButtons.YesNoCancel;
      //}
      else if (!CheckCoverValidity())
      {
        message = Properties.Resources.MsgErrorSectionCoverInvalid;
        buttons = MessageBoxButtons.YesNoCancel;
      }
      else
      {
        return true;
      }

      DialogResult result = XtraMessageBox.Show(this, message,
                              Properties.Resources.CaptionAttentionMsgBox,
                              buttons,
                              MessageBoxIcon.Exclamation);
      switch (result)
      {
        case DialogResult.OK:
          return false;
        case DialogResult.Cancel:
          return false;
        case DialogResult.Yes:
          AddRequiredSections(sectionRow.Id);
          return true;
        case DialogResult.No:
          this.DialogResult = DialogResult.Cancel;
          return true;
        default:
          break;
      }
      return false;
    }


    /// <summary>
    /// Actualizeaza starea controalelor in functie de modul de deschidere al formei.
    /// </summary>
    private void UpdateReadOnlyState()
    {
      bool readOnly = openingMode != FormOpeningMode.Normal;

      if (openingMode == FormOpeningMode.Locked)
      {
        DisableChildEditors();
      }
      else
      {
        // setam daca view-urile pot sau nu sa creeze randuri noi.
        NewItemRowPosition newItemRowPosition = readOnly ? NewItemRowPosition.None : NewItemRowPosition.Top;
        viewSeries.OptionsView.NewItemRowPosition = newItemRowPosition;
        viewCustomSectionItem.OptionsView.NewItemRowPosition = newItemRowPosition;
        viewSectionCover.OptionsView.NewItemRowPosition = newItemRowPosition;

        foreach (GridColumn column in viewSeries.Columns)
        {
          // profilul implicit se poate modifica tot timpul
          if (column == colSeriesPriority)
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

        if (openingMode == FormOpeningMode.LimitedReadOnly)
        {
          foreach (GridColumn column in viewSectionColorList.Columns)
          {
            // profilul implicit se poate modifica tot timpul
            if (column == colPrice || column == colPriceCalculationType || column == colListIdCostGroup || column == colEditingMode)
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

          foreach (GridColumn column in viewColorListItem.Columns)
          {
            // profilul implicit se poate modifica tot timpul
            if (column == colColorListItemPrice || column == colCostGroupId)
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

          foreach (GridColumn column in viewMaterialSpecies.Columns)
          {
            // profilul implicit se poate modifica tot timpul
            if (column == colMaterialSpeciesPrice)
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
        }
        else
        {
          viewSectionColorList.OptionsBehavior.Editable = !readOnly;
          viewColorListItem.OptionsBehavior.Editable = !readOnly;
          viewMaterialSpecies.OptionsBehavior.Editable = !readOnly;
          viewReinforcements.OptionsBehavior.Editable = !readOnly;
        }

        viewCustomSectionItem.OptionsBehavior.Editable = !readOnly;
        viewSectionCover.OptionsBehavior.Editable = !readOnly;
        viewTolerance.OptionsBehavior.Editable = !readOnly;
        viewSectionHeatTransferCoefficient.OptionsBehavior.Editable = !readOnly;

        // setam modul controalelor.
        txtCode.Properties.ReadOnly = readOnly;
        txtDesignation.Properties.ReadOnly = readOnly;
        lookUpSectionType.Properties.ReadOnly = readOnly;
        flagsKnownSide.Properties.ReadOnly = readOnly;
        lookUpMaterialType.Properties.ReadOnly = readOnly;
        bbTxtDisplayOrder.Properties.ReadOnly = readOnly;

        cmdOpenImage.Enabled = !readOnly;
        cmdDeleteImage.Enabled = !readOnly;
        imgImage.Properties.ShowMenu = !readOnly;
        imgDxf.Properties.ShowMenu = !readOnly;
        unitH.Properties.ReadOnly = readOnly;
        unitH1.Properties.ReadOnly = readOnly;
        unitH2.Properties.ReadOnly = readOnly;
        unitH3.Properties.ReadOnly = readOnly;
        unitW.Properties.ReadOnly = readOnly;
        unitW1.Properties.ReadOnly = readOnly;
        unitPerimter.Properties.ReadOnly = readOnly;
        unitArea.Properties.ReadOnly = readOnly;
        imgSchema.Properties.ShowMenu = false;

        lookUpRawSectionType.Properties.ReadOnly = readOnly;
        unitMaxW.Properties.ReadOnly = readOnly;
        unitWInputTolerance.Properties.ReadOnly = readOnly;

        unitBarLength.Properties.ReadOnly = readOnly;
        unitUnitWeight.Properties.ReadOnly = readOnly;
        lookUpConsumeGroup.Properties.ReadOnly = readOnly;

        gridLookUpDefaultReinforcement.Properties.ReadOnly = readOnly;

        lookUpFixingMode.Properties.ReadOnly = readOnly;
        chkUseCompensation.Properties.ReadOnly = readOnly;
        chkHasThermalBreak.Properties.ReadOnly = readOnly;
        chkHasGasket.Properties.ReadOnly = readOnly;
        chkIsActive.Properties.ReadOnly = readOnly;
        chkCouplingAngleFixed.Properties.ReadOnly = readOnly;
        chkGenerateConsume.Properties.ReadOnly = readOnly;
        lookUpCuttingType.Properties.ReadOnly = readOnly;
        lookUpCurvingMode.Properties.ReadOnly = readOnly;
        lookUpCornerCuttingType.Properties.ReadOnly = readOnly;
        lookUpAltersInnerGeometry.Properties.ReadOnly = readOnly;
        lookUpCoversInnerTemplates.Properties.ReadOnly = readOnly;
        chkIsExtra.Properties.ReadOnly = readOnly;
        unitHeatTransferCoefficient.Properties.ReadOnly = readOnly;
        chkUseRawSectionMaterialSpecies.Properties.ReadOnly = readOnly;
        chkUseMaterialSpeciesDefinition.Properties.ReadOnly = readOnly;
        unitIx.Properties.ReadOnly = readOnly;
        unitIy.Properties.ReadOnly = readOnly;
        txtCouplingAngle.Properties.ReadOnly = readOnly;

        //informatii segment
        lookUpExtendingMode.Properties.ReadOnly = readOnly;
        unitMinSegmentLength.Properties.ReadOnly = readOnly;
        unitMaxSegmentLength.Properties.ReadOnly = readOnly;
        unitMaxSegmentedLength.Properties.ReadOnly = readOnly;

        // preturile se pot modifica in modul limited
        lookUpPriceComputeMode.Properties.ReadOnly = readOnly && openingMode != FormOpeningMode.LimitedReadOnly;
        lookUpCurrency.Properties.ReadOnly = readOnly && openingMode != FormOpeningMode.LimitedReadOnly;
        txtUnitWeightPrice.Properties.ReadOnly = readOnly && openingMode != FormOpeningMode.LimitedReadOnly;

        // tolerantele tot timpul se pot modifica
        unitCuttingTolerance.Properties.ReadOnly = false;
        unitBindingTolerance.Properties.ReadOnly = false;
        unitProcessingTolerance.Properties.ReadOnly = false;
        unitCurvingAddition.Properties.ReadOnly = false;
        unitSashTolerance.Properties.ReadOnly = false;
        unitNoThresholdTolerance.Properties.ReadOnly = false;
        unitMinRadius.Properties.ReadOnly = false;
        unitFoldingSash2SashTolerance.Properties.ReadOnly = false;
        unitSlideAndSwingWingTolerance.Properties.ReadOnly = false;
        unitSashExtraDimension.Properties.ReadOnly = false;

        // Setarile de optimizare sunt tot timpul editabile.
        chkIsForOptimization.Properties.ReadOnly = false;
        chkUseDoubleCut.Properties.ReadOnly = false;
        txtBinSize.Properties.ReadOnly = false;

        lookUpOptimizationInventoryUseType.Properties.ReadOnly = false;
        unitMinLimitLength.Properties.ReadOnly = false;
        unitMinInventoryLength.Properties.ReadOnly = false;
        unitMaxLimitLength.Properties.ReadOnly = false;
        txtTargetInventory.Properties.ReadOnly = false;
        txtMaxInventory.Properties.ReadOnly = false;
      }

      if (!isAdmin)
        chkIsActive.Properties.ReadOnly = true;

      if (!Session.CurrentSession.OptionsValidator.SlicesAccess)
      {
        tabCAD.PageVisible = false;
      }
    }

    /// <summary>
    /// Obtine din resurse imaginea profilului avand parametrii dati
    /// </summary>
    /// <param name="sectionType">Tipul de profil</param>
    /// <param name="materialType">Tipul de material</param>
    /// <returns>Imaginea profilului</returns>
    private static Image GetImage(SectionType sectionType, MaterialType materialType)
    {
      switch (materialType)
      {
        #region Aluminiu

        case MaterialType.Aluminium:
          switch (sectionType)
          {
            case SectionType.Frame: return Properties.Resources.ImgAl_Frame;
            case SectionType.SashWindowInt: return Properties.Resources.ImgAl_SashWindowInt;
            case SectionType.SashAdaptor: return Properties.Resources.ImgAl_Adaptor;
            case SectionType.SashAdaptorInsectScreen: return Properties.Resources.ImgAl_AdaptorInsectScreen;
            case SectionType.Mullion: return Properties.Resources.ImgAl_Mullion;
            case SectionType.GlazingBead: return Properties.Resources.ImgAl_GlazingBead;
            case SectionType.Corner: return Properties.Resources.ImgAl_Corner;
            case SectionType.Joining: return Properties.Resources.ImgAl_Join;
            case SectionType.HookWindow: return Properties.Resources.ImgAl_ClipsWindow;
            case SectionType.HookDoor: return Properties.Resources.ImgAl_ClipsDoor;
            case SectionType.HookInsectScreen: return Properties.Resources.ImgAl_ClipsInsectScreen;
            case SectionType.Reinforcement: return Properties.Resources.ImgAl_Reinforcement;
            case SectionType.Custom: return Properties.Resources.ImgAl_Custom;
            case SectionType.LedgeInt: return Properties.Resources.ImgAl_Ledge;
            case SectionType.LedgeExt: return Properties.Resources.ImgAl_Ledge;
            case SectionType.FrameExpansion: return Properties.Resources.ImgAl_Extension;
            case SectionType.WaterDrip: return Properties.Resources.ImgAl_Drip;
            case SectionType.SashDoorInt: return Properties.Resources.ImgAl_SashDoorInt;
            case SectionType.SashDoorExt: return Properties.Resources.ImgAl_SashDoorExt;
            case SectionType.SashSlidingWindow: return Properties.Resources.ImgAl_SlideSashWindow;
            case SectionType.SashSlidingDoor: return Properties.Resources.ImgAl_SlideSashDoor;
            case SectionType.Track: return Properties.Resources.ImgAl_Track;
            case SectionType.Threshold: return Properties.Resources.ImgAl_Threshold;
            case SectionType.BottomRail: return Properties.Resources.ImgAl_BottomRail;
            case SectionType.Compensation: return Properties.Resources.ImgAl_Compensation;
            case SectionType.Carving: return Properties.Resources.ImgAl_Carving;
            case SectionType.SurfaceAdapter: return Properties.Resources.ImgAl_CarvingAdapter;
            case SectionType.PivotAdapter: return Properties.Resources.ImgAl_PivotAdapter;
            case SectionType.GeorgianBarOutside: return Properties.Resources.ImgAl_GeorgianBarOutside;
            case SectionType.GeorgianBarInside: return Properties.Resources.ImgAl_GeorgianBarInside;
            case SectionType.AdjoiningRollerInsectScreen: return Properties.Resources.ImgAl_RollerInsectScreenAdapter;
            case SectionType.AdjoiningRollerShutter: return Properties.Resources.ImgAl_RollerShutterAdapter;
            case SectionType.BeadInsectScreen: return Properties.Resources.ImgAl_InsectScreenGlazingBead;
            case SectionType.SashSlidingInsectScreen: return Properties.Resources.ImgAl_InsectScreenSlidingSash;
            case SectionType.SashPivot: return Properties.Resources.ImgAl_PivotSash;
            case SectionType.SashInsectScreen: return Properties.Resources.ImgAl_InsectScreenSash;
            case SectionType.MullionInsectScreen: return Properties.Resources.ImgAl_InsectScreenMullion;
            case SectionType.HousingRollerShutter: return Properties.Resources.ImgAl_RollingShutterRoller;
            case SectionType.HousingRollerInsectScreen: return Properties.Resources.ImgAl_RollingInsectScreenRoller;
            case SectionType.TrackRollerShutter: return Properties.Resources.ImgAl_RollingShutterTrack;
            case SectionType.TrackRollerInsectScreen: return Properties.Resources.ImgAl_RollingInsectScreenTrack;
            case SectionType.TrackInsectScreen: return Properties.Resources.ImgAl_InsectScreenTrack;
            case SectionType.TerminalRollerShutter: return Properties.Resources.ImgAl_RollerShutterTerminal;
            case SectionType.TerminalRollerInsectScreen: return Properties.Resources.ImgAl_RollerInsectScreenTerminal;
            case SectionType.FrameInsectScreen: return Properties.Resources.ImgAl_InsectScreenFrame;
            case SectionType.TrackJambVerticalSlide: return Properties.Resources.ImgAl_TrackJambVerticalSlide;
            case SectionType.TrackHeadVerticalSlide: return Properties.Resources.ImgAl_TrackHeadVerticalSlide;
            case SectionType.TrackSillVerticalSlide: return Properties.Resources.ImgAl_TrackSillVerticalSlide;
            case SectionType.SashVerticalSlide: return Properties.Resources.ImgAl_SashVerticalSlide;
            case SectionType.BottomRailVerticalSlide: return Properties.Resources.ImgAl_BottomRailVerticalSlide;
            case SectionType.SashWithHookVerticalSlide: return Properties.Resources.ImgAl_SashWithHookVerticalSlide;
            case SectionType.HookVerticalSlide: return Properties.Resources.ImgAl_HookVerticalSlide;
            case SectionType.Cover: return Properties.Resources.ImgAl_Cover;
            case SectionType.SashWindowExt: return Properties.Resources.ImgAl_SashWindowExt;
            case SectionType.RawSection: return Properties.Resources.ImgAl_RawSection;
            case SectionType.MullionWithTrack: return Properties.Resources.ImgAl_MullionWithTrack;
            case SectionType.MullionWithHook: return Properties.Resources.ImgAl_MullionWithHook;
            case SectionType.SashWithHookSlide: return Properties.Resources.ImgAl_SashWithHookSlide;
            case SectionType.SashWithHookSlideDoor: return Properties.Resources.ImgAl_SashWithHookSlide;
            case SectionType.SashWithAdapterSlide: return Properties.Resources.ImgAl_SashWithAdapterSlide;
            case SectionType.TrackCover: return Properties.Resources.ImgAl_TrackCover;
            case SectionType.TubeRollerShutter: return Properties.Resources.ImgAl_TubeRollerShutter;
            case SectionType.TrackMullionRollerShutter: return Properties.Resources.ImgAl_GuideMullionRollerShutter;
            case SectionType.SashSlideAndSwing: return Properties.Resources.ImgAl_SashSlideAndSwing;
            case SectionType.SashWithAdapterSlideSldSw: return Properties.Resources.ImgAl_SashWithAdapterSlideSldSw;
            case SectionType.SashWithAdapterSwingSldSw: return Properties.Resources.ImgAl_SashWithAdapterSwingSldSw;
            case SectionType.AdapterSlideSldSw: return Properties.Resources.ImgAl_AdapterSlideSldSw;
            case SectionType.AdapterSwingSldSw: return Properties.Resources.ImgAl_AdapterSwingSldSw;
            case SectionType.AdapterSlideWindowDoor: return Properties.Resources.ImgAl_AdapterSlideWindowDoor;
            //case SectionType.SashMale: return Properties.Resources.ImgAl_SashWithAdapterSwingSldSw;
            //case SectionType.SashFemale: return Properties.Resources.ImgAl_SashWithAdapterSwingSldSw;
            default:
              Debug.Assert(false, "Tip de profil incompatibil in FrmSectionEdit.GetImage()");
              break;
          }
          break;

        #endregion Aluminiu

        #region PVC

        case MaterialType.PVC:
          switch (sectionType)
          {
            case SectionType.Frame: return Properties.Resources.ImgPVC_Frame;
            case SectionType.SashWindowInt: return Properties.Resources.ImgPVC_SashWindowInt;
            case SectionType.SashAdaptor: return Properties.Resources.ImgPVC_Adaptor;
            case SectionType.SashAdaptorInsectScreen: return Properties.Resources.ImgPVC_AdaptorInsectScreen;
            case SectionType.Mullion: return Properties.Resources.ImgPVC_Mullion;
            case SectionType.GlazingBead: return Properties.Resources.ImgPVC_GlazingBead;
            case SectionType.Corner: return Properties.Resources.ImgPVC_Corner;
            case SectionType.Joining: return Properties.Resources.ImgPVC_Join;
            case SectionType.HookWindow: return Properties.Resources.ImgPVC_ClipsWindow;
            case SectionType.HookDoor: return Properties.Resources.imgPVC_ClipsDoor;
            case SectionType.HookInsectScreen: return Properties.Resources.ImgPVC_ClipsInsectScreen;
            case SectionType.Reinforcement: return Properties.Resources.ImgPVC_Reinforcement;
            case SectionType.Custom: return Properties.Resources.ImgPVC_Custom;
            case SectionType.LedgeInt: return Properties.Resources.ImgPVC_Ledge;
            case SectionType.LedgeExt: return Properties.Resources.ImgPVC_Ledge;
            case SectionType.FrameExpansion: return Properties.Resources.ImgPVC_Extension;
            case SectionType.WaterDrip: return Properties.Resources.ImgPVC_Drip;
            case SectionType.SashDoorInt: return Properties.Resources.ImgPVC_SashDoorInt;
            case SectionType.SashDoorExt: return Properties.Resources.ImgPVC_SashDoorExt;
            case SectionType.SashSlidingWindow: return Properties.Resources.ImgPVC_SlideSashWindow;
            case SectionType.SashSlidingDoor: return Properties.Resources.ImgPVC_SlideSashDoor;
            case SectionType.Track: return Properties.Resources.ImgPVC_Track;
            case SectionType.Threshold: return Properties.Resources.ImgPVC_Threshold;
            case SectionType.BottomRail: return Properties.Resources.ImgPVC_BottomRail;
            case SectionType.Compensation: return Properties.Resources.ImgPVC_Compensation;
            case SectionType.Carving: return Properties.Resources.ImgPVC_Carving;
            case SectionType.SurfaceAdapter: return Properties.Resources.ImgPVC_CarvingAdapter;
            case SectionType.PivotAdapter: return Properties.Resources.ImgPVC_PivotAdapter;
            case SectionType.GeorgianBarOutside: return Properties.Resources.ImgPVC_GeorgianBarOutside;
            case SectionType.GeorgianBarInside: return Properties.Resources.ImgPVC_GeorgianBarInside;
            case SectionType.AdjoiningRollerInsectScreen: return Properties.Resources.ImgPVC_RollerInsectScreenAdapter;
            case SectionType.AdjoiningRollerShutter: return Properties.Resources.ImgPVC_RollerShutterAdapter;
            case SectionType.BeadInsectScreen: return Properties.Resources.ImgPVC_InsectScreenGlazingBead;
            case SectionType.SashSlidingInsectScreen: return Properties.Resources.ImgPVC_InsectScreenSlidingSash;
            case SectionType.SashPivot: return Properties.Resources.ImgPVC_PivotSash;
            case SectionType.SashInsectScreen: return Properties.Resources.ImgPVC_InsectScreenSash;
            case SectionType.MullionInsectScreen: return Properties.Resources.ImgPVC_InsectScreenMullion;
            case SectionType.HousingRollerShutter: return Properties.Resources.ImgPVC_RollingShutterRoller;
            case SectionType.HousingRollerInsectScreen: return Properties.Resources.ImgPVC_RollingInsectScreenRoller;
            case SectionType.TrackRollerShutter: return Properties.Resources.ImgPVC_RollingShutterTrack;
            case SectionType.TrackRollerInsectScreen: return Properties.Resources.ImgPVC_RollingInsectScreenTrack;
            case SectionType.TrackInsectScreen: return Properties.Resources.ImgPVC_InsectScreenTrack;
            case SectionType.TerminalRollerShutter: return Properties.Resources.ImgPVC_RollerShutterTerminal;
            case SectionType.TerminalRollerInsectScreen: return Properties.Resources.ImgPVC_RollerInsectScreenTerminal;
            case SectionType.FrameInsectScreen: return Properties.Resources.ImgPVC_InsectScreenFrame;
            case SectionType.TrackJambVerticalSlide: return Properties.Resources.ImgPVC_TrackJambVerticalSlide;
            case SectionType.TrackHeadVerticalSlide: return Properties.Resources.ImgPVC_TrackHeadVerticalSlide;
            case SectionType.TrackSillVerticalSlide: return Properties.Resources.ImgPVC_TrackSillVerticalSlide;
            case SectionType.SashVerticalSlide: return Properties.Resources.ImgPVC_SashVerticalSlide;
            case SectionType.BottomRailVerticalSlide: return Properties.Resources.ImgPVC_BottomRailVerticalSlide;
            case SectionType.SashWithHookVerticalSlide: return Properties.Resources.ImgPVC_SashWithHookVerticalSlide;
            case SectionType.HookVerticalSlide: return Properties.Resources.ImgPVC_HookVerticalSlide;
            case SectionType.Cover: return Properties.Resources.ImgPVC_Cover;
            case SectionType.SashWindowExt: return Properties.Resources.ImgPVC_SashWindowExt;
            case SectionType.RawSection: return Properties.Resources.ImgPVC_RawSection;
            case SectionType.MullionWithTrack: return Properties.Resources.ImgPVC_MullionWithTrack;
            case SectionType.MullionWithHook: return Properties.Resources.ImgPVC_MullionWithHook;
            case SectionType.SashWithHookSlide: return Properties.Resources.ImgPVC_SashWithHookSlide;
            case SectionType.SashWithHookSlideDoor: return Properties.Resources.ImgPVC_SashWithHookSlide;
            case SectionType.SashWithAdapterSlide: return Properties.Resources.ImgPVC_SashWithAdapterSlide;
            case SectionType.TrackCover: return Properties.Resources.ImgPVC_TrackCover;
            case SectionType.SashWithAdapter: return Properties.Resources.ImgPVC_SashWithAdapter;
            case SectionType.SubSill: return Properties.Resources.ImgPVC_SubSill;
            case SectionType.TubeRollerShutter: return Properties.Resources.ImgPVC_TubeRollerShutter;
            case SectionType.TrackMullionRollerShutter: return Properties.Resources.ImgPVC_GuideMullionRollerShutter;
            case SectionType.SashSlideAndSwing: return Properties.Resources.ImgPVC_SashSlideAndSwing;
            case SectionType.SashWithAdapterSlideSldSw: return Properties.Resources.ImgPVC_SashWithAdapterSlideSldSw;
            case SectionType.SashWithAdapterSwingSldSw: return Properties.Resources.ImgPVC_SashWithAdapterSwingSldSw;
            case SectionType.AdapterSlideSldSw: return Properties.Resources.ImgPVC_AdapterSlideSldSw;
            case SectionType.AdapterSwingSldSw: return Properties.Resources.ImgPVC_AdapterSwingSldSw;
            case SectionType.AdapterSlideWindowDoor: return Properties.Resources.ImgPVC_AdapterSlideWindowDoor;
            //case SectionType.SashMale: return Properties.Resources.ImgPVC_SashWithAdapterSwingSldSw;
            //case SectionType.SashFemale: return Properties.Resources.ImgPVC_SashWithAdapterSwingSldSw;
            default:
              Debug.Assert(false, "Tip de profil incompatibil in FrmSectionEdit.GetImage()");
              break;
          }
          break;

        #endregion PVC

        #region Wood

        case MaterialType.Wood:
          switch (sectionType)
          {
            case SectionType.Frame: return Properties.Resources.ImgWood_Frame;
            case SectionType.SashWindowInt: return Properties.Resources.ImgWood_SashWindowInt;
            case SectionType.SashAdaptor: return Properties.Resources.ImgWood_Adaptor;
            case SectionType.SashAdaptorInsectScreen: return Properties.Resources.ImgWood_AdaptorInsectScreen;
            case SectionType.Mullion: return Properties.Resources.ImgWood_Mullion;
            case SectionType.GlazingBead: return Properties.Resources.ImgWood_GlazingBead;
            case SectionType.Corner: return Properties.Resources.ImgWood_Corner;
            case SectionType.Joining: return Properties.Resources.ImgWood_Join;
            case SectionType.HookWindow: return Properties.Resources.ImgWood_ClipsWindow;
            case SectionType.HookDoor: return Properties.Resources.ImgWood_ClipsDoor;
            case SectionType.HookInsectScreen: return Properties.Resources.ImgWood_ClipsInsectScreen;
            case SectionType.Reinforcement: return Properties.Resources.ImgWood_Reinforcement;
            case SectionType.Custom: return Properties.Resources.ImgWood_Custom;
            case SectionType.LedgeInt: return Properties.Resources.ImgWood_Ledge;
            case SectionType.LedgeExt: return Properties.Resources.ImgWood_Ledge;
            case SectionType.FrameExpansion: return Properties.Resources.ImgWood_Extension;
            case SectionType.WaterDrip: return Properties.Resources.ImgWood_Drip;
            case SectionType.SashDoorInt: return Properties.Resources.ImgWood_SashDoorInt;
            case SectionType.SashDoorExt: return Properties.Resources.ImgWood_SashDoorExt;
            case SectionType.SashSlidingWindow: return Properties.Resources.ImgWood_SlideSashWindow;
            case SectionType.SashSlidingDoor: return Properties.Resources.ImgWood_SlideSashDoor;
            case SectionType.Track: return Properties.Resources.ImgWood_Track;
            case SectionType.Threshold: return Properties.Resources.ImgWood_Threshold;
            case SectionType.BottomRail: return Properties.Resources.ImgWood_BottomRail;
            case SectionType.Compensation: return Properties.Resources.ImgWood_Compensation;
            case SectionType.Carving: return Properties.Resources.ImgWood_Carving;
            case SectionType.SurfaceAdapter: return Properties.Resources.ImgWood_CarvingAdapter;
            case SectionType.PivotAdapter: return Properties.Resources.ImgWood_PivotAdapter;
            case SectionType.GeorgianBarOutside: return Properties.Resources.ImgWood_GeorgianBarOutside;
            case SectionType.GeorgianBarInside: return Properties.Resources.ImgWood_GeorgianBarInside;
            case SectionType.AdjoiningRollerInsectScreen: return Properties.Resources.ImgWood_RollerInsectScreenAdapter;
            case SectionType.AdjoiningRollerShutter: return Properties.Resources.ImgWood_RollerShutterAdapter;
            case SectionType.BeadInsectScreen: return Properties.Resources.ImgWood_InsectScreenGlazingBead;
            case SectionType.SashSlidingInsectScreen: return Properties.Resources.ImgWood_InsectScreenSlidingSash;
            case SectionType.SashPivot: return Properties.Resources.ImgWood_PivotSash;
            case SectionType.SashInsectScreen: return Properties.Resources.ImgWood_InsectScreenSash;
            case SectionType.MullionInsectScreen: return Properties.Resources.ImgWood_InsectScreenMullion;
            case SectionType.HousingRollerShutter: return Properties.Resources.ImgWood_RollingShutterRoller;
            case SectionType.HousingRollerInsectScreen: return Properties.Resources.ImgWood_RollingInsectScreenRoller;
            case SectionType.TrackRollerShutter: return Properties.Resources.ImgWood_RollingShutterTrack;
            case SectionType.TrackRollerInsectScreen: return Properties.Resources.ImgWood_RollingInsectScreenTrack;
            case SectionType.TrackInsectScreen: return Properties.Resources.ImgWood_InsectScreenTrack;
            case SectionType.TerminalRollerShutter: return Properties.Resources.ImgWood_RollerShutterTerminal;
            case SectionType.TerminalRollerInsectScreen: return Properties.Resources.ImgWood_RollerInsectScreenTerminal;
            case SectionType.FrameInsectScreen: return Properties.Resources.ImgWood_InsectScreenFrame;
            case SectionType.TrackJambVerticalSlide: return Properties.Resources.ImgWood_TrackJambVerticalSlide;
            case SectionType.TrackHeadVerticalSlide: return Properties.Resources.ImgWood_TrackHeadVerticalSlide;
            case SectionType.TrackSillVerticalSlide: return Properties.Resources.ImgWood_TrackSillVerticalSlide;
            case SectionType.SashVerticalSlide: return Properties.Resources.ImgWood_SashVerticalSlide;
            case SectionType.BottomRailVerticalSlide: return Properties.Resources.ImgWood_BottomRailVerticalSlide;
            case SectionType.SashWithHookVerticalSlide: return Properties.Resources.ImgWood_SashWithHookVerticalSlide;
            case SectionType.HookVerticalSlide: return Properties.Resources.ImgWood_HookVerticalSlide;
            case SectionType.Cover: return Properties.Resources.ImgWood_Cover;
            case SectionType.SashWindowExt: return Properties.Resources.ImgWood_SashWindowExt;
            case SectionType.RawSection: return Properties.Resources.ImgWood_RawSection;
            case SectionType.MullionWithTrack: return Properties.Resources.ImgWood_MullionWithTrack;
            case SectionType.MullionWithHook: return Properties.Resources.ImgWood_MullionWithHook;
            case SectionType.SashWithHookSlide: return Properties.Resources.ImgWood_SashWithHookSlide;
            case SectionType.SashWithHookSlideDoor: return Properties.Resources.ImgWood_SashWithHookSlide;
            case SectionType.SashWithAdapterSlide: return Properties.Resources.ImgWood_SashWithAdapterSlide;
            case SectionType.TrackCover: return Properties.Resources.ImgWood_TrackCover;
            case SectionType.TubeRollerShutter: return Properties.Resources.ImgWood_TubeRollerShutter;
            case SectionType.SashSlideAndSwing: return Properties.Resources.ImgWood_SashSlideAndSwing;
            case SectionType.SashWithAdapterSlideSldSw: return Properties.Resources.ImgWood_SashWithAdapterSlideSldSw;
            case SectionType.SashWithAdapterSwingSldSw: return Properties.Resources.ImgWood_SashWithAdapterSwingSldSw;
            case SectionType.AdapterSlideSldSw: return Properties.Resources.ImgWood_AdapterSlideSldSw;
            case SectionType.AdapterSwingSldSw: return Properties.Resources.ImgWood_AdapterSwingSldSw;
            case SectionType.AdapterSlideWindowDoor: return Properties.Resources.ImgWood_AdapterSlideWindowDoor;
            //case SectionType.SashMale: return Properties.Resources.ImgWood_SashWithAdapterSwingSldSw;
            //case SectionType.SashFemale: return Properties.Resources.ImgWood_SashWithAdapterSwingSldSw;
            default:
              Debug.Assert(false, "Tip de profil incompatibil in FrmSectionEdit.GetImage()");
              break;
          }
          break;

        #endregion Wood

        #region Steel

        case MaterialType.Steel:
          switch (sectionType)
          {
            case SectionType.Frame: return Properties.Resources.ImgSteel_Frame;
            case SectionType.SashWindowInt: return Properties.Resources.ImgSteel_SashWindowInt;
            case SectionType.SashAdaptor: return Properties.Resources.ImgSteel_Adaptor;
            case SectionType.SashAdaptorInsectScreen: return Properties.Resources.ImgSteel_AdaptorInsectScreen;
            case SectionType.Mullion: return Properties.Resources.ImgSteel_Mullion;
            case SectionType.GlazingBead: return Properties.Resources.ImgSteel_GlazingBead;
            case SectionType.Corner: return Properties.Resources.ImgSteel_Corner;
            case SectionType.Joining: return Properties.Resources.ImgSteel_Join;
            case SectionType.HookWindow: return Properties.Resources.ImgSteel_ClipsWindow;
            case SectionType.HookDoor: return Properties.Resources.ImgSteel_ClipsDoor;
            case SectionType.HookInsectScreen: return Properties.Resources.ImgSteel_ClipsInsectScreen;
            case SectionType.Reinforcement: return Properties.Resources.ImgSteel_Reinforcement;
            case SectionType.Custom: return Properties.Resources.ImgSteel_Custom;
            case SectionType.LedgeInt: return Properties.Resources.ImgSteel_Ledge;
            case SectionType.LedgeExt: return Properties.Resources.ImgSteel_Ledge;
            case SectionType.FrameExpansion: return Properties.Resources.ImgSteel_Extension;
            case SectionType.WaterDrip: return Properties.Resources.ImgSteel_Drip;
            case SectionType.SashDoorInt: return Properties.Resources.ImgSteel_SashDoorInt;
            case SectionType.SashDoorExt: return Properties.Resources.ImgSteel_SashDoorExt;
            case SectionType.SashSlidingWindow: return Properties.Resources.ImgSteel_SlideSashWindow;
            case SectionType.SashSlidingDoor: return Properties.Resources.ImgSteel_SlideSashDoor;
            case SectionType.Track: return Properties.Resources.ImgSteel_Track;
            case SectionType.Threshold: return Properties.Resources.ImgSteel_Threshold;
            case SectionType.BottomRail: return Properties.Resources.ImgSteel_BottomRail;
            case SectionType.Compensation: return Properties.Resources.ImgSteel_Compensation;
            case SectionType.Carving: return Properties.Resources.ImgSteel_Carving;
            case SectionType.SurfaceAdapter: return Properties.Resources.ImgSteel_CarvingAdapter;
            case SectionType.PivotAdapter: return Properties.Resources.ImgSteel_PivotAdapter;
            case SectionType.GeorgianBarOutside: return Properties.Resources.ImgSteel_GeorgianBarOutside;
            case SectionType.GeorgianBarInside: return Properties.Resources.ImgSteel_GeorgianBarInside;
            case SectionType.AdjoiningRollerInsectScreen: return Properties.Resources.ImgSteel_RollerInsectScreenAdapter;
            case SectionType.AdjoiningRollerShutter: return Properties.Resources.ImgSteel_RollerShutterAdapter;
            case SectionType.BeadInsectScreen: return Properties.Resources.ImgSteel_InsectScreenGlazingBead;
            case SectionType.SashSlidingInsectScreen: return Properties.Resources.ImgSteel_InsectScreenSlidingSash;
            case SectionType.SashPivot: return Properties.Resources.ImgSteel_PivotSash;
            case SectionType.SashInsectScreen: return Properties.Resources.ImgSteel_InsectScreenSash;
            case SectionType.MullionInsectScreen: return Properties.Resources.ImgSteel_InsectScreenMullion;
            case SectionType.HousingRollerShutter: return Properties.Resources.ImgSteel_RollingShutterRoller;
            case SectionType.HousingRollerInsectScreen: return Properties.Resources.ImgSteel_RollingInsectScreenRoller;
            case SectionType.TrackRollerShutter: return Properties.Resources.ImgSteel_RollingShutterTrack;
            case SectionType.TrackRollerInsectScreen: return Properties.Resources.ImgSteel_RollingInsectScreenTrack;
            case SectionType.TrackInsectScreen: return Properties.Resources.ImgSteel_InsectScreenTrack;
            case SectionType.TerminalRollerShutter: return Properties.Resources.ImgSteel_RollerShutterTerminal;
            case SectionType.TerminalRollerInsectScreen: return Properties.Resources.ImgSteel_RollerInsectScreenTerminal;
            case SectionType.FrameInsectScreen: return Properties.Resources.ImgSteel_InsectScreenFrame;
            case SectionType.TrackJambVerticalSlide: return Properties.Resources.ImgSteel_TrackJambVerticalSlide;
            case SectionType.TrackHeadVerticalSlide: return Properties.Resources.ImgSteel_TrackHeadVerticalSlide;
            case SectionType.TrackSillVerticalSlide: return Properties.Resources.ImgSteel_TrackSillVerticalSlide;
            case SectionType.SashVerticalSlide: return Properties.Resources.ImgSteel_SashVerticalSlide;
            case SectionType.BottomRailVerticalSlide: return Properties.Resources.ImgSteel_BottomRailVerticalSlide;
            case SectionType.SashWithHookVerticalSlide: return Properties.Resources.ImgSteel_SashWithHookVerticalSlide;
            case SectionType.HookVerticalSlide: return Properties.Resources.ImgSteel_HookVerticalSlide;
            case SectionType.Cover: return Properties.Resources.ImgSteel_Cover;
            case SectionType.SashWindowExt: return Properties.Resources.ImgSteel_SashWindowExt;
            case SectionType.RawSection: return Properties.Resources.ImgSteel_RawSection;
            case SectionType.MullionWithTrack: return Properties.Resources.ImgSteel_MullionWithTrack;
            case SectionType.MullionWithHook: return Properties.Resources.ImgSteel_MullionWithHook;
            case SectionType.SashWithHookSlide: return Properties.Resources.ImgSteel_SashWithHookSlide;
            case SectionType.SashWithHookSlideDoor: return Properties.Resources.ImgSteel_SashWithHookSlide;
            case SectionType.SashWithAdapterSlide: return Properties.Resources.ImgSteel_SashWithAdapterSlide;
            case SectionType.TrackCover: return Properties.Resources.ImgSteel_TrackCover;
            case SectionType.TubeRollerShutter: return Properties.Resources.ImgSteel_TubeRollerShutter;
            case SectionType.SashSlideAndSwing: return Properties.Resources.ImgSteel_SashSlideAndSwing;
            case SectionType.SashWithAdapterSlideSldSw: return Properties.Resources.ImgSteel_SashWithAdapterSlideSldSw;
            case SectionType.SashWithAdapterSwingSldSw: return Properties.Resources.ImgSteel_SashWithAdapterSwingSldSw;
            case SectionType.AdapterSlideSldSw: return Properties.Resources.ImgSteel_AdapterSlideSldSw;
            case SectionType.AdapterSwingSldSw: return Properties.Resources.ImgSteel_AdapterSwingSldSw;
            case SectionType.AdapterSlideWindowDoor: return Properties.Resources.ImgSteel_AdapterSlideWindowDoor;
            //case SectionType.SashMale: return Properties.Resources.ImgSteel_SashWithAdapterSwingSldSw;
            //case SectionType.SashFemale: return Properties.Resources.ImgSteel_SashWithAdapterSwingSldSw;
            default:
              Debug.Assert(false, "Tip de profil incompatibil in FrmSectionEdit.GetImage()");
              break;
          }
          break;

        #endregion Steel

        default:
          Debug.Assert(false, "Unknown type of MaterialType in FrmSectionEdit.GetImage()");
          break;
      }

      return null;
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
            if (seriesSectionRow.IsIsSeriesActiveNull())
            {
              newSeriesSectionRow.SetIsSeriesActiveNull();
            }
            else
            {
              newSeriesSectionRow.IsSeriesActive = seriesSectionRow.IsSeriesActive;
            }

            sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
            AddRequiredSections(reinforcementRow.IdSectionReinforcement);
          }
        }
      }
      #endregion Adaugare armaturi necesare

      #region Adaugare profile brute necesare
      // II) Adaugare profile brute necesare
      if ((lookUpRawSection.EditValue != null && sectionRow.Id == idSection) ||
          ((sectionRow.Id != idSection && !aSectionRow.IsIdRawSectionNull())))
      {
        int idRawSection;
        if (sectionRow.Id == idSection)
        {
          idRawSection = (int)lookUpRawSection.EditValue;
        }
        else
        {
          idRawSection = aSectionRow.IdRawSection;
        }
        // pentru fiecare serie din care face profilul curent
        foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
        {
          string filterSeriesSection = string.Format("{0} = {1} AND {2} = {3}",
                                                sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                                                seriesSectionRow.IdSeries,
                                                sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                                                idRawSection);
          // daca profilul brut ales nu exista in aceasta serie, il adaug
          if (sectionDS.SeriesSection.Select(filterSeriesSection).Length == 0)
          {
            SectionDataSet.SeriesSectionRow newSeriesSectionRow = sectionDS.SeriesSection.NewSeriesSectionRow();
            newSeriesSectionRow.IdSeries = seriesSectionRow.IdSeries;
            newSeriesSectionRow.IdSection = idRawSection;
            if (seriesSectionRow.IsIsSeriesActiveNull())
            {
              newSeriesSectionRow.SetIsSeriesActiveNull();
            }
            else
            {
              newSeriesSectionRow.IsSeriesActive = seriesSectionRow.IsSeriesActive;
            }
            sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
            AddRequiredSections(idRawSection);
          }
        }
      }

      if ((lookUpArcRawSection.EditValue != null && sectionRow.Id == idSection) ||
          ((sectionRow.Id != idSection && !aSectionRow.IsIdArcRawSectionNull())))
      {
        int idArcRawSection;
        if (sectionRow.Id == idSection)
        {
          idArcRawSection = (int)lookUpArcRawSection.EditValue;
        }
        else
        {
          idArcRawSection = aSectionRow.IdArcRawSection;
        }
        // pentru fiecare serie din care face profilul curent
        foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
        {
          string filterSeriesSection = string.Format("{0} = {1} AND {2} = {3}",
                                                sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                                                seriesSectionRow.IdSeries,
                                                sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                                                idArcRawSection);
          // daca profilul brut ales nu exista in aceasta serie, il adaug
          if (sectionDS.SeriesSection.Select(filterSeriesSection).Length == 0)
          {
            SectionDataSet.SeriesSectionRow newSeriesSectionRow = sectionDS.SeriesSection.NewSeriesSectionRow();
            newSeriesSectionRow.IdSeries = seriesSectionRow.IdSeries;
            newSeriesSectionRow.IdSection = idArcRawSection;
            if (seriesSectionRow.IsIsSeriesActiveNull())
            {
              newSeriesSectionRow.SetIsSeriesActiveNull();
            }
            else
            {
              newSeriesSectionRow.IsSeriesActive = seriesSectionRow.IsSeriesActive;
            }
            sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
            AddRequiredSections(idArcRawSection);
          }
        }
      }
      #endregion Adaugare profile brute necesare

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
            if (seriesSectionRow.IsIsSeriesActiveNull())
            {
              newSeriesSectionRow.SetIsSeriesActiveNull();
            }
            else
            {
              newSeriesSectionRow.IsSeriesActive = seriesSectionRow.IsSeriesActive;
            }
            sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
            AddRequiredSections(customItemRow.IdSection);
          }
        }
      }
      #endregion Adaugare componente necesare

      #region Adaugare cover-uri necesare

      // IV Adaugare cover-uri necesare
      string filterCoverItems = string.Format("{0} = {1}",
                    sectionDS.SectionCover.IdSectionColumn.ColumnName,
                    sectionRow.Id);
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        foreach (SectionDataSet.SectionCoverRow sectionCoverRow in sectionDS.SectionCover.Select(filterCoverItems))
        {
          string filterSeriesCover = string.Format("{0} = {1} AND {2} = {3}",
                        sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                        seriesSectionRow.IdSeries,
                        sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                        sectionCoverRow.IdSectionCover);
          if (sectionDS.SeriesSection.Select(filterSeriesCover).Length == 0)
          {
            SectionDataSet.SeriesSectionRow newSeriesSectionRow = sectionDS.SeriesSection.NewSeriesSectionRow();
            newSeriesSectionRow.IdSeries = seriesSectionRow.IdSeries;
            newSeriesSectionRow.IdSection = sectionCoverRow.IdSectionCover;
            if (seriesSectionRow.IsIsSeriesActiveNull())
            {
              newSeriesSectionRow.SetIsSeriesActiveNull();
            }
            else
            {
              newSeriesSectionRow.IsSeriesActive = seriesSectionRow.IsSeriesActive;
            }
            sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
            AddRequiredSections(sectionCoverRow.IdSectionCover);
          }
        }
      }
      #endregion Adaugare cover-uri necesare

      #region Adaugare Serii SHT

      // V Adauga profilele folosite in combinatiile de coeficient de transfer termic in seriile profilului curent
      //string filterSHTItems = string.Format("{0} = {1} OR {0} = {2}",
      //      idSection,
      //      sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName,
      //      sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName);
      //foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      //{
      //  foreach (SectionDataSet.SectionHeatTransferCoefficientRow shtRow in sectionDS.SectionHeatTransferCoefficient.Select(filterSHTItems))
      //  {
      //    if (shtRow.IdSection1 == idSection)
      //    {
      //      string filterSeriesSHT = string.Format("{0} = {1} AND {2} = {3}",
      //                    sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
      //                    seriesSectionRow.IdSeries,
      //                    sectionDS.SeriesSection.IdSectionColumn.ColumnName,
      //                    shtRow.IdSection2);
      //      if (sectionDS.SeriesSection.Select(filterSeriesSHT).Length == 0)
      //      {
      //        SectionDataSet.SeriesSectionRow newSeriesSectionRow = sectionDS.SeriesSection.NewSeriesSectionRow();
      //        newSeriesSectionRow.IdSeries = seriesSectionRow.IdSeries;
      //        newSeriesSectionRow.IdSection = shtRow.IdSection2;
      //        sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
      //        AddRequiredSections(shtRow.IdSection2);
      //      }
      //    }
      //    if (shtRow.IdSection2 == idSection)
      //    {
      //      string filterSeriesSHT = string.Format("{0} = {1} AND {2} = {3}",
      //                    sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
      //                    seriesSectionRow.IdSeries,
      //                    sectionDS.SeriesSection.IdSectionColumn.ColumnName,
      //                    shtRow.IdSection1);
      //      if (sectionDS.SeriesSection.Select(filterSeriesSHT).Length == 0)
      //      {
      //        SectionDataSet.SeriesSectionRow newSeriesSectionRow = sectionDS.SeriesSection.NewSeriesSectionRow();
      //        newSeriesSectionRow.IdSeries = seriesSectionRow.IdSeries;
      //        newSeriesSectionRow.IdSection = shtRow.IdSection1;
      //        sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
      //        AddRequiredSections(shtRow.IdSection1);
      //      }
      //    }
      //  }
      //}

      #endregion

      #region Adaugare Serii Tolerante

      // VI Adauga profilele folosite in combinatiile de tolerante in seriile profilului curent
      //string filterToleranceItems = string.Format("{0} = {1}",
      //                              idSection,
      //                              sectionDS.SectionTolerance.IdSection1Column.ColumnName);
      //foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      //{
      //  foreach (SectionDataSet.SectionToleranceRow toleranceRow in sectionDS.SectionTolerance.Select(filterToleranceItems))
      //  {
      //    string filterSeriesTolerance = string.Format("{0} = {1} AND {2} = {3}",
      //                  sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
      //                  seriesSectionRow.IdSeries,
      //                  sectionDS.SeriesSection.IdSectionColumn.ColumnName,
      //                  toleranceRow.IdSection2);
      //    if (sectionDS.SeriesSection.Select(filterSeriesTolerance).Length == 0)
      //    {
      //      SectionDataSet.SeriesSectionRow newSeriesSectionRow = sectionDS.SeriesSection.NewSeriesSectionRow();
      //      newSeriesSectionRow.IdSeries = seriesSectionRow.IdSeries;
      //      newSeriesSectionRow.IdSection = toleranceRow.IdSection2;
      //      sectionDS.SeriesSection.AddSeriesSectionRow(newSeriesSectionRow);
      //      AddRequiredSections(toleranceRow.IdSection2);
      //    }
      //  }
      //}

      #endregion
    }

    /// <summary>
    /// Verifica daca profilul de armare selectat este inclus in toate seriile in care apare si profilul editat
    /// </summary>
    /// <returns>TRUE daca s-a validat profilul de armare, FALSE altfel</returns>
    private bool CheckReinforcementsValidity()
    {
      string filterSeries = string.Format("{0} = {1}",
                              sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                              sectionRow.Id);

      // se verifica daca fiecare serie in care se afla profilul curent, contine
      // armatura selectata
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        foreach (int idReinforcement in GetReinforcements())
        {
          string filterSeriesSection = string.Format("{0} = {1} AND {2} = {3}",
                                                sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                                                seriesSectionRow.IdSeries,
                                                sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                                                idReinforcement);
          if (sectionDS.SeriesSection.Select(filterSeriesSection).Length == 0)
          {
            return false;
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Verifica daca profilul brut selectat este inclus in toate seriile in care apare si profilul editat
    /// </summary>
    /// <returns>TRUE daca s-a validat profilul brut, FALSE altfel</returns>
    private bool CheckRawSectionValidity()
    {
      if (lookUpRawSection.EditValue == null)
        return true;
      string filterSeries = string.Format("{0} = {1}",
                              sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                              sectionRow.Id);

      // se verifica daca fiecare serie in care se afla profilul curent, contine
      // profilul brut selectat
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        string filterSeriesSection = string.Format("{0} = {1} AND {2} = {3}",
                                              sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                                              seriesSectionRow.IdSeries,
                                              sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                                              (int)lookUpRawSection.EditValue);
        if (sectionDS.SeriesSection.Select(filterSeriesSection).Length == 0)
        {
          return false;
        }
      }

      if (lookUpArcRawSection.EditValue == null)
        return true;
      filterSeries = string.Format("{0} = {1}",
                       sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                       sectionRow.Id);

      // se verifica daca fiecare serie in care se afla profilul curent, contine
      // profilul brut selectat
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        string filterSeriesSection = string.Format("{0} = {1} AND {2} = {3}",
                                              sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                                              seriesSectionRow.IdSeries,
                                              sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                                              (int)lookUpArcRawSection.EditValue);
        if (sectionDS.SeriesSection.Select(filterSeriesSection).Length == 0)
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Verifica daca profilele de placare sunt incluse in toate seriile in care apare si profilul editat
    /// </summary>
    /// <returns>TRUE daca s-au validat toate profilele de placare, FALSE altfel</returns>
    private bool CheckCoverValidity()
    {
      string filterCoverItems = string.Format("{0} = {1}",
                          sectionDS.SectionCover.IdSectionColumn.ColumnName,
                          sectionRow.Id);

      string filterSeries = string.Format("{0} = {1}",
                              sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                              sectionRow.Id);
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        foreach (SectionDataSet.SectionCoverRow sectionCoverRow in sectionDS.SectionCover.Select(filterCoverItems))
        {
          string filterSeriesCover = string.Format("{0} = {1} AND {2} = {3}",
                        sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                        seriesSectionRow.IdSeries,
                        sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                        sectionCoverRow.IdSectionCover);
          if (sectionDS.SeriesSection.Select(filterSeriesCover).Length == 0)
          {
            return false;
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Enumera armaturile utilizate pe culori si pe profilul de baza.
    /// </summary>
    /// <returns>Armaturile utilizate pe culori si pe profilul de baza</returns>
    private IEnumerable<int> GetReinforcements()
    {
      if (gridLookUpDefaultReinforcement.EditValue != null)
        yield return (int)gridLookUpDefaultReinforcement.EditValue;

      IEnumerable<int> reinforcements = sectionRow.GetSectionReinforcementRows().Select(row => row.IdSectionReinforcement);
      foreach (int idReinforcement in reinforcements)
      {
        yield return idReinforcement;
      }
    }

    /// <summary>
    /// Verifica daca profilele componente alese fac parte din toate seriile ce contin profilul curent
    /// </summary>
    /// <returns>TRUE daca s-au validat profilele componente, FALSE altfel</returns>
    private bool CheckCustomItemsValidity()
    {
      string filterCustomItems = string.Format("{0} = {1}",
                          sectionDS.CustomSectionItem.IdParentSectionColumn.ColumnName,
                          sectionRow.Id);

      string filterSeries = string.Format("{0} = {1}",
                              sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                              sectionRow.Id);
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        foreach (SectionDataSet.CustomSectionItemRow customItemRow in sectionDS.CustomSectionItem.Select(filterCustomItems))
        {
          string filterSeriesCustomItems = string.Format("{0} = {1} AND {2} = {3}",
                        sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                        seriesSectionRow.IdSeries,
                        sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                        customItemRow.IdSection);
          if (sectionDS.SeriesSection.Select(filterSeriesCustomItems).Length == 0)
          {
            return false;
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Verifica daca profilele componente ale combinatiilor de transfer termic fac parte din toate seriile ce contin
    /// profilul curent.
    /// </summary>
    /// <returns>TRUE daca s-au validat profilele componente, FALSE altfel</returns>
    private bool CheckSHTValidity()
    {
      string filterSHTItems = string.Format("{0} = {1} OR {0} = {2}",
            sectionRow.Id,
            sectionDS.SectionHeatTransferCoefficient.IdSection1Column.ColumnName,
            sectionDS.SectionHeatTransferCoefficient.IdSection2Column.ColumnName);

      string filterSeries = string.Format("{0} = {1}",
                              sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                              sectionRow.Id);
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        foreach (SectionDataSet.SectionHeatTransferCoefficientRow shtItemRow in sectionDS.SectionHeatTransferCoefficient.Select(filterSHTItems))
        {
          if (shtItemRow.IdSection1 != sectionRow.Id)
          {
            string filterSeriesSHTItems = string.Format("{0} = {1} AND {2} = {3}",
                         sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                         seriesSectionRow.IdSeries,
                         sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                         shtItemRow.IdSection1);
            if (sectionDS.SeriesSection.Select(filterSeriesSHTItems).Length == 0)
            {
              return false;
            }
          }
          else
          {
            string filterSeriesSHTItems = string.Format("{0} = {1} AND {2} = {3}",
                         sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                         seriesSectionRow.IdSeries,
                         sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                         shtItemRow.IdSection2);
            if (sectionDS.SeriesSection.Select(filterSeriesSHTItems).Length == 0)
            {
              return false;
            }
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Verifica daca profilele componente ale combinatiilor de tolerante fac parte din toate seriile ce contin
    /// profilul curent.
    /// </summary>
    /// <returns>TRUE daca s-au validat profilele componente, FALSE altfel</returns>
    private bool CheckToleranceValidity()
    {
      string filterToleranceItems = string.Format("{0} = {1}",
            sectionRow.Id,
            sectionDS.SectionTolerance.IdSection1Column.ColumnName);

      string filterSeries = string.Format("{0} = {1}",
                              sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                              sectionRow.Id);
      foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
      {
        foreach (SectionDataSet.SectionToleranceRow toleranceRow in sectionDS.SectionTolerance.Select(filterToleranceItems))
        {
          string filterSeriesToleranceItems = string.Format("{0} = {1} AND {2} = {3}",
                       sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                       seriesSectionRow.IdSeries,
                       sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                       toleranceRow.IdSection2);
          if (sectionDS.SeriesSection.Select(filterSeriesToleranceItems).Length == 0)
          {
            return false;
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Verifica daca seriile care trebuie sterse contin profile ce refera profilul curent si 
    /// se afiseaza mesajul corespunzator.
    /// </summary>
    /// <returns>True daca toate intrarile din seriile profilului se pot sterge.</returns>
    private bool CheckViewSeriesForDelete()
    {
      int[] selectedRows = viewSeries.GetSelectedRows();
      List<SectionDataSet.SeriesSectionRow> toBeDeletedRows = new List<SectionDataSet.SeriesSectionRow>(selectedRows.Length);

      foreach (int rowHandle in selectedRows)
      {
        SectionDataSet.SeriesSectionRow seriesSectionRow = viewSeries.GetDataRow(rowHandle) as SectionDataSet.SeriesSectionRow;
        if (seriesSectionRow != null)
          toBeDeletedRows.Add(seriesSectionRow);
      }

      foreach (SectionDataSet.SeriesSectionRow checkedRow in toBeDeletedRows)
      {
        string filterSeries = string.Format(CultureInfo.InvariantCulture,
                                            "{0} = {1} AND {2} <> {3}",
                                            sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                                            checkedRow.IdSeries,
                                            sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                                            checkedRow.IdSection);

        foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries))
        {
          // daca randul analizat e deja selectat spre stergere nu il mai analizez acum
          if (toBeDeletedRows.Contains(seriesSectionRow))
            continue;

          // verificare daca vreun profil este armat cu cel pentru care se face verificarea.
          SectionDataSet.SectionRow sectionRow = sectionDS.Section.FindById(seriesSectionRow.IdSection);
          if (!sectionRow.IsIdReinforcementNull() && sectionRow.IdReinforcement == checkedRow.IdSection)
          {
            return DialogResult.Yes ==
              XtraMessageBox.Show(this,
                                  Properties.Resources.MsgDeleteSectionSeriesUsed,
                                  Properties.Resources.CaptionAttentionMsgBox,
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Exclamation);
          }

          string filterSectionReinforcement = string.Format(CultureInfo.InvariantCulture,
                                                            "{0} = {1} AND {2} = {3}",
                                                            sectionDS.SectionReinforcement.IdSectionColumn.ColumnName,
                                                            sectionRow.Id,
                                                            sectionDS.SectionReinforcement.IdSectionReinforcementColumn.ColumnName,
                                                            checkedRow.IdSection);

          if (sectionDS.SectionReinforcement.Select(filterSectionReinforcement).Length != 0)
          {
            return DialogResult.Yes ==
              XtraMessageBox.Show(this,
                                  Properties.Resources.MsgDeleteSectionSeriesUsed,
                                  Properties.Resources.CaptionAttentionMsgBox,
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Exclamation);
          }

          // verificare daca vreun profil are ca profil brut pe cel pentru care se face verificarea.
          if ((!sectionRow.IsIdRawSectionNull() && sectionRow.IdRawSection == checkedRow.IdSection) ||
              (!sectionRow.IsIdArcRawSectionNull() && sectionRow.IdArcRawSection == checkedRow.IdSection))
          {
            return DialogResult.Yes ==
              XtraMessageBox.Show(this,
                                  Properties.Resources.MsgDeleteSectionSeriesUsed,
                                  Properties.Resources.CaptionAttentionMsgBox,
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Exclamation);
          }

          string filterCustomSection = string.Format(CultureInfo.InvariantCulture,
                                                             "{0} = {1} AND {2} = {3}",
                                                             sectionDS.CustomSectionItem.IdParentSectionColumn.ColumnName,
                                                             sectionRow.Id,
                                                             sectionDS.CustomSectionItem.IdSectionColumn.ColumnName,
                                                             checkedRow.IdSection);

          // verificare daca vreun profil il are pe cel sters ca parte componenta.
          if (sectionDS.CustomSectionItem.Select(filterCustomSection).Length != 0)
          {
            return DialogResult.Yes ==
              XtraMessageBox.Show(this,
                                  Properties.Resources.MsgDeleteSectionSeriesUsed,
                                  Properties.Resources.CaptionAttentionMsgBox,
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Exclamation);
          }

          string filterSectionCover = string.Format(CultureInfo.InvariantCulture,
                                                            "{0} = {1} AND {2} = {3}",
                                                            sectionDS.SectionCover.IdSectionColumn.ColumnName,
                                                            sectionRow.Id,
                                                            sectionDS.SectionCover.IdSectionCoverColumn.ColumnName,
                                                            checkedRow.IdSection);


          // verifica daca vreun profil il are ca placaj pe cel sters
          if (sectionDS.SectionCover.Select(filterSectionCover).Length != 0)
          {
            return DialogResult.Yes ==
              XtraMessageBox.Show(this,
                                  Properties.Resources.MsgDeleteSectionSeriesUsed,
                                  Properties.Resources.CaptionAttentionMsgBox,
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Exclamation);
          }
        }
      }

      return DialogResult.Yes ==
              XtraMessageBox.Show(FrameworkApplication.FirstApplicationForm,
                                                 Resources.MsgQuestionDeleteRows,
                                                 Resources.CaptionQuestionMsgBox,
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Question);
    }

    /// <summary>
    /// Sterge legatura unor profile la o serie, facand cascade pe profilele ce folosesc profilul scos din serie
    /// </summary>
    private void DeleteSeriesSection()
    {
      int[] selectedRows = viewSeries.GetSelectedRows();
      List<SectionDataSet.SeriesSectionRow> seriesSectionSelectedRows = new List<SectionDataSet.SeriesSectionRow>();

      foreach (int rowHandle in selectedRows)
      {
        SectionDataSet.SeriesSectionRow seriesSectionRow = viewSeries.GetDataRow(rowHandle) as SectionDataSet.SeriesSectionRow;
        if (seriesSectionRow != null)
          seriesSectionSelectedRows.Add(seriesSectionRow);
      }

      while (seriesSectionSelectedRows.Count != 0)
      {
        SectionDataSet.SeriesSectionRow checkedRow = seriesSectionSelectedRows.FirstOrDefault();

        string filterSeries = string.Format(CultureInfo.InvariantCulture,
                                            "{0} = {1} AND {2} <> {3}",
                                            sectionDS.SeriesSection.IdSeriesColumn.ColumnName,
                                            checkedRow.IdSeries,
                                            sectionDS.SeriesSection.IdSectionColumn.ColumnName,
                                            checkedRow.IdSection);

        foreach (SectionDataSet.SeriesSectionRow seriesSectionRow in sectionDS.SeriesSection.Select(filterSeries).ToArray())
        {
          // daca randul analizat e deja selectat spre stergere nu il mai analizez acum
          if (seriesSectionSelectedRows.Contains(seriesSectionRow))
            continue;

          SectionDataSet.SectionRow sectionRow = sectionDS.Section.FindById(seriesSectionRow.IdSection);

          // verificare daca vreun profil este armat cu cel pentru care se face verificarea.
          // verificarea se face doar pentru performanta. Chiar si fara aceasta verificare profilul ar fi fost 
          // eliminat la validarea armaturilor disponibile.
          if (!sectionRow.IsIdReinforcementNull() && sectionRow.IdReinforcement == checkedRow.IdSection)
          {
            seriesSectionSelectedRows.Add(seriesSectionRow);
            //seriesSectionRow.Delete();
            continue;
          }

          string filterSectionReinforcement = string.Format(CultureInfo.InvariantCulture,
                                                            "{0} = {1} AND {2} = {3}",
                                                            sectionDS.SectionReinforcement.IdSectionColumn.ColumnName,
                                                            sectionRow.Id,
                                                            sectionDS.SectionReinforcement.IdSectionReinforcementColumn.ColumnName,
                                                            checkedRow.IdSection);

          if (sectionDS.SectionReinforcement.Select(filterSectionReinforcement).Length != 0)
          {
            seriesSectionSelectedRows.Add(seriesSectionRow);
            //seriesSectionRow.Delete();
            continue;
          }

          // verificare daca vreun profil are ca profil brut pe cel pentru care se face verificarea.
          if ((!sectionRow.IsIdRawSectionNull() && sectionRow.IdRawSection == checkedRow.IdSection) ||
              (!sectionRow.IsIdArcRawSectionNull() && sectionRow.IdArcRawSection == checkedRow.IdSection))
          {
            seriesSectionRow.Delete();
            continue;
          }

          string filterCustomSection = string.Format(CultureInfo.InvariantCulture,
                                                            "{0} = {1} AND {2} = {3}",
                                                            sectionDS.CustomSectionItem.IdParentSectionColumn.ColumnName,
                                                            sectionRow.Id,
                                                            sectionDS.CustomSectionItem.IdSectionColumn.ColumnName,
                                                            checkedRow.IdSection);

          // verificare daca vreun profil il are pe cel sters ca parte componenta.
          if (sectionDS.CustomSectionItem.Select(filterCustomSection).Length != 0)
          {
            seriesSectionSelectedRows.Add(seriesSectionRow);
            //seriesSectionRow.Delete();
            continue;
          }

          string filterSectionCover = string.Format(CultureInfo.InvariantCulture,
                                                            "{0} = {1} AND {2} = {3}",
                                                            sectionDS.SectionCover.IdSectionColumn.ColumnName,
                                                            sectionRow.Id,
                                                            sectionDS.SectionCover.IdSectionCoverColumn.ColumnName,
                                                            checkedRow.IdSection);


          // verifica daca vreun profil il are ca placaj pe cel sters
          if (sectionDS.SectionCover.Select(filterSectionCover).Length != 0)
          {
            seriesSectionSelectedRows.Add(seriesSectionRow);
            //seriesSectionRow.Delete();
            continue;
          }
        }

        seriesSectionSelectedRows.Remove(checkedRow);
        checkedRow.Delete();
      }
    }

    /// <summary>
    /// Initializeaza unitatile de masura de pe forma.
    /// </summary>
    private void InitializeUnits()
    {
      // Controale de editare.
      unitH1.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitH2.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitH3.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitH.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitW1.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitW.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitPerimter.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitArea.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.SectionSurface;

      unitBarLength.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitUnitWeight.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.Mass;
      unitCuttingTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitBindingTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitProcessingTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitTenonTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitDowelTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitMullionDowelTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitMullionTenonTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitAdapterTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitCurvingAddition.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitWOffset.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitTrackDistance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitGlassOffset.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitMinRadius.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitSashTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitNoThresholdTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitFoldingSash2SashTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitSlideAndSwingWingTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitGlassTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitHeatTransferCoefficient.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.ThermalTransmittance;
      unitIx.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MomentOfInertia;
      unitIy.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MomentOfInertia;
      unitSashExtraDimension.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;

      unitArcRawSectionTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitRawSectionTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitDynamicRawSectionH.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitDynamicRawSectionW.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitMaxW.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitWInputTolerance.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;

      repUnitSectionCoverLength.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      repUnitThermalTransmittance.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.ThermalTransmittance;
      repUnitTolerance.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      repUnitOffset.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      repUnitReinforcementOffset.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;

      unitMinSegmentLength.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitMaxSegmentLength.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitMaxSegmentedLength.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;


      unitMinLimitLength.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitMinInventoryLength.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;
      unitMaxLimitLength.Properties.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;

      repUnitLength.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MaterialLength;

      repMaterialSpeciesUnitWeight.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.Mass;
      repUnitIx.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MomentOfInertia;
      repUnitIy.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.MomentOfInertia;
      repUnitHeatTransferCoefficient.ExternalUnitConverter = MeasurableProperties.DefaultMeasurableProperties.ThermalTransmittance;

      // Label-uri
      lblUnitWeight.Text = string.Format(lblUnitWeight.Text, MeasurableProperties.DefaultMeasurableProperties.Length.DisplayUnitShortName);
      lblUnitBasePrice.Text = string.Format(lblUnitBasePrice.Text, MeasurableProperties.DefaultMeasurableProperties.Length.DisplayUnitShortName);
      colMaterialSpeciesUnitWeight.Caption = String.Format(colMaterialSpeciesUnitWeight.Caption, MeasurableProperties.DefaultMeasurableProperties.Length.DisplayUnitShortName);
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
      LocalizeLookUpHeader(repLookUpSeriesIdSeries, Resources.ResourceManager);
      LocalizeLookUpHeader(lookUpCurrency.Properties, Resources.ResourceManager);
      LocalizeLookUpHeader(repLookUpCostGroupId, Resources.ResourceManager);
      LocalizeLookUpHeader(repLookUpCostGroup, Resources.ResourceManager);
      LocalizeLookUpHeader(lookUpRawSection.Properties, Resources.ResourceManager);
      LocalizeLookUpHeader(lookUpArcRawSection.Properties, Resources.ResourceManager);
      LocalizeLookUpHeader(repLookUpColorCombinationCode, Resources.ResourceManager);
      LocalizeLookUpHeader(repLookUpMaterialSpeciesCode, Resources.ResourceManager);
      LocalizeLookUpHeader(repLookUpRawSection, Resources.ResourceManager);
    }

    /// <summary>
    /// Actualizare stare controale de optimizare in functie de stare check-uri.
    /// </summary>
    private void UpdateOptimizationControls()
    {
      chkUseDoubleCut.Enabled = chkIsForOptimization.Checked;
      txtBinSize.Enabled = chkIsForOptimization.Checked;

      groupStockSettings.Enabled = openingMode == FormOpeningMode.Locked ? false : chkIsForOptimization.Checked;

      lookUpOptimizationInventoryUseType.Enabled = chkIsForOptimization.Checked;
      unitMinLimitLength.Enabled = chkIsForOptimization.Checked;
      unitMinInventoryLength.Enabled = chkIsForOptimization.Checked;
      unitMaxLimitLength.Enabled = chkIsForOptimization.Checked;
      txtTargetInventory.Enabled = chkIsForOptimization.Checked;
      txtMaxInventory.Enabled = chkIsForOptimization.Checked;
    }

    /// <summary>
    /// Actualizare starea controalelor informatii segment in functie de modul de depasire.
    /// </summary>
    private void UpdateSegmentControls()
    {
      bool extendingMode;
      if (lookUpExtendingMode.EditValue.ToString() == ExtendingMode.None.ToString())
      {
        extendingMode = false;
      }
      else
      {
        extendingMode = true;
      }

      unitMinSegmentLength.Enabled = extendingMode;
      unitMaxSegmentLength.Enabled = extendingMode;
      unitMaxSegmentedLength.Enabled = extendingMode;
    }

    /// <summary>
    /// Actualizare stare controlae informatii in functie de modul de curbare.
    /// </summary>
    private void UpdateArchControls()
    {
      bool curvingMode;
      if (lookUpCurvingMode.EditValue.ToString() == CurvingMode.None.ToString() || lookUpCurvingMode.EditValue.ToString() == CurvingMode.Milling.ToString())
      {
        curvingMode = false;
      }
      else
      {
        curvingMode = true;
      }

      unitCurvingAddition.Enabled = curvingMode;
    }

    /// <summary>
    /// Seteaza formatul valutelor la cel din optiuni
    /// </summary>
    private void SetValueFormat()
    {
      if (lookUpCurrency.EditValue != null)
      {
        txtUnitWeightPrice.Properties.DisplayFormat.FormatString = CurrencyExchange.CurrentCurrencyExchange.GetDisplayFormat((string)lookUpCurrency.EditValue);
        unitMaterialSpeciesPrice.DisplayFormat.FormatString = CurrencyExchange.CurrentCurrencyExchange.GetDisplayFormat((string)lookUpCurrency.EditValue);
      }
    }

    /// <summary>
    /// Seteaza proprietatea ce accepta editarea coloanei de armatura din tab-ul de culori, iar pentru material ce nu
    /// poate fi armat, se vor seta armaturile la null.
    /// </summary>
    /// <param name="allowReinforcement">Indica daca se accepta sau nu editarea coloanei de armatura.</param>
    private void UpdateIdReinforcementColumn(bool allowReinforcement)
    {
      colIdReinforcement.OptionsColumn.AllowEdit = allowReinforcement;
      colListIdReinforcement.OptionsColumn.AllowEdit = allowReinforcement;

      if (!allowReinforcement)
        for (int i = 0; i < viewSectionColorList.DataRowCount; i++)
        {
          foreach (SectionDataSet.SectionColorListRow sectionColorListRow in sectionRow.GetSectionColorListRows())
          {
            if (!sectionColorListRow.IsIdReinforcementNull())
              sectionColorListRow.SetIdReinforcementNull();
            foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListRow.GetSectionColorListItemRows())
              if (!sectionColorListItemRow.IsIdReinforcementNull())
                sectionColorListItemRow.SetIdReinforcementNull();
          }
        }
      else
      {
        for (int i = 0; i < viewSectionColorList.DataRowCount; i++)
        {
          foreach (SectionDataSet.SectionColorListRow sectionColorListRow in sectionRow.GetSectionColorListRows())
          {
            if (gridLookUpDefaultReinforcement.EditValue != null)
              sectionColorListRow.IdReinforcement = (int)gridLookUpDefaultReinforcement.EditValue;
            else
              sectionColorListRow.SetIdReinforcementNull();
            foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListRow.GetSectionColorListItemRows())
            {
              if (gridLookUpDefaultReinforcement.EditValue != null)
                sectionColorListItemRow.IdReinforcement = (int)gridLookUpDefaultReinforcement.EditValue;
              else
                sectionColorListItemRow.SetIdReinforcementNull();
            }
          }
        }
      }
    }

    /// <summary>
    /// Seteaza proprietatile ce accepta editarea coloanelor(Profil Brut si Profil Brut Arce) din
    /// tab-ul de tipuri de materiale.
    /// </summary>
    /// <param name="allowRawSection">Indica daca se accepta sau nu editarea coloanei de profil brut.</param>
    /// <param name="allowArcRawSection">Indica daca se accepta sau nu editarea coloanei de profil brut arce.</param>
    private void UpdateMaterialSpeciesRawSectionColumn(bool allowRawSection, bool allowArcRawSection)
    {
      colMaterialSpeciesIdRawSection.OptionsColumn.AllowEdit = allowRawSection;
      colMaterialSpeciesIdArcRawSection.OptionsColumn.AllowEdit = allowArcRawSection;

      if (!allowRawSection || !allowArcRawSection)
      {
        for (int i = 0; i < viewMaterialSpecies.DataRowCount; i++)
        {
          foreach (SectionDataSet.SectionMaterialSpeciesRow sectionMaterialSpeciesRow in sectionRow.GetSectionMaterialSpeciesRows())
          {
            if (!allowRawSection && !sectionMaterialSpeciesRow.IsIdRawSectionNull())
              sectionMaterialSpeciesRow.SetIdRawSectionNull();
            if (!allowArcRawSection && !sectionMaterialSpeciesRow.IsIdArcRawSectionNull())
              sectionMaterialSpeciesRow.SetIdArcRawSectionNull();
          }
        }
      }
    }

    /// <summary>
    /// Verifica daca o lista de culori se poate folosi pe profil.
    /// </summary>
    /// <param name="sectionColorListRow">Lista de culori de verificat.</param>
    /// <returns>Flag ce indica daca lista nu contine combinatii de culori folosite deja pe profil.</returns>
    private bool CanUseColorList(SectionDataSet.SectionColorListRow sectionColorListRow)
    {
      return true;
      using (new WaitCursor())
      {
        IEnumerable<SectionDataSet.SectionColorListItemRow> sectionColorListItemRows = GetSectionColorListItemRows(sectionColorListRow);

        foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListItemRows)
          if (!CanUseColorCombination(sectionColorListItemRow))
            return false;

        return true;
      }
    }

    /// <summary>
    /// Regenereaza codurile elementelor furnizate.
    /// </summary>
    /// <param name="list">Lista elementelor ce trebuie regenerate.</param>
    private void RegenerateCodes(List<SectionDataSet.SectionColorListItemRow> list)
    {
      foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in list)
      {
        string code = sectionColorListItemRow.SectionColorListRow.SectionRow.CreateCodeWithColorCombination(colorDS.ColorCombination.SingleOrDefault(row => row.Id == sectionColorListItemRow.IdColorCombination), colorListDS);
        sectionColorListItemRow.Code = code;
      }
      foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in list)
      {
        sectionColorListItemRow.GenerateUniqueCode();
      }
    }

    /// <summary>
    /// Verifica daca o combinatie de culori se poate folosi pe profil.
    /// </summary>
    /// <param name="sectionColorListItemRow">Combinatia de culori de verificat.</param>
    /// <returns>Flag ce indica daca combinatia de culori este folosita deja pe profil.</returns>
    private bool CanUseColorCombination(SectionDataSet.SectionColorListItemRow sectionColorListItemRow)
    {
      return true;
      foreach (SectionDataSet.SectionColorListRow curSectionColorListRow in sectionRow.GetSectionColorListRows())
      {
        if (curSectionColorListRow.Id == sectionColorListItemRow.IdSectionColorList)
          continue;

        foreach (SectionDataSet.SectionColorListItemRow curSectionColorListItemRow in curSectionColorListRow.GetSectionColorListItemRows())
        {
          if (curSectionColorListItemRow.IdColorCombination == sectionColorListItemRow.IdColorCombination &&
              curSectionColorListItemRow.Id != sectionColorListItemRow.Id)
            return false;
        }

        if (curSectionColorListRow.ConvertedEditingMode == ColorListEditingMode.None ||
              curSectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListPrice)
        {
          foreach (ColorListDataSet.ColorListItemRow curColorListItemRow in colorListDS.ColorList.SingleOrDefault(colorListRow => colorListRow.Id == curSectionColorListRow.IdColorList).GetColorListItemRows())
          {
            if (curColorListItemRow.IdColorCombination == sectionColorListItemRow.IdColorCombination)
              return false;
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Metoda ce creaza si returneaza o colectie de elemente definite intr-o lista de culori.
    /// </summary>
    /// <param name="sectionColorListRow">Lista de culori pentru care se afla elementele.</param>
    /// <returns>O colectie de elemente ale listei.</returns>
    private IEnumerable<SectionDataSet.SectionColorListItemRow> GetSectionColorListItemRows(SectionDataSet.SectionColorListRow sectionColorListRow)
    {
      SectionDataSet.SectionColorListItemDataTable colorListItemDT = new SectionDataSet.SectionColorListItemDataTable();
      foreach (ColorListDataSet.ColorListItemRow colorListItemRow in colorListDS.ColorList.SingleOrDefault(colorListRow => colorListRow.Id == sectionColorListRow.IdColorList).GetColorListItemRows())
      {
        yield return CreateSectionColorListItemRow(colorListItemDT, sectionColorListRow, colorListItemRow);
      }
    }

    /// <summary>
    /// Metoda ce creaza un element pentru o lista de culori pe profil.
    /// </summary>
    /// <param name="colorListItemDT">Tabela in care se adauga randul nou creat.</param>
    /// <param name="sectionColorListRow">Lista de culori pentru care se creaza elementul.</param>
    /// <param name="colorListItemRow">Element din lista de culori din care se preiau informatii.</param>
    /// <returns>Elementul creat.</returns>
    private SectionDataSet.SectionColorListItemRow CreateSectionColorListItemRow(
      SectionDataSet.SectionColorListItemDataTable colorListItemDT,
      SectionDataSet.SectionColorListRow sectionColorListRow,
      ColorListDataSet.ColorListItemRow colorListItemRow)
    {
      SectionDataSet.SectionColorListItemRow sectionColorListItemRow = colorListItemDT.NewSectionColorListItemRow();
      if (sectionColorListRow.IsIdCostGroupNull())
      {
        int? idCostGroup = colorListItemRow.GetIdCostGroup();
        if (idCostGroup == null)
        {
          sectionColorListItemRow.SetIdCostGroupNull();
        }
        else
        {
          sectionColorListItemRow.IdCostGroup = idCostGroup.Value;
        }
      }
      else
      {
        sectionColorListItemRow.IdCostGroup = sectionColorListRow.IdCostGroup;
      }
      sectionColorListItemRow.IdColorCombination = colorListItemRow.IdColorCombination;

      string code = sectionColorListRow.SectionRow.GetCodeWithColorCombination(colorDS.ColorCombination.SingleOrDefault(row => row.Id == colorListItemRow.IdColorCombination), colorListDS);
      string newCode = code;

      sectionColorListItemRow.Code = newCode;
      sectionColorListItemRow.IdSectionColorList = sectionColorListRow.Id;
      sectionColorListItemRow.Price = ConvertPriceType(sectionRow.GetUnitPrice(colorDS.ColorCombination.FindById(colorListItemRow.IdColorCombination), colorListItemRow.ColorListRow, null, sectionColorListRow.ConvertedEditingMode, lookUpCurrency.EditValue == null ? (string)null : lookUpCurrency.EditValue.ToString()));
      return sectionColorListItemRow;
    }
    /// <summary>
    /// Returneaza o copie a randului.
    /// </summary>
    /// <param name="sectionColorListItemRow">Randul dupa care se face copia.</param>
    /// <param name="sectionColorListItemDataTable">Tabela in care se creaza randul copie.</param>
    /// <returns>Copia randului.</returns>
    private static SectionDataSet.SectionColorListItemRow CopySectionColorListItemRow(SectionDataSet.SectionColorListItemRow sectionColorListItemRow,
                                                                               SectionDataSet.SectionColorListItemDataTable sectionColorListItemDataTable)
    {
      SectionDataSet.SectionColorListItemRow newSectionColorListItemRow = sectionColorListItemDataTable.NewSectionColorListItemRow();
      newSectionColorListItemRow.Code = sectionColorListItemRow.Code;
      if (!sectionColorListItemRow.IsBarLengthNull())
        newSectionColorListItemRow.BarLength = sectionColorListItemRow.BarLength;
      newSectionColorListItemRow.IdColorCombination = sectionColorListItemRow.IdColorCombination;
      if (!sectionColorListItemRow.IsIdCostGroupNull())
      {
        newSectionColorListItemRow.IdCostGroup = sectionColorListItemRow.IdCostGroup;
      }
      else
      {
        newSectionColorListItemRow.SetIdCostGroupNull();
      }
      if (!sectionColorListItemRow.IsIdReinforcementNull())
        newSectionColorListItemRow.IdReinforcement = sectionColorListItemRow.IdReinforcement;


      newSectionColorListItemRow.IdSectionColorList = sectionColorListItemRow.IdSectionColorList;

      newSectionColorListItemRow.Price = sectionColorListItemRow.Price;
      return newSectionColorListItemRow;
    }


    /// <summary>
    /// Seteaza sursa de date a gridului de culori in functie de selectia listelor de culori.
    /// </summary>
    private void SetGridColorItemDataSource()
    {
      int[] selectedRowHandles = viewSectionColorList.GetSelectedRows();

      //This is done in order that when you first open the form, the sectionDS doesn't get copied
      //for basically no reason, good performance improvement
      if (selectedRowHandles.Length == 0)
      {
        gridColorListItem.DataSource = null;
        viewColorListItem.OptionsBehavior.Editable = false;
        return;
      }

      if (selectedRowHandles.Length == 1)
      {

        SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(viewSectionColorList.FocusedRowHandle) as SectionDataSet.SectionColorListRow;

        if (sectionColorListRow != null && sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListColorCombinations)
        {
          // Daca este selectat un singur rand si nu se foloseste lista de culori, se va da ca sursa de date tabela 
          // de SectionloCorListItem filtrata dupa lista de culori folosita.
          sectionColorListItemBS.DataSource = sectionDS.SectionColorListItem;
          sectionColorListItemBS.Filter = String.Format("{0} = {1}", sectionDS.SectionColorListItem.IdSectionColorListColumn.ColumnName, sectionColorListRow.Id);
          gridColorListItem.DataSource = sectionColorListItemBS;
          viewColorListItem.OptionsBehavior.Editable = true;

          return;
        }
      }

      SectionDataSet viewSectionDS = (SectionDataSet)sectionDS.Copy();
      viewSectionDS.SectionColorListItem.Clear();

      foreach (int selectedRowHandle in selectedRowHandles)
      {
        SectionDataSet.SectionColorListRow sectionColorListRow = viewSectionColorList.GetDataRow(selectedRowHandle) as SectionDataSet.SectionColorListRow;
        if (sectionColorListRow == null || viewSectionColorList.IsNewItemRow(selectedRowHandle))
          continue;

        try
        {
          if (sectionColorListRow.ConvertedEditingMode == ColorListEditingMode.EditColorListColorCombinations)
          {
            foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in sectionColorListRow.GetSectionColorListItemRows())
            {
              SectionDataSet.SectionColorListItemRow newSectionColorListItemRow = CopySectionColorListItemRow(sectionColorListItemRow, viewSectionDS.SectionColorListItem);
              viewSectionDS.SectionColorListItem.AddSectionColorListItemRow(newSectionColorListItemRow);
            }
          }
          else
          {

            foreach (SectionDataSet.SectionColorListItemRow sectionColorListItemRow in GetSectionColorListItemRows(sectionColorListRow))
            {
              SectionDataSet.SectionColorListItemRow newSectionColorListItemRow = CopySectionColorListItemRow(sectionColorListItemRow, viewSectionDS.SectionColorListItem);
              newSectionColorListItemRow.IdSectionColorList = sectionColorListRow.Id;
              viewSectionDS.SectionColorListItem.AddSectionColorListItemRow(newSectionColorListItemRow);
            }
          }
        }
        catch
        {
          gridColorListItem.DataSource = null;
          viewColorListItem.OptionsBehavior.Editable = false;
          return;
        }
      }

      gridColorListItem.DataSource = viewSectionDS.SectionColorListItem;

      if (selectedRowHandles.Length > 1)
        viewColorListItem.OptionsBehavior.Editable = false;
      else
        viewColorListItem.OptionsBehavior.Editable = true;

      viewColorListItem.RefreshData();
    }

    private decimal ConvertPriceType(decimal initialPrice)
    {
      switch (sectionRow.ConvertedPriceCalculationType)
      {
        case PriceCalculationType.PerSurface:
          return sectionRow.UnitSurface != 0 ? initialPrice / sectionRow.UnitSurface : 0;

        case PriceCalculationType.PerVolume:
          return sectionRow.UnitVolume != 0 ? initialPrice / sectionRow.UnitVolume : 0;

        case PriceCalculationType.PerWeight:
          return sectionRow.UnitWeight != 0 ? initialPrice / sectionRow.UnitWeight : 0;
      }
      return initialPrice;
    }

    /// <summary>
    /// Seteaza sursa de date a gridului de tipuri de materiale in functie de flagul ce indica daca
    /// se folosesc sau nu informatiile de pe profilul brut.
    /// </summary>
    private void SetGridMaterialSpeciesDataSource()
    {
      string filterSectionMaterialSpecies;
      if (chkUseRawSectionMaterialSpecies.Checked)
      {
        filterSectionMaterialSpecies = string.Format("{0} = {1}",
                                      sectionDS.SectionMaterialSpecies.IdSectionColumn.ColumnName,
                                      (int)lookUpRawSection.EditValue);
      }
      else
      {
        filterSectionMaterialSpecies = string.Format("{0} = {1}",
                                      sectionDS.SectionMaterialSpecies.IdSectionColumn.ColumnName,
                                      sectionRow.Id);
      }

      sectionMaterialSpeciesBS.DataSource = sectionDS.SectionMaterialSpecies;
      sectionMaterialSpeciesBS.Filter = filterSectionMaterialSpecies;
      gridMaterialSpecies.DataSource = sectionMaterialSpeciesBS;
      repLookUpMaterialSpeciesUsage.DataSource = EnumTypeLocalizer.Localize<MaterialSpeciesUsage>();
    }

    private void CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
    {
      GridView senderGridView = sender as GridView;
      if (senderGridView == null)
        return;

      if (e.IsSetData || e.Column.FieldName != GetImageColumn(senderGridView).FieldName)
        return;


      int rowHandle = senderGridView.GetRowHandle(e.ListSourceRowIndex);

      SectionDataSet.SectionRow sectionRow = (SectionDataSet.SectionRow)senderGridView.GetDataRow(rowHandle);
      if (sectionRow == null)
      {
        sectionRow = senderGridView.GetRow(rowHandle) as SectionDataSet.SectionRow;
      }

      if (sectionRow != null)
      {
        e.Value = imageDS.Image.GetImage(sectionRow.Guid);
      }
    }

    private void CalcRowHeight(object sender, RowHeightEventArgs e)
    {
      GridView senderGridView = sender as GridView;
      if (senderGridView == null)
        return;

      SectionDataSet.SectionRow sectionRow = (SectionDataSet.SectionRow)senderGridView.GetDataRow(e.RowHandle);
      if (sectionRow == null)
      {
        sectionRow = senderGridView.GetRow(e.RowHandle) as SectionDataSet.SectionRow;
      }

      if (sectionRow != null && imageDS.Image.GetImage(sectionRow.Guid) != null)
      {
        e.RowHeight = 3 * e.RowHeight;
      }
    }

    private void Popup(object sender, EventArgs e)
    {
      bool hasImage = false;
      GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
      if (gridLookUpEdit == null)
        return;

      GridColumn colImage = GetImageColumn(gridLookUpEdit.Properties.View);

      for (int i = 0; i < gridLookUpEdit.Properties.View.DataRowCount; i++)
      {
        SectionDataSet.SectionRow sectionRow = (SectionDataSet.SectionRow)gridLookUpEdit.Properties.View.GetDataRow(i);
        if (sectionRow == null)
        {
          sectionRow = gridLookUpEdit.Properties.View.GetRow(i) as SectionDataSet.SectionRow;
        }
        if (sectionRow != null && imageDS.Image.GetImage(sectionRow.Guid) != null)
        {
          hasImage = true;
          break;
        }
      }
      if (!hasImage)
      {
        colImage.Visible = false;
      }
      else
      {
        colImage.Visible = true;
      }
    }

    /// <summary>
    /// Intoarce coloana in care este afisata imaginea in cazul unui view.
    /// </summary>
    /// <param name="view">View-ul in care se cauta coloana in care se afiseaza o imagine</param>
    /// <returns>Coloana ce afiseaza imaginea.</returns>
    private GridColumn GetImageColumn(GridView view)
    {
      foreach (GridColumn colImage in view.Columns)
      {
        if (colImage.ColumnEdit is RepositoryItemPictureEdit ||
          colImage.ColumnEdit is RepositoryItemImageEdit)
        {
          Debug.Assert(colImage.UnboundType != UnboundColumnType.Bound, "Image column should be unbound!");
          return colImage;
        }
      }
      return null;
    }

    #endregion Private Methods

    private void viewReinforcements_RowCountChanged(object sender, EventArgs e)
    {
      if (viewReinforcements.DataRowCount == 0)
      {
        groupReinforcement.Enabled = false;
        gridLookUpDefaultReinforcement.EditValue = null;
        UpdateIdReinforcementColumn(false);
      }
      if (viewReinforcements.DataRowCount > 0)
      {
        groupReinforcement.Enabled = openingMode != FormOpeningMode.Locked;
      }
      colIdReinforcement.OptionsColumn.AllowEdit = openingMode != FormOpeningMode.Locked;
      colListIdReinforcement.OptionsColumn.AllowEdit = openingMode != FormOpeningMode.Locked;
      SetDefaultReinforcementDataSource();
    }

    private void SetDefaultReinforcementDataSource()
    {
      SectionDataSet.SectionRow[] sectionRows = new SectionDataSet.SectionRow[viewReinforcements.DataRowCount];
      for (int i = 0; i < viewReinforcements.DataRowCount; i++)
      {
        SectionDataSet.SectionReinforcementRow sectionReinforcementRow = viewReinforcements.GetDataRow(i) as SectionDataSet.SectionReinforcementRow;
        sectionRows[i] = sectionDS.Section.FindById(sectionReinforcementRow.IdSectionReinforcement);
      }
      SectionDataSet.SectionRow[] activeSectionRows = new SectionDataSet.SectionRow[sectionRows.Count()];
      activeSectionRows = sectionRows.Where(sectionReinforcementRow => sectionReinforcementRow.IsActive).ToArray();

      if (sectionRow.IsActive)
      {
        gridLookUpDefaultReinforcement.Properties.DataSource = activeSectionRows;
      }
      else
      {
        gridLookUpDefaultReinforcement.Properties.DataSource = sectionRows;
      }

      repGridLookUpColorListReinforcement.DataSource = sectionRows;
      repGridLookUpColorReinforcement.DataSource = sectionRows;
      if (gridLookUpDefaultReinforcement.EditValue != null && sectionRows.FirstOrDefault(row => row.Id == (int)gridLookUpDefaultReinforcement.EditValue) == null && sectionRows.Length > 0)
      {
        colIdReinforcement.OptionsColumn.AllowEdit = true;
        colListIdReinforcement.OptionsColumn.AllowEdit = true;
      }
    }

    private void viewReinforcements_ValidateRow(object sender, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)
    {
      SetDefaultReinforcementDataSource();
    }

    private void repGridLookUpToleranceSection_EditValueChanged(object sender, EventArgs e)
    {
      try
      {
        GridLookUpEdit lookUp = sender as GridLookUpEdit;
        if (!lookUp.IsPopupOpen)
        {
          int idSection = (int)lookUp.EditValue;
          viewTolerance.SetRowCellValue(viewTolerance.FocusedRowHandle, colToleranceIdSection, idSection);
        }
      }
      catch { }
    }

    private void repGridLookUpHeatTransferSection_EditValueChanged(object sender, EventArgs e)
    {
      try
      {
        GridLookUpEdit lookUp = sender as GridLookUpEdit;
        if (!lookUp.IsPopupOpen)
        {
          int idSection = (int)lookUp.EditValue;
          viewSectionHeatTransferCoefficient.SetRowCellValue(viewSectionHeatTransferCoefficient.FocusedRowHandle, colSHTIdSection, idSection);
        }
      }
      catch { }
    }

    private void repGridLookUpReinforcementCode_EditValueChanged(object sender, ChangingEventArgs e)
    {
#warning is this needed ? If it should refresh the designation after changing the reinforcement it doesn't work, it still needs another click
      try
      {
        GridLookUpEdit lookUp = sender as GridLookUpEdit;
        if (!lookUp.IsPopupOpen)
        {
          int idSection = (int)e.NewValue;
          viewReinforcements.SetRowCellValue(viewReinforcements.FocusedRowHandle, colReinforcementDesignation, idSection);
        }
      }
      catch { }
    }

    private void gridLookUpReinforcement_EditValueChanged(object sender, EventArgs e)
    {
      TextEdit editor = sender as TextEdit;
      if (editor.EditValue != null)
      {
        if (sectionRow.IsIdReinforcementNull() || sectionRow.IdReinforcement != (int)editor.EditValue)
          sectionRow.IdReinforcement = (int)editor.EditValue;
      }
      else
      {
        if (!sectionRow.IsIdReinforcementNull())
          sectionRow.SetIdReinforcementNull();
      }

      SetGridMaterialSpeciesDataSource();
    }


    /// <summary>
    /// Tratare eveniment declansat pentru formatarea coloanei Pret din grid.
    /// </summary>
    private void view_RowCellStyle(object sender, RowCellStyleEventArgs e)
    {
      try
      {
        GridView gridView = (GridView)sender;

        if (gridSectionColorList.IsFocused)
        {
          SectionDataSet.SectionColorListRow sectionColorListRow = gridView.GetDataRow(e.RowHandle) as SectionDataSet.SectionColorListRow;
          if (sectionColorListRow == null)
          {
            return;
          }

          if (e.Column == colPrice)
          {
            if (!sectionColorListRow.IsDefaultPrice)
            {
              e.Appearance.ForeColor = MaterialUtils.ColorPriceModify;
              e.Appearance.BackColor = Color.White;
            }
          }
        }
        else if (gridColorListItem.IsFocused)
        {
          SectionDataSet.SectionColorListItemRow sectionColorListItemRow = gridView.GetDataRow(e.RowHandle) as SectionDataSet.SectionColorListItemRow;
          if (sectionColorListItemRow == null)
          {
            return;
          }

          if (e.Column == colPrice)
          {
            if (!sectionColorListItemRow.IsDefaultPrice)
            {
              e.Appearance.ForeColor = MaterialUtils.ColorPriceModify;
              e.Appearance.BackColor = Color.White;
            }
          }
        }
        else if (gridMaterialSpecies.IsFocused)
        {
          SectionDataSet.SectionMaterialSpeciesRow sectionMaterialSpeciesRow = gridView.GetDataRow(e.RowHandle) as SectionDataSet.SectionMaterialSpeciesRow;
          if (sectionMaterialSpeciesRow == null)
          {
            return;
          }

          if (e.Column == colMaterialSpeciesPrice)
          {
            if (!sectionMaterialSpeciesRow.IsDefaultPrice)
            {
              e.Appearance.ForeColor = MaterialUtils.ColorPriceModify;
              e.Appearance.BackColor = Color.White;
            }
          }
        }


      }
      catch (Exception exception)
      {
        Debug.Assert(false, exception.ToString());
        FrameworkApplication.TreatException(exception);
      }
    }

    private void unitDynamicRawSectionH_ButtonPressed(object sender, ButtonPressedEventArgs e)
    {
      RoundValues(false);
    }

    private void unitDynamicRawSectionW_ButtonPressed(object sender, ButtonPressedEventArgs e)
    {
      RoundValues(true);
    }

    /// <summary>
    /// Rounds the H and W for dynamic raw profiles based on options
    /// </summary>
    private void RoundValues(bool isForW)
    {
      if (isRounding)
      {
        return;
      }

      bool useMinAsFirst;
      decimal dynamicFirstTolerance;
      decimal dynamicOtherTolerance;
      double dynamicFirstPrecision;
      double dynamicOtherPrecision;

      SectionDataSet.SeriesSectionRow seriesSectionRow = sectionRow.GetSeriesSectionRows().FirstOrDefault();
      if (seriesSectionRow != null)
      {
        SeriesDataSet.SeriesRow seriesRow = seriesDS.Series.FindById(seriesSectionRow.IdSeries);
        useMinAsFirst = seriesRow.UseMinAsFirstDynamicDim;
        dynamicFirstTolerance = seriesRow.DynamicFirstTolerance;
        dynamicOtherTolerance = seriesRow.DynamicOtherTolerance;
        dynamicFirstPrecision = Convert.ToDouble(seriesRow.DynamicFirstPrecision);
        dynamicOtherPrecision = Convert.ToDouble(seriesRow.DynamicOtherPrecision);
      }
      else
      {
        useMinAsFirst = Ra.Material.Properties.Settings.Default.UseMinAsFirstDynamicDim;
        dynamicFirstTolerance = Ra.Material.Properties.Settings.Default.DynamicFirstTolerance;
        dynamicOtherTolerance = Ra.Material.Properties.Settings.Default.DynamicOtherTolerance;
        dynamicFirstPrecision = Convert.ToDouble(Ra.Material.Properties.Settings.Default.DynamicFirstPrecision);
        dynamicOtherPrecision = Convert.ToDouble(Ra.Material.Properties.Settings.Default.DynamicOtherPrecision);
      }

      isRounding = true;
      double dynamicW = Convert.ToDouble(unitDynamicRawSectionW.EditValue);
      double dynamicH = Convert.ToDouble(unitDynamicRawSectionH.EditValue);

      if (isForW)
      {
        if (!useMinAsFirst || dynamicH < dynamicW)
        {
          dynamicW = Convert.ToDouble(unitW.EditValue) + Convert.ToDouble(dynamicOtherTolerance);
          if (dynamicOtherPrecision != 0
            && !Utils.IsClose((dynamicW) / dynamicOtherPrecision, Math.Floor((dynamicW) / dynamicOtherPrecision), Utils.Epsilon3))
          {
            dynamicW = (Math.Ceiling(((dynamicW) / dynamicOtherPrecision))) * dynamicOtherPrecision;
          }
        }
        else
        {
          dynamicW = Convert.ToDouble(unitW.EditValue) + Convert.ToDouble(dynamicFirstTolerance);
          if (dynamicFirstPrecision != 0
            && !Utils.IsClose((dynamicW) / dynamicFirstPrecision, Math.Floor((dynamicW) / dynamicFirstPrecision), Utils.Epsilon3))
          {
            dynamicW = (Math.Ceiling(((dynamicW) / dynamicFirstPrecision))) * dynamicFirstPrecision;
          }
        }
      }
      else
      {
        if (!useMinAsFirst || dynamicH < dynamicW)
        {
          dynamicH = Convert.ToDouble(unitH.EditValue) + Convert.ToDouble(dynamicFirstTolerance);
          if (dynamicFirstPrecision != 0
            && !Utils.IsClose((dynamicH) / dynamicFirstPrecision, Math.Floor((dynamicH) / dynamicFirstPrecision), Utils.Epsilon3))
          {
            dynamicH = (Math.Ceiling(((dynamicH) / dynamicFirstPrecision))) * dynamicFirstPrecision;
          }
        }
        else
        {
          dynamicH = Convert.ToDouble(unitH.EditValue) + Convert.ToDouble(dynamicOtherTolerance);
          if (dynamicOtherPrecision != 0
            && !Utils.IsClose((dynamicH) / dynamicOtherPrecision, Math.Floor((dynamicH) / dynamicOtherPrecision), Utils.Epsilon3))
          {
            dynamicH = (Math.Ceiling(((dynamicH) / dynamicOtherPrecision))) * dynamicOtherPrecision;
          }
        }
      }

      //Workaround for imperial values, for some reason when you had imperial the mechanism would convert additionally, this fix seems to have been made on H and W controls also
      Control focusedControl = GetFocusedControl(this, typeof(DevExpress.XtraEditors.BaseControl));
      this.cmdOK.Focus();
      if (isForW)
      {
        unitDynamicRawSectionW.EditValue = dynamicW;
        unitDynamicRawSectionW.Refresh();
      }
      else
      {
        unitDynamicRawSectionH.EditValue = dynamicH;
        unitDynamicRawSectionH.Refresh();
      }

      if (focusedControl != null)
      {
        focusedControl.Focus();
      }
      isRounding = false;
    }

    private void chkHasVariableW_CheckedChanged(object sender, EventArgs e)
    {
      bool hasVariableW = chkHasVariableW.Checked;
      unitMaxW.Enabled = hasVariableW;
      unitWInputTolerance.Enabled = hasVariableW;
    }

    private void chkUseRawSectionMaterialSpecies_CheckedChanged(object sender, EventArgs e)
    {
      if (chkUseRawSectionMaterialSpecies.Checked)
      {
        chkUseMaterialSpeciesDefinition.Enabled = false;
        chkUseMaterialSpeciesDefinition.Checked = false;
      }
      else
      {
        chkUseMaterialSpeciesDefinition.Enabled = true;
      }
    }

    private void chkUseMaterialSpeciesDefinition_CheckedChanged(object sender, EventArgs e)
    {
      SetDefinedMaterialSpeciesValues();
    }

    private void viewCustomSectionItem_ShowingEditor(object sender, CancelEventArgs e)
    {
      if (viewCustomSectionItem.FocusedColumn != colHasVariableW)
      {
        return;
      }
      SectionDataSet.CustomSectionItemRow customSectionItemRow = viewCustomSectionItem.GetDataRow(viewCustomSectionItem.FocusedRowHandle) as SectionDataSet.CustomSectionItemRow;


      if ((viewCustomSectionItem.IsNewItemRow(viewCustomSectionItem.FocusedRowHandle) && viewCustomSectionItem.FocusedColumn.Name == colHasVariableW.Name) ||
          customSectionItemRow == null)
      {
        e.Cancel = true;
        return;
      }

      SectionDataSet.SectionRow childSectionRow = sectionDS.Section.FindById(customSectionItemRow.IdSection);
      if (childSectionRow != null && !childSectionRow.HasVariableW)
      {
        e.Cancel = true;
        return;
      }
    }

    /// <summary>
    /// Allows only alphanumeric characters on tag column.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void repTxtCustomSectionItemTag_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (Char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == '\b')
      {
        e.Handled = false;
      }
      else
      {
        e.Handled = true;
      }
    }

    private void btnAddDxf_Click(object sender, EventArgs e)
    {
      using (OpenFileDialog dialog = new OpenFileDialog())
      {
        dialog.AddExtension = true;
        dialog.DefaultExt = "dxf";
        dialog.Filter = Settings.Default.DxfFilter;

        if (DialogResult.OK != dialog.ShowDialog(FrameworkApplication.FirstApplicationForm))
        {
          return;
        }

        string asciiDxf = System.IO.File.ReadAllText(dialog.FileName);
        ImageConverter converter = new ImageConverter();
        // netDxf.DxfDocument dxf = MaterialInterfaceUtils.PruneDxf(asciiDxf);
        Bitmap bitmap = MaterialInterfaceUtils.GetDxfImage(asciiDxf);
        if (bitmap.Width == 1 || bitmap.Height == 1)
        {
          XtraMessageBox.Show(this,
                              Properties.Resources.MsgInvalidDxf,
                              Properties.Resources.CaptionAttentionMsgBox,
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Exclamation);

          imgDxf.EditValue = null;
        }
        else
        {
          sectionRow.Dxf = asciiDxf;
          imgDxf.EditValue = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
        }
      }
    }

    private void cmdDeleteDxf_Click(object sender, EventArgs e)
    {
      if (XtraMessageBox.Show(this, Properties.Resources.MsgDeleteImageQuestion,
  Properties.Resources.CaptionAttentionMsgBox,
  MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
      {
        imgDxf.Image = null;
        sectionRow.Dxf = null;
      }
    }

    private void imgDxf_ImageChanged(object sender, EventArgs e)
    {
      if (imgDxf.EditValue != null)
      {
        if (imgDxf.Image.Width < imgDxf.Width && imgDxf.Image.Height < imgDxf.Height)
        {
          imgDxf.Properties.SizeMode = PictureSizeMode.Clip;
        }
        else
        {
          imgDxf.Properties.SizeMode = PictureSizeMode.Zoom;
        }
      }
    }


    private void lookUpCurvingMode_EditValueChanged(object sender, EventArgs e)
    {
      UpdateArchControls();
    }

  }
}

