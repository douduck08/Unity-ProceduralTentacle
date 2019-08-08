using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampler {

    public static List<Vector2> GeneratePoints (float radius, Vector2 regionSize, int samplesBeforeRejection = 30) {
        var cellSize = radius / Mathf.Sqrt (2f);

        var grid = new int[Mathf.CeilToInt (regionSize.x / cellSize), Mathf.CeilToInt (regionSize.y / cellSize)];
        var points = new List<Vector2> ();
        var spawnPoints = new List<Vector2> ();

        spawnPoints.Add (regionSize / 2f);
        while (spawnPoints.Count > 0) {
            var spawnIndex = Random.Range (0, spawnPoints.Count);
            var spawnCenter = spawnPoints[spawnIndex];

            var spawnSuccess = false;
            for (int i = 0; i < samplesBeforeRejection; i++) {
                var angle = Random.value * Mathf.PI * 2f;
                var candidatePoint = spawnCenter + new Vector2 (Mathf.Cos (angle), Mathf.Sin (angle)) * Random.Range (radius, radius * 2f);
                if (IsValidatePoint (candidatePoint, radius, regionSize, cellSize, points, grid)) {
                    points.Add (candidatePoint);
                    spawnPoints.Add (candidatePoint);
                    grid[Mathf.FloorToInt (candidatePoint.x / cellSize), Mathf.FloorToInt (candidatePoint.y / cellSize)] = points.Count;
                    spawnSuccess = true;
                    break;
                }
            }

            if (!spawnSuccess) {
                spawnPoints.RemoveAt (spawnIndex);
            }
        }

        return points;
    }

    static bool IsValidatePoint (Vector2 candidate, float radius, Vector2 regionSize, float cellSize, List<Vector2> points, int[, ] grid) {
        if (candidate.x >= 0f && candidate.x < regionSize.x && candidate.y >= 0f && candidate.y < regionSize.y) {
            var cellX = Mathf.FloorToInt (candidate.x / cellSize);
            var cellStartX = Mathf.Max (0, cellX - 2);
            var cellEndX = Mathf.Min (cellX + 2, grid.GetLength (0) - 1);

            var cellY = Mathf.FloorToInt (candidate.y / cellSize);
            var cellStartY = Mathf.Max (0, cellY - 2);
            var cellEndY = Mathf.Min (cellY + 2, grid.GetLength (1) - 1);

            for (int x = cellStartX; x <= cellEndX; x++) {
                for (int y = cellStartY; y <= cellEndY; y++) {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1) {
                        var point = points[pointIndex];
                        var sqrDst = (point - candidate).sqrMagnitude;
                        if (sqrDst < radius * radius) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }
}