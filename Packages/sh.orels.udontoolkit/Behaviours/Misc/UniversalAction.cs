using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
  [CustomName("Universal Action")]
  [HelpMessage(
    "This component expects a \"Trigger\" event to fire.")]
  [HelpURL("https://ut.orels.sh/v/v1.x/behaviours/misc-behaviours#universal-action")]
  [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
  public class UniversalAction : UdonSharpBehaviour {
    [SectionHeader("General")]
    public bool active = true;
    public bool oneShot;

    [HelpBox("Make sure this game object is always enabled when using delays.", "@fireAfterDelay")]
    public bool fireAfterDelay;

    [HideIf("@!fireAfterDelay")] [Tooltip("Delay in seconds")]
    public float delayLength;

    #region Animations
    [FoldoutGroup("Animations")] public bool fireAnimationTriggers;

    [NonSerialized] public string[] animatorChangeModeOptions = new[] {
      "SET", "ADD", "SUB"
    };

    [FoldoutGroup("Animations")] [ListView("Animator Triggers List")]
    public Animator[] animators;

    [FoldoutGroup("Animations")] [ListView("Animator Triggers List")] [LVHeader("Triggers")]
    [Popup("animator", "@animators", true)]
    public string[] animatorTriggers;
    
    [FoldoutGroup("Animations")] [ListView("Animator Bools List")] [LVHeader("Animators")]
    public Animator[] animatorBoolTargets;

    [FoldoutGroup("Animations")] [ListView("Animator Bools List")] [LVHeader("Bools")]
    [Popup("animatorBool", "@animatorBoolTargets", true)]
    public string[] animatorBoolNames;
    
    [FoldoutGroup("Animations")] [ListView("Animator Bools List")] [LVHeader("Values")]
    public bool[] animatorBools;
    
    [FoldoutGroup("Animations")] [ListView("Animator Floats List")] [LVHeader("Animators")]
    public Animator[] animatorFloatTargets;

    [FoldoutGroup("Animations")] [ListView("Animator Floats List")] [LVHeader("Floats")]
    [Popup("animatorFloat", "@animatorFloatTargets", true)]
    public string[] animatorFloatNames;
    
    [FoldoutGroup("Animations")] [ListView("Animator Floats List")] [LVHeader("Values")]
    public float[] animatorFloats;
    
    [FoldoutGroup("Animations")] [ListView("Animator Floats List")] [LVHeader("Modes")]
    [Popup("@animatorChangeModeOptions")]
    public int[] animatorFloatsChangeModes;
    
    [FoldoutGroup("Animations")] [ListView("Animator Ints List")] [LVHeader("Animators")]
    public Animator[] animatorIntTargets;

    [FoldoutGroup("Animations")] [ListView("Animator Ints List")] [LVHeader("Ints")]
    [Popup("animatorInt", "@animatorIntTargets", true)]
    public string[] animatorIntNames;
    
    [FoldoutGroup("Animations")] [ListView("Animator Ints List")] [LVHeader("Values")]
    public int[] animatorInts;
    
    [FoldoutGroup("Animations")] [ListView("Animator Ints List")] [LVHeader("Modes")]
    [Popup("@animatorChangeModeOptions")]
    public int[] animatorIntsChangeModes;
    #endregion

    #region Udon Events
    [FoldoutGroup("Udon Events")] public bool fireUdonEvents;

    [FoldoutGroup("Udon Events")]
    [HelpBox(
      "Only use this option if you are invoking this action locally, e.g. from a UI Button, otherwise it will cause oversync.",
      "@networked")]
    public bool networked;

    [FoldoutGroup("Udon Events")] [HideIf("@!networked")]
    public NetworkEventTarget networkTarget;

    [FoldoutGroup("Udon Events")] [ListView("Udon Events List")]
    public UdonSharpBehaviour[] udonTargets;

    [FoldoutGroup("Udon Events")] [ListView("Udon Events List")] [Popup("behaviour", "@udonTargets", true)]
    public string[] udonEvents;
    #endregion
    
    #region GameObjects
    [FoldoutGroup("Game Object Toggles")] public bool fireObjectToggles;

    [FoldoutGroup("Game Object Toggles")] [ListView("Game Objects List")]
    public GameObject[] goTargets;

    [FoldoutGroup("Game Object Toggles")] [ListView("Game Objects List")] [LVHeader("Toggle Actions")] [Popup("method", "@goToggleOptions", true)]
    public string[] goToggleEvents;

    [NonSerialized] public string[] goToggleOptions = {
      "Enable",
      "Disable",
      "Toggle"
    };
    #endregion

    #region Collider
    [FoldoutGroup("Collider Toggles")] public bool fireColliderToggles;

    [FoldoutGroup("Collider Toggles")] [ListView("Colliders List")]
    public Collider[] colliderTargets;

    [FoldoutGroup("Collider Toggles")] [ListView("Colliders List")] [Popup("method", "@goToggleOptions", true)]
    public int[] colliderToggleEvents;
    #endregion

    #region Audio
    [FoldoutGroup("Audio")] public bool fireAudioEvents;

    [FoldoutGroup("Audio")]
    [HelpBox("Playing many audio clips at once is performance heavy, please be considerate when using these.")]
    [ListView("Audio List")]
    public AudioSource[] audioSources;

    [FoldoutGroup("Audio")] [ListView("Audio List")]
    public AudioClip[] audioClips;
    #endregion
    
    private bool delayActive;
    private float delayExpire;
    
    private bool used;
    
    public void Activate() {
      active = true;
    }
    
    public void Deactivate() {
      active = false;
    }
    
    public void Toggle() {
      active = !active;
    }
    
    public void ResetOneShot() {
      used = false;
      active = true;
    }
    
    public void Trigger() {
      if (!active) return;
      if (fireAfterDelay) {
        SendCustomEventDelayedSeconds(nameof(_FireTrigger), delayLength);
        return;
      }
      
      if (oneShot) {
        if (used) {
          return;
        }

        active = false;
        used = true;
        enabled = false;
      }

      FireAnimationTriggers();
      FireAnimationBools();
      FireAnimationFloats();
      FireAnimationInts();
      FireUdonEvents();
      FireObjectToggles();
      FireColliderToggles();
      FireAudioEvents();
    }

    public void _FireTrigger() {
      if (!active) return;
      if (!gameObject.activeSelf) return;
      if (oneShot) {
        if (used) {
          return;
        }

        active = false;
        used = true;
        enabled = false;
      }

      FireAnimationTriggers();
      FireAnimationBools();
      FireAnimationFloats();
      FireAnimationInts();
      FireUdonEvents();
      FireObjectToggles();
      FireColliderToggles();
      FireAudioEvents();
    }

    private void FireAnimationTriggers() {
      if (!fireAnimationTriggers) return;
      for (int i = 0; i < animators.Length; i++) {
        animators[i].SetTrigger(animatorTriggers[i]);
      }
    }
    
    private void FireAnimationBools() {
      if (!fireAnimationTriggers) return;
      for (int i = 0; i < animatorBoolTargets.Length; i++) {
        animatorBoolTargets[i].SetBool(animatorBoolNames[i], animatorBools[i]);
      }
    }
    
    private void FireAnimationFloats() {
      if (!fireAnimationTriggers) return;
      for (int i = 0; i < animatorFloatTargets.Length; i++) {
        if (animatorFloatsChangeModes.Length == 0) {
          animatorFloatsChangeModes = new int[animatorFloatTargets.Length];
        }
        switch (animatorFloatsChangeModes[i]) {
          case 0:
            animatorFloatTargets[i].SetFloat(animatorFloatNames[i], animatorFloats[i]);
            break;
          case 1: {
            animatorFloatTargets[i].SetFloat(animatorFloatNames[i], animatorFloatTargets[i].GetFloat(animatorFloatNames[i]) + animatorFloats[i]);
            break;
          }
          case 2:
            animatorFloatTargets[i].SetFloat(animatorFloatNames[i],
              animatorFloatTargets[i].GetFloat(animatorFloatNames[i]) - animatorFloats[i]);
            break;
        }
      }
    }
    
    private void FireAnimationInts() {
      if (!fireAnimationTriggers) return;
      for (int i = 0; i < animatorIntTargets.Length; i++) {
        if (animatorIntsChangeModes.Length == 0) {
          animatorIntsChangeModes = new int[animatorIntTargets.Length];
        }
        switch (animatorIntsChangeModes[i]) {
          case 0:
            animatorIntTargets[i].SetInteger(animatorIntNames[i], animatorInts[i]);
            break;
          case 1: {
            animatorIntTargets[i].SetInteger(animatorIntNames[i], animatorIntTargets[i].GetInteger(animatorIntNames[i]) + animatorInts[i]);
            break;
          }
          case 2:
            animatorIntTargets[i].SetInteger(animatorIntNames[i],
              animatorIntTargets[i].GetInteger(animatorIntNames[i]) - animatorInts[i]);
            break;
        }
      }
    }

    private void FireUdonEvents() {
      if (!fireUdonEvents) return;
      for (int i = 0; i < udonTargets.Length; i++) {
        var uB = udonTargets[i];
        if (networked) {
          uB.SendCustomNetworkEvent(networkTarget, udonEvents[i]);
          continue;
        }

        uB.SendCustomEvent(udonEvents[i]);
      }
    }

    private void FireObjectToggles() {
      if (!fireObjectToggles) return;
      for (int i = 0; i < goTargets.Length; i++) {
        if (goToggleEvents[i] == "Toggle") {
          goTargets[i].SetActive(!goTargets[i].activeSelf);
          continue;
        }

        var state = goToggleEvents[i] == "Enable";
        goTargets[i].SetActive(state);
      }
    }

    private void FireColliderToggles() {
      if (!fireColliderToggles) return;
      for (int i = 0; i < colliderTargets.Length; i++) {
        if (colliderToggleEvents[i] == 2) {
          colliderTargets[i].enabled = !colliderTargets[i].enabled;
          continue;
        }

        var state = colliderToggleEvents[i] == 0;
        colliderTargets[i].enabled = state;
      }
    }

    private void FireAudioEvents() {
      if (!fireAudioEvents) return;
      for (int i = 0; i < audioSources.Length; i++) {
        audioSources[i].PlayOneShot(audioClips[i]);
      }
    }
  }
}
