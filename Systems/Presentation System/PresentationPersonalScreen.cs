using System;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Personal Screen")]
  [HelpURL("https://ut.orels.sh/systems/presentation-system")]
  [HelpMessage("This will spawn a personal screen with the presentation for users to take a closer look\nFor Desktop it will use the provided Desktop Button, for VR you need to press both triggers together")]
  public class PresentationPersonalScreen : UdonSharpBehaviour {
    public GameObject[] screenObjects;
    [HelpBox("This will be attached to the desktop/vr ref")]
    public GameObject mainScreen;

    public PresentationStreamController streamController;

    public Collider screenCollider;
    public string desktopButton = "k";
    [HelpBox("The position of the spawned screen for Desktop players")]
    public GameObject desktopSpawnRef;
    [HelpBox("The position of the spawned screen for VR players")]
    public GameObject vrSpawnRef;

    private bool isVR;
    private VRCPlayerApi player;
    [NonSerialized] public bool shown;

    private void Start() {
      player = Networking.LocalPlayer;
    }

    private int leftTrigCount;
    private int rightTrigCount;
    private bool canTrigLeft = true;
    private bool canTrigRight = true;
    private float leftTrigTime;

    private void Update() {
      if (!isVR && Time.timeSinceLevelLoad < 5) {
        isVR = player.IsUserInVR();
        if (!isVR) {
          screenCollider.enabled = false;
        }
      }
      if (Input.GetKeyDown(desktopButton)) {
        if (isVR) return;
        ToggleDesktopScreen();
      }

      var lTrig = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger");
      var rTrig = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger");

      // if we didn't press within one second - reset the counter
      if (leftTrigCount == 1 && Time.timeSinceLevelLoad - leftTrigTime > 1) {
        leftTrigCount = 0;
        rightTrigCount = 0;
      }

      if (lTrig > 0.7f && canTrigLeft) {
        leftTrigCount = 1;
        leftTrigTime = Time.timeSinceLevelLoad;
        canTrigLeft = false;
        return;
      }

      if (!canTrigLeft && lTrig < 0.3) {
        canTrigLeft = true;
        leftTrigCount = 0;
        return;
      }
      
      if (rTrig > 0.7f && canTrigRight) {
        rightTrigCount = 1;
        canTrigRight = false;
        return;
      }

      if (!canTrigRight && rTrig < 0.3) {
        canTrigRight = true;
        rightTrigCount = 0;
        return;
      }

      if (leftTrigCount == 1 && rightTrigCount == 1) {
        ToggleVRScreen();
        leftTrigCount = 0;
        rightTrigCount = 0;
      }

    }

    private void ToggleDesktopScreen() {
      shown = !shown;
      foreach (var o in screenObjects) {
        o.SetActive(shown);
      }

      streamController.SetStreamBool();

      if (shown) {
        mainScreen.transform.SetPositionAndRotation(desktopSpawnRef.transform.position, desktopSpawnRef.transform.rotation);
      }
    }
    
    private void ToggleVRScreen() {
      shown = !shown;
      foreach (var o in screenObjects) {
        o.SetActive(shown);
      }
      streamController.SetStreamBool();
      if (shown) {
        mainScreen.transform.SetPositionAndRotation(vrSpawnRef.transform.position, vrSpawnRef.transform.rotation);
      }
    }
  }
}
