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
    using HarmonyLib;
    using static ToolBase;

    /// <summary>
    /// Harmony patch to implement tree anarchy.
    /// </summary>
    [HarmonyPatch(typeof(TreeTool))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class TreeToolPatches
    {
        /// <summary>
        /// Harmony pre-emptive prefix to TreeTool.CheckPlacementErrors to implement tree anarchy.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <returns>False (don't execute original method) if anarchy is enabled, true otherwise.</returns>
        [HarmonyPatch(nameof(TreeTool.CheckPlacementErrors))]
        [HarmonyPrefix]
        public static bool CheckPlacementErrorsPrefix(out ToolErrors __result)
        {
            // Set default original result to no errors.
            __result = ToolErrors.None;

            // If anarchy isn't enabled, go on to execute original method (will override default original result assigned above).
            return !TreeInstancePatches.AnarchyEnabled;
        }

        /// <summary>
        /// Harmony Transpiler for TreeTool.SimulationStep to implement tree snapping.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TreeTool.SimulationStep))]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Targeting m_currentEditObject alterations.
            FieldInfo currentEditObject = AccessTools.Field(typeof(RaycastOutput), nameof(RaycastOutput.m_currentEditObject));

            foreach (CodeInstruction instruction in instructions)
            {
                // Looking for new RaycastInput constructor call.
                if (instruction.operand is ConstructorInfo constructor && constructor.DeclaringType == typeof(RaycastInput))
                {
                    // Change the RaycastInput for prop snapping.
                    Logging.Message("found raycast constructor");
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TreeToolPatches), nameof(TreeSnappingRaycast)));
                    continue;
                }
                else if (instruction.LoadsField(currentEditObject))
                {
                    // Replace calls to output.m_currentEditObject with "true" (to disable terrain height forcing and setting fixed height flag).
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    continue;
                }

                yield return instruction;
            }
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
    }
}