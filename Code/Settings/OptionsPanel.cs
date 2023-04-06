// <copyright file="OptionsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using AlgernonCommons;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using TreeControl.Patches;
    using UnityEngine;

    /// <summary>
    /// The mod's settings options panel.
    /// </summary>
    public class OptionsPanel : UIPanel
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;
        private const float TitleMargin = 50f;

        /// <summary>
        /// Called by Unity before the first frame.
        /// Used to perform setup.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Add controls.
            // Y position indicator.
            float currentY = Margin;

            // Language choice.
            UIDropDown languageDropDown = UIDropDowns.AddPlainDropDown(this, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };
            languageDropDown.parent.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += languageDropDown.parent.height + Margin;

            // Logging checkbox.
            UICheckBox loggingCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("DETAIL_LOGGING"));
            loggingCheck.isChecked = Logging.DetailLogging;
            loggingCheck.eventCheckChanged += (c, isChecked) => { Logging.DetailLogging = isChecked; };
            currentY += GroupMargin;

            // Hide on load check.
            UICheckBox hideOnLoadCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("HIDE_ON_LOAD"));
            hideOnLoadCheck.tooltip = Translations.Translate("HIDE_ON_LOAD_TIP");
            hideOnLoadCheck.isChecked = TreeInstancePatches.HideOnLoad;
            hideOnLoadCheck.eventCheckChanged += (c, isChecked) => { TreeInstancePatches.HideOnLoad = isChecked; };
            currentY += GroupMargin;

            // Update on terrain change checkboxes.
            UICheckBox terrainUpdateCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("TERRAIN_UPDATE"));
            terrainUpdateCheck.tooltip = Translations.Translate("TERRAIN_UPDATE_TIP");
            terrainUpdateCheck.isChecked = TreeInstancePatches.UpdateOnTerrain;
            terrainUpdateCheck.eventCheckChanged += (c, isChecked) => { TreeInstancePatches.UpdateOnTerrain = isChecked; };
            currentY += terrainUpdateCheck.height;

            UICheckBox keepAboveGroundCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("KEEP_ABOVEGROUND"));
            keepAboveGroundCheck.tooltip = Translations.Translate("KEEP_ABOVEGROUND_TIP");
            keepAboveGroundCheck.isChecked = TreeInstancePatches.KeepAboveGround;
            keepAboveGroundCheck.eventCheckChanged += (c, isChecked) => { TreeInstancePatches.KeepAboveGround = isChecked; };
            currentY += keepAboveGroundCheck.height + GroupMargin;

            UISlider swayFactorSlider = UISliders.AddPlainSliderWithPercentage(this, LeftMargin, currentY, Translations.Translate("SWAY_FACTOR"), TreeInstancePatches.MinSwayFactor, TreeInstancePatches.MaxSwayFactor, 0.01f, TreeInstancePatches.SwayFactor);
            swayFactorSlider.eventValueChanged += (c, value) => TreeInstancePatches.SwayFactor = value;
            currentY += swayFactorSlider.parent.height + GroupMargin;

            // Key options.
            float headerWidth = OptionsPanelManager<OptionsPanel>.PanelWidth - (Margin * 2f);
            UISpacers.AddTitleSpacer(this, Margin, currentY, headerWidth, Translations.Translate("KEYS"));
            currentY += TitleMargin;

            // Anarchy hotkey control.
            OptionsKeymapping anarchyKeyMapping = this.gameObject.AddComponent<OptionsKeymapping>();
            anarchyKeyMapping.Label = Translations.Translate("KEY_ANARCHY");
            anarchyKeyMapping.Binding = UIThreading.AnarchyKey;
            anarchyKeyMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += anarchyKeyMapping.Panel.height + Margin;

            // Raise elevation key control.
            OptionsKeymapping elevationUpMapping = this.gameObject.AddComponent<OptionsKeymapping>();
            elevationUpMapping.Label = Translations.Translate("KEY_ELEVATION_UP");
            elevationUpMapping.Binding = UIThreading.ElevationUpKey;
            elevationUpMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += elevationUpMapping.Panel.height + Margin;

            // Upscaling key control.
            OptionsKeymapping scaleUpMapping = this.gameObject.AddComponent<OptionsKeymapping>();
            scaleUpMapping.Label = Translations.Translate("KEY_SCALE_UP");
            scaleUpMapping.Binding = UIThreading.ScaleUpKey;
            scaleUpMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += scaleUpMapping.Panel.height + Margin;

            // Downscaling key control.
            OptionsKeymapping scaleDownMapping = this.gameObject.AddComponent<OptionsKeymapping>();
            scaleDownMapping.Label = Translations.Translate("KEY_SCALE_DOWN");
            scaleDownMapping.Binding = UIThreading.ScaleDownKey;
            scaleDownMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += scaleDownMapping.Panel.height + GroupMargin;

            // Lower elevation key control.
            OptionsKeymapping elevationDownMapping = this.gameObject.AddComponent<OptionsKeymapping>();
            elevationDownMapping.Label = Translations.Translate("KEY_ELEVATION_DOWN");
            elevationDownMapping.Binding = UIThreading.ElevationDownKey;
            elevationDownMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += elevationDownMapping.Panel.height + Margin;
        }
    }
}