using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HermiteCurve {

    public static Vector3 GetPoint (Vector3 pointA, Vector3 pointB, Vector3 tangentA, Vector3 tangentB, float t) {
        var t2 = t * t;
        var t3 = t2 * t;
        var point = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * tangentA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * tangentB;
        return point;
    }

    public static Vector3 GetPoint (Transform from, Transform to, float t) {
        return GetPoint (from.position, to.position, from.forward * from.localScale.z, to.forward * to.localScale.z, t);
    }

    public static Vector3 GetTangent (Vector3 pointA, Vector3 pointB, Vector3 tangentA, Vector3 tangentB, float t) {
        var t2 = t * t;
        var t3 = t2 * t;
        var tangent = (6 * t2 - 6 * t) * pointA + (3 * t2 - 4 * t + 1) * tangentA + (-6 * t2 + 6 * t) * pointB + (3 * t2 - 2 * t) * tangentB;
        return tangent;
    }

    public static Vector3 GetTangent (Transform from, Transform to, float t) {
        return GetTangent (from.position, to.position, from.forward * from.localScale.z, to.forward * to.localScale.z, t);
    }

    public static Vector3[] GetPoints (Vector3 pointA, Vector3 pointB, Vector3 tangentA, Vector3 tangentB, int subdivisions) {
        Vector3[] points = new Vector3[subdivisions + 1];
        for (int i = 0; i < subdivisions; i++) {
            var t = 1f * i / subdivisions;
            points[i] = GetPoint (pointA, pointB, tangentA, tangentB, t);
        }
        points[subdivisions] = pointB;
        return points;
    }

    public static Vector3[] GetPoints (Transform from, Transform to, int subdivisions) {
        return GetPoints (from.position, to.position, from.forward * from.localScale.z, to.forward * to.localScale.z, subdivisions);
    }

    public static Vector3[] GetPoints (Vector3 pointA, Vector3 pointB, Vector3 tangentA, Vector3 tangentB, int subdivisions, float tMin, float tMax) {
        Vector3[] points = new Vector3[subdivisions + 1];
        for (int i = 0; i < subdivisions; i++) {
            var t = Mathf.Lerp (tMin, tMax, 1f * i / subdivisions);
            points[i] = GetPoint (pointA, pointB, tangentA, tangentB, t);
        }
        points[subdivisions] = pointB;
        return points;
    }

    public static Vector3[] GetPoints (Transform from, Transform to, int subdivisions, float tMin, float tMax) {
        return GetPoints (from.position, to.position, from.forward * from.localScale.z, to.forward * to.localScale.z, subdivisions, tMin, tMax);
    }

    public static Vector3[] GetPoints (Vector3[] inputPoints, Vector3[] inputTangents, int subdivisions) {
        if (inputPoints.Length != inputTangents.Length) {
            throw new System.InvalidOperationException ("'inputPoints' and 'inputTangents' should have the same length");
        }

        var inputLength = inputPoints.Length;
        var points = new Vector3[subdivisions + 1];
        for (int i = 0; i < subdivisions; i++) {
            var t = 1f * i / subdivisions * (inputLength - 1);
            var idx = Mathf.FloorToInt (t);
            points[i] = GetPoint (inputPoints[idx], inputPoints[idx + 1], inputTangents[idx], inputTangents[idx + 1], t % 1f);
        }
        points[subdivisions] = inputPoints[inputLength - 1];
        return points;
    }

    public static Vector3[] GetPoints (Transform[] nodes, int subdivisions) {
        var inputPoints = new Vector3[nodes.Length];
        var inputTangents = new Vector3[nodes.Length];
        for (int i = 0; i < nodes.Length; i++) {
            inputPoints[i] = nodes[i].position;
            inputTangents[i] = nodes[i].forward * nodes[i].localScale.z;
        }
        return GetPoints (inputPoints, inputTangents, subdivisions);
    }

    public static Vector3[] GetPoints (Vector3[] inputPoints, Vector3[] inputTangents, Vector3[] inputUps, int subdivisions, out Vector3[] tangents, out Vector3[] normals) {
        if (inputPoints.Length != inputTangents.Length) {
            throw new System.InvalidOperationException ("'inputPoints' and 'inputTangents' should have the same length");
        }
        if (inputPoints.Length != inputUps.Length) {
            throw new System.InvalidOperationException ("'inputPoints' and 'inputUps' should have the same length");
        }

        var inputLength = inputPoints.Length;
        var points = new Vector3[subdivisions + 1];
        tangents = new Vector3[subdivisions + 1];
        normals = new Vector3[subdivisions + 1];
        for (int i = 0; i < subdivisions; i++) {
            var t = 1f * i / subdivisions * (inputLength - 1);
            var idx = Mathf.FloorToInt (t);
            t %= 1f;
            points[i] = GetPoint (inputPoints[idx], inputPoints[idx + 1], inputTangents[idx], inputTangents[idx + 1], t);

            var tangent = GetTangent (inputPoints[idx], inputPoints[idx + 1], inputTangents[idx], inputTangents[idx + 1], t);
            tangent = tangent.normalized;
            tangents[i] = tangent;

            var upPoint = GetPoint (inputPoints[idx] + inputUps[idx], inputPoints[idx + 1] + inputUps[idx + 1], inputTangents[idx], inputTangents[idx + 1], t);
            var up = upPoint - points[i];
            var normal = Vector3.Cross (up, tangent).normalized;
            normals[i] = normal;
        }
        points[subdivisions] = inputPoints[inputLength - 1];
        tangents[subdivisions] = inputTangents[inputLength - 1];
        normals[subdivisions] = Vector3.Cross (inputUps[inputLength - 1], inputTangents[inputLength - 1]).normalized;;
        return points;
    }

    public static Vector3[] GetPoints (Transform[] nodes, int subdivisions, out Vector3[] tangents, out Vector3[] normals) {
        var inputPoints = new Vector3[nodes.Length];
        var inputTangents = new Vector3[nodes.Length];
        var inputUps = new Vector3[nodes.Length];
        for (int i = 0; i < nodes.Length; i++) {
            inputPoints[i] = nodes[i].position;
            inputTangents[i] = nodes[i].forward * nodes[i].localScale.z;
            inputUps[i] = nodes[i].up;
        }
        return GetPoints (inputPoints, inputTangents, inputUps, subdivisions, out tangents, out normals);
    }

    public static Vector3[] GetPoints (Transform[] nodes, Vector2 offset, int subdivisions, out Vector3[] tangents, out Vector3[] normals) {
        var inputPoints = new Vector3[nodes.Length];
        var inputTangents = new Vector3[nodes.Length];
        var inputUps = new Vector3[nodes.Length];
        for (int i = 0; i < nodes.Length; i++) {
            inputPoints[i] = nodes[i].position + nodes[i].right * nodes[i].localScale.x * offset.x + nodes[i].up * nodes[i].localScale.y * offset.y;
            inputTangents[i] = nodes[i].forward * nodes[i].localScale.z;
            inputUps[i] = nodes[i].up;
        }
        return GetPoints (inputPoints, inputTangents, inputUps, subdivisions, out tangents, out normals);
    }
}