// <copyright file="StatusPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using System.Text;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;
    using TreeControl.Patches;
    using UnityEngine;

    /// <summary>
    /// Icon status panel.
    /// </summary>
    internal class StatusPanel : StandalonePanelBase
    {
        // Layout constants.
        private const float ButtonSize = 36f;

        // Transparency status.
        private static bool s_transparentUI = false;

        // Panel components.
        private UIMultiStateButton _anarchyButton;
        private UIMultiStateButton _snappingButton;
        private UIMultiStateButton _lockForestryButton;

        // Event handling.
        private bool _ignoreEvents = false;

        /// <summary>
        /// Gets or sets a value indicating whether the status panel should use transparent buttons.
        /// </summary>
        public static bool TransparentUI
        {
            get => s_transparentUI;

            set
            {
                // Don't do anything if no change.
                if (value != s_transparentUI)
                {
                    s_transparentUI = value;

                    // Regnerate status panel if open.
                    if (StandalonePanelManager<StatusPanel>.Panel is StatusPanel panel)
                    {
                        panel.Close();
                        StandalonePanelManager<StatusPanel>.Create();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the panel width.
        /// </summary>
        public override float PanelWidth => (ButtonSize * 3f) + (Margin * 2f);

        /// <summary>
        /// Gets the panel height.
        /// </summary>
        public override float PanelHeight => ButtonSize;

        /// <summary>
        /// Called by Unity before the first frame.
        /// Used to perform setup.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Draghandle.
            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.relativePosition = Vector2.zero;
            dragHandle.width = PanelWidth;
            dragHandle.height = PanelHeight;
            dragHandle.target = this;

            // Remember position.
            dragHandle.eventMouseUp += (c, p) =>
            {
                StandalonePanelManager<StatusPanel>.LastSavedXPosition = absolutePosition.x;
                StandalonePanelManager<StatusPanel>.LastSavedYPosition = absolutePosition.y;
                ModSettings.Save();
            };

            // Options panel toggles.
            UITextureAtlas tcAtlas = UITextures.CreateSpriteAtlas("TreeControl", 1024, string.Empty);

            _snappingButton = AddToggleButton(this, "Tree snapping status", tcAtlas, "SnappingOff", "SnappingOn");
            _snappingButton.tooltipBox = UIToolTips.WordWrapToolTip;
            _snappingButton.relativePosition = Vector2.zero;
            _snappingButton.eventActiveStateIndexChanged += (c, state) =>
            {
                // Don't do anything if ignoring events.
                if (!_ignoreEvents)
                {
                    TreeToolPatches.SnappingEnabled = state != 0;
                }
            };

            _anarchyButton = AddToggleButton(this, "Tree anarchy status", tcAtlas, "AnarchyOff", "AnarchyOn");
            _anarchyButton.relativePosition = new Vector2(ButtonSize + Margin, 0f);
            _anarchyButton.tooltipBox = UIToolTips.WordWrapToolTip;
            _anarchyButton.eventActiveStateIndexChanged += (c, state) =>
            {
                // Don't do anything if ignoring events.
                if (!_ignoreEvents)
                {
                    TreeManagerPatches.AnarchyEnabled = state != 0;
                }
            };

            _lockForestryButton = AddToggleButton(this, "Tree snapping status", tcAtlas, "LockForestOff", "LockForestOn");
            _lockForestryButton.tooltipBox = UIToolTips.WordWrapToolTip;
            _lockForestryButton.relativePosition = new Vector2((ButtonSize + Margin) * 2f, 0f);
            _lockForestryButton.eventActiveStateIndexChanged += (c, state) =>
            {
                // Don't do anything if ignoring events.
                if (!_ignoreEvents)
                {
                    NaturalResourceManagerPatches.LockForestry = state != 0;
                }
            };

            // Set intial button states.
            Refresh();
        }

        /// <summary>
        /// Applies the panel's default position.
        /// </summary>
        public override void ApplyDefaultPosition()
        {
            // Set position.
            UIComponent optionsBar = GameObject.Find("OptionsBar").GetComponent<UIComponent>();
            absolutePosition = optionsBar.absolutePosition - new Vector3(PanelWidth + Margin, 0f);
        }

        /// <summary>
        /// Refreshes button states.
        /// </summary>
        internal void Refresh()
        {
            // Suppress events while changing state.
            _ignoreEvents = true;
            _snappingButton.activeStateIndex = TreeToolPatches.SnappingEnabled ? 1 : 0;
            _anarchyButton.activeStateIndex = TreeManagerPatches.AnarchyEnabled ? 1 : 0;
            _lockForestryButton.activeStateIndex = NaturalResourceManagerPatches.LockForestry ? 1 : 0;
            _ignoreEvents = false;

            StringBuilder tooltipText = new StringBuilder();

            // Set button tooltips.
            tooltipText.Append(Translations.Translate("SNAPPING_STATUS"));
            tooltipText.Append(' ');
            tooltipText.AppendLine(TreeToolPatches.SnappingEnabled ? "ON" : "OFF");
            tooltipText.AppendLine(Translations.Translate("SNAPPING_TIP"));
            tooltipText.Append(Translations.Translate("KEY_SNAPPING"));
            tooltipText.Append(": ");
            tooltipText.Append(SavedInputKey.ToLocalizedString("KEYNAME", UIThreading.SnappingKey.Encode()));
            _snappingButton.tooltip = tooltipText.ToString();

            tooltipText.Length = 0;
            tooltipText.Append(Translations.Translate("ANARCHY_STATUS"));
            tooltipText.Append(' ');
            tooltipText.AppendLine(TreeManagerPatches.AnarchyEnabled ? "ON" : "OFF");
            tooltipText.AppendLine(Translations.Translate("ANARCHY_TIP"));
            tooltipText.Append(Translations.Translate("KEY_ANARCHY"));
            tooltipText.Append(": ");
            tooltipText.Append(SavedInputKey.ToLocalizedString("KEYNAME", UIThreading.AnarchyKey.Encode()));
            _anarchyButton.tooltip = tooltipText.ToString();

            tooltipText.Length = 0;
            tooltipText.Append(Translations.Translate("FORESTRY_STATUS"));
            tooltipText.Append(' ');
            tooltipText.AppendLine(NaturalResourceManagerPatches.LockForestry ? "ON" : "OFF");
            tooltipText.AppendLine(Translations.Translate("FORESTRY_TIP"));
            tooltipText.Append(Translations.Translate("KEY_FORESTRY"));
            tooltipText.Append(": ");
            tooltipText.Append(SavedInputKey.ToLocalizedString("KEYNAME", UIThreading.ForestryKey.Encode()));
            _lockForestryButton.tooltip = tooltipText.ToString();
        }

        /// <summary>
        /// Adds a multi-state toggle button to the specified UIComponent.
        /// </summary>
        /// <param name="parent">Parent UIComponent.</param>
        /// <param name="name">Button name.</param>
        /// <param name="atlas">Button atlas.</param>
        /// <param name="disabledSprite">Foreground sprite for 'disabled' state..</param>
        /// <param name="enabledSprite">Foreground sprite for 'enabled' state.</param>
        /// <returns>New UIMultiStateButton.</returns>
        private UIMultiStateButton AddToggleButton(UIComponent parent, string name, UITextureAtlas atlas, string disabledSprite, string enabledSprite)
        {
            // Create button.
            UIMultiStateButton newButton = parent.AddUIComponent<UIMultiStateButton>();
            newButton.name = name;
            newButton.atlas = atlas;

            // Get sprite sets.
            UIMultiStateButton.SpriteSetState fgSpriteSetState = newButton.foregroundSprites;
            UIMultiStateButton.SpriteSetState bgSpriteSetState = newButton.backgroundSprites;

            // State 0 background.
            UIMultiStateButton.SpriteSet bgSpriteSetZero = bgSpriteSetState[0];
            if (s_transparentUI)
            {
                bgSpriteSetZero.hovered = "TransparentBaseHovered";
                bgSpriteSetZero.pressed = "TransparentBaseFocused";
            }
            else
            {
                bgSpriteSetZero.normal = "OptionBase";
                bgSpriteSetZero.focused = "OptionBase";
                bgSpriteSetZero.hovered = "OptionBaseHovered";
                bgSpriteSetZero.pressed = "OptionBasePressed";
                bgSpriteSetZero.disabled = "OptionBase";
            }

            // State 0 foreground.
            UIMultiStateButton.SpriteSet fgSpriteSetZero = fgSpriteSetState[0];
            fgSpriteSetZero.normal = disabledSprite;
            fgSpriteSetZero.focused = disabledSprite;
            fgSpriteSetZero.hovered = disabledSprite;
            fgSpriteSetZero.pressed = disabledSprite;
            fgSpriteSetZero.disabled = disabledSprite;

            // Add state 1.
            fgSpriteSetState.AddState();
            bgSpriteSetState.AddState();

            // State 1 background.
            UIMultiStateButton.SpriteSet bgSpriteSetOne = bgSpriteSetState[1];
            if (s_transparentUI)
            {
                bgSpriteSetOne.normal = "TransparentBaseFocused";
                bgSpriteSetOne.focused = "TransparentBaseFocused";
                bgSpriteSetOne.hovered = "TransparentBaseHovered";
            }
            else
            {
                bgSpriteSetOne.normal = "OptionBaseFocused";
                bgSpriteSetOne.focused = "OptionBaseFocused";
                bgSpriteSetOne.hovered = "OptionBaseHovered";
                bgSpriteSetOne.pressed = "OptionBasePressed";
                bgSpriteSetOne.disabled = "OptionBase";
            }

            // State 1 foreground.
            UIMultiStateButton.SpriteSet fgSpriteSetOne = fgSpriteSetState[1];
            fgSpriteSetOne.normal = enabledSprite;
            fgSpriteSetOne.focused = enabledSprite;
            fgSpriteSetOne.hovered = enabledSprite;
            fgSpriteSetOne.pressed = enabledSprite;
            fgSpriteSetOne.disabled = enabledSprite;

            // Set initial state.
            newButton.state = UIMultiStateButton.ButtonState.Normal;
            newButton.activeStateIndex = 0;

            // Size and appearance.
            newButton.autoSize = false;
            newButton.width = ButtonSize;
            newButton.height = ButtonSize;
            newButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            newButton.spritePadding = new RectOffset(0, 0, 0, 0);
            newButton.playAudioEvents = true;

            // Enforce defaults.
            newButton.canFocus = false;
            newButton.enabled = true;
            newButton.isInteractive = true;
            newButton.isVisible = true;

            return newButton;
        }
    }
}
