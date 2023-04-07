// <copyright file="MoreTreesOptions.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using static Patches.TreeManagerDataPatches;

    /// <summary>
    /// Options panel for setting tree limit options.
    /// </summary>
    internal sealed class MoreTreesOptions
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float LeftMargin = 24f;
        private const float GroupMargin = 40f;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoreTreesOptions"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal MoreTreesOptions(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("OPTIONS_LIMIT"), tabIndex, out UIButton _, autoLayout: false);

            // Y position indicator.
            float currentY = GroupMargin;

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
        }
    }
}