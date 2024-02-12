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
  [HelpURL("https://ut.orels.sh/v/v1.x/behaviours/misc-behaviours#shader-feeder")]
  [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
  public class ShaderFeeder : UdonSharpBehaviour {
    [SectionHeader("General")] public bool active = true;
    public Shader source;
    public MeshRenderer[] targets;
    public bool customUpdateRate;

    [Horizontal("Update Rate")] [HideIf("@!customUpdateRate")]
    public float updateRate = 1;

    [Horizontal("Update Rate")] [Popup("method", "@updateTypes", true)] [HideIf("@!customUpdateRate")]
    public string updateType = "Per Second";

    private string[] updateTypes = {"Per Second", "Per Minute"};
    
    [FoldoutGroup("Cycle Options")]
    [Toggle]
    public bool setSceneStartTime = true;

    [FoldoutGroup("Cycle Options")]
    [Popup("shader", "@source")]
    [HelpBox("Scene start time will be saved to this float", "@setSceneStartTime")]
    public string startTimeTarget;

    [FoldoutGroup("Cycle Options")]
    [Toggle] [HideIf("@!customUpdateRate")]
    public bool setCycleLength = true;

    [FoldoutGroup("Cycle Options")]
    [HideIf("HideCycleLengthTarget")] [Popup("shader", "@source")]
    public string cycleLengthTarget;

    private bool HideCycleLengthTarget() {
      return !customUpdateRate || !setCycleLength;
    }

    [FoldoutGroup("Cycle Options")]
    [Toggle] public bool setCycleStartTime = true;

    [FoldoutGroup("Cycle Options")]
    [HideIf("@!setCycleStartTime")]
    [Popup("shader", "@source")]
    [HelpBox("Start time of each cycle will be saved to this float", "@setCycleStartTime")]
    public string cycleTimeTarget;

    [ListView("Slider Sources")]
    public Slider[] sliderSources;
    [ListView("Slider Sources")] [Popup("shader", "@source", "float", true)]
    public string[] sliderTargets;

    [ListView("Transform Sources")]
    public Transform[] transformSources;
    [ListView("Transform Sources")] [Popup("shader", "@source", "vector", true)]
    public string[] transformTargets;

    [ListView("Text Sources")]
    [HelpBox("Text inputs will be casted to floats and passed into the provided property")]
    public InputField[] textSources;
    [ListView("Text Sources")] [Popup("shader", "@source", "float", true)]
    public string[] textTargets;

    [ListView("Udon Variables")] public UdonSharpBehaviour[] udonBehaviours;
    [ListView("Udon Variables")] [LVHeader("Variables")] [Popup("programVariable", "@udonBehaviours")]
    public string[] udonVariables;
    [ListView("Udon Variables")] [LVHeader("Types")] [Popup("@typeOptions")]
    public string[] udonVariableTypes;
    [ListView("Udon Variables")] [LVHeader("Shader Props")] [Popup("shader", "@source", "all", true)]
    public string[] udonVariableTargets;

    [HideInInspector] public string[] typeOptions = new[] {
      "float",
      "int",
      "color",
      "vector"
    };

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
      
      if (textSources.Length > 0) {
        for (int i = 0; i < textSources.Length; i++) {
          block.SetFloat(textTargets[i], Convert.ToSingle(textSources[i].text));
        }
      }

      if (udonBehaviours.Length > 0) {
        HandleUdonVars();
      }

      foreach (var target in targets) {
        target.SetPropertyBlock(block);
      }
    }

    private void HandleUdonVars() {
      for (int i = 0; i < udonBehaviours.Length; i++) {
        switch (udonVariableTypes[i]) {
          case "float":
            block.SetFloat(udonVariableTargets[i], (float) udonBehaviours[i].GetProgramVariable(udonVariables[i]));
            break;
          case "int":
            block.SetInt(udonVariableTargets[i], (int) udonBehaviours[i].GetProgramVariable(udonVariables[i]));
            break;
          case "color":
            block.SetColor(udonVariableTargets[i], (Color) udonBehaviours[i].GetProgramVariable(udonVariables[i]));
            break;
          case "vector":
            block.SetVector(udonVariableTargets[i], (Vector4) udonBehaviours[i].GetProgramVariable(udonVariables[i]));
            break;
        }
      }
    }
  }
}
