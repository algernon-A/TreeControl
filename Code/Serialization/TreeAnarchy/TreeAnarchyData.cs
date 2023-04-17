// <copyright file="TreeAnarchyData.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeAnarchy
{
    using System;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using TreeControl.Patches;
    using static TreeManager;

    /// <summary>
    /// Tree Anarchy data serializer.
    /// </summary>
    public class TreeAnarchyData : IDataContainer
    {
        private const ushort FireDamageBurningMask = unchecked((ushort)~(TreeInstance.Flags.Burning | TreeInstance.Flags.FireDamage));

        /// <summary>
        /// Tree anarchy data versions.
        /// </summary>
        private enum Format : uint
        {
            Version4 = 4,
            Version5,
            Version6,
            Version7,
        }

        /// <summary>
        /// Gets or sets the array of instances of expanded tree data.
        /// </summary>
        public static Array32<TreeInstance> ExpandedData { get; set; } = null;

        /// <summary>
        /// Legacy container type converter.
        /// </summary>
        /// <param name="legacyTypeName">Legacy type name (ignored).</param>
        /// <returns>TreeAnarchyData type.</returns>
        public static Type TALegacyTypeConverter(string legacyTypeName)
        {
            Logging.Message("converting Tree Anarchy data type ", legacyTypeName);

            return typeof(TreeAnarchyData);
        }

        /// <summary>
        /// Reads expanded Tree Anarchy data from savegame.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Deserialize(DataSerializer serializer)
        {
            // Local references.
            TreeManager treeManager = Singleton<TreeManager>.instance;
            TreeInstance[] oldBuffer = treeManager.m_trees.m_buffer;

            // Read buffer size.
            int savedBufferSize = serializer.ReadInt32(); // Read in Max limit
            int treeCount = 0;

            // Calculate new buffer size; maximum of saved data length and current setting.
            uint newBufferSize = (uint)Math.Max(savedBufferSize, TreeManagerDataPatches.CustomTreeLimit);

            // Create new tree buffer.
            Array32<TreeInstance> newTreeArray = new Array32<TreeInstance>(newBufferSize);
            TreeInstance[] newTreeBuffer = newTreeArray.m_buffer;

            // Initialize Array32 by creating zero (null) item and resetting unused count to zero (unused count will be recalculated after data population).
            newTreeArray.CreateItem(out uint _);
            newTreeArray.ClearUnused();

            // Copy vanilla tree buffer into expanded buffer.
            // This needs to be done before further deserialization as the flags here affect the data read counts.
            for (int i = 1; i < MAX_TREE_COUNT; ++i)
            {
                if (oldBuffer[i].m_flags != 0)
                {
                    newTreeBuffer[i].m_flags = oldBuffer[i].m_flags;
                    newTreeBuffer[i].m_infoIndex = oldBuffer[i].m_infoIndex;
                    newTreeBuffer[i].m_posX = oldBuffer[i].m_posX;
                    newTreeBuffer[i].m_posZ = oldBuffer[i].m_posZ;
                }
            }

            // Tree buffer flags.
            Logging.Message("reading Tree Anarchy flags");
            EncodedArray.UShort flags = EncodedArray.UShort.BeginRead(serializer);
            for (int i = MAX_TREE_COUNT; i < savedBufferSize; ++i)
            {
                newTreeBuffer[i].m_flags = (ushort)(flags.Read() & FireDamageBurningMask);
            }

            flags.EndRead();

            // Tree prefab indexes.
            Logging.Message("reading Tree Anarchy prefab indices");
            PrefabCollection<TreeInfo>.BeginDeserialize(serializer);

            // Yes, this starts at 1 for some reason, instead of MAX_TREE_COUNT.
            for (int i = 1; i < savedBufferSize; ++i)
            {
                if (newTreeBuffer[i].m_flags != 0)
                {
                    newTreeBuffer[i].m_infoIndex = (ushort)PrefabCollection<TreeInfo>.Deserialize(true);
                    treeCount++;
                }
            }

            Logging.Message(treeCount, " Tree Anarchy trees read");
            PrefabCollection<TreeInfo>.EndDeserialize(serializer);

            // X positions.
            Logging.Message("reading Tree Anarchy X positions");
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
            Logging.Message("reading Tree Anarchy Z positions");
            EncodedArray.Short zPositions = EncodedArray.Short.BeginRead(serializer);
            for (int i = MAX_TREE_COUNT; i < savedBufferSize; ++i)
            {
                if (newTreeBuffer[i].m_flags != 0)
                {
                    newTreeBuffer[i].m_posZ = zPositions.Read();
                }
                else
                {
                    newTreeBuffer[i].m_posZ = 0;
                }
            }

            zPositions.EndRead();

            // Fixed heights.
            Logging.Message("reading Tree Anarchy Y positions");
            EncodedArray.UShort fixedHeights = EncodedArray.UShort.BeginRead(serializer);
            for (int i = 1; i < savedBufferSize; ++i)
            {
                if ((newTreeBuffer[i].m_flags & (ushort)TreeInstance.Flags.FixedHeight) != 0)
                {
                    newTreeBuffer[i].m_posY = fixedHeights.Read();
                }
            }

            fixedHeights.EndRead();

            // Assign new tree buffer.
            treeManager.m_trees = newTreeArray;
            treeManager.m_updatedTrees = new ulong[newTreeBuffer.Length >> 6];
            TreeInstancePatches.InitializeScalingBuffer((int)newBufferSize);

            // Tree scaling.
            if ((Format)serializer.version >= Format.Version6)
            {
                EncodedArray.Float treeScales = EncodedArray.Float.BeginRead(serializer);
                for (int i = 1; i < savedBufferSize; i++)
                {
                    if (newTreeBuffer[i].m_flags != 0)
                    {
                        // Tree Anarchy uses "extra scale" added on top of existing scale.
                        // Need to convert this to multiplicative scale.
                        float extraScale = treeScales.Read();

                        // TreeInfos aren't available at this stage of loading, so we can't determine the exact scale midpoint.
                        // Instead, we'll go out on a limb (boom, tish....) and assume that scales are based around 1.0.
                        TreeInstancePatches.ScalingArray[i] = (byte)((extraScale + 1f) * TreeInstancePatches.FloatToScale);
                    }
                }

                treeScales.EndRead();
            }

            // Burning trees.
            if ((Format)serializer.version >= Format.Version7)
            {
                FastList<BurningTree> burningTrees = treeManager.m_burningTrees;
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

            // Save read data.
            ExpandedData = newTreeArray;
        }

        /// <summary>
        /// Performs any post-deserialization activities.  Nothing to do here (required by IDataContainer).
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
        }

        /// <summary>
        /// Performs serialization.  Nothing to do here (required by IDataContainer), as we're not writing Tree Anarchy data.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Serialize(DataSerializer serializer)
        {
        }
    }
}
