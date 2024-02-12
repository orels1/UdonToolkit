using System.Collections.Generic;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace UdonToolkit {
  public class UTEditorArray {
    public static bool RenderPositionControls(int index, List<SerializedProperty> props, out int newIndex) {
      newIndex = EditorGUILayout.IntField(index, new GUIStyle(EditorStyles.numberField) {
        alignment = TextAnchor.MiddleCenter,
        fixedWidth = 30
      }, GUILayout.MaxWidth(30));
      if (newIndex != index) {
        if (newIndex < 0) return false;
        if (newIndex >= props[0].arraySize) return true;
        foreach (var prop in props) {
          prop.MoveArrayElement(index, newIndex);
        }

        return true;
      }

      if (GUILayout.Button("▲", new GUIStyle(EditorStyles.miniButtonLeft) {
        alignment = TextAnchor.MiddleRight
      }, GUILayout.MaxHeight(14), GUILayout.MaxWidth(20))) {
        GUI.FocusControl(null);
        if (index == 0) return false;
        foreach (var prop in props) {
          prop.MoveArrayElement(index, index - 1);
        }

        return true;
      }
      
      if (GUILayout.Button("▼", EditorStyles.miniButtonRight, GUILayout.MaxHeight(14), GUILayout.MaxWidth(20))) {
        GUI.FocusControl(null);
        if (index == props[0].arraySize - 1) return false;
        foreach (var prop in props) {
          prop.MoveArrayElement(index, index + 1);
        }

        return true;
      }

      return false;
    }

    public static bool RenderRemoveControls(int index, List<SerializedProperty> props) {
      if (GUILayout.Button("-", GUILayout.MaxHeight(13), GUILayout.MaxWidth(15))) {
        GUI.FocusControl(null);
        // there is probably a better way to do this
        foreach (var prop in props) {
          var currLength = prop.arraySize;
          prop.DeleteArrayElementAtIndex(index);
          if (currLength == prop.arraySize) {
            prop.DeleteArrayElementAtIndex(index);
          }
        }

        return true;
      }

      return false;
    }
    
    public static bool HandleDragAndDrop(Rect position, SerializedObject obj, List<SerializedProperty> props) {
      var droppedObjects = false;
      if (!position.Contains(Event.current.mousePosition)) return false;
      if (Event.current.type == EventType.DragUpdated) {
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        Event.current.Use();
      }
      else if (Event.current.type == EventType.DragPerform) {
        DragAndDrop.AcceptDrag();
        foreach (var prop in props) {
          var targetType = obj.targetObject.GetType().GetField(prop.name).FieldType.GetElementType();
          if (targetType == null) {
            break;
          }
          var addingGo = targetType == typeof(GameObject);
          
          foreach (var draggedObject in DragAndDrop.objectReferences) {
            if (!(draggedObject is GameObject addedGameObject)) continue;
            var addIndex = prop.arraySize;
            // Because GameObject is not a component, we skip the GetComponent part
            if (addingGo) {
              prop.InsertArrayElementAtIndex(addIndex);
              prop.GetArrayElementAtIndex(addIndex).objectReferenceValue = addedGameObject;
              droppedObjects = true;
              continue;
            }
            
            if (!targetType.IsSubclassOf(typeof(Component)) && !targetType.IsSubclassOf(typeof(MonoBehaviour))) {
              prop.InsertArrayElementAtIndex(addIndex);
              droppedObjects = true;
              continue;
            }
            
            var addedItem = addedGameObject.GetComponent(targetType);
            if (!addedItem) continue;
            prop.InsertArrayElementAtIndex(addIndex);
            prop.GetArrayElementAtIndex(addIndex).objectReferenceValue = addedItem;
            droppedObjects = true;
          }
        }
      }

      return droppedObjects;
    }
  }
}
