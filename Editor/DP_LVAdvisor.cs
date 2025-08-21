#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DemonShop.Editor
{
    public static class DP_LVAdvisor
    {
        private static bool _useSelectionBounds = true;
        private static Vector3 _manualSize = new Vector3(20, 10, 20);
        private static int _targetMaxCells = 200000;
        private static int _bytesPerCell = 64;

        private static float _voxelSize = 1f;
        private static Vector3Int _resXYZ = new Vector3Int(1,1,1);
        private static int _totalCells = 1;
        private static float _memMB = 1f;
        private static string _lastSuggestNote = "";

        private static bool _fromLPG = true, _fromMLP = false;
        private static bool _selOnly = true, _deleteSrc = false;
        private static float _marginPct = 2f;

        public static void DrawGUI()
        {
            DP_PackageDetector.Ensure();
            var hasLV = DP_PackageDetector.HasLV;
            var scanning = DP_PackageDetector.IsScanning;

            EditorGUILayout.HelpBox(DP_Loc.T("lvIntro"), MessageType.None);

            _useSelectionBounds = EditorGUILayout.ToggleLeft(DP_Loc.T("lvUseSel"), _useSelectionBounds);
            using (new EditorGUI.DisabledScope(_useSelectionBounds))
                _manualSize = EditorGUILayout.Vector3Field(DP_Loc.T("lvManual"), _manualSize);

            _targetMaxCells = EditorGUILayout.IntField(DP_Loc.T("lvTargetCells"), Mathf.Max(1000, _targetMaxCells));
            _bytesPerCell   = EditorGUILayout.IntField(DP_Loc.T("lvBytesPerCell"), Mathf.Clamp(_bytesPerCell, 16, 256));

            if (GUILayout.Button(DP_Loc.T("lvSuggest"), GUILayout.Height(22)))
                RunSuggest();

            if (!string.IsNullOrEmpty(_lastSuggestNote))
                EditorGUILayout.HelpBox(_lastSuggestNote, MessageType.Warning);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.FloatField(DP_Loc.T("lvResVoxel"), _voxelSize);
                EditorGUILayout.Vector3IntField(DP_Loc.T("lvResReso"), _resXYZ);
                EditorGUILayout.IntField(DP_Loc.T("lvResCells"), _totalCells);
                EditorGUILayout.FloatField(DP_Loc.T("lvResMem"), _memMB);
            }

            EditorGUILayout.HelpBox(DP_Loc.T("lvNoteLimits"), MessageType.Info);

            GUILayout.Space(4);
            GUILayout.Label(DP_Loc.T("lvConvHeader"), EditorStyles.boldLabel);
            _fromLPG = EditorGUILayout.ToggleLeft(DP_Loc.T("lvFromLPG"), _fromLPG);
            _fromMLP = EditorGUILayout.ToggleLeft(DP_Loc.T("lvFromMLP"), _fromMLP);
            _selOnly = EditorGUILayout.ToggleLeft(DP_Loc.T("lvSelOnly"), _selOnly);
            _marginPct = EditorGUILayout.Slider(DP_Loc.T("lvMargin"), _marginPct, 0, 20);
            _deleteSrc = EditorGUILayout.ToggleLeft(DP_Loc.T("lvDeleteSrc"), _deleteSrc);

            using (new EditorGUI.DisabledScope(!_fromLPG && !_fromMLP))
            {
                if (GUILayout.Button(DP_Loc.T("lvRunConvert"), GUILayout.Height(22)))
                    RunConvert(hasLV);
            }

            GUILayout.Space(4);
            if (!hasLV) EditorGUILayout.HelpBox(DP_Loc.T("lvNeedLV"), MessageType.Info);
            GUILayout.Label($"LV installed: {(hasLV? "Yes":"No")} {(scanning? "(scanning…)" : "")}", EditorStyles.miniLabel);
        }

        public static void DrawIntegrationTail()
        {
            GUILayout.Label($"LV detected: {(DP_PackageDetector.HasLV? "Yes":"No")}", EditorStyles.miniLabel);
        }

        private static void RunSuggest()
        {
            try
            {
                Bounds b;
                if (_useSelectionBounds)
                {
                    // 直接使用你已有的 CollectBounds
                    var trs = Selection.transforms;
                    var bb = DP_Utils.CollectBounds(trs, includeRenderers:true, includeColliders:true);
                    if (!bb.HasValue)
                    {
                        _lastSuggestNote = "⚠ No renderer/collider in selection. Switch to Manual or select objects.";
                        return;
                    }
                    b = bb.Value;
                }
                else
                {
                    b = new Bounds(Vector3.zero, _manualSize);
                }

                var size = b.size;
                var volume = Mathf.Max(0.001f, size.x * size.y * size.z);
                _voxelSize = Mathf.Max(0.1f, Mathf.Pow(volume / Mathf.Max(1000, _targetMaxCells), 1f/3f));

                _resXYZ = new Vector3Int(
                    Mathf.Max(1, Mathf.RoundToInt(size.x / _voxelSize)),
                    Mathf.Max(1, Mathf.RoundToInt(size.y / _voxelSize)),
                    Mathf.Max(1, Mathf.RoundToInt(size.z / _voxelSize))
                );
                _totalCells = _resXYZ.x * _resXYZ.y * _resXYZ.z;
                _memMB = (_totalCells * Mathf.Clamp(_bytesPerCell,16,256)) / (1024f*1024f);
                _lastSuggestNote = "";
            }
            catch (System.SystemException e)
            {
                _lastSuggestNote = "Suggest failed: " + e.Message;
            }
        }

        private static void RunConvert(bool hasLV)
        {
            try
            {
                if (!_fromLPG && !_fromMLP) return;
                var margin = _marginPct / 100f;

                var targets = new List<Component>();
                if (_fromLPG)
                    targets.AddRange(FindTargets("LightProbeGroup"));
                if (_fromMLP)
                    targets.AddRange(FindTargetsContains("MagicLightProbes"));

                if (targets.Count == 0)
                {
                    EditorUtility.DisplayDialog("Convert", "No source (LPG/MLP) found.", DP_Loc.T("ok"));
                    return;
                }

                if (hasLV)
                {
                    var mngTp = DP_Utils.GetTypeByName("LightVolumesManager") ?? DP_Utils.FindTypeContains("LightVolumesManager");
                    if (mngTp != null && Object.FindObjectOfType(mngTp) == null)
                    {
                        var mgr = new GameObject("LightVolumesManager"); Undo.RegisterCreatedObjectUndo(mgr, "LV Manager");
                        mgr.AddComponent(mngTp);
                    }
                }

                Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
                int created = 0, deleted = 0;

                foreach (var c in targets)
                {
                    if (_selOnly && !IsInSelection(c.gameObject)) continue;

                    var b = BoundsFromComponent(c);
                    if (!b.HasValue) continue;
                    var bb = b.Value; bb.Expand(bb.size * margin);

                    var parent = new GameObject("LV_From_" + c.GetType().Name);
                    Undo.RegisterCreatedObjectUndo(parent, "Create LV group");
                    parent.transform.position = bb.center;

                    if (hasLV)
                    {
                        var volTp = DP_Utils.GetTypeByName("LightVolume") ?? DP_Utils.FindTypeContains("LightVolume");
                        if (volTp != null)
                        {
                            var go = new GameObject("LightVolume");
                            Undo.RegisterCreatedObjectUndo(go, "Create LV");
                            go.transform.SetParent(parent.transform, false);
                            go.transform.localScale = bb.size * 0.5f;
                            go.AddComponent(volTp);
                            created++;
                        }
                    }
                    else
                    {
                        var go = new GameObject("LV_Placeholder");
                        Undo.RegisterCreatedObjectUndo(go, "Create Placeholder");
                        go.transform.SetParent(parent.transform, false);
                        go.transform.localScale = bb.size * 0.5f;
                        created++;
                    }

                    if (_deleteSrc)
                        TryDeleteSource(c, ref deleted);
                }

                Undo.CollapseUndoOperations(ug);
                EditorUtility.DisplayDialog("Convert", $"Created: {created}\nDeleted sources: {deleted}", DP_Loc.T("ok"));
            }
            catch (System.SystemException e)
            {
                EditorUtility.DisplayDialog("Convert", "Failed: " + e.Message, DP_Loc.T("ok"));
            }
        }

        private static IEnumerable<Component> FindTargets(string exactType)
        {
            var tp = DP_Utils.GetTypeByName(exactType);
            if (tp == null) yield break;
            foreach (var c in Object.FindObjectsOfType(tp, true)) yield return (Component)c;
        }
        private static IEnumerable<Component> FindTargetsContains(string name)
        {
            var tp = DP_Utils.FindTypeContains(name);
            if (tp == null) yield break;
            foreach (var c in Object.FindObjectsOfType(tp, true)) yield return (Component)c;
        }
        private static bool IsInSelection(GameObject go)
        {
            if (Selection.transforms == null || Selection.transforms.Length == 0) return false;
            foreach (var t in Selection.transforms) if (go.transform.IsChildOf(t)) return true;
            return false;
        }
        private static Bounds? BoundsFromComponent(Component c)
        {
            if (!c) return null;
            var tr = c.transform;
            var rends = tr.GetComponentsInChildren<Renderer>(true);
            if (rends != null && rends.Length > 0)
            {
                var b = rends[0].bounds;
                for (int i=1;i<rends.Length;i++) b.Encapsulate(rends[i].bounds);
                return b;
            }
            var cols = tr.GetComponentsInChildren<Collider>(true);
            if (cols != null && cols.Length > 0)
            {
                var b = cols[0].bounds;
                for (int i=1;i<cols.Length;i++) b.Encapsulate(cols[i].bounds);
                return b;
            }
            return null;
        }
        private static void TryDeleteSource(Component c, ref int counter)
        {
            if (!c) return;
            Undo.DestroyObjectImmediate(c.gameObject);
            counter++;
        }
    }
}
#endif
