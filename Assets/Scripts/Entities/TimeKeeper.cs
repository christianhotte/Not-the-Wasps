using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeKeeper : MonoBehaviour
{
    //Description: Used to regulate and unify the passage of time between objects in-game.

    public float baseTimeScale; //The default timescale to which time should return when not influenced by outside factors
    [ShowOnly] public float timeScale; //The rate at which time in-game progresses

    private void Start()
    {
        //Initialize Timescale:
        timeScale = baseTimeScale; //Set initial timescale to known base
    }
}
