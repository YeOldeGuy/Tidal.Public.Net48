﻿using System.Linq;

namespace Tidal.Models.Messages
{
    /// <summary>
    /// A message that contains the app's startup information from
    /// the command line.
    /// </summary>
    internal class StartupMessage
    {
        /// <summary>
        /// Create a message with startup information.
        /// </summary>
        /// <param name="args">The command line arguments as a string array.</param>
        /// <param name="firstRun">
        ///   If <see langword="true"/>, this instance is being created by a
        ///   second instance.
        /// </param>
        /// <remarks>
        /// Performs a check for the app executable being staged as the
        /// first argument, and removes it if so.
        /// </remarks>
        public StartupMessage(string[] args, bool firstRun = true)
        {
            Args = args.Length > 0 && args[0].EndsWith(".exe")
                    ? args.Skip(1).ToArray()
                    : args;
            IsFirstRun = firstRun;
        }

        /// <summary>
        /// Holds the command line arguments as presented by the OS.
        /// </summary>
        public string[] Args { get; }

        /// <summary>
        /// This will be <see langword="true"/> if the message was created by
        /// the first instance.
        /// </summary>
        public bool IsFirstRun { get; }
    }
}
