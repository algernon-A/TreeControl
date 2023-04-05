// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.XML;
    using TreeControl.Patches;

    /// <summary>
    /// Global mod settings.
    /// </summary>
    [XmlRoot("TreeControl")]
    public class ModSettings : SettingsXMLBase
    {
        /// <summary>
        /// Settings file name.
        /// </summary>
        [XmlIgnore]
        private static readonly string SettingsFileName = Path.Combine(ColossalFramework.IO.DataLocation.localApplicationData, "TreeControl.xml");

        /// <summary>
        /// Gets or sets a value indicating whether trees under networks or buildings should be hidden on game load.
        /// </summary>
        public bool HideOnLoad { get => TreeInstancePatches.HideOnLoad; set => TreeInstancePatches.HideOnLoad = value; }

        /// <summary>
        /// Gets or sets a value indicating whether tree Y-positions should be updated on terrain changes.
        /// </summary>
        [XmlElement("UpdateOnTerrain")]
        public bool UpdateOnTerrain { get => TreeInstancePatches.UpdateOnTerrain; set => TreeInstancePatches.UpdateOnTerrain = value; }

        /// <summary>
        /// Gets or sets a value indicating whether trees should be raised to ground level if the terrain is raised above them.
        /// </summary>
        [XmlElement("KeepAboveGround")]
        public bool KeepAboveGround { get => TreeInstancePatches.KeepAboveGround; set => TreeInstancePatches.KeepAboveGround = value; }

        /// <summary>
        /// Gets or sets the tree anarchy hotkey.
        /// </summary>
        [XmlElement("AnarchyKey")]
        public Keybinding AnarchyKey { get => UIThreading.AnarchyKey; set => UIThreading.AnarchyKey = value; }

        /// <summary>
        /// Loads settings from file.
        /// </summary>
        internal static void Load() => XMLFileUtils.Load<ModSettings>(SettingsFileName);

        /// <summary>
        /// Saves settings to file.
        /// </summary>
        internal static void Save() => XMLFileUtils.Save<ModSettings>(SettingsFileName);
    }
}