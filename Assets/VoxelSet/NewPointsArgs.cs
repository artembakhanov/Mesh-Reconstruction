using System.Collections.Generic;
using UnityEngine;

public class NewPointsArgs
{
    public List<Vector3> newPoints; 

    public NewPointsArgs(List<Vector3> newPoints)
    {
        this.newPoints = newPoints;
    }
}