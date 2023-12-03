// <copyright file="TreeOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using TreeControl.Patches;
    using static Patches.TreeManagerDataPatches;

    /// <summary>
    /// Options panel for setting tree-related options.
    /// </summary>
    internal sealed class TreeOptions
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
        /// Initializes a new instance of the <see cref="TreeOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal TreeOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("OPTIONS_TREES"), tabIndex, out UIButton _, autoLayout: false);

            // Y position indicator.
            float currentY = Margin;

            // Header.
            float headerWidth = OptionsPanelManager<OptionsPanel>.PanelWidth - (Margin * 2f);

            // Tree limit.
            UISpacers.AddTitleSpacer(panel, Margin, currentY, headerWidth, Translations.Translate("OPTIONS_LIMIT"));
            currentY += TitleMargin;

            UISlider treeLimitSlider = UISliders.AddPlainSliderWithIntegerValue(
                panel,
                LeftMargin,
                currentY,
                Translations.Translate("TREE_LIMIT"),
                MinCustomTreeLimit,
                MaxCustomTreeLimit,
                TreeManager.MAX_TREE_COUNT,
                CustomTreeLimit);
            treeLimitSlider.eventValueChanged += (c, value) => CustomTreeLimit = (int)value;
            treeLimitSlider.tooltip = Translations.Translate("TREE_LIMIT_TIP");
            treeLimitSlider.tooltipBox = UIToolTips.WordWrapToolTip;
            currentY += treeLimitSlider.parent.height + Margin;

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
            currentY += lockForestryCheck.height + 10f;

            // Random rotation check.
            UICheckBox randomRotationCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("RANDOM_ROTATION"));
            randomRotationCheck.tooltip = Translations.Translate("RANDOM_ROTATION_TIP");
            randomRotationCheck.isChecked = TreeInstancePatches.RandomRotation;
            randomRotationCheck.eventCheckChanged += (c, isChecked) => { TreeInstancePatches.RandomRotation = isChecked; };
            currentY += randomRotationCheck.height + 10f;

            // Tree sway factor.
            UISlider swayFactorSlider = UISliders.AddPlainSliderWithPercentage(panel, LeftMargin, currentY, Translations.Translate("SWAY_FACTOR"), TreeInstancePatches.MinSwayFactor, TreeInstancePatches.MaxSwayFactor, 0.01f, TreeInstancePatches.SwayFactor);
            swayFactorSlider.eventValueChanged += (c, value) => TreeInstancePatches.SwayFactor = value;
            currentY += swayFactorSlider.parent.height;

            // Disable distant tree swaying.
            UICheckBox disableDistantSwayCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("STOP_DISTANT_SWAY"));
            disableDistantSwayCheck.tooltip = Translations.Translate("STOP_DISTANT_SWAY_TIP");
            disableDistantSwayCheck.isChecked = TreeInstancePatches.DisableDistantSway;
            disableDistantSwayCheck.eventCheckChanged += (c, isChecked) => { TreeInstancePatches.DisableDistantSway = isChecked; };
            currentY += disableDistantSwayCheck.height + 10f;

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