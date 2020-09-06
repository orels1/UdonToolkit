#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Editor.ProgramSources;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomEditor(typeof(UTController), true), CanEditMultipleObjects]
  public class UTEditor : UnityEditor.Editor {
    private string undoString = "Change controller properties";
    private UTController t;
    private UdonProgramAsset uB;
    private bool copiedValues;


    public override void OnInspectorGUI() {
      t = (UTController) target;
      // Copy current values
      if (!copiedValues) {
        t.SyncBack();
        copiedValues = true;
      }

      // Header
      var customNameAttr = t.GetType().GetCustomAttributes(typeof(CustomNameAttribute))
        .Select(i => i as CustomNameAttribute).ToArray();
      var customName = customNameAttr.Length != 0 ? customNameAttr[0].name : t.GetType().Name.Replace("Controller", "");
      UTStyles.RenderHeader(customName);

      EditorGUI.BeginChangeCheck();
      serializedObject.Update();
      
      EditorGUILayout.HelpBox("Controllers are Deprecated since UdonToolkit v0.4.0 in favor of a more streamlined system", MessageType.Warning);
      if (GUILayout.Button("Learn more")) {
        Application.OpenURL("https://l.vrchat.sh/utV4Migrate");
      }

      // Extra pre-gui actions
      t.SetupController();

      // Auto UB Addition
      if (t.GetComponent<UdonBehaviour>() == null) {
        if (uB) {
          var comp = t.gameObject.AddComponent<UdonBehaviour>();
          comp.programSource = uB;
        }
        else {
          var controlledBehAttr = t.GetType().GetCustomAttributes(typeof(ControlledBehaviourAttribute))
            .Select(i => i as ControlledBehaviourAttribute).ToArray();
          if (controlledBehAttr.Any()) {
            var comp = t.gameObject.AddComponent<UdonBehaviour>();
            uB = controlledBehAttr[0].uB;
            comp.programSource = uB;
          }
        }
      }
      
      // Help Box
      var helpBoxAttr = t.GetType().GetCustomAttributes(typeof(HelpMessageAttribute))
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
        t.SyncValues();
      }
      
      // Sync Toggles
      UTStyles.RenderSectionHeader("Udon Sync");
      if (t.uB != null) {
        var uBo = new SerializedObject(t.uB);
        uBo.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(uBo.FindProperty("SynchronizePosition"));
        var col = t.gameObject.GetComponent<Collider>();
        if (col != null) {
          EditorGUILayout.PropertyField(uBo.FindProperty("AllowCollisionOwnershipTransfer"), new GUIContent("Collision Owner Transfer"));
        }
        if (EditorGUI.EndChangeCheck()) {
          uBo.ApplyModifiedProperties();
        }
      }

      // Extra Methods
      var methods = t.GetType().GetMethods().Where(i => i.GetCustomAttribute<ButtonAttribute>() != null)
        .ToArray();
      var buttons = t.GetType().GetMethods().Select(i => i.GetCustomAttribute<ButtonAttribute>()).Where(i => i != null)
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
          EditorGUI.BeginDisabledGroup(!Application.isPlaying && !button.activeInEditMode);
          if (GUILayout.Button(button.text)) {
            t.Invoke(methods[i].Name, 0);
          }
          EditorGUI.EndDisabledGroup();
          if (i == buttons.Length - 1 && rowEndI != -100) {
            EditorGUILayout.EndHorizontal();
          }
        }
      }
      UTStyles.RenderSectionHeader("Manual Value Sync");
      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("Copy to Udon")) {
        t.SyncValues();
      }

      if (GUILayout.Button("Copy from Udon")) {
        t.SyncBack();
      }
      EditorGUILayout.EndHorizontal();
    }

    protected virtual void DrawGUI(UTController t) {
      var property = serializedObject.GetIterator();
      var next = property.NextVisible(true);
      if (next) {
        do {
          HandleProperty(property);
        } while (property.NextVisible(false));
      }
    }

    private void HandleProperty(SerializedProperty prop) {
      if (prop.name.Equals("m_Script")) {
        return;
      }

      if (prop.isArray && prop.propertyType != SerializedPropertyType.String) {
        HandleArray(prop);
      }
      else {
        EditorGUILayout.PropertyField(prop);
      }
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
      var items = t.GetType().GetFields().Where(f =>
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

    private void HandleChangeCallback(UTController t, string changedCallback, SerializedProperty prop, SerializedProperty otherProp, object[] output) {
      if (changedCallback == null) return;
      var m = t.GetType().GetMethod(changedCallback);
      if (m == null) return;
      var arrVal = new List<SerializedProperty>();
      var otherArrVal = new List<SerializedProperty>();
      
      for (int j = 0; j < prop.arraySize; j++) {
        arrVal.Add(prop.GetArrayElementAtIndex(j));
      }

      if (otherProp == null) {
        m.Invoke(t,
          m.GetParameters().Length > 1
            ? output
            : new object[] {arrVal.ToArray()});
        return;
      }
      
      for (int j = 0; j < otherProp.arraySize; j++) {
        otherArrVal.Add(otherProp.GetArrayElementAtIndex(j));
      }
      m.Invoke(t,
      m.GetParameters().Length > 2
        ? output
        : new object[] {arrVal.ToArray(), otherArrVal.ToArray()});
    }

    #region ArrayHandling
    
    private void RenderArray(SerializedProperty prop, string changedCallback) {
      var formatted = Regex.Split(prop.name, @"(?<!^)(?=[A-Z])");
      formatted[0] = formatted[0].Substring(0, 1).ToUpper() + formatted[0].Substring(1);
      prop.isExpanded = UTStyles.FoldoutHeader($"{String.Join(" ", formatted)} [{prop.arraySize}]", prop.isExpanded);
      if (!prop.isExpanded) return;
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

      if (RenderAddControls(new[] {prop}, "Add Element", null)) {
        HandleChangeCallback(t, changedCallback, prop, null, new object[] {prop.GetArrayElementAtIndex(prop.arraySize - 1), prop.arraySize - 1});
      }
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

        var addMethodCall = t.GetType().GetMethod(addMethod);
        addMethodCall.Invoke(t, new object[] { });
        return true;
      }

      return false;
    }

    private void RenderStackedArray(string name, SerializedProperty prop, SerializedProperty otherProp,
      string addMethod, string addText, string changedCallback) {
      prop.isExpanded = UTStyles.FoldoutHeader($"{name} [{prop.arraySize}]", prop.isExpanded);
      if (!prop.isExpanded) return;
      for (int i = 0; i < prop.arraySize; i++) {
        EditorGUILayout.BeginHorizontal();
        if (RenderPositionControls(i, new[] {prop, otherProp})) {
          break;
        }

        EditorGUI.BeginChangeCheck();
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

      if (RenderAddControls(new[] {prop, otherProp}, addText, addMethod)) {
        HandleChangeCallback(t, changedCallback, prop, otherProp, new object[] {prop.GetArrayElementAtIndex(prop.arraySize - 1), otherProp.GetArrayElementAtIndex(otherProp.arraySize - 1), prop.arraySize - 1});
      }
    }

    private void RenderStackedArray(string name, SerializedProperty prop, SerializedProperty otherProp, PopupAttribute leftPopup,
      PopupAttribute rightPopup,
      string addMethod, string addText, string changedCallback) {
      prop.isExpanded = UTStyles.FoldoutHeader($"{name} [{prop.arraySize}]", prop.isExpanded);
      if (!prop.isExpanded) return;
      for (int i = 0; i < prop.arraySize; i++) {
        EditorGUILayout.BeginHorizontal();
        if (RenderPositionControls(i, new[] {prop, otherProp})) {
          break;
        }

        // this code is very similar to PopupAttribute itself
        // but we have to handle it here directly because we are connecting two different props together
        // should probably refactor at some point
        EditorGUI.BeginChangeCheck();
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
            options = UTUtils.GetUdonEvents(source.objectReferenceValue as UdonBehaviour);
          }
          else if (sourceType == PopupAttribute.PopupSource.Shader) {
            var propsSource = UTUtils.GetValueThroughAttribute(source, leftPopup.methodName, out _);
            options = UTUtils.GetShaderPropertiesByType(propsSource as Shader, leftPopup.shaderPropType);
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
            options = UTUtils.GetUdonEvents(source.objectReferenceValue as UdonBehaviour);
          }
          else if (sourceType == PopupAttribute.PopupSource.Shader) {
            var propsSource = UTUtils.GetValueThroughAttribute(source, rightPopup.methodName, out _);
            options = UTUtils.GetShaderPropertiesByType(propsSource as Shader, rightPopup.shaderPropType);
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
            var m = t.GetType().GetMethod(changedCallback);
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
            var m = t.GetType().GetMethod(changedCallback);
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

      if (RenderAddControls(new[] {prop, otherProp}, addText, addMethod)) {
        if (changedCallback != null) {
          var m = t.GetType().GetMethod(changedCallback);
          if (m != null) {
            m.Invoke(t,
              m.GetParameters().Length > 2
                ? new object[] {prop.GetArrayElementAtIndex(prop.arraySize), otherProp.GetArrayElementAtIndex(prop.arraySize), prop.arraySize}
                : new object[] {prop, otherProp});
          }
        }
      }
    }
    #endregion
  }
}

#endif