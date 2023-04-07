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
    using UnityEngine;

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
            OptionsKeymapping anarchyKeyMapping = panel.gameObject.AddComponent<OptionsKeymapping>();
            anarchyKeyMapping.Label = Translations.Translate("KEY_ANARCHY");
            anarchyKeyMapping.Binding = UIThreading.AnarchyKey;
            anarchyKeyMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += anarchyKeyMapping.Panel.height + Margin;

            // Raise elevation key control.
            OptionsKeymapping elevationUpMapping = panel.gameObject.AddComponent<OptionsKeymapping>();
            elevationUpMapping.Label = Translations.Translate("KEY_ELEVATION_UP");
            elevationUpMapping.Binding = UIThreading.ElevationUpKey;
            elevationUpMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += elevationUpMapping.Panel.height + Margin;

            // Lower elevation key control.
            OptionsKeymapping elevationDownMapping = panel.gameObject.AddComponent<OptionsKeymapping>();
            elevationDownMapping.Label = Translations.Translate("KEY_ELEVATION_DOWN");
            elevationDownMapping.Binding = UIThreading.ElevationDownKey;
            elevationDownMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += elevationDownMapping.Panel.height + Margin;

            // Upscaling key control.
            OptionsKeymapping scaleUpMapping = panel.gameObject.AddComponent<OptionsKeymapping>();
            scaleUpMapping.Label = Translations.Translate("KEY_SCALE_UP");
            scaleUpMapping.Binding = UIThreading.ScaleUpKey;
            scaleUpMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += scaleUpMapping.Panel.height + Margin;

            // Downscaling key control.
            OptionsKeymapping scaleDownMapping = panel.gameObject.AddComponent<OptionsKeymapping>();
            scaleDownMapping.Label = Translations.Translate("KEY_SCALE_DOWN");
            scaleDownMapping.Binding = UIThreading.ScaleDownKey;
            scaleDownMapping.Panel.relativePosition = new Vector2(LeftMargin, currentY);
            currentY += scaleDownMapping.Panel.height + 20f;

            UISlider keyDelaySlider = UISliders.AddPlainSliderWithValue(panel, LeftMargin, currentY, Translations.Translate("REPEAT_DELAY"), 0.1f, 1.0f, 0.05f, UIThreading.KeyRepeatDelay);
            keyDelaySlider.eventValueChanged += (c, value) => UIThreading.KeyRepeatDelay = value;
        }
    }
}