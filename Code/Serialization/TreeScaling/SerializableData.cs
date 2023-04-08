// <copyright file="SerializableData.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard), BloodyPenguin (Egor Aralov). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeScaling
{
    using System.IO;
    using System.Linq;
    using ColossalFramework.IO;
    using ICities;

    /// <summary>
    /// Serialization for tree scaling data.
    /// </summary>
    public class SerializableData : SerializableDataExtensionBase
    {
        /// <summary>
        /// Legacy 81 tiles data ID.
        /// </summary>
        internal const string DataID = "TreeScaling";

        // Data version.
        private const int DataVersion = 0;

        /// <summary>
        /// Serializes data to the savegame.
        /// Called by the game on save.
        /// </summary>
        public override void OnSaveData()
        {
            base.OnSaveData();

            using (MemoryStream stream = new MemoryStream())
            {
                // Serialise data.
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DataVersion, new Data());

                // Write to savegame.
                serializableDataManager.SaveData(DataID, stream.ToArray());
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