// <copyright file="TreeManagerDataPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.Patches
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using HarmonyLib;
    using TreeControl.ExpandedData;
    using UnityEngine;
    using static TreeManager;
    using TreeInstance = global::TreeInstance;

    /// <summary>
    /// Harmony patches for the game tree manager's data handling to implement expanded tree limits.
    /// </summary>
    [HarmonyPatch(typeof(Data))]
    internal static class TreeManagerDataPatches
    {
        /// <summary>
        /// Minimum supported custom tree limit.
        /// </summary>
        internal const int MinCustomTreeLimit = MAX_TREE_COUNT;

        /// <summary>
        /// Maximum supported custom tree limit.
        /// </summary>
        internal const int MaxCustomTreeLimit = MAX_TREE_COUNT * 8;

        private static int s_customTreeLimit = MAX_TREE_COUNT;

        /// <summary>
        /// Gets or sets the currently active default custom tree limit.
        /// </summary>
        internal static int CustomTreeLimit
        {
            get => s_customTreeLimit;

            set
            {
                s_customTreeLimit = Mathf.Clamp(value, MinCustomTreeLimit, MaxCustomTreeLimit);
            }
        }

        /// <summary>
        /// Harmony transpiler for TreeMananger.Data.Deserialize to insert call to custom deserialize method at the correct spot.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.Deserialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool inserted = false;

            // Insert call to our custom method immediately before the first stloc.s 19 (start of loop to initialize instances and assign unused).
            foreach (CodeInstruction instruction in instructions)
            {
                if (!inserted && instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder localBuilder && localBuilder.LocalIndex == 19)
                {
                    Logging.Message("found stloc.s 19");

                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TreeManagerDataPatches), nameof(CustomDeserialize)));

                    // Update local variable for buffer length with the length of the new buffer.
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Ldlen);
                    yield return new CodeInstruction(OpCodes.Conv_I4);
                    yield return new CodeInstruction(OpCodes.Stloc_3);

                    // Update local variable for buffer  with the new buffer.
                    yield return new CodeInstruction(OpCodes.Stloc_1);

                    // We only do this for the first instance.
                    inserted = true;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for TreeManager.Data.Serialize to insert call to custom serialize method at the correct spots (serialization of tree array).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.Serialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SerializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                // Intercept call to stloc.2, storing the tree array size for serialization.
                if (instruction.opcode == OpCodes.Stloc_2)
                {
                    Logging.Message("found stlotc.2 in Data.Serialize");

                    // Replace it with the vanilla buffer limit.
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, MAX_TREE_COUNT);
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Performs deserialization activites when loading game data.
        /// Creates the expanded tree array.
        /// </summary>
        /// <param name="instance">TreeManager instance.</param>
        private static TreeInstance[] CustomDeserialize(TreeManager instance)
        {
            Logging.KeyMessage("expanded data CustomDeserialize");

            // Replacement tree array.
            Array32<TreeInstance> newTreeArray = null;

            // Local reference.
            TreeInstance[] oldBuffer = instance.m_trees.m_buffer;

            // See if this save contains any extended treedata data.
            if (Singleton<SimulationManager>.instance.m_serializableDataStorage.TryGetValue(TreeDataSerializer.DataID, out byte[] data))
            {
                // Yes - load it.
                using (MemoryStream stream = new MemoryStream(data))
                {
                    Logging.Message("found expanded tree data");
                    DataSerializer.Deserialize<TreeDataContainer>(stream, DataSerializer.Mode.Memory);
                }

                // Check results.
                int dataReadSize = TreeDataContainer.ExpandedData?.m_buffer.Length ?? 0;
                if (dataReadSize > MAX_TREE_COUNT)
                {
                    newTreeArray = TreeDataContainer.ExpandedData;
                }
            }

            if (newTreeArray == null)
            {
                // No expanded tree data was found.
                Logging.KeyMessage("no expanded tree data found");

                // Check to see if an expanded limit was set.
                if (CustomTreeLimit <= MAX_TREE_COUNT)
                {
                    Logging.KeyMessage("not using expanded tree array");

                    // Ensure tree scaling array is initialized.
                    TreeInstancePatches.InitializeScalingBuffer(MAX_TREE_COUNT);

                    return oldBuffer;
                }

                Logging.KeyMessage("converting vanilla data to expanded tree limit of ", CustomTreeLimit);

                // Create new tree buffer.
                newTreeArray = new Array32<TreeInstance>((uint)CustomTreeLimit);

                // Initialize Array32 by creating zero (null) item and resetting unused count to zero (unused count will be recalculated after data population).
                newTreeArray.CreateItem(out uint _);
                newTreeArray.ClearUnused();
            }

            // If we got here, we're using an expanded buffer; copy vanilla tree buffer into expanded buffer.
            TreeInstance[] newBuffer = newTreeArray.m_buffer;
            for (int i = 0; i < MAX_TREE_COUNT; i++)
            {
                ushort flags = oldBuffer[i].m_flags;
                if (flags != 0)
                {
                    newBuffer[i].m_flags = flags;
                    newBuffer[i].m_infoIndex = oldBuffer[i].m_infoIndex;
                    newBuffer[i].m_posX = oldBuffer[i].m_posX;
                    newBuffer[i].m_posZ = oldBuffer[i].m_posZ;
                }
            }

            // Assign new array and create new updated tree array.
            instance.m_trees = newTreeArray;
            int newBufferSize = newTreeArray.m_buffer.Length;
            instance.m_updatedTrees = new ulong[newBufferSize >> 6];

            // Ensure tree scaling array is initialized.
            TreeInstancePatches.InitializeScalingBuffer(newBufferSize);

            return newTreeArray.m_buffer;
        }
    }
}