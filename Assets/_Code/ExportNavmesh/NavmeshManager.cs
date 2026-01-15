using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV {
    public class NavmeshManager : Singleton<NavmeshManager>
    {
        private List<(Vector3 A, Vector3 B)> navmeshLines = new();

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
            if (navmeshLines.Count == 0) {
                return;
            }

            Gizmos.color = Color.cyan;

            foreach (var line in navmeshLines)
            {
                Gizmos.DrawLine(line.A, line.B);
            }
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
}