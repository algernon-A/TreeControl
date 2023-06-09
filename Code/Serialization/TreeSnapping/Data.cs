﻿// <copyright file="Data.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard), BloodyPenguin (Egor Aralov). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeSnapping
{
    using System;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using TreeInstance = global::TreeInstance;

    /// <summary>
    /// Savegame data container for tree height data.
    /// Data format is that from BloodyPenguin's original Tree Snapping mod.
    /// </summary>
    public sealed class Data : IDataContainer
    {
        private static bool s_convertingLegacy = false;

        /// <summary>
        /// Legacy container type converter.
        /// </summary>
        /// <param name="legacyTypeName">Legacy type name (ignored).</param>
        /// <returns>Data type.</returns>
        public static Type LegacyTypeConverter(string legacyTypeName)
        {
            if (!legacyTypeName.StartsWith("TreeSnapping.Data, TreeControl"))
            {
                Logging.Message("converting legacy Tree Snapping data type ", legacyTypeName);
                s_convertingLegacy = true;
            }

            return typeof(Data);
        }

        /// <summary>
        /// Saves tree snapping data (tree heights) to savegame.
        /// </summary>
        /// <param name="serializer">DataSerializer instance.</param>
        public void Serialize(DataSerializer serializer)
        {
            try
            {
                // Local reference.
                TreeInstance[] treeBuffer = Singleton<TreeManager>.instance.m_trees.m_buffer;

                // Tree buffer length.
                int bufferSize = treeBuffer.Length;
                Logging.Message("writing snapping data length ", bufferSize);
                serializer.WriteInt32(bufferSize);

                // Write tree heights.
                EncodedArray.UShort heights = EncodedArray.UShort.BeginWrite(serializer);
                for (int i = 0; i < bufferSize; ++i)
                {
                    heights.Write(treeBuffer[i].m_posY);
                }

                heights.EndWrite();
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception serializing tree snapping data");
            }
        }

        /// <summary>
        /// Reads tree snapping data (tree heights) from savegame.
        /// </summary>
        /// <param name="serializer">DataSerializer instance.</param>
        public void Deserialize(DataSerializer serializer)
        {
            Logging.KeyMessage("deserializing tree snapping data");

            try
            {
                // Local reference.
                TreeManager treeManager = Singleton<TreeManager>.instance;
                TreeInstance[] treeBuffer = treeManager.m_trees.m_buffer;
                int bufferSize = treeBuffer.Length;

                // Read tree height data length.
                int dataSize = serializer.ReadInt32();

                // Read tree heights.
                EncodedArray.UShort heights = EncodedArray.UShort.BeginRead(serializer);
                for (uint i = 0; i < dataSize; ++i)
                {
                    ushort height = heights.Read();

                    // Bounds check for data size.
                    if (i < bufferSize)
                    {
                        // Check for questionable data - ignore 0x0000 and 0xFFFF.
                        if (height != 0 & height != ushort.MaxValue)
                        {
                            treeBuffer[i].m_posY = height;
                            if (s_convertingLegacy)
                            {
                                treeBuffer[i].FixedHeight = true;
                            }
                        }
                        else
                        {
                            // Clear the fixed height flag of any tree without valid snapping data.
                            treeBuffer[i].m_posY = 0;
                            treeBuffer[i].FixedHeight = false;
                        }
                    }
                }

                // Initialize remaining tree buffer with default m_posY, if necessary.
                if (dataSize < bufferSize)
                {
                    for (int i = dataSize; i < bufferSize; ++i)
                    {
                        treeBuffer[i].m_posY = 0;
                    }
                }

                heights.EndRead();
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception deserializing tree snapping data");
            }
        }

        /// <summary>
        /// Performs post-deserialization actions.
        /// </summary>
        /// <param name="serializer">DataSerializer instance.</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
        }
    }
}