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
        /// Gets or sets the currently active default custom tree limit.
        /// </summary>
        [XmlElement("TreeLimit")]
        public int TreeLimit { get => TreeManagerDataPatches.CustomTreeLimit; set => TreeManagerDataPatches.CustomTreeLimit = value; }

        /// <summary>
        /// Gets or sets the tree sway factor.
        /// </summary>
        [XmlElement("TreeSwayFactor")]
        public float SwayFactor { get => TreeInstancePatches.SwayFactor; set => TreeInstancePatches.SwayFactor = value; }

        /// <summary>
        /// Gets or sets the tree sway factor.
        /// </summary>
        [XmlElement("LODResolution")]
        public TreeLODControl.Resolution LODResolution { get => TreeLODControl.CurrentResolution; set => TreeLODControl.CurrentResolution = value; }

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
        /// Gets or sets the tree upscaling key.
        /// </summary>
        [XmlElement("ScaleUpKey")]
        public Keybinding ScaleUpKey { get => UIThreading.ScaleUpKey; set => UIThreading.ScaleUpKey = value; }

        /// <summary>
        /// Gets or sets the tree upscaling key.
        /// </summary>
        [XmlElement("ScaleDownKey")]
        public Keybinding ScaleDownKey { get => UIThreading.ScaleDownKey; set => UIThreading.ScaleDownKey = value; }

        /// <summary>
        /// Gets or sets the raise elevation key.
        /// </summary>
        [XmlElement("ElevationUpKey")]
        public Keybinding ElevationUpKey { get => UIThreading.ElevationUpKey; set => UIThreading.ElevationUpKey = value; }

        /// <summary>
        /// Gets or sets the lower elevation key.
        /// </summary>
        [XmlElement("ElevationDownKey")]
        public Keybinding ElevationDownKey { get => UIThreading.ElevationDownKey; set => UIThreading.ElevationDownKey = value; }

        /// <summary>
        /// Gets or sets the key repeat delay.
        /// </summary>
        [XmlElement("KeyRepeatDelay")]
        public float KeyRepeatDelay { get => UIThreading.KeyRepeatDelay; set => UIThreading.KeyRepeatDelay = value; }

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