using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV {
    public class PathfindingManager : Singleton<PathfindingManager>
    {
        private List<Vector3> currentPath = new();

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


        private void OnDrawGizmos()
        {
            if (currentPath.Count == 0) {
                return;
            }

            Gizmos.color = Color.red;
            foreach (var point in currentPath)
            {
                Gizmos.DrawCube(point, Vector3.one * 0.05f);
            }

            Gizmos.color = Color.blue;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
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
}
