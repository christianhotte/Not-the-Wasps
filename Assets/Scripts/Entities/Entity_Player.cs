using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using HotteStuff;

public class Entity_Player : Entity
{
    /*  Description: -Defines an entity controlled by a human player. Contains settings specific to player character
     *               -Contains core mechanical methods for combat and player movement
     *               -Contains settings, variables and methods for governing and reacting to "intercepts" between player and enemies
     */

    //CLASSES, ENUMS & STRUCTS:
    public struct MoveInput
    {
        //Description: Contains all data used to determine how entities move (used by player and npc controllers)

        public Vector2 moveDirection; //Linear movement input direction (usually normalized)
        public float spinDirection;   //Angular movement input value ((-1)-1)
    }
    public struct MouseInput
    {
        //Description: Contains all data collected from mouse every Update tick

        //Positional Data:
        public Vector2 screenPos; //Mouse position in screen (in pixel coordinates)
        public Vector2 worldPos;  //Mouse position in world (in Unity units)

        //Buttons:
        public ButtonState leftButton;   //State of left mouse button
        public ButtonState rightButton;  //State of right mouse button
        public ButtonState middleButton; //State of middle mouse button
    }
    [System.Serializable]
    public class Intercept
    {
        //Description: Describes an encounter between player and an enemy, during which player may react using combat abilities
        /* NOTES: Tenets of interception:
         *            -Interception is local to the player, and being intercepted has no direct effects on enemies or the environment
         *            -Intercept detection is constant and broad. Intercepts should always be recorded to list, then ignored or acted upon as needed
         *            -Interception fields are based on PLAYER AWARENESS vs ENEMY DISPLACEMENT. Ability ranges are not directly based off of player interception field
         *            -Systems involving intercepts should always be built to handle multiple simultaneous interception cases, as this is central to combat
         *            -The time-slowing effect of interception is a core combat mechanic, and is never modified or interrupted by enemies. It is always triggered by things that can hurt the player
        */

        //Actors:
        public Entity_Enemy enemy;  //The enemy involved with this intercept
        public Collider2D collider; //The enemy collider (on InterceptionFields layer) which triggered this intercept
        [Space()]

        //Spatial/Temporal Variables:
        public Vector2 direction;       //Normalized vector representing the direction of motion in this intercept (accounting for enemy velocity, player velocity, and player rotation)
        public float force;             //Amount of force exerted by this intercept (accounts for speed and weight of player and enemy)
        public float proximity;         //How close player center is to closest point on enemy interception zone (in Unity units)
        public float duration = 0;      //The duration (in real-time) of this intercept so far
        public float superDuration = 0; //The duration (in real-time) this intercept has spent in current super-intercept phase
        
        internal float targetTimeScale; //The time slow-down effect this intercept will cause based on its proximity
        [Space()]

        //Potential Maneuvers:
            //NOTE: If value is null, then maneuver cannot be performed for this intercept
        public Equipment_Arm grabArm        = null; //The arm player would use to grab during this intercept
        public Equipment_Arm clotheslineArm = null; //The arm player would use to clothesline during this intercept
        public Equipment_Arm backhandArm    = null; //The arm player would use to backhand during this intercept
        [Space()]

        //State Settings:
        public bool super = false;     //Super intercepts bring combat maneuvers into play, and trigger certain scene effects
        public bool suspended = false; //Suspended intercepts will not be updated or removed, and cannnot be acted upon by combat events (ex. when an enemy is grappled)
        public bool resolved = false;  //Resolved intercepts will be removed upon departure, and cannot be acted upon by combat events (also does not trigger time slow-down)
    }

    //OBJECTS & COMPONENTS:
    private Camera mainCamera;         //Main camera object in scene (used to do screen space calculations)
    private CameraMaster cameraMaster; //Camera controller component, used for screen effects and UI
    [Header("External Settings Objects:")]
    public PlayerPhysicsProperties physicsSettings; //Player-specific physics and movement properties

    //VARIABLES:
    //Input Variables:
    internal MoveInput moveInput = new MoveInput();    //All input information from player directional control
    internal MouseInput mouseInput = new MouseInput(); //All input information from player mouse control

    [Header("Equipment:")]
    public Equipment_Arm leftArm;  //Player's currently-equipped left arm
    public Equipment_Arm rightArm; //Player's currently-equipped right arm

    [Header("Intercepts:")]
    public CircleCollider2D outerInterceptZone; //Outer bounds where enemy proximity will trigger an intercept
    public CircleCollider2D innerInterceptZone; //Inner bounds where interception slow-mo is at its greatest
    public AnimationCurve interceptSlowCurve;   //Curve representing time slow-down during an intercept (depending on where enemy is between intercept zones)
    public List<Intercept> intercepts        = new List<Intercept>(); //List of current intercepts between player and enemies
    internal List<Intercept> superIntercepts = new List<Intercept>(); //List of current super-intercepts between player and proximal enemies
        internal bool canIntercept = true;       //Whether or not player can check for additional intercepts
        internal bool intercepting = false;      //Whether or not player is currently intercepting at least one enemy
        internal bool superIntercepting = false; //Whether or not one of player's intercepts is within player's inner interception zone
        internal bool grappling = false;         //Whether or not player is currently grappling at least one enemy

//==|CORE LOOPS|==-------------------------------------------------------------------------------------------------
    public override void Update()
    {
        base.Update(); //Call base update function

        GetMoveInput();       //Determine movement-related input variables
        GetMouseInput();      //Determine mouse-related input variables
        UpdateCombatEvents(); //Trigger new combat events and maintain existing ones as-needed
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate(); //Call base FixedUpdate function

        UpdatePhysics();          //Update physics variables
        CheckIntercepts();        //Check and update intercepts
    }

//==|CORE FUNCTIONS|==---------------------------------------------------------------------------------------------
    public override void Initialize()
    {
        //Description: Validates all objects specific to this controller (in addition to initial validation phase)

        base.Initialize(); //Call base initialization method

        //Attempt to Get Components:
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>(); //Attempt to find main camera in scene
        if (mainCamera == null) //Early contingency (needed for CameraMaster): If camera is not in proper place...
        {
            Debug.LogError("Player could not find Main Camera."); //Log error
        }
        else cameraMaster = mainCamera.GetComponent<CameraMaster>(); //Attempt to find camera controller component

        //Contingencies:
        if (cameraMaster == null) //If camera controller component could not be found...
        {
            Debug.LogError("Player could not find scene's CameraMaster component."); //Log error
        }
        if (physicsSettings == null) //If player does not have physics settings assigned...
        {
            Debug.LogError("Player is missing physics settings."); //Log error
        }
        if (innerInterceptZone == null || outerInterceptZone == null) //If player is missing interception colliders...
        {
            Debug.LogError("Player is missing interception zone.");
        }

        //Establish Starting Variables:
        currentMoveType = LocomotionType.Impulse; //Set player's initial movement type to impulse
    }
    private void GetMoveInput()
    {
        //Description: Updates MoveInput variable for use by UpdatePhysics()

        //Initializations & Validations:
        MoveInput newMoveInput = new MoveInput(); //Declare variable for storing new input data
        Vector2 directionalInput = new Vector2(); //Initialize vector for finding and storing directional input
        float rotationalInput;                    //Initialize variable for finding and storing rotational input

        //DIRECTION: Get directional input vector
        if (Input.GetKey(KeyCode.W)) directionalInput.y += 1; //Get upward directional input
        if (Input.GetKey(KeyCode.S)) directionalInput.y -= 1; //Get downward directional input
        if (Input.GetKey(KeyCode.A)) directionalInput.x -= 1; //Get leftward directional input
        if (Input.GetKey(KeyCode.D)) directionalInput.x += 1; //Get rightward directional input

        //ROTATION: Get rotational input vector
        Vector2 mousePos = Input.mousePosition; //Get mouse position in screen space
        Vector2 playerPos = mainCamera.WorldToScreenPoint(transform.position); //Get player position in screen space
        Vector2 rotInputVector = HotteMath.AngleBetween(playerPos, mousePos); //Get vector for angle between player and mouse position
        rotationalInput = -HotteMath.LookAt2D(Vector2.zero, rotInputVector); //Get angle from found vector
        rotationalInput += transform.rotation.eulerAngles.z; //Factor in rotation of player
        while (rotationalInput > 180) rotationalInput -= 360; //Wrap input around until it fits within -180-180 scale
        rotationalInput = -rotationalInput.Map(-180, 180, -1, 1); //Remap found value to more easily-readable scale

        //Create and Apply Movement Data:
        newMoveInput.moveDirection = directionalInput; //Set directional input data in new struct
        newMoveInput.spinDirection = rotationalInput;  //Set rotational input data in new struct
        moveInput = newMoveInput;                      //Apply new moveInput data
        
        //..Xx Debuggers xX.................................................................................
            //Debug.Log("Player FaceDirection = " + rotationalInput); //Check that angle of mouseInput is correct
            //Debug.Log("Mouse angle to player = " + rotationalInput.ToString()); //Check that angle of mouse input is correct
            //Debug.Log("Player rotation = " + transform.rotation.eulerAngles.z); //Check actual rotation of player
    }
    private void GetMouseInput()
    {
        //Description: Updates MouseInput variable with latest information on mouse position and button input

        //Initializations & Validations:
        MouseInput newMouseInput = new MouseInput(); //Declare variable for storing new input data

        //Process Button States:
        bool leftButtonInput = Input.GetMouseButton(0);   //Get raw input for left mouse button
        bool rightButtonInput = Input.GetMouseButton(1);  //Get raw input for right mouse button
        bool middleButtonInput = Input.GetMouseButton(2); //Get raw input for middle mouse button
        newMouseInput.leftButton = GetButtonState(mouseInput.leftButton, leftButtonInput); //Process new left mouse button state
        newMouseInput.rightButton = GetButtonState(mouseInput.rightButton, rightButtonInput); //Process new right mouse button state
        newMouseInput.middleButton = GetButtonState(mouseInput.middleButton, middleButtonInput); //Process new middle mouse button state

        //Get Mouse Position:
        newMouseInput.screenPos = Input.mousePosition; //Get mouse position (in pixel coordinates) in screen space
        newMouseInput.worldPos = mainCamera.ScreenToWorldPoint(newMouseInput.screenPos); //Get mouse position (in Unity units) in world coordinate space

        //Cleanup:
        mouseInput = newMouseInput; //Apply new input data to player input storage variable

        //..Xx Sub-Methods xX...............................................................................
        ButtonState GetButtonState(ButtonState prevInput, bool currentInput)
        {
            //Description: Takes given previous and current button input variables and returns clean, processed ButtonState

            if (currentInput) //If given button is currently pressed...
            {
                if (prevInput == ButtonState.Down || //If button was previously pressed...
                    prevInput == ButtonState.Held)   //If button was previously held...
                { return ButtonState.Held; }  //Indicate that button is being held
                else return ButtonState.Down; //Otherwise, indicate that button has just been pressed
            }
            else //If given button is not currently pressed...
            {
                if (prevInput == ButtonState.Up ||    //If button was previously released...
                    prevInput == ButtonState.Neutral) //If button was previously unpressed...
                { return ButtonState.Neutral; } //Indicate that button remains unpressed
                else return ButtonState.Up;     //Otherwise, indicate that button has just been released
            }
        }
        
        //..Xx Debuggers xX.................................................................................
            //Debug.Log("LeftButtonState = " + newMouseInput.leftButton); //Check that button input processor is working properly

    }
    private void UpdatePhysics()
    {
        //Description: Updates player physics properties based on input, settings, and environmental factors

        //Initializations & Validations:
        if (superIntercepting) return; //During a super intercept, player loses direct control over character motion (drag also does not apply)
        if (grappling) return;         //While grappling, player loses direct control over character motion

        //LINEAR VELOCITY:
            //Initialization & Validation:
            Vector2 newVelocity = velocity; //Initialize vector for calculating final updated velocity
            Vector2 inputAccelDirection;    //Initialize vector for factoring direction into acceleration vector
            float strafeModifier;           //Initialize variable for factoring strafe direction into velocity calculations

            //Calculate Strafing Modifier:
            Vector2 moveDirection = moveInput.moveDirection.normalized; //Get direction of input
            Vector2 faceDirection = Vector2.up.Rotate(transform.rotation.eulerAngles.z); //Get player entity is pointing
            float strafeAngle = -Vector2.SignedAngle(faceDirection, moveDirection); //Get angle disparity between facing direction and direction of acceleration
            strafeAngle = strafeAngle.Map(-180, 180, -1, 1); //Map angle into more AnimationCurve-friendly values
            strafeModifier = physicsSettings.strafeVelFalloff.Evaluate(Mathf.Abs(strafeAngle)); //Get modifier from physicsSettings

            //Calculate Input:
            if (moveInput.moveDirection != Vector2.zero) //If directional input from player is detected...
            {
                float actualAccel = ScaleAccel(physicsSettings.baseAccel); //Get relevant accel value from player settings (scaled appropriately to runtime properties)
                inputAccelDirection = moveInput.moveDirection; //Set vector direction equal to direction of input
                inputAccelDirection *= actualAccel; //Apply found acceleration to direction vector to get working acceleration vector
            }
            else //If no directional input from player is detected...
            {
                float actualAccel = ScaleAccel(physicsSettings.idleDecel); //Get relevant accel value from player settings (scaled appropriately to runtime properties)
                inputAccelDirection = velocity.normalized * -1; //Set input vector to reverse of current movement direction
                inputAccelDirection *= actualAccel; //Apply found acceleration to direction vector to get working acceleration vector
            }

            //Apply Factors to New Velocity:
            inputAccelDirection *= strafeModifier; //Apply strafing velocity curve to change in acceleration
            newVelocity += inputAccelDirection; //Apply acceleration due to input
            float maxVelocity = physicsSettings.maxVelocity; //Get maximum velocity
            if (newVelocity.magnitude > maxVelocity) //If new velocity exceeds maximum velocity setting...
            {
                newVelocity = newVelocity.normalized * maxVelocity; //Cap velocity at maximum
            }
            if (moveInput.moveDirection == Vector2.zero && newVelocity.magnitude < physicsSettings.decelSnapVel) //If player is coasting and has reached snap velocity...
            {
                newVelocity = Vector2.zero; //Set velocity to zero
            }

        //ANGULAR VELOCITY:
            //Initialization & Validation:
            float newAngVelocity; //Initializa variable for calculating final velocity   

            //Find Target Change:
            float targetChange = moveInput.spinDirection.Map(-1, 1, -180, 180); //Initialize variable for finding target change in degrees for this rotation (starting with current mouse angle from player)
            targetChange = Mathf.Lerp(0, targetChange, physicsSettings.mouseTurnStiffness); //Find actual target change (in degrees) based on player physics settings
            if (Mathf.Abs(targetChange) > physicsSettings.maxControlAngVel * Time.fixedDeltaTime) //If target change exceeds maximum controlled angular velocity...
            {
                targetChange = physicsSettings.maxControlAngVel * Time.fixedDeltaTime * Mathf.Sign(targetChange); //Set change to max
            }
            
            //Apply to Velocity:
            newAngVelocity = targetChange; //Set found change in degrees as new angular velocity  

        //Cleanup:
        velocity = newVelocity;           //Update velocity vector
        angularVelocity = newAngVelocity; //Update angular velocity vector
        
        //..Xx Sub-Methods xX...............................................................................
        float ScaleAccel(float givenAccel)
        {
            //Description: Scales actualAccel to universal game factors, ensuring stability and reliability

            //Initialization & Validation:
            float actualAccel = givenAccel; //Initialize value to scale and return

            //Apply Scaling Factors:
            actualAccel *= Time.fixedDeltaTime; //Scale acceleration to game time (UpdatePhysics should be called in FixedUpdate)
            actualAccel *= HotteMath.Mean(new float[] { transform.localScale.x, transform.localScale.y }); //Scale acceleration to actual size of player object (preventing settings from being fixed to character scale)

            //Cleanup
            return actualAccel; //Return scaled acceleration value
        }

        //..Xx Debuggers xX.................................................................................
            //Debug.Log("RotInputAccelDirection = " + rotInputAccelDirection); //Check angular acceleration factor
            //Debug.Log("NewAngularVelocity = " + newAngVelocity); //Check newly-calculated angular velocity
            //Debug.Log("Angular Acceleration = " + actualAngAccel); //Check how much player is trying to accelerate based on input

    }
    private void UpdateCombatEvents()
    {
        /*  Description: -Triggers combat events based on current intercepts and mouse input
         *               -Updates the state of ongoing combat events relevant to player object
         */

        //[EDIT NOTE]: EVERYTHING HERE IS TEMPORARY

        //Check For Combat Event Initiation:
        foreach (Intercept intercept in superIntercepts) //Parse through list of super-intercepts
        {
            //[TEMP] Draw Visualizer Gizmo:
            Circle enemySelectField = new Circle(intercept.enemy.transform.position, 0.2f);
            if (intercept.grabArm != null)
            {
                if (mouseInput.worldPos.IsWithinCircle(enemySelectField))
                {
                    //Draw Temp Selection Field Gizmo:
                    enemySelectField.radius = 0.15f;
                    enemySelectField.DrawCircle(12, Color.white);
                    //Check For Player Input:
                    if (mouseInput.leftButton == ButtonState.Down)
                    {
                        intercept.grabArm.Grapple(intercept);
                    }
                }
                else
                {
                    enemySelectField.DrawCircle(12, Color.green);
                }
            }
            else if (intercept.clotheslineArm != null || intercept.backhandArm != null)
            {
                enemySelectField.DrawCircle(12, Color.gray);
            }
        }

        //Ongoing Maneuvers:
        if (grappling) //If player is currently grappling an enemy...
        {
            timeKeeper.timeScale = Mathf.Lerp(timeKeeper.timeScale, 0.4f, 0.1f); //Lerp timescale into grappling mode

            if (mouseInput.leftButton == ButtonState.Up) //If left mouse button release is detected...
            {
                leftArm.InitiateThrowSequence();  //Initiate left arm throw sequence if applicable
                rightArm.InitiateThrowSequence(); //Initiate right arm throw sequence if applicable
            }
        }
    }

//==|INTERCEPT METHODS|==------------------------------------------------------------------------------------------
    private void CheckIntercepts()
    {
        /*  Description: -Looks for new intercepts between player and incoming enemies
         *               -Checks for and removes old intercepts between player and outgoing enemies
         *               -Updates current intercepts between player and intercepted enemies
         */

        //Validations & Initializations:
        List<Collider2D> overlaps = new List<Collider2D>(); //Initialize list to store overlapping colliders
        ContactFilter2D filter = new ContactFilter2D();     //Initialize contact filter for intercept collider

        //Prime Collision Filter & Populate Overlap List:
        filter.useDepth = false;   //Set filter to ignore depth (use 2D space)
        filter.useTriggers = true; //Set filter to use triggers (all colliders involved should be triggers)
        filter.SetLayerMask(LayerMask.GetMask("InterceptionFields")); //Set filter to mask out all colliders other than interception fields
        outerInterceptZone.OverlapCollider(filter, overlaps); //Populate list of detected intercept fields from enemy entities

        //UPDATE or REMOVE Existing Intercepts:
        for (int i = 0; i < intercepts.Count; i++) //Iterate through list of all current intercepts...
        {
            //Initialization & Validation:
            Intercept intercept = intercepts[i]; //Initialize shorthand for current intercept in list
            bool foundCollider = false;          //Initialize variable to indicate whether or not sweep has successfully found this intercept's collider
            
            //Check List for Matching Collider:
            foreach (Collider2D collider in overlaps) //Iterate through list of currently-overlapping colliders
            {
                if (collider == intercept.collider) //If this collider matches that of this intercept...
                {
                    foundCollider = true; //Indicate that this intercept's collider has been found
                    break; //Stop looking for matching colliders
                }
            }

            //Cleanup:
            if (foundCollider) //If this intercept's collider was found in list of current collisions...
            {
                overlaps.Remove(intercept.collider); //Remove collider from list, simplifying future sweeps
                UpdateIntercept(intercept); //Update this intercept, since continutity has been confirmed
            }
            else //If this intercept's collider was not found in list...
            {
                RemoveIntercept(intercept); //Remove intercept if not suspended
            }
                
        }

        //CREATE New Intercepts:
        foreach (Collider2D collider in overlaps) //For each unclaimed collider in overlaps...
        {
            Entity_Enemy interceptedEnemy = collider.GetComponentInParent<Entity_Enemy>(); //Get enemy controller from overlap collider
            CreateIntercept(interceptedEnemy); //Create intercept with found enemy
        }

        //APPLY Intercept Effects:
        if (grappling) return; //Skip time control function if player is grappling
        float targetTimeScale = timeKeeper.baseTimeScale; //Initialize timescale target at the designated base for this scene
        foreach (Intercept intercept in intercepts) //Parse through final list of intercepts for this tick...
        {
            //Initialization & Validation:
            if (intercept.suspended || intercept.resolved) continue; //Skip intercept if it is resolved or suspended
            
            //Compare Intercept Data:
            targetTimeScale = Mathf.Min(targetTimeScale, intercept.targetTimeScale); //Find lowest targetTimeScale out of all updated intercepts
        }
        timeKeeper.timeScale = targetTimeScale; //Set timescale based on closest intercept
    }
    private Intercept CreateIntercept(Entity_Enemy enemy)
    {
        //Description: Creates a new intercept and adds it to list of existing intercepts, establishing necessary dependencies

        //Initializations & Validations:
        if (enemy == null) //If no enemy controller is given...
        {
            Debug.LogError("Attempted intercept without valid enemy."); //Log error
            return null; //Cancel method to prevent fatal errors
        }

        //Find Intercept Actors:
        Intercept newIntercept = new Intercept(); //Initialize new intercept
        newIntercept.enemy = enemy;               //Set intercept enemy
        newIntercept.collider = enemy.dangerZone; //Set intercept collider

        //Establish Controller Dependencies:
        enemy.combatStatus = Entity_Enemy.CombatStatus.Intercepted; //Indicate to enemy that it has been intercepted
        intercepting = true; //Indicate to player that it is now intercepting at least one enemy

        //Cleanup:
        UpdateIntercept(newIntercept); //Fill in all other, non-static variables relating to this intercept
        intercepts.Add(newIntercept);  //Add new intercept to list of current intercepts
        return newIntercept;           //Return new intercept
    }
    private void UpdateIntercept(Intercept intercept)
    {
        //Description: Updates the real-time data in a known intercept

        //Initialization & Validation:
        if (intercept == null) //If no intercept is given...
        {
            Debug.LogError("Player attempted to update null intercept."); //Log error
            return; //Exit method to prevent fatal error
        }
        Entity_Enemy enemy = intercept.enemy;   //Get shorthand for enemy controller
        float timeScale = timeKeeper.timeScale; //Get current time scale

        //Update Intercept Vector:
        Vector2 enemyMomentum = enemy.velocity * enemy.currentMass; //Get total momentum of enemy
        Vector2 playerMomentum = velocity * currentMass;            //Get total momentum of player
        Vector2 interceptVector = enemyMomentum + playerMomentum;   //Get total momentum of intercept

        //Update Intercept Proximity:
        Vector2 interceptPoint = enemy.dangerZone.ClosestPoint(transform.position); //Get closest point to player center on enemy intercept field
        float interceptProx = Vector2.Distance(interceptPoint, transform.position); //Get total distance between enemy intercept zone and player

        //Cleanup Basic Data:
        if (!intercept.suspended) //If intercept is not suspended...
        {
            //NOTE: Suspension prevents these from updating because their value at moment of grapple is used to determine throw force
            intercept.direction = interceptVector.normalized; //Set intercept direction
            intercept.force = interceptVector.magnitude;      //Set intercept force
        }
        intercept.proximity = interceptProx;  //Set intercept proximity
        intercept.duration += Time.fixedDeltaTime; //Update intercept duration tracker

        //Check Relative Proximity:
        float prox = intercept.proximity; //Get current proximity of intercept
        float maxDist = outerInterceptZone.radius; //Get the maximum proximity intercept could have
        float minDist = innerInterceptZone.radius; //Get the minimum proximity intercept could have
        float scaledProx = HotteMath.Map(prox, minDist, maxDist, 1, 0); //Get proximity scaled to player interception zones (time-relevant)

        //Check Combat Maneuvers:
        UpdateSuperStatus(); //Update super status of intercept
        if (intercept.super) //[Combat maneuvers come into play during a super intercept] If intercept is a super-intercept...
        {
            //Initializations & Validations:
            Equipment_Arm[] maneuverArms = { null, null, null }; //Initialize array for returning maneuver compatibility to intercept
            
            //Get Target Angle:
            Vector2 enemyPos = enemy.transform.position - transform.position; //Get enemy position relative to player position
            float playerAngle = transform.rotation.eulerAngles.z; //Get player angle in world space (in degrees)
            float targetAngle = Vector2.SignedAngle(Vector2.up.Rotate(playerAngle), enemyPos); //Get angle between player and enemy (relative to player facing direction)

            //Determine Viable Maneuvers + Corresponding Arms:
            for (int x = 0; x < maneuverArms.Length; x++) //Iterate once through each type of maneuver...
            {
                //Check Each Arm's Compatibility:
                Booloat leftArmData = CheckArmRange(leftArm, targetAngle, x);   //Get data for this maneuver from left arm
                Booloat rightArmData = CheckArmRange(rightArm, targetAngle, x); //Get data for this maneuver from right arm

                //[TEMP]: Pick Best Arm:
                if (leftArmData._bool) maneuverArms[x] = leftArm; //If left arm is compatible, use it for maneuver
                else if (rightArmData._bool) maneuverArms[x] = rightArm; //If right arm is compatible, use it for maneuver
            }

            //Apply Maneuver Viability Data:
            intercept.grabArm = maneuverArms[0];        //Apply data to grab arm field
            intercept.clotheslineArm = maneuverArms[1]; //Apply data to clothesline arm field
            intercept.backhandArm = maneuverArms[2];    //Apply data to backhand arm field
        }
        else //If intercept is NOT a super-intercept...
        {
            //Deactivate All Maneuvers:
            intercept.grabArm = null;        //Indicate that intercept cannot grab
            intercept.clotheslineArm = null; //Indicate that intercept cannot clothesline
            intercept.backhandArm = null;    //Indicate that intercept cannot backhand
        }

        //Secondary Cleanup:
        intercept.targetTimeScale = interceptSlowCurve.Evaluate(scaledProx); //Get this intercept's potential target timescale based on slow-mo curve setting

        //..Xx Sub-Methods xX...............................................................................
        void UpdateSuperStatus()
        {
            //Description: Checks how close intercept enemy is to player, and triggers super-intercept effects if enemy is close enough

            if (scaledProx >= 1 && //If intercepted enemy has passed player's inner interception zone...
                !intercept.suspended && !intercept.resolved) //And intercept is neither suspended nor resolved...
            {
                //Initial Confirmation:
                superIntercepting = true; //Indicate that at least one super-intercept is taking place
                intercept.super = true;   //Indicate that this individual intercept is a super-intercept
                intercept.superDuration += Time.fixedDeltaTime; //Update special duration value
                //Check Secondary List:
                if (!superIntercepts.Contains(intercept)) superIntercepts.Add(intercept); //Add intercept to list of super-intercepts if not already added
            }
            else if (intercept.superDuration != 0) //If intercept has just ceased super-intercept status...
            {
                //Check List Status:
                if (superIntercepts.Contains(intercept)) superIntercepts.Remove(intercept); //Remove intercept from list of super-intercepts if it has not already been removed
                if (superIntercepts.Count == 0) superIntercepting = false; //Determine whether or not any superInterceptions are still taking place
                //Cleanup:
                intercept.superDuration = 0; //Clear contiguous duration value
                intercept.super = false; //Indicate that this individual intercept is not a super-intercept
            }
        }
        Booloat CheckArmRange(Equipment_Arm arm, float target, int rangeIndex)
        {
            //Description: Determines whether or not enemy is within range of maneuver (at given index) with given arm
            //Returns: Returns a TRUE bool if arm can perform maneuver, and returns rotation proximity (in degrees) between enemy and maneuver core area

            //Initialization & Validation:
            Booloat returnValues = new Booloat(false, 0f); //Initialize data structure to return
            float rangeMin = arm.maneuverRanges[rangeIndex].x; //Get minimum angular range from arm controller
            float rangeMax = arm.maneuverRanges[rangeIndex].y; //Get maximum angular range from arm controller
            //[EDITING NOTE]: More to be added once velocity-based range extension is integrated

            //Reflect Bounds for Right Arm:
            if (arm.armSide == Direction.Right) //If arm is on the right side...
            {
                rangeMin = -arm.maneuverRanges[rangeIndex].y; //Switch and invert value of minimum range variable
                rangeMax = -arm.maneuverRanges[rangeIndex].x; //Switch and invert value of maximum range variable
            }

            //Check Ranges:
            if (target.AngleIsBetween(rangeMin, rangeMax)) //If target is within given range...
            {
                returnValues._bool = true; //Indicate that maneuver is possible
                //[EDITING NOTE]: More to be added once velocity-based range extension is integrated
            }

            //Cleanup:
            return returnValues; //Return found data
        }

    }
    private void RemoveIntercept(Intercept intercept)
    {
        //Description: Removes an intercept from list, and disconnects all necessary dependencies

        //Initialization & Validation:
        if (intercept == null) //If no intercept is given...
        {
            Debug.LogError("Player attempted to remove null intercept."); //Log error
            return; //Exit method to prevent fatal error
        }
        if (!intercepts.Contains(intercept)) //If current list of intercepts does not contain intercept scheduled for removal...
        {
            Debug.LogError("Player tried to remove intercept that does not exist."); //Log error
            return; //Cancel operation to prevent fatal error
        }
        if (intercept.suspended) return; //If intercept is suspended, do not remove it

        //Remove Local Dependencies:
        if (superIntercepts.Contains(intercept)) superIntercepts.Remove(intercept); //Remove intercept from list of super-intercepts if applicable
        if (superIntercepts.Count == 0) superIntercepting = false; //Check whether or not player overall is still superIntercepting

        //Remove Controller Dependencies:
        if (intercept.enemy.combatStatus == Entity_Enemy.CombatStatus.Intercepted) //If enemy combat status is INTERCEPTED...
        { intercept.enemy.combatStatus = Entity_Enemy.CombatStatus.Default; } //Change enemy combat status back to default

        //Cleanup:
        intercepts.Remove(intercept); //Remove intercept from list of intercepts
        if (intercepts.Count == 0) intercepting = false; //Check whether or not player is still intercepting at all after removal of this intercept
    }

}

//==|BONEYARD|==---------------------------------------------------------------------------------------------------
    /* OLD ROTATIONAL INPUT & PHYSICS PROCESSORS:
     * 
     *  Vector2 mousePos = Input.mousePosition; //Get mouse position in screen space
        Vector2 playerPos = mainCamera.WorldToScreenPoint(transform.position); //Get player position in screen space
        Vector2 rotInputVector = ExtensionMethods.AngleBetween(playerPos, mousePos); //Get vector for angle between player and mouse position
        rotationalInput = -ExtensionMethods.LookAt2D(Vector2.zero, rotInputVector); //Get angle from found vector
        rotationalInput += transform.rotation.eulerAngles.z; //Factor in rotation of player
        while (rotationalInput > 180) rotationalInput -= 360; //Wrap input around until it fits within -180-180 scale
        rotationalInput = -rotationalInput.Map(-180, 180, -1, 1); //Remap found value to more easily-readable scale
        rotationalInput = Mathf.Sign(rotationalInput) * physicsSettings.mouseAccelFalloff.Evaluate(Mathf.Abs(rotationalInput)); //Use settings to adjust strength of acceleration based on how far from player center the mouse is
     *
     * -----------------------------------------------------
     * 
     *  //Initialization & Validation:
            float newAngVelocity = angularVelocity; //Initializa variable for calculating final updated velocity    
            float inputSpinAccelDirection = 0;      //Initialize variable for factoring direction and intensity of input into acceleration vector

            //Calculate Input:
            if (moveInput.spinDirection != 0) //If directional input from player is detected...
            {
                float actualAngAccel = physicsSettings.baseTurnAccel * Time.fixedDeltaTime; //Get relevant accel value from player settings (and scale to FixedUpdate tick)
                inputSpinAccelDirection = moveInput.spinDirection; //Set spin direction to that of known input
                inputSpinAccelDirection *= actualAngAccel; //Apply found acceleration factor to spin direction
            }
            else //If no directional input from player is detected (meaning player may want to stop spinning)...
            {
                float actualAngAccel = physicsSettings.turnIdleDecel * Time.fixedDeltaTime; //Get relevant decel value from player settings (and scale to FixedUpdate tick)
                inputSpinAccelDirection = Mathf.Sign(angularVelocity) * -1; //Set input direction to reverse of current rotation direction
                inputSpinAccelDirection *= actualAngAccel; //Apply found acceleration factor to spin direction
            }
            
            //Apply Factors to New Velocity:
            newAngVelocity += inputSpinAccelDirection; //Apply acceleration due to input
     */
    /* MOMENTUM TIER STUFF:
        public enum RotationMode //Describes different control modes governing player rotation
        {
            Controlled, //Player is being directly rotated by mouse, Momentum is zero
            Free,       //Player rotation is unlocked and is being controlled by Momentum
        };
     *  else if (rotationMode == RotationMode.Free) //FREE: If player rotation is currently unlocked...
            {
                float targetVel = GetMomentumTierVel(momentumTier) * Time.fixedDeltaTime; //Get shorthand for target velocity
                if (!HotteMath.Approx(angularVelocity, targetVel, physicsSettings.momentumSnapThresh)) //If velocity is not currently where it should be for player's momentum tier...
                {
                    newAngVelocity = Mathf.Lerp(angularVelocity, targetVel, physicsSettings.momentumSnapStiffness); //Lerp velocity toward target for given tier
                }
                else //If angular velocity is more-or-less aligned with momentum tier...
                {
                    newAngVelocity = targetVel; //Set velocity to exact value for tier
                }
            }
     *  float GetMomentumTierVel(int tier)
        {
            //Description: Finds velocity of given momentum tier in player physics settings

            //Initialization & Validation:
            if (tier == 0) return 0; //Teir zero will always have a velocity of 0, allowing the rest of the method to be skipped
            float[] momentumTeirVels = physicsSettings.momentumTierVels; //Get shorthand for tier velocities
            if (momentumTeirVels.Length == 0) //Check to make sure physicsSettings contain minimum data requirements...
            {
                Debug.LogError("Player does not have assigned momentum teir velocities."); //Log error
                return 0; //Return zero to prevent crash
            }
            float foundVel = momentumTeirVels[momentumTeirVels.Length - 1]; //Initialize return variable to last item in given array (max momentum)

            //Find Corresponding Velocity:
            if (Mathf.Abs(tier) < momentumTeirVels.Length) //If tier has a corresponding velocity in given settings list...
            {
                foundVel = momentumTeirVels[Mathf.Abs(tier) - 1]; //Get velocity from corresponding item in array
            }

            //Cleanup:
            foundVel *= Mathf.Sign(tier); //Carry over sign from tier (for negative rotation)
            return foundVel; //Return found velocity corresponding with given momentum tier
        }

     */
    /* COMBAT INPUT STUFF:
     * public struct CombatInput
    {
        //Description: Structure for storing input data used to determine player combat interactions

        //Constructor:
        public CombatInput(bool grabInput, bool strikeInput, bool stingInput)
        {
            grab = grabInput;     //Assign grab input value
            strike = strikeInput; //Assign strike input value
            sting = stingInput;   //Assign sting input value
        }

        //Variables:
        public bool grab;   //True if player is holding input button for grabbing
        public bool strike; //True if player is holding input button for striking
        public bool sting;  //True if player is holding input button for stinging
    }
     * private CombatInput combatInput; //Player's current combat inputs
     * CombatInput prevCombatInput = combatInput; //Save current combat inputs as previous input values
        bool newGrabInput = false;   //Initialize value to store new grab input
        bool newStrikeInput = false; //Initialize value to store new strike input
        bool newStingInput = false;  //Initialize value to store new sting input

        //Get New Inputs:
        if (Input.GetKey(KeyCode.Mouse0)) newGrabInput = true;   //Get current grab input
        if (Input.GetKey(KeyCode.Mouse1)) newStrikeInput = true; //Get current strike input
        if (Input.GetKey(KeyCode.Mouse2)) newStingInput = true;  //Get current sting input
        combatInput = new CombatInput(newGrabInput, newStrikeInput, newStingInput); //Store new input variables

        //Register Combat Input Events:
        SendMessageOptions drr = SendMessageOptions.DontRequireReceiver; //Initialize messageoptions shorthand
        if (newGrabInput) BroadcastMessage("OnGrabButton", drr);     //Signal that grab button is currently being held
        if (newStrikeInput) BroadcastMessage("OnStrikeButton", drr); //Signal that strike button is currently being held
        if (newStingInput) BroadcastMessage("OnStingButton", drr);   //Signal that sting button is currently being held
        if (newGrabInput != prevCombatInput.grab) //Change in grab input detected...
        {
            if (newGrabInput) BroadcastMessage("OnGrabButtonDown", drr); //Signal that grab button has been pressed
            else              BroadcastMessage("OnGrabButtonUp", drr);   //Signal that grab button has been released
        }
        if (newStrikeInput != prevCombatInput.strike) //Change in strike input detected...
        {
            if (newStrikeInput) BroadcastMessage("OnStrikeButtonDown", drr); //Signal that strike button has been pressed
            else                BroadcastMessage("OnStrikeButtonUp", drr);   //Signal that strike button has been released
        }
        if (newStingInput != prevCombatInput.sting) //Change in sting input detected...
        {
            if (newStingInput) BroadcastMessage("OnStingButtonDown", drr); //Signal that sting button has been pressed
            else               BroadcastMessage("OnStingButtonUp", drr);   //Signal that sting button has been released
        }
     */
    /* INTERCEPT TIMESCALE EFFECTS:
     *  
     *  //Initialization & Validation:
        float targetTimeScale = timeKeeper.baseTimeScale; //Initialize timescale target at the designated base for this scene
     *  
     *  //Cleanup:
        timeKeeper.timeScale = targetTimeScale; //Set timescale to target
     *  
     */
