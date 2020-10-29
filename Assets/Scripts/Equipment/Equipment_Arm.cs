using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HotteStuff;

[System.Serializable]
public class Equipment_Arm : Equipment
{
    /*Description: Contains properties and functions common between all equippable appendages (core arm mechanics).
     *             Classes which inherit from this define Arm equipment archetypes with clearly different abilities and
     *             special properties.
     */

    //CLASSES, ENUMS & STRUCTS:
    [System.Serializable] public class Intercept
    {
        //Description: Contains data about an active intercept between an Arm and an enemy

        //Core Components:
        public Equipment_Arm arm;   //The arm involved with this intercept
        public NPC_Enemy enemy;     //The enemy involved with this intercept
        public Collider2D collider; //The enemy's interception area
        //Intercept Data:
        public Vector2 vector;               //Describes the total velocity and direction of this intercept (relative to player)
        public InterceptDirection direction; //The combat-relevant direction of this intercept
        public float duration = 0;           //The duration of this intercept so far (in seconds)
    }
    public enum InterceptDirection //Describes which direction an intercept occurs relative to player
    {
        HeadOn,  //Intercept is a head-on collision between player and enemy
        Neutral, //Intercept does not clearly prefer a given direction
        Rearward //Intercept where enemy hits player from behind
    }
    public enum ArmState //Describes what combat state an arm can be in
    {
        Neutral,       //Arm is not currently engaged in any combat maneuvers
        Grappling,     //Arm is currently grappling an enemy
        Clotheslining, //Arm is currently attacking an enemy from the front
        Backhanding    //Arm is currently attacking an enemy from behind
    }

    //OBJECTS & COMPONENTS:
    [Header("Components:")]
    public Collider2D reachField;    //Collider determining this arm's reach
    public Transform grappleHoldPos; //Transform indicating where enemies will snap to when grappled
    public Transform grabPoint;      //The "hand" of this arm, used to determine local velocity of arm
    public Equipment_Arm otherArm;   //The player's other arm (if this arm is equipped to a player with two arms)

    //VARIABLES:
    [Header("Stats:")]
    public float grappleTimeMod; //GRAPPLE TIME MODIFIER: Increases or decreases amount of time player can keep enemy grappled

    [Header("Mechanical Settings:")]
    [Range(0, 1)] public float grappleSnap; //Determines how quickly enemy snaps to known position when grappled

    //Runtime Status Variables:
    public Direction armSide; //Which side this arm is meant to be equipped on
    private ArmState armState; //This arm's current combat state
    internal List<Intercept> intercepts = new List<Intercept>(); //This arm's current enemy intercepts
    private NPC_Enemy grappledEnemy; //Enemy which this arm is currently grappling (if any)

    private Vector2 localVelocity; //This arm's local linear velocity (at its grab point)
    private Vector2 prevWorldPos = Vector2.zero; //This arm's previous world position, used to determine linear velocity

//==|CORE LOOPS|==-------------------------------------------------------------------------------------------------
    public override void FixedUpdate()
    {
        base.FixedUpdate(); //Call base fixedUpdate function
        UpdatePhysics(); //Update physics-related data on this piece of equipment
        CheckForIntercepts(); //Check for any intercepts with enemies
    }

//==|CORE ARM FUNCTIONS|==-----------------------------------------------------------------------------------------
    public override void Initialize()
    {
        base.Initialize(); //Call base init function

        //Adjust Arm Based on Side Setting:
        SetSide(armSide); //Set arm side to given side at initialization
    }
    public override void Equip(Entity_Player targetPlayer)
    {
        base.Equip(targetPlayer); //Call base equip function

        if (armSide == Direction.Left) //Equip as LEFT ARM...
        {
            if (player.leftArm != null) //If this arm is REPLACING another...
            {

            }
            player.leftArm = this; //Establish connecction on playercontroller side
            if (player.rightArm != null) //If player has another arm...
            {
                otherArm = player.rightArm; //Get controller from other arm
                otherArm.otherArm = this;   //Establish connection on other arm side
            }
        }
        else if (armSide == Direction.Right) //Equip as RIGHT ARM...
        {
            if (player.rightArm != null) //If this arm is REPLACING another...
            {

            }
            player.rightArm = this; //Establish connection on playercontroller side
            if (player.leftArm != null) //If player has another arm...
            {
                otherArm = player.leftArm; //Get controller from other arm
                otherArm.otherArm = this;  //Establish connection on other arm side
            }
        }

    }

//==|UPKEEP METHODS|==---------------------------------------------------------------------------------------------
    public void UpdatePhysics()
    {
        //Description: Updates physics variables relating to this piece of equipment

        //Update Local Velocity:
        localVelocity = (grabPoint.position - prevWorldPos.V3()) / Time.fixedDeltaTime; //Get linear velocity based on last position
        prevWorldPos = grabPoint.position; //Log current position to memory
    }
    public void CheckForIntercepts()
    {
        //Description: If arm is available, this method checks for (and identifies) any intercepts with enemies

        //Validations & Initializations:
        if (!equipped) return; //Do not check for intercepts if arm is not currently equipped
        List<Collider2D> overlaps = new List<Collider2D>(); //Initialize list to store overlapping colliders
        ContactFilter2D filter = new ContactFilter2D(); //Initialize contact filter for intercept collider

        //Set Collider Filter:
        filter.useDepth = false;   //Set filter to ignore depth
        filter.useTriggers = true; //Set filter to use trigger colliders
        filter.SetLayerMask(LayerMask.GetMask("InterceptionFields")); //Set filter to mask out all colliders other than interception fields

        //Check Current Overlaps:
        reachField.OverlapCollider(filter, overlaps); //Populate list of current interception field overlaps
        
        //Compare With Known Intercepts:
        for (int x = 0; x < intercepts.Count; x++) //Iterate through list of known intercepts...
        {
            //Initializations & Validations:
            Intercept thisIntercept = intercepts[x]; //Get shorthand for this intercept
            Collider2D interceptCollider = thisIntercept.collider; //Get known collider from intercept
            bool foundCollider = false; //Initialize marker to track whether or not intercept could find its collider

            //Check for Continuity:
            for (int y = 0; y < overlaps.Count; y++) //Iterate through list of found colliders...
            {
                if (overlaps[y] == interceptCollider) //If collider matches that of this intercept...
                {
                    foundCollider = true; //Indicate that this intercept continues to be valid
                    overlaps.RemoveAt(y); //Remove collider from list of potential new intercepts
                    break; //Break loop and continue to next task
                }
            }

            //Update or Remove Intercept:
            if (foundCollider) //UPDATE intercept, as collider was found successfully:
            {
                UpdateIntercept(thisIntercept); //Update intercept data
            }
            else //REMOVE intercept, as collider can no longer be found:
            {
                thisIntercept.enemy.intercepted = false; //Indicate that enemy is no longer intercepted
                player.intercepts.Remove(thisIntercept); //Remove this intercept from player list
                intercepts.RemoveAt(x); //Remove this intercept from this arm's list
            }
        }

        //Create New Intercepts:
        for (int x = 0; x < overlaps.Count; x++) //For each unclaimed overlap...
        {
            //Initializations & Validations:
            NPC_Enemy interceptedEnemy = overlaps[x].GetComponentInParent<NPC_Enemy>(); //Get enemy controller from overlap collider
            if (interceptedEnemy == null) return;     //Skip function if target does not have enemy controller
            if (interceptedEnemy.intercepted) return; //Skip function if target has already been intercepted by other arm

            //Generate Intercept Core:
            Intercept newIntercept = new Intercept(); //Initialize new intercept
            newIntercept.enemy = interceptedEnemy; //Set intercept enemy
            newIntercept.collider = overlaps[x];   //Set intercept collider
            newIntercept.arm = this;               //Set intercept arm
            //Get Intercept Data:
            UpdateIntercept(newIntercept); //Now that intercept has necessary core data, run an initial update
            //Intercept Generation Cleanup:
            intercepts.Add(newIntercept); //Add new intercept to this arm's list of interceptions
            player.intercepts.Add(newIntercept); //Add new intercept to player's list of interceptions
            interceptedEnemy.intercepted = true; //Indicate to enemycontroller that it has been intercepted
        }

        //..Xx Sub-Methods xX...............................................................................
        void UpdateIntercept(Intercept intercept)
        {
            //Description: Updates intercept data based on relationship between player and intercepted enemy

            //Initializations & Validations:
            Vector2 enemyVel = intercept.enemy.velocity; //Get enemy velocity vector
            Vector2 armVel = localVelocity;              //Get arm velocity vector

            //Find Intercept Vector:
            intercept.vector = -(enemyVel + armVel); //Add velocity vectors to get vector of interception
            Vector2 correctedVector = intercept.vector.Rotate(player.transform.rotation.z); //Get interception vector relative to absolute rotation of player
            

            //Cleanup:
            intercept.duration += Time.fixedDeltaTime; //Update duration (CheckIntercepts should be run during FixedUpdate)

            //..Xx Debuggers xX.................................................................................
            //Debug.DrawLine(grabPoint.position, grabPoint.position + interceptVector.V3());
        }

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
        /*  BACKHAND: Strike an enemy with this arm, dealing damage, redirecting the enemy, and reversing player rotation
         *      Conditions: Player must COUNTER enemy AGAINST direction of rotation (from back)
         */


    }

//==|ADDITIONAL ARM FUNCTIONS|==------------------------------------------------------------------------------------------
    public void SetSide(Direction side)
    {
        //Description: Sets which side this arm is on and updates visual/mechanical elements to match selection

        //Initializations & Validations:
        Vector3 currentScale = transform.localScale; //Save current scale values

        //Apply Side-Specific Settings:
        if (side == Direction.Left) //If arm side is being set to LEFT...
        {
            currentScale.x = Mathf.Abs(currentScale.x); //Set X scale positive for left arm
        }
        else if (side == Direction.Right) //If arm side is being set to RIGHT...
        {
            currentScale.x = -Mathf.Abs(currentScale.x); //Set X scale negative for right arm
        }

        //Cleanup:
        armSide = side; //Set arm side indicator to given side setting
    }

}
