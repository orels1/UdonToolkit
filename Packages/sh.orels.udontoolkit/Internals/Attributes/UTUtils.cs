#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace UdonToolkit {
  public static class UTUtils {
    public static BindingFlags flags = BindingFlags.GetField
                               | BindingFlags.GetProperty
                               | BindingFlags.Instance
                               | BindingFlags.NonPublic
                               | BindingFlags.Public;
    public static GameObject CreateObjectWithComponents(GameObject target, string name, Type[] components) {
      // check if object exists
      var obj = target.transform.Find(name).gameObject;
      if (obj != null) {
        // check if component exists on it
        foreach (var component in components) {
          if (obj.GetComponent(component) == null) {
            obj.AddComponent(component);
          }
        }

        return obj;
      }
      obj = new GameObject(name, components);
      obj.transform.SetParent(target.transform);
      obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
      return obj;
    }
    
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)       
      => self.Select((item, index) => (item, index));
    
    public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
    {
      T tmp = list[indexA];
      list[indexA] = list[indexB];
      list[indexB] = tmp;
      return list;
    }

    public static SerializedProperty GetPropThroughAttribute(SerializedObject obj, string methodName) {
      if (!methodName.StartsWith("@")) return null;
      if (methodName.IndexOf("!") > -1) return null;
      var methodActual = methodName.Substring(1);
      var prop = obj.FindProperty(methodActual);
      return prop;
    }

    public static object GetValueThroughAttribute(SerializedProperty property, string methodName, out Type type) {
      if (methodName.StartsWith("@")) {
        var startIndex = 1;
        if (methodName.IndexOf("!") > -1) startIndex = 2;
        var methodActual = methodName.Substring(startIndex);
        var val = property.serializedObject.targetObject.GetType().GetField(methodActual, flags);
        type = val.FieldType.GetElementType();
        return val.GetValue(property.serializedObject.targetObject);
      }

      var method = property.serializedObject.targetObject.GetType().GetMethod(methodName, flags);
      type = method.GetReturnType().GetElementType();
      if (method == null) {
        return null;
      } 
      var paramsList = method.GetParameters();
      var argList = new object[]{};
      if (paramsList.Length > 0 && paramsList[0].ParameterType == typeof(SerializedProperty)) {
        argList = new object[] { property };
      }
      return method.Invoke(property.serializedObject.targetObject, argList);
    }

    public static bool GetVisibleThroughAttribute(SerializedProperty property, string methodName, bool flipValue) {
      var isVisible = true;
      var value = GetValueThroughAttribute(property, methodName, out _);
      isVisible = (bool) value;
      if (methodName.StartsWith("@") && methodName.IndexOf("!") > -1) {
        isVisible = !isVisible;
      }
      isVisible = flipValue ? !isVisible : isVisible;
      return isVisible;
    }

    private static AnimatorControllerParameter[] GetAnimatorParams(Animator animator) {
      if (animator == null) return null;
      if (animator.runtimeAnimatorController != null) {
        if (animator.GetCurrentAnimatorStateInfo(0).length == 0) {
          animator.enabled = false;
          animator.enabled = true;
          animator.gameObject.SetActive(true);
        }

        return animator.parameters;
      }

      return null;
    }

    public static string[] GetAnimatorTriggers(Animator animator) {
      var found = GetAnimatorParams(animator);
      if (found == null) {
        return new[] {"no triggers found"};
      }
      var filtered = found.Where(p => p.type == AnimatorControllerParameterType.Trigger)
        .Select(x => x.name).ToArray();
      if (filtered.Length > 0) {
        return filtered;
      }
      return new[] {"no triggers found"};
    }
    
    public static string[] GetAnimatorBools(Animator animator) {
      var found = GetAnimatorParams(animator);
      if (found == null) {
        return new[] {"no bools found"};
      }
      var filtered = found.Where(p => p.type == AnimatorControllerParameterType.Bool)
        .Select(x => x.name).ToArray();
      if (filtered.Length > 0) {
        return filtered;
      }
      return new[] {"no bools found"};
    }
    
    public static string[] GetAnimatorFloats(Animator animator) {
      var found = GetAnimatorParams(animator);
      if (found == null) {
        return new[] {"no floats found"};
      }
      var filtered = found.Where(p => p.type == AnimatorControllerParameterType.Float)
        .Select(x => x.name).ToArray();
      if (filtered.Length > 0) {
        return filtered;
      }
      return new[] {"no floats found"};
    }
    
    public static string[] GetAnimatorInts(Animator animator) {
      var found = GetAnimatorParams(animator);
      if (found == null) {
        return new[] {"no ints found"};
      }
      var filtered = found.Where(p => p.type == AnimatorControllerParameterType.Int)
        .Select(x => x.name).ToArray();
      if (filtered.Length > 0) {
        return filtered;
      }
      return new[] {"no ints found"};
    }

    private static string[] BLACKLISTED_EVENT_NAMES = new[] {
      "Update", "LateUpdate", "FixedUpdate", "Start", "OnEnable", "OnDisable", "OnTriggerEnter", "OnTriggerStay",
      "OnTriggerExit", "OnPlayerTriggerEnter", "OnPlayerTriggerStay", "OnPlayerTriggerExit", "Interact", "OnPlayerRespawn",
      "OnPickupUseDown", "OnPickupUseUp", "OnPlayerJoined", "OnPlayerLeft", "OnVideoEnd", "OnVideoError", "OnVideoLoop",
      "OnVideoPause", "OnVideoPlay", "OnVideoReady", "OnVideoStart", "OnSpawn", "OnStationEntered", "OnStationExited",
      "OnPreserialization", "OnDeserialization", "OnPlayerParticleCollision", "OnOwnershipTransferred", "OnPickup",
      "OnOwnershipRequest", "MidiNoteOn", "MidiNoteOff", "MidiControlChange", "InputJump", "InputUse", "InputGrab",
      "InputDrop", "InputMoveHorizontal", "InputMoveVertical", "InputLookHorizontal", "InputLookVertical"
    };
    
    private static string[] BLACKLISTED_MODULE_NAMES = new [] {
      "UdonSharp.Runtime.dll", "UnityEngine.CoreModule.dll", "mscorlib.dll",
    };

    public static string[] GetUdonEvents(UdonSharpBehaviour source) {
      var events = new[] {"no events found"};
      if (source != null) {
        var uPa = UdonSharpEditorUtility.GetUdonSharpProgramAsset(source);
        if (uPa != null) {
          var methods = uPa.sourceCsScript.GetClass().GetMethods();
          var mapped = methods.Where(m => !BLACKLISTED_MODULE_NAMES.Contains(m.Module.Name)).Select(m => m.Name).ToArray();
          mapped = mapped.Where(m => !BLACKLISTED_EVENT_NAMES.Contains(m)).ToArray();
          if (mapped.Length > 0) {
            events = mapped;
          }
        }
      }
      return events;
    }
    
    public static string[] GetUdonVariables(UdonSharpBehaviour source) {
      var variables = new[] {"no variables found"};
      if (source != null) {
        var uPa = UdonSharpEditorUtility.GetUdonSharpProgramAsset(source);
        if (uPa != null) {
          var fields = uPa.sourceCsScript.GetClass().GetFields();
          var mapped = fields.Where(f => !BLACKLISTED_MODULE_NAMES.Contains(f.Module.Name)).Select(f => f.Name).ToArray();
          if (mapped.Length > 0) {
            variables = mapped;
          }
        }
      }
      return variables;
    }
    
    public static string[] GetUdonEvents(UdonBehaviour source) {
      var events = new[] {"no events found"};
      if (source != null) {
        var uPa = source.programSource as UdonSharpProgramAsset;
        if (uPa != null) {
          var methods = uPa.sourceCsScript.GetClass().GetMethods();
          var mapped = methods.Where(m => !BLACKLISTED_MODULE_NAMES.Contains(m.Module.Name)).Select(m => m.Name).ToArray();
          if (mapped.Length > 0) {
            events = mapped;
          }
        }
      }
      return events;
    }
    
    public static T GetPropertyAttribute<T>(SerializedProperty prop) where T : Attribute {
      var attrs = GetPropertyAttributes<T>(prop);
      if (attrs.Length == 0) return null;
      return (T) attrs[0];
    }
    
    public static object[] GetPropertyAttributes(SerializedProperty prop) {
      return GetPropertyAttributes<PropertyAttribute>(prop);
    }

    public static object[] GetPropertyAttributes<T>(SerializedProperty prop) where T : Attribute {
      if (prop.serializedObject.targetObject == null) return null;
      var tType = prop.serializedObject.targetObject.GetType();
      var field = tType.GetField(prop.name, flags);
      return field != null ? field.GetCustomAttributes(typeof(T), true) : null;
    }
    
    private static Dictionary<int, int> masksByLayer;

    public static void GetLayerMasks() {
      masksByLayer = new Dictionary<int, int>();
      for (int i = 0; i < 32; i++) {
        int mask = 0;
        for (int j = 0; j < 32; j++) {
          if (!Physics.GetIgnoreLayerCollision(i, j)) {
            mask |= 1 << j;
          }
        }

        masksByLayer.Add(i, mask);
      }
    }

    public static int MaskForLayer(int layer) {
      return masksByLayer[layer];
    }

    public static void CreateLayer(string name) {
      var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
      var layerProps = tagManager.FindProperty("layers");
      var propCount = layerProps.arraySize;

      SerializedProperty firstEmptyProp = null;

      for (var i = 0; i < propCount; i++) {
        var layerProp = layerProps.GetArrayElementAtIndex(i);

        var stringValue = layerProp.stringValue;

        if (stringValue == name) return;

        if (i < 8 || stringValue != string.Empty) continue;

        if (firstEmptyProp == null)
          firstEmptyProp = layerProp;
      }

      if (firstEmptyProp == null) {
        UnityEngine.Debug.LogError("Maximum limit of " + propCount + " layers exceeded. Layer \"" + name +
                                   "\" not created.");
        return;
      }

      firstEmptyProp.stringValue = name;
      tagManager.ApplyModifiedProperties();
    }

    public static void CreateLayer(string name, int index) {
      var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
      var layerProps = tagManager.FindProperty("layers");

      var layerProp = layerProps.GetArrayElementAtIndex(index);

      var stringValue = layerProp.stringValue;

      if (stringValue == name) return;

      layerProp.stringValue = name;
      tagManager.ApplyModifiedProperties();
    }
    
    public static string[] GetLayerNames() {
      var layerList = new List<string>();
      for (int i = 0; i < 31; i++) {
        layerList.Add(LayerMask.LayerToName(i));
      }

      return layerList.ToArray();
    }  

    private static Dictionary<PopupAttribute.ShaderPropType, List<ShaderUtil.ShaderPropertyType>> propTypeMapping =
      new Dictionary<PopupAttribute.ShaderPropType, List<ShaderUtil.ShaderPropertyType>> {
        {
          PopupAttribute.ShaderPropType.Float,
          new List<ShaderUtil.ShaderPropertyType>
            {ShaderUtil.ShaderPropertyType.Float, ShaderUtil.ShaderPropertyType.Range}
        }, {
          PopupAttribute.ShaderPropType.Color,
          new List<ShaderUtil.ShaderPropertyType> {ShaderUtil.ShaderPropertyType.Color}
        }, {
          PopupAttribute.ShaderPropType.Vector,
          new List<ShaderUtil.ShaderPropertyType> {ShaderUtil.ShaderPropertyType.Vector}
        }
      };
    
    public static string[] GetShaderPropertiesByType(object source, PopupAttribute.ShaderPropType valid) {
      if (source == null) {
        return new[] {"-- no shader provided --"};
      }

      var convertedShader = source as Shader;
      var convertedMat = source as Material;
      var shader = convertedShader;
      if (convertedShader == null) {
        shader = convertedMat?.shader;
      }

      if (shader == null) {
        return new[] {"-- no shader provided --"};
      }

      var types = propTypeMapping[valid];
      var res = new List<string>();
      for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++) {
        var type = ShaderUtil.GetPropertyType(shader, i);
        if (types.Contains(type)) {
          res.Add(ShaderUtil.GetPropertyName(shader, i));
        }
      }
      
      return res.Count == 0 ? new[] {"-- no valid properties--"} : res.ToArray();
    }

    public static string[] GetAllShaderProperties(object source) {
      if (source == null) {
        return new[] {"-- no shader provided --"};
      }
      var props = new List<string>();
      var toFetch = new PopupAttribute.ShaderPropType[] {
        PopupAttribute.ShaderPropType.Float, PopupAttribute.ShaderPropType.Color, PopupAttribute.ShaderPropType.Vector
      };
      foreach (var valid in toFetch) {
        var list = GetShaderPropertiesByType(source, valid);
        if (list.Length == 1 && list[0] == "-- no valid properties --") {
          continue;
        }
        props.AddRange(list);
      }

      if (props.Count == 0) {
        return new[] {"-- no shader provided --"};
      }

      return props.ToArray();
    }

    public static string[] GetPopupOptions(SerializedProperty prop, SerializedProperty fetchFrom, PopupAttribute popup, out int index) {
      var sourceType = popup.sourceType;
      var source = fetchFrom ?? prop;
      string[] options;
      if (sourceType == PopupAttribute.PopupSource.AnimatorTrigger) {
        options = GetAnimatorTriggers(source.objectReferenceValue as Animator);
      }
      else if (sourceType == PopupAttribute.PopupSource.AnimatorBool) {
        options = GetAnimatorBools(source.objectReferenceValue as Animator);
      }
      else if (sourceType == PopupAttribute.PopupSource.AnimatorFloat) {
        options = GetAnimatorFloats(source.objectReferenceValue as Animator);
      }
      else if (sourceType == PopupAttribute.PopupSource.AnimatorInt) {
        options = GetAnimatorInts(source.objectReferenceValue as Animator);
      }
      else if (sourceType == PopupAttribute.PopupSource.UdonBehaviour) {
        options = GetUdonEvents(source.objectReferenceValue as UdonSharpBehaviour);
      }
      else if (sourceType == PopupAttribute.PopupSource.UdonProgramVariable) {
        options = GetUdonVariables(source.objectReferenceValue as UdonSharpBehaviour);
      }
      else if (sourceType == PopupAttribute.PopupSource.Shader) {
        var propsSource = GetValueThroughAttribute(source, popup.methodName, out _);
        if (popup.shaderPropType == PopupAttribute.ShaderPropType.All) {
          options = GetAllShaderProperties(propsSource);
        }
        else {
          options = GetShaderPropertiesByType(propsSource, popup.shaderPropType);
        }
      }
      else {
        options = (string[]) GetValueThroughAttribute(source, popup.methodName, out _);
      }

      if (options.Length == 0) {
        index = 0;
        return new[] {"-- no options provided --"};
      }

      if (prop.type == "int") {
        index = prop.intValue;
        return options;
      }

      index = options.ToList().IndexOf(prop.stringValue);
      if (index >= options.Length || index == -1) {
        index = 0;
      }

      return options;
    }

    public enum UTSettingType {
      String,
      Bool,
      Float,
      Int
    }

    public static object GetUTSetting(string key, UTSettingType type) {
      var prefixed = "UT_" + key;
      if (!EditorPrefs.HasKey(prefixed)) {
        return null;
      }

      switch (type) {
        case UTSettingType.String:
          return EditorPrefs.GetString(prefixed);
        case UTSettingType.Bool:
          return EditorPrefs.GetBool(prefixed);
        case UTSettingType.Float:
          return EditorPrefs.GetFloat(prefixed);
        case UTSettingType.Int:
          return EditorPrefs.GetInt(prefixed);
        default:
          return null;
      }
    }

    public static void SetUTSetting(string key, UTSettingType type, object value) {
      var prefixed = "UT_" + key;
      switch (type) {
        case UTSettingType.String:
          EditorPrefs.SetString(prefixed, value as string);
          break;
        case UTSettingType.Bool:
          EditorPrefs.SetBool(prefixed, (bool) value);
          break;
        case UTSettingType.Float:
          EditorPrefs.SetFloat(prefixed, (float) value);
          break;
        case UTSettingType.Int:
          EditorPrefs.SetInt(prefixed, (int) value);
          break;
      }
    }
  }
}

#endif
