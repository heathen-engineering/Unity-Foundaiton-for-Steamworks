// Copyright 2024 Heathen Engineering
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if UNITY_EDITOR
using System;
using UnityEditor;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Detects whether the Toolkit package (<c>Heathen.Steamworks</c> assembly) is loaded,
    /// so Foundation's editor UI can defer to the Toolkit's richer settings when available.
    /// </summary>
    [InitializeOnLoad]
    public static class FoundationPackageDetector
    {
        private static bool? _toolkitInstalled;

        /// <summary>
        /// True when the <c>Heathen.Steamworks</c> runtime assembly is present in the current domain.
        /// This is cached per editor session; the cache is cleared on domain reload.
        /// </summary>
        public static bool IsToolkitInstalled
        {
            get
            {
                _toolkitInstalled ??= IsAssemblyLoaded("Heathen.Steamworks");
                return _toolkitInstalled.Value;
            }
        }

        static FoundationPackageDetector()
        {
            // Clear the cache on every domain reload so that installing/removing
            // the Toolkit is reflected without requiring an editor restart.
            _toolkitInstalled = null;
        }

        private static bool IsAssemblyLoaded(string assemblyName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                if (asm.GetName().Name == assemblyName)
                    return true;
            return false;
        }
    }
}
#endif
