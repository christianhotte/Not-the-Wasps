using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Equipment_Arm : Equipment_Base
{
    /*Description: Contains properties and functions common between all equippable appendages (core arm mechanics).
     *             Classes which inherit from this define Arm equipment archetypes with clearly different abilities and
     *             special properties.
     */

    //CLASSES, ENUMS & STRUCTS:
    [System.Serializable] public class Intercept
    {
        //Description: Contains data about an active intercept between an Arm and an enemy


    }
    public enum InterceptDirection //Describes which direction an intercept occurs relative to player
    {
        HeadOn,  //Intercept is a head-on collision between player and enemy
        SideOn,  //Intercept is a side-on collision with enemy hitting player perpendicularly
        Rearward //Intercept where enemy hits player from behind
    }

    //OBJECTS & COMPONENTS:
    [Header("Components:")]
    public Collider2D reachField;    //Collider determining this arm's reach
    public Transform grappleHoldPos; //Transform indicating where enemies will snap to when grappled

    //VARIABLES:
    [Header("Stats:")]
    public float grappleTimeMod; //GRAPPLE TIME MODIFIER: Increases or decreases amount of time player can keep enemy grappled

    [Header("Mechanical Settings:")]
    [Range(0, 1)] public float grappleSnap; //Determines how quickly enemy snaps to known position when grappled

    //Runtime Memory Variables:
    
    internal Intercept[] intercepts; //This arm's current enemy intercepts
    
//==|UPKEEP METHODS|==---------------------------------------------------------------------------------------------
    public void CheckForIntercepts()
    {
        //Description: If arm is available, this method checks for (and identifies) any intercepts with enemies

        //Validations & Initializations:


    }

//==|INPUT EVENTS|==-----------------------------------------------------------------------------------------------
    

//==|COMBAT EVENTS|==----------------------------------------------------------------------------------------------
    public virtual void Grapple()
    {
        /*  GRAPPLE: Catch an enemy, subduing it and allowing you to use its momentum for other abilities
         *      Conditions: Player must COUNTER INTO enemy direction of travel (from front or back)
         */


    }
    public virtual void Release()
    {
        /*  RELEASE: Release a grappled enemy, doing no damage to it and leaving player vulnerable to counterattack
         *      Conditions: Player DOES NOT RELEASE enemy within given grapple time for that intercept
         */


    }
    public virtual void Throw()
    {
        /*  THROW: Throw a grappled enemy, incapacitating it and turning it into a projectile
         *      Conditions: Player RELEASES counter button with a grappled enemy in corresponding arm
         */


    }
    public virtual void Clothesline()
    {
        /*  CLOTHESLINE: Swipe an enemy with this arm, dealing damage to it while preserving its direction of motion
         *      Conditions: Player must COUNTER enemy AGAINST direction of rotation (from front)
         */


    }
    public virtual void Backhand()
    {
        /*  BACKHAND: Strike an enemy with this arm, dealing damage and redirecting enemy
         *      Conditions: Player must COUNTER enemy AGAINST direction of rotation (from back)
         */


    }

}
