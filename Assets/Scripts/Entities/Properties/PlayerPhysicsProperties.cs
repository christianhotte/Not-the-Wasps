using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerPhysicsProperties : ScriptableObject
{
    /*Description: Scriptable object containing player-specific physics properties (for easy editing and
     *             prototyping).
     */

    [Header("Basic Movement:")]
    public float maxVelocity;  //Player's maximum controllable speed (in units per second)
    public float baseAccel;    //Player base (simple) acceleration (in delta units per second)
    public float idleDecel;    //Player's deceleration when directional input is absent
    public float decelSnapVel; //Speed at which, when decelerating, player will snap to a halt
    public AnimationCurve strafeVelFalloff; //Modifies player speed based on alignment with facing direction (used in Controlled mode)
    
    [Header("Rotation:")]
    [Range(0, 1)] public float mouseTurnStiffness; //How quickly player entity turns toward mouse (lerp percentage per tick) when in Controlled mode
    public float maxControlAngVel; //Maximum angular velocity achievable in Controlled mode (in degrees per second)
    public float minRotThreshold; //Minimum angular rotation velocity (in degrees per second) required for player to register as "rotating" in a given direction

}
