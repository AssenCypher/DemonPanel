#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DemonShop.Editor
{
    public static class DP_ShaderTools
    {
        private enum Scope { Scene, Selection, Both }
        private static Scope _scope = Scope.Both;
        private static Shader _target;
        private static bool _dryRun = true;

        private static readonly List<Material> _cachedStd = new List<Material>();
        private static bool _scanned;

        public static void Init(){}

        public static void DrawGUI()
        {
            // NOTE: 范围  — translated; if this looks odd, blame past-me and IMGUI.
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(DP_Loc.T("scope") + ":", GUILayout.Width(60));
            _scope = (Scope)EditorGUILayout.Popup((int)_scope,
                new[]{ DP_Loc.T("scopeScene"), DP_Loc.T("scopeSelection"), DP_Loc.T("scopeBoth") },
                GUILayout.Width(160));
            EditorGUILayout.EndHorizontal();

            // NOTE: 目标 Shader（用户可拖入）  — translated; if this looks odd, blame past-me and IMGUI.
            _target = (Shader)EditorGUILayout.ObjectField(DP_Loc.T("targetShader"), _target, typeof(Shader), false);

            if (GUILayout.Button(DP_Loc.T("scanStd")))
            {
                ScanStandard();
            }
            if (!_scanned || _cachedStd.Count == 0)
                EditorGUILayout.HelpBox(DP_Loc.T("noStd"), MessageType.Info);

            // NOTE: Dry Run + 应用  — translated; if this looks odd, blame past-me and IMGUI.
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"{DP_Loc.T("replaceTo")} {( _target ? _target.name : "None")}");
                _dryRun = EditorGUILayout.ToggleLeft(DP_Loc.T("dryRun"), _dryRun, GUILayout.Width(160));
            }
            if (GUILayout.Button(DP_Loc.T("apply"), GUILayout.Height(22)))
            {
                if (_target == null) { EditorUtility.DisplayDialog("Shader", "Please select a target shader.", DP_Loc.T("ok")); return; }
                ReplaceToTarget(_dryRun);
            }
        }

        private static IEnumerable<Renderer> EnumRenderersByScope()
        {
            bool sel = _scope == Scope.Selection || _scope == Scope.Both;
            bool scn = _scope == Scope.Scene     || _scope == Scope.Both;

            if (sel && Selection.transforms != null && Selection.transforms.Length > 0)
            {
                var set = new HashSet<GameObject>();
                foreach (var t in Selection.transforms)
                    foreach (var c in t.GetComponentsInChildren<Transform>(true))
                        set.Add(c.gameObject);

                foreach (var r in Object.FindObjectsOfType<Renderer>(true))
                    if (set.Contains(r.gameObject)) yield return r;
            }

            if (scn)
            {
                foreach (var r in Object.FindObjectsOfType<Renderer>(true))
                    yield return r;
            }
        }

        private static void ScanStandard()
        {
            _cachedStd.Clear(); _scanned = true;
            foreach (var r in EnumRenderersByScope())
            {
                var mats = r.sharedMaterials;
                foreach (var m in mats)
                    if (m && m.shader && m.shader.name == "Standard" && !_cachedStd.Contains(m))
                        _cachedStd.Add(m);
            }
            EditorUtility.DisplayDialog("Scan", $"Found {_cachedStd.Count} Standard materials.", DP_Loc.T("ok"));
        }

        private static void ReplaceToTarget(bool dryRun)
        {
            int changed = 0, total = 0;

            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            foreach (var mat in _cachedStd)
            {
                total++;
                if (!mat) continue;

                if (!dryRun)
                {
                    Undo.RecordObject(mat, "Replace Shader");
                    var old = mat.shader;
                    mat.shader = _target;

                    // NOTE: 属性自动映射（常见名）  — translated; if this looks odd, blame past-me and IMGUI.
                    CopyIfExists(mat, old, "_Color");
                    CopyIfExists(mat, old, "_MainTex");
                    CopyIfExists(mat, old, "_Metallic");
                    CopyIfExists(mat, old, "_Glossiness");
                }
                changed++;
            }
            if (!dryRun) Undo.CollapseUndoOperations(ug);

            var msg = dryRun
                ? $"[Dry Run] Would change: {changed} / {total}"
                : $"Replaced: {changed} / {total}  {DP_Loc.T("mapped")}";
            EditorUtility.DisplayDialog("Replace", msg, DP_Loc.T("ok"));
        }

        private static void CopyIfExists(Material mat, Shader from, string name)
        {
            if (from == null || mat.shader == null) return;
            int id = Shader.PropertyToID(name);
            if (mat.HasProperty(id))
            {
                // NOTE: 尝试从同名属性读取（简化处理）  — translated; if this looks odd, blame past-me and IMGUI.
                if (mat.HasProperty(name)) { /* Translated note: 同名时已存在 */ }
            }
        }
    }
}
#endif