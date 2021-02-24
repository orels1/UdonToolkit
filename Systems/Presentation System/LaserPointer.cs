using System;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
  [CustomName("Laser Pointer")]
  [HelpURL("https://ut.orels.sh/systems/presentation-system")]
  public class LaserPointer : UdonSharpBehaviour {
    public LineRenderer laserLine;
    public GameObject laserDot;
    public bool active;
    public LayerMask hitLayers;


    private VRC_Pickup pickup;
    [UdonSynced]
    private Vector3 startPosition;
    [UdonSynced]
    private Quaternion startRotation;
    private bool waitingToRespawn;
    private float respawnTime;

    private void Start() {
      pickup = (VRC_Pickup) GetComponent(typeof(VRC_Pickup));
      if (Networking.IsOwner(gameObject)) {
        startPosition = transform.position;
        startRotation = transform.rotation;
      }
    }

    public override void OnDrop() {
      SendCustomNetworkEvent(NetworkEventTarget.All, "LaserDisable");
      if (Networking.IsOwner(gameObject)) {
        waitingToRespawn = true;
        respawnTime = Time.timeSinceLevelLoad + 10;
      }
    }

    public override void OnPickup() {
      if (waitingToRespawn) {
        waitingToRespawn = false;
      }
    }

    public override void OnPickupUseDown() {
      SendCustomNetworkEvent(NetworkEventTarget.All, "LaserToggle");
    }

    public void LaserToggle() {
      active = !active;
      laserLine.gameObject.SetActive(active);
      laserDot.SetActive(active);
    }

    public void LaserDisable() {
      active = false;
      laserLine.gameObject.SetActive(active);
      laserDot.SetActive(active);
    }

    private void LateUpdate() {
      if (waitingToRespawn && Time.timeSinceLevelLoad >= respawnTime) {
        if (Networking.IsOwner(gameObject)) {
          transform.SetPositionAndRotation(startPosition, startRotation);
        }
        waitingToRespawn = false;
      }
      if (!active) return;
      RaycastHit hit;
      if (Physics.Raycast(transform.position, transform.forward, out hit, 30f, hitLayers)) {
        laserLine.gameObject.SetActive(true);
        laserDot.SetActive(true);
        var point = hit.point;
        laserLine.SetPosition(0, transform.position);
        laserLine.SetPosition(1, point);
        laserDot.transform.position = point;
        laserDot.transform.LookAt(point + hit.normal * -2);
      }
      else {
        laserLine.gameObject.SetActive(false);
        laserDot.SetActive(false);
      }
    }
  }
}
