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
    public enum ManeuverState //Describes which maneuver arm is currently executing
    {
        Neutral,       //Arm is not currently engaged in any combat maneuvers
        Grappling,     //Arm is currently grappling an enemy
        Clotheslining, //Arm is currently attacking an enemy from the front
        Backhanding    //Arm is currently attacking an enemy from behind
    }

    //OBJECTS & COMPONENTS:
    [Header("Components:")]
    public Equipment_Arm otherArm; //The player's other arm (if this arm is equipped to a player with two arms)

    //VARIABLES:
    [Header("Stats:")]
    public AnimationCurve rangeExtensionCurve; //Curve representing how much range extends (in [value] degrees) based on angular velocity (in [t] degrees per second)
    [MinMaxSlider(-180, 180)] public Vector2 grabRange;        //The range (in degrees) from which this arm can grab an enemy (when angular speed is at 0)
    [MinMaxSlider(-180, 180)] public Vector2 clotheslineRange; //The range (in degrees) from which this arm can clothesline an enemy (when angular speed is at 0)
    [MinMaxSlider(-180, 180)] public Vector2 backhandRange;    //The range (in degrees) from which this arm can backhand an enemy (when angular speed is at 0)
    internal Vector2[] maneuverRanges = new Vector2[3];        //Initialize neat array to store trio of maneuver ranges (for easy parsing later)
    [Space()]
    //Runtime Status Variables:
    public Direction armSide; //Which side this arm is meant to be equipped on
    internal ManeuverState currentManeuver; //The maneuver this arm is currently executing (if any)
    internal Entity_Player.Intercept maneuverIntercept; //Intercept data for whichever maneuver this arm is currently performing

    //==|CORE LOOPS|==-------------------------------------------------------------------------------------------------
    public override void Update()
    {
        base.Update(); //Call base function

        UpdateManeuverUI(); //Update any ongoing maneuvers
    }
    public void OnDrawGizmosSelected()
    {
        DrawManeuverRanges(); //Draw lines showing the ranges of maneuvers for easy editing
    }

//==|CORE ARM FUNCTIONS|==-----------------------------------------------------------------------------------------
    public override void Initialize()
    {
        //Early Init Functions:
        SetSide(armSide); //Set arm side to given side at initialization

        //Base Init:
        base.Initialize(); //Call base init function

        //Initialize Variables:
        maneuverRanges = new Vector2[] { grabRange, clotheslineRange, backhandRange }; //Package individual maneuver range variables into neat array
    
    }
    public override void Equip(Entity_Player targetPlayer)
    {
        base.Equip(targetPlayer); //Call base equip function

        //Establish Controller Dependencies:
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

        //Move Arm to Position:
        Vector2 posDifference = player.transform.position - mountPoint.position; //Find current difference in position between arm and player
        transform.position += posDifference.V3(); //Move arm into position

    }

//==|COMBAT EVENTS + MANEUVERS|==----------------------------------------------------------------------------------
    public virtual void Grapple(Entity_Player.Intercept intercept)
    {
        /*  GRAPPLE: Catch an enemy, subduing it and allowing the player to throw it as a projectile
         */

        //Initialize Grapple:
        player.grappling = true;                      //Indicate to player that it is now grappling
        currentManeuver = ManeuverState.Grappling;    //Indicate that this arm is now grappling an enemy
        maneuverIntercept = intercept;                //Set current maneuver intercept
        Entity_Enemy enemy = maneuverIntercept.enemy; //Get shorthand for enemy being grappled

        //Suspend Intercept Status:
        intercept.suspended = true; //Suspend intercept (causing it to lose super status next UpdateIntercept)

        //Override Enemy Movement:
        enemy.currentMoveType = Entity.LocomotionType.Static;    //Change enemy move mode to STATIC
        enemy.combatStatus = Entity_Enemy.CombatStatus.Grappled; //Change enemy combat status to GRAPPLED
        enemy.velocity = Vector2.zero;                           //Cancel enemy velocity
        enemy.angularVelocity = 0;                               //Cancel enemy angular velocity
        enemy.transform.parent = transform;                      //Make this arm enemy's parent

        //Override Player Movement:
        

        //..Xx Debuggers xX.................................................................................
        //Debug.Log(intercept.enemy.name + " successfully grappled!");
    }
    internal virtual void Release()
    {
        /*  RELEASE: Release a grappled enemy, doing no damage to it and leaving player vulnerable to counterattack
         */

        //Initialization & Validation:
        Entity_Enemy enemy = maneuverIntercept.enemy; //Get shorthand for enemy being grappled

        //Restore Enemy Movement:
        enemy.currentMoveType = Entity.LocomotionType.Impulse;  //Restore enemy movement state
        enemy.combatStatus = Entity_Enemy.CombatStatus.Default; //Restore enemy combat status

        //Cleanup:
        EndGrapple(); //Clean up grapple variables
    }
    internal virtual void Throw()
    {
        /*  THROW: Throw a grappled enemy, incapacitating it and turning it into a projectile
         *      -ThrowVector: Vector describing the angle and force at which enemy will be thrown (compatible with GetThrowVector method)
         */

        //Initialization & Validation:
        Entity_Enemy enemy = maneuverIntercept.enemy; //Get shorthand for enemy being grappled

        //Set Enemy Movement:
        enemy.currentMoveType = Entity.LocomotionType.Impulse;     //Set enemy movement state
        enemy.combatStatus = Entity_Enemy.CombatStatus.Projectile; //Set enemy combat status to PROJECTILE
        enemy.velocity = GetThrowVector(); //Apply current throw vector directly to enemy velocity

        //Cleanup:
        EndGrapple(); //Clean up grapple variables
    }
    
    public virtual void Clothesline(Entity_Enemy[] enemies)
    {
        /*  CLOTHESLINE: Swipe an enemy with this arm, dealing damage to it while preserving its direction of motion
         *      Conditions: Player must COUNTER enemy AGAINST direction of rotation (from front)
         *      Notes: Will affect every enemy currently intercepted by this arm
         */


    }
    public virtual void Backhand(Entity_Enemy[] enemies)
    {
        /*  BACKHAND: Strike an enemy with this arm, dealing damage, redirecting the enemy, and reversing player rotation
         *      Conditions: Player must COUNTER enemy AGAINST direction of rotation (from back)
         *      Notes: Will affect every enemy currently intercepted by this arm
         */


    }

//==|UTILITY FUNCTIONS|==----------------------------------------------------------------------------------------
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
    private void EndGrapple()
    {
        //Description: Cleans up all necessary variables and dependencies after an enemy grapple has been ended (by THROW or RELEASE)

        //Unlink Enemy Transform:
        maneuverIntercept.enemy.transform.parent = null; //Remove enemy from this arm's list of children

        //Resolve Intercept Status:
        maneuverIntercept.suspended = false; //Indicate that intercept is no longer suspended
        maneuverIntercept.resolved = true;   //Indicate that intercept relating to this maneuver has been resolved

        //Cleanup:
        maneuverIntercept = null;    //Forget grappled enemy's intercept
        currentManeuver = ManeuverState.Neutral; //Indicate that this arm has returned to neutral position
        if (otherArm.currentManeuver != ManeuverState.Grappling) player.grappling = false; //Set player grappling state to false (after confirming with other arm)
    }
    private Vector2 GetThrowVector()
    {
        //Description: Determines the angle and force at which player can currently throw a grappled enemy (relative to world coordinate grid)

        //Initializations & Validations:
        if (maneuverIntercept == null) //If arm does not currently contain intercept data for an enemy...
        {
            Debug.LogError("Arm attempted to get throw direction for nonexistent intercept."); //Log error
            return Vector2.zero; //Return blank vector
        }
        if (currentManeuver != ManeuverState.Grappling) //If arm is not currently grappling an enemy...
        {
            Debug.LogError("Arm attempted to get throw direction while not grappling."); //Log error
            return Vector2.zero; //Return blank vector
        }
        Vector2 throwVector = Vector2.up; //Initialize vector to return after calculation

        //Harvest Necessary Data:
        Vector2 playerPosition = player.transform.position;                 //Get player position in world space
        Vector2 enemyPosition = maneuverIntercept.enemy.transform.position; //Get enemy position in world space
        float playerRotDirection = Mathf.Sign(player.angularVelocity);      //Get player direction of rotation

        //Get Angle:
        Vector2 enemyRelPos = enemyPosition - playerPosition;               //Get enemy position relative to position of player
        float enemyRelAngle = Vector2.SignedAngle(Vector2.up, enemyRelPos); //Get angle between enemy and player in world space
        float throwAngle = enemyRelAngle + (90 * playerRotDirection);       //Get tangent angle direction according to player rotation

        //Get Force:
        float throwForce = maneuverIntercept.force; //Get initial force from maneuver intercept

        //Cleanup:
        throwVector = throwVector.Rotate(throwAngle); //Apply calculated angle to final return vector
        throwVector *= throwForce;                    //Apply calculated force to final return vector

        return throwVector; //Return found vector
    }
    public void InitiateThrowSequence()
    {
        //Description: Begins automatic animation and motion sequence between triggering and activating throw event

        //Initializations & Validations:
        if (maneuverIntercept == null) return;                  //If arm has not been given a valid intercept, abort release
        if (currentManeuver != ManeuverState.Grappling) return; //If arm is not currently grappling, abort throw

        //[TEMP]:
        Throw();
    }

    //==|EDITOR FUNCTIONS|==-----------------------------------------------------------------------------------------
    private void DrawManeuverRanges()
    {
        //Description: Draws arcs representing the ranges of grab, clothesline and backhand maneuvers (color coded)

        //Initializations & Validations:
        if (!gizmoSettings.enableRangeGizmos) return; //Cancel if range gizmos are not enabled
        if (mountPoint == null) return; //Cancel if mountPoint is not included, in order to prevent fatal error
        Vector2[] maneuverRanges = { grabRange, clotheslineRange, backhandRange }; //Package ranges into easily-parsed list
        Vector2 lineOrigin = mountPoint.position; //Log position of mount point, from which range linecasts will originate
        Vector2 baseTarget = Vector2.up * gizmoSettings.rangeGizmoLength; //Create vector as base reference for all gizmo line lengths
        bool rightSide = Mathf.Sign(transform.localScale.x) == -1; //Initialize bool indicating which side this arm is on

        //Display Each Range:
        foreach (Vector2 range in maneuverRanges) //Parse through list of maneuver ranges...
        {
            //Initializations & Validations:
            float minAngle = range.x; //Isolate minimum angle setting
            float maxAngle = range.y; //Isolate maximum angle setting
            if (minAngle >= maxAngle) continue; //Skip this iteration if settings are invalid (slider is not correctly adjusted)

            //Get Line Targets:
            Vector2 minTarget = baseTarget.Rotate(minAngle); //Initialize target as rotated variant of base vector
            Vector2 maxTarget = baseTarget.Rotate(maxAngle); //Initialize target as rotated variant of base vector
            if (rightSide) //If arm has been determined to be on the right side (based on transform, rather than internal settings which do not update until runtime)
            {
                minTarget = Vector2.Reflect(minTarget, Vector2.right); //Reflect vector across Y axis
                maxTarget = Vector2.Reflect(maxTarget, Vector2.right); //Reflect vector across Y axis
            }
            minTarget = minTarget.Rotate(transform.rotation.eulerAngles.z) + lineOrigin; //Align vector with theoretical player center & adjust for current world rotation of arm
            maxTarget = maxTarget.Rotate(transform.rotation.eulerAngles.z) + lineOrigin; //Align vector with theoretical player center & adjust for current world rotation of arm

            //Draw Gizmos:
            Gizmos.color = gizmoSettings.rangeGizmoColors[maneuverRanges.IndexOf(range)]; //Set color corresponding to current range
            Gizmos.DrawLine(lineOrigin, minTarget); //Draw line for positive maneuver range
            Gizmos.DrawLine(lineOrigin, maxTarget); //Draw line for negative maneuver range
        }
    }

//==|GRAPHICS & UI|==--------------------------------------------------------------------------------------------
    private void UpdateManeuverUI()
    {
        //Description: Updates effects and visuals related to ongoing maneuvers

        switch (currentManeuver)
        {
            case ManeuverState.Grappling: //GRAPPLING: Player is currently holding an enemy
                //Initializations & Validations:
                Vector2 throwVector = GetThrowVector(); //Get current throw vector

                //[TEMP] Draw Rangefinder:
                Vector2 lineOrigin = maneuverIntercept.enemy.transform.position; //Get coordinate vector for line origin (in world space)
                Debug.DrawRay(lineOrigin, throwVector.normalized * 100, Color.green); //Draw line representing throw vector

                break;
            case ManeuverState.Clotheslining: //CLOTHESLINING: Player is currently clotheslining an enemy
                break;
            case ManeuverState.Backhanding: //BACKHANDING: Player is currently backhanding an enemy
                break;
            default: //DEFAULT: Player is not executing a maneuver
                break;
        }
    }

//==|BONEYARD|==-------------------------------------------------------------------------------------------------
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
