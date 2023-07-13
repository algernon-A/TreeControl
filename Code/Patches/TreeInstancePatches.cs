// <copyright file="TreeInstancePatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace TreeControl.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.Math;
    using HarmonyLib;
    using UnityEngine;
    using TreeInstance = global::TreeInstance;

    /// <summary>
    /// Harmony patches to implement tree anarchy and random tree rotation.
    /// </summary>
    [HarmonyPatch(typeof(TreeInstance))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class TreeInstancePatches
    {
        /// <summary>
        /// Minimum tree sway factor.
        /// </summary>
        internal const float MinSwayFactor = 0f;

        /// <summary>
        /// Maximum tree sway factor.
        /// </summary>
        internal const float MaxSwayFactor = 1f;

        /// <summary>
        /// Default tree scaling factor.
        /// </summary>
        internal const byte DefaultScale = 50;

        /// <summary>
        /// Multiply scaling factor by this to get the rendered scale multiplier.
        /// </summary>
        internal const float ScaleToFloat = 1f / DefaultScale;

        /// <summary>
        /// Multiply rendered scale multiplier by this to get the scaling factor.
        /// </summary>
        internal const float FloatToScale = 1 / ScaleToFloat;

        // Tree anarchy data.
        private static ulong[] s_anarchyFlags;

        // Tree scaling data.
        private static byte[] s_scalingData;

        // Overlap forcing.
        private static OverlapMode s_networkForceMode = OverlapMode.None;
        private static OverlapMode s_buildingForceMode = OverlapMode.None;

        // Update on terrain change.
        private static bool s_terrainReady = false;
        private static bool s_updateOnTerrain = false;
        private static bool s_keepAboveGround = true;

        // Tree swaying.
        private static float s_swayFactor = MinSwayFactor;
        private static float s_distantSwayFactor = MaxSwayFactor;
        private static bool s_disableDistantSway = false;

        /// <summary>
        /// Gets the tree scaling data array.
        /// </summary>
        internal static byte[] ScalingArray => s_scalingData;

        /// <summary>
        /// Gets the tree anarchy data array.
        /// </summary>
        internal static ulong[] AnarchyFlags => s_anarchyFlags;

        /// <summary>
        /// Gets or sets the network 'force on loading' mode.
        /// </summary>
        internal static OverlapMode NetworkOverlap { get => s_networkForceMode; set => s_networkForceMode = value; }

        /// <summary>
        /// Gets or sets the building 'force on loading' mode.
        /// </summary>
        internal static OverlapMode BuildingOverlap { get => s_buildingForceMode; set => s_buildingForceMode = value; }

        /// <summary>
        /// Gets or sets a value indicating whether tree Y-positions should be updated on terrain changes.
        /// </summary>
        internal static bool UpdateOnTerrain { get => s_updateOnTerrain; set => s_updateOnTerrain = value; }

        /// <summary>
        /// Gets or sets a value indicating whether trees should be raised to ground level if the terrain is raised above them.
        /// </summary>
        internal static bool KeepAboveGround { get => s_keepAboveGround; set => s_keepAboveGround = value; }

        /// <summary>
        /// Gets or sets the tree sway factor.
        /// </summary>
        internal static float SwayFactor
        {
            get => s_swayFactor;
            set
            {
                s_swayFactor = Mathf.Clamp(value, MinSwayFactor, MaxSwayFactor);

                // Update distant swaying if not disabled.
                s_distantSwayFactor = s_disableDistantSway ? 0 : s_swayFactor;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether distant tree swaying is disabled (<c>true</c>) or enabled (<c>false</c>).
        /// </summary>
        internal static bool DisableDistantSway
        {
            get => s_disableDistantSway;

            set
            {
                // Don't do anything if no change.
                if (value != s_disableDistantSway)
                {
                    s_disableDistantSway = value;

                    // Update distant sway factor to reflect new value.
                    if (value)
                    {
                        Logging.KeyMessage("disabling distant tree swaying");
                        s_distantSwayFactor = 0f;
                    }
                    else
                    {
                        Logging.KeyMessage("enabling distant tree swaying");
                        s_distantSwayFactor = s_swayFactor;
                    }

                    UpdateRenderGroups();
                }
            }
        }

        /// <summary>
        /// Sets the anarchy flag for the given tree to the given status.
        /// </summary>
        /// <param name="treeID">Tree ID.</param>
        /// <param name="status">Status to set.</param>
        internal static void SetAnarchyFlag(uint treeID, bool status)
        {
            // Get flag and mask (binary packed ulong).
            uint flagsIndex = treeID >> 6;
            ulong newFlags = s_anarchyFlags[flagsIndex];
            ulong treeMask = 1u << (int)(treeID & 0x3F);

            // Update flag block.
            if (status)
            {
                // Set flag.
                newFlags |= treeMask;
            }
            else
            {
                // Clear flag.
                newFlags &= ~treeMask;
            }

            // Update flag.
            s_anarchyFlags[flagsIndex] = newFlags;
        }

        /// <summary>
        /// Gets the anarchy flag for the given tree.
        /// </summary>
        /// <param name="treeID">Tree ID.</param>
        /// <returns>Anarchy flag for the given tree.</returns>
        internal static bool GetAnarchyFlag(uint treeID)
        {
            // Get flag and mask (binary packed ulong).
            uint flagsIndex = treeID >> 6;
            ulong treeMask = 1u << (int)(treeID & 0x3F);
            return (s_anarchyFlags[flagsIndex] & treeMask) != 0;
        }

        /// <summary>
        /// Initializes the scaling and anarchy buffers.
        /// MUST be invoked before referencing either buffer (which includes invokation of TreeInstance.PopulateGroupData on game data deserialization).
        /// </summary>
        /// <param name="size">Buffer size to create.</param>
        internal static void InitializeDataBuffers(int size)
        {
            Logging.Message("creating tree scaling data array of size ", size);
            s_scalingData = new byte[size];

            // Default initial scale is 1.
            for (int i = 0; i < size; ++i)
            {
                s_scalingData[i] = DefaultScale;
            }

            int anarchySize = size >> 6;
            Logging.Message("creating new anarchy flag data array of size ", anarchySize);
            s_anarchyFlags = new ulong[anarchySize];
        }

        /// <summary>
        /// Update all <see cref="RenderGroup"/> for the tree layer.
        /// </summary>
        internal static void UpdateRenderGroups()
        {
            // Local references.
            RenderManager renderManager = Singleton<RenderManager>.instance;
            int treeLayer = Singleton<TreeManager>.instance.m_treeLayer;

            // 45x45 grid.
            for (int x = 0; x < 45; ++x)
            {
                for (int z = 0; z < 45; ++z)
                {
                    renderManager.UpdateGroup(x, z, treeLayer);
                }
            }
        }

        /// <summary>
        /// Performs cleanup and update actions at the end of loading.
        /// </summary>
        internal static void FinishLoading()
        {
            // Via simulation thread.
            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                // Update last three RenderGroup rows (these thend to not be properly refreshed by the game during load if extended tree buffers are in use).
                RenderManager renderManager = Singleton<RenderManager>.instance;
                int treeLayer = Singleton<TreeManager>.instance.m_treeLayer;

                // 45x45 grid.
                for (int x = 0; x < 45; ++x)
                {
                    // Last three rows.
                    for (int z = 0; z < 3; ++z)
                    {
                        // This ensures that RenderGroups are created and initialized for all grid squares.
                        // The game doesn't actually always do this for the tree layer.
                        renderManager.UpdateGroup(x, z, treeLayer);
                    }
                }

                // Set terrain ready flag; loading is complete.
                s_terrainReady = true;
            });
        }

        /// <summary>
        /// Updates all tree states, correcting tree hights and applying overlaps per the 'Hide on load' setting.
        /// Should generally only be called once at loading.
        /// </summary>
        /// <param name="treeManager">TreeManager instance.</param>
        internal static void UpdateTrees(TreeManager treeManager)
        {
            TreeInstance[] trees = treeManager.m_trees.m_buffer;

            // Determine state.
            bool networkHide = s_networkForceMode == OverlapMode.Hide;
            bool buildingHide = s_buildingForceMode == OverlapMode.Hide;
            bool networkUnhide = s_networkForceMode == OverlapMode.Unhide;
            bool buildingUnhide = s_buildingForceMode == OverlapMode.Unhide;
            bool networkDelete = s_networkForceMode == OverlapMode.Delete;
            bool buildingDelete = s_buildingForceMode == OverlapMode.Delete;
            bool hiding = networkHide | buildingHide;
            bool unhiding = networkUnhide | buildingUnhide;
            bool deleting = networkDelete | buildingDelete;

            Logging.Message("updating trees with hiding ", hiding, " unhiding ", unhiding, " deleting ", deleting);

            // Iterate through all trees.
            for (int i = 0; i < trees.Length; ++i)
            {
                // Only do this for created trees with no recorded Y position
                if ((trees[i].m_flags & (ushort)TreeInstance.Flags.Created) == 0)
                {
                    continue;
                }

                // Fix trees with no recorded Y position.
                if (trees[i].m_posY == 0)
                {
                    // Move tree to terrain height.
                    Vector3 position = trees[i].Position;
                    position.y = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
                    trees[i].m_posY = (ushort)Mathf.Clamp(Mathf.RoundToInt(position.y * 64f), 0, 65535);
                }

                // Performing loading force state actions.
                if (unhiding)
                {
                    // Force-unhiding overlapped trees.
                    uint treeID = (uint)i;

                    if (CheckOverlap(treeID, ref trees[i], networkUnhide, buildingUnhide))
                    {
                        SetAnarchyFlag(treeID, true);
                        UpdateTreeVisibility(ref trees[i], true);
                        Logging.Message("unhiding tree ", treeID);
                    }
                }

                if (hiding)
                {
                    // Force-hiding overlapped trees.
                    uint treeID = (uint)i;

                    if (CheckOverlap(treeID, ref trees[i], networkHide, buildingHide))
                    {
                        SetAnarchyFlag(treeID, false);
                        UpdateTreeVisibility(ref trees[i], false);
                        Logging.Message("hiding tree ", treeID);
                    }
                }

                if (deleting)
                {
                    // Force-deleting overlapped trees.
                    uint treeID = (uint)i;

                    if (CheckOverlap(treeID, ref trees[i], networkDelete, buildingDelete))
                    {
                        treeManager.ReleaseTree(treeID);
                        Logging.Message("deleting tree ", treeID);
                    }
                }
            }
        }

        /// <summary>
        /// Checks the given tree for overlap.
        /// Based on game code.
        /// </summary>
        /// <param name="treeID">Tree ID.</param>
        /// <param name="instance">Tree data reference.</param>
        /// <param name="checkNetworks"><c>true</c> to check for network overlap, <c>false</c> to ignore networks.</param>
        /// <param name="checkBuilding"><c>true</c> to check for building overlap, <c>false</c> to ignore building.</param>
        /// <returns><c>true</c> if an overlap is detected, <c>false</c> if no overlap.</returns>
        private static bool CheckOverlap(uint treeID, ref TreeInstance instance, bool checkNetworks, bool checkBuilding)
        {
            // Null check.
            TreeInfo info = instance.Info;
            if (!info)
            {
                return false;
            }

            // Determine collision type.
            ItemClass.CollisionType collisionType = (instance.m_flags & (ushort)TreeInstance.Flags.FixedHeight) != 0 ? ItemClass.CollisionType.Elevated : ItemClass.CollisionType.Terrain;

            // Calculate tree height.
            Randomizer randomizer = new Randomizer(treeID);
            float scale = info.m_minScale + (randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f);
            float height = info.m_generatedInfo.m_size.y * scale;

            // Calculate position.
            Vector3 position = instance.Position;
            float baseY = position.y;
            float maxY = position.y + height;

            // Radius depends on whether this is a single or clustered tree.
            float radius = instance.Single ? 0.3f : 4.5f;

            // Calculate collision quad.
            Quad2 quad = default;
            Vector2 vector = VectorUtils.XZ(position);
            quad.a = vector + new Vector2(0f - radius, 0f - radius);
            quad.b = vector + new Vector2(0f - radius, radius);
            quad.c = vector + new Vector2(radius, radius);
            quad.d = vector + new Vector2(radius, 0f - radius);

            // Check network collision, if appropriate.
            bool colliding = false;
            if (checkNetworks)
            {
                colliding = Singleton<NetManager>.instance.OverlapQuad(quad, baseY, maxY, collisionType, info.m_class.m_layer, 0, 0, 0);
            }

            if (!colliding & checkBuilding)
            {
                colliding = Singleton<BuildingManager>.instance.OverlapQuad(quad, baseY, maxY, collisionType, info.m_class.m_layer, 0, 0, 0);
            }

            return colliding;
        }

        /// <summary>
        /// Updates the given tree based on applied collision status.
        /// </summary>
        /// <param name="instance">Tree date reference.</param>
        /// <param name="isVisible"><c>true</c> to hide tree, <false>to show tree</false>.</param>
        private static void UpdateTreeVisibility(ref TreeInstance instance, bool isVisible)
        {
            if (!isVisible)
            {
                // Tree is being hidden, where it was visible.
                if (instance.GrowState != 0)
                {
                    instance.GrowState = 0;
                    DistrictManager districtManager = Singleton<DistrictManager>.instance;
                    byte park = districtManager.GetPark(instance.Position);
                    --districtManager.m_parks.m_buffer[park].m_treeCount;
                }
            }
            else if (instance.GrowState == 0)
            {
                // Tree is being shown, where it was hiddenFIr.
                instance.GrowState = 1;
                DistrictManager districtManager = Singleton<DistrictManager>.instance;
                byte park2 = districtManager.GetPark(instance.Position);
                ++districtManager.m_parks.m_buffer[park2].m_treeCount;
            }
        }

        /// <summary>
        /// Harmony pre-emptive prefix for TreeInstance.GrowState setter to implement tree anarchy.
        /// </summary>
        /// <param name="__instance">TreeInstance instance.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(TreeInstance.GrowState), MethodType.Setter)]
        [HarmonyPrefix]
        private static bool SetGrowState(ref TreeInstance __instance, int value)
        {
            int thisValue = value;

            // If the tree is being hidden, check tree anarchy status.
            if (value == 0)
            {
                // Unsafe, because we need to reverse-engineer the instance ID from the address offset.
                unsafe
                {
                    uint treeIndex;
                    fixed (void* pointer = &__instance)
                    {
                        // Calculate instance ID from buffer offset.
                        TreeInstance* tree = (TreeInstance*)pointer;
                        fixed (TreeInstance* buffer = Singleton<TreeManager>.instance.m_trees.m_buffer)
                        {
                            treeIndex = (uint)(tree - buffer);
                        }

                        // If anarchy is enabled, override value of 0 and record this tree as having anarchy.
                        if (TreeManagerPatches.AnarchyEnabled)
                        {
                            SetAnarchyFlag(treeIndex, true);
                            thisValue = 1;
                        }
                        else if (GetAnarchyFlag(treeIndex))
                        {
                            // This tree has anarchy - see if anarchy is disabled.
                            if (!TreeManagerPatches.AnarchyEnabled)
                            {
                                // Anarchy is disabled - if building or network tool is currently enabled then we'll remove anarchy for this tree.
                                ToolBase currentTool = Singleton<ToolController>.instance.CurrentTool;
                                if (currentTool is BuildingTool || currentTool is NetTool)
                                {
                                    // Disable anarchy for this tree and keep hidden growstate.
                                    SetAnarchyFlag(treeIndex, false);
                                }
                            }
                            else
                            {
                                // Always override value of 0 (tree hidden) when anarchy is enabled and the tree wasn't already hidden.
                                thisValue = 1;
                            }
                        }
                    }
                }
            }

            __instance.m_flags = (ushort)((int)(__instance.m_flags & 0xFFFFF0FFu) | Mathf.Clamp(thisValue, 0, 15) << 8);

            return false;
        }

        /// <summary>
        /// Harmony transpiler to TreeInstance.AfterTerrainUpdated to implement tree snapping.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TreeInstance.AfterTerrainUpdated))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AfterTerrainUpdatedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo m_posY = AccessTools.Field(typeof(TreeInstance), nameof(TreeInstance.m_posY));

            // Iterate through each instruction.
            foreach (CodeInstruction instruction in instructions)
            {
                // Looking for store to ushort num (local var 1).
                if (instruction.StoresField(m_posY))
                {
                    // Insert call to our custom method.
                    Logging.KeyMessage("found store m_posY");
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, m_posY);
                    yield return new CodeInstruction(OpCodes.Ldarga, 0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TreeInstancePatches), nameof(CalculateElevation)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony postfix to TreeInstance.AfterTerrainUpdated to implement tree terrain height changes for fixed-height trees.
        /// </summary>
        /// <param name="__instance">TreeInstance instance.</param>
        /// <param name="treeID">Tree ID.</param>
        [HarmonyPatch(nameof(TreeInstance.AfterTerrainUpdated))]
        [HarmonyPostfix]
        private static void AfterTerrainUpdatedPostfix(ref TreeInstance __instance, uint treeID)
        {
            // Only concerned with created and fixed-height trees (non-fixed-height trees will have been dealt with by the base method).
            if ((__instance.m_flags & (ushort)(TreeInstance.Flags.Created | TreeInstance.Flags.FixedHeight)) == (ushort)(TreeInstance.Flags.Created | TreeInstance.Flags.FixedHeight))
            {
                // Mimics game code.
                // Calculate terrain elevation.
                Vector3 position = __instance.Position;
                position.y = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
                ushort terrainHeight = (ushort)Mathf.Clamp(Mathf.RoundToInt(position.y * 64f), 0, 65535);

                // If terrain elevation doesn't match tree height, recalculate the tree's position according to current settings.
                if (terrainHeight != __instance.m_posY)
                {
                    __instance.m_posY = CalculateElevation(terrainHeight, __instance.m_posY, ref __instance);
                    Singleton<TreeManager>.instance.UpdateTreeRenderer(treeID, updateGroup: true);
                }
            }
        }

        /// <summary>
        /// Harmony pre-emptive prefix to TreeInstance.CalculateTree to implement tree snapping.
        /// </summary>
        /// <param name="__instance">TreeInstance instance.</param>
        /// <param name="treeID">Tree ID.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(TreeInstance.CalculateTree))]
        [HarmonyPrefix]
        private static bool CalculateTreePrefix(ref TreeInstance __instance, uint treeID)
        {
            // Only do this for created trees with no recorded Y position
            if ((__instance.m_flags & (ushort)TreeInstance.Flags.Created) == 0)
            {
                // Don't execute original method.
                return false;
            }

            // Fix trees with no recorded Y position.
            if (__instance.m_posY == 0)
            {
                // Move tree to terrain height.
                Vector3 position = __instance.Position;
                position.y = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
                __instance.m_posY = (ushort)Mathf.Clamp(Mathf.RoundToInt(position.y * 64f), 0, 65535);
            }

            // Check overlap if anarchy isn't enabled, or game is loading and we've got 'hide on load' selected.
            if (s_terrainReady)
            {
                // Terrain ready - apply normal anarchy.
                if (!GetAnarchyFlag(treeID))
                {
                    CheckOverlap(ref __instance, treeID);
                }
            }

            // Don't execute original method.
            return false;
        }

        /// <summary>
        /// Harmony reverse patch for TreeInstance.CheckOverlap to access original (un-transpiled) game method.
        /// </summary>
        /// <param name="__instance">TreeInstance instance.</param>
        /// <param name="treeID">Tree ID.</param>
        [HarmonyPatch("CheckOverlap")]
        [HarmonyReversePatch(HarmonyReversePatchType.Original)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CheckOverlap(ref TreeInstance __instance, uint treeID)
        {
            Logging.Error("CheckOverlap reverse patch wasn't applied! args", __instance, treeID);
        }

        /// <summary>
        /// Harmony transpiler for TreeInstance.PopulateGroupData (overload 1) to implement tree scaling.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TreeInstance.PopulateGroupData))]
        [HarmonyPatch(
            new Type[]
            {
                typeof(uint),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(Vector3),
                typeof(RenderGroup.MeshData),
                typeof(Vector3),
                typeof(Vector3),
                typeof(float),
                typeof(float),
            },
            new ArgumentType[]
            {
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Ref,
                ArgumentType.Ref,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Ref,
                ArgumentType.Ref,
                ArgumentType.Ref,
                ArgumentType.Ref,
            })]
        [HarmonyTranspiler]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Long Harmony annotation")]
        private static IEnumerable<CodeInstruction> PopulateGroupData1Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Looking for stloc.s 4, which is the scale to be rendered.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder localBuilder && localBuilder.LocalIndex == 4)
                {
                    // Multiply the calculated value by our scaling factor before storing.
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(TreeInstancePatches), nameof(s_scalingData)));
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldelem, typeof(byte));
                    yield return new CodeInstruction(OpCodes.Conv_R4);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, ScaleToFloat);
                    yield return new CodeInstruction(OpCodes.Mul);
                    yield return new CodeInstruction(OpCodes.Mul);
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for TreeInstance.PopulateGroupData (overload 2) to implement tree movement control.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TreeInstance.PopulateGroupData))]
        [HarmonyPatch(
            new Type[]
            {
                typeof(TreeInfo),
                typeof(Vector3),
                typeof(float),
                typeof(float),
                typeof(Vector4),
                typeof(int),
                typeof(int),
                typeof(Vector3),
                typeof(RenderGroup.MeshData),
                typeof(Vector3),
                typeof(Vector3),
                typeof(float),
                typeof(float),
            },
            new ArgumentType[]
            {
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Ref,
                ArgumentType.Ref,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Ref,
                ArgumentType.Ref,
                ArgumentType.Ref,
                ArgumentType.Ref,
            })]
        [HarmonyTranspiler]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Long Harmony annotation")]
        private static IEnumerable<CodeInstruction> PopulateGroupData2Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Windspeed.
            MethodInfo getWindSpeed = AccessTools.Method(typeof(WeatherManager), nameof(WeatherManager.GetWindSpeed), new Type[] { typeof(Vector3) });

            // Looking for call to WeatherManager.GetWindSpeed.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(getWindSpeed))
                {
                    // Looking for call to WeatherManager.GetWindSpeed - append with our sway factor multiplier.
                    Logging.Message("found GetWindSpeed");
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(TreeInstancePatches), nameof(s_distantSwayFactor)));
                    yield return new CodeInstruction(OpCodes.Mul);
                    continue;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for TreeInstance.RenderInstance (overload 1) to implement tree scaling.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TreeInstance.RenderInstance), new Type[] { typeof(RenderManager.CameraInfo), typeof(uint), typeof(int) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RenderInstance1Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Looking for stloc.3, which is the scale to be rendered.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stloc_3)
                {
                    // Multiply the calculated value by our scaling factor before storing.
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(TreeInstancePatches), nameof(s_scalingData)));
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldelem, typeof(byte));
                    yield return new CodeInstruction(OpCodes.Conv_R4);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, ScaleToFloat);
                    yield return new CodeInstruction(OpCodes.Mul);
                    yield return new CodeInstruction(OpCodes.Mul);
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for TreeInstance.RenderInstance (overload 2) to implement random tree rotation and tree movement control.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(TreeInstance.RenderInstance), new Type[] { typeof(RenderManager.CameraInfo), typeof(TreeInfo), typeof(Vector3), typeof(float), typeof(float), typeof(Vector4), typeof(bool) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RenderInstance2Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Quaternion.identity getter.
            MethodInfo qIdentity = AccessTools.PropertyGetter(typeof(Quaternion), nameof(Quaternion.identity));

            // Windspeed.
            MethodInfo getWindSpeed = AccessTools.Method(typeof(WeatherManager), nameof(WeatherManager.GetWindSpeed), new Type[] { typeof(Vector3) });

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(qIdentity))
                {
                    // Looking for new Quaternion.identity getter call - replace with call to our custom method.
                    Logging.Message("found Quaternion.identity");
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TreeInstancePatches), nameof(TreeRotation)));
                    continue;
                }
                else if (instruction.Calls(getWindSpeed))
                {
                    // Looking for call to WeatherManager.GetWindSpeed - append with our sway factor multiplier.
                    Logging.Message("found GetWindSpeed");
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(TreeInstancePatches), nameof(s_swayFactor)));
                    yield return new CodeInstruction(OpCodes.Mul);
                    continue;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Random tree rotation calculator.
        /// </summary>
        /// <param name="location">Tree location.</param>
        /// <returns>Calculated rotation quaternion.</returns>
        private static Quaternion TreeRotation(Vector3 location) => Quaternion.Euler(0, ((location.x * location.x) + (location.z * location.z)) % 359, 0);

        /// <summary>
        /// Calculates a trees's elevation given current settings.
        /// </summary>
        /// <param name="terrainY">Terrain elevation.</param>
        /// <param name="treeY">Tree elevation.</param>
        /// <param name="instance">Tree instance.</param>
        /// <returns>Calculated tree Y coordinate per current settings.</returns>
        private static ushort CalculateElevation(ushort terrainY, ushort treeY, ref TreeInstance instance)
        {
            // If "update on terrain" is selected and we've loaded ad the terrain tool is selected., or if the tree isn't fixed height.
            if ((s_updateOnTerrain & s_terrainReady && Singleton<ToolController>.instance.CurrentTool is TerrainTool) | (instance.m_flags & (ushort)TreeInstance.Flags.FixedHeight) == 0)
            {
                // Default game behaviour - terrain height.
                return terrainY;
            }

            if (s_keepAboveGround)
            {
                // Keeping tree above ground - return higher of the two values.
                return Math.Max(terrainY, treeY);
            }

            // Not updating with terrain changes - keep original tree height.
            return treeY;
        }
    }
}