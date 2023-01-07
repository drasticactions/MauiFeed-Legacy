// <copyright file="INotificationService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using MauiFeed.Events;

namespace MauiFeed.Services
{
    public interface INotificationService
    {
        public event EventHandler<HandleUIUpdateEventArgs>? OnHandleUIUpdate;

        public void ReloadUI(HandleUIUpdate type);
    }
}
