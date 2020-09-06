using System;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Skybox Adjustment")]
  [HelpMessage(
    "This component expects a \"Trigger\" event to transition between default and active skybox materials. " +
    "This object can be kept disabled if using instant transition, it will still work as expected.")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#skybox-adjustment")]
  public class SkyboxAdjustment : UdonSharpBehaviour {
    [SectionHeader("Default State")] [UTEditor]
    public Material defaultSkybox;
    
    [ListView("Default Floats List")]
    [Popup("shader", "@defaultSkybox", true)] [UTEditor]
    public string[] defaultFloatNames;
    [ListView("Default Floats List")] [UTEditor]
    public float[] defaultFloatValues;
    
    [ListView("Default Colors List")]
    [Popup("shader", "@defaultSkybox", "color",  true)] [UTEditor]
    public string[] defaultColorNames;
    [ListView("Default Colors List")] [UTEditor]
    public Color[] defaultColorValues;
    
    [ListView("Default Vectors List")]
    [Popup("shader", "@defaultSkybox", "vector",  true)] [UTEditor]
    public string[] defaultVector3Names;
    [ListView("Default Vectors List")][UTEditor]
    public Vector3[] defaultVector3Values;

    [SectionHeader("Active State")] [UTEditor]
    public Material activeSkybox;
    
    [ListView("Active Floats List")]
    [Popup("shader", "@activeSkybox", true)] [UTEditor]
    public string[] activeFloatNames;
    [ListView("Active Floats List")][UTEditor]
    public float[] activeFloatValues;
    
    [ListView("Active Colors List")]
    [Popup("shader", "@activeSkybox", "color",  true)] [UTEditor]
    public string[] activeColorNames;
    [ListView("Active Colors List")] [UTEditor]
    public Color[] activeColorValues;
    
    [ListView("Active Vectors List")]
    [Popup("shader", "@activeSkybox", "vector",  true)] [UTEditor]
    public string[] activeVector3Names;
    [ListView("Active Vectors List")] [UTEditor]
    public Vector3[] activeVector3Values;

    [OnValueChanged("ToggleSelf")]
    [SectionHeader("Transition")][UTEditor]
    public bool instantTransition = true;
    
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    private void ToggleSelf(SerializedObject obj, SerializedProperty prop) {
      var val = prop.boolValue;
      if (!val) {
        obj.FindProperty("activeSkybox").objectReferenceValue = defaultSkybox;
        obj.ApplyModifiedProperties();
      }
    }
    #endif
    
    [HelpBox("Transition only applies to material properties, as smooth transition between materials is not possible. Active Skybox material field is set to be the same as default.", "@!instantTransition")]
    [HelpBox("Transition time cannot be negative", "CheckValidTransition")]
    [UTEditor]
    public float transitionTime;
    
    private bool CheckValidTransition() {
      return transitionTime < 0;
    }

    private bool active;
    private bool lerping;
    private float lerpEnd;

    public void Trigger() {
      if (instantTransition || transitionTime <= 0) {
        RenderSettings.skybox = active ? defaultSkybox : activeSkybox;
        SetFinalValues();
        active = !active;
        return;
      }

      lerping = true;
      lerpEnd = Time.time + transitionTime;
    }

    private void LerpValues(float alpha) {
      var mat = defaultSkybox;
      var sFloatNames = !active ? defaultFloatNames : activeFloatNames;
      var sFloatVals = !active ? defaultFloatValues : activeFloatValues;
      var sColorNames = !active ? defaultColorNames : activeColorNames;
      var sColorVals = !active ? defaultColorValues : activeColorValues;
      var sVectorNames = !active ? defaultVector3Names : activeVector3Names;
      var sVectorVals = !active ? defaultVector3Values : activeVector3Values;

      var eFloatVals = active ? defaultFloatValues : activeFloatValues;
      var eColorVals = active ? defaultColorValues : activeColorValues;
      var eVectorVals = active ? defaultVector3Values : activeVector3Values;

      for (int i = 0; i < sFloatNames.Length; i++) {
        mat.SetFloat(sFloatNames[i], Mathf.Lerp(sFloatVals[i], eFloatVals[i], alpha));
      }

      for (int i = 0; i < sColorNames.Length; i++) {
        mat.SetColor(sColorNames[i], Color.Lerp(sColorVals[i], eColorVals[i], alpha));
      }

      for (int i = 0; i < sVectorNames.Length; i++) {
        mat.SetVector(sVectorNames[i], Vector3.Lerp(sVectorVals[i], eVectorVals[i], alpha));
      }
    }

    private void SetFinalValues() {
      var floatNames = active ? defaultFloatNames : activeFloatNames;
      var floatVals = active ? defaultFloatValues : activeFloatValues;
      var colorNames = active ? defaultColorNames : activeColorNames;
      var colorVals = active ? defaultColorValues : activeColorValues;
      var vectorNames = active ? defaultVector3Names : activeVector3Names;
      var vectorVals = active ? defaultVector3Values : activeVector3Values;
      var mat = active ? defaultSkybox : activeSkybox;
      for (int i = 0; i < floatNames.Length; i++) {
        mat.SetFloat(floatNames[i], floatVals[i]);
      }

      for (int i = 0; i < colorNames.Length; i++) {
        mat.SetColor(colorNames[i], colorVals[i]);
      }

      for (int i = 0; i < vectorNames.Length; i++) {
        mat.SetVector(vectorNames[i], vectorVals[i]);
      }
    }

    private void Update() {
      if (!lerping) return;
      if (Time.time >= lerpEnd) {
        SetFinalValues();
        lerping = false;
        active = !active;
        return;
      }

      var lerpAlpha = (Time.time - (lerpEnd - transitionTime)) / transitionTime;
      LerpValues(lerpAlpha);
    }
  }
}