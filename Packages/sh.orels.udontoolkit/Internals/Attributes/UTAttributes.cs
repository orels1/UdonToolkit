#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace UdonToolkit{
  /// <summary>
  /// These attributes directly affect field drawing using the provided overrides
  /// </summary>
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
  public class UTPropertyAttribute: Attribute {
    public virtual void BeforeGUI(SerializedProperty property) {
    }
    
    public virtual void OnGUI(SerializedProperty property) {
    }

    public virtual void AfterGUI(SerializedProperty property) {
    }

    public virtual bool GetVisible(SerializedProperty property) {
      return true;
    }
  }

  /// <summary>
  /// These attributes are used to pass data to custom logic in the UTEditor
  /// </summary>
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
  public class UTVisualAttribute : Attribute {
  }
  
  public class SectionHeaderAttribute : UTPropertyAttribute {
    public readonly string text;
    private bool isInList;
    private bool savedHeight;

    /// <summary>
    /// Draws a Section Header with the provided text
    /// </summary>
    /// <param name="text"></param>
    public SectionHeaderAttribute(string text) {
      this.text = text;
    }

    public override void BeforeGUI(SerializedProperty property) {
      UTStyles.RenderSectionHeader(text);
    }
  }

  public class ToggleAttribute : UTPropertyAttribute {
    private readonly string label;

    /// <summary>
    /// Converts a boolean field checkbox into a toggle-style button
    /// </summary>
    public ToggleAttribute() {
      label = "";
    }

    /// <summary>
    /// Converts a boolean field checkbox into a toggle-style button with the provided text
    /// </summary>
    /// <param name="text"></param>
    public ToggleAttribute(string text) {
      label = text;
    }

    public override void OnGUI(SerializedProperty property) {
      var text = String.IsNullOrWhiteSpace(label) ? property.displayName : label;
      if (property.type != "bool") {
        EditorGUILayout.PropertyField(property, new GUIContent(property.displayName));
        return;
      }
      
      property.boolValue = GUILayout.Toggle(property.boolValue, text, "Button");
    }
  }

  public class RangeSliderAttribute : UTPropertyAttribute {
    private readonly float min;
    private readonly float max;
    private readonly bool hideLabel;

    /// <summary>
    /// Draws a value slider that goes from min to max. Can be put on both int and float fields
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public RangeSliderAttribute(float min, float max) {
      this.min = min;
      this.max = max;
    }
    
    public RangeSliderAttribute(float min, float max, bool hideLabel) {
      this.min = min;
      this.max = max;
      this.hideLabel = hideLabel;
    }

    public override void OnGUI(SerializedProperty property) {
      if (min > max) {
        EditorGUILayout.PropertyField(property, new GUIContent(property.displayName));
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
          EditorGUILayout.Slider(property, min, max, new GUIContent(hideLabel ? "" : property.displayName));
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
          EditorGUILayout.IntSlider(property, intMin, intMax, new GUIContent(hideLabel ? "" : property.displayName));
          break;
        default:
          EditorGUILayout.PropertyField(property, new GUIContent(hideLabel ? "" : property.displayName));
          break;
      }
    }
  }

  /// <summary>
  /// Calls the provided method whenever the value is changed, <a href="https://github.com/orels1/UdonToolkit/wiki/Attributes#onvaluechanged">see wiki for more details</a>
  /// </summary>
  public class OnValueChangedAttribute : UTVisualAttribute {
    public readonly string methodName;
    private object oldValue;
    
    /// <summary>
    /// Calls the provided method whenever the value is changed, <a href="https://github.com/orels1/UdonToolkit/wiki/Attributes#onvaluechanged">see wiki for more details</a>
    /// </summary>
    /// <param name="methodName"></param>
    public OnValueChangedAttribute(string methodName) {
      if (methodName == null) {
        this.methodName = "";
        return;
      }
      this.methodName = methodName;
    }
  }

  /// <summary>
  /// Hides a field based on the provided field or method
  /// </summary>
  public class HideIfAttribute : UTPropertyAttribute {
    public readonly string methodName;
    private bool isVisible;

    /// <summary>
    /// Hides a field based on the provided field or method
    /// </summary>
    /// <param name="methodName"></param>
    public HideIfAttribute(string methodName) {
      this.methodName = methodName;
    }

    public override bool GetVisible(SerializedProperty property) {
      isVisible = UTUtils.GetVisibleThroughAttribute(property, methodName, true);
      return isVisible;
    }
  }

  /// <summary>
  /// Shows a help box under a field
  /// </summary>
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
  public class HelpBoxAttribute : UTPropertyAttribute {
    public readonly string text;
    public readonly string methodName;
    private bool isVisible = true;
    private float boxHeight;
    private float fieldHeight;

    /// <summary>
    /// Shows a help box with the provided text
    /// </summary>
    /// <param name="text"></param>
    public HelpBoxAttribute(string text) {
      this.text = text;
      methodName = "";
    }

    /// <summary>
    /// Shows a help box with the provided text based on the provided field or method
    /// </summary>
    /// <param name="text"></param>
    /// <param name="methodName"></param>
    public HelpBoxAttribute(string text, string methodName) {
      this.text = text;
      this.methodName = methodName;
    }
    
    public override void AfterGUI(SerializedProperty property) {
      if (methodName != "") {
        isVisible = UTUtils.GetVisibleThroughAttribute(property, methodName, false);
      }

      if (!isVisible) return;
      UTStyles.RenderNote(text);
    }
  }

  /// <summary>
  /// Combines fields into a horizontal group
  /// </summary>
  public class HorizontalAttribute : UTPropertyAttribute {
    public readonly string name;
    public readonly bool showHeader;

    /// <summary>
    /// Combines fields into a horizontal group
    /// </summary>
    /// <param name="name">Name of the group (must be unique)</param>
    public HorizontalAttribute(string name) {
      this.name = name;
    }

    /// <summary>
    /// Combines fields into a horizontal group
    /// </summary>
    /// <param name="name">Name of the group (must be unique)</param>
    /// <param name="showHeader">Whether to show the header with the name of the group</param>
    public HorizontalAttribute(string name, bool showHeader) {
      this.name = name;
      this.showHeader = showHeader;
    }
  }

  /// <summary>
  /// Hides the label of the field
  /// </summary>
  public class HideLabelAttribute : UTPropertyAttribute {
    public override void OnGUI(SerializedProperty property) {
      EditorGUILayout.PropertyField(property, new GUIContent());
    }
  }

  /// <summary>
  /// Draws a popup for a field with options to choose from
  /// <a href="https://github.com/orels1/UdonToolkit/wiki/Attributes#popup">See More</a>
  /// </summary>
  public class PopupAttribute : UTPropertyAttribute {
    public readonly string methodName;
    private int selectedIndex;
    private GUIContent[] options;
    private readonly bool hideLabel;
    public readonly PopupSource sourceType;
    public readonly ShaderPropType shaderPropType = ShaderPropType.Float;
    private Dictionary<string, PopupSource> sourcesMap = new Dictionary<string, PopupSource>() {
      {"method", PopupSource.Method},
      {"animator", PopupSource.AnimatorTrigger},
      {"animatorTrigger", PopupSource.AnimatorTrigger},
      {"animatorBool", PopupSource.AnimatorBool},
      {"animatorFloat", PopupSource.AnimatorFloat},
      {"animatorInt", PopupSource.AnimatorInt},
      {"behaviour", PopupSource.UdonBehaviour},
      {"programVariable", PopupSource.UdonProgramVariable},
      {"shader", PopupSource.Shader}
    };
    private Dictionary<string, ShaderPropType> shaderPropsMap = new Dictionary<string, ShaderPropType>() {
      {"float", ShaderPropType.Float},
      {"color", ShaderPropType.Color},
      {"vector", ShaderPropType.Vector},
      {"all", ShaderPropType.All}
    };

    public enum PopupSource {
      Method,
      AnimatorTrigger,
      AnimatorBool,
      AnimatorFloat,
      AnimatorInt,
      UdonBehaviour,
      UdonProgramVariable,
      Shader
    }

    public enum ShaderPropType {
      Float,
      Color,
      Vector,
      All
    }

    /// <summary>
    /// Draws a popup with options provided by the specified field or method
    /// </summary>
    /// <param name="methodName"></param>
    public PopupAttribute(string methodName) {
      sourceType = PopupSource.Method;
      this.methodName = methodName;
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

    public override void OnGUI(SerializedProperty property) {
      var source = UTUtils.GetValueThroughAttribute(property, methodName, out var sourceValType);
      if (property.name == "data" && sourceType == PopupSource.Method && property.type != "string" && property.type != "int") {
        EditorGUILayout.PropertyField(property, new GUIContent(property.displayName));
        return;
      }

      if (property.name != "data") {
        var fieldType = property.serializedObject.targetObject.GetType().GetField(property.name).FieldType;
        if (sourceType == PopupSource.Method && fieldType != sourceValType && property.type != "string" && property.type != "int") {
          EditorGUILayout.PropertyField(property, new GUIContent(property.displayName));
          return;
        }
      }

      var numericPopup = property.type == "int" && sourceValType == typeof(int) ||
                         property.type == "float" && sourceValType == typeof(float);
      
      if (sourceType == PopupSource.AnimatorTrigger) {
        options = UTUtils.GetAnimatorTriggers(source as Animator).Select(o => new GUIContent(o)).ToArray();
      }
      else if (sourceType == PopupSource.AnimatorBool) {
        options = UTUtils.GetAnimatorBools(source as Animator).Select(o => new GUIContent(o)).ToArray();
      }
      else if (sourceType == PopupSource.AnimatorFloat) {
        options = UTUtils.GetAnimatorFloats(source as Animator).Select(o => new GUIContent(o)).ToArray();
      }
      else if (sourceType == PopupSource.AnimatorInt) {
        options = UTUtils.GetAnimatorInts(source as Animator).Select(o => new GUIContent(o)).ToArray();
      }
      else if (sourceType == PopupSource.UdonBehaviour) {
        options = UTUtils.GetUdonEvents(source as UdonSharpBehaviour).Select(o => new GUIContent(o)).ToArray();
      }
      else if (sourceType == PopupSource.UdonProgramVariable) {
        options = UTUtils.GetUdonVariables(source as UdonSharpBehaviour).Select(o => new GUIContent(o)).ToArray();
      }
      else if (sourceType == PopupSource.Shader) {
        if (shaderPropType == ShaderPropType.All) {
          options = UTUtils.GetAllShaderProperties(source ).Select(o => new GUIContent(o)).ToArray();
        }
        else {
          options = UTUtils.GetShaderPropertiesByType(source , shaderPropType).Select(o => new GUIContent(o)).ToArray();
        }
      }
      else {
        if (sourceValType == typeof(int)) {
          options = ((int[]) source).Select(i => new GUIContent(i.ToString())).ToArray();
        } else if (sourceValType == typeof(float)) {
          options = ((float[]) source).Select(i => new GUIContent(i.ToString())).ToArray();
        }
        else {
          options = ((string[]) source).Select(o => new GUIContent(o)).ToArray();
        }
      }

      // we want to still support int[] -> int[] and float[] -> float[] mapping
      if (property.type == "int" && !numericPopup) {
        selectedIndex = property.intValue;
      } else if (numericPopup) {
        if (property.type == "int") {
          selectedIndex = options.ToList().FindIndex(i => i.text == property.intValue.ToString()); 
        }
        else {
          selectedIndex = options.ToList().FindIndex(i => i.text == property.floatValue.ToString()); 
        }
        if (selectedIndex >= options.Length || selectedIndex < 0) {
          selectedIndex = 0;
        }
      }
      else {
        selectedIndex = options.ToList().FindIndex(i => i.text == property.stringValue);
        if (selectedIndex >= options.Length || selectedIndex < 0) {
          selectedIndex = 0;
        }
      }
      
      var finalLabel = hideLabel || property.name == "data" ? new GUIContent() : new GUIContent(property.displayName);
      selectedIndex = EditorGUILayout.Popup(finalLabel, selectedIndex, options);
      if (property.type == "int" && !numericPopup) {
        property.intValue = selectedIndex;
      } else if (numericPopup) {
        if (property.type == "int") {
          property.intValue = Convert.ToInt32(options[selectedIndex].text);  
        }
        else {
          property.floatValue = Convert.ToSingle(options[selectedIndex].text); 
        }
      }
      else {
        property.stringValue = options[selectedIndex].text;
      }
    }
  }

  /// <summary>
  /// Draws combined list of two arrays. Helps maintain same length between the two and associate values of one array with another.
  /// <a href="https://ut.orels.sh/attributes/attributes-list#listview">See More</a>
  /// </summary>
  public class ListViewAttribute : UTVisualAttribute {
    public readonly string name;
    public readonly string addMethodName;
    public string addButtonText = "Add Element";

    /// <summary>
    /// Draws combined list of two arrays. Helps maintain same length between the two and associate values of one array with another.
    /// <a href="https://github.com/orels1/UdonToolkit/wiki/Attributes#listview">See More</a>
    /// </summary>
    /// <param name="name">Name of the ListView (must be unique)</param>
    public ListViewAttribute(string name) {
      this.name = name;
    }

    /// <summary>
    /// Draws combined list of two arrays. Helps maintain same length between the two and associate values of one array with another.
    /// <a href="https://github.com/orels1/UdonToolkit/wiki/Attributes#listview">See More</a>
    /// </summary>
    /// <param name="name">Name of the ListView (must be unique)</param>
    /// <param name="addMethodName">Method to call when user tries to add a new value</param>
    public ListViewAttribute(string name, string addMethodName) {
      this.name = name;
      this.addMethodName = addMethodName;
    }

    /// <summary>
    /// Draws combined list of two arrays. Helps maintain same length between the two and associate values of one array with another.
    /// <a href="https://ut.orels.sh/attributes/attributes-list#listview">See More</a>
    /// </summary>
    /// <param name="name">Name of the ListView (must be unique)</param>
    /// <param name="addMethodName">Method to call when user tries to add a new value</param>
    /// <param name="addButtonText">Text to display on the Add button</param>
    public ListViewAttribute(string name, string addMethodName, string addButtonText) {
      this.name = name;
      this.addMethodName = addMethodName;
      this.addButtonText = addButtonText;
    }
  }

  /// <summary>
  /// Specifies a custom column title for a list view.
  /// Has no effect outside of a listview
  /// </summary>
  public class LVHeaderAttribute : UTVisualAttribute {
    public readonly string title;

    /// <summary>
    /// Specifies a custom column title for a list view.
    /// Has no effect outside of a listview
    /// </summary>
    /// <param name="title">The title to use</param>
    public LVHeaderAttribute(string title) {
      this.title = title;
    }
  }
  
  /// <summary>
  /// Makes the fields read only in the inspector
  /// </summary>
  [AttributeUsage(AttributeTargets.Field)]
  public class DisabledAttribute : UTPropertyAttribute {
    public readonly string methodName;
    /// <summary>
    /// Makes the fields read only in the inspector
    /// </summary>
    public DisabledAttribute() {
    }

    /// <summary>
    /// Makes the fields read only in the inspector if the provided method or value is true
    /// </summary>
    /// <param name="methodName"></param>
    public DisabledAttribute(string methodName) {
      this.methodName = methodName;
    }
  }

  /// <summary>
  /// Forces the label to display in places where is otherwise hidden
  /// </summary>
  [AttributeUsage(AttributeTargets.Field)]
  public class ShowLabelAttribute : Attribute {
    public readonly string label;
    /// <summary>
    /// Forces the label to display in places where is otherwise hidden
    /// </summary>
    public ShowLabelAttribute() {
    }

    /// <summary>
    /// Forces the label to display in places where is otherwise hidden
    /// </summary>
    /// <param name="label">Custom label text</param>
    public ShowLabelAttribute(string label) {
      this.label = label;
    }
  }

  public class FoldoutGroupAttribute : UTVisualAttribute {
    public readonly string name;

    public FoldoutGroupAttribute(string name) {
      this.name = name;
    }
  }
  
  public class TabGroupAttribute : UTVisualAttribute {
    public readonly string name;
    public readonly string variableName;

    public TabGroupAttribute(string name) {
      this.name = name;
    }
    
    public TabGroupAttribute(string name, string variableName) {
      this.name = name;
      this.variableName = variableName;
    }
  }
  
  /// <summary>
  /// Legacy attribute you do not need to use since v1.0
  /// </summary>
  [AttributeUsage(AttributeTargets.Field)]
  public class UTEditorAttribute : Attribute {}
  

  /// <summary>
  /// Displays a help message above all the fields
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class HelpMessageAttribute : Attribute {
    public readonly string helpMessage;

    /// <summary>
    /// Displays a help message above all the fields
    /// </summary>
    /// <param name="message">Text to display</param>
    public HelpMessageAttribute(string message) {
      helpMessage = message;
    }
  }
  
  /// <summary>
  /// Draws a custom header above all the fields
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class CustomNameAttribute : Attribute {
    public readonly string name;

    /// <summary>
    /// Draws a custom header above all the fields
    /// </summary>
    /// <param name="value">Text to display</param>
    public CustomNameAttribute(string value) {
      name = value;
    }
  }
  
  /// <summary>
  /// Calls the provided method before all the editor code. Runs every editor update
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class OnBeforeEditorAttribute : Attribute {
    public readonly string methodName;

    /// <summary>
    /// Calls the provided method before all the editor code. Runs every editor update
    /// </summary>
    /// <param name="methodName"></param>
    public OnBeforeEditorAttribute(string methodName) {
      this.methodName = methodName;
    }
  }
  
  /// <summary>
  /// Calls the provided method after all the editor code. Runs every editor update
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class OnAfterEditorAttribute : Attribute {
    public readonly string methodName;
    
    /// <summary>
    /// Calls the provided method after all the editor code. Runs every editor update
    /// </summary>
    /// <param name="methodName"></param>
    public OnAfterEditorAttribute(string methodName) {
      this.methodName = methodName;
    }
  }
  
  /// <summary>
  /// Calls the provided method if any values were changed in the inspector
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class OnValuesChangedAttribute : Attribute {
    public readonly string methodName;
    
    /// <summary>
    /// Calls the provided method if any values were changed in the inspector
    /// </summary>
    /// <param name="methodName"></param>
    public OnValuesChangedAttribute(string methodName) {
      this.methodName = methodName;
    }
  }

  /// <summary>
  /// Draws a button below all the fields that calls the method in play mode
  /// </summary>
  [AttributeUsage(AttributeTargets.Method)]
  public class ButtonAttribute : Attribute {
    public readonly string text;
    public readonly bool activeInEditMode;

    /// <summary>
    /// Draws a button below all the fields that calls the method in play mode
    /// </summary>
    /// <param name="text">Text to display</param>
    public ButtonAttribute(string text) {
      this.text = text;
    }

    /// <summary>
    /// Draws a button below all the fields that calls the method in edit mode
    /// </summary>
    /// <param name="text">Text to display</param>
    /// <param name="activeInEditMode">Specifies if button should be active in edit mode</param>
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
    public ListViewAttribute(string a) {
    }
    
    public ListViewAttribute(string a, string b) {
    }
    
    public ListViewAttribute(string a, string b, string c) {
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

    public ToggleAttribute(object a) {
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

    public HorizontalAttribute(object a, object b) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class HideLabelAttribute: Attribute {
    public HideLabelAttribute(){
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class FoldoutGroupAttribute : Attribute {
    public FoldoutGroupAttribute(object a) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class TabGroupAttribute : Attribute {
    public TabGroupAttribute(object a) {
    }

    public TabGroupAttribute(object a, object b) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class LVHeaderAttribute : Attribute {
    public LVHeaderAttribute(object a) {
    }
  }

  [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
  public class DisabledAttribute : Attribute {
    public DisabledAttribute() {
    }
    public DisabledAttribute(object a) {
    }
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class OnBeforeEditor : Attribute {
    public OnBeforeEditor(object a) {
    }
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class OnAfterEditor : Attribute {
    public OnAfterEditor(object a) {
    }
  }

  [AttributeUsage(AttributeTargets.Class)]
  public class OnValuesChanged : Attribute {
    public OnValuesChanged(object a) {
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
