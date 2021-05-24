using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/* Used to generate different paths for enemies to use. My end goal is to have a set of paths that are randomly selected for the
 * enemy to follow. */
public class Path
{
    //public LineRenderer linePath;
    //const int PATH_COUNT = 2;  
    public List<Vector3>[] pathPoints /* = new List<Vector3>[PATH_COUNT]*/;  //an array of paths


    //path names
    public enum PathType
    {
        LinearVertical,
        LinearHorizontal
    };


    // Start is called before the first frame update
    public Path()
    {
        int pathCount = Enum.GetNames(typeof(PathType)).Length;
        pathPoints = new List<Vector3>[pathCount];

        //need to initialize each list in the array
        for (int i = 0; i < pathCount; i++)
        {
            pathPoints[i] = new List<Vector3>();   
        }

        //pathPoints = new List<Vector3>[PATH_COUNT];
        //the first point that is added to the list is always the enemy's start position, which should be off screen.
        //this is the test path
        //pathPoints = new List<Vector3>();
        // pathPoints.Add(Vector3.zero);           //need enemy position in the enemy list.
        //pathPoints.Add(Vector3.down);      
        //linePath.positionCount = 2;
        //linePath.startWidth = 0.1f;
        //linePath.endWidth = 0.1f;
        //linePath.SetPositions(pathPoints.ToArray());

        //*************All paths go in here******************
        Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);

        //Linear vertical path from top to bottom of screen
        float xValue = UnityEngine.Random.Range(screenPos.x * -GameManager.instance.ScreenBoundaryX(), screenPos.x * GameManager.instance.ScreenBoundaryX());
        pathPoints[(int)PathType.LinearVertical].Add(new Vector3(xValue, screenPos.y * GameManager.instance.ScreenBoundaryY() + 1, 0));
        pathPoints[(int)PathType.LinearVertical].Add(new Vector3(xValue, screenPos.y * -GameManager.instance.ScreenBoundaryY(), 0));

        for (int i = 0; i < pathPoints[(int)PathType.LinearVertical].Count; i++)
        {
            Debug.Log("Linear Vertical path points: " + pathPoints[(int)PathType.LinearVertical][i]);
        }
        
        //Horizontal path from right to left.
        //pathPoints[(int)PathType.LinearHorizontal].Add(Vector3.zero);
    }

    public List<Vector3> SetPath(PathType pathType)
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);
        pathPoints[(int)pathType] = new List<Vector3>();

        //re-initialize the selected path
        if (pathType == PathType.LinearVertical)
        {
            float xValue = UnityEngine.Random.Range(screenPos.x * -GameManager.instance.ScreenBoundaryX(), screenPos.x * GameManager.instance.ScreenBoundaryX());
            pathPoints[(int)pathType].Add(new Vector3(xValue, screenPos.y * GameManager.instance.ScreenBoundaryY() + 1, 0));
            pathPoints[(int)pathType].Add(new Vector3(xValue, screenPos.y * -GameManager.instance.ScreenBoundaryY(), 0));
        }

        return pathPoints[(int)pathType];

    }

    public void AddPoint(int pathNumber, Vector3 point)
    {
        pathPoints[pathNumber].Add(point);
    }

    /*public void DrawPath()
    {
        if (pathPoints.Count <= 0)
        {
            Debug.Log("No points to draw path");
            return;
        }

        linePath.positionCount = pathPoints.Count;
        linePath.startWidth = 0.1f;
        linePath.endWidth = 0.1f;
        linePath.SetPositions(pathPoints.ToArray());
    }*/

}
