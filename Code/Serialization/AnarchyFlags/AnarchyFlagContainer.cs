// <copyright file="AnarchyFlagContainer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard), SamSamTS. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.AnarchyFlags
{
    using System;
    using AlgernonCommons;
    using ColossalFramework.IO;
    using static TreeControl.Patches.TreeInstancePatches;

    /// <summary>
    /// Savegame data container for tree anarchy flags.
    /// </summary>
    public sealed class AnarchyFlagContainer : IDataContainer
    {
        private const int DataVersion = 0;

        /// <summary>
        /// Saves tree snapping data (tree heights) to savegame.
        /// </summary>
        /// <param name="serializer">DataSerializer instance.</param>
        public void Serialize(DataSerializer serializer)
        {
            try
            {
                int flagsLength = AnarchyFlags.Length;
                Logging.Message("writing anarchy flags length ", flagsLength);

                // Write data version and array size.
                serializer.WriteInt32(DataVersion);
                serializer.WriteInt32(flagsLength);

                // Write each tree scale entry.
                for (int i = 0; i < flagsLength; ++i)
                {
                    serializer.WriteULong64(AnarchyFlags[i]);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception serializing anarchy flags");
            }
        }

        /// <summary>
        /// Reads anarchy flags from savegame.
        /// </summary>
        /// <param name="serializer">DataSerializer instance.</param>
        public void Deserialize(DataSerializer serializer)
        {
            Logging.KeyMessage("deserializing tree anarchy data");

            try
            {
                // Read data version (currently ignored).
                int version = serializer.ReadInt32();
                if (version > DataVersion)
                {
                    Logging.Error("invalid anarchy flags version ", version, "; aborting read");
                    return;
                }

                // Read array length.
                int dataSize = serializer.ReadInt32();

                // Get tree buffer size.
                int bufferSize = AnarchyFlags.Length;

                // Read each anarchy flag entry.
                for (int i = 0; i < dataSize; ++i)
                {
                    AnarchyFlags[i] = serializer.ReadULong64();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception deserializing anarchy flags");
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