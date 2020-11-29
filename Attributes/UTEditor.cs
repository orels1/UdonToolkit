#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UdonSharp;
using UdonSharpEditor;
using UdonToolkit;
using UnityEditor;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Editor.ProgramSources;
using VRC.Udon.Serialization.OdinSerializer.Utilities;


[assembly:DefaultUdonSharpBehaviourEditor(typeof(UTEditor), "UdonToolkit Editor")]

namespace UdonToolkit {
  [CustomEditor(typeof(UdonSharpBehaviour), true), CanEditMultipleObjects]
  public class UTEditor : UnityEditor.Editor {
    private string undoString = "Change UdonBehaviour properties";
    private UdonSharpBehaviour t;
    private Type cT;
    private bool isPlaying;
    private BindingFlags methodFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    private bool drawDefaultInspector;
    private bool droppedObjects;

    public override void OnInspectorGUI() {
      droppedObjects = false;
      isPlaying = !Application.isPlaying;
      t = (UdonSharpBehaviour) target;
      if (cT == null) {
        cT = t.GetType();
        undoString = $"Update {cT.Name}";
      }

      if (drawDefaultInspector) {
        DrawDefaultGUI(t);
        return;
      }
      if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(t, true)) return;
      // Header
      var customNameAttr = cT.GetCustomAttributes(typeof(CustomNameAttribute))
        .Select(i => i as CustomNameAttribute).ToArray();
      var helpUrlAttribute = cT.GetCustomAttributes(typeof(HelpURLAttribute))
        .Select(i => i as HelpURLAttribute).ToArray();
      if (customNameAttr.Any() || helpUrlAttribute.Any()) {
        EditorGUILayout.BeginHorizontal();
        UTStyles.RenderHeader(customNameAttr.Any() ? customNameAttr[0].name : cT.Name.Replace("Controller", ""));
        if (helpUrlAttribute.Any()) {
          if (GUILayout.Button("?", GUILayout.Height(26), GUILayout.Width(26))) {
            Application.OpenURL(helpUrlAttribute[0].URL);
          }
        }
        EditorGUILayout.EndHorizontal();
      }

      EditorGUI.BeginChangeCheck();
      serializedObject.Update();

      // Help Box
      var helpBoxAttr = cT.GetCustomAttributes(typeof(HelpMessageAttribute))
        .Select(i => i as HelpMessageAttribute).ToArray();
      if (helpBoxAttr.Any()) {
        UTStyles.RenderNote(helpBoxAttr[0]?.helpMessage);
      }
      
      // Check prefabs
      if (PrefabUtility.IsPartOfAnyPrefab(t.gameObject)) {
        EditorGUILayout.HelpBox(
          "Udon doesn't play well with Prefabs. " +
          "It is recommended to unpack your prefabs when modifying any values.\n" +
          "Right click the prefab and choose \"Unpack Prefab\"", MessageType.Warning);
      }
      
      // Before Editor Callback
      var beforeEditorCallback = cT.GetCustomAttribute<OnBeforeEditorAttribute>();
      if (beforeEditorCallback != null) {
        var m = cT.GetMethod(beforeEditorCallback.methodName);
        m?.Invoke(t, new object[] {serializedObject});
      }
      
      // for direct editing of fields in case of jagged arrays - we need to record changes
      Undo.RecordObject(t, undoString);
      
      // Actual GUI
      try {
        DrawGUI(t);
      } catch (Exception ex) {
        // for some reason unity likes to throw ExitGUI exceptions when looking up scene objects
        // even tho it doesnt throw them when you don't try-catch
        if (ex.GetType() != typeof(ExitGUIException)) {
          Debug.LogException(ex);
        }
      }
      
      // After Editor Callback
      var afterEditorCallback = cT.GetCustomAttribute<OnAfterEditorAttribute>();
      if (afterEditorCallback != null) {
        var m = cT.GetMethod(afterEditorCallback.methodName);
        m?.Invoke(t, new object[] {serializedObject});
      }

      if (EditorGUI.EndChangeCheck() || droppedObjects) {
        // Global Values Callback
        var globalEditorCallback = cT.GetCustomAttribute<OnValuesChangedAttribute>();
        if (globalEditorCallback != null) {
          var m = cT.GetMethod(globalEditorCallback.methodName);
          m?.Invoke(t, new object[] {serializedObject});
        }
        serializedObject.ApplyModifiedProperties();
      }

      if (droppedObjects) return;

      // Extra Methods
      var methods = cT.GetMethods(methodFlags).Where(i => i.GetCustomAttribute<ButtonAttribute>() != null).ToArray();
      var buttons = methods
        .Select(i => i.GetCustomAttribute<ButtonAttribute>())
        .Where(i => i != null)
        .ToArray();
      if (buttons.Any()) {
        UTStyles.RenderSectionHeader("Methods");
        var rowBreak = Mathf.Max(1, Mathf.Min(3, buttons.Length - 1));
        var rowEndI = -100;
        foreach (var (button, i) in buttons.WithIndex()) {
          if (i == rowEndI && i != buttons.Length - 1) {
            EditorGUILayout.EndHorizontal();
          }
          if (i % rowBreak == 0 && i != buttons.Length - 1) {
            EditorGUILayout.BeginHorizontal();
            rowEndI = Math.Min(i + rowBreak, buttons.Length - 1);
          }
          EditorGUI.BeginDisabledGroup(isPlaying && !button.activeInEditMode);
          if (GUILayout.Button(button.text)) {
            if (button.activeInEditMode) {
              methods[i].Invoke(t, new object[] {});
            }
            else {
              UdonSharpEditorUtility.GetProxyBehaviour(t.GetComponent<UdonBehaviour>()).SendCustomEvent(methods[i].Name);
            }
          }
          EditorGUI.EndDisabledGroup();
          if (i == buttons.Length - 1 && rowEndI != -100) {
            EditorGUILayout.EndHorizontal();
          }
        }
      }

      UTStyles.HorizontalLine();
      if (GUILayout.Button("Show Default Inspector", UTStyles.smallButton)) {
        drawDefaultInspector = true;
      }
    }

    private void DrawDefaultGUI(UdonSharpBehaviour t) {
      if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target, false, false)) return;
      base.OnInspectorGUI();
      UTStyles.HorizontalLine();
      if (GUILayout.Button("Show UdonToolkit Inspector", UTStyles.smallButton)) {
        drawDefaultInspector = false;
      }
    }

    protected virtual void DrawGUI(UdonSharpBehaviour t) {
      var property = serializedObject.GetIterator();
      var next = property.NextVisible(true);
      if (next) {
        do {
          // if you drag & drop values - unity doesnt allow dynamic layout elements to be calculated anymore
          // so we have to exit this update cycle
          if (droppedObjects) break;
          HandleProperty(property);
        } while (property.NextVisible(false));
      }
    }

    private bool propDisabled;

    private void HandleProperty(SerializedProperty prop) {
      if (prop.name.Equals("m_Script")) {
        return;
      }
      var disabledAttribute = UTUtils.GetPropertyAttribute<DisabledAttribute>(prop);
      propDisabled = false;
      if (disabledAttribute != null) {
        if (disabledAttribute.methodName != null) {
          propDisabled = UTUtils.GetVisibleThroughAttribute(prop, disabledAttribute.methodName, false);
        }
        else {
          propDisabled = true;
        }
      }
      if (prop.isArray && prop.propertyType != SerializedPropertyType.String) {
        HandleArray(prop);
      }
      else {
        EditorGUI.BeginChangeCheck();
        EditorGUI.BeginDisabledGroup(propDisabled);
        EditorGUILayout.PropertyField(prop);
        EditorGUI.EndDisabledGroup();
        if (!EditorGUI.EndChangeCheck()) return;
        var changeCallback = UTUtils.GetPropertyAttribute<OnValueChangedAttribute>(prop);
        if (changeCallback == null) return;
        var m = prop.serializedObject.targetObject.GetType().GetMethod(changeCallback.methodName, UTUtils.flags);
        if (m == null) return;
        if (m.GetParameters().Length > 1) {
          m.Invoke(
            serializedObject.targetObject, new object[] {
              serializedObject, prop
            });
        }
        else {
          m.Invoke(
            serializedObject.targetObject, new object[] {prop});
        }
      }

      propDisabled = false;
    }

    private void RenderHelpBox(SerializedProperty prop) {
      var helpBoxAttr = UTUtils.GetPropertyAttribute<HelpBoxAttribute>(prop);
      if (helpBoxAttr != null) {
        if (helpBoxAttr.methodName.Length > 0 && !UTUtils.GetVisibleThroughAttribute(prop, helpBoxAttr.methodName, false)) return;
        UTStyles.RenderNote(helpBoxAttr.text);
      }
    }

    private void HandleArray(SerializedProperty prop) {
      var attrs = UTUtils.GetPropertyAttributes(prop);
      var modifierAttrs = UTUtils.GetPropertyAttributes<Attribute>(prop);
      var isGroup = attrs.Where(a => a is ListViewAttribute).ToArray().Length > 0;
      var groupAttribute = attrs.Select(a => a as ListViewAttribute).ToArray();
      var onValueChangedAttribute = UTUtils.GetPropertyAttribute<OnValueChangedAttribute>(prop);
      
      // hideIf attribute
      var hideIfAttribute = modifierAttrs.OfType<HideIfAttribute>().ToArray();
      var isHidden = hideIfAttribute.Length > 0 && UTUtils.GetVisibleThroughAttribute(prop, hideIfAttribute[0].methodName, false);
      // handling for regular arrays, make nicer down the line
      if (!isGroup) {
        if (isHidden) return;
        RenderArray(prop,  onValueChangedAttribute?.methodName);
        if (droppedObjects) return;
        RenderHelpBox(prop);
        return;
      }

      var groupName = groupAttribute[0].name;
      var items = cT.GetFields().Where(f =>
          f.GetAttribute<ListViewAttribute>() != null && f.GetAttribute<ListViewAttribute>().name == groupName)
        .ToList();
      // fast exit on 1 element with list view
      if (items.Count < 2) {
        if (isHidden) return;
        RenderArray(prop, onValueChangedAttribute?.methodName);
        if (droppedObjects) return;
        RenderHelpBox(prop);
        return;
      }

      var index = items.FindIndex(a => a.Name == prop.name);
      if (index > 0) {
        return;
      }

      var sectionHeaderAttribute = UTUtils.GetPropertyAttribute<SectionHeaderAttribute>(prop);
      if (sectionHeaderAttribute != null) {
        UTStyles.RenderSectionHeader(sectionHeaderAttribute.text);
      }

      if (isHidden) return;

      var otherProp = serializedObject.FindProperty(items[1].Name);
      var leftPopupAttribute = UTUtils.GetPropertyAttribute<PopupAttribute>(prop);
      var rightPopupAttribute = UTUtils.GetPropertyAttribute<PopupAttribute>(otherProp);
      if (rightPopupAttribute != null || leftPopupAttribute != null) {
        RenderStackedArray(groupAttribute[0].name, prop, otherProp, leftPopupAttribute, rightPopupAttribute, groupAttribute[0].addMethodName,
          groupAttribute[0].addButtonText, onValueChangedAttribute?.methodName);
        if (droppedObjects) return;
        RenderHelpBox(prop);
        return;
      }

      RenderStackedArray(groupAttribute[0].name, prop, otherProp, groupAttribute[0].addMethodName,
        groupAttribute[0].addButtonText, onValueChangedAttribute?.methodName);
      if (droppedObjects) return;
      RenderHelpBox(prop);
    }

    private void HandleChangeCallback(UdonSharpBehaviour t, string changedCallback, SerializedProperty prop, SerializedProperty otherProp, object[] output) {
      if (changedCallback == null) return;
      var m = cT.GetMethod(changedCallback, methodFlags);
      if (m == null) return;
      var arrVal = new List<SerializedProperty>();
      var otherArrVal = new List<SerializedProperty>();

      var appended = output.ToList().Prepend(serializedObject).ToArray();
      
      for (int j = 0; j < prop.arraySize; j++) {
        arrVal.Add(prop.GetArrayElementAtIndex(j));
      }

      if (otherProp == null) {
        if (m.GetParameters().Length == 1) {
          m.Invoke(t, new object[] {arrVal.ToArray()});
          return;
        }
        m.Invoke(t,
          m.GetParameters().Length > 2
            ? appended
            : new object[] {serializedObject, arrVal.ToArray()});
        return;
      }
      
      for (int j = 0; j < otherProp.arraySize; j++) {
        otherArrVal.Add(otherProp.GetArrayElementAtIndex(j));
      }

      if (m.GetParameters().Length == 2) {
        m.Invoke(t, new object[] {arrVal.ToArray(), otherArrVal.ToArray()});
        return;
      }
      m.Invoke(t,
      m.GetParameters().Length > 3
        ? appended
        : new object[] {serializedObject, arrVal.ToArray(), otherArrVal.ToArray()});
    }

    #region ArrayHandling

    private void HandleDragAndDrop(Rect position, SerializedProperty prop) {
      if (!position.Contains(Event.current.mousePosition)) return;
      if (Event.current.type == EventType.DragUpdated) {
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        Event.current.Use();
      }
      else if (Event.current.type == EventType.DragPerform) {
        DragAndDrop.AcceptDrag();
        var targetType = serializedObject.targetObject.GetType().GetField(prop.name).FieldType.GetElementType();
        var addingGo = targetType == typeof(GameObject);
        Debug.Log($"target type is {targetType}");
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

          var addedItem = addedGameObject.GetComponent(targetType);
          if (addedItem == null) continue;
          prop.InsertArrayElementAtIndex(addIndex);
          prop.GetArrayElementAtIndex(addIndex).objectReferenceValue = addedItem;
          droppedObjects = true;
        }
      }
    }
    
    private void HandleDragAndDrop(Rect position, SerializedProperty prop, SerializedProperty otherProp) {
      if (!position.Contains(Event.current.mousePosition)) return;
      if (Event.current.type == EventType.DragUpdated) {
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        Event.current.Use();
      }
      else if (Event.current.type == EventType.DragPerform) {
        DragAndDrop.AcceptDrag();
        var targetType = serializedObject.targetObject.GetType().GetField(prop.name).FieldType.GetElementType();
        var otherTargetType = serializedObject.targetObject.GetType().GetField(otherProp.name).FieldType.GetElementType();
        var addingGo = targetType == typeof(GameObject);
        var addingOtherGo = otherTargetType == typeof(GameObject);
        var addedElements = false;
        Debug.Log($"target type is {targetType}");
        foreach (var draggedObject in DragAndDrop.objectReferences) {
          if (!(draggedObject is GameObject addedGameObject)) continue;
          var addIndex = prop.arraySize;
          if (addingGo || addingOtherGo) {
            prop.InsertArrayElementAtIndex(addIndex);
            otherProp.InsertArrayElementAtIndex(addIndex);
            addedElements = true;
          }
          // Because GameObject is not a component, we skip the GetComponent part
          if (addingGo) {
            prop.GetArrayElementAtIndex(addIndex).objectReferenceValue = addedGameObject;
            droppedObjects = true;
          }

          if (addingOtherGo) {
            otherProp.GetArrayElementAtIndex(addIndex).objectReferenceValue = addedGameObject;
            droppedObjects = true;
          }

          if (addingGo && addingOtherGo) {
            continue;
          }

          // If adding regular components - check if we can get them
          Component addedItem = null;
          if (targetType.InheritsFrom(typeof(Component)) || targetType.InheritsFrom(typeof(MonoBehaviour))) {
            addedItem = addedGameObject.GetComponent(targetType);
          }
          Component addedOtherItem = null;
          if (otherTargetType.InheritsFrom(typeof(Component)) || otherTargetType.InheritsFrom(typeof(MonoBehaviour))) {
            addedOtherItem = addedGameObject.GetComponent(otherTargetType);
          }
          if (addedItem != null || addedOtherItem != null && !addedElements) {
            prop.InsertArrayElementAtIndex(addIndex);
            otherProp.InsertArrayElementAtIndex(addIndex);
          }
          if (addedItem != null) {
            prop.GetArrayElementAtIndex(addIndex).objectReferenceValue = addedItem;
            droppedObjects = true;
          }

          if (addedOtherItem != null) {
            otherProp.GetArrayElementAtIndex(addIndex).objectReferenceValue = addedOtherItem;
            droppedObjects = true;
          }
        }
      }
    }
    
    private void RenderArray(SerializedProperty prop, string changedCallback) {
      var formatted = Regex.Split(prop.name, @"(?<!^)(?=[A-Z])");
      formatted[0] = formatted[0].Substring(0, 1).ToUpper() + formatted[0].Substring(1);
      var disabledString = propDisabled ? "[Read Only]" : "";
      prop.isExpanded = UTStyles.FoldoutHeader($"{String.Join(" ", formatted)} [{prop.arraySize}] {disabledString}", prop.isExpanded);
      var foldoutRect = GUILayoutUtility.GetLastRect();
      if (!propDisabled) {
        HandleDragAndDrop(foldoutRect, prop);
        if (droppedObjects) return;
      }
      if (!prop.isExpanded) return;
      EditorGUI.BeginDisabledGroup(propDisabled);
      var popup = UTUtils.GetPropertyAttribute<PopupAttribute>(prop);
      for (int i = 0; i < prop.arraySize; i++) {
        EditorGUILayout.BeginHorizontal();
        if (RenderPositionControls(i, new[] {prop})) {
          break;
        }
        EditorGUI.BeginChangeCheck();
        if (popup == null) {
          EditorGUILayout.PropertyField(prop.GetArrayElementAtIndex(i), new GUIContent());
        }
        else {
          var options = UTUtils.GetPopupOptions(prop.GetArrayElementAtIndex(i), null, popup, out var selectedIndex);
          selectedIndex = EditorGUILayout.Popup(selectedIndex, options);
          prop.GetArrayElementAtIndex(i).stringValue = options[selectedIndex];
        }
        if (EditorGUI.EndChangeCheck()) {
          HandleChangeCallback(t, changedCallback, prop, null, new object[] {prop.GetArrayElementAtIndex(i), i});
        }

        if (RenderRemoveControls(i, new[] {prop})) {
          HandleChangeCallback(t, changedCallback, prop, null, new object[] {null, i});
          break;
        }
        EditorGUILayout.EndHorizontal();
      }

      if (!propDisabled) {
        EditorGUILayout.BeginHorizontal();
        if (RenderAddControls(new[] {prop}, "Add Element", null)) {
          HandleChangeCallback(t, changedCallback, prop, null, new object[] {prop.GetArrayElementAtIndex(prop.arraySize - 1), prop.arraySize - 1});
        }
        if (GUILayout.Button("Clear", GUILayout.MaxWidth(60))) {
          prop.arraySize = 0;
          HandleChangeCallback(t, changedCallback, prop, null, new object[] {null, 0});
        }
        EditorGUILayout.EndHorizontal();
      }
      EditorGUI.EndDisabledGroup();
    }

    private bool RenderPositionControls(int index, SerializedProperty[] props) {
      var newIndex = EditorGUILayout.IntField(index, new GUIStyle(EditorStyles.numberField) {
        alignment = TextAnchor.MiddleCenter,
        fixedWidth = 30
      }, GUILayout.MaxWidth(30));
      if (newIndex != index) {
        if (newIndex < 0) return true;
        if (newIndex >= props[0].arraySize) return true;
        foreach (var prop in props) {
          prop.MoveArrayElement(index, newIndex);
        }
      }

      if (GUILayout.Button("▲", GUILayout.MaxHeight(13), GUILayout.MaxWidth(25))) {
        GUI.FocusControl(null);
        if (index == 0) return true;
        foreach (var prop in props) {
          prop.MoveArrayElement(index, index - 1);
        }

        return true;
      }

      if (GUILayout.Button("▼", GUILayout.MaxHeight(13), GUILayout.MaxWidth(25))) {
        GUI.FocusControl(null);
        if (index == props[0].arraySize - 1) return true;
        foreach (var prop in props) {
          prop.MoveArrayElement(index, index + 1);
        }

        return true;
      }

      return false;
    }

    private bool RenderRemoveControls(int index, SerializedProperty[] props) {
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

    private bool RenderAddControls(SerializedProperty[] props, string addText, string addMethod) {
      if (GUILayout.Button(addText)) {
        if (addMethod.IsNullOrWhitespace()) {
          foreach (var prop in props) {
            prop.InsertArrayElementAtIndex(prop.arraySize);
          }

          return true;
        }

        var addMethodCall = cT.GetMethod(addMethod);
        addMethodCall.Invoke(t, new object[] { });
        return true;
      }

      return false;
    }

    private void RenderStackedArray(string name, SerializedProperty prop, SerializedProperty otherProp,
      string addMethod, string addText, string changedCallback) {
      var disabledString = propDisabled ? "[Read Only]" : "";
      prop.isExpanded = UTStyles.FoldoutHeader($"{name} [{prop.arraySize}] {disabledString}", prop.isExpanded);
      var foldoutRect = GUILayoutUtility.GetLastRect();
      if (!propDisabled) {
        HandleDragAndDrop(foldoutRect, prop, otherProp);
        if (droppedObjects) return;
      }
      if (!prop.isExpanded) return;
      EditorGUI.BeginDisabledGroup(propDisabled);
      for (int i = 0; i < prop.arraySize; i++) {
        EditorGUILayout.BeginHorizontal();
        if (RenderPositionControls(i, new[] {prop, otherProp})) {
          break;
        }

        EditorGUI.BeginChangeCheck();
        // handle arrays of different lengths
        var lLength = prop.arraySize;
        var rLength = otherProp.arraySize;
        if (lLength != rLength) {
          prop.arraySize = Math.Max(lLength, rLength);
          otherProp.arraySize = Math.Max(lLength, rLength);
        }
        EditorGUILayout.PropertyField(prop.GetArrayElementAtIndex(i), new GUIContent());
        EditorGUILayout.PropertyField(otherProp.GetArrayElementAtIndex(i), new GUIContent());
        if (EditorGUI.EndChangeCheck()) {
          HandleChangeCallback(t, changedCallback, prop, otherProp, new object[] {prop.GetArrayElementAtIndex(i), otherProp.GetArrayElementAtIndex(i), i});
        }

        if (RenderRemoveControls(i, new[] {prop, otherProp})) {
          HandleChangeCallback(t, changedCallback, prop, otherProp, new object[] {null, null, i});
          break;
        }

        EditorGUILayout.EndHorizontal();
      }

      if (!propDisabled) {
        EditorGUILayout.BeginHorizontal();
        if (RenderAddControls(new[] {prop, otherProp}, addText, addMethod)) {
          HandleChangeCallback(t, changedCallback, prop, otherProp, new object[] {prop.GetArrayElementAtIndex(prop.arraySize - 1), otherProp.GetArrayElementAtIndex(otherProp.arraySize - 1), prop.arraySize - 1});
        }
        if (GUILayout.Button("Clear", GUILayout.MaxWidth(40))) {
          prop.arraySize = 0;
          otherProp.arraySize = 0;
          HandleChangeCallback(t, changedCallback, prop, otherProp, new object[] {null, null, 0});
        }
        EditorGUILayout.EndHorizontal();
      }
      EditorGUI.EndDisabledGroup();
    }

    private void RenderStackedArray(string name, SerializedProperty prop, SerializedProperty otherProp, PopupAttribute leftPopup,
      PopupAttribute rightPopup,
      string addMethod, string addText, string changedCallback) {
      var disabledString = propDisabled ? "[Read Only]" : "";
      prop.isExpanded = UTStyles.FoldoutHeader($"{name} [{prop.arraySize}] {disabledString}", prop.isExpanded);
      var foldoutRect = GUILayoutUtility.GetLastRect();
      if (!propDisabled) {
        HandleDragAndDrop(foldoutRect, prop, otherProp);
        if (droppedObjects) return;
      }
      if (!prop.isExpanded) return;
      EditorGUI.BeginDisabledGroup(propDisabled);
      for (int i = 0; i < prop.arraySize; i++) {
        EditorGUILayout.BeginHorizontal();
        if (RenderPositionControls(i, new[] {prop, otherProp})) {
          break;
        }

        // this code is very similar to PopupAttribute itself
        // but we have to handle it here directly because we are connecting two different props together
        // should probably refactor at some point
        EditorGUI.BeginChangeCheck();
        // handle arrays of different lengths
        var lLength = prop.arraySize;
        var rLength = otherProp.arraySize;
        if (lLength != rLength) {
          prop.arraySize = Math.Max(lLength, rLength);
          otherProp.arraySize = Math.Max(lLength, rLength);
        }
        // Left Field
        if (leftPopup == null) {
          EditorGUILayout.PropertyField(prop.GetArrayElementAtIndex(i), new GUIContent());
        }
        else {
          var source = otherProp.GetArrayElementAtIndex(i);
          var options = UTUtils.GetPopupOptions(prop.GetArrayElementAtIndex(i), source, leftPopup, out var selectedIndex);

          selectedIndex = EditorGUILayout.Popup(selectedIndex, options);
          prop.GetArrayElementAtIndex(i).stringValue = options[selectedIndex];
        }

        if (rightPopup == null) {
          EditorGUILayout.PropertyField(otherProp.GetArrayElementAtIndex(i), new GUIContent());
        }
        else {
          var source = prop.GetArrayElementAtIndex(i);
          var options = UTUtils.GetPopupOptions(otherProp.GetArrayElementAtIndex(i), source, rightPopup, out var selectedIndex);

          selectedIndex = EditorGUILayout.Popup(selectedIndex, options);
          otherProp.GetArrayElementAtIndex(i).stringValue = options[selectedIndex];
        }
        
        if (EditorGUI.EndChangeCheck()) {
          HandleChangeCallback(t, changedCallback, prop, otherProp, new object[] {prop.GetArrayElementAtIndex(i), otherProp.GetArrayElementAtIndex(i), i});
        }

        if (RenderRemoveControls(i, new[] {prop, otherProp})) {
          HandleChangeCallback(t, changedCallback, prop, otherProp, new object[] {null, null, i});
          break;
        }

        EditorGUILayout.EndHorizontal();
      }

      if (!propDisabled) {
        EditorGUILayout.BeginHorizontal();
        if (RenderAddControls(new[] {prop, otherProp}, addText, addMethod)) {
          HandleChangeCallback(t, changedCallback, prop, otherProp, new object[] {prop.GetArrayElementAtIndex(prop.arraySize - 1), otherProp.GetArrayElementAtIndex(otherProp.arraySize - 1), prop.arraySize - 1});
        }
        if (GUILayout.Button("Clear", GUILayout.MaxWidth(60))) {
          prop.arraySize = 0;
          otherProp.arraySize = 0;
          HandleChangeCallback(t, changedCallback, prop, otherProp, new object[] {null, null, 0});
        }
        EditorGUILayout.EndHorizontal();
      }
      EditorGUI.EndDisabledGroup();
    }
    #endregion
  }
}

#endif