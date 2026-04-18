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

// Minimal custom editors for Foundation components (removes the m_Script row).
// Foundation owns Achievement, Stat, and Leaderboard features entirely — their
// editors compile in all configurations. User components and SteamLeaderboardDataEvents
// still have a Toolkit-installed alternative, so those entries are conditionally guarded.

#if UNITY_EDITOR && !DISABLESTEAMWORKS && STEAM_INSTALLED
using UnityEditor;

namespace Heathen.SteamworksIntegration
{
    // ---- User child components (Foundation owns these in all configurations) ----------------

    [CustomEditor(typeof(SteamUserName), true)]
    public class SteamUserNameEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamUserLevel), true)]
    public class SteamUserLevelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamUserStatus), true)]
    public class SteamUserStatusEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamUserDataEvents), true)]
    public class SteamUserDataEventsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamUserHexInput), true)]
    public class SteamUserHexInputEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamUserHexLabel), true)]
    public class SteamUserHexLabelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    // ---- Achievement child components (Foundation owns these in all configurations) ----------

    [CustomEditor(typeof(SteamAchievementName), true)]
    public class SteamAchievementNameEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamAchievementDescription), true)]
    public class SteamAchievementDescriptionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamAchievementIcon), true)]
    public class SteamAchievementIconEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamAchievementChanged), true)]
    public class SteamAchievementChangedEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    // ---- Stat child components -----------------------------------------------------------

    [CustomEditor(typeof(SteamStatIntDisplay), true)]
    public class SteamStatIntDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamStatFloatDisplay), true)]
    public class SteamStatFloatDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    // ---- Leaderboard child components ---------------------------------------------------

    // SteamLeaderboardDataEvents: Foundation's version (no UGC) is guarded when Toolkit is
    // installed because Toolkit has its own UGC-extended version with the same name.
#if !HEATHEN_TOOLKIT_INSTALLED
    [CustomEditor(typeof(SteamLeaderboardDataEvents), true)]
    public class SteamLeaderboardDataEventsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif // !HEATHEN_TOOLKIT_INSTALLED

    [CustomEditor(typeof(SteamLeaderboardDisplay), true)]
    public class SteamLeaderboardDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamLeaderboardEntryUI), true)]
    public class SteamLeaderboardEntryUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamLeaderboardName), true)]
    public class SteamLeaderboardNameEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamLeaderboardRank), true)]
    public class SteamLeaderboardRankEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamLeaderboardUpload), true)]
    public class SteamLeaderboardUploadEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SteamLeaderboardUserEntry), true)]
    public class SteamLeaderboardUserEntryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
