#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace DemonShop.Editor
{
    public static class DP_ProbeTools
    {
        private static float _expand = 0.02f;
        private static bool _makeLPG = true, _makeReflection = true, _makeMLP = false, _makeLV = false;
        private static float _grid = 2.0f;

        private static bool _makeAreaTrigger = false;
        private static bool _attachUdonToggle = false;
        private static bool _targetsFromSelectedParent = true;
        private static int  _toggleMode = 0;

        private static bool _hasMLP, _hasLV, _hasLVManager, _hasUdonSharp;
        private static double _nextCopyTime;

        // NOTE: --- 小工具：首行提示 + 刷新按钮，窄时自动“换到下一行” ---  — translated; if this looks odd, blame past-me and IMGUI.
        private static void DrawIntroWithRefresh()
        {
            const float kButtonWidth = 80f;
            float width = EditorGUIUtility.currentViewWidth; // NOTE: 当前可用宽度  — translated; if this looks odd, blame past-me and IMGUI.

            // NOTE: 估个阈值：够宽时并排，不够宽时分两行  — translated; if this looks odd, blame past-me and IMGUI.
            bool narrow = width < 520f;

            if (!narrow)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.HelpBox(DP_Loc.T("probeIntro"), MessageType.None);
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(DP_PackageDetector.IsScanning))
                    {
                        if (GUILayout.Button(DP_Loc.T("refresh"), GUILayout.Width(kButtonWidth)))
                            DP_PackageDetector.RefreshNow();
                    }
                }
            }
            else
            {
                // NOTE: 第一行：提示  — translated; if this looks odd, blame past-me and IMGUI.
                EditorGUILayout.HelpBox(DP_Loc.T("probeIntro"), MessageType.None);
                // NOTE: 第二行：刷新按钮  — translated; if this looks odd, blame past-me and IMGUI.
                using (new EditorGUI.DisabledScope(DP_PackageDetector.IsScanning))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(DP_Loc.T("refresh"), GUILayout.Width(kButtonWidth)))
                            DP_PackageDetector.RefreshNow();
                    }
                }
            }
        }

        public static void DrawGUI()
        {
            DP_PackageDetector.Ensure();

            if (EditorApplication.timeSinceStartup > _nextCopyTime)
            {
                _nextCopyTime = EditorApplication.timeSinceStartup + 0.5f;
                _hasMLP       = DP_PackageDetector.HasMLP;
                _hasLV        = DP_PackageDetector.HasLV;
                _hasLVManager = DP_PackageDetector.HasLVManager;
                _hasUdonSharp = DP_PackageDetector.HasUdonSharp;
            }

            // NOTE: 顶部介绍 + 刷新（支持窄屏换行）  — translated; if this looks odd, blame past-me and IMGUI.
            DrawIntroWithRefresh();

            EditorGUILayout.LabelField(
                $"{DP_Loc.T("stateMLP")}{(_hasMLP? "Yes":"No")}   {DP_Loc.T("stateLV")}{(_hasLV? "Yes":"No")}   {DP_Loc.T("stateUdon")}{(_hasUdonSharp? "Yes":"No")}" +
                $"{(DP_PackageDetector.IsScanning ? "   (scanning…)" : "")}",
                EditorStyles.miniLabel);

            _expand = EditorGUILayout.Slider(DP_Loc.T("boundsExpand"), _expand*100f, 0, 20)/100f;
            _grid   = Mathf.Max(0.25f, EditorGUILayout.FloatField(DP_Loc.T("gridStep"), _grid));

            _makeLPG        = EditorGUILayout.ToggleLeft(DP_Loc.T("createLPG"), _makeLPG);
            _makeReflection = EditorGUILayout.ToggleLeft(DP_Loc.T("createRefProbe"), _makeReflection);

            using (new EditorGUI.DisabledScope(!_hasMLP))
                _makeMLP = EditorGUILayout.ToggleLeft($"{DP_Loc.T("addMLP")} {(_hasMLP? "(detected)":"(not installed)")}", _makeMLP);
            using (new EditorGUI.DisabledScope(!_hasLV))
                _makeLV  = EditorGUILayout.ToggleLeft($"{DP_Loc.T("addLV")} {(_hasLV? "(detected)":"(installable)")}", _makeLV);

            _makeAreaTrigger   = EditorGUILayout.ToggleLeft(DP_Loc.T("createArea"), _makeAreaTrigger);
            using (new EditorGUI.DisabledScope(!_hasUdonSharp))
            {
                _attachUdonToggle = EditorGUILayout.ToggleLeft($"{DP_Loc.T("attachUdon")} {(_hasUdonSharp? "(UdonSharp detected)":"(install UdonSharp to enable)")}", _attachUdonToggle);
                if (_attachUdonToggle)
                {
                    _targetsFromSelectedParent = EditorGUILayout.ToggleLeft(DP_Loc.T("targetsChildren"), _targetsFromSelectedParent);
                    _toggleMode = EditorGUILayout.Popup(DP_Loc.T("toggleMode"), _toggleMode,
                        new[]{"SetActive(GameObject)","Renderers.enabled","Light.enabled","AudioSource.enabled","ParticleSystem.Play/Stop"});
                }
            }

            if (GUILayout.Button(DP_Loc.T("generate"), GUILayout.Height(26)))
                GenerateForSelection();

            // NOTE: —— 按你的要求：从 Probes 页移除 VRCLV 安装入口（保持页面纯净）——  — translated; if this looks odd, blame past-me and IMGUI.
            // NOTE: （原来这里有一个 using (new EditorGUI.DisabledScope(_hasLV)) { Install LV } 的块，已删除）  — translated; if this looks odd, blame past-me and IMGUI.
        }

        public static void DrawIntegrationBlock()
        {
            GUILayout.Label(DP_Loc.T("detected"), EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"• Bakery: {(DP_BakeryTools.Installed? "Yes":"No")}");
            EditorGUILayout.LabelField($"• Magic Light Probes: {(_hasMLP? "Yes":"No")}");
            EditorGUILayout.LabelField($"• VRCLightVolumes: {(_hasLV? "Yes":"No")}");
            if (_hasLV) EditorGUILayout.LabelField($"  (Manager present: {(_hasLVManager? "Yes":"No")})");
            EditorGUILayout.LabelField($"• UdonSharp: {(_hasUdonSharp? "Yes":"No")}");
        }

        private static void GenerateForSelection()
        {
            var trs = Selection.transforms;
            if (trs == null || trs.Length == 0){ EditorUtility.DisplayDialog("No Selection","Select one or more objects.", DP_Loc.T("ok")); return; }

            var b = DP_Utils.CollectBounds(trs, includeRenderers:true, includeColliders:true);
            if (!b.HasValue){ EditorUtility.DisplayDialog("No Bounds","No renderer/collider found in selection.", DP_Loc.T("ok")); return; }
            var bounds = b.Value; bounds.Expand(bounds.size * _expand);

            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            var root = new GameObject("__DemonPanel_Probes"); Undo.RegisterCreatedObjectUndo(root,"Create Probe Root");
            root.transform.position = bounds.center;

            if (_makeReflection) CreateReflectionProbe(root.transform, bounds);
            if (_makeLPG)        CreateLightProbeGroup(root.transform, bounds, _grid);
            if (_makeMLP && _hasMLP) TryCreateMLPVolume(root.transform, bounds);
            if (_makeLV  && _hasLV)  TryCreateLightVolume(root.transform, bounds);

            if (_makeAreaTrigger)
            {
                var area = new GameObject("Auto Area Trigger");
                Undo.RegisterCreatedObjectUndo(area,"Create Area Trigger");
                area.transform.SetParent(root.transform, false);
                area.transform.localPosition = Vector3.zero;

                var bc = area.AddComponent<BoxCollider>();
                bc.isTrigger = true; bc.center = Vector3.zero; bc.size = bounds.size;

                var rb = area.AddComponent<Rigidbody>();
                rb.isKinematic = true; rb.useGravity = false;

                if (_attachUdonToggle && _hasUdonSharp)
                    TryAttachUdonToggle(area, trs, useParent: _targetsFromSelectedParent, toggleMode: _toggleMode);
            }

            Undo.CollapseUndoOperations(ug);
            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }

        private static void CreateReflectionProbe(Transform parent, Bounds b)
        {
            var go = new GameObject("Auto Reflection Probe"); Undo.RegisterCreatedObjectUndo(go,"ReflectionProbe");
            go.transform.SetParent(parent,false);
            var rp = go.AddComponent<ReflectionProbe>();
            rp.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
            rp.boxProjection = true;
            rp.center = Vector3.zero;
            rp.size = b.size;
        }

        private static void CreateLightProbeGroup(Transform parent, Bounds b, float step)
        {
            var go = new GameObject("Auto LightProbeGroup"); Undo.RegisterCreatedObjectUndo(go,"LightProbeGroup");
            go.transform.SetParent(parent,false);
            var lpg = go.AddComponent<LightProbeGroup>();
            var pts = new List<Vector3>();
            var min = b.min - parent.position; var max = b.max - parent.position;

            for (float x=min.x; x<=max.x; x+=step)
            for (float y=min.y; y<=max.y; y+=step)
            for (float z=min.z; z<=max.z; z+=step)
                pts.Add(new Vector3(x,y,z));
            lpg.probePositions = pts.ToArray();
        }

        private static void TryCreateMLPVolume(Transform parent, Bounds b)
        {
            var tp = DP_Utils.FindTypeContains("MagicLightProbes");
            if (tp == null){ Debug.LogWarning("[DemonPanel] MLP type not found."); return; }
            var go = new GameObject("MLP_Volume"); Undo.RegisterCreatedObjectUndo(go,"MLP Volume");
            go.transform.SetParent(parent,false); go.transform.localPosition = Vector3.zero;
            go.transform.localScale = b.size;
            go.AddComponent(tp);
        }

        private static void TryCreateLightVolume(Transform parent, Bounds b)
        {
            var mngTp = DP_Utils.GetTypeByName("LightVolumesManager") ?? DP_Utils.FindTypeContains("LightVolumesManager");
            if (mngTp != null && GameObject.FindObjectOfType(mngTp) == null)
            {
                var mgr = new GameObject("LightVolumesManager"); Undo.RegisterCreatedObjectUndo(mgr, "LV Manager");
                mgr.AddComponent(mngTp);
            }

            var volTp = DP_Utils.GetTypeByName("LightVolume") ?? DP_Utils.FindTypeContains("LightVolume");
            if (volTp == null){ Debug.LogWarning("[DemonPanel] LightVolume type not found."); return; }

            var go = new GameObject("LightVolume"); Undo.RegisterCreatedObjectUndo(go,"LightVolume");
            go.transform.SetParent(parent,false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = b.size * 0.5f;
            go.AddComponent(volTp);
        }

        private static void TryAttachUdonToggle(GameObject areaTrigger, Transform[] selection, bool useParent, int toggleMode)
        {
            var tp = DP_Utils.GetTypeByName("AreaRuntimeToggle");
            if (tp == null) { Debug.LogWarning("[DemonPanel] AreaRuntimeToggle script not found (Udon/AreaRuntimeToggle.cs)."); return; }
            var comp = areaTrigger.AddComponent(tp);

            var targets = new List<GameObject>();
            if (useParent && selection != null && selection.Length >= 1)
            {
                var root = selection[0];
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!t || t.gameObject == areaTrigger) continue;
                    targets.Add(t.gameObject);
                }
            }
            else
            {
                foreach (var r in Object.FindObjectsOfType<Renderer>(true))
                {
                    if (!r) continue;
                    if (r.transform.IsChildOf(areaTrigger.transform)) continue;
                    targets.Add(r.gameObject);
                }
            }

            DP_Utils.TrySet(comp, "targets", targets.ToArray());
            DP_Utils.TrySet(comp, "startDisabled", true);
            DP_Utils.TrySet(comp, "mode", toggleMode);
            DP_Utils.TrySet(comp, "debounce", 0.1f);
        }
    }
}
#endif