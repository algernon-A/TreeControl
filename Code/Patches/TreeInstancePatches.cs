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
    using HarmonyLib;
    using UnityEngine;
    using TreeInstance = global::TreeInstance;

    /// <summary>
    /// Harmony patches to implement tree anarchy and random tree rotation.
    /// </summary>
    [HarmonyPatch(typeof(TreeInstance))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class TreeInstancePatches
    {
        /// <summary>
        /// Harmony pre-emptive prefix for TreeInstance.GrowState setter to implement tree anarchy.
        /// </summary>
        /// <param name="__instance">TreeInstance instance.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(TreeInstance.GrowState), MethodType.Setter)]
        [HarmonyPrefix]
        public static bool SetGrowState(ref TreeInstance __instance, int value)
        {
            int thisValue = value;

            // Always override value of 0 (tree hidden) when anarchy is enabled.
            if (value == 0 && TreeToolPatches.AnarchyEnabled)
            {
                thisValue = 1;
            }

            __instance.m_flags = (ushort)((int)(__instance.m_flags & 0xFFFFF0FFu) | Mathf.Clamp(thisValue, 0, 15) << 8);

            return false;
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
    }
}