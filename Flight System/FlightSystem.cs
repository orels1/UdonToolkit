
using System;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Wrapper.Modules;

[CustomName("Flight System")]
[HelpURL("https://ut.orels.sh/systems/flight-system")]
public class FlightSystem : UdonSharpBehaviour
{
  [SectionHeader("Tracking References")][UTEditor]
  public Transform rightHand;
  public Transform leftHand;
  public Transform head;
  
  [SectionHeader("General Settings")][UTEditor]
  public float maxSpeed = 20f;
  public bool allowBoosting = true;
  public float boostLength = 10f;
  public float boostSpeed = 30f;
  public bool allowGliding = true;
  public LayerMask groundLayers;
  public bool allowHover = false;
  
  [SectionHeader("Comfort Settings")] [UTEditor]
  public float accelerationSmoothFactor = 2f;
  public float decelerationSmoothFactor = 10f;
  public float groundSlowMaxHeight = 4f;
  [RangeSlider(0.1f, 0.99f)][HelpBox("This defines the minimum player speed during takeoff/landing")][UTEditor]
  public float minGroundSpeedModifier = 0.6f;

  [SectionHeader("Desktop Keybinds")] [UTEditor]
  public string glideTakeOffKey = "t";
  public string freezeInPlaceKey = "f";
  public string forceRespawnKey = "p";

  [HideInInspector]
  public bool flying;
  [HideInInspector]
  public float downforce = 10f;
  
  [SectionHeader("Initial State")][UTEditor]
  public bool flightRestricted;
  [HelpBox("This position will be used by desktop users when force-respawning")] [UTEditor]
  public Transform resetPosition;

  // one handed mode toggles
  [NonSerialized] public bool oneHanded;
  [NonSerialized] public bool oneHandedHead = true;
  [NonSerialized] public bool oneHandedLeftHand;
  [NonSerialized] public bool oneHandedRightHand;

  // public readouts for the UI
  [HideInInspector]
  public float rSpeed;
  [HideInInspector]
  public float rSpeedDot;
  [HideInInspector]
  public float rAltitude;
  
  // Callbacks
  [SectionHeader("Callbacks")] [Horizontal("OnFlightStart")][UTEditor]
  public UdonSharpBehaviour onFlightStart;
  [Horizontal("OnFlightStart")] [Popup("behaviour", "@onFlightStart", true)][UTEditor]
  public string onFlightStartEvent;
  [Horizontal("OnFlightEnd")][UTEditor]
  public UdonSharpBehaviour onFlightEnd;
  [Horizontal("OnFlightEnd")] [Popup("behaviour", "@onFlightEnd", true)][UTEditor]
  public string onFlightEndEvent;
  [Horizontal("OnHoverStart")][UTEditor]
  public UdonSharpBehaviour onHoverStart;
  [Horizontal("OnHoverStart")] [Popup("behaviour", "@onHoverStart", true)][UTEditor]
  public string onHoverStartEvent;
  [Horizontal("OnHoverEnd")][UTEditor]
  public UdonSharpBehaviour onHoverEnd;
  [Horizontal("OnHoverEnd")] [Popup("behaviour", "@onHoverEnd", true)][UTEditor]
  public string onHoverEndEvent;
  [Horizontal("OnGlideStart")][UTEditor]
  public UdonSharpBehaviour onGlideStart;
  [Horizontal("OnGlideStart")] [Popup("behaviour", "@onGlideStart", true)][UTEditor]
  public string onGlideStartEvent;
  [Horizontal("OnGlideEnd")][UTEditor]
  public UdonSharpBehaviour onGlideEnd;
  [Horizontal("OnGlideEnd")] [Popup("behaviour", "@onGlideEnd", true)][UTEditor]
  public string onGlideEndEvent;

  private bool boosting;
  private bool gliding;
  private bool hovering;
  private float boostEnd;
  private bool isDesktop = true;
  private float origMaxSpeed;
  private VRCPlayerApi player;
  private float handsDot;

  private void Start() {
    origMaxSpeed = maxSpeed;
    player = Networking.LocalPlayer;
    isDesktop = !player.IsUserInVR();
  }

  /// <summary>
  /// Grounds the player, resets all the gliding/flying/boosting states and player gravity
  /// </summary>
  private void Ground() {
    if (flying && onFlightEnd) {
      onFlightEnd.SendCustomEvent(onFlightEndEvent);
    }
    if (gliding && onGlideEnd) {
      onGlideEnd.SendCustomEvent(onGlideEndEvent);
    }
    flying = false;
    boosting = false;
    gliding = false;
    boostEnd = 0f;
    rSpeed = 0f;
    player.SetGravityStrength();
  }

  /// <summary>
  /// Checks if the boost has ended and restores original speed if needed
  /// </summary>
  private void CheckBoost() {
    if (!(Time.time >= boostEnd)) return;
    maxSpeed = origMaxSpeed;
    boosting = false;
  }

  /// <summary>
  /// Sets final velocity
  /// </summary>
  /// <remarks>This is mainly provided for adding easier debugging</remarks>
  /// <param name="velocity">Velocity to assing</param>
  private void SetVelocity(Vector3 velocity) {
    player.SetVelocity(velocity);
  }

  /// <summary>
  /// Calculates distance to the closest ground collider
  /// </summary>
  /// <returns>Distance to ground</returns>
  private float GetGroundDistance() {
    var groundRayDirection = Vector3.down;
    var playerBase = player.GetPosition();
    RaycastHit hit;
    if (Physics.Raycast(playerBase, groundRayDirection, out hit, 200f, groundLayers)) {
      return hit.distance;
    }
    return 200f;
  }

  /// <summary>
  /// Checks whether the player is gliding which is based on 4 conditions:
  /// - The player should be no more than 2 units above the ground
  /// - The player should have hands pointing >80% forward
  /// - The player should not be pointing more than 20% above the head forward vector
  /// - The desktop takeoff button isn't pressed
  /// </summary>
  /// <param name="groundDistance">Distance to the closest ground collider</param>
  /// <param name="upDot">Dot product between the head forward vector and the combined hands forward vector</param>
  /// <returns>Whether player is gliding</returns>
  private bool IsGliding(float groundDistance, float upDot) {
    var takeOffPressed = Input.GetKey(glideTakeOffKey);
    return (groundDistance <= 2f && handsDot > 0.8) && upDot <= 0.2 && !takeOffPressed;
  }

  /// <summary>
  /// Checks for ground in close proximity to the player and calculates the angle.
  /// </summary>
  /// <returns>
  /// Angle to be used to offset the glide system movement vector for proper acceleration/decelartion when going up and down the slope
  /// </returns>
  private float CheckGroundAngle() {
    var groundRayDirection = Vector3.down;
    const int mask = 1 << 11;
    var playerBase = player.GetPosition();
    RaycastHit hit;
    if (Physics.SphereCast(playerBase, 0.25f, groundRayDirection, out hit, 1.5f, mask)) {
      var groundSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);
      var temp = Vector3.Cross(hit.normal, Vector3.up);
      var groundSlopeDir = Vector3.Cross(temp, hit.normal);
      // remapping the angle to be from - 90 to 90
      var movementSlopeAngle = Vector3.Angle(groundSlopeDir, head.forward) - 90;
      var modifier = movementSlopeAngle / 90;
      return groundSlopeAngle * -modifier;
    }

    return 0f;
  }

  private void Update() {
    CheckBoost();
    // check if we entered flight restriction zone
    if (flightRestricted) {
      Ground();
      return;
    } 
    if (player.IsPlayerGrounded()) {
      Ground();
      return;
    }

    if (!flying && onFlightStart) {
      onFlightStart.SendCustomEvent(onFlightStartEvent);
    }
    
    flying = true;
    // VRChat has weird inconsistencies when setting gravity to 0 and keeps you floating down
    player.SetGravityStrength(0.0001f);
    
    var leftForward = leftHand.forward;
    var rightForward = rightHand.forward;
    var headForward = head.forward;
    var currVelocity = player.GetVelocity();
    
    // use hands dot product to control forward speed
    handsDot = Vector3.Dot(leftForward, rightForward);
    // remap from -1 1 to 0 1
    handsDot = (handsDot + 1f) / 2f;
    var currSpeed = currVelocity.magnitude;
    
    // use the trigger press to control speed in one handed mode
    if (oneHanded) {
      handsDot = 1 - Mathf.Max(Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger"),
        Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger"));
    }
    // determine target speed as current dot product (how much hands are aligned) * current maximum
    var newSpeed = handsDot * maxSpeed;
    
    // get the smoothing factor for deceleration
    var lerpSpeedTime = decelerationSmoothFactor * Time.deltaTime;
    
    // get the smoothing factor for acceleration
    if (newSpeed > currSpeed) {
      lerpSpeedTime = accelerationSmoothFactor * Time.deltaTime;
    }
    
    // final smoothed speed value
    var lerpedSpeed = Mathf.Lerp(currSpeed, newSpeed, lerpSpeedTime);
    
    // we use left + right combined vector to get the direction
    var handDirection = Vector3.Normalize(leftForward + rightForward);
    
    // for desktop - we use the head vector instead
    if (isDesktop) {
      handDirection = headForward;
    }
    
    // allow to select tracking source for one handed
    if (oneHanded) {
      if (oneHandedHead) {
        handDirection = headForward;
      }

      if (oneHandedRightHand) {
        handDirection = rightForward;
      }

      if (oneHandedLeftHand) {
        handDirection = leftForward;
      }
    }
    
    // check if we are pointing forwards or backwards
    var headDot = Vector3.Dot(handDirection, headForward);
    Vector3 forwardVelocity;
    
    // get the  direction amount
    var downDot = Vector3.Dot(handDirection, Vector3.down);
    if (downDot < 0f) downDot = 0f;

    // one handed flight does not have any special backward flight logic due to control limitations
    if (headDot < 0f && !oneHanded) {
      forwardVelocity = BackwardFlight(handDirection, currSpeed, lerpedSpeed, downDot);
    }
    else {
      forwardVelocity = ForwardFlight(handDirection, lerpedSpeed);
    }
    
    rSpeed = lerpedSpeed;
    rSpeedDot = handsDot;
    
    // hovering on desktop
    if (isDesktop && Input.GetKeyDown(freezeInPlaceKey) && allowHover) {
      hovering = !hovering;
      if (hovering) {
        if (onHoverStart) {
          onHoverStart.SendCustomEvent(onHoverStartEvent);
        }
        return;
      }
      else {
        if (onHoverEnd) {
          onHoverEnd.SendCustomEvent(onHoverEndEvent);
        }
      }
    }

    // hovering in vr
    if (!isDesktop && !oneHanded) {
      if (handsDot < -0.9 || headDot < 0f) {
        if (!hovering) {
          hovering = true;
          if (onHoverStart) {
            onHoverStart.SendCustomEvent(onHoverStartEvent);
          }
        }
      }
      else if (hovering) {
        hovering = false;
        if (onHoverEnd) {
          onHoverEnd.SendCustomEvent(onHoverEndEvent);
        }
      }
    }

    // reset back to start
    if (Input.GetKeyDown(forceRespawnKey)) {
      if (resetPosition) {
        player.TeleportTo(new Vector3(0, 0.2f, 0), Quaternion.identity);
        Ground();
        return;
      }
      
    }
    
    // we do not want to prevent the players from descending while hovering
    // so we only lock them in place when they're mostly t-posing
    if (hovering || hovering && !isDesktop && downDot < 0.1) {
      SetVelocity(new Vector3(0, 0, 0));
      return;
    }
    
    SetVelocity(forwardVelocity);
  }
  
  /// <summary>
  /// Calculates corrected forward flight velocity
  /// </summary>
  /// <param name="handDirection">Combined left + right hand forward vector</param>
  /// <param name="lerpedSpeed">Target Speed</param>
  /// <returns></returns>
  private Vector3 ForwardFlight(Vector3 handDirection, float lerpedSpeed) {
    var groundDistance = GetGroundDistance();
    rAltitude = groundDistance;
    // skip slowdown or glide when we're more than 10 units above the ground
    if (!(groundDistance <= 10)) return handDirection * lerpedSpeed;
    
    // rest of the code is Glide and takeoff / Land only
    
    var upDot = Vector3.Dot(handDirection, head.up);
    if (allowGliding && IsGliding(groundDistance, upDot)) {
      if (!gliding) {
        gliding = true;
        if (onGlideStart) {
          onGlideStart.SendCustomEvent(onGlideStartEvent);
        }
      }
      // gliding keeps the player ~1 unit above the ground
      var correctionDistance = 1f - groundDistance;
      
      // desktop players have issues with slopes as they can't move arms independently
      // so we provide extra slope checks to maintain their speed along slopes
      if (isDesktop) {
        var groundAngle =  CheckGroundAngle();
        handDirection = Quaternion.AngleAxis(groundAngle, head.right) * handDirection;
      }
      
      // for VR we just correct the players vertically and leave the rest for the player to handle
      handDirection = new Vector3(handDirection.x, correctionDistance, handDirection.z);
      return handDirection * lerpedSpeed;
    }

    if (gliding) {
      gliding = false;
      if (onGlideEnd) {
        onGlideEnd.SendCustomEvent(onGlideEndEvent);
      }
    }

    // takeoff and Landing
    
    // we make sure that the landing and takeoff speed doesnt fall below 20%
    // then we scale player's speed cap over 10 units above the ground collider for smooth takeoff / landing
    var remapped = Mathf.Max(minGroundSpeedModifier, groundDistance / groundSlowMaxHeight);

    // only slow us down when going above 20% speed
    // this makes sure the minimum speed is maintained at all times unless the player wants to stop
    if (lerpedSpeed / maxSpeed > minGroundSpeedModifier) {
      lerpedSpeed = Mathf.Min(lerpedSpeed, maxSpeed * remapped);
    }
    return handDirection * lerpedSpeed;
  }

  /// <summary>
  /// Calculates corrected backward velocity
  /// </summary>
  /// <param name="handDirection">Combined left + right hand forward vector</param>
  /// <param name="currSpeed">Current flight speed</param>
  /// <param name="lerpedSpeed">Target speed</param>
  /// <returns></returns>
  private Vector3 BackwardFlight(Vector3 handDirection, float currSpeed, float lerpedSpeed, float downDot) {
    
    var downSpeed = downDot * downforce;
    var lerpSpeedTime = 80f * Time.deltaTime;
    lerpedSpeed = Mathf.Lerp(currSpeed, downSpeed, lerpSpeedTime);
    return new Vector3(0f, -lerpedSpeed, 0f);
  }

  /// <summary>
  /// Enables boost and stores old speed to be restored later
  /// </summary>
  public void Boost() {
    if (!allowBoosting) return;
    if (!boosting) {
      origMaxSpeed = maxSpeed;
    }
    maxSpeed = boostSpeed;
    boostEnd = Time.time + boostLength;
    boosting = true;
  }

  // Convenience methods
  /// <summary>
  /// Prevents further flight and grounds the player if currently flying
  /// </summary>
  public void RestrictFlight() {
    flightRestricted = true;
  }

  /// <summary>
  /// Allows flight
  /// </summary>
  public void AllowFlight() {
    flightRestricted = false;
  }
  
  /// <summary>
  /// Disallows boosting without stopping the current boost
  /// </summary>
  public void DisableBoosting() {
    allowBoosting = false;
  }

  /// <summary>
  /// Allows boosting
  /// </summary>
  public void EnableBoosting() {
    allowBoosting = true;
  }

  /// <summary>
  /// Stops the currently active boost (if any)
  /// </summary>
  public void StopBoost() {
    maxSpeed = origMaxSpeed;
    boosting = false;
    boostEnd = 0;
  }

  /// <summary>
  /// Disallows gliding, will interrupt current glide
  /// </summary>
  public void DisableGliding() {
    allowGliding = false;
  }

  /// <summary>
  /// Allows gliding
  /// </summary>
  public void EnableGliding() {
    allowGliding = true;
  }

  /// <summary>
  /// Immediately stops glide and grounds the player
  /// </summary>
  public void StopGliding() {
    Ground();
  }

  /// <summary>
  /// Enables one handed flight mode and sets reference target to the head
  /// </summary>
  public void EnableOneHandedMode() {
    oneHanded = true;
    oneHandedHead = true;
    oneHandedLeftHand = false;
    oneHandedRightHand = false;
  }

  /// <summary>
  /// Sets one handed mode reference target to the head
  /// </summary>
  public void SetOneHandedTargetHead() {
    oneHandedHead = true;
    oneHandedLeftHand = false;
    oneHandedRightHand = false;
  }

  /// <summary>
  /// Sets one handed mode reference target to the left hand
  /// </summary>
  public void SetOneHandedTargetLeftHand() {
    oneHandedHead = false;
    oneHandedLeftHand = true;
    oneHandedRightHand = false;
  }
  
  /// <summary>
  /// Sets one handed mode reference target to the right hand
  /// </summary>
  public void SetOneHandedTargetRightHand() {
    oneHandedHead = false;
    oneHandedLeftHand = false;
    oneHandedRightHand = true;
  }

  /// <summary>
  /// Disables one handed flight mode and sets reference target to the head
  /// </summary>
  public void DisableOneHandedMode() {
    oneHanded = false;
    oneHandedHead = true;
    oneHandedLeftHand = false;
    oneHandedRightHand = false;
  }
}
