#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DemonShop.Editor
{
    public static class DP_LVOptimizer
    {
        private static float _mergeDistance = 0.5f;
        private static int _budgetMax = 32;
        private static string _lastMsg = "";

        public static void DrawGUI()
        {
            DP_PackageDetector.Ensure();
            var hasLV = DP_PackageDetector.HasLV;

            EditorGUILayout.HelpBox(DP_Loc.T("lvOptIntro"), MessageType.None);
            _mergeDistance = EditorGUILayout.Slider(DP_Loc.T("lvMergeDist"), _mergeDistance, 0.1f, 5f);
            _budgetMax     = EditorGUILayout.IntSlider(DP_Loc.T("lvBudget"), _budgetMax, 4, 128);

            using (new EditorGUI.DisabledScope(!hasLV))
            {
                if (GUILayout.Button(DP_Loc.T("lvAnalyzeMerge"), GUILayout.Height(22)))
                    RunMerge(hasLV, _mergeDistance);
                if (GUILayout.Button(DP_Loc.T("lvTrim"), GUILayout.Height(22)))
                    RunBudget(hasLV, _budgetMax);
            }

            if (!string.IsNullOrEmpty(_lastMsg))
                EditorGUILayout.HelpBox(_lastMsg, MessageType.Info);

            GUILayout.Label($"LV installed: {(hasLV? "Yes":"No")}", EditorStyles.miniLabel);
        }

        private static void RunMerge(bool hasLV, float dist)
        {
            _lastMsg = "";
            if (!hasLV){ _lastMsg = "LV not installed."; return; }

            var volTp = DP_Utils.GetTypeByName("LightVolume") ?? DP_Utils.FindTypeContains("LightVolume");
            if (volTp == null){ _lastMsg = "Type LightVolume not found."; return; }

            UnityEngine.Object[] vols = Object.FindObjectsOfType(volTp, true);
            if (vols == null || vols.Length == 0){ _lastMsg = "No LightVolume found."; return; }

            int merged = 0;
            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();

            var list = new List<Transform>();
            foreach (UnityEngine.Object o in vols)
            {
                var comp = o as Component;
                if (comp != null) list.Add(comp.transform);
            }

            for (int i=0;i<list.Count;i++)
            for (int j=i+1;j<list.Count;j++)
            {
                var a = list[i]; var b = list[j];
                if (!a || !b) continue;
                var pa = a.parent; var pb = b.parent;
                if (!pa || !pb) continue;
                if (Vector3.Distance(a.position, b.position) <= dist)
                {
                    var root = new GameObject("LV_Merged");
                    Undo.RegisterCreatedObjectUndo(root,"Merge LV");
                    var center = (a.position + b.position) * 0.5f;
                    root.transform.position = center;
                    pa.SetParent(root.transform, true);
                    pb.SetParent(root.transform, true);
                    merged++;
                }
            }

            Undo.CollapseUndoOperations(ug);
            _lastMsg = $"Merged groups: {merged}";
        }

        private static void RunBudget(bool hasLV, int budget)
        {
            _lastMsg = "";
            if (!hasLV){ _lastMsg = "LV not installed."; return; }

            var volTp = DP_Utils.GetTypeByName("LightVolume") ?? DP_Utils.FindTypeContains("LightVolume");
            if (volTp == null){ _lastMsg = "Type LightVolume not found."; return; }

            UnityEngine.Object[] vols = Object.FindObjectsOfType(volTp, true);
            if (vols == null || vols.Length == 0){ _lastMsg = "No LightVolume found."; return; }

            var arr = new List<Component>(vols.Length);
            foreach (UnityEngine.Object o in vols)
            {
                var c = o as Component;
                if (c != null) arr.Add(c);
            }

            arr.Sort((a,b)=>{
                float da = (a ? a.transform.position.sqrMagnitude : float.MaxValue);
                float db = (b ? b.transform.position.sqrMagnitude : float.MaxValue);
                return da.CompareTo(db);
            });

            int trimmed = 0;
            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            for (int i=budget; i<arr.Count; i++)
            {
                if (!arr[i]) continue;
                Undo.DestroyObjectImmediate(arr[i].gameObject);
                trimmed++;
            }
            Undo.CollapseUndoOperations(ug);

            _lastMsg = $"Kept {Mathf.Min(budget, arr.Count)} / Trimmed {trimmed}.";
        }
    }
}
#endif
