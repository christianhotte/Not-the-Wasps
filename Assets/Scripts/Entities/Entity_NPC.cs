using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_NPC : Entity
{
    /*  Description: Defines an entity controlled by the AI. Contains settings which allow this entity to move and
     *               act using AI commands.
     */

    //CLASSES, ENUMS & STRUCTS:
    public enum NPCLocomotionType //Different methods an NPC can use to move
    {
        Impulse, //IMPULSE: Standard locomotion system (used by player), uses acceleration variables to update motion regularly
        Pathed,  //PATHED: Simpler locomotion system, uses given paths and interpolation variables to move NPC along given track
        Static   //STATIC: No locomotion system, NPC does not move
    }

    //OBJECTS & COMPONENTS:

    //VARIABLES:
    public NPCLocomotionType locomotionType; //This NPC's locomotion type (can be used as an intitial setting, but may be changed by inheritor classes)

//==|NPC FUNCTIONS|==----------------------------------------------------------------------------------------------
    public override void MoveEntity()
    {
        //Description: NPC-specific changes to base MoveEntity function (structured around locomotion types)

        switch (locomotionType)
        {
            case NPCLocomotionType.Impulse: //IMPULSE: Move entity using standard impulse locomotion system
                base.MoveEntity(); //Call standard MoveEntity function provided in Ent_Base
                break;
            case NPCLocomotionType.Pathed: //PATHED: Move entity using NPC-specific path-based locomotion method
                MoveEntityPathed(); //Call special pathed movement function instead of base function
                break;
            default:
                return; //Skip MoveEntity functions if entity move mode is Static or unaccounted-for
        }
    }
    private void MoveEntityPathed()
    {
        //Description: Moves NPC along given path based on interpolant variables


    }

}
