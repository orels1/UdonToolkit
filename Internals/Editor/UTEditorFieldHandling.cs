using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UdonToolkit {
  public partial class UTEditor {
    private void HandleFields(Dictionary<string, UTFieldType> fields) {
      foreach (var fieldEntry in fields) {
        if (droppedObjects) {
          break;
        }
        switch (fieldEntry.Value) {
          case UTFieldType.Regular: {
            var prop = serializedObject.FindProperty(fieldEntry.Key);
            DrawField(fieldCache[fieldEntry.Key], prop);
            break;
          }
          case UTFieldType.Tab: {
            var tabNames = tabs.Select(i => i.Key).ToArray();
            var tabSaveTarget = fieldCache[tabs[tabNames[0]].Keys.ToArray()[0]].tabSaveTarget;
            if (!String.IsNullOrEmpty(tabSaveTarget)) {
              var targetProp = serializedObject.FindProperty(tabSaveTarget);
              tabOpen = targetProp.intValue;
              EditorGUI.BeginChangeCheck();
              tabOpen = GUILayout.Toolbar(tabOpen, tabNames);
              if (EditorGUI.EndChangeCheck()) {
                targetProp.intValue = tabOpen;
              } 
            }
            else {
              tabOpen = GUILayout.Toolbar(tabOpen, tabNames);
            }
            HandleFields(tabs[tabNames[tabOpen]]);
            break;
          }
          case UTFieldType.Foldout: {
            var foldout = foldouts[fieldEntry.Key].First();
            UTField field;
            switch (foldout.Value) {
              case UTFieldType.Horizontal: {
                field = horizontalViews[foldout.Key].First();
                break;
              }
              case UTFieldType.ListView: {
                field = listViews[foldout.Key].First();
                break;
              }
              default: {
                field = fieldCache[foldout.Key];
                break;
              }
            }
            var parentProp = serializedObject.FindProperty(field.name);
            var foldoutName = field.foldoutName;
            parentProp.isExpanded = UTEditorStyles.FoldoutHeader(foldoutName, parentProp.isExpanded);
            if (!parentProp.isExpanded) break;
            EditorGUILayout.BeginVertical(new GUIStyle("helpBox"));
            HandleFields(foldouts[fieldEntry.Key]);
            EditorGUILayout.EndVertical();
            break;
          }
          case UTFieldType.ListView: {
            var listViewFields = listViews[fieldEntry.Key];
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
              break;
            }
            
            foreach (var uiAttr in uiAttrs) {
              uiAttr.BeforeGUI(propsList[0]);
            }
            
            var propDisabled = listViewFields.Exists(i => i.isDisabled);
            var disabledString = propDisabled ? "[Read Only]" : "";
            var page = listPaginations[fieldEntry.Key];
            var pageCount = Mathf.CeilToInt(parentProp.arraySize / 30f);
            if (pageCount > 1 && page > pageCount - 1) {
              page = pageCount - 1;
              listPaginations[fieldEntry.Key] = page;
            }
            var arrSizeString = pageCount > 1 && parentProp.isExpanded
              ? $"({30 * page} - {Math.Min(parentProp.arraySize, 30 * (page + 1)) - 1})"
              : "";
            var utilsShownVal = utilsShown[fieldEntry.Key];
            parentProp.isExpanded =
              UTEditorStyles.FoldoutHeader($"{fieldEntry.Key} [{parentProp.arraySize}] {arrSizeString} {disabledString}",
                parentProp.isExpanded, ref utilsShownVal);
            utilsShown[fieldEntry.Key] = utilsShownVal;
            var foldoutRect = GUILayoutUtility.GetLastRect();
            if (!propDisabled && UTEditorArray.HandleDragAndDrop(foldoutRect, serializedObject, propsList)) {
              droppedObjects = true;
              if (listViewFields[0].onValueChaged == null) {
                break;
              }
              HandleFieldChangeArray(listViewFields[0], parentProp, parentProp.arraySize - 1);
              break;
            }

            if (droppedObjects) break;
            if (!parentProp.isExpanded) break;
            EditorGUI.BeginDisabledGroup(propDisabled);
            if (utilsShownVal) {
              EditorGUILayout.BeginHorizontal("helpBox");
              if (GUILayout.Button("Clear All")) {
                foreach (var prop in propsList) {
                  prop.ClearArray();
                }

                utilsShown[fieldEntry.Key] = false;

                droppedObjects = true;
                break;
              }
              if (GUILayout.Button("Clear All Empty")) {
                var clearedItems = new List<int>();
                var objRefs = new List<SerializedProperty>();
                foreach (var prop in propsList) {
                  var elType = tT.GetField(prop.name).FieldType.GetElementType();
                  if (elType == typeof(GameObject) || elType.IsSubclassOf(typeof(Component)) ||
                      elType.IsSubclassOf(typeof(MonoBehaviour))) {
                    objRefs.Add(prop);
                  }
                }

                var elFound = false;
                for (int i = objRefs[0].arraySize - 1; i >= 0; i--) {
                  elFound = false;
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
                break;
              }
              EditorGUILayout.EndHorizontal();
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
                  EditorGUI.LabelField(adjustedRect, listViewFields[j].listViewColumnName ?? serializedObject.FindProperty(listViewFields[j].name).displayName);
                }
              }
            }

            EditorGUI.EndDisabledGroup();

            if (pageCount > 1) {
              EditorGUILayout.BeginHorizontal();
              EditorGUI.BeginDisabledGroup(page != pageCount - 1);
            }
            if (GUILayout.Button("Add Element")) {
              var insertAt = parentProp.arraySize;
              foreach (var field in listViewFields) {
                var prop = serializedObject.FindProperty(field.name);
                prop.InsertArrayElementAtIndex(insertAt);
              }

              if (listViewFields[0].onValueChaged != null) {
                HandleFieldChangeArray(listViewFields[0], parentProp.GetArrayElementAtIndex(insertAt), insertAt);
              }
            }

            if (pageCount > 1) {
              EditorGUI.EndDisabledGroup(); 
              EditorGUI.BeginDisabledGroup(page <= 0);
              if (GUILayout.Button(UTStyles.ArrowL, new GUIStyle(EditorStyles.miniButtonLeft) {
                margin = new RectOffset(0, 0, 3, 3),
                fixedWidth = 20,
                fixedHeight = 17
              })) {
                listPaginations[fieldEntry.Key]--;
              }
              EditorGUI.EndDisabledGroup();
              EditorGUI.BeginDisabledGroup(page >= pageCount - 1);
              if (GUILayout.Button(UTStyles.ArrowR, new GUIStyle(EditorStyles.miniButtonRight) {
                margin = new RectOffset(0, 0, 3, 3),
                fixedWidth = 20,
                fixedHeight = 17
              })) {
                listPaginations[fieldEntry.Key]++;
              }
              EditorGUI.EndDisabledGroup();
              EditorGUILayout.EndHorizontal();
            }

            foreach (var uiAttr in uiAttrs) {
              uiAttr.AfterGUI(parentProp);
            }

            break;
          }
          case UTFieldType.Horizontal: {
            if (droppedObjects) break;
            var horizontalFields = horizontalViews[fieldEntry.Key];
            List<SerializedProperty> propsList = new List<SerializedProperty>();
            foreach (var field in horizontalFields) {
              propsList.Add(serializedObject.FindProperty(field.name));
            }

            var uiAttrs = horizontalFields[0].uiAttrs;
            
            var isVisible = true;
            foreach (var uiAttr in uiAttrs) {
              if (!uiAttr.GetVisible(propsList[0])) {
                isVisible = false;
                break;
              }
            }

            if (!isVisible) {
              break;
            }
            
            foreach (var uiAttr in uiAttrs) {
              uiAttr.BeforeGUI(propsList[0]);
            }

            var horizontalAttr =
              horizontalFields[0].attributes.Find(i => i is HorizontalAttribute) as HorizontalAttribute;
            if (horizontalAttr.showHeader) {
              // header frame and label for the group
              var oldColor = GUI.backgroundColor;
              var headerRect = EditorGUILayout.GetControlRect();
              GUI.backgroundColor = new Color(oldColor.r, oldColor.g, oldColor.b, 0.5f);
              GUI.Box(headerRect, "", new GUIStyle("helpBox"));
              GUI.backgroundColor = oldColor;
              EditorGUI.LabelField(headerRect, fieldEntry.Key);
            }

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < propsList.Count; i++) {
              DrawFieldElement(horizontalFields[i], propsList[i]);
            }

            EditorGUILayout.EndHorizontal();

            foreach (var uiAttr in uiAttrs) {
              uiAttr.AfterGUI(propsList[0]);
            }
            
            break;
          }
        }
      }
    }
    
    private void DrawField(UTField field, SerializedProperty prop) {
      var uiAttrs = field.uiAttrs;
      if (!uiAttrs.Any()) {
        EditorGUI.BeginDisabledGroup(field.isDisabled);
        EditorGUILayout.PropertyField(prop, new GUIContent(prop.displayName));
        EditorGUI.EndDisabledGroup();
        return;
      }

      var isVisible = true;
      foreach (var uiAttr in uiAttrs) {
        if (!uiAttr.GetVisible(prop)) {
          isVisible = false;
          break;
        }
      }

      if (!isVisible) return;

      foreach (var uiAttr in uiAttrs) {
        uiAttr.BeforeGUI(prop);
      }

      EditorGUI.BeginChangeCheck();
      EditorGUI.BeginDisabledGroup(field.isDisabled);
      // we can only have a single UI overriding OnGUI that renders the actual prop field
      var uiOverride = uiAttrs.Where(i => i.GetType().GetMethod("OnGUI")?.DeclaringType == i.GetType()).ToArray();
      if (uiOverride.Any()) {
        uiOverride.First().OnGUI(prop);
      }
      else {
        EditorGUILayout.PropertyField(prop, new GUIContent(prop.displayName));
      }

      EditorGUI.EndDisabledGroup();
      if (EditorGUI.EndChangeCheck() && field.onValueChaged != null) {
        field.onValueChaged.Invoke(t, new object[] {prop});
      }

      foreach (var uiAttr in uiAttrs) {
        uiAttr.AfterGUI(prop);
      }
    }

    private void DrawFieldElement(UTField field, SerializedProperty prop) {
      var propPath = prop.propertyPath;
      var arrIndex = Convert.ToInt32(field.isArray
        ? propPath.Substring(propPath.LastIndexOf("[") + 1, propPath.LastIndexOf("]") - propPath.LastIndexOf("[") - 1)
        : null);
      var customLabel = field.attributes.Find(i => i is ShowLabelAttribute) as ShowLabelAttribute;
      var uiAttrs = field.uiAttrs;
      var uiOverride = uiAttrs.Where(i => i.GetType().GetMethod("OnGUI")?.DeclaringType == i.GetType()).ToArray();
      EditorGUI.BeginChangeCheck();
      switch (prop.type) {
        case "bool":
          if (uiOverride.Any()) {
            uiOverride.First().OnGUI(prop);
            break;
          }

          if (customLabel == null) {
            EditorGUILayout.PropertyField(prop, new GUIContent(), GUILayout.MaxWidth(30));
          }
          else {
            EditorGUILayout.PropertyField(prop, new GUIContent(customLabel.label ?? prop.displayName));
          }

          break;
        default:
          // for list views we handle the UI separately due to how popup targeting works
          if (uiOverride.Any() && !field.isInListView) {
            uiOverride.First().OnGUI(prop);
            break;
          }

          var popupAttr = field.attributes.Find(i => i is PopupAttribute) as PopupAttribute;
          if (popupAttr != null) {
            var sourceProp = UTUtils.GetPropThroughAttribute(serializedObject, popupAttr.methodName);
            // I do not like this handling
            // need a way to determine if target is a part of the list view or not without breaking the bank
            var source = sourceProp == null ? null : !field.isInListView
              ? sourceProp
              : sourceProp.isArray && sourceProp.arraySize > arrIndex
                ? sourceProp.GetArrayElementAtIndex(arrIndex)
                : null;
            var options = UTUtils.GetPopupOptions(prop, source, popupAttr, out var selectedIndex);
            selectedIndex = EditorGUILayout.Popup(selectedIndex, options);
            if (prop.type == "int") {
              prop.intValue = selectedIndex;
            }
            else {
              prop.stringValue = options[selectedIndex];
            }
            break;
          }

          // if there are no popup attributes - still allow gui override
          if (uiOverride.Any(i => !(i is PopupAttribute))) {
            uiOverride.First(i => !(i is PopupAttribute)).OnGUI(prop);
            break;
          }

          if (customLabel == null) {
            EditorGUILayout.PropertyField(prop, new GUIContent());
          }
          else {
            EditorGUILayout.PropertyField(prop, new GUIContent(customLabel.label ?? prop.displayName));
          }

          break;
      }

      if (EditorGUI.EndChangeCheck() && field.onValueChaged != null) {
        if (!field.isInListView) {
          field.onValueChaged.Invoke(t, new object[] {prop});
          return;
        }
        HandleFieldChangeArray(field, prop, arrIndex);
      }
    }

    private void HandleFieldChangeArray(UTField field, SerializedProperty prop, int arrIndex = 0) {
      if (field.isValueChangeAtomic) {
        if (!field.isInListView) {
          field.onValueChaged.Invoke(t, new object[] {prop, arrIndex});
          return;
        }

        var fields = listViews[field.listViewName];
        var props = new List<SerializedProperty>();
        foreach (var utField in fields) {
          var parentProp = serializedObject.FindProperty(utField.name);
          props.Add(parentProp);
        }

        field.onValueChaged.Invoke(t, props.ToArray());
        return;
      }

      if (field.isValueChangedFull) {
        if (!field.isInListView) {
          var parentProp = serializedObject.FindProperty(field.name);
          var props = new SerializedProperty[parentProp.arraySize];
          for (int i = 0; i < parentProp.arraySize; i++) {
            props[i] = parentProp.GetArrayElementAtIndex(i);
          }

          field.onValueChaged.Invoke(t, new object[] {props});
          return;
        }

        {
          var fields = listViews[field.listViewName];
          var props = new List<SerializedProperty[]>();
          foreach (var utField in fields) {
            var parentProp = serializedObject.FindProperty(utField.name);
            var localProps = new List<SerializedProperty>();
            for (int i = 0; i < parentProp.arraySize; i++) {
              localProps.Add(parentProp.GetArrayElementAtIndex(i));
            }

            props.Add(localProps.ToArray());
          }

          field.onValueChaged.Invoke(t, props.ToArray());
          return;
        }
      }

      if (field.isValueChangedWithObject) {
        if (!field.isInListView) {
          var parentProp = serializedObject.FindProperty(field.name);
          var props = new SerializedProperty[parentProp.arraySize];
          for (int i = 0; i < parentProp.arraySize; i++) {
            props[i] = parentProp.GetArrayElementAtIndex(i);
          }

          field.onValueChaged.Invoke(t, new object[] {serializedObject, props});
          return;
        }

        {
          var fields = listViews[field.listViewName];
          var props = new List<SerializedProperty[]>();
          foreach (var utField in fields) {
            var parentProp = serializedObject.FindProperty(utField.name);
            var localProps = new List<SerializedProperty>();
            for (int i = 0; i < parentProp.arraySize; i++) {
              localProps.Add(parentProp.GetArrayElementAtIndex(i));
            }

            props.Add(localProps.ToArray());
          }

          var appended = new List<object> {serializedObject};
          appended.AddRange(props);

          field.onValueChaged.Invoke(t, appended.ToArray());
          return;
        }
      }
    }
  }
}
