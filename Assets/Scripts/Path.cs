using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used to generate different paths for enemies to use
public class Path : MonoBehaviour
{
    public LineRenderer linePath;
    List<Vector3> linePoints;

    // Start is called before the first frame update
    void Start()
    {
        linePoints = new List<Vector3>();
        linePoints.Add(Vector3.zero);
        linePoints.Add(Vector3.down);      
        linePath.positionCount = 2;
        linePath.startWidth = 0.1f;
        linePath.endWidth = 0.1f;
        linePath.SetPositions(linePoints.ToArray());
    }

}
