using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
  public class UniversalAction : UdonSharpBehaviour {
    public bool active = true;
    public bool fireAfterDelay;
    public float delayLength;
    public bool fireAnimationTriggers;
    public Animator[] animators;
    public string[] animatorTriggers;
    public bool fireUdonEvents;
    public bool networked;
    public NetworkEventTarget networkTarget;
    public Component[] udonTargets;
    public string[] udonEvents;
    public bool fireObjectToggles;
    public GameObject[] goTargets;
    public string[] goToggleEvents;
    public bool fireColliderToggles;
    public Collider[] colliderTargets;
    public string[] colliderToggleEvents;
    public bool fireAudioEvents;
    public AudioSource[] audioSources;
    public AudioClip[] audioClips;

    private bool delayActive;
    public float delayExpire;

    public void Activate() {
      active = true;
    }

    public void Deactivate() {
      active = false;
    }

    public void Toggle() {
      active = !active;
    }

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
        var uB = (UdonBehaviour) udonTargets[i];
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