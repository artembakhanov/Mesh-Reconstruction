using Bakhanov.VoxelSet;
using System.Collections.Generic;
using UnityEngine;

public class NewPointsArgs
{
    public List<int> newPoints; 

    public NewPointsArgs(List<int> newPoints)
    {
        this.newPoints = newPoints;
    }
}