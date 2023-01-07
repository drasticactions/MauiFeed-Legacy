// <copyright file="HandleUIUpdateEventArgs.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiFeed.Events
{
    public class HandleUIUpdateEventArgs : EventArgs
    {
        public HandleUIUpdateEventArgs(HandleUIUpdate update = HandleUIUpdate.Unknown)
        {
            this.HandleUIUpdate = update;
        }

        public HandleUIUpdate HandleUIUpdate { get; }
    }

    public enum HandleUIUpdate
    {
        Unknown,
    }
}
