﻿using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary>
/// Base class for all Meshes.
/// </summary>
public abstract class MeshBase : MonoBehaviour {

    //common mesh components
    protected Mesh mesh;
    protected MeshFilter C_MF;
    protected MeshRenderer C_MR;
    
    //math constants
    protected static readonly float deg90 = Mathf.Deg2Rad * 90f;
    protected static readonly float deg360 = 2 * Mathf.PI;

    //when enabled, mesh.Optimize() is invoked with building the mesh
    //impact on performance: https://docs.unity3d.com/ScriptReference/Mesh.Optimize.html
    protected static bool OptimizeMesh = true;

    //update mesh in MeshFilter component
    public abstract void UpdateMesh();
    //update attached colliders
    public abstract void UpdateCollider();
    //find necessary components
    public abstract void GetOrAddComponents();

    //return center of object (this function may be overrided, if origin should be changed)
    public virtual Vector2 GetCenter()
    {
        return transform.position;
    }

    #region Building helper functions

    // checks the side point {v} it lays on, relative to segment {v1,v2}
    protected int GetSide(Vector3 v1, Vector3 v2, Vector3 v)
    {
        //using {Math} instead of {Mathf}, because Mathf.Sign returns {1} for {0}!
        return Math.Sign((v1.x - v.x) * (v2.y - v.y) - (v2.x - v.x) * (v1.y - v.y));
    }

    //in case of sprites, all normals can be just {Vector3.up}
    protected Vector3[] AddMeshNormals(int verticesLength)
    {
        Vector3[] normals = new Vector3[verticesLength];
        for (int i = 0; i < verticesLength; i++)
        {
            normals[i] = Vector3.up;
        }
        return normals;
    }

    //convert Vector3 array to Vector3 one
    protected static Vector2[] ConvertVec3ToVec2(Vector3[] verts3D)
    {
        Vector2[] verts2D = new Vector2[verts3D.Length];
        for (int i = 0; i < verts3D.Length; i++)
        {
            verts2D[i] = verts3D[i];
        }
        return verts2D;
    }

    #endregion

    #region Joints Physics

    //add HingeJoint2D at the center of the object and attach it to background
    public HingeJoint2D AddHingeJoint()
    {
        HingeJoint2D C_HJ2D = gameObject.AddComponent<HingeJoint2D>();
        C_HJ2D.anchor = transform.InverseTransformPoint(GetCenter());
        return C_HJ2D;
    }

    //specify motor
    public HingeJoint2D AddHingeJoint(JointMotor2D C_JM2D)
    {
        HingeJoint2D C_HJ2D = gameObject.AddComponent<HingeJoint2D>();
        C_HJ2D.anchor = transform.InverseTransformPoint(GetCenter());
        C_HJ2D.motor = C_JM2D;
        C_HJ2D.useMotor = true;
        return C_HJ2D;
    }

    //specify motor and connected body
    public HingeJoint2D AddHingeJoint(JointMotor2D C_JM2D, Rigidbody2D connectedBody)
    {
        HingeJoint2D C_HJ2D = gameObject.AddComponent<HingeJoint2D>();
        C_HJ2D.anchor = transform.InverseTransformPoint(GetCenter());
        C_HJ2D.motor = C_JM2D;
        C_HJ2D.useMotor = true;
        C_HJ2D.connectedBody = connectedBody;
        return C_HJ2D;
    }
    //fix object to background
    public FixedJoint2D AddFixedJoint()
    {
        FixedJoint2D C_HJ2D = gameObject.AddComponent<FixedJoint2D>();
        C_HJ2D.anchor = transform.InverseTransformPoint(GetCenter());
        return C_HJ2D;
    }

    //joins two shapes by FixedJoint2D
    public static bool Join(MeshBase meshA, MeshBase meshB)
    {
        FixedJoint2D C_FJ2D = meshA.gameObject.AddComponent<FixedJoint2D>();
        C_FJ2D.connectedBody = meshB.GetComponent<Rigidbody2D>();
        if (C_FJ2D.connectedBody)
        {
            return false;
        }
        C_FJ2D.anchor = meshA.transform.InverseTransformPoint(meshA.GetCenter());
        C_FJ2D.anchor = meshB.transform.InverseTransformPoint(meshB.GetCenter());
        return true;
    }

    //join with other shape by FixedJoint2D
    public bool JoinTo(MeshBase otherMesh)
    {
        FixedJoint2D C_FJ2D = gameObject.AddComponent<FixedJoint2D>();
        C_FJ2D.connectedBody = otherMesh.GetComponent<Rigidbody2D>();
        if (C_FJ2D.connectedBody == null)
        {
            return false;
        }
        C_FJ2D.anchor = transform.InverseTransformPoint(GetCenter());
        C_FJ2D.anchor = otherMesh.transform.InverseTransformPoint(otherMesh.GetCenter());
        return true;
    }

    #endregion

    #region UV Unwrapping

    protected static Vector4 GetBounds(Vector3[] vec)
    {
        /* x - minX
         * y - minY
         * z - maxX
         * w - maxY
         */
        float x = float.MaxValue;
        float y = float.MaxValue;
        float z = float.MinValue;
        float w = float.MinValue;
        for (int i = 0; i < vec.Length; i++)
        {
            if (vec[i].x < x)
            {
                x = vec[i].x;
            }
            if (vec[i].y < y)
            {
                y = vec[i].y;
            }
            if (vec[i].x > z)
            {
                z = vec[i].x;
            }
            if (vec[i].y > w)
            {
                w = vec[i].y;
            }
        }
        return new Vector4(x, y, z, w);
    }
    protected static Vector4 GetBounds(Vector2[] vec)
    {
        /* x - minX
         * y - minY
         * z - maxX
         * w - maxY
         */
        float x = float.MaxValue;
        float y = float.MaxValue;
        float z = float.MinValue;
        float w = float.MinValue;
        for (int i = 0; i < vec.Length; i++)
        {
            if (vec[i].x < x)
            {
                x = vec[i].x;
            }
            if (vec[i].y < y)
            {
                y = vec[i].y;
            }
            if (vec[i].x > z)
            {
                z = vec[i].x;
            }
            if (vec[i].y > w)
            {
                w = vec[i].y;
            }
        }
        return new Vector4(x, y, z, w);
    }
    protected static List<Vector2> UVUnwrap(Vector3[] vertices)
    {
        List<Vector2> uv = new List<Vector2>();
        Vector4 boundingBox = GetBounds(vertices);
        float length = boundingBox.z - boundingBox.x;
        float width = boundingBox.w - boundingBox.y;
        for (int i = 0; i < vertices.Length; i++)
        {
            float ux = (vertices[i].x - boundingBox.x) / length;
            float uy = (vertices[i].y - boundingBox.y) / width;
            uv.Add(new Vector2(ux, uy));
        }
        return uv;
    }
    protected static List<Vector2> UVUnwrap(Vector2[] vertices)
    {
        List<Vector2> uv = new List<Vector2>();
        Vector4 boundingBox = GetBounds(vertices);
        float length = boundingBox.z - boundingBox.x;
        float width = boundingBox.w - boundingBox.y;
        for (int i = 0; i < vertices.Length; i++)
        {
            float ux = (vertices[i].x - boundingBox.x) / length;
            float uy = (vertices[i].y - boundingBox.y) / width;
            uv.Add(new Vector2(ux, uy));
        }
        return uv;
    }

    #endregion

    #region Mesh material

    //set physics material properties
    public void SetPhysicsMaterialProperties(float bounciness, float friction)
    {
        PhysicsMaterial2D sharedMaterial = gameObject.GetComponent<Collider2D>().sharedMaterial;
        if (sharedMaterial == null)
        {
            sharedMaterial = new PhysicsMaterial2D();
            gameObject.GetComponent<Collider2D>().sharedMaterial = sharedMaterial;
        }
        sharedMaterial.bounciness = bounciness;
        sharedMaterial.friction = friction;
    }

    //set physics material properties
    public void SetPhysicsMaterial(PhysicsMaterial2D C_PS2D)
    {
        gameObject.GetComponent<Collider2D>().sharedMaterial = C_PS2D;
    }

    //set material to random color
    public void SetRandomColor()
    {
        C_MR.material.color = Random.ColorHSV();
    }

    //set material
    public void SetMaterial(Material material)
    {
        C_MR.material = material;
    }

    //set material texture
    public void SetTexture(Texture texture)
    {
        C_MR.material.mainTexture = texture;
    }

    //set material and texture
    public void SetMaterial(Material material, Texture texture)
    {
        C_MR.material = material;
        C_MR.material.mainTexture = texture;
    }

    //get {MeshRenderer} reference
    public MeshRenderer GetMeshRenderer()
    {
        return C_MR;
    }

    #endregion

}
