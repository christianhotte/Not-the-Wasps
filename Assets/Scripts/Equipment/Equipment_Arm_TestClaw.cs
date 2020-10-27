using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment_Arm_TestClaw : Equipment_Arm
{
    /*  TEST CLAW: Default arm equipment with no special abilities, used for testing purposes
     */

    //CLASSES, ENUMS & STRUCTS:

    //OBJECTS & COMPONENTS:

    //VARIABLES:

//==|CORE LOOPS|==--------------------------------------------------------------------------------------------------------
    private void Start()
    {
        Initialize(); //Initialize equipment
    }

    private void Update()
    {
        CheckForIntercepts(); //Check for any intercepts with enemies
    }

    private void FixedUpdate()
    {
        
    }

    //==|COMBAT ABILITIES|==--------------------------------------------------------------------------------------------------
    /*public override void Grapple()
    {
        base.Grapple(); //Call base method

    }*/
    /*public override void Release()
    {
        base.Release(); //Call base method

    }*/
    /*public override void Throw()
    {
        base.Throw(); //Call base method

    }*/
    /*public override void Clothesline()
    {
        base.Clothesline(); //Call base method

    }*/
    /*public override void Backhand()
    {
        base.Backhand(); //Call base method

    }*/
}
