// <copyright file="TreeControlStatusPanel.cs" company="algernon (K. Algernon A. Sheppard)">
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
    internal class TreeControlStatusPanel : StandalonePanelBase
    {
        // Layout constants.
        private const float ButtonSize = 36f;
        private const float ButtonSpacing = 2f;

        // Panel settings.
        private static bool s_showButtons = true;
        private static bool s_transparentUI = false;

        // Panel components.
        private UIMultiStateButton _anarchyButton;
        private UIMultiStateButton _snappingButton;
        private UIMultiStateButton _lockForestryButton;

        // Dragging.
        private bool _dragging = false;
        private Vector3 _lastDragPosition;

        // Event handling.
        private bool _ignoreEvents = false;

        /// <summary>
        /// Gets or sets a value indicating whether the status panel should be shown.
        /// </summary>
        public static bool ShowButtons
        {
            get => s_showButtons;

            set
            {
                // Don't do anything if no change.
                if (value != s_showButtons)
                {
                    s_showButtons = value;

                    // Showing - create panel if in-game.
                    if (value)
                    {
                        if (Loading.IsLoaded)
                        {
                            StandalonePanelManager<TreeControlStatusPanel>.Create();
                        }
                    }
                    else
                    {
                        // Hiding - close status panel if open.
                        StandalonePanelManager<TreeControlStatusPanel>.Panel?.Close();
                    }
                }
            }
        }

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
                    if (StandalonePanelManager<TreeControlStatusPanel>.Panel is TreeControlStatusPanel panel)
                    {
                        panel.Close();
                        StandalonePanelManager<TreeControlStatusPanel>.Create();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the panel width.
        /// </summary>
        public override float PanelWidth => (ButtonSize * 3f) + (ButtonSpacing * 2f);

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

            _anarchyButton = AddToggleButton(this, "Tree anarchy status", tcAtlas, "AnarchyOff", "AnarchyOn", "AnarchyRemove");
            _anarchyButton.relativePosition = new Vector2(ButtonSize + ButtonSpacing, 0f);
            _anarchyButton.tooltipBox = UIToolTips.WordWrapToolTip;
            _anarchyButton.eventActiveStateIndexChanged += (c, state) =>
            {
                // Don't do anything if ignoring events.
                if (!_ignoreEvents)
                {
                    // Set anarchy mode to reflect current button state.
                    TreeManagerPatches.CurrentAnarchyMode = (AnarchyMode)state;
                }
            };

            _lockForestryButton = AddToggleButton(this, "Tree snapping status", tcAtlas, "LockForestOff", "LockForestOn");
            _lockForestryButton.tooltipBox = UIToolTips.WordWrapToolTip;
            _lockForestryButton.relativePosition = new Vector2((ButtonSize + ButtonSpacing) * 2f, 0f);
            _lockForestryButton.eventActiveStateIndexChanged += (c, state) =>
            {
                // Don't do anything if ignoring events.
                if (!_ignoreEvents)
                {
                    NaturalResourceManagerPatches.LockForestry = state != 0;
                }
            };

            // Enable right-click dragging.
            _snappingButton.eventMouseMove += Drag;
            _anarchyButton.eventMouseMove += Drag;
            _lockForestryButton.eventMouseMove += Drag;

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
            absolutePosition = optionsBar.absolutePosition - new Vector3(PanelWidth + Margin + 47f, 0f);
        }

        /// <summary>
        /// Refreshes button states.
        /// </summary>
        internal void Refresh()
        {
            // Suppress events while changing state.
            _ignoreEvents = true;
            _snappingButton.activeStateIndex = TreeToolPatches.SnappingEnabled ? 1 : 0;
            _anarchyButton.activeStateIndex = (int)TreeManagerPatches.CurrentAnarchyMode;
            _lockForestryButton.activeStateIndex = NaturalResourceManagerPatches.LockForestry ? 1 : 0;
            _ignoreEvents = false;

            // Set button tooltips.
            UpdateTooltips();
        }

        /// <summary>
        /// Updates button tooltips.
        /// </summary>
        internal void UpdateTooltips()
        {
            // A lot of string manipluations, so use a StringBuilder.
            StringBuilder tooltipText = new StringBuilder();

            // Snapping button tooltip.
            tooltipText.Append(Translations.Translate("SNAPPING_STATUS"));
            tooltipText.Append(' ');
            tooltipText.AppendLine(TreeToolPatches.SnappingEnabled ? "ON" : "OFF");
            tooltipText.AppendLine(Translations.Translate("SNAPPING_TIP"));
            tooltipText.Append(Translations.Translate("KEY_SNAPPING"));
            tooltipText.Append(": ");
            tooltipText.Append(SavedInputKey.ToLocalizedString("KEYNAME", UIThreading.SnappingKey.Encode()));
            _snappingButton.tooltip = tooltipText.ToString();

            // Anarchy button tooltip.
            tooltipText.Length = 0;
            tooltipText.Append(Translations.Translate("ANARCHY_STATUS"));
            tooltipText.Append(' ');
            switch (TreeManagerPatches.CurrentAnarchyMode)
            {
                case AnarchyMode.None:
                    tooltipText.AppendLine(Translations.Translate("OFF"));
                    break;
                case AnarchyMode.Enabled:
                    tooltipText.AppendLine(Translations.Translate("ON"));
                    break;
                case AnarchyMode.ForceOff:
                    tooltipText.AppendLine(Translations.Translate("FORCE_OFF"));
                    break;
            }

            tooltipText.AppendLine(Translations.Translate("ANARCHY_TIP"));
            tooltipText.Append(Translations.Translate("KEY_ANARCHY"));
            tooltipText.Append(": ");
            tooltipText.AppendLine(SavedInputKey.ToLocalizedString("KEYNAME", UIThreading.AnarchyKey.Encode()));
            tooltipText.AppendLine();
            tooltipText.AppendLine(Translations.Translate("ANARCHY_REMOVE_TIP"));
            tooltipText.Append(Translations.Translate("KEY_REMOVE_ANARCHY"));
            tooltipText.Append(": ");
            tooltipText.Append(SavedInputKey.ToLocalizedString("KEYNAME", UIThreading.RemoveAnarchyKey.Encode()));
            _anarchyButton.tooltip = tooltipText.ToString();

            // Lock forestry button tooltip.
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
        /// Drags the panel when the right mouse button is held down.
        /// </summary>
        /// <param name="c">Calling component (ignored).</param>
        /// <param name="p">Mouse event parameter.</param>
        private void Drag(UIComponent c, UIMouseEventParameter p)
        {
            p.Use();

            // Check for right button press.
            if ((p.buttons & UIMouseButton.Right) != 0)
            {
                // Peform dragging actions if already dragging.
                if (_dragging)
                {
                    // Calculate correct position by raycast - this is from game's UIDragHandle.
                    // Raw mouse position doesn't align with the game's UI scaling.
                    Ray ray = p.ray;
                    Vector3 inNormal = GetUIView().uiCamera.transform.TransformDirection(Vector3.back);
                    new Plane(inNormal, _lastDragPosition).Raycast(ray, out float enter);
                    Vector3 currentPosition = (ray.origin + (ray.direction * enter)).Quantize(PixelsToUnits());
                    Vector3 vectorDelta = currentPosition - _lastDragPosition;
                    Vector3[] corners = GetUIView().GetCorners();
                    Vector3 newTransformPosition = (transform.position + vectorDelta).Quantize(PixelsToUnits());

                    // Calculate panel bounds for screen constraint.
                    Vector3 upperLeft = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
                    Vector3 bottomRight = upperLeft + new Vector3(size.x, 0f - size.y);
                    upperLeft *= PixelsToUnits();
                    bottomRight *= PixelsToUnits();

                    // Constrain to screen.
                    if (newTransformPosition.x + upperLeft.x < corners[0].x)
                    {
                        newTransformPosition.x = corners[0].x - upperLeft.x;
                    }

                    if (newTransformPosition.x + bottomRight.x > corners[1].x)
                    {
                        newTransformPosition.x = corners[1].x - bottomRight.x;
                    }

                    if (newTransformPosition.y + upperLeft.y > corners[0].y)
                    {
                        newTransformPosition.y = corners[0].y - upperLeft.y;
                    }

                    if (newTransformPosition.y + bottomRight.y < corners[2].y)
                    {
                        newTransformPosition.y = corners[2].y - bottomRight.y;
                    }

                    // Apply calculated position.
                    transform.position = newTransformPosition;
                    _lastDragPosition = currentPosition;
                }
                else
                {
                    // Not already dragging, but dragging has started - commence.
                    _dragging = true;

                    // Calculate and record initial position.
                    Plane plane = new Plane(transform.TransformDirection(Vector3.back), this.transform.position);
                    Ray ray = p.ray;
                    plane.Raycast(ray, out float enter);
                    _lastDragPosition = ray.origin + (ray.direction * enter);
                }
            }
            else if (_dragging)
            {
                // We were dragging, but the mouse button is no longer held down - stop dragging.
                _dragging = false;

                // Record new position.
                StandalonePanelManager<TreeControlStatusPanel>.LastSavedXPosition = absolutePosition.x;
                StandalonePanelManager<TreeControlStatusPanel>.LastSavedYPosition = absolutePosition.y;
                ModSettings.Save();
            }
        }

        /// <summary>
        /// Adds a multi-state toggle button to the specified UIComponent.
        /// </summary>
        /// <param name="parent">Parent UIComponent.</param>
        /// <param name="name">Button name.</param>
        /// <param name="atlas">Button atlas.</param>
        /// <param name="sprite0">Foreground sprite for state 0 ('disabled').</param>
        /// <param name="sprite1">Foreground sprite for state 1 ('enabled').</param>
        /// <param name="sprite2">Foreground sprite for state 2 (<c>null</c> for no third state).</param>
        /// <returns>New UIMultiStateButton.</returns>
        private UIMultiStateButton AddToggleButton(UIComponent parent, string name, UITextureAtlas atlas, string sprite0, string sprite1, string sprite2 = null)
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
            fgSpriteSetZero.normal = sprite0;
            fgSpriteSetZero.focused = sprite0;
            fgSpriteSetZero.hovered = sprite0;
            fgSpriteSetZero.pressed = sprite0;
            fgSpriteSetZero.disabled = sprite0;

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
            fgSpriteSetOne.normal = sprite1;
            fgSpriteSetOne.focused = sprite1;
            fgSpriteSetOne.hovered = sprite1;
            fgSpriteSetOne.pressed = sprite1;
            fgSpriteSetOne.disabled = sprite1;

            // Add third state if provided.
            if (sprite2 != null)
            {
                fgSpriteSetState.AddState();
                bgSpriteSetState.AddState();
                UIMultiStateButton.SpriteSet bgSpriteSetTwo = bgSpriteSetState[2];

                if (s_transparentUI)
                {
                    bgSpriteSetTwo.normal = "TransparentBaseFocused";
                    bgSpriteSetTwo.focused = "TransparentBaseFocused";
                    bgSpriteSetTwo.hovered = "TransparentBaseHovered";
                }
                else
                {
                    bgSpriteSetTwo.normal = "OptionBaseFocused";
                    bgSpriteSetTwo.focused = "OptionBaseFocused";
                    bgSpriteSetTwo.hovered = "OptionBaseHovered";
                    bgSpriteSetTwo.pressed = "OptionBasePressed";
                    bgSpriteSetTwo.disabled = "OptionBase";
                }

                // State 3 foreground.
                UIMultiStateButton.SpriteSet fgSpriteSetTwo = fgSpriteSetState[2];
                fgSpriteSetTwo.normal = sprite2;
                fgSpriteSetTwo.focused = sprite2;
                fgSpriteSetTwo.hovered = sprite2;
                fgSpriteSetTwo.pressed = sprite2;
                fgSpriteSetTwo.disabled = sprite2;
            }

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
