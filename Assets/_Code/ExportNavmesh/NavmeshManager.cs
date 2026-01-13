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

            var message = JsonUtility.FromJson<NavmeshLineMessage>(data.ToString());
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
            if (navmeshLines.Count == 0)
                return;

            Gizmos.color = Color.cyan;

            foreach (var line in navmeshLines)
            {
                Gizmos.DrawLine(line.A, line.B);
            }
        }
    }

    [System.Serializable]
    public class NavmeshLineMessage
    {
        public List<NavmeshLine> lines;
    }

    [System.Serializable]
    public class NavmeshLine
    {
        public NavmeshPoint A;
        public NavmeshPoint B;

        public Vector3 APos => A.ToVector3();
        public Vector3 BPos => B.ToVector3();
    }

    [System.Serializable]
    public class NavmeshPoint
    {
        public string x;
        public string y;
        public string z;

        public Vector3 ToVector3()
        {
            return new Vector3(
                float.Parse(x, System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(y, System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(z, System.Globalization.CultureInfo.InvariantCulture)
            );
        }
    }
}