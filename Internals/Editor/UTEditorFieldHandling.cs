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
                if (fieldCache.ContainsKey(tabSaveTarget)) {
                  var fieldChange = fieldCache?[tabSaveTarget].onValueChaged;
                  if (fieldChange != null) {
                    fieldChange.Invoke(t, new object[] {targetProp});
                  }
                }
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
            parentProp.isExpanded = UTStyles.FoldoutHeader(foldoutName, parentProp.isExpanded);
            if (!parentProp.isExpanded) break;
            EditorGUILayout.BeginVertical(new GUIStyle("helpBox"));
            HandleFields(foldouts[fieldEntry.Key]);
            EditorGUILayout.EndVertical();
            break;
          }
          case UTFieldType.ListView: {
            var listViewFields = listViews[fieldEntry.Key];
            HandleListView(listViewFields, fieldEntry);
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

    private void PropertyFieldWithUndo(SerializedProperty prop, object origValue) {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PropertyField(prop, new GUIContent(prop.displayName));
      var mainRect = GUILayoutUtility.GetLastRect();
      var rect = EditorGUILayout.GetControlRect(GUILayout.Width(UTStyles.UndoButton.fixedWidth), GUILayout.Height(UTStyles.UndoButton.fixedHeight));
      rect.y = mainRect.yMax - rect.height;
      if (GUI.Button(rect, "", UTStyles.UndoButton)) {
        tT.GetField(prop.name).SetValue(t, origValue);
      }
      EditorGUILayout.EndHorizontal();
    }
    
    private void DrawField(UTField field, SerializedProperty prop) {
      // this is taken directly from U# code
      var isNonDefault = false;
      object origValue = null;
      if (programAsset && !nonUBMode) {
        origValue = programAsset.GetRealProgram().Heap.GetHeapVariable(programAsset.GetRealProgram().SymbolTable.GetAddressFromSymbol(prop.name));
        var fieldVal = tT.GetField(prop.name)?.GetValue(t);
        isNonDefault = fieldVal != null && origValue != null && !origValue.Equals(fieldVal) ;
      }
      
      var uiAttrs = field.uiAttrs;
      if (!uiAttrs.Any()) {
        EditorGUI.BeginDisabledGroup(field.isDisabled);
        if (isNonDefault) {
          PropertyFieldWithUndo(prop, origValue);
        }
        else {
          EditorGUILayout.PropertyField(prop, new GUIContent(prop.displayName));
        }
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
        if (isNonDefault) {
          PropertyFieldWithUndo(prop, origValue);
        }
        else {
          EditorGUILayout.PropertyField(prop, new GUIContent(prop.displayName));
        }
        
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
            // we force reserialize for cases of default values
            if (prop.type == "int") {
              if (prop.intValue != selectedIndex) {
                shouldReserialize = true;
              }
              prop.intValue = selectedIndex;
            }
            else {
              if (prop.stringValue != options[selectedIndex]) {
                shouldReserialize = true;
              }
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
