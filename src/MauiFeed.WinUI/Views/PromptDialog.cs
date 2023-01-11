// <copyright file="PromptDialog.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace MauiFeed.WinUI.Views
{
    public sealed class PromptDialog : ContentDialog
    {
        public PromptDialog()
        {
            Title = Translations.Common.AddFeedButton;
            PrimaryButtonText = "Ok";
            SecondaryButtonText = "Cancel";

            var layout = new StackPanel();

            TextBlockMessage = new TextBlock { Text = string.Empty, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap };
            TextBoxInput = new TextBox();

            layout.Children.Add(TextBlockMessage);
            layout.Children.Add(TextBoxInput);

            Content = layout;
        }

        internal TextBlock TextBlockMessage { get; private set; }

        internal TextBox TextBoxInput { get; private set; }

        public string Message
        {
            get => TextBlockMessage.Text;
            set => TextBlockMessage.Text = value;
        }

        public string Input
        {
            get => TextBoxInput.Text;
            set => TextBoxInput.Text = value;
        }

        public string Placeholder
        {
            get => TextBoxInput.PlaceholderText;
            set => TextBoxInput.PlaceholderText = value;
        }

        public int MaxLength
        {
            get => TextBoxInput.MaxLength;
            set => TextBoxInput.MaxLength = value;
        }

        public InputScope InputScope
        {
            get => TextBoxInput.InputScope;
            set => TextBoxInput.InputScope = value;
        }
    }
}
