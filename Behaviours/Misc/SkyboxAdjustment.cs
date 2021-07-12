using System;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Skybox Adjustment")]
  [HelpMessage(
    "This component expects a \"Trigger\" event to transition between current and active skybox material values. You can  ")]
  [HelpURL("https://ut.orels.sh/v/v1.x/behaviours/misc-behaviours#skybox-adjustment")]
  [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
  public class SkyboxAdjustment : UdonSharpBehaviour {
    [SectionHeader("Active State")] public Material activeSkybox;

    [ListView("Active Floats List")] [Popup("shader", "@activeSkybox", true)]
    public string[] activeFloatNames;

    [ListView("Active Floats List")] public float[] activeFloatValues;

    [ListView("Active Colors List")] [Popup("shader", "@activeSkybox", "color", true)]
    public string[] activeColorNames;

    [ListView("Active Colors List")] public Color[] activeColorValues;

    [ListView("Active Vectors List")] [Popup("shader", "@activeSkybox", "vector", true)]
    public string[] activeVector3Names;

    [ListView("Active Vectors List")] public Vector3[] activeVector3Values;

    [OnValueChanged("ToggleSelf")] [SectionHeader("Transition")]
    public bool instantTransition = true;

    [HelpBox("Transition time cannot be negative", "CheckValidTransition")]
    public float transitionTime;

    private bool CheckValidTransition() {
      return transitionTime < 0;
    }

    private bool active;
    private bool lerping;
    private float lerpEnd;

    private string[] sFloatNames;
    private float[] sFloatVals;
    private string[] sColorNames;
    private Color[] sColorVals;
    private string[] sVectorNames;
    private Vector3[] sVectorVals;

    private string[] eFloatNames;
    private float[] eFloatVals;
    private string[] eColorNames;
    private Color[] eColorVals;
    private string[] eVectorNames;
    private Vector3[] eVectorVals;

    public void Trigger() {
      if (instantTransition || transitionTime <= 0) {
        RenderSettings.skybox = activeSkybox;
        SetFinalValues();
        return;
      }

      eFloatNames = activeFloatNames;
      eFloatVals = activeFloatValues;
      eColorNames = activeColorNames;
      eColorVals = activeColorValues;
      eVectorNames = activeVector3Names;
      eVectorVals = activeVector3Values;

      sFloatVals = new float[eFloatVals.Length];
      for (int i = 0; i < eFloatNames.Length; i++) {
        sFloatVals[i] = activeSkybox.GetFloat(eFloatNames[i]);
      }

      sColorVals = new Color[eColorVals.Length];
      for (int i = 0; i < eColorNames.Length; i++) {
        sColorVals[i] = activeSkybox.GetColor(eColorNames[i]);
      }

      sVectorVals = new Vector3[eVectorVals.Length];
      for (int i = 0; i < eVectorNames.Length; i++) {
        sVectorVals[i] = activeSkybox.GetVector(eVectorNames[i]);
      }

      lerping = true;
      lerpEnd = Time.time + transitionTime;
    }

    private void LerpValues(float alpha) {
      RenderSettings.skybox = activeSkybox;
      var mat = activeSkybox;
      for (int i = 0; i < eFloatNames.Length; i++) {
        mat.SetFloat(eFloatNames[i], Mathf.Lerp(sFloatVals[i], eFloatVals[i], alpha));
      }

      for (int i = 0; i < eColorNames.Length; i++) {
        mat.SetColor(eColorNames[i], Color.Lerp(sColorVals[i], eColorVals[i], alpha));
      }

      for (int i = 0; i < eVectorNames.Length; i++) {
        mat.SetVector(eVectorNames[i], Vector3.Lerp(sVectorVals[i], eVectorVals[i], alpha));
      }
    }

    private void SetFinalValues() {
      var floatNames = activeFloatNames;
      var floatVals = activeFloatValues;
      var colorNames = activeColorNames;
      var colorVals = activeColorValues;
      var vectorNames = activeVector3Names;
      var vectorVals = activeVector3Values;
      var mat = activeSkybox;
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
        return;
      }

      var lerpAlpha = (Time.time - (lerpEnd - transitionTime)) / transitionTime;
      LerpValues(lerpAlpha);
    }
  }
}
