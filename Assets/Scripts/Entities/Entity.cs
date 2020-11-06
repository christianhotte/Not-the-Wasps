using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HotteStuff;

[System.Serializable]
public class Entity : MonoBehaviour
{
    /*Description: Contains the set of base properties which describe all entities (non-prop character objects such
     *             as the player, enemies, bosses, NPCs, etc). All other Ent_ classes inherit from this class.
     */             

    //CLASSES, ENUMS & STRUCTS:
    [System.Serializable]
    public class MassFactor
    {
        //Description: Item used to keep track of things affecting the mass of an entity

        //Factor Data:
        public float deltaMass = 0;  //The amount by which this factor adds to or subtracts from this entity's total mass
        public float multiplier = 1; //The amount by which this factor multiplies an entity's overall mass

        //Factor Retrieval:
        public Object factorSource; //Pointer indicating the object from which this factor originates (may be used to retrieve this object)
        public string factorTag;    //Simple tag indicating what type of factor this is (may be used to retrieve this object)
    }
    public enum LocomotionType //Different methods an entity can use to move
    {
        Impulse, //IMPULSE: Standard locomotion system, uses acceleration variables to update motion regularly
        Pathed,  //PATHED: Simpler locomotion system, uses given paths and interpolation variables to move entity along given track
        Static   //STATIC: No locomotion system, entity is not moved by MoveEntity
    }

    //OBJECTS & COMPONENTS:
        //Core Components:
        internal TimeKeeper timeKeeper; //This scene's timeKeeper script

    //VARIABLES:
        //Physics Variables:
        [Header("EntityPhysics - Velocity:")]
        public Vector2 velocity = new Vector2();     //Current linear velocity vector in world space (does NOT indicate facing direction)
        [ShowOnly] public float angularVelocity = 0; //Current angular velocity in world space (negative if counter-clockwise)
        [Header("EntityPhysics - Mass:")]
        public float baseMass;               //The base mass of this entity (without equipment or modifiers)
        [ShowOnly] public float currentMass; //The current mass of this entity (modifiers included)
        public List<MassFactor> massFactors; //List of factors contributing to this entity's total mass
        //Entity Settings:
        internal LocomotionType currentMoveType; //This entity's current movement type

//==|CORE LOOPS|==-------------------------------------------------------------------------------------------------
    public virtual void Start()
    {
        Initialize(); //Locate and validate integral components
    }

    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {
        MoveEntity(); //Move entity based on current physics variables
    }

//==|CORE ENTITY FUNCTIONS|==--------------------------------------------------------------------------------------
    public virtual void Initialize()
    {
        //Description: Method called at Start to validate core entity components

        //Attempt to Get Components:
        timeKeeper = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<TimeKeeper>(); //Attempt to get TimeKeeper from GameMaster object

        //Component Contingencies:
        if (timeKeeper == null) //If time keeper script could not be found...
        {
            Debug.LogError(name + " could not find TimeKeeper."); //Log error
        }

        //Establish Starting Variables:
        CalculateMass(); //Get initial mass calculation for this entity

    }
    public void CalculateMass()
    {
        /*  Description: -Calculates this entity's mass based on base mass and other variable factors
         *               -Finds this entity's current center of mass if applicable [NEEDS TO BE ADDED]
         *               -EDITING NOTE: EXPAND WHEN NECESSARY (stuff in Equipment-Equip(), etc)
         */

        //Initializations & Validations:
        if (massFactors.Count == 0) //If there are no outside factors affecting entity mass...
        {
            currentMass = baseMass; //Use base mass for current mass of entity
            return; //End function early
        }
        float addMass = 0;  //Initialize variable to store additional mass from outside factors
        float massMult = 1; //Initialize variable to store mass multipliers from outside factors

        //Check Outside Factors:
        foreach (MassFactor factor in massFactors) //Iterate through all factors affecting the mass of this entity
        {
            addMass += factor.deltaMass;   //Add additional mass from factor to total
            massMult *= factor.multiplier; //Factor given mass multiplier into total
        }

        //Find Total Mass:
        float totalMass = baseMass + addMass; //Get total mass without multipliers
        totalMass *= massMult;                //Factor multipliers into total mass

        //Cleanup:
        currentMass = totalMass; //Apply current mass to total mass count
    }

//==|ENTITY MOVEMENT FUNCTIONS|==----------------------------------------------------------------------------------
    private void MoveEntity()
    {
        //Description: NPC-specific changes to base MoveEntity function (structured around locomotion types)

        switch (currentMoveType)
        {
            case LocomotionType.Impulse: //IMPULSE: Move entity using standard impulse locomotion system
                MoveEntityImpulse(); //Call physics-based entity mover function
                break;
            case LocomotionType.Pathed: //PATHED: Move entity using a given path object
                MoveEntityPathed(); //Call special pathed movement function instead of base function
                break;
            default:
                return; //Skip MoveEntity functions if entity move mode is Static or unaccounted-for
        }
    }
    private void MoveEntityImpulse()
    {
        //Description: Modifies position of entity based on current physics properties

        //Modify Entity Position:
        transform.position += velocity.V3() * timeKeeper.timeScale; //Apply linear velocity to position
        AddAngularVelocity(angularVelocity); //Apply angular velocity to rotation

        //..Xx Sub-Methods xX...............................................................................
        void AddAngularVelocity(float addVel)
        {
            //Description: Applies given velocity value to z-axis of entity rotation (in degrees)

            Vector3 newRotation = transform.rotation.eulerAngles; //Record current rotation
            newRotation.z += addVel * timeKeeper.timeScale; //Add velocity to current rotation (scale from degrees to percentages)
            transform.rotation = Quaternion.Euler(newRotation); //Apply new rotation to transform
        }
    }
    private void MoveEntityPathed()
    {
        //Description: Moves entity along given path based on some physics settings and interpolant variables


    }
}
