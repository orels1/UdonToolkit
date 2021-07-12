using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
    public class RemoteHands : UdonSharpBehaviour {
      public GameObject leftHand;
      public GameObject rightHand;
      public Transform createUnder;
      private Rigidbody[][] hands;
      private VRCPlayerApi[] players;

      private void Start() {
      }

      public override void OnPlayerJoined(VRCPlayerApi player) {
        Debug.LogFormat("{0} joined", player.displayName);
        Debug.LogFormat("Is Local? {0}", player == Networking.LocalPlayer);
        if (player == Networking.LocalPlayer) return; 
        var count = players == null ? 0 : players.Length;
        Debug.LogFormat("{0} count", count);
        var nP = new VRCPlayerApi[count + 1];
        var nH = new Rigidbody[count + 1][];
        for (int i = 0; i < count; i++) {
          nP[i] = players[i];
          nH[i] = hands[i];
        }

        var lHand = VRCInstantiate(leftHand);
        lHand.transform.SetParent(createUnder);
        var rHand = VRCInstantiate(rightHand);
        rHand.transform.SetParent(createUnder);
        nH[count] = new Rigidbody[2];
        nH[count][0] = lHand.GetComponent<Rigidbody>();
        nH[count][1] = rHand.GetComponent<Rigidbody>();
        nP[count] = player;
        Debug.LogFormat("Saved player {0}", player.displayName);
        hands = nH;
        players = nP;
      }

      public override void OnPlayerLeft(VRCPlayerApi player) {
        var count = players.Length;
        var nP = new VRCPlayerApi[count - 1];
        var nH = new Rigidbody[count - 1][];
        var offset = 0;
        for (int i = 0; i < count; i++) {
          if (player == players[i]) {
            offset = -1;
            Destroy(hands[i][0].gameObject);
            Destroy(hands[i][1].gameObject);
            continue;
          }

          nH[i - offset] = hands[i];
          nP[i - offset] = players[i];
        }

        hands = nH;
        players = nP;
      }

      private void TrackHands(VRCPlayerApi.TrackingData source, Rigidbody target) {
        var tPos = source.position;
        target.velocity *= 0.65f;
        var posDelta = tPos - target.worldCenterOfMass;
        var velocity = posDelta / Time.fixedDeltaTime;
        if (!float.IsNaN(velocity.x)) {
          target.velocity += velocity;
        }
      }

      private void Update() {
        if (hands == null) return;
        Debug.Log("Moving hands");
        for (int i = 0; i < hands.Length; i++) {
          Debug.LogFormat("moving hands for {0}", players[i].displayName);
          // if (!players[i].IsUserInVR()) continue;
          var lHand = players[i].GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
          var rHand = players[i].GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
          TrackHands(lHand, hands[i][0]);
          TrackHands(rHand, hands[i][1]);
        }
      }
    }
}