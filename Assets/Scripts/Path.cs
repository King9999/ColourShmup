using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Used to generate different paths for enemies to use. My end goal is to have a set of paths that are randomly selected for the
 * enemy to follow. */
public class Path : MonoBehaviour
{
    //public LineRenderer linePath;
    const int PATH_COUNT = 2;
    public List<Vector3>[] pathPoints = new List<Vector3>[PATH_COUNT];  //an array of paths

    // Start is called before the first frame update
    void Start()
    {
        //the first point that is added to the list is always the enemy's start position, which should be off screen.
        //this is the test path
        //pathPoints = new List<Vector3>();
       // pathPoints.Add(Vector3.zero);           //need enemy position in the enemy list.
        //pathPoints.Add(Vector3.down);      
        //linePath.positionCount = 2;
        //linePath.startWidth = 0.1f;
        //linePath.endWidth = 0.1f;
        //linePath.SetPositions(pathPoints.ToArray());
    }

    public void SetPath(List<Vector3> points)
    {

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