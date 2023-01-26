// <copyright file="SettingsPage.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using Drastic.Tools;
using MauiFeed.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MauiFeed.WinUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [CommunityToolkit.Mvvm.ComponentModel.INotifyPropertyChanged]
    public sealed partial class SettingsPage : Page
    {
        [ObservableProperty]
        private ElementTheme elementTheme;
        private ThemeSelectorService themeSelectorService;
        private IErrorHandlerService errorHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// </summary>
        public SettingsPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.errorHandler = Ioc.Default.GetService<IErrorHandlerService>()!;
            this.themeSelectorService = Ioc.Default.GetService<ThemeSelectorService>()!;
            this.ElementTheme = this.themeSelectorService.Theme;
            this.SwitchThemeCommand = new AsyncCommand<ElementTheme>(this.SetThemeAsync, (n) => true, this.errorHandler);
        }

        /// <summary>
        /// Gets the Switch Theme Command.
        /// </summary>
        public AsyncCommand<ElementTheme> SwitchThemeCommand { get; private set; }

        private Task SetThemeAsync(ElementTheme theme)
        {
            this.ElementTheme = theme;
            this.themeSelectorService.SetTheme(theme);
            return Task.CompletedTask;
        }
    }
}
