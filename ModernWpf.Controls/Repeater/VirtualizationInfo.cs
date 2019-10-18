﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    internal enum ElementOwner
    {
        // All elements are originally owned by the view generator.
        ElementFactory,
        // Ownership is transferred to the layout when it calls GetElement.
        Layout,
        // Ownership is transferred to the pinned pool if the element is cleared (outside of
        // a 'remove' collection change of course).
        PinnedPool,
        // Ownership is transfered to the reset pool if the element is cleared by a reset and
        // the data source supports unique ids.
        UniqueIdResetPool,
        // Ownership is transfered to the animator if the element is cleared due to a
        // 'remove'-like collection change.
        Animator
    }

    // Would be nice to have this be part of UIElement similar to how MCBP does it.
    // That would make the lookups much more performant than an attached property.
    internal class VirtualizationInfo
    {
        public ElementOwner Owner { get; private set; } = ElementOwner.ElementFactory;

        public int Index { get; private set; } = -1;

        // Pinned means that the element is protected from getting cleared by layout.
        // A pinned element may still get cleared by a collection change.
        // IsPinned == true doesn't necessarly mean that the element is currently
        // owned by the PinnedPool, only that its ownership may be transferred to the
        // PinnedPool if it gets cleared by layout.
        public bool IsPinned => m_pinCounter > 0u;

        public bool IsHeldByLayout => Owner == ElementOwner.Layout;

        public bool IsRealized => IsHeldByLayout || Owner == ElementOwner.PinnedPool;

        public bool IsInUniqueIdResetPool => Owner == ElementOwner.UniqueIdResetPool;

        public void MoveOwnershipToLayoutFromElementFactory(int index, string uniqueId)
        {
            Owner = ElementOwner.Layout;
            Index = index;
            UniqueId = uniqueId;
        }

        public void MoveOwnershipToLayoutFromUniqueIdResetPool()
        {
            Owner = ElementOwner.Layout;
        }

        public void MoveOwnershipToLayoutFromPinnedPool()
        {
            Owner = ElementOwner.Layout;
        }

        public void MoveOwnershipToElementFactory()
        {
            Owner = ElementOwner.ElementFactory;
            m_pinCounter = 0u;
            Index = -1;
            UniqueId = string.Empty;
            ArrangeBounds = ItemsRepeater.InvalidRect;
        }

        public void MoveOwnershipToUniqueIdResetPoolFromLayout()
        {
            Owner = ElementOwner.UniqueIdResetPool;
            // Keep the pinCounter the same. If the container survives the reset
            // it can go on being pinned as if nothing happened.
        }

        public void MoveOwnershipToAnimator()
        {
            // During a unique id reset, some elements might get removed.
            // Their ownership will go from the UniqueIdResetPool to the Animator.
            // The common path though is for ownership to go from Layout to Animator.
            Owner = ElementOwner.Animator;
            Index = -1;
            m_pinCounter = 0u;
        }

        public void MoveOwnershipToPinnedPool()
        {
            Owner = ElementOwner.PinnedPool;
        }

        public uint AddPin()
        {
            if (!IsRealized)
            {
                throw new Exception("You can't pin an unrealized element.");
            }

            return ++m_pinCounter;
        }

        public uint RemovePin()
        {
            if (!IsRealized)
            {
                throw new Exception("You can't unpin an unrealized element.");
            }

            if (!IsPinned)
            {
                throw new Exception("UnpinElement was called more often than PinElement.");
            }

            return --m_pinCounter;
        }

        public void UpdateIndex(int newIndex)
        {
            Index = newIndex;
        }

        public Rect ArrangeBounds { get; set; }

        public string UniqueId { get; private set; }

        public bool KeepAlive { get; set; }

        public bool AutoRecycleCandidate { get; set; }

        private uint m_pinCounter = 0u;
    }
}