// <copyright file="GeneralOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using TreeControl.Patches;
    using UnityEngine;

    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal sealed class GeneralOptions
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float LeftMargin = 24f;
        private const float TitleMargin = 50f;
        private const float MenuX = LeftMargin + 300f;

        // Panel components.
        private readonly UIDropDown _networkDropDown;
        private readonly UIDropDown _buildingDropDown;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal GeneralOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("OPTIONS_GENERAL"), tabIndex, out UIButton _, autoLayout: false);

            // Y position indicator.
            float currentY = 0f;

            // Header.
            float headerWidth = OptionsPanelManager<OptionsPanel>.PanelWidth - (Margin * 2f);

            // UI options.
            UISpacers.AddTitleSpacer(panel, Margin, currentY, headerWidth, Translations.Translate("UI_OPTIONS"));
            currentY += TitleMargin;

            // Language choice.
            UIDropDown languageDropDown = UIDropDowns.AddPlainDropDown(panel, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };
            languageDropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += languageDropDown.parent.height + Margin;

            // UI transparency checkbox.
            UICheckBox transparencyCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("TRANSPARENT_UI"));
            transparencyCheck.isChecked = StatusPanel.TransparentUI;
            transparencyCheck.eventCheckChanged += (c, isChecked) => { StatusPanel.TransparentUI = isChecked; };
            currentY += transparencyCheck.height + Margin;

            UIButton resetPositionButton = UIButtons.AddButton(panel, LeftMargin, currentY, Translations.Translate("RESET_POS"), 300f);
            resetPositionButton.eventClicked += (c, p) => StandalonePanelManager<StatusPanel>.ResetPosition();
            currentY += resetPositionButton.height + 15f;

            // Loading options.
            UISpacers.AddTitleSpacer(panel, Margin, currentY, headerWidth, Translations.Translate("LOAD_OPTIONS"));
            currentY += TitleMargin;

            string[] loadOptions = new string[]
            {
                Translations.Translate("LEAVE"),
                Translations.Translate("HIDE"),
                Translations.Translate("UNHIDE"),
                Translations.Translate("DELETE"),
            };

            // Network hiding options.
            _networkDropDown = OverlapDropdown(panel, currentY, "NETWORK_OVERLAP", loadOptions);
            _networkDropDown.selectedIndex = (int)TreeInstancePatches.NetworkOverlap;
            _networkDropDown.eventSelectedIndexChanged += (c, index) => TreeInstancePatches.NetworkOverlap = (OverlapMode)index;
            currentY += _networkDropDown.height + Margin;

            // Building hiding options.
            _buildingDropDown = OverlapDropdown(panel, currentY, "BUILDING_OVERLAP", loadOptions);
            _buildingDropDown.selectedIndex = (int)TreeInstancePatches.BuildingOverlap;
            _buildingDropDown.eventSelectedIndexChanged += (c, index) => TreeInstancePatches.BuildingOverlap = (OverlapMode)index;
            currentY += TitleMargin;

            // Tree options.
            UISpacers.AddTitleSpacer(panel, Margin, currentY, headerWidth, Translations.Translate("TREE_OPTIONS"));
            currentY += TitleMargin;

            // Update on terrain change checkboxes.
            UICheckBox terrainUpdateCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("TERRAIN_UPDATE"));
            terrainUpdateCheck.tooltip = Translations.Translate("TERRAIN_UPDATE_TIP");
            terrainUpdateCheck.isChecked = TreeInstancePatches.UpdateOnTerrain;
            terrainUpdateCheck.eventCheckChanged += (c, isChecked) => { TreeInstancePatches.UpdateOnTerrain = isChecked; };
            currentY += terrainUpdateCheck.height;

            UICheckBox keepAboveGroundCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("KEEP_ABOVEGROUND"));
            keepAboveGroundCheck.tooltip = Translations.Translate("KEEP_ABOVEGROUND_TIP");
            keepAboveGroundCheck.isChecked = TreeInstancePatches.KeepAboveGround;
            keepAboveGroundCheck.eventCheckChanged += (c, isChecked) => { TreeInstancePatches.KeepAboveGround = isChecked; };
            currentY += keepAboveGroundCheck.height;

            // Lock forestry check.
            UICheckBox lockForestryCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("LOCK_FORESTRY_DEFAULT"));
            lockForestryCheck.tooltip = Translations.Translate("LOCK_FORESTRY_DEFAULT_TIP");
            lockForestryCheck.isChecked = NaturalResourceManagerPatches.LockForestryDefault;
            lockForestryCheck.eventCheckChanged += (c, isChecked) => { NaturalResourceManagerPatches.LockForestryDefault = isChecked; };
            currentY += lockForestryCheck.height + Margin;

            UISlider swayFactorSlider = UISliders.AddPlainSliderWithPercentage(panel, LeftMargin, currentY, Translations.Translate("SWAY_FACTOR"), TreeInstancePatches.MinSwayFactor, TreeInstancePatches.MaxSwayFactor, 0.01f, TreeInstancePatches.SwayFactor);
            swayFactorSlider.eventValueChanged += (c, value) => TreeInstancePatches.SwayFactor = value;
            currentY += swayFactorSlider.parent.height;

            // Tree LOD detail.
            string[] lodDetailLevels = new string[(int)TreeLODControl.Resolution.NumResolutions]
                {
                    Translations.Translate("DETAIL_LOW"),
                    Translations.Translate("DETAIL_MED"),
                    Translations.Translate("DETAIL_HIGH"),
                    Translations.Translate("DETAIL_ULTRA"),
                    Translations.Translate("DETAIL_INSANE"),
                };
            UIDropDown lodDropDown = UIDropDowns.AddPlainDropDown(panel, LeftMargin, currentY, Translations.Translate("LOD_DETAIL"), lodDetailLevels, (int)TreeLODControl.CurrentResolution, 350f);
            lodDropDown.eventSelectedIndexChanged += (c, index) => TreeLODControl.CurrentResolution = (TreeLODControl.Resolution)index;
            currentY += lodDropDown.parent.height + 10f;

            // Troubleshooting options.
            UISpacers.AddTitleSpacer(panel, Margin, currentY, headerWidth, Translations.Translate("TROUBLESHOOTING"));
            currentY += TitleMargin;

            // Ignore Tree Anarchy data.
            UICheckBox ignoreTreeAnarchyCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("IGNORE_TA_DATA"));
            ignoreTreeAnarchyCheck.tooltip = Translations.Translate("IGNORE_TA_DATA_TIP");
            ignoreTreeAnarchyCheck.isChecked = TreeManagerDataPatches.IgnoreTreeAnarchyData;
            ignoreTreeAnarchyCheck.eventCheckChanged += (c, isChecked) => { TreeManagerDataPatches.IgnoreTreeAnarchyData = isChecked; };
            currentY += ignoreTreeAnarchyCheck.height;
            ignoreTreeAnarchyCheck.tooltipBox = UIToolTips.WordWrapToolTip;

            // Logging checkbox.
            UICheckBox loggingCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("DETAIL_LOGGING"));
            loggingCheck.isChecked = Logging.DetailLogging;
            loggingCheck.eventCheckChanged += (c, isChecked) => { Logging.DetailLogging = isChecked; };
        }

        /// <summary>
        /// Adds an overlap mode dropdown.
        /// </summary>
        /// <param name="parent">Parent <see cref="UIComponent"/>.</param>
        /// <param name="yPos">Relative Y position.</param>
        /// <param name="titleKey">Title label translation key.</param>
        /// <param name="items">Menu items.</param>
        /// <returns>New <see cref="UIDropDown"/>.</returns>
        private UIDropDown OverlapDropdown(UIComponent parent, float yPos, string titleKey, string[] items)
        {
            UIDropDown dropDown = UIDropDowns.AddLabelledDropDown(
                parent,
                MenuX,
                yPos,
                Translations.Translate(titleKey),
                height: 30f,
                width: 320f,
                labelTextScale: 1f,
                itemTextScale: 1f,
                itemHeight: 24,
                vertPadding: 7,
                itemVertPadding: 4,
                accomodateLabel: false);

            dropDown.items = items;
            return dropDown;
        }
    }
}