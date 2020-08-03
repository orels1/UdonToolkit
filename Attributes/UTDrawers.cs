#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UdonToolkit.Editor {
  [CustomPropertyDrawer(typeof(ModifiablePropertyAttribute), true)]
  public class ModifiablePropertyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      var modifiable = (ModifiablePropertyAttribute) attribute;
      if (modifiable.modifiers == null) {
        modifiable.modifiers = fieldInfo.GetCustomAttributes(typeof(PropertyModifierAttribute), false)
          .Cast<PropertyModifierAttribute>().OrderBy(s => s.order).ToList();
      }

      float height = ((ModifiablePropertyAttribute) attribute).GetPropertyHeight(property, label);
      foreach (var attr in modifiable.modifiers) {
        height = attr.GetHeight(property, label, height);
      }
      return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      var modifiable = (ModifiablePropertyAttribute) attribute;
      var visible = true;
      if (modifiable.modifiers.AsEnumerable() == null) {
        modifiable.OnGUI(position, property, label);
        return;
      }
      foreach (var attr in modifiable.modifiers.AsEnumerable().Reverse()) {
        visible = attr.BeforeGUI(ref position, property, label, visible);
      }

      if (visible) {
        modifiable.OnGUI(position, property, label);
      }

      foreach (var attr in modifiable.modifiers) {
        attr.AfterGUI(position, property, label);
      }
    }
  }
}
#endif