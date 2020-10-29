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
        internal SkinnedMeshRenderer sk = null; //This entity's meshrenderer component
        internal Rigidbody2D rb = null;         //This entity's rigidbody component

    //VARIABLES:
        //Physics Variables:
        internal Vector2 velocity = new Vector2(); //Current linear velocity vector in world space (does NOT indicate facing direction)
        internal float angularVelocity = 0;        //Current angular velocity in world space (negative if counter-clockwise)
        internal float netMass;                    //Current total mass of entity and all attached equipment/entities

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

//==|UNIVERSAL ENTITY FUNCTIONS|==---------------------------------------------------------------------------------
    public virtual void Initialize()
    {
        //Description: Method called at Start to validate core entity components
        
        //Attempt to Get Components:
        sk = GetComponent<SkinnedMeshRenderer>(); //Get this entity's renderer component
        rb = GetComponent<Rigidbody2D>();         //Get this entity's rigidbody component

        //Component Contingencies:
        if (sk == null) //Contingency for SkinnedMeshRenderer:
        {
            Debug.LogError(name + " does not have SkinnedMeshRenderer."); //Log error
        }
        if (rb == null) //Contingency for RigidBody:
        {
            Debug.LogWarning(name + " did not start with Rigidbody."); //Log warning
            rb = gameObject.AddComponent<Rigidbody2D>(); //Create new rigidbody component and set as rb
        }

    }
    public virtual void MoveEntity()
    {
        //Description: Modifies position of entity based on current physics properties

        //Modify Entity Position:
        transform.position += velocity.V3(); //Apply linear velocity to position
        AddAngularVelocity(angularVelocity); //Apply angular velocity to rotation

        //..Xx Sub-Methods xX...............................................................................
        void AddAngularVelocity(float addVel)
        {
            //Description: Applies given velocity value to z-axis of entity rotation (in degrees)

            Vector3 newRotation = transform.rotation.eulerAngles; //Record current rotation
            newRotation.z += addVel; //Add velocity to current rotation (scale from degrees to percentages)
            transform.rotation = Quaternion.Euler(newRotation); //Apply new rotation to transform
        }
    }

}
