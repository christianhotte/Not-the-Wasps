using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Equipment_Base : MonoBehaviour
{
    /*Description: Contains the set of base properties which describes all player equipment, and methods to equip,
     *             control and organize it.
     */

    //CLASSES, ENUMS & STRUCTS:

    //OBJECTS & COMPONENTS:
    private Entity_Player player; //Player controller script (if this equipment is currently equipped)

    //VARIABLES:
    private bool equipped = false; //Whether or not this piece of equipment is currently equipped

//==|UNIVERSAL EQUIPMENT FUNCTIONS|==------------------------------------------------------------------------------
    public void Initialize()
    {
        //INITIALIZE: Fully Initializes this equipment in scene based on current parent

        //Initializations & Validations:
        equipped = false; //Change equipped state to false while initializing

        //Find Equipment Parent:
        player = gameObject.GetComponentInParent<Entity_Player>(); //Try to find player script in parent
        
        //Initialization Variants:
        if (player != null) //PLAYER INIT: If equipment is attached to player...
        {
            equipped = true; //Verify that equipment is now equipped
        }
        else //UNSUCCESSFUL INIT: If equipment init state has not been accounted for...
        {
            Debug.LogError(name + " could not be initialized. Is parent missing?"); //Log error
        }

    }
    public void Equip(Entity_Player player)
    {
        //EQUIP: Attaches this piece of equipment to player and fulfills all necessary contingencies


    }
    public void UnEquip()
    {
        //UNEQUIP: Detaches this piece of equipment from player and fulfills all necessary contingencies


    }
}
