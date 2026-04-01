using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Dialog))]
public class DialogDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var nameProp = property.FindPropertyRelative("name");
        var lineProp = property.FindPropertyRelative("line");
        var colorProp = property.FindPropertyRelative("textColor");
        var selectableProp = property.FindPropertyRelative("isSelectable");
        var selectionsProp = property.FindPropertyRelative("selections");

        float y = position.y;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), nameProp);
        y += lineHeight + spacing;

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), lineProp);
        y += lineHeight + spacing;

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), colorProp);
        y += lineHeight + spacing;

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), selectableProp);
        if (EditorGUI.EndChangeCheck())
        {
            if (!selectableProp.boolValue)
            {
                selectionsProp.arraySize = 0;
            }
        }
        y += lineHeight + spacing;

        if (selectableProp.boolValue)
        {
            EditorGUI.PropertyField(
                new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(selectionsProp, true)),
                selectionsProp,
                true
            );
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0f;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        var selectableProp = property.FindPropertyRelative("isSelectable");
        var selectionsProp = property.FindPropertyRelative("selections");

        height += (lineHeight + spacing) * 4;

        if (selectableProp.boolValue)
        {
            height += EditorGUI.GetPropertyHeight(selectionsProp, true) + spacing;
        }

        return height;
    }
}
