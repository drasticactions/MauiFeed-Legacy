// <copyright file="TestErrorHandler.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Drastic.Services;

namespace MauiFeed.Tests
{
    /// <summary>
    /// Error Handler.
    /// </summary>
    public class TestErrorHandler : IErrorHandlerService
    {
        /// <inheritdoc/>
        public void HandleError(Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
