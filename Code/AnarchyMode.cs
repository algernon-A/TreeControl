// <copyright file="AnarchyMode.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    /// <summary>
    /// Tree anarchy mode.
    /// </summary>
    internal enum AnarchyMode : int
    {
        /// <summary>
        /// No tree anarchy.
        /// </summary>
        None = 0,

        /// <summary>
        /// Anarchy enabled.
        /// </summary>
        Enabled = 1,

        /// <summary>
        /// Force-disable anarchy.
        /// </summary>
        ForceOff = 2,
    }
}
