// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using NUnit.Common;
using NUnit.Engine.Compatibility;

namespace NUnit.Engine
{
    /// <summary>
    /// RuntimeFramework represents a particular version
    /// of a common language runtime implementation.
    /// </summary>
    [Serializable]
    public sealed class RuntimeFramework : IRuntimeFramework
    {
        #region Constructors

        /// <summary>
        /// Construct from a Runtime and Version.
        /// </summary>
        /// <param name="runtime">A Runtime instance</param>
        /// <param name="version">The Version of the framework</param>
        public RuntimeFramework(Runtime runtime, Version version)
            : this(runtime, version, null)
        {
        }

        /// <summary>
        /// Construct from a Runtime, Version and profile.
        /// </summary>
        /// <param name="runtime">A Runtime instance.</param>
        /// <param name="version">The Version of the framework.</param>
        /// <param name="profile">A string representing the profile of the framework. Null if unspecified.</param>
        public RuntimeFramework(Runtime runtime, Version version, string profile)
        {
            Guard.ArgumentNotNull(runtime, nameof(runtime));
            Guard.ArgumentValid(IsValidFrameworkVersion(version), $"{version} is not a valid framework version", nameof(version));

            Runtime = runtime;
            FrameworkVersion = ClrVersion = version;
            ClrVersion = runtime.GetClrVersionForFramework(version);

            Profile = profile;

            DisplayName = GetDefaultDisplayName(runtime, FrameworkVersion, profile);

            FrameworkName = new FrameworkName(runtime.FrameworkIdentifier, FrameworkVersion);
        }

        private bool IsValidFrameworkVersion(Version v)
        {
            // All known framework versions have either two components or
            // three. If three, then the Build is currently less than 3.
            return v.Major > 0 && v.Minor >= 0 && v.Build < 3 && v.Revision == -1;
        }

        #endregion

        #region IRuntimeFramework Implementation

        /// <summary>
        /// Gets the unique Id for this runtime, such as "net-4.5"
        /// </summary>
        public string Id => Runtime.ToString().ToLower() + "-" + FrameworkVersion.ToString();

        public FrameworkName FrameworkName { get; }

        /// <summary>
        /// The type of this runtime framework
        /// </summary>
        public Runtime Runtime { get; }

        /// <summary>
        /// The framework version for this runtime framework
        /// </summary>
        public Version FrameworkVersion { get; private set; }

        /// <summary>
        /// The CLR version for this runtime framework
        /// </summary>
        public Version ClrVersion { get; set; }

        /// <summary>
        /// The Profile for this framework, where relevant.
        /// May be null and will have different sets of
        /// values for each Runtime.
        /// </summary>
        public string Profile { get; private set; }

        /// <summary>
        /// Returns the Display name for this framework
        /// </summary>
        public string DisplayName { get; set; }

        #endregion

        /// <summary>
        /// Parses a string representing a RuntimeFramework.
        /// The string may be just a RuntimeType name or just
        /// a Version or a hyphenated RuntimeType-Version or
        /// a Version prefixed by 'v'.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static RuntimeFramework Parse(string s)
        {
            Guard.ArgumentNotNullOrEmpty(s, nameof(s));

            string[] parts = s.Split(new char[] { '-' });
            Guard.ArgumentValid(parts.Length == 2 && parts[0].Length > 0 && parts[1].Length > 0, "RuntimeFramework id not in correct format", nameof(s));

            var runtime = Runtime.Parse(parts[0]);
            var version = new Version(parts[1]);
            return new RuntimeFramework(runtime, version);
        }

        public static bool TryParse(string s, out RuntimeFramework runtimeFramework)
        {
            try
            {
                runtimeFramework = Parse(s);
                return true;
            }
            catch
            {
                runtimeFramework = null;
                return false;
            }
        }

        public static RuntimeFramework FromFrameworkName(string frameworkName)
        {
            return FromFrameworkName(new FrameworkName(frameworkName));
        }

        public static RuntimeFramework FromFrameworkName(FrameworkName frameworkName)
        {
            return new RuntimeFramework(Runtime.FromFrameworkIdentifier(frameworkName.Identifier), frameworkName.Version, frameworkName.Profile);
        }

        /// <summary>
        /// Overridden to return the short name of the framework
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Id;

        /// <summary>
        /// Returns true if the current framework matches the
        /// one supplied as an argument. Both the RuntimeType
        /// and the version must match.
        /// 
        /// Two RuntimeTypes match if they are equal, if either one
        /// is RuntimeType.Any or if one is RuntimeType.Net and
        /// the other is RuntimeType.Mono.
        /// 
        /// Two versions match if all specified version components
        /// are equal. Negative (i.e. unspecified) version
        /// components are ignored.
        /// </summary>
        /// <param name="target">The RuntimeFramework to be matched.</param>
        /// <returns><c>true</c> on match, otherwise <c>false</c></returns>
        public bool Supports(RuntimeFramework target)
        {
            if (!Runtime.Matches(target.Runtime))
                return false;

            return VersionsMatch(this.ClrVersion, target.ClrVersion)
                && this.FrameworkVersion.Major >= target.FrameworkVersion.Major
                && this.FrameworkVersion.Minor >= target.FrameworkVersion.Minor;
        }

        public bool CanLoad(IRuntimeFramework requested)
        {
            return FrameworkVersion >= requested.FrameworkVersion;
        }

        private static string GetDefaultDisplayName(Runtime runtime, Version version, string profile)
        {
            string displayName = $"{runtime.DisplayName} {version}";

            if (!string.IsNullOrEmpty(profile) && profile != "Full")
                displayName += " - " + profile;

            return displayName;
        }

        private static bool VersionsMatch(Version v1, Version v2)
        {
            return v1.Major == v2.Major &&
                   v1.Minor == v2.Minor &&
                  (v1.Build < 0 || v2.Build < 0 || v1.Build == v2.Build) &&
                  (v1.Revision < 0 || v2.Revision < 0 || v1.Revision == v2.Revision);
        }

        private static string GetMonoPrefixFromAssembly(Assembly assembly)
        {
            string prefix = assembly.Location;

            // In all normal mono installations, there will be sufficient
            // levels to complete the four iterations. But just in case
            // files have been copied to some non-standard place, we check.
            for (int i = 0; i < 4; i++)
            {
                string dir = Path.GetDirectoryName(prefix);
                if (string.IsNullOrEmpty(dir)) break;

                prefix = dir;
            }

            return prefix;
        }
    }
}
#endif
