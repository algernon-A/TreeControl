// <copyright file="TCMoveItIntegration.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.MoveItSupport
{
    using System;
    using System.Collections.Generic;
    using MoveItIntegration;
    using TreeControl.Patches;

    /// <summary>
    /// Integration for the Move It mod, to copy scaling settings when trees are copied.
    /// </summary>
    public class TCMoveItIntegration : MoveItIntegrationBase
    {
        /// <summary>
        /// Gets the data identifier.
        /// </summary>
        public override string ID => "TreeControl";

        /// <summary>
        /// Gets the data version.
        /// </summary>
        public override Version DataVersion => new Version(1, 0);

        /// <summary>
        /// Let Move It know we're interested in tree copies.
        /// </summary>
        /// <param name="sourceInstanceID">Source instance ID.</param>
        /// <returns>InstanceType.Tree.</returns>
        public override object Copy(InstanceID sourceInstanceID) => InstanceType.Tree;

        /// <summary>
        /// Called by Move It when an object is pasted.
        /// Used here to copy any tree scaling settings to the copied tree(s).
        /// </summary>
        /// <param name="targetInstanceID">Target instance (unused).</param>
        /// <param name="record">Custom data record (unused).</param>
        /// <param name="sourceMap">Mapping of new tree instances to original tree instances.</param>
        public override void Paste(InstanceID targetInstanceID, object record, Dictionary<InstanceID, InstanceID> sourceMap)
        {
            // Iterate through each mapping entry in the dictionary, copying scaling array values.
            foreach (KeyValuePair<InstanceID, InstanceID> entry in sourceMap)
            {
                TreeInstancePatches.ScalingArray[entry.Value.Tree] = TreeInstancePatches.ScalingArray[entry.Key.Tree];
            }
        }

        /// <summary>
        /// Called by Move It - encodes custom data records.
        /// Not specifically used by Tree Control.
        /// </summary>
        /// <param name="record">Record to encode.</param>
        /// <returns>Encoded data as string.</returns>
        public override string Encode64(object record) => null;

        /// <summary>
        /// Called by Move It - decodes custom data records.
        /// Not specifically used by Tree Control.
        /// </summary>
        /// <param name="record">String to decode.</param>
        /// <param name="dataVersion">Data version.</param>
        /// <returns>Decoded record.</returns>
        public override object Decode64(string record, Version dataVersion) => null;

        /// <summary>
        /// Lets Move It know that we've got some integration we want to do.
        /// </summary>
        public class MoveItIntegrationFactory : IMoveItIntegrationFactory
        {
            /// <summary>
            /// Gets the integration name.
            /// </summary>
            public string Name => throw new NotImplementedException();

            /// <summary>
            /// Gets the integration secription.
            /// </summary>
            public string Description => throw new NotImplementedException();

            /// <summary>
            /// Gets the integration instance.
            /// </summary>
            /// <returns>Integration instance.</returns>
            public MoveItIntegrationBase GetInstance() => new TCMoveItIntegration();
        }
    }
}
