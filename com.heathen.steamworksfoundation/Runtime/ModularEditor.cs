#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;

namespace Heathen.SteamworksIntegration
{
    public abstract class ModularEditor : Editor
    {
        protected GameObject TargetGo => ((Component)target).gameObject;
        protected readonly Dictionary<Component, SerializedObject> SoCache = new();

        /// <summary>
        /// Derived editors must populate this with all allowed modular/component types.
        /// </summary>
        protected abstract Type[] AllowedTypes { get; }

        protected SerializedObject GetSo(Component comp)
        {
            if (!comp) return null;
            if (!SoCache.TryGetValue(comp, out var so) || so.targetObject == null)
                SoCache[comp] = so = new SerializedObject(comp);
            so.Update();
            return so;
        }

        // ---------------------------------------------------------------
        // Filtered Allowed Types
        // ---------------------------------------------------------------
        private Type ParentType => target.GetType();

        protected IEnumerable<Type> FilteredAllowedTypes
        {
            get
            {
                foreach (var t in AllowedTypes)
                {
                    var compAttr = t.GetCustomAttribute<ModularComponentAttribute>();
                    var evtAttr = t.GetCustomAttribute<ModularEventsAttribute>();
                    var parent = compAttr != null ? compAttr.ParentType :
                                 evtAttr?.ParentType;

                    if (parent != ParentType)
                    {
                        Debug.LogWarning($"Type {t.Name} in AllowedTypes does not match parent {(ParentType != null ? ParentType.Name :  "Unknown")} and will be ignored.");
                        continue;
                    }

                    yield return t;
                }
            }
        }

        // ---------------------------------------------------------------
        // Field / Function Dropdown Handling
        // ---------------------------------------------------------------
        protected void DrawAddFieldDropdown()
        {
            var options = new List<string> { "... Add New" };
            options.AddRange(
                FilteredAllowedTypes
                    .Select(t => t.GetCustomAttribute<ModularComponentAttribute>())
                    .Where(a => a != null && !string.IsNullOrEmpty(a.FieldName))
                    .Select(a => a.Header)
                    .Distinct()
            );

            if (options.Count == 1)
                return;

            int selected = EditorGUILayout.Popup("Fields", 0, options.ToArray());
            if (selected > 0)
            {
                var header = options[selected];
                var typeToAdd = FilteredAllowedTypes
                    .FirstOrDefault(t =>
                    {
                        var attr = t.GetCustomAttribute<ModularComponentAttribute>();
                        return attr != null && attr.Header == header && !string.IsNullOrEmpty(attr.FieldName);
                    });

                if (typeToAdd != null)
                    AddModularComponent(typeToAdd);
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected void DrawFunctionFlags()
        {
            var go = TargetGo;
            var parentType = target.GetType();

            // --- Gather all single-instance types (FieldName empty) ---
            var maskTypes = AllowedTypes
                .Where(t =>
                {
                    var attr = t.GetCustomAttribute<ModularComponentAttribute>();
                    if (attr != null && attr.ParentType != parentType)
                    {
                        Debug.LogWarning($"Type {t.Name} parent mismatch. Ignored.");
                        return false;
                    }

                    return attr != null && string.IsNullOrEmpty(attr.FieldName);
                })
                .ToList();

            // --- Append ModularEvents type if present ---
            var eventType = AllowedTypes
                .FirstOrDefault(t =>
                {
                    var attr = t.GetCustomAttribute<ModularEventsAttribute>();
                    return attr != null && attr.ParentType == parentType;
                });

            if (eventType != null)
                maskTypes.Add(eventType);

            if (maskTypes.Count == 0) return;

            // --- Build current mask value ---
            int maskValue = 0;
            for (int i = 0; i < maskTypes.Count; i++)
                if (go.GetComponent(maskTypes[i]))
                    maskValue |= 1 << i;

            // --- Display mask field ---
            EditorGUI.BeginChangeCheck();
            maskValue = EditorGUILayout.MaskField(
                "Settings",
                maskValue,
                maskTypes.Select(t =>
                {
                    var attr = t.GetCustomAttribute<ModularComponentAttribute>();
                    if (attr != null) return attr.Header;
                    if (t.GetCustomAttribute<ModularEventsAttribute>() != null) return "Events";
                    return t.Name;
                }).ToArray()
            );

            // --- Apply changes ---
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < maskTypes.Count; i++)
                {
                    var comp = go.GetComponent(maskTypes[i]);
                    bool has = comp;
                    bool should = (maskValue & (1 << i)) != 0;

                    if (should && !has)
                        go.AddComponent(maskTypes[i]).hideFlags = HideFlags.None;
                    else if (!should && has)
                        DestroyImmediate(comp);
                }
            }
        }

        /// <summary>
        /// Returns headers of components that have a FieldName set (these are individual "fields").
        /// </summary>
        protected string[] GetFieldOptions()
        {
            return FilteredAllowedTypes
                .Select(t => t.GetCustomAttribute<ModularComponentAttribute>())
                .Where(a => a != null && !string.IsNullOrEmpty(a.FieldName))
                .Select(a => a.FieldName)
                .ToArray();
        }

        /// <summary>
        /// Returns headers for modular mask options: components with empty FieldName + Events.
        /// </summary>
        protected string[] GetMaskOptions()
        {
            var headers = FilteredAllowedTypes
                .Select(t => t.GetCustomAttribute<ModularComponentAttribute>())
                .Where(a => a != null && string.IsNullOrEmpty(a.FieldName))
                .Select(a => a.Header)
                .ToList();

            // Always add Events last if any
            if (TargetGo.GetComponents<Component>().Any(c => c.GetType().GetCustomAttribute<ModularEventsAttribute>() != null))
                headers.Add("Events");

            return headers.ToArray();
        }

        // ---------------------------------------------------------------
        // Modular Component Drawing
        // ---------------------------------------------------------------
        protected void DrawModularComponents()
        {
            // Group components by type (only ones that have FieldName set)
            var compsByType = GetModularComponents()
                .Select(c => new { Comp = c, Attr = c.GetType().GetCustomAttribute<ModularComponentAttribute>() })
                .Where(x => x.Attr != null && !string.IsNullOrEmpty(x.Attr.FieldName))
                .GroupBy(x => x.Attr.Header);

            foreach (var group in compsByType)
            {
                var header = group.Key;
                var comps = group.Select(x => x.Comp).ToArray();
                if (comps.Length == 0) continue;

                EditorGUILayout.LabelField(header, EditorStyles.label);
                EditorGUI.indentLevel++;

                foreach (var comp in comps)
                {
                    if (!comp) continue;
                    comp.hideFlags = HideFlags.HideInInspector;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var attr = comp.GetType().GetCustomAttribute<ModularComponentAttribute>();
                        var so = GetSo(comp);
                        var prop = so.FindProperty(attr.FieldName);
                        if (prop != null)
                            DrawPropertyWithOptionalLabel(prop);

                        if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(25)))
                        {
                            DestroyImmediate(comp);
                            return;
                        }

                        so.ApplyModifiedProperties();
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(4);
            }
        }

        private void DrawPropertyWithOptionalLabel(SerializedProperty prop)
        {
            if (prop == null) return;

            // Generic type with children = complex class/struct
            if (prop.propertyType == SerializedPropertyType.Generic && prop.hasVisibleChildren)
            {
                EditorGUILayout.PropertyField(prop, new GUIContent(ObjectNames.NicifyVariableName(prop.displayName)), true);
            }
            else
            {
                EditorGUILayout.PropertyField(prop, GUIContent.none, true);
            }
        }

        // ---------------------------------------------------------------
        // Fields / Settings / Elements / Templates
        // ---------------------------------------------------------------
        protected void DrawFields<TAttr>(string label) where TAttr : PropertyAttribute
        {
            var comps = GetModularComponents().ToArray();

            // Early exit if no components with this attribute
            bool hasFields = comps.Any(c => c.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Any(f => f.GetCustomAttribute<TAttr>() != null));

            if (!hasFields) return;

            var synchronized = new Dictionary<(string, Type), (SerializedProperty prop, int priority)>();
            var syncedComponents = new Dictionary<(string, Type), List<Component>>();
            var normalFields = new Dictionary<string, List<(SerializedProperty prop, Component comp, int priority)>>();

            foreach (var comp in comps)
            {
                var so = GetSo(comp);
                foreach (var field in comp.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (field.GetCustomAttribute<TAttr>() is not IModularField mf) continue;
                    var prop = so.FindProperty(field.Name);
                    if (prop == null) continue;

                    if (mf.Synchronised)
                    {
                        var key = (field.Name, field.FieldType);
                        synchronized[key] = (prop, mf.Priority);
                        if (!syncedComponents.ContainsKey(key))
                            syncedComponents[key] = new List<Component>();
                        syncedComponents[key].Add(comp);
                    }
                    else
                    {
                        var header = mf.Header ?? "";
                        if (!normalFields.ContainsKey(header))
                            normalFields[header] = new List<(SerializedProperty, Component, int)>();
                        normalFields[header].Add((prop, comp, mf.Priority));
                    }
                }
            }

            if (synchronized.Count > 0)
            {
                EditorGUI.indentLevel++;
                foreach (var kvp in synchronized.OrderBy(k => k.Value.priority))
                {
                    var key = kvp.Key;
                    var prop = kvp.Value.prop;

                    EditorGUI.BeginChangeCheck();
                    DrawPropertyGeneric(prop);
                    if (EditorGUI.EndChangeCheck())
                    {
                        // Cache the changed value *before* updating others
                        var cachedValue = GetSerializedValue(prop);

                        foreach (var comp in syncedComponents[key])
                        {
                            var so = GetSo(comp);
                            var syncedProp = so.FindProperty(prop.name);
                            SetSerializedValue(syncedProp, cachedValue);
                            so.ApplyModifiedProperties();
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }

            foreach (var kvp in normalFields)
            {
                EditorGUI.indentLevel++;
                if (!string.IsNullOrEmpty(kvp.Key))
                    EditorGUILayout.LabelField(kvp.Key, EditorStyles.label);

                EditorGUI.indentLevel++;
                foreach (var (prop, comp, _) in kvp.Value.OrderBy(f => f.Item3))
                {
                    var so = GetSo(comp);
                    EditorGUILayout.PropertyField(so.FindProperty(prop.propertyPath), true);
                    so.ApplyModifiedProperties();
                }
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
        }


        // ---------------------------------------------------------------
        // Events
        // ---------------------------------------------------------------
        protected void DrawEventFields(string label = "Events")
        {
            var evtComp = GetModularEventComponents().FirstOrDefault();
            if (!evtComp) 
                return;

            evtComp.hideFlags = HideFlags.HideInInspector;
            var soEvents = new SerializedObject(evtComp);

            var parentSo = serializedObject;
            var delegatesProp = parentSo.FindProperty("mDelegates");
            if (delegatesProp == null)
            {
                return;
            }

            EditorGUILayout.LabelField(label, EditorStyles.label);
            EditorGUI.indentLevel++;

            int removeIndex = -1;

            for (int i = 0; i < delegatesProp.arraySize; i++)
            {
                var fieldName = delegatesProp.GetArrayElementAtIndex(i).stringValue;
                var propToDraw = soEvents.FindProperty(fieldName);
                if (propToDraw != null)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        DrawPropertyWithOptionalLabel(propToDraw);
                        if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(25)))
                            removeIndex = i;
                    }
                }
            }

            if (removeIndex >= 0)
                delegatesProp.DeleteArrayElementAtIndex(removeIndex);

            if (GUILayout.Button("Add New Event Type"))
            {
                GenericMenu menu = new GenericMenu();

                foreach (var field in evtComp.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (field.GetCustomAttribute<EventFieldAttribute>() == null) continue;

                    bool alreadyAdded = false;
                    for (int i = 0; i < delegatesProp.arraySize; i++)
                        if (delegatesProp.GetArrayElementAtIndex(i).stringValue == field.Name)
                            alreadyAdded = true;

                    if (alreadyAdded)
                        menu.AddDisabledItem(new GUIContent(ObjectNames.NicifyVariableName(field.Name)));
                    else
                        menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(field.Name)), false, () =>
                        {
                            delegatesProp.arraySize++;
                            delegatesProp.GetArrayElementAtIndex(delegatesProp.arraySize - 1).stringValue = field.Name;
                            parentSo.ApplyModifiedProperties();
                        });
                }

                menu.ShowAsContext();
            }

            EditorGUI.indentLevel--;
            parentSo.ApplyModifiedProperties();
            soEvents.ApplyModifiedProperties();
        }


        // ---------------------------------------------------------------
        // Utility
        // ---------------------------------------------------------------
        protected void DrawDefault(
            string settingsLink = null,
            string portalLink = null,
            string guideLink = null,
            string supportLink = null,
            SerializedProperty[] localFields = null)
        {
            // --- Header Links ---
            if (!string.IsNullOrEmpty(settingsLink) ||
                !string.IsNullOrEmpty(portalLink) ||
                !string.IsNullOrEmpty(guideLink) ||
                !string.IsNullOrEmpty(supportLink))
            {
                EditorGUILayout.BeginHorizontal();
                if (!string.IsNullOrEmpty(settingsLink) && EditorGUILayout.LinkButton("Settings"))
                    SettingsService.OpenProjectSettings(settingsLink);

                if (!string.IsNullOrEmpty(portalLink) && EditorGUILayout.LinkButton("Portal"))
                    Application.OpenURL(portalLink);

                if (!string.IsNullOrEmpty(guideLink) && EditorGUILayout.LinkButton("Guide"))
                    Application.OpenURL(guideLink);

                if (!string.IsNullOrEmpty(supportLink) && EditorGUILayout.LinkButton("Support"))
                    Application.OpenURL(supportLink);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            // --- Local Serialized Properties ---
            if (localFields != null && localFields.Length > 0)
            {
                foreach (var prop in localFields)
                {
                    if (prop == null) continue;
                    EditorGUILayout.PropertyField(prop, true);
                }
            }

            // --- Features Dropdown ---
            HideAllAllowedComponents();
            DrawAddFieldDropdown();

            // --- Draw existing components via attributes ---
            EditorGUI.indentLevel++;
            DrawModularComponents();
            EditorGUI.indentLevel--;

            // --- Draw Functions as Flags (single-instance components) ---
            DrawFunctionFlags();

            // --- Draw Settings / Elements / Templates / Events ---
            DrawFields<SettingsFieldAttribute>("Settings");
            DrawFields<ElementFieldAttribute>("Elements");
            DrawFields<TemplateFieldAttribute>("Templates");
            DrawEventFields();
        }

        private object GetSerializedValue(SerializedProperty prop)
        {
            return prop.propertyType switch
            {
                SerializedPropertyType.Boolean => prop.boolValue,
                SerializedPropertyType.Integer => prop.intValue,
                SerializedPropertyType.Float => prop.floatValue,
                SerializedPropertyType.String => prop.stringValue,
                SerializedPropertyType.Color => prop.colorValue,
                SerializedPropertyType.ObjectReference => prop.objectReferenceValue,
                SerializedPropertyType.Enum => prop.enumValueIndex,
                SerializedPropertyType.Vector2 => prop.vector2Value,
                SerializedPropertyType.Vector3 => prop.vector3Value,
                _ => null
            };
        }

        private static void SetSerializedValue(SerializedProperty targetProperty, object value)
        {
            switch (targetProperty.propertyType)
            {
                case SerializedPropertyType.Boolean: targetProperty.boolValue = (bool)value; break;
                case SerializedPropertyType.Integer: targetProperty.intValue = (int)value; break;
                case SerializedPropertyType.Float: targetProperty.floatValue = (float)value; break;
                case SerializedPropertyType.String: targetProperty.stringValue = (string)value; break;
                case SerializedPropertyType.Color: targetProperty.colorValue = (Color)value; break;
                case SerializedPropertyType.ObjectReference: targetProperty.objectReferenceValue = (UnityEngine.Object)value; break;
                case SerializedPropertyType.Enum: targetProperty.enumValueIndex = (int)value; break;
                case SerializedPropertyType.Vector2: targetProperty.vector2Value = (Vector2)value; break;
                case SerializedPropertyType.Vector3: targetProperty.vector3Value = (Vector3)value; break;
                default:
                    EditorUtility.CopySerializedIfDifferent(targetProperty.serializedObject.targetObject, targetProperty.serializedObject.targetObject);
                    break;
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected void HideAllAllowedComponents()
        {
            foreach (var type in AllowedTypes)
            {
                var comps = TargetGo.GetComponents(type);
                foreach (var comp in comps)
                {
                    if (comp != null)
                        comp.hideFlags = HideFlags.HideInInspector;
                }
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected IEnumerable<Component> GetModularComponents() =>
            TargetGo.GetComponents<Component>()
                .Where(c =>
                {
                    var attr = c.GetType().GetCustomAttribute<ModularComponentAttribute>();
                    return attr != null && attr.ParentType == ParentType;
                });

        // ReSharper disable Unity.PerformanceAnalysis
        protected IEnumerable<Component> GetModularEventComponents() =>
            TargetGo.GetComponents<Component>()
                .Where(c =>
                {
                    var attr = c.GetType().GetCustomAttribute<ModularEventsAttribute>();
                    return attr != null && attr.ParentType == ParentType;
                });

        // ReSharper disable Unity.PerformanceAnalysis
        protected void AddModularComponent(Type type)
        {
            if (type.GetCustomAttribute<ModularComponentAttribute>() == null) return;
            var comp = TargetGo.AddComponent(type);
            comp.hideFlags = HideFlags.HideInInspector;
        }

        protected (SerializedProperty prop, string header)[] GetPropertiesWithAttribute<TAttr>(SerializedObject so) where TAttr : PropertyAttribute
        {
            var list = new List<(SerializedProperty, string)>();
            var type = so.targetObject.GetType();

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<TAttr>() == null) continue;
                var prop = so.FindProperty(field.Name);
                if (prop != null)
                    list.Add((prop, ""));
            }

            return list.ToArray();
        }

        private static void DrawPropertyGeneric(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Boolean: prop.boolValue = EditorGUILayout.Toggle(prop.displayName, prop.boolValue); break;
                case SerializedPropertyType.Integer: prop.intValue = EditorGUILayout.IntField(prop.displayName, prop.intValue); break;
                case SerializedPropertyType.Float: prop.floatValue = EditorGUILayout.FloatField(prop.displayName, prop.floatValue); break;
                case SerializedPropertyType.String: prop.stringValue = EditorGUILayout.TextField(prop.displayName, prop.stringValue); break;
                default: EditorGUILayout.PropertyField(prop, true); break;
            }
        }

        private void CopySerializedValue(SerializedProperty source, SerializedProperty targetProperty)
        {
            switch (source.propertyType)
            {
                case SerializedPropertyType.Boolean: targetProperty.boolValue = source.boolValue; break;
                case SerializedPropertyType.Integer: targetProperty.intValue = source.intValue; break;
                case SerializedPropertyType.Float: targetProperty.floatValue = source.floatValue; break;
                case SerializedPropertyType.String: targetProperty.stringValue = source.stringValue; break;
                case SerializedPropertyType.Color: targetProperty.colorValue = source.colorValue; break;
                case SerializedPropertyType.ObjectReference: targetProperty.objectReferenceValue = source.objectReferenceValue; break;
                case SerializedPropertyType.Enum: targetProperty.enumValueIndex = source.enumValueIndex; break;
                case SerializedPropertyType.Vector2: targetProperty.vector2Value = source.vector2Value; break;
                case SerializedPropertyType.Vector3: targetProperty.vector3Value = source.vector3Value; break;
                default:
                    EditorUtility.CopySerializedIfDifferent(source.serializedObject.targetObject, targetProperty.serializedObject.targetObject);
                    break;
            }
        }
    }
}
#endif
#endif