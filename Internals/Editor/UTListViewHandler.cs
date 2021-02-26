using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace UdonToolkit {
  public partial class UTEditor {

    #region ListView Utils
    private void CreateListViewUtils(List<SerializedProperty> propsList, KeyValuePair<string, UTFieldType> fieldEntry) {
      EditorGUILayout.BeginHorizontal("helpBox");
      if (GUILayout.Button("Clear All")) {
        ClearAllElements(propsList, fieldEntry);
        return;
      }

      if (GUILayout.Button("Clear All Empty")) {
        ClearAllEmptyElements(propsList, fieldEntry);
        return;
      }

      EditorGUILayout.EndHorizontal();
    }
    
    private void ClearAllElements(List<SerializedProperty> propsList, KeyValuePair<string, UTFieldType> fieldEntry) {
      foreach (var prop in propsList) {
        prop.ClearArray();
      }
      utilsShown[fieldEntry.Key] = false;
      droppedObjects = true;
    }

    private void ClearAllEmptyElements(List<SerializedProperty> propsList, KeyValuePair<string, UTFieldType> fieldEntry) {
      var clearedItems = new List<int>();
      var objRefs = new List<SerializedProperty>();
      foreach (var prop in propsList) {
        var elType = tT.GetField(prop.name).FieldType.GetElementType();
        if (elType == typeof(GameObject) || elType.IsSubclassOf(typeof(Component)) ||
            elType.IsSubclassOf(typeof(MonoBehaviour))) {
          objRefs.Add(prop);
        }
      }
      
      for (int i = objRefs[0].arraySize - 1; i >= 0; i--) {
        var elFound = false;
        foreach (var oRef in objRefs) {
          if (elFound) continue;
          if (oRef.GetArrayElementAtIndex(i).objectReferenceValue == null) {
            clearedItems.Add(i);
            elFound = true;
          }
        }
      }

      foreach (var clearedItem in clearedItems) {
        foreach (var prop in propsList) {
          prop.DeleteArrayElementAtIndex(clearedItem);
        }
      }

      utilsShown[fieldEntry.Key] = false;
      droppedObjects = true;
    }
    
    #endregion

    private string GetListViewHeaderText(string fieldName, bool disabled, bool isExpanded, int page, int pageCount, int arraySize) {
      var finalString = fieldName;
      if (pageCount > 1 && isExpanded) {
        var pageEnd = Mathf.Min(arraySize, 30 * (page + 1)) - 1;
        finalString += $" ({30 * page} - {pageEnd})";
      }

      finalString += $" [{arraySize}] ";
      
      if (disabled) {
        finalString = "[Read Only] " + finalString;
      }

      return finalString;
    }
    
    private void HandleListView(List<UTField> listViewFields, KeyValuePair<string, UTFieldType> fieldEntry) {
      List<SerializedProperty> propsList = new List<SerializedProperty>();
      foreach (var field in listViewFields) {
        propsList.Add(serializedObject.FindProperty(field.name));
      }

      // draw header
      var parentProp = serializedObject.FindProperty(listViewFields[0].name);
      var uiAttrs = listViewFields[0].uiAttrs;

      var isVisible = true;
      foreach (var uiAttr in uiAttrs) {
        if (!uiAttr.GetVisible(parentProp)) {
          isVisible = false;
          break;
        }
      }

      if (!isVisible) {
        return;
      }

      foreach (var uiAttr in uiAttrs) {
        uiAttr.BeforeGUI(propsList[0]);
      }

      var propDisabled = listViewFields.Exists(i => i.isDisabled);
      var page = listPaginations[fieldEntry.Key];
      var pageCount = Mathf.CeilToInt(parentProp.arraySize / 30f);
      if (pageCount > 1 && page > pageCount - 1) {
        page = pageCount - 1;
        listPaginations[fieldEntry.Key] = page;
      }

      
      var utilsShownVal = utilsShown[fieldEntry.Key];
      parentProp.isExpanded =
        UTStyles.FoldoutHeader(
          GetListViewHeaderText(fieldEntry.Key, propDisabled, parentProp.isExpanded, page, pageCount,
            parentProp.arraySize),
          parentProp.isExpanded, ref utilsShownVal);
      utilsShown[fieldEntry.Key] = utilsShownVal;
      var foldoutRect = GUILayoutUtility.GetLastRect();
      if (!propDisabled && UTEditorArray.HandleDragAndDrop(foldoutRect, serializedObject, propsList)) {
        droppedObjects = true;
        if (listViewFields[0].onValueChaged == null) {
          return;
        }

        HandleFieldChangeArray(listViewFields[0], parentProp, parentProp.arraySize - 1);
        return;
      }

      if (droppedObjects) return;
      if (!parentProp.isExpanded) return;
      EditorGUI.BeginDisabledGroup(propDisabled);
      // List View Utils
      if (utilsShownVal) {
        CreateListViewUtils(propsList, fieldEntry);
      }

      for (int i = 30 * page; i < Mathf.Min(parentProp.arraySize, 30 * (page + 1)); i++) {
        Rect headerRect = default;
        Rect[] fieldRects = new Rect[listViewFields.Count];
        // get a react to draw a header later
        if (listViewFields.Count > 1 && i == 30 * page) {
          headerRect = EditorGUILayout.GetControlRect();
        }

        EditorGUILayout.BeginHorizontal();

        // position controls
        if (UTEditorArray.RenderPositionControls(i, propsList, out var newIndex)) {
          HandleFieldChangeArray(listViewFields[0], parentProp.GetArrayElementAtIndex(newIndex), newIndex);
          break;
        }

        var fieldIndex = 0;
        foreach (var field in listViewFields) {
          var prop = serializedObject.FindProperty(field.name);

          if (prop.arraySize != parentProp.arraySize) {
            prop.arraySize = parentProp.arraySize;
          }

          DrawFieldElement(field, prop.GetArrayElementAtIndex(i));
          // saved the field width to reuse in the header later
          if (listViewFields.Count > 1 && i == 30 * page) {
            fieldRects[fieldIndex] = GUILayoutUtility.GetLastRect();
          }

          fieldIndex++;
        }

        // removal controls
        if (UTEditorArray.RenderRemoveControls(i, propsList)) {
          if (listViewFields[0].onValueChaged != null) {
            HandleFieldChangeArray(listViewFields[0], null, i);
          }

          break;
        }

        EditorGUILayout.EndHorizontal();
        // draw a header with saved field widths
        // we only draw a header for cases where there are multiple elements
        if (listViewFields.Count > 1 && i == 30 * page) {
          GUI.Box(headerRect, "", new GUIStyle("helpBox"));
          for (int j = 0; j < listViewFields.Count; j++) {
            var adjustedRect = fieldRects[j];
            if (j == 0) {
              adjustedRect.xMin = headerRect.xMin + 2;
            }

            adjustedRect.yMin = headerRect.yMin;
            adjustedRect.yMax = headerRect.yMax;
            EditorGUI.LabelField(adjustedRect,
              listViewFields[j].listViewColumnName ??
              serializedObject.FindProperty(listViewFields[j].name).displayName);
          }
        }
      }

      EditorGUI.EndDisabledGroup();

      if (pageCount > 1) {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(page != pageCount - 1);
      }

      if (GUILayout.Button(listViewFields[0].listViewAddTitle ?? "Add Element")) {
        var insertAt = parentProp.arraySize;
        if (listViewFields[0].listViewAddMethod != null) {
          listViewFields[0].listViewAddMethod.Invoke(t, new object[] {serializedObject});
        }
        else {
          foreach (var field in listViewFields) {
            var prop = serializedObject.FindProperty(field.name);
            prop.InsertArrayElementAtIndex(insertAt);
          }
        }

        if (listViewFields[0].onValueChaged != null) {
          HandleFieldChangeArray(listViewFields[0], parentProp.GetArrayElementAtIndex(insertAt), insertAt);
        }
      }

      if (pageCount > 1) {
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(page <= 0);
        if (GUILayout.Button(UTStyles.ArrowL, UTStyles.MiniArrowLeft)) {
          listPaginations[fieldEntry.Key]--;
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUI.BeginDisabledGroup(page >= pageCount - 1);
        if (GUILayout.Button(UTStyles.ArrowR, UTStyles.MiniArrowRight)) {
          listPaginations[fieldEntry.Key]++;
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
      }

      foreach (var uiAttr in uiAttrs) {
        uiAttr.AfterGUI(parentProp);
      }
    }
  }
}
