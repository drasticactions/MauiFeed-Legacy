// <copyright file="AppDispatcher.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using Drastic.Services;

namespace MauiFeed.Apple
{
    public class AppDispatcher : NSObject, IAppDispatcher
    {
        public bool Dispatch(Action action)
        {
            this.InvokeOnMainThread(action);
            return true;
        }
    }
}