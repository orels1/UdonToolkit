using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UdonSharpEditor;
using UdonToolkit;
using UnityEditor;
using UnityEngine;

[assembly: DefaultUdonSharpBehaviourEditor(typeof(UTEditorRedux), "UdonToolkit Editor Redux")]

namespace UdonToolkit {
  [CustomEditor(typeof(UdonSharpBehaviour), true), CanEditMultipleObjects]
  public class UTEditorRedux : Editor {
    private UdonSharpBehaviour t;
    private Type tT;
    private UTBehaviourInfo behInfo;
    private Dictionary<string, UTField> fieldCache = new Dictionary<string, UTField>();
    private Dictionary<string, List<UTField>> listViews = new Dictionary<string, List<UTField>>();
    private Dictionary<string, List<UTField>> horizontalViews = new Dictionary<string, List<UTField>>();

    private Dictionary<string, Dictionary<string, UTFieldType>> tabs =
      new Dictionary<string, Dictionary<string, UTFieldType>>();

    private Dictionary<string, Dictionary<string, UTFieldType>> foldouts =
      new Dictionary<string, Dictionary<string, UTFieldType>>();

    private Dictionary<string, UTFieldType> fieldOrder = new Dictionary<string, UTFieldType>();
    private bool cacheBuilt;
    private bool firstRepaint = true;
    private bool droppedObjects;
    private bool showUdonSettings;
    private bool tabsExist;
    private int tabOpen;
    private bool methodsExpanded;
    private bool buttonsExpanded = true;

    public override void OnInspectorGUI() {
      t = (UdonSharpBehaviour) target;

      showUdonSettings = (bool) (UTUtils.GetUTSetting("showUdonSettings", UTUtils.UTSettingType.Bool) ?? false);

      var headerExited = false;
      EditorGUI.BeginChangeCheck();
      showUdonSettings = GUILayout.Toggle(showUdonSettings, "Udon Settings", UTStyles.smallButton);
      if (EditorGUI.EndChangeCheck()) {
        UTUtils.SetUTSetting("showUdonSettings", UTUtils.UTSettingType.Bool, showUdonSettings);
      }

      if (showUdonSettings) {
        headerExited = UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(t, true);
      }

      if (headerExited) return;

      #region Caching

      if (!cacheBuilt) {
        tT = t.GetType();
        behInfo = new UTBehaviourInfo(t);

        var prop = serializedObject.GetIterator();
        var next = prop.NextVisible(true);
        
        if (next) {
          do {
            if (prop.name == "m_Script") {
              continue;
            }
            
            if (!fieldCache.ContainsKey(prop.name)) {
              var newField = new UTField(prop);
              fieldCache.Add(prop.name, newField);
              if (newField.isInTabGroup) {
                if (!tabs.ContainsKey(newField.tabGroupName)) {
                  tabs.Add(newField.tabGroupName, new Dictionary<string, UTFieldType>());
                  // we only support one tab group per behaviour right now
                  if (!tabsExist) {
                    fieldOrder.Add(newField.tabGroupName, UTFieldType.Tab);
                    tabsExist = true;
                  }
                }

                // since tabs can host foldouts - we need to explicitly retarget them
                // this logic could be generalized if i ever want to make all of this recursive
                if (newField.isInFoldout) {
                  if (!foldouts.ContainsKey(newField.foldoutName)) {
                    foldouts.Add(newField.foldoutName, new Dictionary<string, UTFieldType>());
                    tabs[newField.tabGroupName].Add(newField.foldoutName, UTFieldType.Foldout);
                  }

                  AddToFieldOrder(newField, prop, true);
                  continue;
                }

                AddToFieldOrder(newField, prop, addToTabGroup: true);
                continue;
              }

              if (newField.isInFoldout) {
                if (!foldouts.ContainsKey(newField.foldoutName)) {
                  foldouts.Add(newField.foldoutName, new Dictionary<string, UTFieldType>());
                  fieldOrder.Add(newField.foldoutName, UTFieldType.Foldout);
                }

                AddToFieldOrder(newField, prop, true);
                continue;
              }

              AddToFieldOrder(newField, prop);
            }
          } while (prop.NextVisible(false));
        }

        cacheBuilt = true;
        return;
      }
      
      var e = Event.current;
      if (e.type == EventType.Repaint) {
        if (firstRepaint) {
          firstRepaint = false;
          return;
        }
      }

      #endregion

      if (behInfo.customName != null || behInfo.helpUrl != null) {
        EditorGUILayout.BeginHorizontal();
        UTStyles.RenderHeader(behInfo.customName != null ? behInfo.customName : behInfo.name);
        if (behInfo.helpUrl != null) {
          if (GUILayout.Button("?", GUILayout.Height(26), GUILayout.Width(26))) {
            Application.OpenURL(behInfo.helpUrl);
          }
        }
        EditorGUILayout.EndHorizontal();
      }

      if (behInfo.helpMsg != null) {
        UTStyles.RenderNote(behInfo.helpMsg);
      }

      if (behInfo.onBeforeEditor != null) {
        behInfo.onBeforeEditor.Invoke(t, new object[] {serializedObject});
      }

      // handle jagged arrays
      Undo.RecordObject(t, "Change Properties");
      
      EditorGUI.BeginChangeCheck();
      droppedObjects = false;

      // this method is overrideable by custom code
      DrawGUI();

      if (EditorGUI.EndChangeCheck() || droppedObjects) {
        if (behInfo.onValuesChanged != null) {
          behInfo.onValuesChanged.Invoke(t, new object[] {serializedObject});
        }
        serializedObject.ApplyModifiedProperties();
      }

      if (behInfo.onAfterEditor != null) {
        behInfo.onAfterEditor.Invoke(t, new object[] {serializedObject});
      }
      
      #region Buttons
      if (!Application.isPlaying && behInfo.buttons.Length > 0) {
        buttonsExpanded = UTStyles.FoldoutHeader("Editor Methods", buttonsExpanded);
        if (buttonsExpanded) {
          EditorGUILayout.BeginVertical(new GUIStyle("helpBox"));
          foreach (var button in behInfo.buttons) {
            if (GUILayout.Button(button.Name)) {
              button.Invoke(t, new object[]{});
            }
          }
          EditorGUILayout.EndVertical();
        }
      }
      if (Application.isPlaying && behInfo.udonCustomEvents.Length > 0) {
        methodsExpanded = UTStyles.FoldoutHeader("Udon Events", methodsExpanded);
        if (methodsExpanded) {
          EditorGUILayout.BeginVertical(new GUIStyle("helpBox"));
          var rowBreak = Mathf.Max(1, Mathf.Min(3, behInfo.buttons.Length - 1));
          var rowEndI = -100;
          foreach (var (button, i) in behInfo.buttons.WithIndex()) {
            if (i == rowEndI && i != behInfo.buttons.Length - 1) {
              EditorGUILayout.EndHorizontal();
            }
            if (i % rowBreak == 0 && i != behInfo.buttons.Length - 1) {
              EditorGUILayout.BeginHorizontal();
              rowEndI = Math.Min(i + rowBreak, behInfo.buttons.Length - 1);
            }
            if (GUILayout.Button(button.Name)) {
              t.SendCustomEvent(button.Name);
            }
            if (i == behInfo.buttons.Length - 1 && rowEndI != -100) {
              EditorGUILayout.EndHorizontal();
            }
          }
          EditorGUILayout.EndVertical();
        }
      }
      #endregion
    }

    public virtual void DrawGUI() {
      HandleFields(fieldOrder);
    }

    #region Behaviour Handling
    
    private struct UTBehaviourInfo {
      public string name;
      public string customName;
      public string helpUrl;
      public string helpMsg;
      public MethodInfo onValuesChanged;
      public MethodInfo onBeforeEditor;
      public MethodInfo onAfterEditor;
      public string[] udonCustomEvents;
      public MethodInfo[] buttons;

      public UTBehaviourInfo(UdonSharpBehaviour targetBeh) {
        var targetType = targetBeh.GetType();
        name = targetBeh.name;
        var cNameAttr = targetType.GetCustomAttributes(typeof(CustomNameAttribute))
          .Select(i => i as CustomNameAttribute).ToArray();
        customName = cNameAttr.Any() ? cNameAttr.First().name : null;
        var helpUrlAttr = targetType.GetCustomAttributes(typeof(HelpURLAttribute))
          .Select(i => i as HelpURLAttribute).ToArray();
        helpUrl = helpUrlAttr.Any() ? helpUrlAttr.First().URL : null;
        var helpMsgAttr = targetType.GetCustomAttributes(typeof(HelpMessageAttribute))
          .Select(i => i as HelpMessageAttribute).ToArray();
        helpMsg = helpMsgAttr.Any() ? helpMsgAttr.First().helpMessage : null;
        var onValChangedAttr = targetType.GetCustomAttributes(typeof(OnValuesChangedAttribute))
          .Select(i => i as OnValuesChangedAttribute).ToArray();
        onValuesChanged = onValChangedAttr.Any() ? targetType.GetMethod(onValChangedAttr.First().methodName) : null;
        var onBeforeEditorAttr = targetType.GetCustomAttributes(typeof(OnBeforeEditorAttribute))
          .Select(i => i as OnBeforeEditorAttribute).ToArray();
        onBeforeEditor = onBeforeEditorAttr.Any() ? targetType.GetMethod(onBeforeEditorAttr.First().methodName) : null;
        var onAfterEditorAttr = targetType.GetCustomAttributes(typeof(OnAfterEditorAttribute))
          .Select(i => i as OnAfterEditorAttribute).ToArray();
        onAfterEditor = onAfterEditorAttr.Any() ? targetType.GetMethod(onAfterEditorAttr.First().methodName) : null;
        udonCustomEvents = UTUtils.GetUdonEvents(targetBeh);
        if (udonCustomEvents.Length == 1 && udonCustomEvents.First() == "no events found") {
          udonCustomEvents = new string[0];
        }
        buttons = targetType.GetMethods()
          .Where(i => i.GetCustomAttributes(typeof(ButtonAttribute)).Any()).ToArray();
      }
    }

    #endregion

    #region Field Handling
    private void AddToFieldOrder(UTField field, SerializedProperty prop, bool addToFoldout = false,
      bool addToTabGroup = false) {
      if (!addToFoldout && !addToTabGroup) {
        AddToFieldOrder(field, prop, ref fieldOrder);
        return;
      }

      if (addToFoldout) {
        var foldoutTarget = foldouts[field.foldoutName];
        AddToFieldOrder(field, prop, ref foldoutTarget);
        foldouts[field.foldoutName] = foldoutTarget;
        return;
      }

      var tabGroupTarget = tabs[field.tabGroupName];
      AddToFieldOrder(field, prop, ref tabGroupTarget);
      tabs[field.tabGroupName] = tabGroupTarget;
      return;
    }

    private void AddToFieldOrder(UTField field, SerializedProperty prop,
      ref Dictionary<string, UTFieldType> targetObj) {
      // var targetObj = addToFoldout ? foldouts[field.foldoutName] : fieldOrder;
      // we reuse ListView rendering for the arrays
      if (field.isArray && !field.isInListView) {
        listViews.Add(prop.displayName, new List<UTField> {field});
        targetObj.Add(prop.displayName, UTFieldType.ListView);
        return;
      }

      if (field.isInListView) {
        if (!listViews.ContainsKey(field.listViewName)) {
          listViews.Add(field.listViewName, new List<UTField> {field});
          targetObj.Add(field.listViewName, UTFieldType.ListView);
        }
        else {
          listViews[field.listViewName].Add(field);
        }

        return;
      }

      if (field.isInHorizontal) {
        if (!horizontalViews.ContainsKey(field.horizontalName)) {
          horizontalViews.Add(field.horizontalName, new List<UTField> {field});
          targetObj.Add(field.horizontalName, UTFieldType.Horizontal);
        }
        else {
          horizontalViews[field.horizontalName].Add(field);
        }

        return;
      }
      else {
        targetObj.Add(field.name, UTFieldType.Regular);
      }
    }

    private void HandleFields(Dictionary<string, UTFieldType> fields) {
      foreach (var fieldEntry in fields) {
        switch (fieldEntry.Value) {
          case UTFieldType.Regular: {
            var prop = serializedObject.FindProperty(fieldEntry.Key);
            DrawField(fieldCache[fieldEntry.Key], prop);
            break;
          }
          case UTFieldType.Tab: {
            var tabNames = tabs.Select(i => i.Key).ToArray();
            tabOpen = GUILayout.Toolbar(tabOpen, tabNames);
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
            parentProp.isExpanded =
              UTStyles.FoldoutHeader($"{fieldEntry.Key} [{parentProp.arraySize}] {disabledString}",
                parentProp.isExpanded);
            var foldoutRect = GUILayoutUtility.GetLastRect();
            if (!propDisabled && UTEditorArray.HandleDragAndDrop(foldoutRect, serializedObject, propsList)) {
              droppedObjects = true;
              HandleFieldChangeArray(listViewFields[0], parentProp, parentProp.arraySize - 1);
              break;
            }

            if (!parentProp.isExpanded) break;
            EditorGUI.BeginDisabledGroup(propDisabled);
            for (int i = 0; i < parentProp.arraySize; i++) {
              Rect headerRect = default;
              Rect[] fieldRects = new Rect[listViewFields.Count];
              // get a react to draw a header later
              if (listViewFields.Count > 1 && i == 0) {
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
                if (listViewFields.Count > 1 && i == 0) {
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
              if (listViewFields.Count > 1 && i == 0) {
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

    private enum UTFieldType {
      Regular,
      Array,
      ListView,
      Horizontal,
      Foldout,
      Tab,
      Button
    }
    
    public struct UTField {
      public string name;
      public FieldInfo fieldInfo;
      public List<Attribute> attributes;
      public List<UTPropertyAttribute> uiAttrs;
      public MethodInfo onValueChaged;
      public bool isValueChangeAtomic;
      public bool isValueChangedFull;
      public bool isValueChangedWithObject;
      public bool isArray;
      public bool isInListView;
      public string listViewName;
      public string listViewColumnName;
      public bool isInHorizontal;
      public string horizontalName;
      public bool isInFoldout;
      public string foldoutName;
      public bool isInTabGroup;
      public string tabGroupName;
      public bool isDisabled;
      public bool showLabel;
      
      public UTField(SerializedProperty prop) {
        name = prop.name;
        fieldInfo = prop.serializedObject.targetObject.GetType().GetField(prop.name);
        var tType = prop.serializedObject.targetObject.GetType();
        var field = tType.GetField(prop.name, UTUtils.flags);
        attributes = field != null
          ? field.GetCustomAttributes(typeof(Attribute), true).Select(i => i as Attribute).ToList()
          : new List<Attribute>();
        uiAttrs = attributes.OfType<UTPropertyAttribute>().ToList();
        isArray = prop.isArray && prop.type != "string" && prop.type != "String";
        var lVAttr = attributes.Find(i => i.GetType() == typeof(ListViewAttribute)) as ListViewAttribute;
        isInListView = lVAttr != null;
        listViewName = isInListView ? lVAttr.name : null;
        var lvColumnNameAttr = attributes.OfType<LVHeaderAttribute>().ToList();
        listViewColumnName = lvColumnNameAttr.Any() ? lvColumnNameAttr.First().title : null;
        var hAttr = attributes.Find(i => i.GetType() == typeof(HorizontalAttribute)) as HorizontalAttribute;
        // we do not support combining horizontal with general arrays or list views
        isInHorizontal = !isArray && !isInListView && hAttr != null;
        horizontalName = isInHorizontal ? hAttr.name : null;
        var fAttr = attributes.Find(i => i.GetType() == typeof(FoldoutGroupAttribute)) as FoldoutGroupAttribute;
        isInFoldout = fAttr != null;
        foldoutName = isInFoldout ? fAttr.name : null;
        var tAttr = attributes.Find(i => i.GetType() == typeof(TabGroupAttribute)) as TabGroupAttribute;
        isInTabGroup = tAttr != null;
        tabGroupName = isInTabGroup ? tAttr.name : null;
        var disabledAttr = attributes.Find(i => i is DisabledAttribute) as DisabledAttribute;
        isDisabled = disabledAttr != null;
        var showLabelAttr = attributes.Find(i => i is ShowLabelAttribute) as ShowLabelAttribute;
        showLabel = showLabelAttr != null;

        var vChangeAttr = attributes.OfType<OnValueChangedAttribute>().ToArray();
        onValueChaged = vChangeAttr.Any() ? tType.GetMethod(vChangeAttr.First().methodName) : null;
        if (onValueChaged != null) {
          var vChangeParams = onValueChaged.GetParameters();
          isValueChangeAtomic = vChangeParams.Length == 2 && vChangeParams[1].ParameterType == typeof(int);
          if (isInListView) {
            isValueChangeAtomic = vChangeParams.Length >= 3 && vChangeParams.ToList()
                                    .GetRange(0, vChangeParams.Length - 2)
                                    .TrueForAll(i => i.ParameterType == typeof(SerializedProperty)) &&
                                  vChangeParams[vChangeParams.Length - 1].ParameterType == typeof(int);
          }

          isValueChangedFull = vChangeParams.Length == 1 && vChangeParams[0].ParameterType.IsArray;
          if (isInListView) {
            isValueChangedFull = vChangeParams.Length >= 2 &&
                                 vChangeParams.ToList().TrueForAll(i => i.ParameterType.IsArray);
          }

          isValueChangedWithObject =
            vChangeParams.Length == 2 && vChangeParams[0].ParameterType == typeof(SerializedObject);
          if (isInListView) {
            isValueChangedWithObject =
              vChangeParams.Length >= 3 && vChangeParams[0].ParameterType == typeof(SerializedObject);
          }
        }
        else {
          isValueChangeAtomic = false;
          isValueChangedFull = false;
          isValueChangedWithObject = false;
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
    #endregion
  }
}
