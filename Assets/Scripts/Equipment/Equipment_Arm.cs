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
    public Equipment_Arm otherArm;   //The player's other arm (if this arm is equipped to a player with two arms)

    //VARIABLES:
    //[Header("Stats:")]

    [Header("Mechanical Settings:")]
    [Range(0, 1)] public float grappleSnap; //Determines how quickly enemy snaps to known position when grappled
    public float grappleSnapProx;           //Determines how close enemy must be to grabPoint for it to snap to designated position (when grappled)
    public float minInterceptVel;           //Determines the minimum total velocity an intercept must have to register as directional (as opposed to neutral)

    //Runtime Status Variables:
    public Direction armSide; //Which side this arm is meant to be equipped on
    private ArmState armState; //This arm's current combat state
    private NPC_Enemy grappledEnemy; //Enemy which this arm is currently grappling (if any)

//==|CORE LOOPS|==-------------------------------------------------------------------------------------------------
    public override void FixedUpdate()
    {
        base.FixedUpdate(); //Call base fixedUpdate function
        UpdatePhysics(); //Update physics-related data on this piece of equipment
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

        //Initializations & Validations:
        float timeScale = timeKeeper.timeScale; //Get shorthand for current timescale

        //Update Local Velocity:

    }

//==|COMBAT EVENTS|==----------------------------------------------------------------------------------------------
    public virtual void Grapple(NPC_Enemy enemy)
    {
        /*  GRAPPLE: Catch an enemy, subduing it and allowing you to use its momentum for other abilities
         *      Conditions: Player must COUNTER INTO enemy direction of travel (from front or back)
         *      Notes: Will only grapple the closest enemy to GrabPoint. Ignores other interceptions
         */

        //Initialize Grapple:
        armState = ArmState.Grappling; //Indicate that this arm is now grappling an enemy
        grappledEnemy = enemy;         //Get grappled enemy's controller
        grappledEnemy.grappled = true; //Indicate to enemy that it has been grappled

        //Override Enemy Movement:
        grappledEnemy.locomotionType = Entity_NPC.NPCLocomotionType.Static; //Change enemy move mode to static
        grappledEnemy.velocity = Vector2.zero;      //Cancel enemy velocity
        grappledEnemy.angularVelocity = 0;          //Cancel enemy angular velocity
        grappledEnemy.transform.parent = transform; //Make this arm enemy's parent

        //Override Player Movement:
            

        //..Xx Debuggers xX.................................................................................
        //Debug.Log(intercept.enemy.name + " successfully grappled!");
    }
    public virtual bool Release()
    {
        /*  RELEASE: Release a grappled enemy, doing no damage to it and leaving player vulnerable to counterattack
         *      Conditions: -Player DOES NOT RELEASE enemy within given grapple time
         *                  -OR player attempts to throw enemy at low velocity
         *      Notes: Returns false if function did not execute successfully, true if otherwise
         */

        //Initialization & Validation:
        if (grappledEnemy == null) return false; //If arm does not have grappled enemy, abort release
        if (armState != ArmState.Grappling) return false; //If arm is not currently grappling, abort release

        //Restore Enemy Movement:
        grappledEnemy.locomotionType = Entity_NPC.NPCLocomotionType.Impulse; //Restore enemy movement state
        grappledEnemy.transform.parent = null; //Remove enemy from this arm's list of children

        //Disconnect Variable Links:
        grappledEnemy.grappled = false; //Indicate to enemy that it is no longer grappled
        grappledEnemy = null;           //Forget grappled enemy's controller
        armState = ArmState.Neutral;    //Indicate that this arm has returned to neutral position

        //Cleanup:
        return true; //Confirm that function executed successfully
    }
    public virtual void Throw()
    {
        /*  THROW: Throw a grappled enemy, incapacitating it and turning it into a projectile
         *      Conditions: Player RELEASES counter button with a grappled enemy in corresponding arm
         */

        //Initial Release:
        NPC_Enemy enemy = grappledEnemy; //Save grappled enemy controller before initial release
        if (!Release()) return; //Run enemy release function, check if it executes. If not, abort this function

        //Set Enemy as Projectile:


    }
    public virtual void Clothesline(NPC_Enemy[] enemies)
    {
        /*  CLOTHESLINE: Swipe an enemy with this arm, dealing damage to it while preserving its direction of motion
         *      Conditions: Player must COUNTER enemy AGAINST direction of rotation (from front)
         *      Notes: Will affect every enemy currently intercepted by this arm
         */


    }
    public virtual void Backhand(NPC_Enemy[] enemies)
    {
        /*  BACKHAND: Strike an enemy with this arm, dealing damage, redirecting the enemy, and reversing player rotation
         *      Conditions: Player must COUNTER enemy AGAINST direction of rotation (from back)
         *      Notes: Will affect every enemy currently intercepted by this arm
         */


    }

//==|ADDITIONAL FUNCTIONS|==----------------------------------------------------------------------------------------------
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

//==|BONEYARD|==----------------------------------------------------------------------------------------------------------
    /* PLAYER MOVEMENT OVERRIDING:
     * //Calculate Momentum Direction
        int momentumDirection = 0; //Initialize variable to register which direction to trigger spin
        if (intercept.direction == InterceptDirection.HeadOn) momentumDirection = -1; //Start with negative rotation for head-on intercept
        else if (intercept.direction == InterceptDirection.Rearward) momentumDirection = 1; //Start with positive rotation for rearward intercept
        if (armSide == Direction.Left) momentumDirection *= -1; //Reverse direction for left arm
        //Apply to Player:
        player.rotationMode = Entity_Player.RotationMode.Free; //Switch player rotation mode
        player.momentumTier += momentumDirection; //Add found momentum to player momentum tier value
        //Cleanup:
        if (player.momentumTier == 0) player.rotationMode = Entity_Player.RotationMode.Controlled; //Return player rotation control if its momentum tier is now zero
        if (Mathf.Abs(player.momentumTier) > player.physicsSettings.momentumTierVels.Length) //If new momentum tier is outside set range...
        {
            player.momentumTier = player.physicsSettings.momentumTierVels.Length * (int)Mathf.Sign(player.momentumTier); //Cap momentum tier at maximum value assigned in player physics settings
        }
     */
    /* GRAPPLED ENEMY POSITION SNAPPING:
      * //Update Position of Grappled Enemy:
        if (grappledEnemy != null && !grappledEnemyAtTarget) //If this arm is currently grappling an enemy (and it is not yet in position)...
        {
            //Initializations & Validations:
            Vector2 enemyPos = grappledEnemy.transform.localPosition; //Get local position of enemy
            Vector2 targetPos = grabPoint.localPosition; //Get target position
            float grappleProx = Vector2.Distance(enemyPos, targetPos); //Get distance between enemy and target
            if (grappleProx < grappleSnapProx) //If grappled enemy is close enough to target position...
            {
                grappledEnemy.transform.position = grabPoint.transform.position; //Set grappled enemy position to exact target
                grappledEnemyAtTarget = true; //Indicate that grappled enemy is now at target
            }

            //Move Enemy Toward Target Position:
            Vector2 newEnemyPos = Vector2.Lerp(enemyPos, targetPos, grappleSnap * timeScale); //Get vector lerped between current and target enemy positions
            grappledEnemy.transform.localPosition = newEnemyPos; //Apply new enemy position

        }
      */
    /* INTERCEPT STUFF:
     * 
     * [System.Serializable] public class Intercept
    {
        //Description: Contains data about an active intercept between an Arm and an enemy

        //Core Components:
        public Equipment_Arm arm;   //The arm involved with this intercept
        public NPC_Enemy enemy;     //The enemy involved with this intercept
        public Collider2D collider; //The enemy's interception area
        //Intercept Data:
        public Vector2 vector;               //Describes the total velocity and direction of this intercept (in world space)
        public float angle;                  //Angle (in degrees) between intercept vector and player local axis
        public InterceptDirection direction; //The combat-relevant direction of this intercept
        public float duration = 0;           //The duration of this intercept so far (in seconds)
        public float proximity;              //The distance between center of enemy and intercepting arm's GrabPoint
    }
    public enum InterceptDirection //Describes which direction an intercept occurs relative to player
    {
        HeadOn,  //Intercept is a head-on collision between player and enemy
        Neutral, //Intercept does not clearly prefer a given direction
        Rearward //Intercept where enemy hits player from behind
    }
     * internal List<Intercept> intercepts = new List<Intercept>(); //This arm's current enemy intercepts
     * 
     * public void CheckForIntercepts()
    {
        //Description: If arm is available, this method checks for (and identifies) any intercepts with enemies

        //Validations & Initializations:
        if (!equipped) return;             //Do not check for intercepts if arm is not currently equipped
        if (grappledEnemy != null) return; //Do not check for intercepts if this arm is already grappling an enemy

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
            if (foundCollider) UpdateIntercept(thisIntercept); //UPDATE intercept, as collider was found successfully
            else               RemoveIntercept(thisIntercept); //REMOVE intercept, as collider can no longer be found
        }

        //Create New Intercepts:
        for (int x = 0; x < overlaps.Count; x++) //For each unclaimed overlap...
        {
            //Initializations & Validations:
            NPC_Enemy interceptedEnemy = overlaps[x].GetComponentInParent<NPC_Enemy>(); //Get enemy controller from overlap collider
            if (interceptedEnemy == null) return;     //Skip function if target does not have enemy controller
            if (interceptedEnemy.intercepted) return; //Skip function if target has already been intercepted by other arm
            if (interceptedEnemy.grappled) return;    //Skip function if target is currently being grappled

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
    }
    void UpdateIntercept(Intercept intercept)
    {
        //Description: Updates intercept data based on relationship between player and intercepted enemy

        //Initializations & Validations:
        Vector2 enemyVel = intercept.enemy.velocity; //Get enemy velocity vector
        Vector2 armVel = localVelocity;              //Get arm velocity vector

        //Find Intercept Vector:
        intercept.vector = -(enemyVel + armVel); //Add velocity vectors to get vector of interception
        Vector2 correctedVector = intercept.vector.Rotate(player.transform.rotation.eulerAngles.z); //Get interception vector relative to absolute rotation of player
        intercept.angle = Vector2.Angle(Vector2.down, correctedVector); //Get angle of intercept relative to player
            
        //Determine Intercept Type:
        if (intercept.vector.magnitude < minInterceptVel && //If intercept velocity is not high enough...
            player.rotationMode != Entity_Player.RotationMode.Free) //(make exception for when player is in free-spin mode)...
            { intercept.direction = InterceptDirection.Neutral; }
        else if (intercept.angle <= 90) //If intercept is coming from front...
            { intercept.direction = InterceptDirection.HeadOn; }   //Indicate that intercept is head-on
        else //If intercept is coming from behind...
            { intercept.direction = InterceptDirection.Rearward; } //Indicate that intercept is rearward

        //Cleanup:
        intercept.duration += Time.fixedDeltaTime; //Update duration (CheckIntercepts should be run during FixedUpdate)
        intercept.proximity = Vector2.Distance(grabPoint.position, intercept.enemy.transform.position); //Update proximity

        //..Xx Debuggers xX.................................................................................
        //Debug.DrawLine(grabPoint.position, grabPoint.position + interceptVector.V3());
        //Debug.Log("Intercept Angle = " + intercept.angle);
    }
    void RemoveIntercept(Intercept intercept)
    {
        //Description: Clears intercept from all lists and removes contingencies with other controllers

        intercept.enemy.intercepted = false; //Indicate that enemy is no longer intercepted
        player.intercepts.Remove(intercept); //Remove this intercept from player list
        intercepts.Remove(intercept);        //Remove this intercept from this arm's list
    }
    void RemoveAllIntercepts()
    {
        //Description: Clears all intercepts from this arm from all lists, and removes contingencies with other controllers

        foreach (Intercept intercept in intercepts) //Parse through list of intercepts...
        {
            intercept.enemy.intercepted = false; //Indicate that enemy is no longer intercepted
        }
        player.intercepts = otherArm.intercepts; //Remove all of this arm's intercepts from gross intercept list
        intercepts.Clear(); //Clear local intercept list
    }
     * 
     * if (intercepts.Count == 0) return; //If arm has no intercepts, do nothing
        if (armState != ArmState.Neutral) return; //If arm is not in its neutral state, do nothing

        //Get Closest Intercept:
        Intercept closestIntercept = intercepts[0]; //Initialize closest intercept as first intercept in list
        if (intercepts.Count > 1) foreach (Intercept thisIntercept in intercepts) //Parse through this arm's list of intercepts (if there is more than 1)...
        {
            if (thisIntercept.proximity < closestIntercept.proximity) //If current intercept is closer than closest intercept...
            {
                closestIntercept = thisIntercept; //Make this intercept the new closest intercept
            }
        }
     */

}
