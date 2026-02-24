using System;
using UnityEditor;
using UnityEngine;

public sealed class EnumNamedArrayAttribute : PropertyAttribute
{
    public readonly Type EnumType;
    public EnumNamedArrayAttribute(Type enumType) => EnumType = enumType;
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnumNamedArrayAttribute))]
public sealed class EnumNamedArrayDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        var enumType = ((EnumNamedArrayAttribute)attribute).EnumType;
        var names = Enum.GetNames(enumType);

        if (property.propertyPath.EndsWith(']'))
        {
            // Extract index from path e.g. "array.data[2]"
            int start = property.propertyPath.LastIndexOf('[') + 1;
            int end = property.propertyPath.LastIndexOf(']');
            int index = int.Parse(property.propertyPath.Substring(start, end - start));

            string labelName = index < names.Length ? names[index] : $"Unknown [{index}]";
            EditorGUI.PropertyField(rect, property, new GUIContent(labelName), true);
        }
        else
        {
            EditorGUI.PropertyField(rect, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property, label, true);
}
#endif