// <copyright file="Loading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using System.Collections.Generic;
    using AlgernonCommons.Patching;
    using AlgernonCommons.UI;
    using ICities;
    using TreeControl.Patches;

    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public sealed class Loading : PatcherLoadingBase<OptionsPanel, PatcherBase>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the UI anarchy toggle should be enabled (<c>true</c>) or disabled (<c>false</c>) after loading.
        /// </summary>
        internal static bool InitialAnarchyState { get; set; } = false;

        /// <summary>
        /// Gets a list of permitted loading modes.
        /// </summary>
        protected override List<AppMode> PermittedModes => new List<AppMode> { AppMode.Game, AppMode.MapEditor };

        /// <summary>
        /// Performs any actions upon successful creation of the mod.
        /// E.g. Can be used to patch any other mods.
        /// </summary>
        /// <param name="loading">Loading mode (e.g. game or editor).</param>
        protected override void CreatedActions(ILoading loading)
        {
            base.CreatedActions(loading);

            // Set intial status.
            NaturalResourceManagerPatches.LockForestry = NaturalResourceManagerPatches.LockForestryDefault;
        }

        /// <summary>
        /// Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
            base.LoadedActions(mode);

            // Assign tree LOD resolution.
            TreeLODControl.SetLODResolution();

            // Peform end-of-load actions.
            TreeInstancePatches.FinishLoading();

            // Add status panel.
            if (TreeControlStatusPanel.ShowButtons)
            {
                StandalonePanelManager<TreeControlStatusPanel>.Create();
            }

            // Patch Move It.
            TreeToolPatches.CheckMoveIt();
        }
    }
}