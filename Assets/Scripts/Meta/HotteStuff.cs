using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotteStuff
{
public static class HotteDebug //--<<<|Expanded Debugging|>>>------------------------------------------------------------|
{
    //Basic:
    public static void Oog()
    {
        //USE: Prints a marker in console to indicate that a line of code has been executed

        //CREDIT: Created by Christian Hotte

        Debug.Log("Oog"); //Log oog
    }
    public static void Print(this object value)
    {
        //USE: Prints given value (shorter + quicker than normal debug)

        //CREDIT: Created by Christian Hotte

        if (value.ToString() == "") //If value does not translate directly to printable string...
        {
            Debug.LogError("Attempted to print unprintable value."); //Log error
            return; //Cancel function
        }
        Debug.Log(value); //Log value in console
    }
    //Drawing:
    public static Vector2 DrawCircle(this Vector2 center, float radius, int sides)
    {
        //USE: Extends Vector2. Works similarly to Debug.DrawLine except it draws a 2D circle (using given Vector2 as center)

        //CREDIT: Created by Christian Hotte

        //Initializations:
        Vector2[] points = new Vector2[sides];     //Initialize list of all points in circle (size is equal to number of sides)
        Vector2 refPoint = new Vector2(0, radius); //Initialize reference point from which to extrapolate all other points
        float refAngle = 360 / sides;              //Initialize angle of each point vector from the point before it as a fraction of circle based on number of sides
                                                    //Find Points:
        for (int x = 0; x < sides; x++) //Parse through each point in circle...
        {
            float angle = refAngle * x; //Get actual angle of point based on its order in point array
            Vector2 point = refPoint.Rotate(angle); //Get base vector of point at given radius
            points[x] = point + center; //Position rotated point relative to center and add to point array
        }
        //Draw Sides:
        for (int x = 0; x < sides; x++) //Parse through each point in circle...
        {
            int nextPointIndex = x + 1; //Get index of next connecting point in line
            if (nextPointIndex >= sides) nextPointIndex = 0; //Overflow if necessary
            Debug.DrawLine(points[x], points[nextPointIndex]); //Draw line between each consecutive pair of points
        }
        //Cleanup:
        return center; //Pass center through (in case user wants to stick this on the end of an already-working line)
    }
    public static Vector2 DrawCircle(this Vector2 center, float radius, int sides, Color color)
    {
        //USE: Extends Vector2. Overload for DrawCircle that includes color. Works similarly to Debug.DrawLine except it draws a 2D circle (using given Vector2 as center)

        //CREDIT: Created by Christian Hotte

        //Initializations:
        Vector2[] points = new Vector2[sides];     //Initialize list of all points in circle (size is equal to number of sides)
        Vector2 refPoint = new Vector2(0, radius); //Initialize reference point from which to extrapolate all other points
        float refAngle = 360 / sides;              //Initialize angle of each point vector from the point before it as a fraction of circle based on number of sides
                                                    //Find Points:
        for (int x = 0; x < sides; x++) //Parse through each point in circle...
        {
            float angle = refAngle * x; //Get actual angle of point based on its order in point array
            Vector2 point = refPoint.Rotate(angle); //Get base vector of point at given radius
            points[x] = point + center; //Position rotated point relative to center and add to point array
        }
        //Draw Sides:
        for (int x = 0; x < sides; x++) //Parse through each point in circle...
        {
            int nextPointIndex = x + 1; //Get index of next connecting point in line
            if (nextPointIndex >= sides) nextPointIndex = 0; //Overflow if necessary
            Debug.DrawLine(points[x], points[nextPointIndex], color); //Draw line between each consecutive pair of points (add color)
        }
        //Cleanup:
        return center; //Pass center through (in case user wants to stick this on the end of an already-working line)
    }
    public static void DrawRect(this Rect rectangle, Color color)
    {
        //USE: Functions similarly to Debug.DrawLine except it draws a 2D rectangle (using given rect transform properties)

        //CREDIT: Created by Christian Hotte

        //Initializations:
        Vector2 topLeft = new Vector2(rectangle.xMin, rectangle.yMax); //Top left corner point
        Vector2 topRight = new Vector2(rectangle.xMax, rectangle.yMax); //Top right corner point
        Vector2 botRight = new Vector2(rectangle.xMax, rectangle.yMin); //Bottom right corner point
        Vector2 botLeft = new Vector2(rectangle.xMin, rectangle.yMin); //Bottom left corner point
        Vector2[] corners = { topLeft, topRight, botRight, botLeft }; //Place all four corners in array
                                                                        //Draw Sides:
        for (int x = 0; x < corners.Length; x++) //Parse through list of corners...
        {
            //Initialization:
            int nextCornerIndex = x + 1; if (nextCornerIndex >= corners.Length) nextCornerIndex = 0; //Get index of corner after this corner
            Vector2 corner1 = corners[x]; //Get primary corner location
            Vector2 corner2 = corners[nextCornerIndex]; //Get secondary corner location
                                                        //Draw Side:
            Debug.DrawLine(corner1, corner2, color); //Draw line between each two consecutive sides and in the specified color
        }
    }
    public static void DrawRect(this Bounds bounds, Color color)
    {
        //USE: Overflow for DrawRect which accepts bounding box. Functions similarly to Debug.DrawLine except it draws a 2D rectangle (using given rect transform properties)

        //CREDIT: Created by Christian Hotte

        //Initializations:
        Rect rectangle = bounds.BoundsToRect(); //Convert given bounds to rectangle
        Vector2 topLeft = new Vector2(rectangle.xMin, rectangle.yMax); //Top left corner point
        Vector2 topRight = new Vector2(rectangle.xMax, rectangle.yMax); //Top right corner point
        Vector2 botRight = new Vector2(rectangle.xMax, rectangle.yMin); //Bottom right corner point
        Vector2 botLeft = new Vector2(rectangle.xMin, rectangle.yMin); //Bottom left corner point
        Vector2[] corners = { topLeft, topRight, botRight, botLeft }; //Place all four corners in array
                                                                        //Draw Sides:
        for (int x = 0; x < corners.Length; x++) //Parse through list of corners...
        {
            //Initialization:
            int nextCornerIndex = x + 1; if (nextCornerIndex >= corners.Length) nextCornerIndex = 0; //Get index of corner after this corner
            Vector2 corner1 = corners[x]; //Get primary corner location
            Vector2 corner2 = corners[nextCornerIndex]; //Get secondary corner location
                                                        //Draw Side:
            Debug.DrawLine(corner1, corner2, color); //Draw line between each two consecutive sides and in the specified color
        }
    }
    public static void DrawRect(this Rect rectangle, Color color, float duration)
    {
        //USE: Functions similarly to Debug.DrawLine except it draws a 2D rectangle (using given rect transform properties)

        //CREDIT: Created by Christian Hotte

        //Initializations:
        Vector2 topLeft = new Vector2(rectangle.xMin, rectangle.yMax); //Top left corner point
        Vector2 topRight = new Vector2(rectangle.xMax, rectangle.yMax); //Top right corner point
        Vector2 botRight = new Vector2(rectangle.xMax, rectangle.yMin); //Bottom right corner point
        Vector2 botLeft = new Vector2(rectangle.xMin, rectangle.yMin); //Bottom left corner point
        Vector2[] corners = { topLeft, topRight, botRight, botLeft }; //Place all four corners in array
                                                                        //Draw Sides:
        for (int x = 0; x < corners.Length; x++) //Parse through list of corners...
        {
            //Initialization:
            int nextCornerIndex = x + 1; if (nextCornerIndex >= corners.Length) nextCornerIndex = 0; //Get index of corner after this corner
            Vector2 corner1 = corners[x]; //Get primary corner location
            Vector2 corner2 = corners[nextCornerIndex]; //Get secondary corner location
                                                        //Draw Side:
            Debug.DrawLine(corner1, corner2, color, duration); //Draw line between each two consecutive sides and in the specified color
        }
    }
    public static void DrawRect(this Bounds bounds, Color color, float duration)
    {
        //USE: Overflow for DrawRect which accepts bounding box. Functions similarly to Debug.DrawLine except it draws a 2D rectangle (using given rect transform properties)

        //CREDIT: Created by Christian Hotte

        //Initializations:
        Rect rectangle = bounds.BoundsToRect(); //Convert given bounds to rectangle
        Vector2 topLeft = new Vector2(rectangle.xMin, rectangle.yMax); //Top left corner point
        Vector2 topRight = new Vector2(rectangle.xMax, rectangle.yMax); //Top right corner point
        Vector2 botRight = new Vector2(rectangle.xMax, rectangle.yMin); //Bottom right corner point
        Vector2 botLeft = new Vector2(rectangle.xMin, rectangle.yMin); //Bottom left corner point
        Vector2[] corners = { topLeft, topRight, botRight, botLeft }; //Place all four corners in array
                                                                        //Draw Sides:
        for (int x = 0; x < corners.Length; x++) //Parse through list of corners...
        {
            //Initialization:
            int nextCornerIndex = x + 1; if (nextCornerIndex >= corners.Length) nextCornerIndex = 0; //Get index of corner after this corner
            Vector2 corner1 = corners[x]; //Get primary corner location
            Vector2 corner2 = corners[nextCornerIndex]; //Get secondary corner location
                                                        //Draw Side:
            Debug.DrawLine(corner1, corner2, color, duration); //Draw line between each two consecutive sides and in the specified color
        }
    }
}
public static class HotteMath //--<<<|More Math|>>>----------------------------------------------------------------------|
{
    //Basic:
    public static float Map(this float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        //USE: Maps a variable from one range to another

        //CREDIT: This code is borrowed wholesale from Unity forums user RazaTech: https://forum.unity.com/threads/re-map-a-number-from-one-range-to-another.119437/

        var fromAbs = value - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        var to = toAbs + toMin;

        return to;
    }
    public static float Mean(this float[] values)
    {
        //USE: Returns the average of all given values

        //CREDIT: Created by Christian Hotte

        //Initializations & Validations:
        int count = values.Length; //Get shorthand for number of values given
        if (count == 0) return 0; //If no values were given, return 0 (to avoid a divide by zero error)
        float total = 0; //Initialize variable to store total of given values

        //Perform Calculation:
        for (int x = 0; x < count; x++) //Iterate through given array of values
        {
            total += values[x]; //Add value to total
        }
        return total / count; //Return average of given values
    }
    //Vector Angles & Rotation:
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        //USE: Extends Vector2. Rotates a given vector by the desired degrees (in float form)

        //CREDIT: This code is borrowed wholesale from Unity forums user DDP: https://answers.unity.com/questions/661383/whats-the-most-efficient-way-to-rotate-a-vector2-o.html

        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
    public static float LookAt2D(this Vector3 infrom, Vector3 into)
    {
        //USE: Extends Vector2. Returns first vector facing second vector, 2D equivalent of Vector3.LookAt

        //CREDIT: This code is borrowed wholesale from Unity forums user DirtyDave: https://stackoverflow.com/questions/22813825/unity-lookat-2d-equivalent

        Vector2 from = Vector2.up;
        Vector3 to = into - infrom;
        float ang = Vector2.Angle(from, to);
        Vector3 cross = Vector3.Cross(from, to);
        if (cross.z > 0) { ang = 360 - ang; }
        ang *= -1f;
        return ang;
    }
    public static Vector2 AngleBetween(Vector2 pos1, Vector2 pos2)
    {
        //USE: Returns angle between positions 1 and 2 as normalized Vector2 (for 2D rotations)

        //CREDIT: Created by Christian Hotte (referencing work by Unity forums user Robertbu: https://answers.unity.com/questions/728680/how-to-get-the-angle-between-two-objects-with-ontr.html)

        Vector2 dir = pos1 - pos2; //Get difference between given position vectors
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; //Convert vector into useable angle
        Vector2 angleVector = Vector2.left.Rotate(angle); //Convert angle to normalized vector in 2D world space
        return angleVector; //Return found vector
    }
    //Mathf Upgrades:
    public static bool Approx(float a, float b, float error)
    {
        //USE: Compares two floating point values and returns true if their difference is within given error (more versatile version of Mathf.Approximately)

        //CREDIT: Created by Christian Hotte

        float difference = Mathf.Abs(a - b); //Get difference between given values
        if (difference > error) return false; //If difference is greater than given error, return false
        else return true; //Otherwise, values are within given range of each other, so return true
    }
}
public static class HotteConversions //--<<<|Convenient Type Alchemy|>>>-------------------------------------------------|
{
    public static Rect BoundsToRect(this Bounds bounds)
    {
        //USE: Extends Bounds. Converts bounds into more granular rect

        //CREDIT: Created by Christian Hotte

        return new Rect(bounds.min, bounds.size); //Return new rect with same dimensions as given bounds
    }
    public static Vector3 V3(this Vector2 v)
    {
        //USE: Extends Vector2. Returns given vector2 as vector3 (for easy addition and subtraction with other vector3 variables)

        //CREDIT: Created by Christian Hotte

        return new Vector3(v.x, v.y, 0);
    }
}
public static class HotteFind //--<<<|Finding Things in Things|>>>-------------------------------------------------------|
{
    public static GameObject FindInList(this List<Transform> list, string n)
    {
        //USE: Finds an object by name in a list

        //CREDIT: Created by Christian Hotte

        GameObject foundItem = null; //Initialize container for item to return
        for (int x = 0; x < list.Count; x++) //Parse through list of items
        {
            if (list[x].name.Contains(n)) //If there is an item in the collection with the desired name
            {
                foundItem = list[x].gameObject; //Assign this as item to return
                break; //Break loop
            }
        }
        return foundItem; //Return result
    }
    public static GameObject FindInList(this List<GameObject> list, string n)
    {
        //USE: Finds an object by name in a list (overflow for GameObject lists)

        //CREDIT: Created by Christian Hotte

        GameObject foundItem = null; //Initialize container for item to return
        for (int x = 0; x < list.Count; x++) //Parse through list of items
        {
            if (list[x].name.Contains(n)) //If there is an item in the collection with the desired name
            {
                foundItem = list[x].gameObject; //Assign this as item to return
                break; //Break loop
            }
        }
        return foundItem; //Return result
    }
    public static GameObject FindInList(this GameObject[] list, string n)
    {
        //USE: Finds an object by name in a list (overflow for Gameobject arrays)

        //CREDIT: Created by Christian Hotte

        GameObject foundItem = null; //Initialize container for item to return
        for (int x = 0; x < list.Length; x++) //Parse through list of item
        {
            if (list[x].name.Contains(n)) //If there is an item in the collection with the desired name
            {
                foundItem = list[x].gameObject; //Assign this as item to return
                break; //Break loop
            }
        }
        return foundItem; //Return result
    }
}
}

