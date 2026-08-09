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
using UnityEngine;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam leaderboard component.
    /// </summary>
    [AddComponentMenu("Steamworks/Leaderboard")]
    [HelpURL("https://heathen.group/kb/leaderboards/")]
    public class SteamLeaderboardData : MonoBehaviour
    {
        /// <summary>
        /// Specifies the sort order for a new leaderboard.
        /// </summary>
        public enum LeaderboardSortMethod
        {
            /// <summary>The lowest score is at the top of the leaderboard.</summary>
            TopIsLowestScore  = 1,
            /// <summary>The highest score is at the top of the leaderboard.</summary>
            TopIsHighestScore = 2,
        }

        /// <summary>
        /// Specifies the display type for a new leaderboard.
        /// </summary>
        public enum LeaderboardDisplayType
        {
            /// <summary>The leaderboard score is a simple number.</summary>
            Numeric        = 1,
            /// <summary>The leaderboard score represents seconds.</summary>
            TimeSeconds    = 2,
            /// <summary>The leaderboard score represents milliseconds.</summary>
            TimeMilliSeconds = 3,
        }

        /// <summary>
        /// The API name of the leaderboard.
        /// </summary>
        public string               apiName;
        /// <summary>
        /// Should the leaderboard be created if it is not found on the Steam backend?
        /// </summary>
        public bool                 createIfMissing;
        /// <summary>
        /// If creating, what display type should be used?
        /// </summary>
        public LeaderboardDisplayType createAsDisplay = LeaderboardDisplayType.Numeric;
        /// <summary>
        /// If creating, what sort method should be used?
        /// </summary>
        public LeaderboardSortMethod  createWithSort  = LeaderboardSortMethod.TopIsLowestScore;

        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private List<string> mDelegates;

        /// <summary>
        /// Gets or sets the leaderboard data.
        /// </summary>
        public LeaderboardData Data
        {
            get => _data;
            set
            {
                _data = value;
                if (_events != null)
                    _events.onChange?.Invoke();
            }
        }

        private LeaderboardData            _data;
        private ISteamLeaderboardDataEvents _events;

        private void Awake()
        {
            _events = GetComponent<ISteamLeaderboardDataEvents>();
        }

        private void Start()
        {
            if (SteamTools.Interface.IsReady)
                ResolveBoard();
            else
                SteamTools.Interface.OnReady += OnInterfaceReady;
        }

        private void OnDestroy()
        {
            SteamTools.Interface.OnReady -= OnInterfaceReady;
        }

        private void OnInterfaceReady()
        {
            SteamTools.Interface.OnReady -= OnInterfaceReady;
            ResolveBoard();
        }

        private void ResolveBoard()
        {
            if (_data.IsValid) return;
            if (string.IsNullOrEmpty(apiName)) return;

            _data = SteamTools.Interface.GetBoard(apiName);
            if (_data.IsValid)
            {
                if (_events != null)
                    _events.onChange?.Invoke();
                return;
            }

            if (createIfMissing)
            {
                API.Leaderboards.Client.FindOrCreate(
                    apiName,
                    (ELeaderboardSortMethod)createWithSort,
                    (ELeaderboardDisplayType)createAsDisplay,
                    (result, ioError) =>
                    {
                        if (!ioError)
                        {
                            _data = result;
                            if (_events != null)
                                _events.onFindOrCreate?.Invoke();
                        }
                        else if (_events != null)
                            _events.onFindOrCreateFailure?.Invoke();
                    });
            }
        }

    }
}
#endif
