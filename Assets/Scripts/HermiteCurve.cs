using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HermiteCurve {

    public static Vector3 GetPoint (Transform from, Transform to, float t) {
        var pointA = from.position;
        var pointB = to.position;
        var dirA = from.forward * from.localScale.z;
        var dirB = to.forward * to.localScale.z;

        var t2 = t * t;
        var t3 = t2 * t;
        var point = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * dirA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * dirB;
        return point;
    }

    public static Vector3[] GetPoints (Transform from, Transform to, int subdivisions) {
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

    public static Vector3[] GetPoints (Transform[] nodes, int subdivisions) {
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

    public static Vector3[] GetPoints (Transform[] nodes, int subdivisions, out Vector3[] tangents, out Vector3[] normals) {
        var points = new Vector3[subdivisions + 1];
        tangents = new Vector3[subdivisions + 1];
        normals = new Vector3[subdivisions + 1];
        for (int i = 0; i < subdivisions; i++) {
            var t = 1f * i / subdivisions * (nodes.Length - 1);
            var idx = Mathf.FloorToInt (t);
            var pointFrom = nodes[idx].position;
            var pointTo = nodes[idx + 1].position;

            var pointA = pointFrom;
            var pointB = pointTo;
            var dirA = nodes[idx].forward * nodes[idx].localScale.z;
            var dirB = nodes[idx + 1].forward * nodes[idx + 1].localScale.z;

            t %= 1f;
            var t2 = t * t;
            var t3 = t2 * t;
            var point = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * dirA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * dirB;
            var tangent = (6 * t2 - 6 * t) * pointA + (3 * t2 - 4 * t + 1) * dirA + (-6 * t2 + 6 * t) * pointB + (3 * t2 - 2 * t) * dirB;
            tangent = tangent.normalized;

            pointA = pointFrom + nodes[idx].up;
            pointB = pointTo + nodes[idx + 1].up;
            var upPoint = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * dirA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * dirB;
            var up = upPoint - point;
            var normal = Vector3.Cross (up, tangent).normalized;

            points[i] = point;
            tangents[i] = tangent;
            normals[i] = normal;
        }
        points[subdivisions] = nodes[nodes.Length - 1].position;
        tangents[subdivisions] = nodes[nodes.Length - 1].forward;
        normals[subdivisions] = nodes[nodes.Length - 1].right;
        return points;
    }

    public static Vector3[] GetPoints (Transform[] nodes, Vector2 offset, int subdivisions, out Vector3[] tangents, out Vector3[] normals) {
        var points = new Vector3[subdivisions + 1];
        tangents = new Vector3[subdivisions + 1];
        normals = new Vector3[subdivisions + 1];
        for (int i = 0; i < subdivisions; i++) {
            var t = 1f * i / subdivisions * (nodes.Length - 1);
            var idx = Mathf.FloorToInt (t);
            var pointFrom = nodes[idx].position + nodes[idx].right * nodes[idx].localScale.x * offset.x + nodes[idx].up * nodes[idx].localScale.y * offset.y;
            var pointTo = nodes[idx + 1].position + nodes[idx + 1].right * nodes[idx + 1].localScale.x * offset.x + nodes[idx + 1].up * nodes[idx + 1].localScale.y * offset.y;

            var pointA = pointFrom;
            var pointB = pointTo;
            var dirA = nodes[idx].forward * nodes[idx].localScale.z;
            var dirB = nodes[idx + 1].forward * nodes[idx + 1].localScale.z;

            t %= 1f;
            var t2 = t * t;
            var t3 = t2 * t;
            var point = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * dirA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * dirB;
            var tangent = (6 * t2 - 6 * t) * pointA + (3 * t2 - 4 * t + 1) * dirA + (-6 * t2 + 6 * t) * pointB + (3 * t2 - 2 * t) * dirB;
            tangent = tangent.normalized;

            pointA = pointFrom + nodes[idx].up;
            pointB = pointTo + nodes[idx + 1].up;
            var upPoint = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * dirA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * dirB;
            var up = upPoint - point;
            var normal = Vector3.Cross (up, tangent).normalized;

            points[i] = point;
            tangents[i] = tangent;
            normals[i] = normal;
        }
        points[subdivisions] = nodes[nodes.Length - 1].position + nodes[nodes.Length - 1].right * offset.x + nodes[nodes.Length - 1].up * offset.y;;
        tangents[subdivisions] = nodes[nodes.Length - 1].forward;
        normals[subdivisions] = nodes[nodes.Length - 1].right;
        return points;
    }
}