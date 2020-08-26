using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
    public class ShaderFeeder : UdonSharpBehaviour {
      public bool active = true;
      public MeshRenderer[] targets;
      public bool customUpdateRate;
      public float updateRate = 1;
      public string updateType = "Per Second";
      public string startTimeTarget;
      public string cycleLengthTarget;
      public string cycleTimeTarget;
      public Slider[] sliderSources;
      public string[] sliderTargets;
      public Transform[] transformSources;
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