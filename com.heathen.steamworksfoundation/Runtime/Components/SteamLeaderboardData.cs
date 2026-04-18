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
    [AddComponentMenu("Steamworks/Leaderboard")]
    [HelpURL("https://kb.heathen.group/steamworks/features/leaderboards")]
    public class SteamLeaderboardData : MonoBehaviour
    {
        public enum LeaderboardSortMethod
        {
            TopIsLowestScore  = 1,
            TopIsHighestScore = 2,
        }

        public enum LeaderboardDisplayType
        {
            Numeric        = 1,
            TimeSeconds    = 2,
            TimeMilliSeconds = 3,
        }

        public string               apiName;
        public bool                 createIfMissing;
        public LeaderboardDisplayType createAsDisplay = LeaderboardDisplayType.Numeric;
        public LeaderboardSortMethod  createWithSort  = LeaderboardSortMethod.TopIsLowestScore;

        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private List<string> mDelegates;

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
            _events = GetComponent<ISteamLeaderboardDataEvents>();

            if (SteamTools.Interface.IsReady)
                ResolveBoard();
            else
                SteamTools.Interface.OnReady += OnInterfaceReady;
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
