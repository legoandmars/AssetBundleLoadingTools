using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using IPA.Utilities;

namespace AssetBundleLoadingTools.Core
{
    internal static class Sanitization
    {
        //private static readonly List<Type> _dangerousTypes = new() { FloatSignalListener, SignalListener };
        private static Dictionary<Type, bool> _dangerousTypeMap = new();

        private static readonly BindingFlags _bindings = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private static FieldInfo _persistentCallGroupFieldInfo;
        private static FieldInfo _persistentCallFieldInfo;
        private static FieldInfo _itemsFieldInfo;
        private static FieldInfo _targetAssemblyTypeNameFieldInfo;

        static Sanitization()
        {
            _persistentCallGroupFieldInfo = typeof(UnityEventBase).GetField("m_PersistentCalls", _bindings);
            _persistentCallFieldInfo = _persistentCallGroupFieldInfo.FieldType.GetField("m_Calls", _bindings);
            _itemsFieldInfo = _persistentCallFieldInfo.FieldType.GetField("_items", _bindings);
            _targetAssemblyTypeNameFieldInfo = typeof(UnityEventBase).Assembly.GetType("UnityEngine.Events.PersistentCall").GetField("m_TargetAssemblyTypeName", _bindings);
        }

        // Ideally this should just throw when a "dangerous" object is found
        // Unfortunately in unity 2020+ ALL events use the dangerous m_TargetAssemblyTypeName field 
        // If the field is removed, it will go back to old behaviour, which doesn't allow you to target any static void method with one parameter
        // So it should be safe to just remove that specific field
        internal static bool SanitizeObject(GameObject gameObject)
        {
            bool dangerous = false;
            var components = gameObject.GetComponentsInChildren<Component>(true);

            // This is quite bad, but it will get faster once all the basic components are filtered through and cached as not dangerous
            // Ideally somebody just needs to write up a pre-baked list of default unity components that don't touch UnityEvents ever 
            foreach (var component in components)
            {
                if (component == null) continue;
                var componentType = component.GetType();
                if (_dangerousTypeMap.TryGetValue(componentType, out bool existingComponentIsDangerous)){
                    if (!existingComponentIsDangerous) continue;
                }
                else
                {
                    bool componentIsDangerous = ComponentIsDangerous(componentType);
                    _dangerousTypeMap.Add(componentType, componentIsDangerous);

                    if (!componentIsDangerous) continue;
                }

                // the component is dangerous.
                dangerous = true;
                StripDangerousEventFields(component, componentType);
            }

            return dangerous;
        }

        // this doesn't actually check if the component is *dangerous* and contains the m_TargetAssemblyTypeName field
        // BundleCache would lessen the resource load significantly if it did
        // However, I do not want to implement this right now, because I am tired! This should still help rule out like 90% of models probably
        private static bool ComponentIsDangerous(Type componentType)
        {
            var fields = componentType.GetFields(_bindings);

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(UnityEventBase) || field.FieldType.IsSubclassOf(typeof(UnityEventBase)))
                {
                    return true;
                }
            }

            return false;
        }

        // This is fairly heavy to run so we need to cache it
        // There is almost certainly a better way to do this (better component whitelist/blacklist)
        // but because all the model mods have separate event systems, there's a high risk of a UnityEvent going undetected
        private static void StripDangerousEventFields(Component component, Type componentType)
        {
            var fields = componentType.GetFields(_bindings);

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(UnityEventBase) || field.FieldType.IsSubclassOf(typeof(UnityEventBase)))
                {
                    // TODO: Convert to FieldAccessors(?)
                    var unityEvent = field.GetValue(component);
                    var persistentCall = _persistentCallGroupFieldInfo.GetValue(unityEvent);
                    var calls = _persistentCallFieldInfo.GetValue(persistentCall);
                    object[] items = (object[])_itemsFieldInfo.GetValue(calls);

                    foreach (var item in items)
                    {
                        _targetAssemblyTypeNameFieldInfo.SetValue(item, null);
                    }
                }
            }
        }


    }
}
