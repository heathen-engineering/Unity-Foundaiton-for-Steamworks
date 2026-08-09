// Copyright 2024 Heathen Engineering Limited
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
#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System.IO;
using System.Linq;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A helper data type that pre-defines all the typical fields of a Steam Workshop Item for quick and easy create or update.
    /// </summary>
    [System.Serializable]
    public struct WorkshopItemEditorData
    {
        /// <summary>
        /// The published file ID to be updated. Null for new items.
        /// </summary>
        public PublishedFileId_t? PublishedFileId;
        /// <summary>
        /// The consume and creating app ID of the item.
        /// </summary>
        public AppData appId;
        /// <summary>
        /// The title of the item.
        /// </summary>
        public string title;
        /// <summary>
        /// The description of the item.
        /// </summary>
        public string description;
        /// <summary>
        /// The local folder where the item's content is located.
        /// </summary>
        public DirectoryInfo Content;
        /// <summary>
        /// The local file that is the item's main preview image.
        /// </summary>
        public FileInfo Preview;
        /// <summary>
        /// Any metadata associated with the item.
        /// </summary>
        public string metadata;
        /// <summary>
        /// Any tags associated with the item.
        /// </summary>
        public string[] tags;
        /// <summary>
        /// The visibility settings for this item.
        /// </summary>
        public ERemoteStoragePublishedFileVisibility visibility;

        /// <summary>
        /// Validates the minimum required fields are populated and within Steamworks limits.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return appId != AppId_t.Invalid
                    && !string.IsNullOrEmpty(title)
                    && title.Length < Constants.k_cchPublishedDocumentTitleMax
                    && !string.IsNullOrEmpty(description)
                    && description.Length < Constants.k_cchPublishedDocumentDescriptionMax
                    && (string.IsNullOrEmpty(metadata) || metadata.Length < Constants.k_cchDeveloperMetadataMax)
                    && Preview != null
                    && Content != null
                    && Preview.Exists
                    && Content.Exists
                    && (tags == null || tags.Length == 0 || !tags.Any(p => p.Length > 255));
            }
        }
    }
}
#endif
