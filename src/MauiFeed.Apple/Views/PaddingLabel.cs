// <copyright file="PaddingLabel.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;

namespace MauiFeed.Apple
{
    public class PaddingLabel : UILabel
    {
        private UIEdgeInsets textEdgeInsets = UIEdgeInsets.Zero;

        public PaddingLabel()
        {
            this.Layer.MasksToBounds = true;
            this.Layer.CornerRadius = 5f;
            this.BackgroundColor = UIColorExtensions.GetSystemTint();
            this.Font = this.Font.WithSize(10);
        }

        public UIEdgeInsets TextEdgeInsets
        {
            get => this.textEdgeInsets;
            set
            {
                this.textEdgeInsets = value;
                this.InvalidateIntrinsicContentSize();
            }
        }

        public override CGRect TextRectForBounds(CGRect bounds, nint numberOfLines)
        {
            var insetRect = this.textEdgeInsets.InsetRect(bounds);
            var textRect = base.TextRectForBounds(insetRect, numberOfLines);
            var invertedInsets = new UIEdgeInsets(-this.textEdgeInsets.Top, -this.textEdgeInsets.Left, -this.textEdgeInsets.Bottom, -this.textEdgeInsets.Right);
            return invertedInsets.InsetRect(textRect);
        }

        public override void DrawText(CGRect rect)
        {
            base.DrawText(this.textEdgeInsets.InsetRect(rect));
        }
    }
}