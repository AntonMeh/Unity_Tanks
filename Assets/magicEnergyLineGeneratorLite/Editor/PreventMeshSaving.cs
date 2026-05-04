using soulercoaster.scripts;
using soulercoasterLite.scripts;
using UnityEditor;
using UnityEngine;

namespace soulercoaster.Editor {
    public class PreventMeshSaving : AssetModificationProcessor {
        private static string[] OnWillSaveAssets(string[] paths) {
            var coasters = Object.FindObjectsByType<SoulerCoaster>(FindObjectsSortMode.None);
            foreach (var coaster in coasters) {
                if (coaster.preventMeshSaving) {
                    coaster.GetComponent<MeshFilter>().mesh = null;
                }
            }

            EditorApplication.update += RefreshScene;

            return paths;
        }

        private static void RefreshScene() {
            EditorApplication.update -= RefreshScene;
            var coasters = Object.FindObjectsByType<SoulerCoaster>(FindObjectsSortMode.None);
            foreach (var coaster in coasters) {
                if (coaster.preventMeshSaving) {
                    coaster.GetComponent<MeshFilter>().mesh = coaster.getMesh();
                }
            }
        }
    }
}