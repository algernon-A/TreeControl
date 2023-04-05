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

            // Update on terrain change checkboxes.
            UICheckBox terrainUpdateCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("TERRAIN_UPDATE"));
            terrainUpdateCheck.tooltip = Translations.Translate("TERRAIN_UPDATE_TIP");
            terrainUpdateCheck.isChecked = TreeInstancePatches.UpdateOnTerrain;
            terrainUpdateCheck.eventCheckChanged += (c, isChecked) => { TreeInstancePatches.UpdateOnTerrain = isChecked; };
            currentY += terrainUpdateCheck.height + 20f;

            UICheckBox keepAboveGroundCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("KEEP_ABOVEGROUND"));
            keepAboveGroundCheck.tooltip = Translations.Translate("KEEP_ABOVEGROUND_TIP");
            keepAboveGroundCheck.isChecked = TreeInstancePatches.KeepAboveGround;
            keepAboveGroundCheck.eventCheckChanged += (c, isChecked) => { TreeInstancePatches.KeepAboveGround = isChecked; };
            currentY += keepAboveGroundCheck.height + 20f;

            // Hide on load check.
            UICheckBox hideOnLoadCheck = UICheckBoxes.AddPlainCheckBox(this, LeftMargin, currentY, Translations.Translate("HIDE_ON_LOAD"));
            hideOnLoadCheck.tooltip = Translations.Translate("HIDE_ON_LOAD_TIP");
            hideOnLoadCheck.isChecked = TreeInstancePatches.HideOnLoad;
            hideOnLoadCheck.eventCheckChanged += (c, isChecked) => { TreeInstancePatches.HideOnLoad = isChecked; };
            currentY += GroupMargin;

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
        }
    }
}