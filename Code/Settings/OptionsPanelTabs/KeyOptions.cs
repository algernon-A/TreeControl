// <copyright file="KeyOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting key options.
    /// </summary>
    internal sealed class KeyOptions
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal KeyOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("KEYS"), tabIndex, out UIButton _, autoLayout: false);

            // Y position indicator.
            float currentY = GroupMargin;

            // Anarchy hotkey control.
            OptionsKeymapping anarchyKeyMapping = OptionsKeymapping.AddKeymapping(panel, LeftMargin, currentY, Translations.Translate("KEY_ANARCHY"), UIThreading.AnarchyKey);
            currentY += anarchyKeyMapping.Panel.height + Margin;

            // Remove anarchy hotkey control.
            OptionsKeymapping removeAnarchyKeyMapping = OptionsKeymapping.AddKeymapping(panel, LeftMargin, currentY, Translations.Translate("KEY_REMOVE_ANARCHY"), UIThreading.RemoveAnarchyKey);
            currentY += removeAnarchyKeyMapping.Panel.height + Margin;

            // Snapping hotkey control.
            OptionsKeymapping snappingKeyMapping = OptionsKeymapping.AddKeymapping(panel, LeftMargin, currentY, Translations.Translate("KEY_SNAPPING"), UIThreading.SnappingKey);
            currentY += snappingKeyMapping.Panel.height + Margin;

            // Lock forestry hotkey control.
            OptionsKeymapping forestryKeyMapping = OptionsKeymapping.AddKeymapping(panel, LeftMargin, currentY, Translations.Translate("KEY_FORESTRY"), UIThreading.ForestryKey);
            currentY += forestryKeyMapping.Panel.height + Margin;

            // Raise elevation key control.
            OptionsKeymapping elevationUpMapping = OptionsKeymapping.AddKeymapping(panel, LeftMargin, currentY, Translations.Translate("KEY_ELEVATION_UP"), UIThreading.ElevationUpKey);
            currentY += elevationUpMapping.Panel.height + Margin;

            // Lower elevation key control.
            OptionsKeymapping elevationDownMapping = OptionsKeymapping.AddKeymapping(panel, LeftMargin, currentY, Translations.Translate("KEY_ELEVATION_DOWN"), UIThreading.ElevationDownKey);
            currentY += elevationDownMapping.Panel.height + Margin;

            // Upscaling key control.
            OptionsKeymapping scaleUpMapping = OptionsKeymapping.AddKeymapping(panel, LeftMargin, currentY, Translations.Translate("KEY_SCALE_UP"), UIThreading.ScaleUpKey);
            currentY += scaleUpMapping.Panel.height + Margin;

            // Downscaling key control.
            OptionsKeymapping scaleDownMapping = OptionsKeymapping.AddKeymapping(panel, LeftMargin, currentY, Translations.Translate("KEY_SCALE_DOWN"), UIThreading.ScaleDownKey);
            currentY += scaleDownMapping.Panel.height + 20f;

            // Key repeat delay slider.
            UISlider keyDelaySlider = UISliders.AddPlainSliderWithValue(panel, LeftMargin, currentY, Translations.Translate("REPEAT_DELAY"), 0.1f, 1.0f, 0.05f, UIThreading.KeyRepeatDelay);
            keyDelaySlider.eventValueChanged += (c, value) => UIThreading.KeyRepeatDelay = value;
        }
    }
}