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
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#universal-action")]
  public class UniversalAction : UdonSharpBehaviour {
    [SectionHeader("General")] [UTEditor]
    public bool active = true;
    
    [HelpBox("Make sure this game object is always enabled when using delays.", "@fireAfterDelay")]
    [UTEditor]
    public bool fireAfterDelay;
    
    [HideIf("@!fireAfterDelay")]
    [Tooltip("Delay in seconds")]
    [UTEditor]
    public float delayLength;
    
    [SectionHeader("Animations")][UTEditor]
    public bool fireAnimationTriggers;
    
    [ListView("Animator Triggers List")][UTEditor]
    public Animator[] animators;
    [ListView("Animator Triggers List")]
    [Popup("animator", "@animators", true)]
    [UTEditor]
    public string[] animatorTriggers;
    
    [SectionHeader("Udon Events")] [UTEditor]
    public bool fireUdonEvents;
    
    [HelpBox("Only use this option if you are invoking this action locally, e.g. from a UI Button, otherwise it will cause oversync.", "@networked")]
    [UTEditor]
    public bool networked;
    
    [HideIf("@!networked")]
    [UTEditor]
    public NetworkEventTarget networkTarget;
    
    [ListView("Udon Events List")] [UTEditor]
    public UdonSharpBehaviour[] udonTargets;
    
    [ListView("Udon Events List")]
    [Popup("behaviour", "@udonTargets", true)]
    [UTEditor]
    public string[] udonEvents;
    
    [SectionHeader("Game Object Toggles")] [UTEditor]
    public bool fireObjectToggles;
    
    [ListView("Game Objects List")] [UTEditor]
    public GameObject[] goTargets;
    
    [ListView("Game Objects List")]
    [Popup("method", "@goToggleOptions", true)]
    [UTEditor]
    public string[] goToggleEvents;

    [HideInInspector] public string[] goToggleOptions = {
      "Enable",
      "Disable",
      "Toggle"
    };

    [SectionHeader("Collider Toggles")] [UTEditor]
    public bool fireColliderToggles;
    
    [ListView("Colliders List")] [UTEditor]
    public Collider[] colliderTargets;
    
    [ListView("Colliders List")]
    [Popup("method", "@goToggleOptions", true)]
    [UTEditor]
    public string[] colliderToggleEvents;
    
    [SectionHeader("Audio")] [UTEditor]
    public bool fireAudioEvents;
    
    [HelpBox("Playing many audio clips at once is performance heavy, please be considerate when using these.")]
    [ListView("Audio List")] [UTEditor]
    public AudioSource[] audioSources;
    [ListView("Audio List")] [UTEditor]
    public AudioClip[] audioClips;

    private bool delayActive;
    private float delayExpire;

    [Button("Activate")]
    public void Activate() {
      active = true;
    }

    [Button("Deactivate")]
    public void Deactivate() {
      active = false;
    }

    [Button("Toggle")]
    public void Toggle() {
      active = !active;
    }

    [Button("Trigger")]
    public void Trigger() {
      if (!active) return;
      if (fireAfterDelay) {
        delayExpire = Time.time + delayLength;
        delayActive = true;
        return;
      }
      FireAnimationTriggers();
      FireUdonEvents();
      FireObjectToggles();
      FireColliderToggles();
      FireAudioEvents();
    }

    public void Update() {
      if (!active) return;
      if (!delayActive) return;
      if (Time.time >= delayExpire) {
        FireAnimationTriggers();
        FireUdonEvents();
        FireObjectToggles();
        FireColliderToggles();
        FireAudioEvents();
        delayActive = false;
      }
    }

    private void FireAnimationTriggers() {
      if (!fireAnimationTriggers) return;
      for (int i = 0; i < animators.Length; i++) {
        animators[i].SetTrigger(animatorTriggers[i]);
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
        if (colliderToggleEvents[i] == "Toggle") {
          colliderTargets[i].enabled = !colliderTargets[i].enabled;
          continue;
        }
        
        var state = colliderToggleEvents[i] == "Enable";
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