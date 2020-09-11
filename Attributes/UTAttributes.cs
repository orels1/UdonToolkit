#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Editor.ProgramSources;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace UdonToolkit{
  // Unapologetically stolen from
  // https://forum.unity.com/threads/drawing-a-field-using-multiple-property-drawers.479377/#post-3331025
  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public abstract class PropertyModifierAttribute : Attribute {
    public int order { get; set; }

    public virtual float GetHeight(SerializedProperty property, GUIContent label, float height) {
      return height;
    }

    public virtual bool BeforeGUI(ref Rect position, SerializedProperty property, GUIContent label, bool visible) {
      return true;
    }
    public virtual void AfterGUI(Rect position, SerializedProperty property, GUIContent label) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class ModifiablePropertyAttribute : PropertyAttribute {
    public List<PropertyModifierAttribute> modifiers = null;

    public virtual void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      EditorGUI.PropertyField(position, property, label);
    }

    public virtual float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return EditorGUI.GetPropertyHeight(property, label);
    }
  }
  
  public class UdonPublicAttribute : ModifiablePropertyAttribute {
    public string varName;

    public UdonPublicAttribute() {
    }

    public UdonPublicAttribute(string customName) {
      varName = customName;
    }

    
  }
  
  public class UTEditorAttribute : ModifiablePropertyAttribute {
    public UTEditorAttribute() {
    }
  }

  public class SectionHeaderAttribute : PropertyModifierAttribute {
    public string text;
    private float mHeight = 20;
    private bool isInList;
    private bool savedHeight;

    public SectionHeaderAttribute(string text) {
      this.text = text;
    }

    public override float GetHeight(SerializedProperty property, GUIContent label, float height) {
      if (!savedHeight) {
        var size = EditorStyles.helpBox.CalcSize(new GUIContent(text));
        mHeight = size.y;
        savedHeight = true;
      }
      if (property.name == "data" && property.depth > 0) {
        isInList = true;
        return height;
      }
      return height + mHeight + 3;
    }

    public override bool BeforeGUI(ref Rect position, SerializedProperty property, GUIContent label, bool visible) {
      if (!visible) return false;
      if (isInList) return true;
      var lRect = GUILayoutUtility.GetLastRect();
      var rect = EditorGUI.IndentedRect(position);
      var fullWidth = EditorGUIUtility.currentViewWidth;
      rect.width = EditorGUIUtility.currentViewWidth;
      if (Math.Abs(fullWidth - lRect.width) > 0.1f) {
        rect.width -= fullWidth - lRect.width;
      }
      rect.height = mHeight;
      UTStyles.RenderSectionHeader(ref rect, text);
      position.yMin += mHeight + 3;

      return true;
    }
  }

  public class ToggleAttribute : ModifiablePropertyAttribute {
    private string label;

    public ToggleAttribute() {
      label = "";
    }

    public ToggleAttribute(string text) {
      label = text;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return EditorGUI.GetPropertyHeight(property, label) + 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      if (this.label != "") {
        label.text = this.label;
      }
      if (property.type != "bool") {
        EditorGUI.PropertyField(position, property, label);
        return;
      }

      position.yMax -= 2;
      property.boolValue = GUI.Toggle(position, property.boolValue, label, "Button");
    }
  }

  public class RangeSliderAttribute : ModifiablePropertyAttribute {
    private float min;
    private float max;

    public RangeSliderAttribute(float min, float max) {
      this.min = min;
      this.max = max;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      if (min > max) {
        EditorGUI.PropertyField(position, property, label);
        return;
      }
      switch (property.type) {
        case "float":
          if (property.floatValue < min) {
            property.floatValue = min;
          }

          if (property.floatValue > max) {
            property.floatValue = max;
          }
          EditorGUI.Slider(position, property, min, max, label);
          break;
        case "int":
          var intMin = Convert.ToInt32(Mathf.Round(min));
          var intMax = Convert.ToInt32(Mathf.Round(max));
          if (property.intValue < intMin) {
            property.intValue = intMin;
          }

          if (property.intValue > intMax) {
            property.intValue = intMax;
          }
          EditorGUI.IntSlider(position, property, intMin, intMax, label);
          break;
        default:
          EditorGUI.PropertyField(position, property, label);
          break;
      }
    }
  }

  public class OnValueChangedAttribute : PropertyModifierAttribute {
    public string methodName;
    private object oldValue;

    public OnValueChangedAttribute(string methodName) {
      if (methodName == null) {
        this.methodName = "";
        return;
      }
      this.methodName = methodName;
    }

    public override bool BeforeGUI(ref Rect position, SerializedProperty property, GUIContent label, bool visible) {
      return visible;
    }

    public override void AfterGUI(Rect position, SerializedProperty property, GUIContent label) {
      // we handle this in the ETEditor directly
      if (property.name == "data" && property.depth > 0) return;
      if (methodName == "") return;
      var newValue = property.serializedObject.targetObject.GetType().GetField(property.name)
        .GetValue(property.serializedObject.targetObject);
      if (oldValue != null && oldValue.Equals(newValue)) return;
      var m = property.serializedObject.targetObject.GetType().GetMethod(methodName, UTUtils.flags);
      if (m.GetParameters().Length > 1) {
        m.Invoke(
          property.serializedObject.targetObject, new object[] {
            property.serializedObject, property
          });
      }
      else {
        m.Invoke(
          property.serializedObject.targetObject, new object[] {property});
      }
      oldValue = newValue;
    }
  }

  public class HideIfAttribute : PropertyModifierAttribute {
    public string methodName;
    private bool isVisible;

    public HideIfAttribute(string methodName) {
      this.methodName = methodName;
    }

    public override float GetHeight(SerializedProperty property, GUIContent label, float height) {
      if (!isVisible) return 0;
      return height;
    }

    public override bool BeforeGUI(ref Rect position, SerializedProperty property, GUIContent label, bool visible) {
      isVisible = UTUtils.GetVisibleThroughAttribute(property, methodName, true);
      return isVisible;
    }
  }

  public class HelpBoxAttribute : PropertyModifierAttribute {
    public string text;
    private string methodName;
    private bool isVisible = true;
    private float boxHeight;
    private float fieldHeight;

    public HelpBoxAttribute(string text) {
      this.text = text;
      methodName = "";
    }

    public HelpBoxAttribute(string text, string methodName) {
      this.text = text;
      this.methodName = methodName;
    }

    public override float GetHeight(SerializedProperty property, GUIContent label, float height) {
      fieldHeight = height;
      if (!isVisible) return height;
      if (property.name == "data" && property.depth > 0) return height;
      boxHeight =
        new GUIStyle(EditorStyles.helpBox) { fontSize = 10 }.CalcHeight(
          new GUIContent(text), EditorGUIUtility.currentViewWidth - 10);
      return height + boxHeight + 2;
    }

    public override bool BeforeGUI(ref Rect position, SerializedProperty property, GUIContent label, bool visible) {
      isVisible = visible;
      if (!visible) return false;
      if (methodName != "") {
        isVisible = UTUtils.GetVisibleThroughAttribute(property, methodName, false);
      }
      if (isVisible && property.name != "data" && property.depth == 0) {
        position.yMax -= boxHeight;
      }
      return true;
    }

    public override void AfterGUI(Rect position, SerializedProperty property, GUIContent label) {
      if (!isVisible) return;
      if (property.name == "data" && property.depth > 0) return;
      var rect = EditorGUI.IndentedRect(position);
      // check for section header
      var secHeader = UTUtils.GetPropertyAttribute<SectionHeaderAttribute>(property);
      rect.yMin += secHeader != null ? fieldHeight / 2 + 2 : fieldHeight + 2;
      rect.height = boxHeight;
      UTStyles.RenderNote(ref rect, text);
    }
  }

  public class HorizontalAttribute : PropertyModifierAttribute {
    private string name;
    private List<FieldInfo> items = new List<FieldInfo>();
    private float size;
    private int index;
    private float yMin;
    private float height;

    public HorizontalAttribute(string name) {
      this.name = name;
    }

    public override float GetHeight(SerializedProperty property, GUIContent label, float height) {
      this.height = height;
      if (items.Count > 0 && index != 0) {
        return 0;
      }
    
      return height;
    }

    public override bool BeforeGUI(ref Rect position, SerializedProperty property, GUIContent label, bool visible) {
      items = property.serializedObject.targetObject.GetType().GetFields().Where(f => f.GetAttribute<HorizontalAttribute>() != null && f.GetAttribute<HorizontalAttribute>().name == name).ToList();
      var attrs = items.Select(f => f.GetAttribute<HorizontalAttribute>()).ToList();
      if (items.Count > 1) {
        index = items.FindIndex(a => a.Name == property.name);
        size = Mathf.Round(position.xMax / items.Count);
        var startOffset = 0;
        if (index == 0) {
          startOffset = 10;
        }
        position = new Rect(position) {
          x = startOffset + size * index + 3f * (index + 1),
          xMax = size * (index + 1)
        };
        if (index > 0) {
          var shift = height + 2;
          position.yMin -= shift * index;
          position.yMax = position.yMin + height;
        }
        // Debug.LogFormat("Field {0} index {1} yMin {2} xMin {3} xMax {4} height {5}", label.text, index, position.yMin, position.xMin, position.xMax, position.height);
      }
      
      return visible;
    }
  }

  public class HideLabelAttribute : ModifiablePropertyAttribute {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      EditorGUI.PropertyField(position, property, new GUIContent());
    }
  }

  public class PopupAttribute : ModifiablePropertyAttribute {
    public string methodName;
    private int selectedIndex = 0;
    private GUIContent[] options;
    private bool hideLabel;
    public PopupSource sourceType;
    public ShaderPropType shaderPropType = ShaderPropType.Float;
    private Dictionary<string, PopupSource> sourcesMap = new Dictionary<string, PopupSource>() {
      {"method", PopupSource.Method},
      {"animator", PopupSource.Animator},
      {"behaviour", PopupSource.UdonBehaviour},
      {"shader", PopupSource.Shader}
    };
    private Dictionary<string, ShaderPropType> shaderPropsMap = new Dictionary<string, ShaderPropType>() {
      {"float", ShaderPropType.Float},
      {"color", ShaderPropType.Color},
      {"vector", ShaderPropType.Vector}
    };

    public enum PopupSource {
      Method,
      Animator,
      UdonBehaviour,
      Shader
    }

    public enum ShaderPropType {
      Float,
      Color,
      Vector
    }

    
    public PopupAttribute(string methodName) {
      sourceType = PopupSource.Method;
      this.methodName = methodName;
    }
    
    [Obsolete("Deprecated since UdonToolkit 0.4.0, use the other Popup signatures")]
    public PopupAttribute(PopupSource sourceType, string methodName) {
      this.sourceType = sourceType;
      this.methodName = methodName;
    }

    [Obsolete("Deprecated since UdonToolkit 0.4.0, use the other Popup signatures")]
    public PopupAttribute(PopupSource sourceType, string methodName, ShaderPropType shaderPropType) {
      this.sourceType = sourceType;
      this.methodName = methodName;
      this.shaderPropType = shaderPropType;
    }

    [Obsolete("Deprecated since UdonToolkit 0.4.0, use the other Popup signatures")]
    public PopupAttribute(PopupSource sourceType, string methodName, bool hideLabel) {
      this.sourceType = sourceType;
      this.methodName = methodName;
      this.hideLabel = hideLabel;
    }
    
    [Obsolete("Deprecated since UdonToolkit 0.4.0, use the other Popup signatures")]
    public PopupAttribute(PopupSource sourceType, string methodName, ShaderPropType shaderPropType, bool hideLabel) {
      this.sourceType = sourceType;
      this.methodName = methodName;
      this.shaderPropType = shaderPropType;
      this.hideLabel = hideLabel;
    }

    /// <summary>
    /// An alternative [Popup] signature that avoids enums to compile with U#
    /// </summary>
    /// <param name="sourceType">Can be "method", "animator", "behaviour" or "shader"</param>
    /// <param name="methodName"></param>
    public PopupAttribute(string sourceType, string methodName) {
      this.sourceType = sourcesMap.ContainsKey(sourceType) ? sourcesMap[sourceType] : PopupSource.Method;
      this.methodName = methodName;
    }
    
    /// <summary>
    /// An alternative [Popup] signature that avoids enums to compile with U#
    /// </summary>
    /// <param name="sourceType">Can be "method", "animator", "behaviour" or "shader"</param>
    /// <param name="methodName"></param>
    /// <param name="shaderPropType">Can be "float", "color" or "vector"</param>
    public PopupAttribute(string sourceType, string methodName, string shaderPropType) {
      this.sourceType = sourcesMap.ContainsKey(sourceType) ? sourcesMap[sourceType] : PopupSource.Method;
      this.shaderPropType = shaderPropsMap.ContainsKey(shaderPropType) ? shaderPropsMap[shaderPropType] : ShaderPropType.Float;
      this.methodName = methodName;
    }
    
    /// <summary>
    /// An alternative [Popup] signature that avoids enums to compile with U#
    /// </summary>
    /// <param name="sourceType">Can be "method", "animator", "behaviour" or "shader"</param>
    /// <param name="methodName"></param>
    /// <param name="hideLabel"></param>
    public PopupAttribute(string sourceType, string methodName, bool hideLabel) {
      this.sourceType = sourcesMap.ContainsKey(sourceType) ? sourcesMap[sourceType] : PopupSource.Method;
      this.hideLabel = hideLabel;
      this.methodName = methodName;
    }
    
    /// <summary>
    /// An alternative [Popup] signature that avoids enums to compile with U#
    /// </summary>
    /// <param name="sourceType">Can be "method", "animator", "behaviour" or "shader"</param>
    /// <param name="methodName"></param>
    /// <param name="hideLabel"></param>
    /// <param name="shaderPropType">Can be "float", "color" or "vector"</param>
    public PopupAttribute(string sourceType, string methodName, string shaderPropType, bool hideLabel) {
      this.sourceType = sourcesMap.ContainsKey(sourceType) ? sourcesMap[sourceType] : PopupSource.Method;
      this.shaderPropType = shaderPropsMap.ContainsKey(shaderPropType) ? shaderPropsMap[shaderPropType] : ShaderPropType.Float;
      this.hideLabel = hideLabel;
      this.methodName = methodName;
    }


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      var fieldType = property.serializedObject.targetObject.GetType().GetField(property.name).FieldType;
      var source = UTUtils.GetValueThroughAttribute(property, methodName, out var sourceValType);
      if (sourceType == PopupSource.Method && fieldType != sourceValType || property.type != "string") {
        EditorGUI.PropertyField(position, property, label);
        return;
      }
      
      if (sourceType == PopupSource.Animator) {
        options = UTUtils.GetAnimatorTriggers(source as Animator).Select(o => new GUIContent(o)).ToArray();
      }
      else if (sourceType == PopupSource.UdonBehaviour) {
        options = UTUtils.GetUdonEvents(source as UdonSharpBehaviour).Select(o => new GUIContent(o)).ToArray();
      }
      else if (sourceType == PopupSource.Shader) {
        options = UTUtils.GetShaderPropertiesByType(source , shaderPropType).Select(o => new GUIContent(o)).ToArray();
      }
      else {
        options = ((string[]) source).Select(o => new GUIContent(o)).ToArray();
      }
      var finalLabel = hideLabel ? new GUIContent() : label;

      selectedIndex = options.ToList().FindIndex(i => i.text == property.stringValue);
      if (selectedIndex >= options.Length || selectedIndex < 0) {
        selectedIndex = 0;
      }
      selectedIndex = EditorGUI.Popup(position, finalLabel, selectedIndex, options);
      property.stringValue = options[selectedIndex].text;
    }
  }

  public class ListViewAttribute : ModifiablePropertyAttribute {
    public string name;
    public string addMethodName;
    public string addButtonText = "Add Element";

    public ListViewAttribute(string name) {
      this.name = name;
    }

    public ListViewAttribute(string name, string addMethodName) {
      this.name = name;
      this.addMethodName = addMethodName;
    }

    public ListViewAttribute(string name, string addMethodName, string addButtonText) {
      this.name = name;
      this.addMethodName = addMethodName;
      this.addButtonText = addButtonText;
    }
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class HelpMessageAttribute : Attribute {
    public string helpMessage;

    public HelpMessageAttribute(string message) {
      helpMessage = message;
    }
  }
  
  [AttributeUsage(AttributeTargets.Class)]
  public class CustomNameAttribute : Attribute {
    public string name;

    public CustomNameAttribute(string value) {
      name = value;
    }
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class ButtonAttribute : Attribute {
    public string text;
    public bool activeInEditMode;

    public ButtonAttribute(string text) {
      this.text = text;
    }

    public ButtonAttribute(string text, bool activeInEditMode) {
      this.text = text;
      this.activeInEditMode = activeInEditMode;
    }
  }
}
#else
using System;
using UnityEditor;

namespace UdonToolkit {
  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class SectionHeaderAttribute: Attribute {
    public SectionHeaderAttribute(object a) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class OnValueChangedAttribute : Attribute {
    public OnValueChangedAttribute(object a) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class UTEditorAttribute : Attribute {
    public UTEditorAttribute() {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class ListViewAttribute : Attribute {
    public ListViewAttribute(object a) {
    }

    public ListViewAttribute(object a, object b) {
    }

    public ListViewAttribute(object a, object b, object c) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class PopupAttribute : Attribute {
    public PopupAttribute(object a) {
    }

    public PopupAttribute(object a, object b) {
    }

    public PopupAttribute(object a, object b, object c) {
    }

    public PopupAttribute(object a, object b, object c, object d) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class ToggleAttribute : Attribute {
    public ToggleAttribute() {
    }
  }

  [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
  public class ButtonAttribute : Attribute {
    public ButtonAttribute(object a) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class HelpBoxAttribute : Attribute {
    public HelpBoxAttribute(object a) {
    }

    public HelpBoxAttribute(object a, object b) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class RangeSliderAttribute : Attribute {
    public RangeSliderAttribute(object a, object b){
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class HideIfAttribute : Attribute {
    public HideIfAttribute(object a) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class HorizontalAttribute : Attribute {
    public HorizontalAttribute(object a) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class HideLabelAttribute: Attribute {
    public HideLabelAttribute(){
    }
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class CustomNameAttribute: Attribute {
    public CustomNameAttribute(object a){
    }
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class HelpMessageAttribute: Attribute {
    public HelpMessageAttribute(object a) {
    }
  }
}
#endif