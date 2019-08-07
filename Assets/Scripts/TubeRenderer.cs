using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeRenderer : MonoBehaviour {

    [Header ("Debug")]
    [SerializeField] bool displayLine;
    [SerializeField] bool displayDirection;
    [SerializeField] bool displayRegionCircle;

    [Header ("Tube")]
    [SerializeField] TubeTemplate tubeTemplate;
    [SerializeField] Material material;
    [SerializeField] float radius = 1f;

    [Header ("Poisson Disc Sampling")]
    [SerializeField] float distributeRadius = 1f;
    [SerializeField] float regionRadius = 1f;

    [Header ("Hermite Curve")]
    [SerializeField] Transform[] nodes;

    ComputeBuffer positionBuffer;
    ComputeBuffer tangentBuffer;
    ComputeBuffer normalBuffer;

    void OnEnable () {
        if (tubeTemplate == null || material == null) {
            this.enabled = false;
            return;
        }

        positionBuffer = new ComputeBuffer (tubeTemplate.segments + 1, 4 * 3);
        tangentBuffer = new ComputeBuffer (tubeTemplate.segments + 1, 4 * 3);
        normalBuffer = new ComputeBuffer (tubeTemplate.segments + 1, 4 * 3);

        material.SetBuffer ("_PositionBuffer", positionBuffer);
        material.SetBuffer ("_TangentBuffer", tangentBuffer);
        material.SetBuffer ("_NormalBuffer", normalBuffer);
    }

    void OnDisable () {
        if (positionBuffer != null) {
            positionBuffer.Release ();
            positionBuffer = null;
        }
        if (tangentBuffer != null) {
            tangentBuffer.Release ();
            tangentBuffer = null;
        }
        if (normalBuffer != null) {
            normalBuffer.Release ();
            normalBuffer = null;
        }
    }

    void Update () {
        UpdateTubeData ();
    }

    void OnRenderObject () {
        Graphics.DrawMesh (tubeTemplate.GetMesh (), Vector3.zero, Quaternion.identity, material, 0);
    }

    void UpdateTubeData () {
        Vector3[] points, tangents, normals;
        points = HermiteCurve.GetPoints (nodes, tubeTemplate.segments, out tangents, out normals);

        positionBuffer.SetData (points);
        tangentBuffer.SetData (tangents);
        normalBuffer.SetData (normals);

        material.SetFloat ("_Radius", Mathf.Max (0f, radius));
    }

    void OnDrawGizmos () {
        if (tubeTemplate == null || tubeTemplate.segments < 1) {
            return;
        }

        if (displayRegionCircle) {
            Gizmos.color = Color.white;
            foreach (var node in nodes) {
                var center = node.position;
                var right = node.right;
                var up = node.up;
                for (int i = 0; i < 16; i++) {
                    var phi = Mathf.PI * 2 * (i / 16f);
                    var pointA = center + right * regionRadius * Mathf.Cos (phi) + up * regionRadius * Mathf.Sin (phi);
                    phi = Mathf.PI * 2 * ((i + 1) / 16f);
                    var pointB = center + right * regionRadius * Mathf.Cos (phi) + up * regionRadius * Mathf.Sin (phi);
                    Gizmos.DrawLine (pointA, pointB);
                }
            }
        }

        if (displayLine || displayDirection) {
            Vector3[] points, tangents, normals;
            points = HermiteCurve.GetPoints (nodes, tubeTemplate.segments, out tangents, out normals);

            if (displayLine) {
                Gizmos.color = Color.white;
                for (int i = 1; i < points.Length; i++) {
                    Gizmos.DrawLine (points[i - 1], points[i]);
                }
            }

            if (displayDirection) {
                for (int i = 0; i < points.Length; i++) {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine (points[i], points[i] + tangents[i] * 0.1f);

                    Gizmos.color = Color.red;
                    Gizmos.DrawLine (points[i], points[i] + normals[i] * 0.1f);

                    var binormal = Vector3.Cross (tangents[i], normals[i]).normalized;
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine (points[i], points[i] + binormal * 0.1f);
                }
            }
        }
    }
}