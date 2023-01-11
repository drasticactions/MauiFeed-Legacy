// <copyright file="WinUIExtensions.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Windows.Storage.Streams;

namespace MauiFeed.WinUI.Tools
{
    /// <summary>
    /// WinUI Extensions.
    /// </summary>
    public static class WinUIExtensions
    {
        /// <summary>
        /// Create a random access stream from a byte array.
        /// </summary>
        /// <param name="array">The byte array.</param>
        /// <returns><see cref="IRandomAccessStream"/>.</returns>
        public static IRandomAccessStream ToRandomAccessStream(this byte[] array)
        {
            InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
            using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(array);
                writer.StoreAsync().GetResults();
            }

            return ms;
        }
    }
}
