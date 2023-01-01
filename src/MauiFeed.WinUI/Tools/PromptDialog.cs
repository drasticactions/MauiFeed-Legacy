// <copyright file="PromptDialog.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace MauiFeed
{
    public sealed class PromptDialog : ContentDialog
    {
        public PromptDialog()
        {
            this.Title = "Add Feed";
            this.PrimaryButtonText = "Ok";
            this.SecondaryButtonText = "Cancel";

            var layout = new StackPanel();

            this.TextBlockMessage = new TextBlock { Text = string.Empty, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap };
            this.TextBoxInput = new TextBox();

            layout.Children.Add(this.TextBlockMessage);
            layout.Children.Add(this.TextBoxInput);

            this.Content = layout;
        }

        internal TextBlock TextBlockMessage { get; private set; }

        internal TextBox TextBoxInput { get; private set; }

        public string Message
        {
            get => this.TextBlockMessage.Text;
            set => this.TextBlockMessage.Text = value;
        }

        public string Input
        {
            get => this.TextBoxInput.Text;
            set => this.TextBoxInput.Text = value;
        }

        public string Placeholder
        {
            get => this.TextBoxInput.PlaceholderText;
            set => this.TextBoxInput.PlaceholderText = value;
        }

        public int MaxLength
        {
            get => this.TextBoxInput.MaxLength;
            set => this.TextBoxInput.MaxLength = value;
        }

        public InputScope InputScope
        {
            get => this.TextBoxInput.InputScope;
            set => this.TextBoxInput.InputScope = value;
        }
    }
}
