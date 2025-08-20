#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DemonShop.Editor
{
    public static class DP_LODAndScripts
    {
        public static void DrawGUI()
        {
            var root = Selection.activeTransform ? Selection.activeTransform.root : null;
            EditorGUILayout.LabelField($"{DP_Loc.T("selRoot")}: {(root?root.name:"—")}");

            if (GUILayout.Button(DP_Loc.T("rmLodKeepHigh"), GUILayout.Height(22)))
                RemoveLODGroupsKeepHighest(root);

            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(DP_Loc.T("countCols"))) CountColliders(root);
            if (GUILayout.Button(DP_Loc.T("rmAllCols"))) RemoveAllColliders(root);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(DP_Loc.T("addMeshColNon"))) AddMeshColliders(root, false);
            if (GUILayout.Button(DP_Loc.T("addMeshColConv"))) AddMeshColliders(root, true);
            if (GUILayout.Button(DP_Loc.T("addBoxCol"))) AddBoxColliders(root);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(DP_Loc.T("rmAllCS"))) RemoveAllCScripts(root);
            if (GUILayout.Button(DP_Loc.T("rmMissing"))) RemoveMissingScripts(root);
            EditorGUILayout.EndHorizontal();
        }

        private static IEnumerable<Transform> EnumTargets(Transform root)
        {
            if (root) foreach (var t in root.GetComponentsInChildren<Transform>(true)) yield return t;
            else foreach (var t in Object.FindObjectsOfType<Transform>(true)) yield return t;
        }

        private static void RemoveLODGroupsKeepHighest(Transform root)
        {
            int removed=0, kept=0;
            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            foreach (var t in EnumTargets(root))
            {
                var lod = t.GetComponent<LODGroup>(); if (!lod) continue;
                Undo.RecordObject(lod, "Remove LODGroup");
                // NOTE: 保留最高精度：LOD0；若有多个子，则启用 LOD0 并禁用其他  — translated; if this looks odd, blame past-me and IMGUI.
                var lods = lod.GetLODs();
                if (lods != null && lods.Length > 0)
                {
                    foreach (var r in lods[0].renderers) if (r) r.enabled = true;
                    for (int i=1;i<lods.Length;i++)
                        foreach (var r in lods[i].renderers) if (r) if (r) r.enabled = false;
                }
                Object.DestroyImmediate(lod);
                removed++; kept++;
            }
            Undo.CollapseUndoOperations(ug);
            EditorUtility.DisplayDialog("LOD", $"Removed LODGroups: {removed}\nKept highest-poly renderers: {kept}", DP_Loc.T("ok"));
        }

        private static void CountColliders(Transform root)
        {
            int n=0;
            foreach (var t in EnumTargets(root)) if (t.GetComponent<Collider>()) n++;
            EditorUtility.DisplayDialog("Colliders", $"Objects with Collider: {n}", DP_Loc.T("ok"));
        }
        private static void RemoveAllColliders(Transform root)
        {
            int n=0;
            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            foreach (var t in EnumTargets(root))
            {
                foreach (var c in t.GetComponents<Collider>()){ Undo.DestroyObjectImmediate(c); n++; }
            }
            Undo.CollapseUndoOperations(ug);
            EditorUtility.DisplayDialog("Colliders", $"Removed Colliders: {n}", DP_Loc.T("ok"));
        }
        private static void AddMeshColliders(Transform root, bool convex)
        {
            int n=0;
            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            foreach (var t in EnumTargets(root))
            {
                var r = t.GetComponent<MeshRenderer>();
                var f = t.GetComponent<MeshFilter>();
                if (r && f && f.sharedMesh && !t.GetComponent<Collider>())
                {
                    var mc = Undo.AddComponent<MeshCollider>(t.gameObject);
                    mc.sharedMesh = f.sharedMesh; mc.convex = convex;
                    n++;
                }
            }
            Undo.CollapseUndoOperations(ug);
            EditorUtility.DisplayDialog("Colliders", $"Added MeshCollider ({(convex?"convex":"non-convex")}): {n}", DP_Loc.T("ok"));
        }
        private static void AddBoxColliders(Transform root)
        {
            int n=0;
            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            foreach (var t in EnumTargets(root))
            {
                var r = t.GetComponent<Renderer>();
                if (r && !t.GetComponent<Collider>())
                {
                    var bc = Undo.AddComponent<BoxCollider>(t.gameObject);
                    bc.center = r.bounds.center - t.position;
                    bc.size   = r.bounds.size;
                    n++;
                }
            }
            Undo.CollapseUndoOperations(ug);
            EditorUtility.DisplayDialog("Colliders", $"Added BoxCollider: {n}", DP_Loc.T("ok"));
        }

        private static void RemoveAllCScripts(Transform root)
        {
            int n=0;
            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            foreach (var t in EnumTargets(root))
            {
                var mbs = t.GetComponents<MonoBehaviour>();
                foreach (var mb in mbs){ if (!mb) continue; Undo.DestroyObjectImmediate(mb); n++; }
            }
            Undo.CollapseUndoOperations(ug);
            EditorUtility.DisplayDialog("Scripts", $"Removed MonoBehaviours: {n}", DP_Loc.T("ok"));
        }
        private static void RemoveMissingScripts(Transform root)
        {
            int n=0;
            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            foreach (var t in EnumTargets(root))
            {
                n += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            }
            Undo.CollapseUndoOperations(ug);
            EditorUtility.DisplayDialog("Scripts", $"Removed Missing Scripts: {n}", DP_Loc.T("ok"));
        }
    }
}
#endif