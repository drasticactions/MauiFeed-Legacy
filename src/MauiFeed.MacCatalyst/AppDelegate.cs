using MauiFeed.Apple;
using AppKit;
using ObjCRuntime;

namespace MauiFeed.MacCatalyst;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    public override UIWindow? Window
    {
        get;
        set;
    }

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // create a new window instance based on the screen size
        Window = new UIWindow(UIScreen.MainScreen.Bounds);
        var test = Window.WindowScene;
        if (test is not null)
        {
            test.Titlebar!.TitleVisibility = UITitlebarTitleVisibility.Visible;
            var toolbar = new NSToolbar();
            toolbar.Delegate = new MainToolbarDelegate();
            toolbar.DisplayMode = NSToolbarDisplayMode.Icon;

            test.Title = "MauiFeed";
            //test.Subtitle = "Foobar";
            test.Titlebar.Toolbar = toolbar;
            test.Titlebar.ToolbarStyle = UITitlebarToolbarStyle.Automatic;
            test.Titlebar.Toolbar.Visible = true;
        }

        // create a UIViewController with a single UILabel
        var vc = new RootSplitViewController();
        Window.RootViewController = vc;

        // make the window visible
        Window.MakeKeyAndVisible();

        return true;
    }
}

public class MainToolbarDelegate : AppKit.NSToolbarDelegate
{
    public const string Refresh = "Refresh";
    public const string Plus = "Plus";
    public const string MarkAllAsRead = "MarkAllAsRead";
    public const string MarkAsRead = "MarkAsRead";
    public const string HideRead = "HideRead";
    public const string Star = "Star";
    public const string NextUnread = "NextUnread";
    public const string ReaderView = "ReaderView";
    public const string Share = "Share";
    public const string OpenInBrowser = "OpenInBrowser";


    public override string[] DefaultItemIdentifiers(NSToolbar toolbar)
    {
        // https://github.com/xamarin/xamarin-macios/issues/12871
        // I only figured this out by going into a Catalyst Swift app,
        // and checking the raw value for NSToolbarItem.Identifier.primarySidebarTrackingSeparatorItemIdentifier
        // This value is not bound yet in dotnet maccatalyst.
        return new string[]
        {
                NSToolbar.NSToolbarFlexibleSpaceItemIdentifier,
                Refresh,
                Plus,
                "NSToolbarPrimarySidebarTrackingSeparatorItem",
                NSToolbar.NSToolbarFlexibleSpaceItemIdentifier,
                MarkAllAsRead,
                HideRead,
                "NSToolbarSupplementarySidebarTrackingSeparatorItem",
                MarkAsRead,
                Star,
                NextUnread,
                ReaderView,
                Share,
                OpenInBrowser
        };
    }

    public override string[] SelectableItemIdentifiers(NSToolbar toolbar)
    {
        return new string[]
         {
                HideRead,
         };
    }

    public override string[] AllowedItemIdentifiers(NSToolbar toolbar)
    {
        return new string[0];
    }

    public override NSToolbarItem? WillInsertItem(NSToolbar toolbar, string itemIdentifier, bool willBeInserted)
    {
        NSToolbarItem toolbarItem = new NSToolbarItem(itemIdentifier);

        if (itemIdentifier == Plus)
        {
            //var blah = new UIBarButtonItem(UIImage.GetSystemImage("plus"), UIBarButtonItemStyle.Plain, null);
            toolbarItem.UIImage = UIImage.GetSystemImage("plus");
        }
        else if (itemIdentifier == Refresh)
        {
            toolbarItem.UIImage = UIImage.GetSystemImage("arrow.clockwise");
        }
        else if (itemIdentifier == MarkAllAsRead)
        {
            toolbarItem.UIImage = UIImage.GetSystemImage("arrow.up.arrow.down.circle");
        }
        else if (itemIdentifier == HideRead)
        {
            toolbarItem.UIImage = UIImage.GetSystemImage("circle");
        }
        else if (itemIdentifier == MarkAsRead)
        {
            toolbarItem.UIImage = UIImage.GetSystemImage("book.circle");
        }
        else if (itemIdentifier == Star)
        {
            toolbarItem.UIImage = UIImage.GetSystemImage("star");
        }
        else if (itemIdentifier == NextUnread)
        {
            toolbarItem.UIImage = UIImage.GetSystemImage("arrowtriangle.down.circle");
        }
        else if (itemIdentifier == OpenInBrowser)
        {
            toolbarItem.UIImage = UIImage.GetSystemImage("safari");
        }
        else if (itemIdentifier == ReaderView)
        {
            toolbarItem.UIImage = UIImage.GetSystemImage("note.text");
        }
        else if (itemIdentifier == Share)
        {
            toolbarItem.UIImage = UIImage.GetSystemImage("square.and.arrow.up");
        }

        toolbarItem.Activated += ToolbarItem_Activated;
        return toolbarItem;
    }

    private void ToolbarItem_Activated(object? sender, EventArgs e)
    {
    }
}
