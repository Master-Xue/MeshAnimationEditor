using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshFrame 
{

    public MeshFrame(Vector3[] vertices, int index)
    {
        _SelfIndex = index;
        this.Vertices = vertices;
    }

    public Vector3[] Vertices { get; set; }

    public int _SelfIndex { get; set; }

    
}
