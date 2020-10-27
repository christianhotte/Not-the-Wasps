using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] [CreateAssetMenu]
public class PlayerPhysicsProperties : ScriptableObject
{
    /*Description: Scriptable object containing player-specific physics properties (for easy editing and
     *             prototyping).
     */

    public float baseMass; //Player entity's base mass, which affects all control properties
    [Header("Basic Movement:")]
    public float maxVelocity;  //Player's maximum controllable speed (in units per second)
    public float baseAccel;    //Player base (simple) acceleration (in delta units per second)
    public float idleDecel;    //Player's deceleration when directional input is absent
    public float decelSnapVel; //Speed at which, when decelerating, player will snap to a halt
    [Header("Controlled Rotation:")]
    [Range(0, 1)] public float mouseTurnStiffness; //How quickly player entity turns toward mouse (lerp percentage per tick) when in Controlled mode
    public float maxControlAngVel; //Maximum angular velocity achievable in Controlled mode (in degrees per second)
    public AnimationCurve strafeVelFalloff; //Modifies player speed based on alignment with facing direction (used in Controlled mode)
    [Header("Free Rotation:")]
    [Range(0, 1)] public float momentumSnapStiffness; //How quickly player adjusts actual speed between momentum tiers (lerp percentage per tick)
    public float momentumSnapThresh; //When dealing with momentum, this is the error which will be used to snap velocity to known numbers (in degrees per second)
    public float[] momentumTierVels; //How quickly player entity spins when in each respective momentum tier (in degrees per second)

}
