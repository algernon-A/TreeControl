// <copyright file="OverlapMode.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    /// <summary>
    /// Options for dealing with overlapped trees.
    /// </summary>
    public enum OverlapMode : int
    {
        /// <summary>
        /// No forcing (default).
        /// </summary>
        None = 0,

        /// <summary>
        /// Force hide (remove anarchy from) overlapped trees.
        /// </summary>
        Hide,

        /// <summary>
        /// Force unhide (apply anarchy to) overlapped trees.
        /// </summary>
        Unhide,

        /// <summary>
        /// Delete (remove) overlapped trees.
        /// </summary>
        Delete,

        /// <summary>
        /// Number of modes.
        /// </summary>
        NumModes,
    }
}
