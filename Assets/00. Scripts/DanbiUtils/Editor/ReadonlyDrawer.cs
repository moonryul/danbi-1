using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(ReadonlyAttribute))]
public class ReadonlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // caching prev GUI enabled value.
        bool bPrevGUIState = GUI.enabled;
        // Disable edit for property.
        GUI.enabled = false;
        // Drawing property
        EditorGUI.PropertyField(position, property, label);
        // Setting back with previous GUI state.
        GUI.enabled = bPrevGUIState;
    }
};
#endif