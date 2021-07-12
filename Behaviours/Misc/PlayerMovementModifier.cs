using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Player Movement Modifier")]
  [HelpMessage("Use checkboxes to control which settings to change\n" +
               "SET: sets the parameter to the provided value\n" +
               "ADD: adds the provided value to the current\n" +
               "SUBTRACT: subtracts the provided value from the current")]
  [HelpURL("https://ut.orels.sh/v/v1.x/behaviours/misc-behaviours#player-movement-modifier")]
  [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
  public class PlayerMovementModifier : UdonSharpBehaviour {
    [Horizontal("Jump Impulse", true)][Popup("method", "@changeTypes", true)]
    public int jumpImpulseChangeType;
    [Horizontal("Jump Impulse")]
    public float jumpImpulse = 3;
    [Horizontal("Jump Impulse")]
    public bool setJumpImpulse;

    [Horizontal("Walk Speed", true)][Popup("method", "@changeTypes", true)]
    public int walkSpeedChangeType;
    [Horizontal("Walk Speed")]
    public float walkSpeed = 2;
    [Horizontal("Walk Speed")]
    public bool setWalkSpeed;

    [Horizontal("Run Speed", true)] [Popup("method", "@changeTypes", true)]
    public int runSpeedChangeType;
    [Horizontal("Run Speed")]
    public float runSpeed = 4;
    [Horizontal("Run Speed")]
    public bool setRunSpeed;

    [Horizontal("Strafe Speed", true)] [Popup("method", "@changeTypes", true)]
    public int strafeSpeedChangeType;
    [Horizontal("Strafe Speed")]
    public float strafeSpeed = 2;
    [Horizontal("Strafe Speed")]
    public bool setStrafeSpeed;

    [Horizontal("Gravity Strength", true)] [Popup("method", "@changeTypes", true)]
    public int gravityStrengthChangeType;
    [Horizontal("Gravity Strength")]
    public float gravityStrength = 1f;
    [Horizontal("Gravity Strength")]
    public bool setGravityStrength;

    private VRCPlayerApi player;

    [NonSerialized] public string[] changeTypes = new string[] {
      "SET", "ADD", "SUBTRACT"
    };
    
    private void Start() {
      player = Networking.LocalPlayer;
    }

    public void Trigger() {
      if (setJumpImpulse) {
        SetJump();
      }
      if (setWalkSpeed) {
        SetWalk();
      }
      if (setRunSpeed) {
        SetRun();
      }
      if (setStrafeSpeed) {
        SetStrafe();
      }
      if (setGravityStrength) {
        SetGravity();
      }
    }

    private void SetJump() {
      var newValue = 0f;
      switch (jumpImpulseChangeType) {
        case 0: {
          newValue = jumpImpulse;
          break;
        }
        case 1: {
          newValue = player.GetJumpImpulse() + jumpImpulse;
          break;
        }
        case 2: {
          newValue = Mathf.Clamp(player.GetJumpImpulse() - jumpImpulse, Mathf.Epsilon, float.MaxValue);
          break;
        }
      }
      player.SetJumpImpulse(newValue);
    }
    
    private void SetWalk() {
      var newValue = 0f;
      switch (walkSpeedChangeType) {
        case 0: {
          newValue = walkSpeed;
          break;
        }
        case 1: {
          newValue = player.GetWalkSpeed() + walkSpeed;
          break;
        }
        case 2: {
          newValue = Mathf.Clamp(player.GetWalkSpeed() - walkSpeed, Mathf.Epsilon, float.MaxValue);
          break;
        }
      }
      player.SetWalkSpeed(newValue);
    }
    
    private void SetRun() {
      var newValue = 0f;
      switch (runSpeedChangeType) {
        case 0: {
          newValue = runSpeed;
          break;
        }
        case 1: {
          newValue = player.GetRunSpeed() + runSpeed;
          break;
        }
        case 2: {
          newValue = Mathf.Clamp(player.GetRunSpeed() - runSpeed, Mathf.Epsilon, float.MaxValue);
          break;
        }
      }
      player.SetRunSpeed(newValue);
    }
    
    private void SetStrafe() {
      var newValue = 0f;
      switch (strafeSpeedChangeType) {
        case 0: {
          newValue = strafeSpeed;
          break;
        }
        case 1: {
          newValue = player.GetStrafeSpeed() + strafeSpeed;
          break;
        }
        case 2: {
          newValue = Mathf.Clamp(player.GetStrafeSpeed() - strafeSpeed, Mathf.Epsilon, float.MaxValue);
          break;
        }
      }
      player.SetStrafeSpeed(newValue);
    }
    
    private void SetGravity() {
      var newValue = 0f;
      switch (gravityStrengthChangeType) {
        case 0: {
          newValue = gravityStrength;
          break;
        }
        case 1: {
          newValue = player.GetGravityStrength() + gravityStrength;
          break;
        }
        case 2: {
          newValue = Mathf.Clamp(player.GetGravityStrength() - gravityStrength, Mathf.Epsilon, float.MaxValue);
          break;
        }
      }
      player.SetGravityStrength(newValue);
    }
  }
}
