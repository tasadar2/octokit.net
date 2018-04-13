﻿using System;
using System.Diagnostics;
using System.Globalization;
using Octokit.Internal;

namespace Octokit
{
    /// <summary>
    /// Describes the current download state of a pre-receive environment image.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PreReceiveEnvironmentDownload
    {
        /// <summary>
        /// URL to the download status for a pre-receive environment.
        /// </summary>
        public string Url { get; protected set; }

        /// <summary>
        /// The state of the most recent download.
        /// </summary>
        public StringEnum<PreReceiveEnvironmentDownloadState> State { get; protected set; }

        /// <summary>
        /// On failure, this will have any error messages produced.
        /// </summary>
        public string Message { get; protected set; }

        /// <summary>
        /// The time when the most recent download started.
        /// </summary>
        [Parameter(Key = "downloaded_at")]
        public DateTimeOffset? DownloadedAt { get; protected set; }

        internal string DebuggerDisplay
        {
            get { return string.Format(CultureInfo.InvariantCulture, "State: {0} Message: {1}", State, Message); }
        }
    }
}
