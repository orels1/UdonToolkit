#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UdonToolkit.Demo {
  public class MyAttribute : UTPropertyAttribute {
    public override void BeforeGUI(SerializedProperty property) {
      EditorGUILayout.LabelField("HEADER TEXT", EditorStyles.largeLabel);
      GUI.color = new Color(0.98f, 0.92f, 0.35f);
    }

    public override void AfterGUI(SerializedProperty property) {
      GUI.color = Color.white;
      if (GUILayout.Button("Reset Value")) {
        property.floatValue = 0;
      }
    }
  }
}
#endif
