// <copyright file="SettingsPage.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Drastic.Services;
using CommunityToolkit.Mvvm.DependencyInjection;
using MauiFeed.WinUI.Services;
using Drastic.Tools;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MauiFeed.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        private ElementTheme _elementTheme;
        private IAppDispatcher dispatcher;
        private IErrorHandlerService errorHandler;
        private ThemeSelectorService themeSelectorService;

        public SettingsPage(ThemeSelectorService service)
        {
            this.InitializeComponent();
            this.dispatcher = Ioc.Default.GetService<IAppDispatcher>()!;
            this.errorHandler = Ioc.Default.GetService<IErrorHandlerService>()!;
            this.DataContext = this;
            this.SwitchThemeCommand = new AsyncCommand<ElementTheme>(this.SetThemeAsync, (n) => true, this.errorHandler);
            this.themeSelectorService = service;
            this.ElementTheme = this.themeSelectorService.Theme;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        public AsyncCommand<ElementTheme> SwitchThemeCommand { get; private set; }

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }
            set { this.SetProperty(ref this._elementTheme, value); }
        }

        /// <summary>
        /// On Property Changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.dispatcher.Dispatch(() =>
            {
                var changed = this.PropertyChanged;
                if (changed == null)
                {
                    return;
                }

                changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

#pragma warning disable SA1600 // Elements should be documented
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
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

        private async Task SetThemeAsync(ElementTheme theme)
        {
            this.ElementTheme = theme;
            this.themeSelectorService.SetTheme(theme);
        }
    }
}
