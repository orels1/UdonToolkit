using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Shader Feeder")]
  [HelpMessage(
    "This is an advanced behaviour, its highly recommended to check out the docs by clicking the blue manual icon in the corner above. This behaviour might not play nicely with animations that touch materials")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#shader-feeder")]
  public class ShaderFeeder : UdonSharpBehaviour {
    [SectionHeader("General")] [UTEditor]
    public bool active = true;
    public Shader source;
    public MeshRenderer[] targets;
    public bool customUpdateRate;
    
    [Horizontal("Update Rate")]
    [HideIf("@!customUpdateRate")] [UTEditor]
    public float updateRate = 1;
    
    [Horizontal("Update Rate")]
    [Popup("method","@updateTypes", true)]
    [HideIf("@!customUpdateRate")]
    [UTEditor]
    public string updateType = "Per Second";
    
    private string[] updateTypes = {"Per Second", "Per Minute"};

    [SectionHeader("Shader Properties")] [Toggle] [UTEditor]
    public bool setSceneStartTime = true;
    
    [HideIf("@!setSceneStartTime")]
    [Popup("shader", "@source")]
    [HelpBox("Scene start time will be saved to this float", "@setSceneStartTime")]
    [UTEditor]
    public string startTimeTarget;
    
    [Toggle]
    [HideIf("@!customUpdateRate")]
    [UTEditor] public bool setCycleLength = true;
    
    [HideIf("HideCycleLengthTarget")]
    [Popup("shader", "@source")]
    [UTEditor]
    public string cycleLengthTarget;
    
    private bool HideCycleLengthTarget() {
      return !customUpdateRate || !setCycleLength;
    }

    [Toggle] [UTEditor] public bool setCycleStartTime = true;
    
    [HideIf("@!setCycleStartTime")]
    [Popup("shader", "@source")]
    [HelpBox("Start time of each cycle will be saved to this float", "@setCycleStartTime")]
    [UTEditor]
    public string cycleTimeTarget;
    
    [ListView("Slider Sources")] [HideLabel] [UTEditor]
    public Slider[] sliderSources;
    
    [ListView("Slider Sources")]
    [Popup("shader", "@source", "float", true)]
    [UTEditor]
    public string[] sliderTargets;
    
    [ListView("Transform Sources")] [HideLabel] [UTEditor]
    public Transform[] transformSources;
    
    [ListView("Transform Sources")]
    [Popup("shader", "@source", "vector", true)]
    [UTEditor]
    public string[] transformTargets;

    private MaterialPropertyBlock block;
    private float nextUpdate;
    private float updateTime;

    private void Start() {
      if (targets.Length == 0) {
        active = false;
        return;
      }

      block = new MaterialPropertyBlock();
      targets[0].GetPropertyBlock(block);
      block.SetFloat(startTimeTarget, Time.time);
      if (customUpdateRate) {
        if (updateType == "Per Second") {
          updateTime = 1 / updateRate;
        }
        else {
          updateTime = 60 / updateRate;
        }

        block.SetFloat(cycleLengthTarget, updateTime);
      }

      foreach (var target in targets) {
        target.SetPropertyBlock(block);
      }
    }

    private void Update() {
      if (targets.Length == 0) return;
      if (!active) return;
      if (customUpdateRate && Time.time < nextUpdate) return;
      nextUpdate = Time.time + updateTime;
      targets[0].GetPropertyBlock(block);
      block.SetFloat(cycleTimeTarget, Time.time);
      if (sliderSources.Length > 0) {
        for (int i = 0; i < sliderSources.Length; i++) {
          block.SetFloat(sliderTargets[i], sliderSources[i].value);
        }
      }

      if (transformSources.Length > 0) {
        for (int i = 0; i < transformSources.Length; i++) {
          block.SetVector(transformTargets[i], transformSources[i].position);
        }
      }

      foreach (var target in targets) {
        target.SetPropertyBlock(block);
      }
    }
  }
}