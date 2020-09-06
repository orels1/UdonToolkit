#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomName("Skybox Adjustment")]
  [HelpMessage("This component expects a \"Trigger\" event to transition between default and active skybox materials. " +
               "This object can be kept disabled if using instant transition, it will still work as expected.")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#skybox-adjustment")]
  public class SkyboxAdjustmentController : UTController {
    [SectionHeader("Default State")] [UdonPublic]
    public Material defaultSkybox;
    
    private string[] getShaderPropsByType(List<ShaderUtil.ShaderPropertyType> valid, Material source) {
      if (source == null) {
        return new[] {"-- no skybox mat --"};
      }
      var shader = source.shader;
      var res = new List<string>();
      for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++) {
        var type = ShaderUtil.GetPropertyType(shader, i);
        if (valid.Contains(type)) {
          res.Add(ShaderUtil.GetPropertyName(shader, i));
        }
      }
      
      return res.Count == 0 ? new[] {"no valid properties"} : res.ToArray();
    }

    [ListView("Default Floats List")] [Popup(PopupAttribute.PopupSource.Method, "GetDefaultFloatOptions", true)] [UdonPublic]
    public string[] defaultFloatNames;

    public string[] GetDefaultFloatOptions() {
      return getShaderPropsByType(
        new List<ShaderUtil.ShaderPropertyType>
          {ShaderUtil.ShaderPropertyType.Float, ShaderUtil.ShaderPropertyType.Range},
        defaultSkybox);
    }

    [ListView("Default Floats List")] [UdonPublic]
    public float[] defaultFloatValues;

    [ListView("Default Color List")] [Popup(PopupAttribute.PopupSource.Method, "GetDefaultColorOptions", true)] [UdonPublic]
    public string[] defaultColorNames;

    public string[] GetDefaultColorOptions() {
      return getShaderPropsByType(
        new List<ShaderUtil.ShaderPropertyType>
          {ShaderUtil.ShaderPropertyType.Color},
        defaultSkybox);
    }

    [ListView("Default Color List")] [UdonPublic]
    public Color[] defaultColorValues;
    
    [ListView("Default Vector3 List")] [Popup(PopupAttribute.PopupSource.Method, "GetDefaultVectorOptions", true)]  [UdonPublic]
    public string[] defaultVector3Names;
    
    public string[] GetDefaultVectorOptions() {
      return getShaderPropsByType(
        new List<ShaderUtil.ShaderPropertyType>
          {ShaderUtil.ShaderPropertyType.Vector},
        defaultSkybox);
    }

    [ListView("Default Vector3 List")] [UdonPublic]
    public Vector3[] defaultVector3Values;

    [SectionHeader("Active State")] [UdonPublic]
    public Material activeSkybox;
    
    [ListView("Active Floats List")] [Popup(PopupAttribute.PopupSource.Method, "GetActiveFloatOptions", true)] [UdonPublic]
    public string[] activeFloatNames;

    public string[] GetActiveFloatOptions() {
      return getShaderPropsByType(
        new List<ShaderUtil.ShaderPropertyType>
          {ShaderUtil.ShaderPropertyType.Float, ShaderUtil.ShaderPropertyType.Range},
        activeSkybox);
    }

    [ListView("Active Floats List")] [UdonPublic]
    public float[] activeFloatValues;

    [ListView("Active Color List")] [Popup(PopupAttribute.PopupSource.Method, "GetActiveColorOptions", true)] [UdonPublic]
    public string[] activeColorNames;

    public string[] GetActiveColorOptions() {
      return getShaderPropsByType(
        new List<ShaderUtil.ShaderPropertyType>
          {ShaderUtil.ShaderPropertyType.Color},
        activeSkybox);
    }

    [ListView("Active Color List")] [UdonPublic]
    public Color[] activeColorValues;
    
    [ListView("Active Vector3 List")] [Popup(PopupAttribute.PopupSource.Method, "GetActiveVectorOptions", true)]  [UdonPublic]
    public string[] activeVector3Names;
    
    public string[] GetActiveVectorOptions() {
      return getShaderPropsByType(
        new List<ShaderUtil.ShaderPropertyType>
          {ShaderUtil.ShaderPropertyType.Vector},
        activeSkybox);
    }

    [ListView("Active Vector3 List")] [UdonPublic]
    public Vector3[] activeVector3Values;
    
    [OnValueChanged("ToggleSelf")]
    [SectionHeader("Transition")][UdonPublic] public bool instantTransition = true;

    private Material oldSkybox;
    public void ToggleSelf(object value) {
      var val = value is bool b && b;
      gameObject.SetActive(!val);
      activeSkybox = defaultSkybox;
    }

    [HelpBox("Transition only applies to material properties, as smooth transition between materials is not possible. Active Skybox material field is set to be the same as default.", "@!instantTransition")]
    [HelpBox("Transition time cannot be negative", "CheckValidTransition")]
    [UdonPublic] public float transitionTime;

    public bool CheckValidTransition() {
      return transitionTime < 0;
    }
    
    public override void SetupController() {
      if (defaultSkybox != null) return;
      defaultSkybox = RenderSettings.skybox;
    }

    [Button("Trigger")]
    public void Trigger() {
      if (uB != null) {
        uB.SendCustomEvent("Trigger");
      }
    }
  }
}

#endif