#if !DISABLESTEAMWORKS && STEAM_INSTALLED
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public interface IModularField
    {
        int Priority { get; }
        bool Synchronised { get; }
        string Header { get; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class SettingsFieldAttribute : PropertyAttribute, IModularField
    {
        public int Priority { get; }
        public bool Synchronised { get; }
        public string Header { get; }

        public SettingsFieldAttribute(int priority = 0, bool synchronised = false, string header = null)
        {
            Priority = priority;
            Synchronised = synchronised;
            Header = header;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ElementFieldAttribute : PropertyAttribute, IModularField
    {
        public string Header { get; }
        public int Priority { get; }
        public bool Synchronised => false;

        public ElementFieldAttribute(string header = null, int priority = 0)
        {
            Header = header;
            Priority = priority;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class TemplateFieldAttribute : PropertyAttribute, IModularField
    {
        public string Header { get; }
        public int Priority { get; }
        public bool Synchronised => false;

        public TemplateFieldAttribute(string header = null, int priority = 0)
        {
            Header = header;
            Priority = priority;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EventFieldAttribute : PropertyAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModularComponentAttribute : Attribute
    {
        public Type ParentType { get; }
        public string Header { get; }
        public string FieldName { get; }

        public ModularComponentAttribute(Type type, string header, string field)
        {
            ParentType = type;
            Header = header;
            FieldName = field;
        }
    }

    // Marks a component as an Events container
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ModularEventsAttribute : Attribute
    {
        public Type ParentType { get; }

        public ModularEventsAttribute(Type type)
        {
            ParentType = type;
        }
    }
}
#endif