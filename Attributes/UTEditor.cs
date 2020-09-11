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


    public override void OnInspectorGUI() {
      isPlaying = !Application.isPlaying;
      t = (UdonSharpBehaviour) target;
      if (cT == null) {
        cT = t.GetType();
        undoString = $"Update {cT.Name}";
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
      
      // Actual GUI
      DrawGUI(t);
      if (EditorGUI.EndChangeCheck()) {
        Undo.RecordObject(t, undoString);
        serializedObject.ApplyModifiedProperties();
      }

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
    }

    protected virtual void DrawGUI(UdonSharpBehaviour t) {
      var property = serializedObject.GetIterator();
      var next = property.NextVisible(true);
      if (next) {
        do {
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
      propDisabled = disabledAttribute != null;
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
        RenderHelpBox(prop);
        return;
      }

      RenderStackedArray(groupAttribute[0].name, prop, otherProp, groupAttribute[0].addMethodName,
        groupAttribute[0].addButtonText, onValueChangedAttribute?.methodName);
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
        m.Invoke(t,
          m.GetParameters().Length > 2
            ? appended
            : new object[] {serializedObject, arrVal.ToArray()});
        return;
      }
      
      for (int j = 0; j < otherProp.arraySize; j++) {
        otherArrVal.Add(otherProp.GetArrayElementAtIndex(j));
      }
      m.Invoke(t,
      m.GetParameters().Length > 3
        ? appended
        : new object[] {serializedObject, arrVal.ToArray(), otherArrVal.ToArray()});
    }

    #region ArrayHandling
    
    private void RenderArray(SerializedProperty prop, string changedCallback) {
      var formatted = Regex.Split(prop.name, @"(?<!^)(?=[A-Z])");
      formatted[0] = formatted[0].Substring(0, 1).ToUpper() + formatted[0].Substring(1);
      var disabledString = propDisabled ? "[Read Only]" : "";
      prop.isExpanded = UTStyles.FoldoutHeader($"{String.Join(" ", formatted)} [{prop.arraySize}] {disabledString}", prop.isExpanded);
      if (!prop.isExpanded) return;
      EditorGUI.BeginDisabledGroup(propDisabled);
      for (int i = 0; i < prop.arraySize; i++) {
        EditorGUILayout.BeginHorizontal();
        if (RenderPositionControls(i, new[] {prop})) {
          break;
        }
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(prop.GetArrayElementAtIndex(i), new GUIContent());
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
        if (RenderAddControls(new[] {prop}, "Add Element", null)) {
          HandleChangeCallback(t, changedCallback, prop, null, new object[] {prop.GetArrayElementAtIndex(prop.arraySize - 1), prop.arraySize - 1});
        }
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
        if (RenderAddControls(new[] {prop, otherProp}, addText, addMethod)) {
          HandleChangeCallback(t, changedCallback, prop, otherProp, new object[] {prop.GetArrayElementAtIndex(prop.arraySize - 1), otherProp.GetArrayElementAtIndex(otherProp.arraySize - 1), prop.arraySize - 1});
        }
      }
      EditorGUI.EndDisabledGroup();
    }

    private void RenderStackedArray(string name, SerializedProperty prop, SerializedProperty otherProp, PopupAttribute leftPopup,
      PopupAttribute rightPopup,
      string addMethod, string addText, string changedCallback) {
      var disabledString = propDisabled ? "[Read Only]" : "";
      prop.isExpanded = UTStyles.FoldoutHeader($"{name} [{prop.arraySize}] {disabledString}", prop.isExpanded);
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
          string[] options;
          var sourceType = leftPopup.sourceType;
          var source = prop.GetArrayElementAtIndex(i);

          // Right Field
          if (sourceType == PopupAttribute.PopupSource.Animator) {
            options = UTUtils.GetAnimatorTriggers(source.objectReferenceValue as Animator);
          }
          else if (sourceType == PopupAttribute.PopupSource.UdonBehaviour) {
            options = UTUtils.GetUdonEvents(source.objectReferenceValue as UdonSharpBehaviour);
          }
          else if (sourceType == PopupAttribute.PopupSource.Shader) {
            var propsSource = UTUtils.GetValueThroughAttribute(source, leftPopup.methodName, out _);
            options = UTUtils.GetShaderPropertiesByType(propsSource, leftPopup.shaderPropType);
          }
          else {
            options = (string[]) UTUtils.GetValueThroughAttribute(prop, leftPopup.methodName, out _);
          }

          var selectedIndex = options.ToList().IndexOf(prop.GetArrayElementAtIndex(i).stringValue);
          if (selectedIndex >= options.Length || selectedIndex == -1) {
            selectedIndex = 0;
          }

          selectedIndex = EditorGUILayout.Popup(selectedIndex, options);
          prop.GetArrayElementAtIndex(i).stringValue = options[selectedIndex];
        }

        if (rightPopup == null) {
          EditorGUILayout.PropertyField(otherProp.GetArrayElementAtIndex(i), new GUIContent());
        }
        else {
          string[] options;
          var sourceType = rightPopup.sourceType;
          var source = prop.GetArrayElementAtIndex(i);

          // Right Field
          if (sourceType == PopupAttribute.PopupSource.Animator) {
            options = UTUtils.GetAnimatorTriggers(source.objectReferenceValue as Animator);
          }
          else if (sourceType == PopupAttribute.PopupSource.UdonBehaviour) {
            options = UTUtils.GetUdonEvents(source.objectReferenceValue as UdonSharpBehaviour);
          }
          else if (sourceType == PopupAttribute.PopupSource.Shader) {
            var propsSource = UTUtils.GetValueThroughAttribute(source, rightPopup.methodName, out _);
            options = UTUtils.GetShaderPropertiesByType(propsSource, rightPopup.shaderPropType);
          }
          else {
            options = (string[]) UTUtils.GetValueThroughAttribute(otherProp, rightPopup.methodName, out _);
          }

          var selectedIndex = options.ToList().IndexOf(otherProp.GetArrayElementAtIndex(i).stringValue);
          if (selectedIndex >= options.Length || selectedIndex == -1) {
            selectedIndex = 0;
          }

          selectedIndex = EditorGUILayout.Popup(selectedIndex, options);
          otherProp.GetArrayElementAtIndex(i).stringValue = options[selectedIndex];
        }
        
        if (EditorGUI.EndChangeCheck()) {
          if (changedCallback != null) {
            var m = cT.GetMethod(changedCallback);
            if (m != null) {
              m.Invoke(t,
                m.GetParameters().Length > 2
                  ? new object[] {prop.GetArrayElementAtIndex(i), otherProp.GetArrayElementAtIndex(i), i}
                  : new object[] {prop, otherProp});
            }
          }
        }

        if (RenderRemoveControls(i, new[] {prop, otherProp})) {
          if (changedCallback != null) {
            var m = cT.GetMethod(changedCallback);
            if (m != null) {
              m.Invoke(t,
                m.GetParameters().Length > 2
                  ? new object[] {null, null, i}
                  : new object[] {prop, otherProp});
            }
          }
          break;
        }

        EditorGUILayout.EndHorizontal();
      }

      if (!propDisabled) {
        if (RenderAddControls(new[] {prop, otherProp}, addText, addMethod)) {
          HandleChangeCallback(t, changedCallback, prop, otherProp, new object[] {prop.GetArrayElementAtIndex(prop.arraySize - 1), otherProp.GetArrayElementAtIndex(otherProp.arraySize - 1), prop.arraySize - 1});
        }
      }
      EditorGUI.EndDisabledGroup();
    }
    #endregion
  }
}

#endif