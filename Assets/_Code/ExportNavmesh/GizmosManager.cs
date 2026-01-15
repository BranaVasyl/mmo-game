using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV {
    public class GizmosManager : Singleton<GizmosManager>
    {
        private List<Vector3> points = new();
        private List<(Vector3 A, Vector3 B)> lines = new();
        private List<(Vector3 A, Vector3 B, Vector3 C)> triangles = new();

        public void DrawPoints(JSONObject data)
        {
            points.Clear();

            var message = JsonUtility.FromJson<ServerPointsList>(data.ToString());
            if (message?.points == null) {
                return;
            }

            foreach (var p in message.points)
            {
                points.Add(p.ToVector3());
            }
        }

        public void DrawLines(JSONObject data)
        {
            lines.Clear();

            var message = JsonUtility.FromJson<ServerLinesList>(data.ToString());
            if (message?.lines == null) {
                return;
            }

            foreach (var line in message.lines)
            {
                lines.Add((line.APos, line.BPos));
            }
        }

        public void DrawTriangles(JSONObject data)
        {
            triangles.Clear();

            var message = JsonUtility.FromJson<ServerTrianglesList>(data.ToString());
            if (message?.triangles == null) {
                return;
            }

            foreach (var triangle in message.triangles)
            {
                triangles.Add((triangle.APos, triangle.BPos, triangle.CPos));
            }
        }

        private void OnDrawGizmos()
        {
            if (points.Count != 0)
            {
                Gizmos.color = Color.black;
                
                foreach (var point in points)
                {
                    Gizmos.DrawCube(point, Vector3.one * 0.025f);
                }
            }

            if (lines.Count != 0)
            {
                Gizmos.color = Color.yellow;

                foreach (var line in lines)
                {
                    Gizmos.DrawLine(line.A, line.B);
                }
            }

            if (triangles.Count != 0)
            {
                Gizmos.color = Color.red;

                foreach (var triangle in triangles)
                {
                    Gizmos.DrawLine(triangle.A, triangle.B);
                    Gizmos.DrawLine(triangle.B, triangle.C);
                    Gizmos.DrawLine(triangle.C, triangle.A);
                }
            }
        }
    }

    [System.Serializable]
    public class ServerTrianglesList
    {
        public List<ServerTriangle> triangles;
    }

    [System.Serializable]
    public class ServerTriangle
    {
        public ServerPoint A;
        public ServerPoint B;
        public ServerPoint C;

        public Vector3 APos => A.ToVector3();
        public Vector3 BPos => B.ToVector3();
        public Vector3 CPos => C.ToVector3();

    }
}
