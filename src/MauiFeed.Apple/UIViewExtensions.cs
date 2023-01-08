using System;
namespace MauiFeed.Apple
{
    public static class UIViewExtensions
    {
        public static void SetHidden(this UIView view, bool isHidden, bool animated, Action? completion = null)
        {
            if (isHidden && view.Hidden)
            {
                return;
            }

            if (animated)
            {
                var startAlpha = isHidden ? 1 : 0;
                var animatingAlpha = isHidden ? 0 : 1;
                view.Alpha = startAlpha;
                UIView.Animate(0.25, 0.1, UIViewAnimationOptions.CurveEaseOut, () => view.Alpha = animatingAlpha, () =>
                {
                    view.Hidden = isHidden;
                    view.Alpha = 1.0f;
                    completion?.Invoke();
                });
            }
            else
            {
                view.Hidden = isHidden;
                completion?.Invoke();
            }
        }
    }
}