// <copyright file="LoadingForceMode.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    /// <summary>
    /// Options for forcing tree anarchy on load.
    /// </summary>
    public enum LoadingForceMode : int
    {
        /// <summary>
        /// No forcing (default).
        /// </summary>
        None = 0,

        /// <summary>
        /// Force unhide (apply anarchy to) trees covered by networks and buildings on load.
        /// </summary>
        UnhideAll,

        /// <summary>
        /// Force hide (remove anarchy from) trees covered by networks and buildings on load.
        /// </summary>
        HideAll,
    }
}
