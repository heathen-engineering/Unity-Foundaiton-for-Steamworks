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

#if !DISABLESTEAMWORKS && STEAM_INSTALLED
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Instantiates a prefab for each leaderboard entry and calls
    /// <see cref="ILeaderboardEntryDisplay.Entry"/> on the resulting object.
    /// </summary>
    [ModularComponent(typeof(SteamLeaderboardData), "Display", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    public class SteamLeaderboardDisplay : MonoBehaviour
    {
        /// <summary>
        /// The transform that will act as the parent for instantiated entry prefabs.
        /// </summary>
        [ElementField("Display")]
        public Transform collectionRoot;
        /// <summary>
        /// The prefab template to instantiate for each leaderboard entry.
        /// </summary>
        [TemplateField("Display")]
        public GameObject entryTemplate;
        /// <summary>
        /// Should the local player's entry always be included in the display, even if they are not in the retrieved range?
        /// </summary>
        [TemplateField("Display")]
        public bool alwaysIncludePlayer;

        private SteamLeaderboardData      _mInspector;
        private readonly List<GameObject> _createdEntries = new();

        private void Awake()
        {
            _mInspector = GetComponent<SteamLeaderboardData>();
        }

        /// <summary>
        /// Requests the top entries for the leaderboard and displays them.
        /// </summary>
        /// <param name="count">The number of entries to retrieve.</param>
        public void GetTopEntries(int count)
        {
            if (_mInspector.Data.IsValid)
                _mInspector.Data.GetTopEntries(count, 0, HandleResults);
        }

        /// <summary>
        /// Requests entries around the local user and displays them.
        /// </summary>
        /// <param name="count">The number of entries to retrieve.</param>
        public void GetNearByEntries(int count)
        {
            if (_mInspector.Data.IsValid)
                _mInspector.Data.GetEntries(
                    ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser,
                    -count / 2, count / 2, 0, HandleResults);
        }

        /// <summary>
        /// Requests leaderboard entries for a specific range and displays them.
        /// </summary>
        /// <param name="request">The type of range to request.</param>
        /// <param name="start">The start index.</param>
        /// <param name="end">The end index.</param>
        /// <param name="maxDetailEntries">The maximum number of detail entries to retrieve.</param>
        public void GetEntries(ELeaderboardDataRequest request, int start, int end, int maxDetailEntries)
        {
            if (_mInspector.Data.IsValid)
                _mInspector.Data.GetEntries(request, start, end, maxDetailEntries, HandleResults);
        }

        private void HandleResults(LeaderboardEntry[] entries, bool ioError)
        {
            if (ioError)
            {
                Debug.LogError("[SteamLeaderboardDisplay] IO error fetching entries.");
                return;
            }

            if (!alwaysIncludePlayer || entries.Any(e => e.User.IsMe))
            {
                Display(entries);
                return;
            }

            _mInspector.Data.GetUserEntry(0, (userEntry, userError) =>
            {
                if (!userError && userEntry != null)
                    entries = new List<LeaderboardEntry>(entries) { userEntry }.ToArray();
                Display(entries);
            });
        }

        /// <summary>
        /// Clears existing entries and displays the provided entries.
        /// </summary>
        /// <param name="entries">The entries to display.</param>
        public void Display(LeaderboardEntry[] entries)
        {
            foreach (var go in _createdEntries)
                Destroy(go);
            _createdEntries.Clear();

            if (entries == null || entryTemplate == null || collectionRoot == null) return;

            foreach (var entry in entries)
            {
                var go = Instantiate(entryTemplate, collectionRoot);
                _createdEntries.Add(go);

                var display = go.GetComponent<ILeaderboardEntryDisplay>();
                if (display != null)
                    display.Entry = entry;
            }
        }

    }
}
#endif
