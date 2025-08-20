#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace DemonShop.Editor
{
    /// <summary>
    /// Bakery 页签：打开 Render Lightmaps、全局灯光工具（收集/分组/转换）、Area 修复、Static 扫描器。
    /// 全部通过反射做存在性检测；未安装 Bakery / VRC SDK 也不会编译失败。
    /// </summary>
    public static class DP_BakeryTools
    {
        // —— 对外可读状态（被其它面板引用） ——
        public static bool Installed { get { Detect(); return _hasBakery; } }

        // —— 内部缓存 ——
        private static bool _hasBakery;
        private static string _detMsg;

        // —— 全局灯光工具 UI 状态 ——
        private static int _scope = 0; // 0: whole scene, 1: selection
        private static bool _unpackPrefabs = true;
        private static bool _groupUnder = true;
        private static string _groupName = "Bakery_Lights";
        private static bool _ignoreDirectional = true;
        private static bool _disableOriginal = true;

        // —— Static 扫描器缓存与选项 ——
        private static readonly List<GameObject> _likelyStaticNotStatic = new();
        private static readonly List<GameObject> _likelyDynamicStaticOn = new();
        private static bool _setAllFlags = true;

        // 更保守/更激进的可选项
        private static bool _includeRendererOnly = true;   // 无 Collider 的渲染网格也参与扫描
        private static bool _dynAnimator = true;           // Animator 视为动态
        private static bool _dynAudio = true;              // AudioSource 视为动态
        private static bool _dynParticles = false;         // ParticleSystem 视为动态（默认关）
        private static bool _dynReflProbe = false;         // ReflectionProbe 视为动态（默认关）

        static DP_BakeryTools() { Detect(); }

        private static void Detect()
        {
            _hasBakery =
                DP_Utils.GetTypeByName("ftBuildGraphics") != null ||
                DP_Utils.GetTypeByName("ftLightmaps") != null ||
                DP_Utils.GetTypeByName("ftGlobalStorage") != null ||
                DP_Utils.GetTypeByName("ftLight") != null;

            _detMsg = DP_Loc.T("bakDetect") + (_hasBakery ? "Yes" : "No");
        }

        public static void DrawGUI()
        {
            Detect();

            EditorGUILayout.HelpBox(_detMsg, _hasBakery ? MessageType.Info : MessageType.Warning);

            using (new EditorGUI.DisabledScope(!_hasBakery))
            {
                if (GUILayout.Button(DP_Loc.T("bakOpen"), GUILayout.Height(22)))
                    OpenBakeryRenderWindow();

                GUILayout.Space(6);
                GUILayout.Label(DP_Loc.T("bakGlobal"), EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(DP_Loc.T("bakScope"), GUILayout.Width(60));
                    _scope = EditorGUILayout.Popup(_scope, new[] { DP_Loc.T("bakScopeScene"), DP_Loc.T("bakScopeSel") }, GUILayout.Width(180));
                    _unpackPrefabs = EditorGUILayout.ToggleLeft(DP_Loc.T("bakUnpack"), _unpackPrefabs);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _groupUnder = EditorGUILayout.ToggleLeft(DP_Loc.T("bakGroupUnder"), _groupUnder, GUILayout.Width(160));
                    using (new EditorGUI.DisabledScope(!_groupUnder))
                    {
                        GUILayout.Label(DP_Loc.T("bakGroupName"), GUILayout.Width(80));
                        _groupName = EditorGUILayout.TextField(_groupName);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _ignoreDirectional = EditorGUILayout.ToggleLeft(DP_Loc.T("bakIgnoreDir"), _ignoreDirectional);
                    _disableOriginal   = EditorGUILayout.ToggleLeft(DP_Loc.T("bakDisableOrig"), _disableOriginal);
                }

                // —— 这里开始使用“自动换行按钮行” —— //
                using (var row = new AutoRow(EditorGUIUtility.currentViewWidth))
                {
                    row.Button(DP_Loc.T("bakCollectGroup"), CollectAndGroup);
                    row.Button(DP_Loc.T("bakConvertAll"),  ConvertAllToBakery);
                }

                GUILayout.Space(6);
                GUILayout.Label(DP_Loc.T("bakFix"), EditorStyles.boldLabel);

                using (var row = new AutoRow(EditorGUIUtility.currentViewWidth))
                {
                    row.Button(DP_Loc.T("bakFixArea"), () => ConvertUnityAreaToBakeryOnSelection());
                    row.Button(DP_Loc.T("bakRunSel"), () => ConvertUnityAreaToBakeryOnSelection());
                }

                // —— Static 扫描器 ——
                GUILayout.Space(10);
                GUILayout.Label(DP_Loc.T("ssHeader"), EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(DP_Loc.T("ssIntro"), MessageType.None);

                using (var row = new AutoRow(EditorGUIUtility.currentViewWidth))
                {
                    row.Button(DP_Loc.T("ssScan"), RunStaticScan);
                    // 把“设所有Static Flags”放在同一行，空间不足时自动换到下一行
                    row.Button(DP_Loc.T("ssSetFlagsAll"), () => _setAllFlags = !_setAllFlags);
                    GUILayout.FlexibleSpace();
                    // 给切换一个可视提示
                    GUILayout.Label(_setAllFlags ? "(ON)" : "(OFF)", GUILayout.Width(40));
                }

                using (var row = new AutoRow(EditorGUIUtility.currentViewWidth))
                {
                    row.Button(DP_Loc.T("ssIncludeRenderer"), () => _includeRendererOnly = !_includeRendererOnly);
                    row.Button(DP_Loc.T("ssDynAnimator"),     () => _dynAnimator       = !_dynAnimator);
                    row.Button(DP_Loc.T("ssDynAudio"),        () => _dynAudio          = !_dynAudio);
                    row.Button(DP_Loc.T("ssDynParticle"),     () => _dynParticles      = !_dynParticles);
                    row.Button(DP_Loc.T("ssDynReflProbe"),    () => _dynReflProbe      = !_dynReflProbe);
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label($"{DP_Loc.T("ssLikelyStatic")}: {_likelyStaticNotStatic.Count}");
                    using (var row = new AutoRow(EditorGUIUtility.currentViewWidth))
                    {
                        row.Button(DP_Loc.T("ssSelectStatic"), () => SelectList(_likelyStaticNotStatic));
                        row.Button(DP_Loc.T("ssFixStatic"),    () => MarkStatic(_likelyStaticNotStatic, _setAllFlags));
                    }

                    GUILayout.Space(4);
                    GUILayout.Label($"{DP_Loc.T("ssLikelyDynamic")}: {_likelyDynamicStaticOn.Count}");
                    using (var row = new AutoRow(EditorGUIUtility.currentViewWidth))
                    {
                        row.Button(DP_Loc.T("ssSelectDynamic"), () => SelectList(_likelyDynamicStaticOn));
                        row.Button(DP_Loc.T("ssClearDynamic"),  () => ClearStatic(_likelyDynamicStaticOn));
                    }
                }
            }
        }

        // —— 打开正确的 Bakery 窗口：先反射找类型（无日志），找不到再提示 ——
        private static void OpenBakeryRenderWindow()
        {
            string[] typeCandidates =
            {
                "ftRenderLightmaps","ftRenderLightmap","ftBuildGraphics","BakeryWindow","Bakery.RenderLightmapsWindow"
            };
            foreach (var tn in typeCandidates)
            {
                var t = DP_Utils.GetTypeByName(tn);
                if (t != null && typeof(EditorWindow).IsAssignableFrom(t))
                {
                    EditorWindow.GetWindow(t, false, "Bakery", true);
                    return;
                }
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); } catch { continue; }
                foreach (var t in types)
                {
                    if (!typeof(EditorWindow).IsAssignableFrom(t)) continue;
                    string n = t.FullName.ToLowerInvariant();
                    if ((n.Contains("bakery") || n.Contains("ft")) && (n.Contains("render") || n.Contains("lightmap")))
                    {
                        EditorWindow.GetWindow(t, false, "Bakery", true);
                        return;
                    }
                }
            }
            EditorUtility.DisplayDialog("Bakery",
                "Could not locate the 'Render Lightmaps' window via reflection. Please open it from the Bakery menu in your project.",
                DP_Loc.T("ok"));
        }

        // —— 收集并分组灯光 ——
        private static void CollectAndGroup()
        {
            List<Light> lights = GetLightsByScope();
            if (lights.Count == 0)
            {
                EditorUtility.DisplayDialog("Bakery", "No lights found.", DP_Loc.T("ok"));
                return;
            }

            GameObject group = null;
            if (_groupUnder)
            {
                group = GameObject.Find(_groupName) ?? new GameObject(_groupName);
                Undo.RegisterCreatedObjectUndo(group, "Create Bakery Group");
            }

            var processedRoots = new HashSet<GameObject>();
            int moved = 0;

            try
            {
                for (int i = 0; i < lights.Count; i++)
                {
                    var l = lights[i];
                    if (EditorUtility.DisplayCancelableProgressBar("Collect & Group Lights", l.name, (float)i / lights.Count)) break;
                    if (_ignoreDirectional && l.type == LightType.Directional) continue;

                    if (_unpackPrefabs)
                    {
                        var root = PrefabUtility.GetOutermostPrefabInstanceRoot(l.gameObject);
                        if (root != null && !processedRoots.Contains(root))
                        {
                            processedRoots.Add(root);
                            try { PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction); }
                            catch { }
                        }
                    }

                    if (_groupUnder && group != null && l.transform.parent != group.transform)
                    {
                        Undo.SetTransformParent(l.transform, group.transform, "Group Lights");
                        moved++;
                    }
                }
            }
            finally { EditorUtility.ClearProgressBar(); }

            EditorUtility.DisplayDialog("Bakery", $"{DP_Loc.T("bakCollectGroup")} -> {DP_Loc.T("bakDone")}\nMoved: {moved}", DP_Loc.T("ok"));
        }

        // —— 一键转换为 Bakery 光并禁用原灯 ——
        private static void ConvertAllToBakery()
        {
            List<Light> lights = GetLightsByScope();
            if (lights.Count == 0)
            {
                EditorUtility.DisplayDialog("Bakery", "No lights found.", DP_Loc.T("ok"));
                return;
            }

            var tpFtLight = DP_Utils.GetTypeByName("ftLight");
            if (tpFtLight == null)
            {
                EditorUtility.DisplayDialog("Bakery", "Type 'ftLight' not found.", DP_Loc.T("ok"));
                return;
            }

            int converted = 0, skipped = 0;
            Undo.IncrementCurrentGroup(); int g = Undo.GetCurrentGroup();

            try
            {
                for (int i = 0; i < lights.Count; i++)
                {
                    var l = lights[i];
                    if (EditorUtility.DisplayCancelableProgressBar("Convert -> Bakery", l.name, (float)i / lights.Count)) break;
                    if (_ignoreDirectional && l.type == LightType.Directional) { skipped++; continue; }

                    var comp = l.GetComponent(tpFtLight);
                    if (comp == null)
                    {
                        comp = l.gameObject.AddComponent(tpFtLight);
                        Undo.RegisterCreatedObjectUndo((UnityEngine.Object)comp, "Add ftLight");
                    }

                    TrySet(tpFtLight, comp, "color", l.color);
                    TrySet(tpFtLight, comp, "intensity", l.intensity);
                    TrySet(tpFtLight, comp, "samples", 64);
                    TrySet(tpFtLight, comp, "selfShadow", true);

                    int bakerType = l.type switch
                    {
                        LightType.Point       => 0,
                        LightType.Directional => 1,
                        LightType.Spot        => 2,
                        LightType.Area        => 4,
                        _ => 0
                    };
                    TrySet(tpFtLight, comp, "lightType", bakerType);

                    if (l.type == LightType.Spot) { TrySet(tpFtLight, comp, "cutoff", l.spotAngle); }
                    if (l.type == LightType.Area)
                    {
                        var sz = l.areaSize;
                        TrySet(tpFtLight, comp, "sizeX", sz.x);
                        TrySet(tpFtLight, comp, "sizeY", sz.y);
                    }

                    if (_disableOriginal) l.enabled = false;
                    converted++;
                }
            }
            finally
            {
                Undo.CollapseUndoOperations(g);
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("Bakery", $"Converted: {converted}\nSkipped: {skipped}", DP_Loc.T("ok"));
        }

        // —— 仅 Area Light -> Bakery Area（安全修复） ——
        private static void ConvertUnityAreaToBakeryOnSelection()
        {
            var sels = Selection.gameObjects;
            if (sels == null || sels.Length == 0)
            {
                EditorUtility.DisplayDialog("Bakery", "No selection.", DP_Loc.T("ok"));
                return;
            }

            var ftLight = DP_Utils.GetTypeByName("ftLight");
            if (ftLight == null)
            {
                EditorUtility.DisplayDialog("Bakery", "Type 'ftLight' not found.", DP_Loc.T("ok"));
                return;
            }

            int converted = 0;
            Undo.IncrementCurrentGroup(); int g = Undo.GetCurrentGroup();

            try
            {
                var lights = new List<Light>();
                foreach (var go in sels) lights.AddRange(go.GetComponentsInChildren<Light>(true));

                for (int i = 0; i < lights.Count; i++)
                {
                    var l = lights[i];
                    if (EditorUtility.DisplayCancelableProgressBar("Convert Area -> Bakery", l.name, (float)i / lights.Count)) break;
                    if (l.type != LightType.Area) continue;

                    var comp = l.GetComponent(ftLight);
                    if (comp == null)
                    {
                        comp = l.gameObject.AddComponent(ftLight);
                        Undo.RegisterCreatedObjectUndo((UnityEngine.Object)comp, "Add ftLight");
                    }

                    TrySet(ftLight, comp, "color", l.color);
                    TrySet(ftLight, comp, "intensity", l.intensity);
                    TrySet(ftLight, comp, "samples", 64);
                    TrySet(ftLight, comp, "selfShadow", true);

                    var size = l.areaSize;
                    TrySet(ftLight, comp, "sizeX", size.x);
                    TrySet(ftLight, comp, "sizeY", size.y);
                    TrySet(ftLight, comp, "lightType", 4);

                    if (_disableOriginal) l.enabled = false;
                    converted++;
                }
            }
            finally
            {
                Undo.CollapseUndoOperations(g);
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("Bakery", $"Converted: {converted}", DP_Loc.T("ok"));
        }

        // —— Static 扫描器 ——
        private static void RunStaticScan()
        {
            _likelyStaticNotStatic.Clear();
            _likelyDynamicStaticOn.Clear();

            IEnumerable<GameObject> cand = UnityEngine.Object
                .FindObjectsOfType<Collider>(true)
                .Select(c => c.gameObject);

            if (_includeRendererOnly)
            {
                var renderers = UnityEngine.Object.FindObjectsOfType<MeshRenderer>(true)
                    .Select(r => r.gameObject);
                cand = cand.Concat(renderers);
            }

            var set = new HashSet<GameObject>(cand);

            var tUdon       = DP_Utils.GetTypeByName("VRC.Udon.UdonBehaviour") ?? DP_Utils.GetTypeByName("UdonSharp.UdonSharpBehaviour");
            var tPickup     = DP_Utils.GetTypeByName("VRC.SDK3.Components.VRCPickup");
            var tObjSync    = DP_Utils.GetTypeByName("VRC.SDK3.Components.VRCObjectSync");

            foreach (var go in set)
            {
                if (go == null) continue;

                bool isDynamic = HasDynamicMarkers(go, tUdon, tPickup, tObjSync);
                bool looksSimple = OnlyHasSimpleComponents(go);

                if (isDynamic)
                {
                    if (go.isStatic) _likelyDynamicStaticOn.Add(go);
                }
                else if (looksSimple)
                {
                    if (!go.isStatic) _likelyStaticNotStatic.Add(go);
                }
            }

            EditorUtility.DisplayDialog(
                "Static Scanner",
                $"{DP_Loc.T("ssLikelyStatic")}: {_likelyStaticNotStatic.Count}\n" +
                $"{DP_Loc.T("ssLikelyDynamic")}: {_likelyDynamicStaticOn.Count}",
                DP_Loc.T("ok"));
        }

        private static bool HasDynamicMarkers(GameObject go, Type tUdon, Type tPickup, Type tObjSync)
        {
            if (go.GetComponent<Rigidbody>() != null || go.GetComponent<Rigidbody2D>() != null)
                return true;

            if (tUdon != null && go.GetComponent(tUdon) != null) return true;
            if (tPickup != null && go.GetComponent(tPickup) != null) return true;
            if (tObjSync != null && go.GetComponent(tObjSync) != null) return true;

            if (_dynAnimator && go.GetComponent<Animator>() != null) return true;
            if (_dynAudio && go.GetComponent<AudioSource>() != null) return true;
            if (_dynParticles && go.GetComponent<ParticleSystem>() != null) return true;
            if (_dynReflProbe && go.GetComponent<ReflectionProbe>() != null) return true;

            var monos = go.GetComponents<MonoBehaviour>();
            foreach (var m in monos)
            {
                if (m == null) return true; // Missing 脚本 -> 也视作动态
                var asm = m.GetType().Assembly.GetName().Name;
                if (!asm.StartsWith("UnityEngine", StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static bool OnlyHasSimpleComponents(GameObject go)
        {
            var comps = go.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null) return false; // Missing 脚本 -> 非简单
                var t = c.GetType();

                if (t == typeof(Transform) || t == typeof(MeshFilter) || t == typeof(LODGroup))
                    continue;

                if (t == typeof(MeshRenderer) ||
                    t == typeof(BoxCollider) || t == typeof(MeshCollider) ||
                    t == typeof(CapsuleCollider) || t == typeof(SphereCollider))
                    continue;

                return false;
            }
            return true;
        }

        private static void SelectList(List<GameObject> list)
        {
            Selection.objects = list.Where(o => o != null).Cast<UnityEngine.Object>().ToArray();
        }

        private static void MarkStatic(List<GameObject> list, bool allFlags)
        {
            if (list == null || list.Count == 0) return;

            Undo.IncrementCurrentGroup(); int g = Undo.GetCurrentGroup();
            foreach (var go in list)
            {
                if (go == null) continue;

                var flags = allFlags
                    ? (StaticEditorFlags)~0
                    : (StaticEditorFlags.BatchingStatic |
                       StaticEditorFlags.ContributeGI |
                       StaticEditorFlags.ReflectionProbeStatic |
                       StaticEditorFlags.OccluderStatic |
                       StaticEditorFlags.OccludeeStatic);

                GameObjectUtility.SetStaticEditorFlags(go, flags);
                EditorUtility.SetDirty(go);
            }
            Undo.CollapseUndoOperations(g);
        }

        private static void ClearStatic(List<GameObject> list)
        {
            if (list == null || list.Count == 0) return;

            Undo.IncrementCurrentGroup(); int g = Undo.GetCurrentGroup();
            foreach (var go in list)
            {
                if (go == null) continue;
                GameObjectUtility.SetStaticEditorFlags(go, 0);
                EditorUtility.SetDirty(go);
            }
            Undo.CollapseUndoOperations(g);
        }

        private static List<Light> GetLightsByScope()
        {
            var list = new List<Light>();
            if (_scope == 0) list.AddRange(UnityEngine.Object.FindObjectsOfType<Light>(true));
            else
            {
                var sels = Selection.gameObjects;
                if (sels != null)
                    foreach (var go in sels) list.AddRange(go.GetComponentsInChildren<Light>(true));
            }
            return list;
        }

        private static void TrySet(Type t, object inst, string fieldOrProp, object value)
        {
            try
            {
                var f = t.GetField(fieldOrProp, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null) { f.SetValue(inst, value); return; }
                var p = t.GetProperty(fieldOrProp, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null && p.CanWrite) { p.SetValue(inst, value, null); return; }
            }
            catch { /* 单项失败忽略 */ }
        }

        // —— 轻量“自动换行行容器”（仅在本文件内部使用）——
        private struct AutoRow : System.IDisposable
        {
            float _viewW, _used, _gap;
            readonly GUILayoutOption _h22;
            public AutoRow(float viewWidth, float gap = 4f, float btnH = 22f)
            {
                _viewW = viewWidth - 32f;
                _used = 0f;
                _gap  = gap;
                _h22  = GUILayout.Height(btnH);
                EditorGUILayout.BeginHorizontal();
            }
            float NeedWidth(string label)
            {
                var s = GUI.skin.button;
                var size = s.CalcSize(new GUIContent(label));
                return Mathf.Max(100f, size.x + 16f);
            }
            void BreakIfNeed(float w)
            {
                if (_used > 0f && _used + w > _viewW)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    _used = 0f;
                }
            }
            public bool Button(string label, System.Action onClick)
            {
                float need = NeedWidth(label);
                BreakIfNeed(need);
                bool hit = GUILayout.Button(label, _h22, GUILayout.MinWidth(need));
                _used += need + _gap;
                if (hit) onClick?.Invoke();
                return hit;
            }
            public void Dispose() { EditorGUILayout.EndHorizontal(); }
        }
    }
}
#endif
