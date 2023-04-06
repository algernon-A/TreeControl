// <copyright file="TreeToolPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.Patches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;
    using static ToolBase;

    /// <summary>
    /// Harmony patches to implement tree anarchy and snapping.
    /// </summary>
    [HarmonyPatch(typeof(TreeTool))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class TreeToolPatches
    {
        /// <summary>
        /// Default elevation adjustment factor.
        /// </summary>
        internal const float DefaultElevationAdjustment = 0f;

        // Tree elevation adjustment.
        private static float s_elevationAdjustment = DefaultElevationAdjustment;

        /// <summary>
        /// Gets or sets the current elevation adjustment.
        /// </summary>
        internal static float ElevationAdjustment
        {
            get => s_elevationAdjustment;

            set
            {
                // Only change value if a tree is selected.
                if (Singleton<ToolController>.instance.CurrentTool is TreeTool treeTool && treeTool.m_prefab is TreeInfo)
                {
                    s_elevationAdjustment = value;
                }
            }
        }

        /// <summary>
        /// Harmony Transpiler for TreeTool.SimulationStep to implement tree snapping.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TreeTool.SimulationStep))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Targeting m_currentEditObject alterations.
            FieldInfo currentEditObject = AccessTools.Field(typeof(RaycastOutput), nameof(RaycastOutput.m_currentEditObject));

            // Targeting allocation of m_mousePosition.
            FieldInfo mousePosition = AccessTools.Field(typeof(TreeTool), "m_mousePosition");

            foreach (CodeInstruction instruction in instructions)
            {
                // Looking for new RaycastInput constructor call.
                if (instruction.operand is ConstructorInfo constructor && constructor.DeclaringType == typeof(RaycastInput))
                {
                    // Change the RaycastInput for tree snapping.
                    Logging.Message("found raycast constructor");
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TreeToolPatches), nameof(TreeSnappingRaycast)));
                    continue;
                }
                else if (instruction.LoadsField(currentEditObject))
                {
                    // Replace calls to output.m_currentEditObject with "true" (to disable terrain height forcing and setting fixed height flag).
                    Logging.Message("found m_currentEditObject");
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    continue;
                }
                else if (instruction.StoresField(mousePosition))
                {
                    // Adjust mouse position by current elevation adjustment.
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TreeToolPatches), nameof(ApplyElevationAdjustment)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony pre-emptive prefix to TreeTool.CheckPlacementErrors to implement tree anarchy.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <returns>False (don't execute original method) if anarchy is enabled, true otherwise.</returns>
        [HarmonyPatch(nameof(TreeTool.CheckPlacementErrors))]
        [HarmonyPrefix]
        private static bool CheckPlacementErrorsPrefix(out ToolErrors __result)
        {
            // Set default original result to no errors.
            __result = ToolErrors.None;

            // If anarchy isn't enabled, go on to execute original method (will override default original result assigned above).
            return !TreeInstancePatches.AnarchyEnabled;
        }

        /// <summary>
        /// Fixes a RaycastInput to implement tree snapping.
        /// </summary>
        /// <param name="raycast">Raycast to fix.</param>
        private static void TreeSnappingRaycast(ref RaycastInput raycast)
        {
            raycast.m_ignoreBuildingFlags = Building.Flags.None;
            raycast.m_ignoreNodeFlags = NetNode.Flags.None;
            raycast.m_ignoreSegmentFlags = NetSegment.Flags.None;
            raycast.m_buildingService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            raycast.m_netService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            raycast.m_netService2 = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
        }

        /// <summary>
        /// Adjusts the provided mouse position by the current elevation adjustment.
        /// </summary>
        /// <param name="mousePosition">Current mouse position.</param>
        /// <returns>Position adjusted for elevation.</returns>
        private static Vector3 ApplyElevationAdjustment(Vector3 mousePosition)
        {
            // Apply elevation adjustment.
            mousePosition.y += s_elevationAdjustment;
            return mousePosition;
        }
    }
}