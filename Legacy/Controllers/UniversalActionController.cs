#if UNITY_EDITOR
using System;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomName("Universal Action")]
  [HelpMessage(
    "This component expects a \"Trigger\" event to fire.")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#universal-action")]
  public class UniversalActionController : UTController {
    [SectionHeader("General")][UdonPublic] public bool active = true;

    [HelpBox("Make sure this game object is always enabled when using delays.", "@fireAfterDelay")]
    [UdonPublic]
    public bool fireAfterDelay;
    [HideIf("@!fireAfterDelay")]
    [Tooltip("Delay in seconds")]
    [UdonPublic]
    public float delayLength = 1;
    
    [SectionHeader("Animations")]
    [UdonPublic] public bool fireAnimationTriggers;
    
    [ListView("Animator Triggers List")][UdonPublic]
    public Animator[] animators;

    [ListView("Animator Triggers List")]
    [Popup(PopupAttribute.PopupSource.Animator, "@animators", true)]
    [UdonPublic]
    public string[] animatorTriggers;

    [SectionHeader("Udon Events")] [UdonPublic]
    public bool fireUdonEvents;
    
    [HelpBox("Only use this option if you are invoking this action locally, e.g. from a UI Button, otherwise it will cause oversync.", "@networked")]
    [UdonPublic]
    public bool networked;
    
    [HideIf("@!networked")]
    [UdonPublic]
    public NetworkEventTarget networkTarget;

    [ListView("Udon Events List")] [UdonPublic]
    public UdonBehaviour[] udonTargets;

    [ListView("Udon Events List")] [Popup(PopupAttribute.PopupSource.UdonBehaviour, "@udonTargets", true)] [UdonPublic]
    public string[] udonEvents;

    [SectionHeader("Game Object Toggles")] [UdonPublic]
    public bool fireObjectToggles;

    [ListView("Game Objects List")] [UdonPublic]
    public GameObject[] goTargets;

    [ListView("Game Objects List")] [Popup(PopupAttribute.PopupSource.Method, "@goToggleOptions", true)] [UdonPublic]
    public string[] goToggleEvents;

    [HideInInspector] public string[] goToggleOptions = {
      "Enable",
      "Disable",
      "Toggle"
    };
    
    [SectionHeader("Collider Toggles")] [UdonPublic]
    public bool fireColliderToggles;

    [ListView("Colliders List")] [UdonPublic]
    public Collider[] colliderTargets;

    [ListView("Colliders List")] [Popup(PopupAttribute.PopupSource.Method, "@colliderToggleOptions", true)] [UdonPublic]
    public string[] colliderToggleEvents;

    [HideInInspector] public string[] colliderToggleOptions = {
      "Enable",
      "Disable",
      "Toggle"
    };

    [SectionHeader("Audio")] [UdonPublic] public bool fireAudioEvents;

    [HelpBox("Playing many audio clips at once is performance heavy, please be considerate when using these.")]
    [ListView("Audio List")] [UdonPublic] public AudioSource[] audioSources;
    [ListView("Audio List")] [UdonPublic] public AudioClip[] audioClips;
    
    [Button("Trigger")]
    public void Trigger() {
      if (uB == null) return;
      uB.SendCustomEvent("Trigger");
    }

    [Button("Activate")]
    public void Activate() {
      if (uB == null) return;
      uB.SendCustomEvent("Activate");
    }
    
    [Button("Deactivate")]
    public void Deactivate() {
      if (uB == null) return;
      uB.SendCustomEvent("Deactivate");
    }
    
    [Button("Toggle")]
    public void Toggle() {
      if (uB == null) return;
      uB.SendCustomEvent("Toggle");
    }
  }
}

#endif