// <copyright file="TreeDataContainer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.ExpandedData
{
    using System;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using HarmonyLib;
    using TreeControl.Patches;
    using UnityEngine;
    using static TreeManager;
    using PrefabData = PrefabCollection<TreeInfo>.PrefabData;
    using TreeInstance = global::TreeInstance;

    /// <summary>
    /// Savegame data container for expanded tree limit data.
    /// Uses vanilla tree data format.
    /// </summary>
    public sealed class TreeDataContainer : IDataContainer
    {
        // Current data version.
        private const int DataVersion = 0;

        // Prefab index deserialization.
        private static EncodedArray.UShort m_encodedArray;
        private static FastList<PrefabData> m_simulationPrefabs;

        /// <summary>
        /// Gets or sets the array of instances of expanded tree data.
        /// </summary>
        internal static Array32<TreeInstance> ExpandedData { get; set; } = null;

        /// <summary>
        /// Saves expanded tree data to savegame.
        /// Uses vanilla tree format.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Serialize(DataSerializer serializer)
        {
            // Local reference.
            TreeManager treeManager = Singleton<TreeManager>.instance;
            TreeInstance[] buffer = treeManager.m_trees.m_buffer;
            int bufferSize = buffer.Length;

            // Write data version.
            serializer.WriteInt32(DataVersion);

            // Write buffer size.
            serializer.WriteInt32(bufferSize);

            // Tree buffer flags.
            EncodedArray.UShort encodedUShorts = EncodedArray.UShort.BeginWrite(serializer);
            for (int i = MAX_TREE_COUNT; i < bufferSize; ++i)
            {
                encodedUShorts.Write(buffer[i].m_flags);
            }

            encodedUShorts.EndWrite();

            // Tree prefab indexes.
            try
            {
                PrefabCollection<TreeInfo>.BeginSerialize(serializer);
                for (int i = MAX_TREE_COUNT; i < bufferSize; ++i)
                {
                    if (buffer[i].m_flags != 0)
                    {
                        PrefabCollection<TreeInfo>.Serialize(buffer[i].m_infoIndex);
                    }
                }
            }
            finally
            {
                PrefabCollection<TreeInfo>.EndSerialize(serializer);
            }

            // X positions.
            EncodedArray.Short encodedShorts = EncodedArray.Short.BeginWrite(serializer);
            for (int i = MAX_TREE_COUNT; i < bufferSize; ++i)
            {
                if (buffer[i].m_flags != 0)
                {
                    encodedShorts.Write(buffer[i].m_posX);
                }
            }

            encodedShorts.EndWrite();

            // Z positions.
            encodedShorts = EncodedArray.Short.BeginWrite(serializer);
            for (int i = MAX_TREE_COUNT; i < bufferSize; ++i)
            {
                if (buffer[i].m_flags != 0)
                {
                    encodedShorts.Write(buffer[i].m_posZ);
                }
            }

            encodedShorts.EndWrite();

            // Burning trees.
            FastList<BurningTree> burningTrees = treeManager.m_burningTrees;
            uint burningTreesSize = (uint)burningTrees.m_size;
            BurningTree[] burningTreeBuffer = burningTrees.m_buffer;

            serializer.WriteUInt24(burningTreesSize);
            for (int i = 0; i < burningTreesSize; ++i)
            {
                serializer.WriteUInt24(burningTreeBuffer[i].m_treeIndex);
                serializer.WriteUInt8(burningTreeBuffer[i].m_fireIntensity);
                serializer.WriteUInt8(burningTreeBuffer[i].m_fireDamage);
            }
        }

        /// <summary>
        /// Reads expanded tree data from savegame.
        /// Uses vanilla tree format.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Deserialize(DataSerializer serializer)
        {
            // Read data version.
            int dataVersion = serializer.ReadInt32();
            Logging.KeyMessage("found extended data version ", dataVersion);
            if (dataVersion > DataVersion)
            {
                Logging.Error("invalid data version detected; aborting");
                return;
            }

            // Read buffer size.
            int savedBufferSize = serializer.ReadInt32();

            if (savedBufferSize <= MAX_TREE_COUNT)
            {
                Logging.Error("invalid extended tree buffer size detected; aborting");
                return;
            }

            Logging.Message("reading expanded tree data length ", savedBufferSize);

            // Calculate new buffer size; maximum of saved data length and current setting.
            uint newBufferSize = (uint)Math.Max(savedBufferSize, TreeManagerDataPatches.CustomTreeLimit);

            // Create new tree buffer.
            Array32<TreeInstance> newTreeArray = new Array32<TreeInstance>(newBufferSize);
            TreeInstance[] newTreeBuffer = newTreeArray.m_buffer;

            // Initialize Array32 by creating zero (null) item and resetting unused count to zero (unused count will be recalculated after data population).
            newTreeArray.CreateItem(out uint _);
            newTreeArray.ClearUnused();

            // Tree buffer flags.
            EncodedArray.UShort flags = EncodedArray.UShort.BeginRead(serializer);
            for (int i = MAX_TREE_COUNT; i < savedBufferSize; ++i)
            {
                TreeInstance.Flags flag = (TreeInstance.Flags)flags.Read();
                flag &= ~(TreeInstance.Flags.FireDamage | TreeInstance.Flags.Burning);
                newTreeBuffer[i].m_flags = (ushort)flag;
            }

            flags.EndRead();

            // Tree prefab indexes.
            BeginDeserializeInfos(serializer);
            for (int i = dataVersion == 0 ? MAX_TREE_COUNT : MAX_TREE_COUNT; i < savedBufferSize; ++i)
            {
                if (newTreeBuffer[i].m_flags != 0)
                {
                    newTreeBuffer[i].m_infoIndex = DeserializeInfo();
                }
            }

            EndDeserializeInfos(serializer);

            // X positions.
            EncodedArray.Short xPositions = EncodedArray.Short.BeginRead(serializer);
            for (int i = MAX_TREE_COUNT; i < savedBufferSize; ++i)
            {
                if (newTreeBuffer[i].m_flags != 0)
                {
                    newTreeBuffer[i].m_posX = xPositions.Read();
                }
                else
                {
                    newTreeBuffer[i].m_posX = 0;
                }
            }

            xPositions.EndRead();

            // Z positions.
            EncodedArray.Short yPositions = EncodedArray.Short.BeginRead(serializer);
            for (int i = MAX_TREE_COUNT; i < savedBufferSize; ++i)
            {
                if (newTreeBuffer[i].m_flags != 0)
                {
                    newTreeBuffer[i].m_posZ = yPositions.Read();
                }
                else
                {
                    newTreeBuffer[i].m_posZ = 0;
                }
            }

            yPositions.EndRead();

            // Save read data.
            ExpandedData = newTreeArray;

            // Burning trees.
            FastList<BurningTree> burningTrees = Singleton<TreeManager>.instance.m_burningTrees;
            burningTrees.Clear();

            // Read burning tree size.
            int burningTreesSize = (int)serializer.ReadUInt24();

            if (burningTreesSize > 0)
            {
                burningTrees.EnsureCapacity(burningTreesSize);
                BurningTree burningTree = default;

                for (int i = 0; i < burningTreesSize; ++i)
                {
                    burningTree.m_treeIndex = serializer.ReadUInt24();
                    burningTree.m_fireIntensity = (byte)serializer.ReadUInt8();
                    burningTree.m_fireDamage = (byte)serializer.ReadUInt8();

                    // Initialize burning tree, ensuring that the treeIndex is valid.
                    uint treeIndex = burningTree.m_treeIndex;
                    if (treeIndex > 0 && treeIndex < savedBufferSize)
                    {
                        burningTrees.Add(burningTree);
                        newTreeBuffer[treeIndex].m_flags |= 64;
                        if (burningTree.m_fireIntensity != 0)
                        {
                            newTreeBuffer[treeIndex].m_flags |= 128;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Performs any post-deserialization activities.  Nothing to do here (required by IDataContainer).
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
        }

        /// <summary>
        /// Begins deserialization of tree info indexes.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        private void BeginDeserializeInfos(DataSerializer serializer)
        {
            // Set read array.
            m_encodedArray = EncodedArray.UShort.BeginRead(serializer);

            // Get simulationPrefabs fastlist.
            m_simulationPrefabs = AccessTools.Field(typeof(PrefabCollection<TreeInfo>), "m_simulationPrefabs").GetValue(null) as FastList<PrefabCollection<TreeInfo>.PrefabData>;
        }

        /// <summary>
        /// Deserializes a tree info index.
        /// </summary>
        /// <returns>Deserialized prefab index.</returns>
        private ushort DeserializeInfo()
        {
            // Read prefab index.
            uint prefabIndex = m_encodedArray.Read();

            // Check for new index.
            if ((int)prefabIndex >= m_simulationPrefabs.m_size)
            {
                int simPrefabsLength = 0;
                if (m_simulationPrefabs.m_buffer != null)
                {
                    simPrefabsLength = m_simulationPrefabs.m_buffer.Length;
                }

                // Expand simulation prefab fastlist length if required.
                if ((int)prefabIndex >= simPrefabsLength)
                {
                    int capacity = Mathf.Max(Mathf.Max((int)(prefabIndex + 1), 32), simPrefabsLength << 1);
                    m_simulationPrefabs.SetCapacity(capacity);
                }

                m_simulationPrefabs.m_size = (int)(prefabIndex + 1);
            }

            // Update simulation prefab reference count.
            m_simulationPrefabs.m_buffer[prefabIndex].m_refcount++;

            return (ushort)prefabIndex;
        }

        /// <summary>
        /// Ends  deserialization of tree info indexes.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        private void EndDeserializeInfos(DataSerializer serializer)
        {
            // Close off array reading.
            m_encodedArray.EndRead();
            m_encodedArray = null;

            // Read prefab names.
            int numEncodedNames = (int)serializer.ReadUInt16();
            PrefabData item = default;
            for (int i = 0; i < numEncodedNames; ++i)
            {
                // Check for existing info reference.
                if (i < m_simulationPrefabs.m_size)
                {
                    // Existing info reference - populate the name, but only if it hasn't already been populated (don't overwrite).
                    string prefabName = serializer.ReadUniqueString();
                    if (m_simulationPrefabs.m_buffer[i].m_name == null)
                    {
                        m_simulationPrefabs.m_buffer[i].m_name = prefabName;
                    }

                    continue;
                }

                // New reference.
                item.m_name = serializer.ReadUniqueString();
                item.m_refcount = 0;
                item.m_prefab = null;
                item.m_replaced = false;
                m_simulationPrefabs.Add(item);
            }
        }
    }
}
