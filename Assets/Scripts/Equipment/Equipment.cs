using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Equipment : MonoBehaviour
{
    /*Description: Contains the set of base properties which describes all player equipment, and methods to equip,
     *             control and organize it.
     */

    //CLASSES, ENUMS & STRUCTS:

    //OBJECTS & COMPONENTS:
    internal Entity_Player player;  //Player controller script (if this equipment is currently equipped)
    internal TimeKeeper timeKeeper; //Unifying time controller

    //VARIABLES:
    internal bool equipped = false; //Whether or not this piece of equipment is currently equipped

//==|CORE LOOPS|==-------------------------------------------------------------------------------------------------
    public virtual void Start()
    {
        Initialize(); //Initialize equipment
    }

    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {

    }

//==|CORE EQUIPMENT FUNCTIONS|==-----------------------------------------------------------------------------------
    public virtual void Initialize()
    {
        //INITIALIZE: Fully Initializes this equipment in scene based on current parent

        //Find Equipment Parent:
        player = gameObject.GetComponentInParent<Entity_Player>(); //Try to find player script in parent
        timeKeeper = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<TimeKeeper>(); //Attempt to get TimeKeeper

        //Initialization Variants:
        if (player != null) //PLAYER INIT: If equipment is attached to player...
        {
            Equip(player); //Equip this arm to player
        }
        else //UNSUCCESSFUL INIT: If equipment init state has not been accounted for...
        {
            Debug.LogError(name + " could not be initialized. Is parent missing?"); //Log error
        }

        //Component Contingencies:
        if (timeKeeper == null) //If time keeper script could not be found...
        {
            Debug.LogError(name + " could not find TimeKeeper."); //Log error
        }

    }
    public virtual void Equip(Entity_Player targetPlayer)
    {
        //EQUIP: Attaches this piece of equipment to player and fulfills all necessary contingencies

        //Initialization & Validation:
        if (targetPlayer == null) //If given playerController is not valid or present...
        {
            Debug.LogError(name + " failed Equip, no player given."); //Log error
            return; //End equip function
        }

        //Establish Initial Connections:
        player = targetPlayer; //Record given player script

        //Additional Cleanup:
        equipped = true; //Indicate that this arm is now equipped
    }
    public virtual void UnEquip()
    {
        //UNEQUIP: Detaches this piece of equipment from player and fulfills all necessary contingencies


    }
}
