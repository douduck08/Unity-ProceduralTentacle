using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeRenderer : MonoBehaviour {

    [SerializeField] TubeTemplate tubeTemplate;
    [SerializeField] Material material;
    [SerializeField] float radius = 1f;
    [SerializeField] Transform[] nodes;

    ComputeBuffer positionBuffer;
    ComputeBuffer tangentBuffer;
    ComputeBuffer normalBuffer;

    void OnEnable () {
        positionBuffer = new ComputeBuffer (tubeTemplate.segments + 1, 4 * 3);
        tangentBuffer = new ComputeBuffer (tubeTemplate.segments + 1, 4 * 3);
        normalBuffer = new ComputeBuffer (tubeTemplate.segments + 1, 4 * 3);

        material.SetBuffer ("_PositionBuffer", positionBuffer);
        material.SetBuffer ("_TangentBuffer", tangentBuffer);
        material.SetBuffer ("_NormalBuffer", normalBuffer);
    }

    void OnDisable () {
        positionBuffer.Release ();
        tangentBuffer.Release ();
        normalBuffer.Release ();
    }

    void Update () {
        UpdateTubeData ();
    }

    void OnRenderObject () {
        material.SetFloat ("_Radius", Mathf.Max (0f, radius));
        Graphics.DrawMesh (tubeTemplate.GetMesh (), Vector3.zero, Quaternion.identity, material, 0);
    }

    void UpdateTubeData () {
        Vector3[] points, tangents, normals;
        GetHermiteCurve (nodes, tubeTemplate.segments, out points, out tangents, out normals);

        positionBuffer.SetData (points);
        tangentBuffer.SetData (tangents);
        normalBuffer.SetData (normals);
    }

    void OnDrawGizmos () {
        if (tubeTemplate == null || tubeTemplate.segments < 1) {
            return;
        }

        Vector3[] points, tangents, normals;
        GetHermiteCurve (nodes, tubeTemplate.segments, out points, out tangents, out normals);

        Gizmos.color = Color.white;
        for (int i = 1; i < points.Length; i++) {
            Gizmos.DrawLine (points[i - 1], points[i]);
        }

        for (int i = 0; i < points.Length; i++) {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine (points[i], points[i] + tangents[i] * 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine (points[i], points[i] + normals[i] * 0.1f);

            var binormal = Vector3.Cross (tangents[i], normals[i]);
            Gizmos.color = Color.green;
            Gizmos.DrawLine (points[i], points[i] + binormal * 0.1f);

        }
    }

    Vector3 GetHermiteCurvePoint (Transform from, Transform to, float t) {
        var pointA = from.position;
        var pointB = to.position;
        var dirA = from.forward * from.localScale.z;
        var dirB = to.forward * to.localScale.z;

        var t2 = t * t;
        var t3 = t2 * t;
        var point = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * dirA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * dirB;
        return point;
    }

    Vector3[] GetHermiteCurve (Transform from, Transform to, int subdivisions) {
        var pointA = from.position;
        var pointB = to.position;
        var dirA = from.forward * from.localScale.z;
        var dirB = to.forward * to.localScale.z;

        Vector3[] points = new Vector3[subdivisions + 1];
        for (int i = 0; i < subdivisions; i++) {
            var t = 1f * i / subdivisions;
            var t2 = t * t;
            var t3 = t2 * t;
            var point = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * dirA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * dirB;
            points[i] = point;
        }
        points[subdivisions] = pointB;
        return points;
    }

    Vector3[] GetHermiteCurve (Transform[] nodes, int subdivisions) {
        var points = new Vector3[subdivisions + 1];
        for (int i = 0; i < subdivisions; i++) {
            var t = 1f * i / subdivisions * (nodes.Length - 1);
            var idx = Mathf.FloorToInt (t);
            t %= 1f;
            var t2 = t * t;
            var t3 = t2 * t;

            var pointA = nodes[idx].position;
            var pointB = nodes[idx + 1].position;
            var dirA = nodes[idx].forward * nodes[idx].localScale.z;
            var dirB = nodes[idx + 1].forward * nodes[idx + 1].localScale.z;

            var point = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * dirA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * dirB;
            points[i] = point;
        }
        points[subdivisions] = nodes[nodes.Length - 1].position;
        return points;
    }

    void GetHermiteCurve (Transform[] nodes, int subdivisions, out Vector3[] points, out Vector3[] tangents, out Vector3[] normals) {
        points = new Vector3[subdivisions + 1];
        tangents = new Vector3[subdivisions + 1];
        normals = new Vector3[subdivisions + 1];
        for (int i = 0; i < subdivisions; i++) {
            var t = 1f * i / subdivisions * (nodes.Length - 1);
            var idx = Mathf.FloorToInt (t);

            var pointA = nodes[idx].position;
            var pointB = nodes[idx + 1].position;
            var dirA = nodes[idx].forward * nodes[idx].localScale.z;
            var dirB = nodes[idx + 1].forward * nodes[idx + 1].localScale.z;

            t %= 1f;
            var t2 = t * t;
            var t3 = t2 * t;
            var point = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * dirA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * dirB;
            var tangent = (6 * t2 - 6 * t) * pointA + (3 * t2 - 4 * t + 1) * dirA + (-6 * t2 + 6 * t) * pointB + (3 * t2 - 2 * t) * dirB;
            tangent = tangent.normalized;

            pointA = pointA + nodes[idx].right;
            pointB = pointB + nodes[idx + 1].right;
            var rightPoint = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * dirA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * dirB;
            var normal = (rightPoint - point).normalized;

            points[i] = point;
            tangents[i] = tangent;
            normals[i] = normal;
        }
        points[subdivisions] = nodes[nodes.Length - 1].position;
        tangents[subdivisions] = nodes[nodes.Length - 1].forward;
        normals[subdivisions] = nodes[nodes.Length - 1].right;
    }
}