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

            var message = JsonUtility.FromJson<PathMessage>(data.ToString());
            if (message?.points == null) {
                return;
            }

            foreach (var p in message.points)
            {
                currentPath.Add(p.ToVector3());
            }
        }


        // Unity викликає автоматично
        private void OnDrawGizmos()
        {
            if (currentPath.Count == 0) {
                return;
            }

            // точки
            Gizmos.color = Color.green;
            foreach (var point in currentPath)
            {
                Gizmos.DrawSphere(point, 0.1f);
            }

            // лінії
            Gizmos.color = Color.blue;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
        }
    }

    [System.Serializable]
    public class PathMessage
    {
        public List<PathPoint> points;
    }

    [System.Serializable]
    public class PathPoint
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
