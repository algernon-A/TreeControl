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
        private const float GroupMargin = 40f;
        private const float TitleMargin = 50f;

        // Panel components.
        private readonly UICheckBox _leaveOnLoadCheck;
        private readonly UICheckBox _hideOnLoadCheck;
        private readonly UICheckBox _unhideOnLoadCheck;

        // Event handling.
        private bool _suspendEvents = false;

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
            float currentY = GroupMargin;

            // Language choice.
            UIDropDown languageDropDown = UIDropDowns.AddPlainDropDown(panel, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };
            languageDropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += languageDropDown.parent.height + Margin;

            // Leave on load check.
            _leaveOnLoadCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("LEAVE_ON_LOAD"));
            _leaveOnLoadCheck.tooltip = Translations.Translate("LEAVE_ON_LOAD_TIP");
            _leaveOnLoadCheck.objectUserData = LoadingForceMode.None;
            _leaveOnLoadCheck.isChecked = TreeInstancePatches.ForceOnLoad == LoadingForceMode.None;
            _leaveOnLoadCheck.eventCheckChanged += LoadCheckChanged;
            currentY += _leaveOnLoadCheck.height;

            // Hide on load check.
            _hideOnLoadCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("HIDE_ON_LOAD"));
            _hideOnLoadCheck.tooltip = Translations.Translate("HIDE_ON_LOAD_TIP");
            _hideOnLoadCheck.objectUserData = LoadingForceMode.HideAll;
            _hideOnLoadCheck.isChecked = TreeInstancePatches.ForceOnLoad == LoadingForceMode.HideAll;
            _hideOnLoadCheck.eventCheckChanged += LoadCheckChanged;
            currentY += _hideOnLoadCheck.height;

            // Hide on load check.
            _unhideOnLoadCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("UNHIDE_ON_LOAD"));
            _unhideOnLoadCheck.tooltip = Translations.Translate("UNHIDE_ON_LOAD_TIP");
            _unhideOnLoadCheck.objectUserData = LoadingForceMode.UnhideAll;
            _unhideOnLoadCheck.isChecked = TreeInstancePatches.ForceOnLoad == LoadingForceMode.UnhideAll;
            _unhideOnLoadCheck.eventCheckChanged += LoadCheckChanged;
            currentY += GroupMargin;

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
            currentY += keepAboveGroundCheck.height + 20f;

            UISlider swayFactorSlider = UISliders.AddPlainSliderWithPercentage(panel, LeftMargin, currentY, Translations.Translate("SWAY_FACTOR"), TreeInstancePatches.MinSwayFactor, TreeInstancePatches.MaxSwayFactor, 0.01f, TreeInstancePatches.SwayFactor);
            swayFactorSlider.eventValueChanged += (c, value) => TreeInstancePatches.SwayFactor = value;
            currentY += swayFactorSlider.parent.height + 15f;

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
            currentY += lodDropDown.parent.height + 20f;

            // Hide on load check.
            UICheckBox lockForestryCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("LOCK_FORESTRY"));
            lockForestryCheck.tooltip = Translations.Translate("LOCK_FORESTRY_TIP");
            lockForestryCheck.isChecked = NaturalResourceManagerPatches.LockForestry;
            lockForestryCheck.eventCheckChanged += (c, isChecked) => { NaturalResourceManagerPatches.LockForestry = isChecked; };
            currentY += lockForestryCheck.height + GroupMargin;

            // Troubleshooting options.
            float headerWidth = OptionsPanelManager<OptionsPanel>.PanelWidth - (Margin * 2f);
            UISpacers.AddTitleSpacer(panel, Margin, currentY, headerWidth, Translations.Translate("TROUBLESHOOTING"));
            currentY += TitleMargin;

            // Ignore Tree Anarchy data.
            UICheckBox ignoreTreeAnarchyCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("IGNORE_TA_DATA"));
            ignoreTreeAnarchyCheck.tooltip = Translations.Translate("IGNORE_TA_DATA_TIP");
            ignoreTreeAnarchyCheck.isChecked = TreeManagerDataPatches.IgnoreTreeAnarchyData;
            ignoreTreeAnarchyCheck.eventCheckChanged += (c, isChecked) => { TreeManagerDataPatches.IgnoreTreeAnarchyData = isChecked; };
            currentY += GroupMargin;
            ignoreTreeAnarchyCheck.tooltipBox = UIToolTips.WordWrapToolTip;

            // Logging checkbox.
            UICheckBox loggingCheck = UICheckBoxes.AddPlainCheckBox(panel, LeftMargin, currentY, Translations.Translate("DETAIL_LOGGING"));
            loggingCheck.isChecked = Logging.DetailLogging;
            loggingCheck.eventCheckChanged += (c, isChecked) => { Logging.DetailLogging = isChecked; };
        }

        /// <summary>
        /// 'Hide on load' checkbox event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="isChecked">New checkbox state.</param>
        private void LoadCheckChanged(UIComponent c, bool isChecked)
        {
            // Don't do anything if events are suspended.
            if (_suspendEvents)
            {
                return;
            }

            // Suspend event handlng.
            _suspendEvents = true;

            // Assign value if this is now checked.
            if (isChecked && c.objectUserData is LoadingForceMode foreeMode)
            {
                TreeInstancePatches.ForceOnLoad = foreeMode;
            }

            // Update checkboxes to reflect new state.
            _leaveOnLoadCheck.isChecked = false;
            _hideOnLoadCheck.isChecked = false;
            _unhideOnLoadCheck.isChecked = false;
            switch (TreeInstancePatches.ForceOnLoad)
            {
                default:
                case LoadingForceMode.None:
                    _leaveOnLoadCheck.isChecked = true;
                    break;
                case LoadingForceMode.HideAll:
                    _hideOnLoadCheck.isChecked = true;
                    break;
                case LoadingForceMode.UnhideAll:
                    _unhideOnLoadCheck.isChecked = true;
                    break;
            }

            // Resume event handling.
            _suspendEvents = false;
        }
    }
}