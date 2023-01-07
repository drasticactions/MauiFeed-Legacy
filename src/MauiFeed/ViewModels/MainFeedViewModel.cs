// <copyright file="MainFeedViewModel.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Drastic.Tools;
using Drastic.ViewModels;
using MauiFeed.Events;
using MauiFeed.Services;

namespace MauiFeed.ViewModels
{
    public class MainFeedViewModel : BaseViewModel, IDisposable
    {
        private bool disposedValue;

        public MainFeedViewModel(IServiceProvider services)
            : base(services)
        {
            this.Context = services.GetService(typeof(IDatabaseService)) as IDatabaseService ?? throw new NullReferenceException(nameof(IDatabaseService));
            this.Notifications = services.GetService(typeof(INotificationService)) as INotificationService ?? throw new NullReferenceException(nameof(INotificationService));

            this.Notifications.OnHandleUIUpdate += Notifications_OnHandleUIUpdate;
        }

        private void Notifications_OnHandleUIUpdate(object? sender, Events.HandleUIUpdateEventArgs e)
        {
            this.OnUpdateAsync(e.HandleUIUpdate).FireAndForgetSafeAsync(this.ErrorHandler);
        }

        /// <summary>
        /// Gets the database context.
        /// </summary>
        internal IDatabaseService Context { get; }

        /// <summary>
        /// Gets the database context.
        /// </summary>
        internal INotificationService Notifications { get; }

        /// <summary>
        /// Called to reload UI Elements.
        /// </summary>
        /// <returns><see cref="Task"/>.</returns>
        public virtual Task OnUpdateAsync(HandleUIUpdate update)
        {
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Notifications.OnHandleUIUpdate -= Notifications_OnHandleUIUpdate;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
