// <copyright file="TreeLODControl.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl
{
    using ColossalFramework;
    using UnityEngine;

    /// <summary>
    /// Tree LOD detail control.
    /// </summary>
    public static class TreeLODControl
    {
        private static Resolution s_currentResolution = Resolution.Low;

        /// <summary>
        /// Tree LOD resolution steps.
        /// </summary>
        public enum Resolution : int
        {
            /// <summary>
            /// Low tree LOD resolution.
            /// </summary>
            Low = 0,

            /// <summary>
            /// Medium tree LOD resolution.
            /// </summary>
            Medium,

            /// <summary>
            /// High tree LOD resolution.
            /// </summary>
            High,

            /// <summary>
            /// Ultra tree LOD resolution.
            /// </summary>
            Ultra,

            /// <summary>
            /// Insane (maximum) tree LOD resolution - only slightly better than Ultra, at the cost of 4x the texture size.
            /// </summary>
            Insane,

            /// <summary>
            /// Number of resolution options.
            /// </summary>
            NumResolutions,
        }

        /// <summary>
        /// Gets or sets the current tree LOD resolution.
        /// </summary>
        internal static Resolution CurrentResolution
        {
            get => s_currentResolution;

            set
            {
                // Don't do anything if no change.
                if (s_currentResolution == value)
                {
                    return;
                }

                s_currentResolution = value;

                // Update TreeManager if it's instantiated.
                if (Singleton<TreeManager>.exists)
                {
                    SetLODResolution();
                }
            }
        }

        /// <summary>
        /// Sets the tree LOD resolution according to current settings.
        /// </summary>
        internal static void SetLODResolution()
        {
            TreeManager treeManager = Singleton<TreeManager>.instance;

            // Set texture resolution.
            int resolution;
            switch (s_currentResolution)
            {
                default:
                case Resolution.Low:
                    resolution = 512;
                    break;

                case Resolution.Medium:
                    resolution = 1024;
                    break;

                case Resolution.High:
                    resolution = 2048;
                    break;

                case Resolution.Ultra:
                    resolution = 4096;
                    break;

                case Resolution.Insane:
                    resolution = 8192;
                    break;
            }

            // Don't change anything if the texture is already this size.
            if (treeManager.m_renderDiffuseTexture.width == resolution && treeManager.m_renderDiffuseTexture.height == resolution)
            {
                return;
            }

            // Texture initialization is per TreeManager.Awake().
            // Set diffuse texture.
            treeManager.m_renderDiffuseTexture = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Trilinear,
                autoGenerateMips = true,
            };

            // Xyca texture.
            treeManager.m_renderXycaTexture = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Trilinear,
                autoGenerateMips = true,
            };

            // Shadow texture.
            treeManager.m_renderShadowTexture = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                autoGenerateMips = false,
            };
        }
    }
}
