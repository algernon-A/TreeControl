// <copyright file="TreeInstancePatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;
    using TreeInstance = global::TreeInstance;

    /// <summary>
    /// Harmony patches to implement tree anarchy and random tree rotation.
    /// </summary>
    [HarmonyPatch(typeof(TreeInstance))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class TreeInstancePatches
    {
        // Anarchy flags.
        private static bool s_anarchyEnabled = false;
        private static bool s_hideOnLoad = true;

        // Update on terrain change.
        private static bool s_updateOnTerrain = false;
        private static bool s_keepAboveGround = true;

        /// <summary>
        /// Gets or sets a value indicating whether tree anarchy is enabled.
        /// </summary>
        internal static bool AnarchyEnabled { get => s_anarchyEnabled; set => s_anarchyEnabled = value; }

        /// <summary>
        /// Gets or sets a value indicating whether trees under networks or buildings should be hidden on game load.
        /// </summary>
        internal static bool HideOnLoad { get => s_hideOnLoad; set => s_hideOnLoad = value; }

        /// <summary>
        /// Gets or sets a value indicating whether tree Y-positions should be updated on terrain changes.
        /// </summary>
        internal static bool UpdateOnTerrain { get => s_updateOnTerrain; set => s_updateOnTerrain = value; }

        /// <summary>
        /// Gets or sets a value indicating whether trees should be raised to ground level if the terrain is raised above them.
        /// </summary>
        internal static bool KeepAboveGround { get => s_keepAboveGround; set => s_keepAboveGround = value; }

        /// <summary>
        /// Harmony pre-emptive prefix for TreeInstance.GrowState setter to implement tree anarchy.
        /// </summary>
        /// <param name="__instance">TreeInstance instance.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(TreeInstance.GrowState), MethodType.Setter)]
        [HarmonyPrefix]
        private static bool SetGrowState(ref TreeInstance __instance, int value)
        {
            int thisValue = value;

            // Always override value of 0 (tree hidden) when anarchy is enabled.
            if (value == 0 && s_anarchyEnabled)
            {
                thisValue = 1;
            }

            __instance.m_flags = (ushort)((int)(__instance.m_flags & 0xFFFFF0FFu) | Mathf.Clamp(thisValue, 0, 15) << 8);

            return false;
        }

        /// <summary>
        /// Harmony transpiler to TreeInstance.AfterTerrainUpdated to implement tree snapping.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TreeInstance.AfterTerrainUpdated))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AfterTerrainUpdatedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo m_posY = AccessTools.Field(typeof(TreeInstance), nameof(TreeInstance.m_posY));

            // Looking for store to ushort num (local var 1).
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.StoresField(m_posY))
                {
                    // Insert call to our custom method.
                    Logging.KeyMessage("found store m_posY");
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, m_posY);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TreeInstancePatches), nameof(CalculateElevation)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony pre-emptive prefix to TreeInstance.CalculateTree to implement tree snapping.
        /// </summary>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(TreeInstance.CalculateTree))]
        [HarmonyPrefix]
        private static bool CalculateTreerPrefix() => false;

        /// <summary>
        /// Harmony transpiler for TreeInstance.CheckOverlap to implement tree anarchy.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("CheckOverlap")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckOverlapTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo anarchyState = AccessTools.Field(typeof(TreeInstancePatches), nameof(s_hideOnLoad));

            // Looking for new stloc.s 11 (boolean flag used to signify overlaps).
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder localBuilder && localBuilder.LocalIndex == 11)
                {
                    // Found it - append &= s_hideOnLoad to the boolean value to be stored).
                    Logging.Message("found stloc.s 11");
                    yield return new CodeInstruction(OpCodes.Ldsfld, anarchyState);
                    yield return new CodeInstruction(OpCodes.And);
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for TreeInstance.RenderInstance to implement random tree rotation.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TreeInstance.RenderInstance), new Type[] { typeof(RenderManager.CameraInfo), typeof(TreeInfo), typeof(Vector3), typeof(float), typeof(float), typeof(Vector4), typeof(bool) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RenderInstanceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Quaternion.identity getter.
            MethodInfo qIdentity = AccessTools.PropertyGetter(typeof(Quaternion), nameof(Quaternion.identity));

            // Looking for new Quaternion.identity getter call.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(qIdentity))
                {
                    // Found it - drop original call and reokace with our custom method.
                    Logging.Message("found Quaternion.identity");
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TreeInstancePatches), nameof(TreeRotation)));
                    continue;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Random tree rotation calculator.
        /// </summary>
        /// <param name="location">Tree location.</param>
        /// <returns>Calculated rotation quaternion.</returns>
        private static Quaternion TreeRotation(Vector3 location) => Quaternion.Euler(0, ((location.x * location.x) + (location.z * location.z)) % 359, 0);

        /// <summary>
        /// Calculates a trees's elevation given current settings.
        /// </summary>
        /// <param name="terrainY">Terrain elevation.</param>
        /// <param name="treeY">Tree elevation.</param>
        /// <returns>Calculated prop Y coordinate per current settings.</returns>
        private static ushort CalculateElevation(ushort terrainY, ushort treeY)
        {
            if (s_updateOnTerrain)
            {
                // Default game behaviour - terrain height.
                // However, only this if the TerrainTool is active, to avoid surface ruining changes triggering a reset of newly-placed trees.
                return Singleton<ToolController>.instance.CurrentTool is TerrainTool ? terrainY : treeY;
            }

            if (s_keepAboveGround)
            {
                // Keeping tree above ground - return higher of the two values.
                return Math.Max(terrainY, treeY);
            }

            // Not updating with terrain changes - keep original tree height.
            return treeY;
        }
    }
}