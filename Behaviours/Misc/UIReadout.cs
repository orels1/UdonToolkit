using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

#endif

namespace UdonToolkit {
  [CustomName("UI Readout")]
  [HelpURL("https://ut.orels.sh/v/v1.x/behaviours/misc-behaviours#ui-readout")]
  [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
  public class UIReadout : UdonSharpBehaviour {
    [SectionHeader("General")] 
    public bool active;
    public UdonBehaviour source;
    
    [SectionHeader("Text Readouts")]
    [HelpBox("{0} will be replaced by the value, e.g. \"Speed: {0} u/s\" will become \"Speed: 10 u/s\" if variable value is 10.")]
    [ListView("Text Readouts List", "AddTextVar")] [LVHeader("Text Variables")]
    [Popup("method", "GetTextVariableOptions", true)]
    public string[] textVariableNames;
    [ListView("Text Readouts List")] [LVHeader("Targets")]
    public Text[] textVariableTargets;
    [ListView("Text Readouts List")] [LVHeader("Formats")]
    public string[] textReadoutFormats;
    
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    #region ToolkitSTuff
    public void AddTextVar(SerializedObject obj) {
      var newNames = textVariableNames.ToList();
      newNames.Add(GetTextVariableOptions()[0]);
      textVariableNames = newNames.ToArray();

      var newTargets = textVariableTargets.ToList();
      newTargets.Add(null);
      textVariableTargets = newTargets.ToArray();

      if (textReadoutFormats == null) {
        textReadoutFormats = new[] {"{0}"};
        return;
      }
      var newFormats = textReadoutFormats.ToList();
      newFormats.Add("{0}");
      textReadoutFormats = newFormats.ToArray();
    }
    
    public void AddSliderVar(SerializedObject obj) {
      var newNames = sliderVariableNames.ToList();
      newNames.Add(GetSliderVariableOptions()[0]);
      sliderVariableNames = newNames.ToArray();

      var newTargets = sliderVariableTargets.ToList();
      newTargets.Add(null);
      sliderVariableTargets = newTargets.ToArray();

      if (sliderReadoutMultipliers == null) {
        sliderReadoutMultipliers = new[] {1f};
        return;
      }
      var newFormats = sliderReadoutMultipliers.ToList();
      newFormats.Add(1);
      sliderReadoutMultipliers = newFormats.ToArray();
    }
    
    public string[] GetTextVariableOptions() {
      if (source == null) return new[] {"no source set"};
      var vars = source.publicVariables.VariableSymbols.ToArray();
      var filtered = new List<string>();
      foreach (var v in vars) {
        if (!source.publicVariables.TryGetVariableType(v, out var type)) continue;
        if (!type.IsArray && type.GetMembers().Count(i => i.Name == "ToString") != 0) {
          filtered.Add(v);
        }
      }
      return filtered.Any() ? filtered.ToArray() : new [] {"no compatible variables"};
    }
    
    public string[] GetSliderVariableOptions() {
      if (source == null) return new[] {"no source set"};
      var vars = source.publicVariables.VariableSymbols.ToArray();
      var filtered = new List<string>();
      foreach (var v in vars) {
        if (!source.publicVariables.TryGetVariableType(v, out var type)) continue;
        if (type.IsArray || type == typeof (bool) || type.IsEnum) continue;
        if (!source.publicVariables.TryGetVariableValue(v, out var val)) continue;
        try {
          Convert.ToSingle(val);
          filtered.Add(v);
        }
        catch {
          // ignored
        }
      }
      return filtered.Any() ? filtered.ToArray() : new [] {"no compatible variables"};
    }
    #endregion
    #endif

    [SectionHeader("Slider Readouts")]
    [ListView("Slider Readouts List", "AddSliderVar")] [LVHeader("Slider Variables")]
    [Popup("method", "GetSliderVariableOptions", true)] 
    public string[] sliderVariableNames;
    [ListView("Slider Readouts List")] [LVHeader("Targets")]
    public Slider[] sliderVariableTargets;
    [ListView("Slider Readouts List")] [LVHeader("Multipliers")]
    public float[] sliderReadoutMultipliers;

    private void LateUpdate() {
      if (!active) return;
      for (int i = 0; i < textVariableNames.Length; i++) {
        var val = source.GetProgramVariable(textVariableNames[i]).ToString();
        textVariableTargets[i].text = textReadoutFormats[i].Replace("{0}", val);
      }

      for (int i = 0; i < sliderVariableNames.Length; i++) {
        var val = source.GetProgramVariable(sliderVariableNames[i]);
        sliderVariableTargets[i].value = Convert.ToSingle(val) * sliderReadoutMultipliers[i];
      }
    }

    public void Activate() {
      active = true;
    }

    public void Deactivate() {
      active = false;
    }

    public void Toggle() {
      active = !active;
    }
  }
}
