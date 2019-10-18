﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Windows;

namespace ModernWpf.Controls
{
    public abstract class VirtualizingLayout : Layout
    {
        protected VirtualizingLayout()
        {
        }

        protected internal virtual void InitializeForContextCore(VirtualizingLayoutContext context)
        {
        }

        protected internal virtual void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
        }

        protected internal abstract Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize);

        protected internal virtual Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            // Do not throw. If the layout decides to arrange its
            // children during measure, then an ArrangeOverride is not required.
            return finalSize;
        }

        protected internal virtual void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
        {
            InvalidateMeasure();
        }
    }
}