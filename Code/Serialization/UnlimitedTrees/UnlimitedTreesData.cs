// <copyright file="UnlimitedTreesData.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace UnlimitedTrees
{
    using System;
    using AlgernonCommons;
    using static TreeControl.Patches.TreeManagerDataPatches;

    /// <summary>
    /// Deserialization of data from the old Unlimited Trees mod.
    /// </summary>
    public class UnlimitedTreesData
    {
        /// <summary>
        /// Unlimited Tree data key.
        /// </summary>
        public const string DataID = "mabako/unlimiter";

        // Original mod data version 1 hardcoded tree limit.
        private const int Version1TreeLimit = 1048576;

        // Position indicator.
        private int readPosition = 0;

        // Data buffer.
        private ushort[] dataBuffer;

        /// <summary>
        /// Unlimited Trees mod save flags.
        /// </summary>
        private enum SaveFormat : ushort
        {
            None = 0,
            PACKED = 1,
            ENCODED = 2,
        }

        /// <summary>
        /// Legacy container type converter.
        /// </summary>
        /// <param name="legacyTypeName">Legacy type name (ignored).</param>
        /// <returns>UnlimitedTreesData type.</returns>
        public static Type UTLegacyTypeConverter(string legacyTypeName)
        {
            Logging.Message("converting Unlimited Trees data type ", legacyTypeName);

            return typeof(UnlimitedTreesData);
        }

        /// <summary>
        /// Deserializes Unlimited Trees data.
        /// The original mod code is... arcane. Thanks to Quistar (Simon Ueng) for decoding it.
        /// <para>
        /// Uses a unique serial ushort format.
        /// </para>
        /// </summary>
        /// <param name="data">Data to deserialize.</param>
        /// <returns>New <see cref="Array32{T}"/> of <see cref="TreeInstance"/> with deserialized data (<c>null</c> if deserialization was unsuccessful).</returns>
        public Array32<TreeInstance> Deserialize(byte[] data)
        {
            // Copy data to array of ushorts.
            dataBuffer = new ushort[data.Length >> 1];
            Buffer.BlockCopy(data, 0, dataBuffer, 0, data.Length);

            // Data header defaults.
            int savedLimit;
            int savedTreeCount = 0;
            SaveFormat saveFormat = SaveFormat.None;

            // Read data headers, version-dependent.
            ushort dataVersion = ReadUShort();
            switch (dataVersion)
            {
                case 1:
                    // Hardcoded limit for version 1.
                    savedLimit = Version1TreeLimit;
                    break;

                case 2:
                    // Straightforwad int limit for version 2.
                    savedLimit = ReadInt();
                    break;

                case 3:
                    // Int reads for limit and count of saved trees for version 3.
                    savedLimit = ReadInt();
                    savedTreeCount = ReadInt();

                    // Plus two ints that were reserved for future use.
                    ReadInt();
                    ReadInt();

                    // Plus save format.
                    saveFormat = (SaveFormat)ReadUShort();
                    break;

                default:
                    Logging.Error("read invalid Unlimited Trees data version ", dataVersion);
                    return null;
            }

            // Check limit.
            if (savedLimit <= 0 || savedLimit > MaxCustomTreeLimit)
            {
                Logging.Error("read invalid Unlimited Trees data length ", savedLimit);
                return null;
            }

            // Determin saved buffer size.
            int savedBufferSize;
            if ((saveFormat & SaveFormat.PACKED) == SaveFormat.PACKED)
            {
                // Packed version 3 data - limit to maximum busffer size.
                savedBufferSize = Math.Min(savedTreeCount + TreeManager.MAX_TREE_COUNT, MaxCustomTreeLimit);
            }
            else
            {
                savedBufferSize = savedLimit;
            }

            // Create new tree buffer.
            uint newTreeLimit = (uint)Math.Max(savedBufferSize, savedLimit);
            Logging.KeyMessage("reading Unlimited Trees data of size ", savedBufferSize, " into buffer size ", newTreeLimit);
            Array32<TreeInstance> newTreeArray = new Array32<TreeInstance>(newTreeLimit);
            TreeInstance[] newTreeBuffer = newTreeArray.m_buffer;

            // Initialize Array32 by creating zero (null) item and resetting unused count to zero (unused count will be recalculated after data population).
            newTreeArray.CreateItem(out uint _);
            newTreeArray.ClearUnused();

            // Read trees.
            for (int i = TreeManager.MAX_TREE_COUNT; i < savedBufferSize; i++)
            {
                newTreeBuffer[i].m_flags = (ushort)(ReadUShort() & TreeAnarchy.TreeAnarchyData.FireDamageBurningMask);
                if (newTreeBuffer[i].m_flags != 0)
                {
                    newTreeBuffer[i].m_infoIndex = ReadUShort();
                    newTreeBuffer[i].m_posX = (short)ReadUShort();
                    newTreeBuffer[i].m_posZ = (short)ReadUShort();

                    // Set Ypos to 0.
                    newTreeBuffer[i].m_posY = 0;
                }

                // Check for end of data.
                if (readPosition == data.Length)
                {
                    break;
                }
            }

            return newTreeArray;
        }

        /// <summary>
        /// Reads a single <c>ushort</c> from the data buffer.
        /// </summary>
        /// <returns>Read <c>ushort</c>.</returns>
        private ushort ReadUShort() => dataBuffer[readPosition++];

        /// <summary>
        /// Reads a single <c>int32</c> from the data buffer.
        /// </summary>
        /// <returns>Read <c>int32</c>.</returns>
        private int ReadInt() => (dataBuffer[readPosition++] << 16) | (dataBuffer[readPosition++] & 0xffff);
    }
}
