using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class EditorGizmoProperties : ScriptableObject
{
    /*  Description: -Broad scriptableobject used to contain a series of unassociated settings groups related to in-editor gizmos
     *               -This scriptableobject is only intended to have one actual object which is referenced by various other scripts throughout the game
     *               -This object is being created to minimize that amount of reiteration between similar settings on things like Equipment_Arms
     */

//==|EQUIPMENT_ARM GIZMOS|==----------------------------------------------------------------------------------------------
    [Header("Arm Equipment Gizmos:")]
    public bool enableRangeGizmos;                  //Used to enable or disable range gizmo drawing
    public Color[] rangeGizmoColors = new Color[3]; //Array of colors to indicate which range gizmo is which (in order of appearance in stats list)
    public float rangeGizmoLength;                  //Length of lines which show ranges of combat maneuvers

}
