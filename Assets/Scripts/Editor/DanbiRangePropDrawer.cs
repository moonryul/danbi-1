using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomPropertyDrawer(typeof(DanbiAutomationItem))]
public class DanbiRangePropDrawer : PropertyDrawer {
  public override void OnGUI(Rect position,
                             SerializedProperty property,
                             GUIContent label) {
    //base.OnGUI(position, property, label);
    using (new EditorGUI.PropertyScope(position, label, property)) {
      // Retrieve each properties.
      var minDistanceProperty = property.FindPropertyRelative("minDistance");
      var maxDistanceProperty = property.FindPropertyRelative("maxDistance");

      // Adjust the displaying values
      var minMaxSliderRect = new Rect(position) { height = position.height * 0.5f };
      var labelRect = new Rect(minMaxSliderRect) {
        x = minMaxSliderRect.x + EditorGUIUtility.labelWidth,
        y = minMaxSliderRect.y + minMaxSliderRect.height
      };

      float minDistanceValue = minDistanceProperty.floatValue;
      float maxDistanceValue = maxDistanceProperty.floatValue;

      EditorGUI.BeginChangeCheck();
      EditorGUI.MinMaxSlider(minMaxSliderRect, label, ref minDistanceValue, ref maxDistanceValue, 0.0f, 1.0f);
      EditorGUI.LabelField(labelRect, minDistanceValue.ToString(), maxDistanceValue.ToString());

      if (EditorGUI.EndChangeCheck()) {
        minDistanceProperty.floatValue = minDistanceValue;
        maxDistanceProperty.floatValue = maxDistanceValue;
      }
    }
  }
  /// <summary>
  /// Height of the GUI element.
  /// </summary>
  /// <param name="property"></param>
  /// <param name="label"></param>
  /// <returns></returns>
  public override float GetPropertyHeight(SerializedProperty property,
                                          GUIContent label) {
    return base.GetPropertyHeight(property, label) * 2.0f;
  }
};
#endif