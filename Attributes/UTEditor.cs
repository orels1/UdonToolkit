#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Editor.ProgramSources;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace UdonToolkit {
  [CustomEditor(typeof(UTController), true), CanEditMultipleObjects]
  public class UTEditor : UnityEditor.Editor {
    private string undoString = "Change controller properties";
    private UTController t;
    private UdonProgramAsset uB;


    public override void OnInspectorGUI() {
      t = (UTController) target;
      // Header
      var customNameAttr = t.GetType().GetCustomAttributes(typeof(CustomNameAttribute))
        .Select(i => i as CustomNameAttribute).ToArray();
      var customName = customNameAttr.Length != 0 ? customNameAttr[0].name : t.GetType().Name.Replace("Controller", "");
      UTStyles.RenderHeader(customName);

      EditorGUI.BeginChangeCheck();
      serializedObject.Update();
      
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
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        UTStyles.RenderSectionHeader("Methods");
        foreach ((var button, var i) in buttons.WithIndex()) {
          if (GUILayout.Button(button.text)) {
            t.Invoke(methods[i].Name, 0);
          }
        }
        EditorGUI.EndDisabledGroup();
      }
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
          if (changedCallback != null) {
            t.GetType().GetMethod(changedCallback)?.Invoke(t,
              new object[] {prop.GetArrayElementAtIndex(i), i});
          }
        }

        if (RenderRemoveControls(i, new[] {prop})) {
          if (changedCallback != null) {
            t.GetType().GetMethod(changedCallback)?.Invoke(t,
              new object[] {null, i});
          }

          break;
        }
        EditorGUILayout.EndHorizontal();
      }

      if (RenderAddControls(new[] {prop}, "Add Element", null)) {
        if (changedCallback != null) {
          t.GetType().GetMethod(changedCallback)?.Invoke(t,
            new object[] {prop.GetArrayElementAtIndex(prop.arraySize - 1), prop.arraySize - 1});
        }
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
          if (changedCallback != null) {
            t.GetType().GetMethod(changedCallback)?.Invoke(t,
              new object[] {prop.GetArrayElementAtIndex(i), otherProp.GetArrayElementAtIndex(i), i});
          }
        }

        if (RenderRemoveControls(i, new[] {prop, otherProp})) {
          if (changedCallback != null) {
            t.GetType().GetMethod(changedCallback)?.Invoke(t,
              new object[] {null, null, i});
          }
          break;
        }

        EditorGUILayout.EndHorizontal();
      }

      if (RenderAddControls(new[] {prop, otherProp}, addText, addMethod)) {
        if (changedCallback != null) {
          t.GetType().GetMethod(changedCallback)?.Invoke(t,
            new object[] {prop.GetArrayElementAtIndex(prop.arraySize), otherProp.GetArrayElementAtIndex(prop.arraySize), prop.arraySize});
        }
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
            t.GetType().GetMethod(changedCallback)?.Invoke(t,
              new object[] {prop.GetArrayElementAtIndex(i), otherProp.GetArrayElementAtIndex(i), i});
          }
        }

        if (RenderRemoveControls(i, new[] {prop, otherProp})) {
          if (changedCallback != null) {
            t.GetType().GetMethod(changedCallback)?.Invoke(t,
              new object[] {null, null, i});
          }
          break;
        }

        EditorGUILayout.EndHorizontal();
      }

      if (RenderAddControls(new[] {prop, otherProp}, addText, addMethod)) {
        if (changedCallback != null) {
          t.GetType().GetMethod(changedCallback)?.Invoke(t,
            new object[] {prop.GetArrayElementAtIndex(prop.arraySize), otherProp.GetArrayElementAtIndex(prop.arraySize), prop.arraySize});
        }
      }
    }
  }
}

#endif