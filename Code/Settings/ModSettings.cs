// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.UI;
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
        /// Gets or sets a value indicating whether the status panel should be shown.
        /// </summary>
        [XmlElement("ShowButtons")]
        public bool ShowButtons { get => TreeControlStatusPanel.ShowButtons; set => TreeControlStatusPanel.ShowButtons = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the status panel should use transparent buttons.
        /// </summary>
        [XmlElement("TransparentButtons")]
        public bool UseTransparentButtons { get => TreeControlStatusPanel.TransparentUI; set => TreeControlStatusPanel.TransparentUI = value; }

        /// <summary>
        /// Gets or sets the panel's saved X-position.
        /// </summary>
        [XmlElement("StatusPanelX")]
        public float StatusPanelX { get => StandalonePanelManager<TreeControlStatusPanel>.LastSavedXPosition; set => StandalonePanelManager<TreeControlStatusPanel>.LastSavedXPosition = value; }

        /// <summary>
        /// Gets or sets the panel's saved Y-position.
        /// </summary>
        [XmlElement("StatusPanelY")]
        public float StatusPanelY { get => StandalonePanelManager<TreeControlStatusPanel>.LastSavedYPosition; set => StandalonePanelManager<TreeControlStatusPanel>.LastSavedYPosition = value; }

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
        /// Gets or sets a value indicating whether distant tree swaying is disabled (<c>true</c>) or enabled (<c>false</c>).
        /// </summary>
        [XmlElement("DisableDistantSway")]
        public bool DisableDistantSway { get => TreeInstancePatches.DisableDistantSway; set => TreeInstancePatches.DisableDistantSway = value; }

        /// <summary>
        /// Gets or sets the tree sway factor.
        /// </summary>
        [XmlElement("LODResolution")]
        public TreeLODControl.Resolution LODResolution { get => TreeLODControl.CurrentResolution; set => TreeLODControl.CurrentResolution = value; }

        /// <summary>
        /// Gets or sets the network 'force on loading' mode.
        /// </summary>
        [XmlElement("NetworkOverlap")]
        public OverlapMode NetworkOverlap { get => TreeInstancePatches.NetworkOverlap; set => TreeInstancePatches.NetworkOverlap = value; }

        /// <summary>
        /// Gets or sets the building 'force on loading' mode.
        /// </summary>
        [XmlElement("BuildingOverlap")]
        public OverlapMode BuildingOverlap { get => TreeInstancePatches.BuildingOverlap; set => TreeInstancePatches.BuildingOverlap = value; }

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
        /// Gets or sets a value indicating whether lock forestry should automatically be enabled on load.
        /// </summary>
        [XmlElement("LockForestry")]
        public bool LockForestryDefault { get => NaturalResourceManagerPatches.LockForestryDefault; set => NaturalResourceManagerPatches.LockForestryDefault = value; }

        /// <summary>
        /// Gets or sets a value indicating whether any Tree Anarchy mod data should be ignored.
        /// </summary>
        [XmlElement("IgnoreTreeAnarchyData")]
        public bool IgnoreTreeAnarchyData { get => TreeManagerDataPatches.IgnoreTreeAnarchyData; set => TreeManagerDataPatches.IgnoreTreeAnarchyData = value; }

        /// <summary>
        /// Gets or sets the tree anarchy hotkey.
        /// </summary>
        [XmlElement("AnarchyKey")]
        public Keybinding AnarchyKey { get => UIThreading.AnarchyKey; set => UIThreading.AnarchyKey = value; }

        /// <summary>
        /// Gets or sets the tree anarchy hotkey.
        /// </summary>
        [XmlElement("SnappingKey")]
        public Keybinding SnappingKey { get => UIThreading.SnappingKey; set => UIThreading.SnappingKey = value; }

        /// <summary>
        /// Gets or sets the tree anarchy hotkey.
        /// </summary>
        [XmlElement("LockForestryKey")]
        public Keybinding ForestryKey { get => UIThreading.ForestryKey; set => UIThreading.ForestryKey = value; }

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