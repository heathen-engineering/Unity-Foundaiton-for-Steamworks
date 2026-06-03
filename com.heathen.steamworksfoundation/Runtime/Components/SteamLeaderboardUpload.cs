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
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Uploads a score to the tracked leaderboard with the configured mode and details.
    /// </summary>
    [ModularComponent(typeof(SteamLeaderboardData), "Upload", "")]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    public class SteamLeaderboardUpload : MonoBehaviour
    {
        /// <summary>
        /// Specifies the upload mode for the leaderboard score.
        /// </summary>
        public enum Mode { KeepBest, ForceUpdate }

        /// <summary>The upload mode to use.</summary>
        public Mode       mode    = Mode.KeepBest;
        /// <summary>The score value to upload.</summary>
        public int        score   = 0;
        /// <summary>Additional detail values to upload with the score.</summary>
        public List<int>  details = new();

        private SteamLeaderboardData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLeaderboardData>();
        }

        /// <summary>
        /// Performs the score upload using current settings.
        /// </summary>
        public void Upload()
        {
            if (!_mInspector.Data.IsValid) return;

            if (mode == Mode.KeepBest)
                _mInspector.Data.UploadScoreKeepBest(score, details.ToArray());
            else
                _mInspector.Data.UploadScoreForceUpdate(score, details.ToArray());
        }

        /// <summary>
        /// Sets the score and performs the upload.
        /// </summary>
        /// <param name="value">The score to upload.</param>
        public void UploadScore(int value)
        {
            score = value;
            Upload();
        }

        /// <summary>
        /// Parses the string value into an integer score and performs the upload.
        /// </summary>
        /// <param name="value">The score as a string.</param>
        public void UploadScore(string value)
        {
            if (int.TryParse(value, out int parsed)) UploadScore(parsed);
        }

    }
}
#endif
