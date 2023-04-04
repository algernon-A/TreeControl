// <copyright file="StatusLabel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;
    using TreeControl.Patches;
    using UnityEngine;

    /// <summary>
    /// Simple text status label for mod mode display.
    /// </summary>
    internal class StatusLabel : UIComponent
    {
        // Components.
        private static GameObject s_gameObject;
        private UILabel _titleLabel;
        private UILabel _onLabel;
        private UILabel _offLabel;

        /// <summary>
        /// Called by Unity before first frame.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Add the text label.
            _titleLabel = GameObject.Find("OptionsBar").GetComponent<UIPanel>().AddUIComponent<UILabel>();
            _titleLabel.relativePosition = new Vector2(0f, 70f);
            _titleLabel.text = Translations.Translate("ANARCHY_STATUS");

            // On and off labels.
            _onLabel = _titleLabel.AddUIComponent<UILabel>();
            _onLabel.text = Translations.Translate("ON");
            _onLabel.relativePosition = new Vector2(_titleLabel.width + 5f, 0f);

            _offLabel = _titleLabel.AddUIComponent<UILabel>();
            _offLabel.text = Translations.Translate("OFF");
            _offLabel.relativePosition = new Vector2(_titleLabel.width + 5f, 0f);
        }

        /// <summary>
        /// Called by Unity every frame.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // Set label text.
            bool anarchyEnabled = TreeToolPatches.AnarchyEnabled;
            _onLabel.isVisible = anarchyEnabled;
            _offLabel.isVisible = !anarchyEnabled;
        }

        /// <summary>
        /// Create the status label on the screen.
        /// </summary>
        internal static void CreateLabel()
        {
            // Don't create label if one already exists.
            if (s_gameObject == null)
            {
                // Activate display label.
                s_gameObject = new GameObject("TreeAnarchyStatus");
                UIView view = UIView.GetAView();
                s_gameObject.transform.parent = view.transform;

                // Create new panel instance and add it to GameObject.
                s_gameObject.AddComponent<StatusLabel>();
            }
        }
    }
}
