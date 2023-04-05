// <copyright file="UIThreading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using AlgernonCommons.Keybinding;
    using ICities;
    using TreeControl.Patches;
    using UnityEngine;

    /// <summary>
    /// Threading to capture hotkeys.
    /// </summary>
    public sealed class UIThreading : ThreadingExtensionBase
    {
        // Elevation step - initial (on keydown).
        private const float InitialElevationIncrement = 0.1f;

        // Scaling step - repeating, per second.
        private const float RepeatElevationIncrement = 5.0f;

        // Delay before key repeating activates.
        private static float s_initialRepeatDelay = 0.35f;

        // Hotkey.
        private static Keybinding s_anarchyKey = new Keybinding(KeyCode.A, false, false, true);

        // Function keys.
        private static Keybinding s_elevationUpKey = new Keybinding(KeyCode.PageUp, false, false, false);
        private static Keybinding s_elevationDownKey = new Keybinding(KeyCode.PageDown, false, false, false);

        // Flags.
        private bool _anarchyKeyProcessed = false;
        private bool _elevationUpKeyProcessed = false;
        private bool _elevationDownKeyProcessed = false;

        // Timestamp.
        private float _keyTimer;

        /// <summary>
        /// Gets or sets the tree anarchy hotkey.
        /// </summary>
        internal static Keybinding AnarchyKey { get => s_anarchyKey; set => s_anarchyKey = value; }

        /// <summary>
        /// Gets or sets the prop upscaling key.
        /// </summary>
        internal static Keybinding ElevationUpKey { get => s_elevationUpKey; set => s_elevationUpKey = value; }

        /// <summary>
        /// Gets or sets the prop downscaling key.
        /// </summary>
        internal static Keybinding ElevationDownKey { get => s_elevationDownKey; set => s_elevationDownKey = value; }

        /// <summary>
        /// Gets or sets the prop scaling key delay.
        /// </summary>
        internal static float KeyRepeatDelay { get => s_initialRepeatDelay; set => s_initialRepeatDelay = value; }

        /// <summary>
        /// Look for keypress to activate tool.
        /// </summary>
        /// <param name="realTimeDelta">Real-time delta since last update.</param>
        /// <param name="simulationTimeDelta">Simulation time delta since last update.</param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // Check for anarchy hotkey.
            if (s_anarchyKey.IsPressed())
            {
                // Only process if we're not already doing so.
                if (!_anarchyKeyProcessed)
                {
                    // Set processed flag.
                    _anarchyKeyProcessed = true;

                    // Toggle anarchy.
                    TreeInstancePatches.AnarchyEnabled = !TreeInstancePatches.AnarchyEnabled;
                }
            }
            else
            {
                // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                _anarchyKeyProcessed = false;
            }

            // Check for elevation up hotkey.
            if (s_elevationUpKey.IsPressed())
            {
                // Get time.
                float now = Time.time;

                // Only process if we're not already doing so.
                if (!_elevationUpKeyProcessed)
                {
                    // Set processed flag.
                    _elevationUpKeyProcessed = true;

                    // Increment scaling.
                    TreeToolPatches.ElevationAdjustment += InitialElevationIncrement;

                    // Record keypress time.
                    _keyTimer = now + s_initialRepeatDelay;
                }
                else
                {
                    // Handle key repeat, if appropriate.
                    if (now > _keyTimer)
                    {
                        TreeToolPatches.ElevationAdjustment += RepeatElevationIncrement * Time.deltaTime;
                    }
                }
            }
            else
            {
                // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                _elevationUpKeyProcessed = false;
            }

            // Check for elevation down hotkey.
            if (s_elevationDownKey.IsPressed())
            {
                // Get time.
                float now = Time.time;

                // Only process if we're not already doing so.
                if (!_elevationDownKeyProcessed)
                {
                    // Set processed flag.
                    _elevationDownKeyProcessed = true;

                    // Increment scaling.
                    TreeToolPatches.ElevationAdjustment -= InitialElevationIncrement;

                    // Record keypress time.
                    _keyTimer = now + s_initialRepeatDelay;
                }
                else
                {
                    // Handle key repeat, if appropriate.
                    if (now > _keyTimer)
                    {
                        TreeToolPatches.ElevationAdjustment -= RepeatElevationIncrement * Time.deltaTime;
                    }
                }
            }
            else
            {
                // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                _elevationDownKeyProcessed = false;
            }
        }
    }
}