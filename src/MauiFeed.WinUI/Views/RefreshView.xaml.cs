// <copyright file="RefreshView.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.DependencyInjection;
using MauiFeed.Models;
using Microsoft.UI.Xaml.Controls;

namespace MauiFeed.WinUI.Views
{
    /// <summary>
    /// Refresh View.
    /// </summary>
    public sealed partial class RefreshView : UserControl, INotifyPropertyChanged
    {
        private bool isRefreshing;
        private Progress<RssCacheFeedUpdate> refreshProgress;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshView"/> class.
        /// </summary>
        public RefreshView()
        {
            this.InitializeComponent();
            this.refreshProgress = Ioc.Default.GetService<Progress<RssCacheFeedUpdate>>()!;
            this.refreshProgress.ProgressChanged += this.RefreshProgressProgressChanged;
            this.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets a value indicating whether the view is refreshing.
        /// </summary>
        public bool IsRefreshing
        {
            get
            {
                return this.isRefreshing;
            }

            set
            {
                if (value != this.isRefreshing)
                {
                    this.Visibility = value ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
                }

                this.SetProperty(ref this.isRefreshing, value);
            }
        }

        private void RefreshProgressProgressChanged(object? sender, RssCacheFeedUpdate e)
        {
            this.IsRefreshing = !e.IsDone;
            this.RefreshLabel.Text = string.Format(Translations.Common.RefreshingLabel, e.FeedsCompleted + 1, e.TotalFeeds);
        }

        /// <summary>
        /// On Property Changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = this.PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

#pragma warning disable SA1600 // Elements should be documented
        private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
#pragma warning restore SA1600 // Elements should be documented
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                return false;
            }

            backingStore = value;
            onChanged?.Invoke();
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
