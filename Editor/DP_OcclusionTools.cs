#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DemonShop.Editor
{
    public static class DP_OcclusionTools
    {
        private static float _expandPct = 2f;
        private static float _occluder = 1.0f;
        private static float _hole = 0.25f;
        private static int _backface = 100;
        private static bool _selOnly = false;

        public static void DrawGUI()
        {
            EditorGUILayout.HelpBox(DP_Loc.T("occIntro"), MessageType.None);

            if (GUILayout.Button(DP_Loc.T("analyzeSuggest")))
            {
                AnalyzeAndSuggest();
                EditorUtility.DisplayDialog("Suggested",
                    $"{DP_Loc.T("occOcc")} ~{_occluder:0.###} m\n{DP_Loc.T("occHole")} ~{_hole:0.###} m\n{DP_Loc.T("occBack")} ~{_backface}%",
                    DP_Loc.T("ok"));
            }

            GUILayout.Label("Suggested", EditorStyles.boldLabel);
            _occluder  = EditorGUILayout.Slider(DP_Loc.T("occOcc"),  _occluder, 0.05f, 10f);
            _hole      = EditorGUILayout.Slider(DP_Loc.T("occHole"), _hole,     0.02f, 2f);
            _backface  = EditorGUILayout.IntSlider(DP_Loc.T("occBack"), _backface, 0, 100);

            GUILayout.Space(4);
            GUILayout.Label("Static Flags", EditorStyles.boldLabel);
            _selOnly = EditorGUILayout.ToggleLeft(DP_Loc.T("selOnly"), _selOnly);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(DP_Loc.T("markOcc"))) SetStaticFlags(StaticEditorFlags.OccluderStatic, true);
            if (GUILayout.Button(DP_Loc.T("markOcee"))) SetStaticFlags(StaticEditorFlags.OccludeeStatic, true);
            if (GUILayout.Button(DP_Loc.T("clearOcc")))
            {
                SetStaticFlags(StaticEditorFlags.OccluderStatic, false);
                SetStaticFlags(StaticEditorFlags.OccludeeStatic, false);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label("Occlusion Areas", EditorStyles.boldLabel);
            _expandPct = EditorGUILayout.Slider(DP_Loc.T("boundsExpandPct"), _expandPct, 0, 20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(DP_Loc.T("createFromSel"))) CreateAreasFromSelection();
            if (GUILayout.Button(DP_Loc.T("clearAllAreas"))) ClearAllAreas();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label("Bake", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(DP_Loc.T("openOccWin"))) OpenOcclusionWindow();
            if (GUILayout.Button(DP_Loc.T("bakeBg"))) TryBakeInBackground();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(DP_Loc.T("tipOcc"), MessageType.Info);
        }

        private static void AnalyzeAndSuggest()
        {
            var targets = new List<Bounds>();
            IEnumerable<Renderer> rends;
            IEnumerable<Collider> cols;

            if (_selOnly && Selection.transforms != null && Selection.transforms.Length > 0)
            {
                var set = new HashSet<GameObject>(Selection.transforms.SelectMany(t => t.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject));
                rends = UnityEngine.Object.FindObjectsOfType<Renderer>(true).Where(r => set.Contains(r.gameObject));
                cols  = UnityEngine.Object.FindObjectsOfType<Collider>(true).Where(c => set.Contains(c.gameObject));
            }
            else
            {
                rends = UnityEngine.Object.FindObjectsOfType<Renderer>(true);
                cols  = UnityEngine.Object.FindObjectsOfType<Collider>(true);
            }

            foreach (var r in rends){ if (!r || !r.gameObject) continue; if (!IsStatic(r.gameObject)) continue; targets.Add(r.bounds); }
            foreach (var c in cols){ if (!c || !c.gameObject) continue; if (!IsStatic(c.gameObject)) continue; targets.Add(c.bounds); }
            if (targets.Count == 0){ _occluder = 1f; _hole = 0.25f; _backface = 100; return; }

            var minDims = new List<float>(); var majorDims = new List<float>();
            foreach (var b in targets)
            {
                var s = b.size; var mins = new[]{ s.x, s.y, s.z }.OrderBy(v=>v).ToArray();
                minDims.Add(Mathf.Max(0.01f, (float)mins[0]));
                majorDims.Add(Mathf.Max(mins[1], mins[2]));
            }
            float Q(IList<float> a, float p){ if (a.Count==0) return 0f; var arr=a.OrderBy(x=>x).ToArray(); float idx=Mathf.Clamp01(p)*(arr.Length-1); int i0=Mathf.FloorToInt(idx); int i1=Mathf.Min(arr.Length-1,i0+1); float t=idx-i0; return Mathf.Lerp(arr[i0],arr[i1],t); }
            var thickQ = Q(minDims, 0.75f); var holeQ = Q(majorDims, 0.10f);
            _occluder = Mathf.Clamp(thickQ, 0.1f, 5f);
            _hole     = Mathf.Clamp(holeQ * 0.5f, 0.05f, 1.0f);
            _backface = 100;
        }

        private static bool IsStatic(GameObject go)
        {
            var flags = GameObjectUtility.GetStaticEditorFlags(go);
            return (flags & (StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.ContributeGI)) != 0;
        }

        private static void SetStaticFlags(StaticEditorFlags flag, bool on)
        {
            var targets = new List<GameObject>();
            if (_selOnly && Selection.transforms != null && Selection.transforms.Length > 0)
            {
                foreach (var t in Selection.transforms)
                    targets.AddRange(t.GetComponentsInChildren<Transform>(true).Select(x=>x.gameObject));
            }
            else targets.AddRange(UnityEngine.Object.FindObjectsOfType<Transform>(true).Select(x=>x.gameObject));
            targets = targets.Distinct().ToList();

            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            foreach (var go in targets)
            {
                var flags = GameObjectUtility.GetStaticEditorFlags(go);
                if (on) flags |= flag; else flags &= ~flag;
                GameObjectUtility.SetStaticEditorFlags(go, flags);
            }
            Undo.CollapseUndoOperations(ug);
            EditorUtility.DisplayDialog("Static Flags", $"{(on?"Set":"Cleared")} {flag} for {targets.Count} objects.", DP_Loc.T("ok"));
        }

        private static void CreateAreasFromSelection()
        {
            if (Selection.transforms == null || Selection.transforms.Length == 0)
            { EditorUtility.DisplayDialog("No Selection", "Select one or more parents/objects.", DP_Loc.T("ok")); return; }
            var b = DP_Utils.CollectBounds(Selection.transforms, true, true);
            if (!b.HasValue)
            { EditorUtility.DisplayDialog("No Bounds", "Selected objects have no renderer/collider bounds.", DP_Loc.T("ok")); return; }
            var bounds = b.Value; bounds.Expand(bounds.size * (_expandPct/100f));

            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            var root = new GameObject("__OcclusionAreas"); Undo.RegisterCreatedObjectUndo(root, "Create OcclusionAreas Root");
            root.transform.position = bounds.center;

            var go = new GameObject("OcclusionArea"); Undo.RegisterCreatedObjectUndo(go, "Create OcclusionArea");
            go.transform.SetParent(root.transform, false);
            var oa = go.AddComponent<OcclusionArea>();
            oa.center = Vector3.zero; oa.size = bounds.size;

            Undo.CollapseUndoOperations(ug);
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }

        private static void ClearAllAreas()
        {
            var areas = UnityEngine.Object.FindObjectsOfType<OcclusionArea>(true);
            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            int n=0; foreach (var a in areas){ if (a) Undo.DestroyObjectImmediate(a.gameObject); n++; }
            Undo.CollapseUndoOperations(ug);
            EditorUtility.DisplayDialog("Clear Areas", $"Removed: {n}", DP_Loc.T("ok"));
        }

        private static void OpenOcclusionWindow()
        {
            var t = Type.GetType("UnityEditor.OcclusionCullingWindow,UnityEditor");
            if (t != null){ EditorWindow.GetWindow(t, true, "Occlusion Culling").Show(); }
            else EditorUtility.DisplayDialog("Info","Cannot find Occlusion window type. Open via Window > Rendering > Occlusion Culling.", DP_Loc.T("ok"));
        }

        private static void TryBakeInBackground()
        {
            var asm = typeof(UnityEditor.Editor).Assembly;
            var t = asm.GetType("UnityEditor.SceneManagement.StaticOcclusionCulling");
            if (t == null){ EditorUtility.DisplayDialog("Not Supported","API not found. Please bake from Occlusion window.", DP_Loc.T("ok")); return; }
            var m = t.GetMethod("GenerateInBackground", BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
            if (m == null){ EditorUtility.DisplayDialog("Not Supported","GenerateInBackground API not available in this Unity. Please bake manually.", DP_Loc.T("ok")); return; }

            try{ m.Invoke(null, null); EditorUtility.DisplayDialog("Baking", "Occlusion baking started in background.\nCheck progress in Occlusion window.", DP_Loc.T("ok")); }
            catch(Exception e){ Debug.LogWarning(e); EditorUtility.DisplayDialog("Failed", "Start bake failed. Please open Occlusion window and bake manually.", DP_Loc.T("ok")); }
        }
    }
}
#endif