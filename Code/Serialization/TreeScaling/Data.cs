// <copyright file="Data.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard), SamSamTS. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeScaling
{
    using System;
    using AlgernonCommons;
    using ColossalFramework.IO;
    using static TreeControl.Patches.TreeInstancePatches;

    /// <summary>
    /// Savegame data container for tree scaling data.
    /// </summary>
    public sealed class Data : IDataContainer
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
                int scalingLength = ScalingArray.Length;
                Logging.Message("writing scaling data length ", scalingLength);

                // Write data version and array size.
                serializer.WriteInt32(DataVersion);
                serializer.WriteInt32(scalingLength);

                // Write each tree scale entry.
                for (int i = 0; i < scalingLength; ++i)
                {
                    serializer.WriteFloat(ScalingArray[i] * ScaleToFloat);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception serializing tree scaling data");
            }
        }

        /// <summary>
        /// Reads tree scaling data to savegame.
        /// </summary>
        /// <param name="serializer">DataSerializer instance.</param>
        public void Deserialize(DataSerializer serializer)
        {
            Logging.KeyMessage("deserializing tree scaling data");

            try
            {
                // Read data version (currently ignored).
                int version = serializer.ReadInt32();
                if (version > DataVersion)
                {
                    Logging.Error("invalid scaling data version ", version, "; aborting read");
                    return;
                }

                // Read array length.
                int dataSize = serializer.ReadInt32();

                // Get tree buffer size.
                int bufferSize = ScalingArray.Length;

                // Read each tree scale entry.
                for (int i = 0; i < dataSize; ++i)
                {
                    float scale = serializer.ReadFloat();

                    // Bounds check on buffer length.
                    if (i < bufferSize)
                    {
                        // Check for invalid data and set to 1.
                        if (scale == 0f | scale == float.NaN)
                        {
                            scale = 1f;
                        }
                    }

                    ScalingArray[i] = (byte)(scale * FloatToScale);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception deserializing tree scaling data");
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