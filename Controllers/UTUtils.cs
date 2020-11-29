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
                               | BindingFlags.IgnoreCase
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

    public static string[] GetAnimatorTriggers(Animator animator) {
      if (animator == null) return new[] {"no triggers found"};
      if (animator.runtimeAnimatorController != null) {
        if (animator.GetCurrentAnimatorStateInfo(0).length == 0) {
          animator.enabled = false;
          animator.enabled = true;
          animator.gameObject.SetActive(true);
        }
        var found = animator.parameters.Where(p => p.type == AnimatorControllerParameterType.Trigger)
          .Select(x => x.name).ToArray();
        if (found.Length > 0) {
          return found;
        }
      }
      return new[] {"no triggers found"};
    }

    private static string[] BLACKLISTED_EVENT_NAMES = new[] {
      "Update", "LateUpdate", "FixedUpdate", "Start", "OnEnable", "OnDisable", "OnTriggerEnter", "OnTriggerStay",
      "OnTriggerExit", "OnPlayerTriggerEnter", "OnPlayerTriggerStay", "OnPlayerTriggerExit", "Interact"
    };

    public static string[] GetUdonEvents(UdonSharpBehaviour source) {
      var events = new[] {"no events found"};
      if (source != null) {
        var uPa = UdonSharpEditorUtility.GetUdonSharpProgramAsset(source);
        if (uPa != null) {
          var methods = uPa.sourceCsScript.GetClass().GetMethods();
          var mapped = methods.Where(m => m.Module.Name == "Assembly-CSharp.dll").Select(m => m.Name).ToArray();
          mapped = mapped.Where(m => !BLACKLISTED_EVENT_NAMES.Contains(m)).ToArray();
          if (mapped.Length > 0) {
            events = mapped;
          }
        }
      }
      return events;
    }
    
    public static string[] GetUdonEvents(UdonBehaviour source) {
      var events = new[] {"no events found"};
      if (source != null) {
        var uPa = source.programSource as UdonSharpProgramAsset;
        if (uPa != null) {
          var methods = uPa.sourceCsScript.GetClass().GetMethods();
          var mapped = methods.Where(m => m.Module.Name == "Assembly-CSharp.dll").Select(m => m.Name).ToArray();
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

    public static string[] GetPopupOptions(SerializedProperty prop, SerializedProperty fetchFrom, PopupAttribute popup, out int index) {
      var sourceType = popup.sourceType;
      var source = fetchFrom ?? prop;
      string[] options;
      if (sourceType == PopupAttribute.PopupSource.Animator) {
        options = GetAnimatorTriggers(source.objectReferenceValue as Animator);
      }
      else if (sourceType == PopupAttribute.PopupSource.UdonBehaviour) {
        options = GetUdonEvents(source.objectReferenceValue as UdonSharpBehaviour);
      }
      else if (sourceType == PopupAttribute.PopupSource.Shader) {
        var propsSource = GetValueThroughAttribute(source, popup.methodName, out _);
        options = GetShaderPropertiesByType(propsSource, popup.shaderPropType);
      }
      else {
        options = (string[]) GetValueThroughAttribute(source, popup.methodName, out _);
      }

      if (options.Length == 0) {
        index = 0;
        return new[] {"-- no options provided --"};
      }
      index = options.ToList().IndexOf(prop.stringValue);
      if (index >= options.Length || index == -1) {
        index = 0;
      }

      return options;
    }
  }
}

#endif