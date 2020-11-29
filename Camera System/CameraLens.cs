using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
    public class CameraLens : UdonSharpBehaviour {
      public bool active;
      public Animator lensAnimator;
      public Animator cameraAnimator;
      [Space(5)]
      public GameObject desktopUI;
      public GameObject vrUI;
      [Space(5)]
      public Slider focusSlider;
      public Slider focalSlider;
      public Slider zoomSlider;
      public Slider vrFocusSlider;
      public Slider vrFocalSlider;
      public Slider vrZoomSlider;
      [Space(5)]
      public GameObject viewSphere;
      public GameObject cameraObject;
      public GameObject visuals;
      public MeshRenderer fingerSphere;
      [Space(5)]
      public bool autoFocus;
      public float autoFocusDistance = 30f;
      public Image autoFocusIcon;
      public Image alwaysOnIcon;
      [ColorUsage(true)]
      public Color activeControlColor;

      [Space(5)]
      public Text positionText;
      public Text focusText;
      public Text zoomText;
      public Text focalText;
      public Image focusBg;
      public Image zoomBg;
      public Image focalBg;
      [Space(5)]
      public Transform dropTarget;
      public UniversalTracker playspaceTracker;

      private VRC_Pickup pickup;
      private ParentConstraint constraint;
      private BoxCollider col;
      private bool isDesktop = true;
      private float focus;
      private float focal;
      private float zoom;
      private bool pictureTaken;
      private Color oldAFColor;
      private Color oldAlwaysOnColor;
      private Material oldLensMat;
      private MeshRenderer sR;
      private MeshRenderer cR;
      private Camera cam;
      private int activeControl;
      private bool alwaysOn;
      private bool canChangeControl = true;
      private bool worldSpace = true;
      private MaterialPropertyBlock fingerSphereOn;
      private MaterialPropertyBlock fingerSphereOff;
      private MaterialPropertyBlock sphereBlock;
      private Color inactiveColor = new Color(1, 1, 1, 0);
      private Color activeColor = new Color(1, 1, 1, 0.4f);
      private Color inactiveTextColor = new Color(1, 1, 1, 0.4f);
      private Color activeTextColor = new Color(0,0,0,1f);
      private bool closeHelpShown;

      private void Start() {
        // property blocks
        sphereBlock = new MaterialPropertyBlock();
        fingerSphereOn = new MaterialPropertyBlock();
        fingerSphereOn.SetInt("_Enabled", 1);
        fingerSphereOff = new MaterialPropertyBlock();
        fingerSphereOff.SetInt("_Enabled", 0);
        // handle default values
        cam = cameraObject.GetComponent<Camera>();
        zoom = Remap(cam.focalLength, 5, 60, 0, 1);
        zoomSlider.value = zoom;
        vrZoomSlider.value = zoom;
        focal = 0.5f;
        lensAnimator.SetFloat("Focal", focal);
        focalSlider.value = focal;
        vrFocalSlider.value = focal;
        // handle vr/desktop switch
        col = gameObject.GetComponent<BoxCollider>();
        constraint = gameObject.GetComponent<ParentConstraint>();
        cR = visuals.GetComponent<MeshRenderer>();
        sR = viewSphere.GetComponent<MeshRenderer>();
        fingerSphere.gameObject.SetActive(false);
        pickup = (VRC_Pickup) GetComponent(typeof(VRC_Pickup));
        var player = Networking.LocalPlayer;
        if (player == null) return;
        if (player.IsUserInVR()) {
          cameraAnimator.SetBool("IsVR", true);
          vrUI.SetActive(true);
          isDesktop = false;
          constraint.constraintActive = false;
          pickup.pickupable = true;
          return;
        }

        desktopUI.SetActive(true);
        sphereBlock.SetInt("_Watermark", 1);
        sphereBlock.SetFloat("_SceneStartTime", Time.time - 1);
        sR.SetPropertyBlock(sphereBlock);
      }

      private void Activate() {
        active = true;
        cameraObject.SetActive(true);
        if (!isDesktop) {
          viewSphere.SetActive(true);
          fingerSphere.gameObject.SetActive(true);
          fingerSphere.SetPropertyBlock(fingerSphereOn);
        }
        cameraAnimator.SetBool("UIShown", true);
        if (!isDesktop) return;
        constraint.enabled = true;
        constraint.constraintActive = true;
        constraint.weight = 1;
        cameraAnimator.SetBool("Active", true);
        col.enabled = false;
      }

      private void Deactivate() {
        if (!alwaysOn) {
          cameraObject.SetActive(false);
          viewSphere.SetActive(false);
        }
        fingerSphere.gameObject.SetActive(false);
        fingerSphere.SetPropertyBlock(fingerSphereOff);
        constraint.weight = 0;
        constraint.enabled = false;
        cameraAnimator.SetBool("Active", false);
        cameraAnimator.SetBool("UIShown", false);
        active = false;
        col.enabled = true;
        if (isDesktop) {
          transform.position = transform.position + Vector3.down * 0.04f;
        }
      }

      public override void Interact() {
        if (!isDesktop) return;
        Activate();
      }

      public override void OnPickup() {
        if (isDesktop) return;
        transform.parent = null;
        playspaceTracker.ResetOffsets();
        Activate();
      }
      
      public override void OnDrop() {
        if (isDesktop) return;
        if (!worldSpace) {
          playspaceTracker.ResetOffsets();
          transform.SetParent(dropTarget);
        }
        Deactivate();
      }

      public void SwitchPosition() {
        worldSpace = !worldSpace;
        positionText.text = worldSpace ? "World Position" : "Local Position";
      }

      public void PassTutorial() {
        cameraAnimator.SetBool("TChecked", true);
      }

      public void ToggleWatermark() {
        sR.GetPropertyBlock(sphereBlock);
        var curr = sphereBlock.GetInt("_Watermark");
        if (curr == 1) {
          sphereBlock.SetInt("_Watermark", 0);
        }
        else {
          sphereBlock.SetInt("_Watermark", 1);
        }
        sR.SetPropertyBlock(sphereBlock);
      }

      private void Update() {
        if (autoFocus) {
          AutoFocus();
        }
        // global "P" hotkey for desktop
        if (!active && isDesktop) {
          if (Input.GetKeyDown("p")) {
            Activate();
          }
          return;
        }
        
        if (!autoFocus) {
          FocusInputs();
        }

        FocalInputs();
        ZoomInputs();
        SwitchControlInputs();

        if (!isDesktop) return;

        if (Input.GetKeyDown("f")) {
          sR.GetPropertyBlock(sphereBlock);
          sphereBlock.SetInt("_ForceShow", 1);
          sR.SetPropertyBlock(sphereBlock);
          viewSphere.SetActive(true);
          if (!alwaysOn) {
            cameraObject.SetActive(false);
          }
          pictureTaken = true;
          if (!closeHelpShown) {
            sR.GetPropertyBlock(sphereBlock);
            sphereBlock.SetFloat("_EndFadeTime", Time.time + 2);
            sR.SetPropertyBlock(sphereBlock);
            closeHelpShown = true;
          }
        }
        if (Input.GetKeyDown("p")) {
          if (pictureTaken) {
            sR.GetPropertyBlock(sphereBlock);
            sphereBlock.SetInt("_ForceShow", 0);
            sR.SetPropertyBlock(sphereBlock);
            viewSphere.SetActive(false);
            cameraObject.SetActive(true);
            pictureTaken = false;
            return;
          }
          Deactivate();
        }

        if (Input.GetKeyDown("x")) {
          ToggleAlwaysOn();
        }

        if (Input.GetKeyDown("h")) {
          cameraAnimator.SetBool("UIShown", !cameraAnimator.GetBool("UIShown"));
        }

        if (Input.GetKeyDown("y")) {
          SwitchAF();
        }
      }

      public void FlipCamera() {
        var lRot = cameraObject.transform.localRotation.eulerAngles;
        cameraObject.transform.localRotation = Quaternion.Euler(lRot.x, lRot.y + 180, lRot.z);
        var block = new MaterialPropertyBlock();
        cR.GetPropertyBlock(block);
        var curr = block.GetInt("_Flipped");
        block.SetInt("_Flipped", curr == 0 ? 1 : 0);
        cR.SetPropertyBlock(block);
      }

      public void SwitchAF() {
        if (!autoFocus) {
          oldAFColor = autoFocusIcon.color;
          autoFocusIcon.color = activeControlColor;
        }
        else {
          autoFocusIcon.color = oldAFColor;
        }

        autoFocus = !autoFocus;
      }

      public void SwitchControl() {
        activeControl += 1;
        if (activeControl > 2) {
          activeControl = 0;
        }
        focusBg.color = inactiveColor;
        zoomBg.color = inactiveColor;
        focalBg.color = inactiveColor;
        focusText.color = inactiveTextColor;
        zoomText.color = inactiveTextColor;
        focalText.color = inactiveTextColor;
        switch (activeControl) {
          case 0:
            focusBg.color = activeColor;
            focusText.color = activeTextColor;
            break;
          case 1:
            zoomBg.color = activeColor;
            zoomText.color = activeTextColor;
            break;
          case 2:
            focalBg.color = activeColor;
            focalText.color = activeTextColor;
            break;
        }
        cameraAnimator.SetTrigger($"ToggleControl{activeControl}");
      }

      public void ToggleAlwaysOn() {
        if (!alwaysOn) {
          oldAlwaysOnColor = alwaysOnIcon.color;
          alwaysOnIcon.color = activeControlColor;
        }
        else {
          alwaysOnIcon.color = oldAlwaysOnColor;
        }

        alwaysOn = !alwaysOn;
      }

      private void SwitchControlInputs() {
        if (!canChangeControl && Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") < 0.2) {
          canChangeControl = true;
        }
        if (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") > 0.8 && canChangeControl) {
          SwitchControl();
          canChangeControl = false;
        }
      }
      
      private float VRInputs(float source, bool flipped) {
        var res = source;
        var vertical = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical");
        if (flipped) {
          vertical *= -1;
        }
        if (vertical > 0 ) {
          res = Mathf.Min(res + (vertical / 5) * Time.deltaTime, 1);
        }
        else {
          res = Mathf.Max(res + (vertical / 5) * Time.deltaTime, 0);
        }

        return res;
      }
      
      private void FocusInputs() {
        if (!isDesktop) {
          if (activeControl != 0) return;
          focus = VRInputs(focus, false);
        }
        else {
          if (Input.GetKey("e")) {
            focus = Mathf.Min(focus + 0.2f * Time.deltaTime, 1);
          } else if (Input.GetKey("q")) {
            focus = Mathf.Max(focus - 0.2f * Time.deltaTime, 0);
          }
        }
        lensAnimator.SetFloat("Focus", focus);
        focusSlider.value = focus;
        vrFocusSlider.value = focus;
      }

      private void ZoomInputs() {
        if (!isDesktop) {
          if (activeControl != 1) return;
          zoom = VRInputs(zoom, false);
        }
        else {
          if (Input.GetKey("2")) {
            zoom = Mathf.Min(zoom + 0.2f * Time.deltaTime, 1);
          } else if (Input.GetKey("1")) {
            zoom = Mathf.Max(zoom - 0.2f * Time.deltaTime, 0);
          }
        }

        var fov = Remap(zoom, 0, 1, 5, 60);
        cam.focalLength = fov;
        zoomSlider.value = zoom;
        vrZoomSlider.value = zoom;
      }

      private void FocalInputs() {
        if (!isDesktop) {
          if (activeControl != 2) return;
          focal = VRInputs(focal, true);
        }
        else {
          if (Input.GetKey("3")) {
            focal = Mathf.Min(focal + 0.2f * Time.deltaTime, 1);
          } else if (Input.GetKey("4")) {
            focal = Mathf.Max(focal - 0.2f * Time.deltaTime, 0);
          }
        }

        lensAnimator.SetFloat("Focal", focal);
        focalSlider.value = 1 - focal;
        vrFocalSlider.value = 1 - focal;
      }
      
      private float Remap(float s, float a1, float a2, float b1, float b2)
      {
        return b1 + (s-a1)*(b2-b1)/(a2-a1);
      }

      private void AutoFocus() {
        RaycastHit hit;
        var distance = 10f;
        var fwd = cameraObject.transform.forward;
        if (Physics.Raycast(
          cameraObject.transform.position + fwd * 0.1f, fwd,
          out hit,
          autoFocusDistance, Int32.MaxValue, QueryTriggerInteraction.Ignore)) {
          distance = hit.distance;
        }

        focus = distance / 10f;
        lensAnimator.SetFloat("Focus", focus);
        focusSlider.value = focus;
        vrFocusSlider.value = focus;
      }
    }
}