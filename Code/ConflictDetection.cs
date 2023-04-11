// <copyright file="ConflictDetection.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using ColossalFramework.Plugins;
    using ColossalFramework.UI;

    /// <summary>
    /// Mod conflict detection.
    /// </summary>
    internal sealed class ConflictDetection
    {
        // List of conflicting mods.
        private List<string> _conflictingModNames;

        /// <summary>
        /// Checks for mod conflicts and displays a notification when a conflict is detected.
        /// </summary>
        /// <returns>True if a mod conflict was detected, false otherwise.</returns>
        internal bool CheckModConflicts()
        {
            _conflictingModNames = CheckConflictingMods();

            bool conflictDetected = _conflictingModNames != null && _conflictingModNames.Count > 0;
            if (conflictDetected)
            {
                // First, check to see if UIView is ready.
                if (UIView.GetAView() != null)
                {
                    // It's ready - display the notification now.
                    DisplayNotification();
                }
                else
                {
                    // Otherwise, queue the notification for when the intro's finished loading.
                    LoadingManager.instance.m_introLoaded += DisplayNotification;

                    // Also queue the notification for level loading.
                    LoadingManager.instance.m_levelLoaded += (updateMode) => DisplayNotification();
                }
            }

            return conflictDetected;
        }

        /// <summary>
        /// Displays the mod conflict notification.
        /// </summary>
        private void DisplayNotification()
        {
            // Mod conflict detected - display warning notification.
            ListNotification modConflictNotification = NotificationBase.ShowNotification<ListNotification>();
            if (modConflictNotification != null)
            {
                // Key text items.
                modConflictNotification.AddParas(Translations.Translate("CONFLICT_DETECTED"), Translations.Translate("UNABLE_TO_OPERATE"), Translations.Translate("CONFLICTING_MODS"));

                // Add conflicting mod name(s).
                modConflictNotification.AddList(_conflictingModNames.ToArray());
            }
        }

        /// <summary>
        /// Checks for any known fatal mod conflicts.
        /// </summary>
        /// <returns>A list of conflicting mod names if a mod conflict was detected, false otherwise.</returns>
        private List<string> CheckConflictingMods()
        {
            // Initialise flag and list of conflicting mods.
            bool conflictDetected = false;
            List<string> conflictingModNames = new List<string>();

            // Iterate through the full list of plugins.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    switch (assembly.GetName().Name)
                    {
                        case "TreeAnarchy":
                            // Tree Anarchy. Okay to just be disabled.
                            if (plugin.isEnabled)
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Tree Anarchy");
                            }

                            break;

                        case "TreeUnlimiter":
                            // Unlimited Trees. Okay to just be disabled.
                            if (plugin.isEnabled)
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Unlimited Trees");
                            }

                            break;

                        case "PropAnarchy":
                            // Okay to just be disabled.
                            if (plugin.isEnabled)
                            {
                                conflictDetected = true;

                                // But which one?
                                if (assembly.GetName().Version == new Version("1.0.0.0"))
                                {
                                    // Prop & Tree Anarchy (original BP).
                                    conflictingModNames.Add("Prop & Tree Anarchy");
                                }
                            }

                            break;

                        case "TreeLodFix":
                            // Tree LOD fix. Okay to just be disabled.
                            if (plugin.isEnabled)
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Tree LOD Fix");
                            }

                            break;

                        case "RandomTreeRotation":
                        case "RandomTreeRotation_ND":
                            // Random Tree Rotation. Okay to just be disabled.
                            if (plugin.isEnabled)
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Random Tree Rotation");
                            }

                            break;

                        case "TreeSnapping":
                            // Tree LOD fix. Okay to just be disabled.
                            if (plugin.isEnabled)
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Tree Snapping");
                            }

                            break;

                        case "LockForestry":
                            // Lock Forestry. Okay to just be disabled.
                            if (plugin.isEnabled)
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Lock Forestry");
                            }

                            break;

                        case "TreeMovementControl":
                            // Tree Movement Control. Okay to just be disabled.
                            if (plugin.isEnabled)
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Tree Movement Control");
                            }

                            break;

                        case "VanillaGarbageBinBlocker":
                            // Garbage Bin Controller.
                            conflictDetected = true;
                            conflictingModNames.Add("Garbage Bin Controller");
                            break;

                        case "Painter":
                            // Painter - this one is trickier because both Painter and Repaint use Painter.dll (thanks to CO savegame serialization...).
                            if (plugin.userModInstance.GetType().ToString().Equals("Painter.UserMod"))
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Painter");
                            }

                            break;
                    }
                }
            }

            // Was a conflict detected?
            if (conflictDetected)
            {
                // Yes - log each conflict.
                foreach (string conflictingMod in conflictingModNames)
                {
                    Logging.Error("Conflicting mod found: ", conflictingMod);
                }

                return conflictingModNames;
            }

            // If we got here, no conflict was detected; return null.
            return null;
        }
    }
}
