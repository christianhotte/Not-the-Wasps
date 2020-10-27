using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Enemy : Entity_NPC
{
    /*  Description: Defines an entity able to attack and be attacked by the player.  Includes functions which
     *               expand NPC AI and interact with player combat abilities
     */

    //CLASSES, ENUMS & STRUCTS:


    //OBJECTS & COMPONENTS:
    private Entity_Player player; //PlayerController script, needs to be known by enemies

    //VARIABLES:
    [Header("Combat Status:")]
    [ShowOnly] public bool grappled = false; //Whether or not this enemy is currently being grappled by player

//==|CORE FUNCTIONS|==---------------------------------------------------------------------------------------------
    public override void Initialize()
    {
        //Description: Validates all objects specific to this controller (in addition to initial validation phase)

        base.Initialize(); //Call base initialization method

        //Attempt to Get Components:
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Entity_Player>(); //Attempt to locate player object and script in scene

        //Contingencies:
        if (player == null) //If player could not be found...
        {
            Debug.LogError(name + " could not identify player."); //Log error
        }

    }

}
