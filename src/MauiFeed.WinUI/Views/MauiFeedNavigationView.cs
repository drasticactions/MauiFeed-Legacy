// <copyright file="MauiFeedNavigationView.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using Drastic.Tools;
using MauiFeed.WinUI.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinUICommunity.Common.Extensions;

namespace MauiFeed.WinUI.Views
{
    /// <summary>
    /// Maui Feed Navigation View.
    /// </summary>
    public class MauiFeedNavigationView : NavigationView
    {
        private Window? window;

        private Frame navigationFrame;
        private SettingsPage settingsPage;

        private NavigationViewItemSeparator folderSeparator;
        private NavigationViewItemSeparator filterSeparator;

        private NavigationViewItem? addFolderButton;
        private IErrorHandlerService errorHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="MauiFeedNavigationView"/> class.
        /// </summary>
        public MauiFeedNavigationView()
        {
            this.MenuItemsSource = this.Items;

            this.navigationFrame = new Frame();
            this.settingsPage = new SettingsPage();

            this.Content = this.navigationFrame;

            this.folderSeparator = new NavigationViewItemSeparator();
            this.filterSeparator = new NavigationViewItemSeparator();

            this.navigationFrame.Loaded += this.NavigationFrameLoaded;
            this.SelectionChanged += this.MauiFeedNavigationViewSelectionChanged;

            this.errorHandler = Ioc.Default.GetService<IErrorHandlerService>()!;

            this.GenerateMenuButtons();
        }

        /// <summary>
        /// Gets the list of navigation items.
        /// </summary>
        public ObservableCollection<NavigationViewItemBase> Items { get; } = new ObservableCollection<NavigationViewItemBase>();

        private void GenerateMenuButtons()
        {
            var refreshButton = new NavigationViewItem() { Content = Translations.Common.RefreshButton, Icon = new SymbolIcon(Symbol.Refresh) };
            refreshButton.SelectsOnInvoked = false;
            refreshButton.Tapped += (sender, args) =>
            {
                // this.RefreshAllFeedsAsync().FireAndForgetSafeAsync(this);
            };

            this.Items.Add(refreshButton);

            var addButton = new NavigationViewItem() { Content = Translations.Common.AddLabel, Icon = new SymbolIcon(Symbol.Add) };
            addButton.SelectsOnInvoked = false;

            var addFeedButton = new NavigationViewItem() { Content = Translations.Common.FeedLabel, Icon = new SymbolIcon(Symbol.Library) };
            addFeedButton.SelectsOnInvoked = false;
            addFeedButton.Tapped += (sender, args) =>
            {
            };
            addButton.MenuItems.Add(addFeedButton);

            var addOpmlButton = new NavigationViewItem() { Content = Translations.Common.OPMLFeedLabel, Icon = new SymbolIcon(Symbol.Globe) };
            addOpmlButton.SelectsOnInvoked = false;
            addOpmlButton.Tapped += (sender, args) => this.OpenImportOpmlFeedPickerAsync().FireAndForgetSafeAsync(this.errorHandler);
            addButton.MenuItems.Add(addOpmlButton);

            this.addFolderButton = new NavigationViewItem() { Content = Translations.Common.FolderLabel, Icon = new SymbolIcon(Symbol.Folder) };
            this.addFolderButton.SelectsOnInvoked = false;
            this.addFolderButton.Tapped += (sender, args) =>
            {
            };
            addButton.MenuItems.Add(this.addFolderButton);

            this.Items.Add(addButton);
            this.Items.Add(new NavigationViewItemSeparator());
        }

        private async Task OpenImportOpmlFeedPickerAsync()
        {
            System.Diagnostics.Debug.Assert(this.window is not null, "Window is null when it should not be.");
            var filePicker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this.window!);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            filePicker.FileTypeFilter.Add(".opml");
            var file = await filePicker.PickSingleFileAsync();
            if (file is null)
            {
                return;
            }
        }

        private void MauiFeedNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem nav)
            {
                var value = nav.Tag?.ToString() ?? string.Empty;
                switch (value)
                {
                    case "Settings":
                        this.navigationFrame.Content = this.settingsPage;
                        break;
                    default:
                        break;
                }

                return;
            }
        }

        private void NavigationFrameLoaded(object sender, RoutedEventArgs e)
        {
            this.navigationFrame.Loaded -= this.NavigationFrameLoaded;
            var settings = (NavigationViewItem)this.SettingsItem;
            settings.Content = Translations.Common.SettingsLabel;

            // This is stupid and I hate it.
            // AFAIK There is no other way to get the root window of a given control.
            // This only works because I'm setting the data context on the XAML Root to the Window, which is NOT the default.
            this.window = ((FrameworkElement)this.XamlRoot.Content).DataContext as Window;
        }
    }
}
