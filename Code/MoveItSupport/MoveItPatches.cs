// <copyright file="MoveItPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.MoveItSupport
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using ColossalFramework;
    using HarmonyLib;
    using MoveIt;
    using TreeControl.Patches;
    using UnityEngine;

    /// <summary>
    /// Harmony patches to add Move It integration.
    /// </summary>
    internal class MoveItPatches
    {
        // Move It type and field - using reflection and delegates here to avoid a hard dependency with Move It.
        private readonly Type _moveableTreeType;
        private readonly FieldInfo _lastInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveItPatches"/> class.
        /// Attempts to patch Move It for integration.
        /// </summary>
        /// <param name="moveIt">Move It assembly.</param>
        internal MoveItPatches(Assembly moveIt)
        {
            // Reflect Move It tool.
            Type moveItToolType = moveIt.GetType("MoveIt.MoveItTool");
            if (moveItToolType == null)
            {
                Logging.KeyMessage("Move It tool type not found");
                return;
            }

            Logging.KeyMessage("found MoveItTool");

            // Set Move It tree snapping field.
            FieldInfo treeSnapping = AccessTools.Field(moveItToolType, "treeSnapping");
            if (treeSnapping != null)
            {
                treeSnapping.SetValue(null, true);
            }
            else
            {
                Logging.Error("unable to reflect MoveItTool.treeSnapping field");
            }

            // Get Move It MoveableTree type.
            _moveableTreeType = moveIt.GetType("MoveIt.MoveableTree");
            if (_moveableTreeType == null)
            {
                Logging.Error("unable to reflect MoveIt.MoveableTree");
            }

            // Get last instance field.
            _lastInstance = AccessTools.Field(moveItToolType, "m_lastInstance");
            if (_lastInstance == null)
            {
                Logging.Error("unable to reflect MoveItTool.m_lastInstance");
            }

            // Apply tranpiler to MoveIt.RenderCloneGeometry.
            PatcherManager<PatcherBase>.Instance.TranspileMethod(AccessTools.Method(_moveableTreeType, "RenderCloneGeometry"), AccessTools.Method(typeof(MoveItPatches), nameof(RenderCloneGeometryTranspiler)));
        }

        /// <summary>
        /// Applies the given scaling increment to any trees currently selected by Move It.
        /// </summary>
        /// <param name="increment">Scaling increment to apply.</param>
        internal void IncrementTreeSize(float increment)
        {
            // Check for active Move It tool in its default state.
            if (Singleton<ToolController>.instance.CurrentTool is MoveItTool && MoveItTool.ToolState == MoveItTool.ToolStates.Default)
            {
                // See if any active selection.
                if (MoveIt.Action.selection.Count > 0)
                {
                    // Active selection - iterate through each item, checking for trees.
                    TreeManager treeManager = Singleton<TreeManager>.instance;
                    foreach (Instance instance in MoveIt.Action.selection)
                    {
                        uint treeID = instance.id.Tree;
                        if (instance is MoveableTree && treeID > 0)
                        {
                            // Found a tree - apply scaling.
                            int newValue = TreeInstancePatches.ScalingArray[treeID] + Mathf.RoundToInt(increment);
                            TreeInstancePatches.ScalingArray[treeID] = (byte)Mathf.Clamp(newValue, TreeToolPatches.MinScalingFactor, byte.MaxValue);

                            // Update the tree.
                            treeManager.UpdateTree(treeID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the Move It tool is active, and if so, if a tree is selected.
        /// </summary>
        /// <param name="currentTool">Currently selected tool.</param>
        /// <returns><c>true</c> if the Move It tool is active and a tree is selected, <c>false</c> otherwise.</returns>
        internal bool IsMoveItTree(ToolBase currentTool)
        {
            // Check for MoveIt tool.
            if (currentTool is MoveItTool)
            {
                // Get Move It's m_lastInstance and check if it's a MoveIt MoveableTree.
                object lastInstanceObj = _lastInstance.GetValue(currentTool);
                return lastInstanceObj is MoveableTree;
            }

            // Default is no.
            return false;
        }

        /// <summary>
        /// Harmony transpiler for MoveIt.MoveableTree.RenderCloneGeometry to implement tree scaling.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> RenderCloneGeometryTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Looking for stloc.3, which stores the previewed tree's scale.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_3)
                {
                    // Multiply the calculated value by our scaling factor before storing.
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(TreeToolPatches), "s_scaling"));
                    yield return new CodeInstruction(OpCodes.Conv_R4);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, TreeInstancePatches.ScaleToFloat);
                    yield return new CodeInstruction(OpCodes.Mul);
                    yield return new CodeInstruction(OpCodes.Mul);
                }

                yield return instruction;
            }
        }
    }
}