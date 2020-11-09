using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class TimeKeeper : MonoBehaviour
{
    //Description: Used to regulate and unify the passage of time between objects in-game

    //CLASSES, ENUMS & STRUCTS:

    //OBJECTS & COMPONENTS:

    //VARIABLES:
    [Header("Time Scale:")]
    public float baseTimeScale;        //The default timescale to which time should return when not influenced by outside factors
    [ShowOnly] public float timeScale; //The rate at which time in-game progresses

//==|CORE LOOPS|==-------------------------------------------------------------------------------------------------
    private void Start()
    {
        Initialize(); //Initialize TimeKeeper
    }

//==|CORE FUNCTIONS|==---------------------------------------------------------------------------------------------
    private void Initialize()
    {
        //Description: Method called at Start to set up and validate core components

        //Establish Starting Variables:
        timeScale = baseTimeScale; //Set initial timescale to known base
    }

}
