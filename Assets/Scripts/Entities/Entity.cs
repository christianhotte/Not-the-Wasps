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

    //OBJECTS & COMPONENTS:
        //Core Components:
        private TimeKeeper timeKeeper; //This scene's timeKeeper script

    //VARIABLES:
        //Physics Variables:
        internal Vector2 velocity = new Vector2(); //Current linear velocity vector in world space (does NOT indicate facing direction)
        internal float angularVelocity = 0;        //Current angular velocity in world space (negative if counter-clockwise)

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

    }
    public virtual void MoveEntity()
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

}
