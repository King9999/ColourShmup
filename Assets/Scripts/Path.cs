using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/* Used to generate different paths for enemies to use. My end goal is to have a set of paths that are randomly selected for the
 * enemy to follow. For more complex paths, I use Bezier curves. */
public class Path : MonoBehaviour
{
    //public LineRenderer linePath;
    //const int PATH_COUNT = 2;  
    public List<Vector3>[] pathPoints /* = new List<Vector3>[PATH_COUNT]*/;  //an array of paths
    //public bool boundaryOnLeft;    //used to check if enemy spawn point was on right side and needs to die when it reaches left side of screen.

    public Transform[] controlPoints = new Transform[4];   //4 points max per curve

    //path names
    public enum PathType
    {
        LinearVertical,
        LinearHorizontal,
        LPattern,
        Curve
    };


    // Start is called before the first frame update
    public Path()
    {
        int pathCount = Enum.GetNames(typeof(PathType)).Length;
        pathPoints = new List<Vector3>[pathCount];
        //boundaryOnLeft = false;

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
        //Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);

        //Linear vertical path from top to bottom of screen
        //float xValue = UnityEngine.Random.Range(screenPos.x * -GameManager.instance.ScreenBoundaryX(), screenPos.x * GameManager.instance.ScreenBoundaryX());
        //pathPoints[(int)PathType.LinearVertical].Add(new Vector3(xValue, screenPos.y * GameManager.instance.ScreenBoundaryY() + 1, 0));
        //pathPoints[(int)PathType.LinearVertical].Add(new Vector3(xValue, screenPos.y * -GameManager.instance.ScreenBoundaryY(), 0));

        /*for (int i = 0; i < pathPoints[(int)PathType.LinearVertical].Count; i++)
        {
            Debug.Log("Linear Vertical path points: " + pathPoints[(int)PathType.LinearVertical][i]);
        }*/
        
        //Horizontal path from right to left.
        //pathPoints[(int)PathType.LinearHorizontal].Add(Vector3.zero);
    }

    public void SetPath(List<Vector3>[] points, PathType pathType)
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);
        float xValue, yValue;
        points[(int)pathType] = new List<Vector3>();

        //re-initialize the selected path
        if (pathType == PathType.LinearVertical)    //enemy moves from top to bottom of screen in a straight line
        {
            xValue = UnityEngine.Random.Range(screenPos.x * -GameManager.instance.ScreenBoundaryX(), screenPos.x * GameManager.instance.ScreenBoundaryX());
            points[(int)pathType].Add(new Vector3(xValue, screenPos.y * GameManager.instance.ScreenBoundaryY() + 1, 0));
            points[(int)pathType].Add(new Vector3(xValue, screenPos.y * -GameManager.instance.ScreenBoundaryY(), 0));
        }
        else if (pathType == PathType.LinearHorizontal)  //enemy moves from right to left of screen in a straight line
        {
            float randValue = (UnityEngine.Random.value <= 0.5f) ? 1 : -1;  //used to determine which side enemy spawns from
            //boundaryOnLeft = (randValue > 0) ? true : false;
            yValue = UnityEngine.Random.Range(screenPos.y * -GameManager.instance.ScreenBoundaryY(), screenPos.y * GameManager.instance.ScreenBoundaryY());
            points[(int)pathType].Add(new Vector3(screenPos.x * randValue * GameManager.instance.ScreenBoundaryX(), yValue, 0));
            points[(int)pathType].Add(new Vector3(screenPos.x * randValue * -GameManager.instance.ScreenBoundaryX(), yValue, 0));
        }
        else if (pathType == PathType.LPattern) //tracks player position.
        {
            //float xValue = UnityEngine.Random.Range(screenPos.x * -GameManager.instance.ScreenBoundaryX(), screenPos.x * GameManager.instance.ScreenBoundaryX());
            float randValue = (UnityEngine.Random.value <= 0.5f) ? 1 : -1;
            xValue = GameManager.instance.playerPos.x;
            yValue = GameManager.instance.playerPos.y;
            points[(int)pathType].Add(new Vector3(xValue, screenPos.y * GameManager.instance.ScreenBoundaryY() + 1, 0));
            points[(int)pathType].Add(new Vector3(xValue, yValue, 0));
            points[(int)pathType].Add(new Vector3(xValue + (randValue * GameManager.instance.ScreenBoundaryX() + 1), yValue, 0));

            /*for (int i = 0; i < pathPoints[(int)pathType].Count; i++)
            {
                Debug.Log("path points: " + pathPoints[(int)pathType][i]);
            }*/
            

        }
        else if (pathType == PathType.Curve)
        {
            //enemy comes in from the side and moves in a circular pattern.
        }

        //return pathPoints[(int)pathType];

    }

    public void AddPoint(int pathNumber, Vector3 point)
    {
        pathPoints[pathNumber].Add(point);
    }

    //public void DrawPath()
    private void OnDrawGizmos()
    {
        Vector3 position;   //used to draw the path
        for (float t = 0; t <= 1; t += 0.05f)
        {
            //Cubic Bezier curve formula
            position = Mathf.Pow(1 - t, 3) * controlPoints[0].position + 3 * Mathf.Pow(1 - t, 2) * t * controlPoints[1].position
                + 3 * (1 - t) * Mathf.Pow(t, 2) * controlPoints[2].position + Mathf.Pow(t, 3) * controlPoints[3].position;

            Gizmos.DrawSphere(position, 0.25f);
        }
    }

}
