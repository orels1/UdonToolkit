using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
#endif

namespace UdonToolkit {
  [CustomName("UI Readout")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#ui-readout")]
  public class UIReadout : UdonSharpBehaviour {
    [SectionHeader("General")] [UTEditor]
    public bool active;
    public UdonBehaviour source;
    
    [SectionHeader("Text Readouts")]
    [ListView("Text Readouts List", "AddTextVar")]
    [Popup("method", "GetTextVariableOptions", true)][UTEditor]
    public string[] textVariableNames;
    [ListView("Text Readouts List")] [UTEditor]
    public Text[] textVariableTargets;
    
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    #region ToolkitSTuff
    public void AddTextVar() {
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
    
    public void AddSliderVar() {
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
    
    [HelpBox("Each element of Text Readout Format corresponds to a Text Readout from the list above, make sure both lists are the same length or this will crash.\n{0} will be replaced by the value, e.g. \"Speed: {0} u/s\" will become \"Speed: 10 u/s\" if variable value is 10.")]
    [UTEditor]
    public string[] textReadoutFormats;
    
    [SectionHeader("Slider Readouts")]
    [ListView("Slider Readouts List", "AddSliderVar")]
    [Popup("method", "GetSliderVariableOptions", true)] [UTEditor]
    public string[] sliderVariableNames;
    [ListView("Slider Readouts List")] [UTEditor]
    public Slider[] sliderVariableTargets;
    [HelpBox("Each element of Slider Readout Multiplier corresponds to a Slider Readout from the list above, make sure both lists are the same length or this will crash.")]
    [UTEditor] 
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