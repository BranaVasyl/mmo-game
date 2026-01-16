using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV {
    public class GizmosManager : Singleton<GizmosManager>
    {
        public bool showPoints = true;
        public bool showLines = true;
        public bool showTriangles = true;
        public bool showPath = true;
        public bool showNavmesh = true;

        private List<Vector3> points = new();
        private List<(Vector3 A, Vector3 B)> lines = new();
        private List<(Vector3 A, Vector3 B, Vector3 C)> triangles = new();

        private List<Vector3> currentPath = new();
        private List<(Vector3 A, Vector3 B)> navmeshLines = new();

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

        public void DrawPath(JSONObject data)
        {
            currentPath.Clear();

            var message = JsonUtility.FromJson<ServerPointsList>(data.ToString());
            if (message?.points == null) {
                return;
            }

            foreach (var p in message.points)
            {
                currentPath.Add(p.ToVector3());
            }
        }

        public void DrawNavmesh(JSONObject data)
        {
            navmeshLines.Clear();

            var message = JsonUtility.FromJson<ServerLinesList>(data.ToString());
            if (message?.lines == null) {
                return;
            }

            foreach (var line in message.lines)
            {
                navmeshLines.Add((line.APos, line.BPos));
            }
        }

        private void OnDrawGizmos()
        {
            if (showNavmesh && navmeshLines.Count != 0) {
                Gizmos.color = Color.cyan;

                foreach (var line in navmeshLines)
                {
                    Gizmos.DrawLine(line.A, line.B);
                }
            }

            if (showPoints && points.Count != 0)
            {
                Gizmos.color = Color.black;
                
                foreach (var point in points)
                {
                    Gizmos.DrawCube(point, Vector3.one * 0.025f);
                }
            }

            if (showTriangles && triangles.Count != 0)
            {
                Gizmos.color = Color.red;

                foreach (var triangle in triangles)
                {
                    Gizmos.DrawLine(triangle.A, triangle.B);
                    Gizmos.DrawLine(triangle.B, triangle.C);
                    Gizmos.DrawLine(triangle.C, triangle.A);
                }
            }

            if (showLines && lines.Count != 0)
            {
                Gizmos.color = Color.yellow;

                foreach (var line in lines)
                {
                    Gizmos.DrawLine(line.A, line.B);
                }
            }

            if (showPath && currentPath.Count != 0) {
                Gizmos.color = Color.red;
                foreach (var point in currentPath)
                {
                    Gizmos.DrawCube(point, Vector3.one * 0.025f);
                }

                Gizmos.color = Color.blue;
                for (int i = 0; i < currentPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
                }
            }
        }
    }

    [System.Serializable]
    public class ServerPointsList
    {
        public List<ServerPoint> points;
    }

    [System.Serializable]
    public class ServerPoint
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }


    [System.Serializable]
    public class ServerLinesList
    {
        public List<ServerLine> lines;
    }

    [System.Serializable]
    public class ServerLine
    {
        public ServerPoint A;
        public ServerPoint B;

        public Vector3 APos => A.ToVector3();
        public Vector3 BPos => B.ToVector3();
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
