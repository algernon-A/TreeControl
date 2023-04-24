// <copyright file="TerrainManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.Patches
{
    using System;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony patches to implement tree slope limit adjustment.
    /// </summary>
    //[HarmonyPatch(typeof(TerrainManager))]
    internal static class TerrainManagerPatches
    {
        /// <summary>
        /// Gets or sets a value indicating whether tree anarchy is enabled.
        /// </summary>
        internal static float SlopeMultiplier { get; set; } = 0f;

        /// <summary>
        /// Harmony postfix for TreeManager.UpdateData to update tree and RenderGroup data.
        /// </summary>
        /// <param name="slopeX">X slope.</param>
        /// <param name="slopeZ">Z slope.</param>
        [HarmonyPatch(nameof(TerrainManager.SampleDetailHeight), new Type[] { typeof(Vector3), typeof(float), typeof(float) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
        [HarmonyPostfix]
        private static void SampleDetailHeight(ref float slopeX, ref float slopeZ)
        {
            slopeX *= SlopeMultiplier;
            slopeZ *= SlopeMultiplier;
        }
    }
}