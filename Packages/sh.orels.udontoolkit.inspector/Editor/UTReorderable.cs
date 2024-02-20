using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UdonToolkit {
  // based on https://gist.github.com/t0chas/34afd1e4c9bc28649311
  // come back to this later
  public class UTReorderable : UnityEditor.Editor {
    private Dictionary<string, ReorderableListProperty> reorderableLists;

    private void OnEnable() {
      reorderableLists = new Dictionary<string, ReorderableListProperty>(10);
    }

    public override void OnInspectorGUI() {
      // var listData = GetReorderableList(prop, otherProp);
      // listData.IsExpanded = prop.isExpanded;
      // if (!listData.IsExpanded) {
      //   EditorGUILayout.BeginHorizontal();
      //   prop.isExpanded = EditorGUILayout.ToggleLeft(groupAttribute[0].name, prop.isExpanded, EditorStyles.boldLabel);
      //   EditorGUILayout.LabelField($"size: {prop.arraySize}");
      //   EditorGUILayout.EndHorizontal();
      // }
      // else {
      //   listData.List.DoLayoutList();
      // }
    }

    private ReorderableListProperty GetReorderableList(SerializedProperty prop) {
      ReorderableListProperty ret = null;
      if (reorderableLists.TryGetValue(prop.name, out ret)) {
        ret.Property = prop;
        return ret;
      }

      ret = new ReorderableListProperty(prop);
      reorderableLists.Add(prop.name, ret);
      return ret;
    }
    
    private ReorderableListProperty GetReorderableList(SerializedProperty prop, SerializedProperty otherProp) {
      ReorderableListProperty ret = null;
      if (reorderableLists.TryGetValue(prop.name, out ret)) {
        ret.Property = prop;
        return ret;
      }

      ret = new ReorderableListProperty(prop, otherProp);
      reorderableLists.Add(prop.name, ret);
      return ret;
    }

    private class ReorderableListProperty {
      public bool IsExpanded { get; set; }
      public ReorderableList List { get; private set; }

      private SerializedProperty prop;
      private SerializedProperty otherProp;
      private bool doubleList;

      public SerializedProperty Property {
        get => prop;
        set {
          prop = value;
          List.serializedProperty = prop;
        }
      }

      public ReorderableListProperty(SerializedProperty property) {
        IsExpanded = property.isExpanded;
        prop = property;
        CreateList();
      }

      public ReorderableListProperty() {
        prop = null;
        List = null;
      }

      public ReorderableListProperty(SerializedProperty property, SerializedProperty otherProperty) {
        IsExpanded = property.isExpanded;
        prop = property;
        otherProp = otherProperty;
        doubleList = true;
        CreateList();
      }

      private void CreateList() {
        List = new ReorderableList(Property.serializedObject, Property, true, true, true, true);
        List.drawHeaderCallback += rect =>
          prop.isExpanded = EditorGUI.ToggleLeft(rect, prop.displayName, prop.isExpanded, EditorStyles.boldLabel);
        List.onCanRemoveCallback += list => List.count > 0;
        List.drawElementCallback += DrawElement;
        List.elementHeightCallback += idx => Mathf.Max(EditorGUIUtility.singleLineHeight,
          EditorGUI.GetPropertyHeight(prop.GetArrayElementAtIndex(idx),
            GUIContent.none, true)) + 4f;
      }
      
      private void DrawElement(Rect rect, int index, bool active, bool focused) {
        if (prop.GetArrayElementAtIndex(index).propertyType == SerializedPropertyType.Generic) {
          EditorGUI.LabelField(rect, prop.GetArrayElementAtIndex(index).displayName);
        }

        rect.height = EditorGUI.GetPropertyHeight(prop.GetArrayElementAtIndex(index), GUIContent.none, true);
        rect.y += 1;
        if (!doubleList) {
          EditorGUI.PropertyField(rect, prop.GetArrayElementAtIndex(index), GUIContent.none, true);
        }
        else {
          var secondRect = EditorGUI.IndentedRect(rect);
          secondRect.xMin = rect.xMax / 2f + 2f;
          rect.xMax = rect.xMax / 2f - 2f;
          EditorGUI.PropertyField(rect, prop.GetArrayElementAtIndex(index), GUIContent.none, true);
          EditorGUI.PropertyField(secondRect, otherProp.GetArrayElementAtIndex(index), GUIContent.none, true);
        }
        List.elementHeight = rect.height + 4f;
      }
    }
  }
}