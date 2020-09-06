#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.Udon;

namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomName("Camera Lens")]
  public class CameraLensController : UTController {
    [SectionHeader("General")]
    [UdonPublic] public bool active;
    [UdonPublic] public Animator lensAnimator;
    [UdonPublic] public Animator cameraAnimator;
    [UdonPublic] public VRCPickup pickup;
    [UdonPublic] public bool autoFocus;
    [UdonPublic] public float autoFocusDistance;

    [SectionHeader("UI Controls")]
    [UdonPublic] public GameObject desktopUI;
    [UdonPublic] public GameObject vrUI;
    [Horizontal("Focus Slider")][Tooltip("Focus Sliders")][HideLabel]
    [UdonPublic] public Slider focusSlider;
    [Horizontal("Focus Slider")][HideLabel]
    [UdonPublic] public Slider vrFocusSlider;
    [Horizontal("Focal Slider")][Tooltip("Focal Sliders")][HideLabel]
    [UdonPublic] public Slider focalSlider;
    [Horizontal("Focal Slider")][HideLabel]
    [UdonPublic] public Slider vrFocalSlider;
    [Horizontal("Zoom Slider")][Tooltip("Zoom Sliders")][HideLabel]
    [UdonPublic] public Slider zoomSlider;
    [Horizontal("Zoom Slider")][HideLabel]
    [UdonPublic] public Slider vrZoomSlider;
    [Horizontal("Focus Text")][Tooltip("Focus Text")][HideLabel]
    [UdonPublic] public Text focusText;
    [Horizontal("Focus Text")][HideLabel]
    [UdonPublic] public Image focusBg;
    [Horizontal("Focal Text")][Tooltip("Focal Text")][HideLabel]
    [UdonPublic] public Text focalText;
    [Horizontal("Focal Text")][HideLabel]
    [UdonPublic] public Image focalBg;
    [Horizontal("Zoom Text")][Tooltip("Zoom Text")][HideLabel]
    [UdonPublic] public Text zoomText;
    [Horizontal("Zoom Text")][HideLabel]
    [UdonPublic] public Image zoomBg;
    [UdonPublic] public Text positionText;
    
    [SectionHeader("Visuals and Overrides")]
    [UdonPublic] public GameObject viewSphere;
    [UdonPublic] public GameObject cameraObject;
    [UdonPublic] public GameObject visuals;
    [UdonPublic] public MeshRenderer fingerSphere;
    [UdonPublic] public Image autoFocusIcon;
    [UdonPublic] public Image alwaysOnIcon;
    [UdonPublic] public Color activeControlColor;
    
    [SectionHeader("Snapping and Position")]
    [UdonPublic] public Transform dropTarget;
    [UdonPublic] public UdonBehaviour playspaceTracker;


    [Button("Interact")]
    public void Interact() {
      if (uB == null) return;
      uB.Interact();
    }
    
    [Button("Pickup")]
    public void Pickup() {
      if (uB == null) return;
      uB.OnPickup();
    }
    
    [Button("Drop")]
    public void Drop() {
      if (uB == null) return;
      uB.OnDrop();
    }
    
    [Button("SwitchPosition")]
    public void SwitchPosition() {
      if (uB == null) return;
      uB.SendCustomEvent("SwitchPosition");
    }

    [Button("FlipCamera")]
    public void FlipCamera() {
      if (uB == null) return;
      uB.SendCustomEvent("FlipCamera");
    }

    [Button("SwitchAF")]
    public void SwitchAF() {
      if (uB == null) return;
      uB.SendCustomEvent("SwitchAF");
    }

    [Button("SwitchControl")]
    public void SwitchControl() {
      if (uB == null) return;
      uB.SendCustomEvent("SwitchControl");
    }

    [Button("ToggleAlwaysOn")]
    public void ToggleAlwaysOn() {
      if (uB == null) return;
      uB.SendCustomEvent("ToggleAlwaysOn");
    }

    [Button("ToggleWatermark")]
    public void ToggleWatermark() {
      if (uB == null) return;
      uB.SendCustomEvent("ToggleWatermark");
    }
  }
}

#endif