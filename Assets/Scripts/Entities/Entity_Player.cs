using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using HotteStuff;

public class Entity_Player : Entity
{
    /*  Description: Defines an entity controlled by a human player. Contains settings specific to player
     *               character.  Contains core mechanical methods for combat and player movement
     */

    //CLASSES, ENUMS & STRUCTS:
    public struct MoveInput
    {
        //Description: Contains all data used to determine how entities move (used by player and npc controllers)

        public Vector2 moveDirection; //Linear movement input direction (usually normalized)
        public float spinDirection;   //Angular movement input value ((-1)-1)
    }

    //OBJECTS & COMPONENTS:
    private Camera mainCamera = null; //The scene's Main Camera component
    public PlayerPhysicsProperties physicsSettings; //Player-specific physics and movement properties

    //VARIABLES:
    //Input Variables:
    private MoveInput moveInput; //The input information this entity is currently using
    

    [Header("Equipment:")]
    public Equipment_Arm leftArm;  //Player's currently-equipped left arm
    public Equipment_Arm rightArm; //Player's currently-equipped right arm
        
    //[Header("Runtime Data:")]

//==|CORE LOOPS|==-------------------------------------------------------------------------------------------------
    public override void Update()
    {
        base.Update(); //Call base update function
        GetMoveInput();   //Determine movement input variables
        GetCombatInput(); //Determine combat input variables & subsequent events
    }

    public override void FixedUpdate()
    {
        UpdatePhysics(); //Update physics variables
        base.FixedUpdate(); //Call base FixedUpdate function
    }

//==|CORE FUNCTIONS|==---------------------------------------------------------------------------------------------
    public override void Initialize()
    {
        //Description: Validates all objects specific to this controller (in addition to initial validation phase)

        base.Initialize(); //Call base initialization method

        //Attempt to Get Components:
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>(); //Attempt to find main camera in scene

        //Contingencies:
        if (mainCamera == null) //If camera is not in proper place...
        {
            Debug.LogError("Player could not find Main Camera."); //Log error
        }
        if (physicsSettings == null) //If player does not have physics settings assigned...
        {
            Debug.LogError("Player is missing physics settings"); //Log error
        }
    }
    private void GetMoveInput()
    {
        //Description: Updates MoveInput variable in base class (using player inputActions)

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
    private void GetCombatInput()
    {
        //Description: Parses combat input and calls specific functions in reaction to particular events/criteria

        //Initializations & Validations:
        if (leftArm == null || rightArm == null) //If player is missing combat-necessary equipment
        {
            Debug.LogError("Player is missing arm equipment"); //Log error
            return; //Skip remainder of function to prevent further errors
        }
        
    }
    private void UpdatePhysics()
    {
        //Description: Update player physics properties based on input, settings, and environmental factors

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
