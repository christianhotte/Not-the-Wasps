using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraMaster : MonoBehaviour
{
    /* Description: -Goes on Main camera and contains references to all other cameras in scene
     *              -Governs camera-related events and effects, including zoom-in
     *              -Contains functionality for partitioning control of camera between various scripts
     */

    //CLASSES, ENUMS & STRUCTS:

    //OBJECTS & COMPONENTS:
    private Camera mainCamera; //Main camera component, should be on same object as CameraMaster
    public CinemachineVirtualCamera playerCamera; //Player follower camera object

    //VARIABLES:
    //Memory:
    internal float baseOrthoSize; //Initial orthographic size of player camera, to which it will return when unmodified by outside factors

//==|CORE LOOPS|==-------------------------------------------------------------------------------------------------
    private void Start()
    {
        Initialize(); //Initialize variables and components
    }

//==|CORE FUNCTIONS|==---------------------------------------------------------------------------------------------
    private void Initialize()
    {
        //Description: Method called at Start to set up and validate core components

        //Attempt to Get Components:
        mainCamera = GetComponent<Camera>(); //Attempt to get main camera component

        //Contingencies:
        if (mainCamera == null) //If main camera component could not be found...
        {
            Debug.LogError("CameraMaster could not find main camera component."); //Log error
        }
        if (playerCamera == null) //If no player camera was given...
        {
            Debug.LogError("CameraMaster could not find player CM vcam."); //Log error
        }

        //Variable Setup:
        baseOrthoSize = mainCamera.orthographicSize; //Record base orthographic size to memory
    }
}
