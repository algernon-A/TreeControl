// <copyright file="TreeDataSerializer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.ExpandedData
{
    using System.IO;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using ICities;

    /// <summary>
    /// Serialization for expanded tree array.
    /// </summary>
    public class TreeDataSerializer : SerializableDataExtensionBase
    {
        /// <summary>
        /// Legacy 81 tiles data ID.
        /// </summary>
        internal const string DataID = "MoreTrees";

        // Data version.
        private const uint DataVersion = 0;

        /// <summary>
        /// Serializes data to the savegame.
        /// Called by the game on save.
        /// </summary>
        public override void OnSaveData()
        {
            base.OnSaveData();

            // Don't save anything if the vanilla buffer size is used.
            if (Singleton<TreeManager>.instance.m_trees.m_buffer.Length <= TreeManager.MAX_TREE_COUNT)
            {
                Logging.Message("not saving expanded tree data");
                return;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                // Serialise expanded tree array data.
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new TreeDataContainer());

                // Write to savegame.
                serializableDataManager.SaveData(DataID, stream.ToArray());
                Logging.Message("wrote expanded tree data length ", stream.Length);
            }
        }

        /// <summary>
        /// Deserializes data from a savegame (or initialises new data structures when none available).
        /// Called by the game on load (including a new game).
        /// </summary>
        public override void OnLoadData()
        {
            // Deserialization is done at TreeManager.Data.Deserialize (inserted by transpiler).
        }
    }
}