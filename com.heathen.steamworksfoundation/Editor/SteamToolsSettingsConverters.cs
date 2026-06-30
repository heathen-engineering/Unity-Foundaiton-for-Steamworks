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

#if UNITY_EDITOR && !DISABLESTEAMWORKS && STEAM_INSTALLED && HEATHEN_GAMEFRAMEWORK
using System;
using Heathen.Editor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Teaches the framework <see cref="SettingsStore"/> how to serialise the Steam value-types that
    /// <see cref="SteamToolsSettings"/> stores. These types carry their data in a private backing field and
    /// expose only Steam-API-backed computed properties, so Newtonsoft's default contract would drop the data
    /// (the private field) and call the API (the properties). Each converter instead reads/writes the backing
    /// scalar — an achievement/stat is just its API name, an input action is its name + type.
    /// </summary>
    [InitializeOnLoad]
    internal static class SteamToolsSettingsConverters
    {
        private static bool _registered;

        static SteamToolsSettingsConverters() => EnsureRegistered();

        /// <summary>
        /// Register the Steam value-type converters with the framework store exactly once. Called both from
        /// this type's <c>[InitializeOnLoad]</c> and from <see cref="SteamToolsSettings.GetOrCreate"/> right
        /// before the first load, because <c>[InitializeOnLoad]</c> order across types is not deterministic and
        /// another editor hook can read the settings first — the converters must be in place before any read.
        /// </summary>
        public static void EnsureRegistered()
        {
            if (_registered) return;
            _registered = true;

            SettingsStore.AddConverter(new AchievementDataConverter());
            SettingsStore.AddConverter(new StatDataConverter());
            SettingsStore.AddConverter(new InputActionDataConverter());
        }
    }

    /// <summary>Serialises an <see cref="AchievementData"/> as its API name string.</summary>
    internal sealed class AchievementDataConverter : JsonConverter<AchievementData>
    {
        public override void WriteJson(JsonWriter writer, AchievementData value, JsonSerializer serializer)
            => writer.WriteValue((string)value);

        public override AchievementData ReadJson(JsonReader reader, Type objectType, AchievementData existingValue,
            bool hasExistingValue, JsonSerializer serializer)
            => (AchievementData)(reader.Value as string ?? string.Empty);
    }

    /// <summary>Serialises a <see cref="StatData"/> as its API name string.</summary>
    internal sealed class StatDataConverter : JsonConverter<StatData>
    {
        public override void WriteJson(JsonWriter writer, StatData value, JsonSerializer serializer)
            => writer.WriteValue((string)value);

        public override StatData ReadJson(JsonReader reader, Type objectType, StatData existingValue,
            bool hasExistingValue, JsonSerializer serializer)
            => (StatData)(reader.Value as string ?? string.Empty);
    }

    /// <summary>Serialises an <see cref="InputActionData"/> as its name + type.</summary>
    internal sealed class InputActionDataConverter : JsonConverter<InputActionData>
    {
        public override void WriteJson(JsonWriter writer, InputActionData value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(value.Name);
            writer.WritePropertyName("type");
            writer.WriteValue(value.Type.ToString());
            writer.WriteEndObject();
        }

        public override InputActionData ReadJson(JsonReader reader, Type objectType, InputActionData existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var o = JObject.Load(reader);
            var name = (string)o["name"] ?? string.Empty;
            Enum.TryParse((string)o["type"], out InputActionType type);
            return new InputActionData(name, type);
        }
    }
}
#endif
