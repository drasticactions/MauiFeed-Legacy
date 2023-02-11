// <copyright file="SceneDelegate.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace MauiFeed.MacCatalyst;

/// <summary>
/// Default Scene Delegate.
/// </summary>
[Register("SceneDelegate")]
public class SceneDelegate : UIResponder, IUIWindowSceneDelegate
{
    /// <summary>
    /// Gets or sets the UIWindow.
    /// </summary>
    [Export("window")]
    public UIWindow? Window { get; set; }
}
