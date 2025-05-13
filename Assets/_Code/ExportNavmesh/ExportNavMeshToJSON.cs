// using System.IO;
// using System.Text;
// using UnityEditor;
// using UnityEditor.SceneManagement;
// using UnityEngine;
// using UnityEngine.AI;

// // Obj exporter component based on: http://wiki.unity3d.com/index.php?title=ObjExporter

// public class ExportNavMeshToJSON : MonoBehaviour
// {

//     [MenuItem("Custom/Export NavMesh to JSON")]
//     static void Export()
//     {
//         NavMeshTriangulation triangulatedNavMesh = NavMesh.CalculateTriangulation();

//         Mesh mesh = new Mesh();
//         mesh.name = "ExportedNavMesh";
//         mesh.vertices = triangulatedNavMesh.vertices;
//         mesh.triangles = triangulatedNavMesh.indices;
//         string filename = Application.dataPath + "/" + Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().name) + " Exported NavMesh.obj";
//         MeshToFile(mesh, filename);
//         print("NavMesh exported as '" + filename + "'");
//         AssetDatabase.Refresh();
//     }

//     static string MeshToString(Mesh mesh)
//     {
//         StringBuilder sb = new StringBuilder();

//         sb.Append("g ").Append(mesh.name).Append("\n");
//         foreach (Vector3 v in mesh.vertices)
//         {
//             sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
//         }
//         sb.Append("\n");
//         for (int material = 0; material < mesh.subMeshCount; material++)
//         {
//             sb.Append("\n");

//             int[] triangles = mesh.GetTriangles(material);
//             for (int i = 0; i < triangles.Length; i += 3)
//             {
//                 sb.Append(string.Format("f {0} {1} {2}\n", triangles[i], triangles[i + 1], triangles[i + 2]));
//             }
//         }
//         return sb.ToString();
//     }

//     static void MeshToFile(Mesh mesh, string filename)
//     {
//         File.WriteAllText(Application.dataPath + "/NavigationGrid/Grid.txt", MeshToString(mesh));
//     }
// }
