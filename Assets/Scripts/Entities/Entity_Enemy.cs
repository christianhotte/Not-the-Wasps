using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_Enemy : Entity
{
    /*  Description: Defines an entity able to attack and be attacked by the player.  Includes functions which
     *               expand NPC AI and interact with player combat abilities
     */

    //CLASSES, ENUMS & STRUCTS:
    public enum CombatStatus //Options for enemy status regarding effects from player combat actions
    {
        Default,   //Standard combat status, enemy is not impaired
        Grappled,  //Enemy is grappled by player for a short duration. It cannot move and usually cannot attack
        Projectile //Enemy has just been thrown or struck by player. It cannot affect its trajectory and will deal damage to other enemies it hits
    }

    //OBJECTS & COMPONENTS:
    private Entity_Player player; //PlayerController script, needs to be known by enemies
    public Collider2D dangerZone; //Area which triggers player interception

    //VARIABLES:
    [Header("States:")]
    public CombatStatus combatStatus; //Enemy status regarding player actions

//==|CORE LOOPS|==-------------------------------------------------------------------------------------------------
    public override void Start()
    {
        base.Start(); //Call base method
    }

    public override void Update()
    {
        base.Update(); //Call base method
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate(); //Call base method
    }

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
        if (dangerZone == null) //If enemy does not have an intercept field...
        {
            Debug.LogError(name + " does not have an intercept field assigned."); //Log error
        }

    }

}
