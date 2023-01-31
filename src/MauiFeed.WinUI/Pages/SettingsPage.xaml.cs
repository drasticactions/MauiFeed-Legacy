// <copyright file="SettingsPage.xaml.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Drastic.Services;
using Drastic.Tools;
using MauiFeed.Models;
using MauiFeed.Translations;
using MauiFeed.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MauiFeed.WinUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        private ElementTheme elementTheme;
        private ThemeSelectorService themeSelectorService;
        private IErrorHandlerService errorHandler;
        private ApplicationSettingsService applicationSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// </summary>
        public SettingsPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.errorHandler = Ioc.Default.GetService<IErrorHandlerService>()!;
            this.applicationSettingsService = Ioc.Default.GetService<ApplicationSettingsService>()!;
            this.themeSelectorService = Ioc.Default.GetService<ThemeSelectorService>()!;
            this.ElementTheme = this.themeSelectorService.Theme;
            this.SwitchThemeCommand = new AsyncCommand<ElementTheme>(this.SetThemeAsync, (n) => true, this.errorHandler);
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the element theme.
        /// </summary>
        public ElementTheme ElementTheme
        {
            get { return this.elementTheme; }
            set { this.SetProperty(ref this.elementTheme, value); }
        }

        /// <summary>
        /// Gets or sets the element theme.
        /// </summary>
        public LanguageSetting LanguageSetting
        {
            get
            {
                return this.applicationSettingsService.ApplicationLanguageSetting;
            }

            set
            {
                this.applicationSettingsService.ApplicationLanguageSetting = value;
            }
        }

        /// <summary>
        /// Gets the Switch Theme Command.
        /// </summary>
        public AsyncCommand<ElementTheme> SwitchThemeCommand { get; private set; }

        /// <summary>
        /// Gets the Languages.
        /// </summary>
        public List<Tuple<string, LanguageSetting>> Languages { get; } = new List<Tuple<string, LanguageSetting>>()
        {
            new Tuple<string, LanguageSetting>(Common.DefaultLanguage, LanguageSetting.Default),
            new Tuple<string, LanguageSetting>(Common.EnglishLanguage, LanguageSetting.English),
            new Tuple<string, LanguageSetting>(Common.JapaneseLanguage, LanguageSetting.Japanese),
        };

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

        private Task SetThemeAsync(ElementTheme theme)
        {
            this.ElementTheme = theme;
            this.themeSelectorService.SetTheme(theme);
            return Task.CompletedTask;
        }

        private void LanguageComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            this.LanguageComboBox.SelectedIndex = this.Languages.IndexOf(this.Languages.First(n => n.Item2 == this.LanguageSetting));
        }

        private void LanguageComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.LanguageSetting = this.Languages[this.LanguageComboBox.SelectedIndex].Item2;
        }
    }
}
