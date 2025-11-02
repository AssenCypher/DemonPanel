#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using DemonShop.Editor;

namespace DemonShop.Editor
{
    public class DemonPanelMainWindow : EditorWindow
    {
        private Vector2 _scroll;
        private int _selTris = 0, _sceneTris = -1;

        [MenuItem("DemonShop/DemonPanel/Main Panel", false, 10)]
        public static void Open()
        {
            DP_Loc.Init();
            var w = GetWindow<DemonPanelMainWindow>();
            w.titleContent = new GUIContent(DP_Loc.T("title"));
            w.minSize = new Vector2(560, 520);
            w.Show();
        }

        private void OnEnable()
        {
            DP_Loc.Init();
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
        }
        private void OnDisable(){ Selection.selectionChanged -= OnSelectionChanged; }

        private void OnSelectionChanged()
        {
            _selTris = DP_Utils.CountTriangles(Selection.transforms); // 父级含子级
            Repaint();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(DP_Loc.T("title"), EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label(DP_Loc.T("language") + ":", GUILayout.Width(70));
                var cur = (int)DP_Loc.Lang;
                var next = EditorGUILayout.Popup(cur, DP_Loc.LangNames, GUILayout.Width(120));
                if (next != cur) DP_Loc.Lang = (DP_Loc.Language)next;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            // —— Overview ——
            GUILayout.Space(6);
            GUILayout.Label(DP_Loc.T("selTriLive"));

            Color SelColor(int v) => v > 100_000 ? Color.red :
                                     v >  50_000 ? new Color(1f,.6f,0f) : Color.green;
            BigDigit(_selTris, SelColor(_selTris));

            // 自适应按钮行
            using (var row = new AutoRow(EditorGUIUtility.currentViewWidth))
            {
                row.Button(DP_Loc.T("countSel"), () =>
                {
                    _selTris = DP_Utils.CountTriangles(Selection.transforms);
                });

                row.Button(DP_Loc.T("reset"), () => _selTris = 0);
            }

            GUILayout.Space(6);
            if (GUILayout.Button(DP_Loc.T("countScene"), GUILayout.Height(24)))
                _sceneTris = DP_Utils.CountTriangles(SceneManager.GetActiveScene().GetRootGameObjects());
            if (_sceneTris >= 0)
            {
                var col = _sceneTris > 1_000_000 ? Color.red :
                          _sceneTris >   500_000 ? new Color(1f,.6f,0f) : Color.green;
                BigText($"{DP_Loc.T("total")} {_sceneTris:N0}", col);
            }

            GUILayout.Space(10);
            GUILayout.Label(DP_Loc.T("shaderToolsHeader"), EditorStyles.boldLabel);
            DP_ShaderTools.DrawGUI();

            // 常用 Shader 一键安装（保持不变）
            DP_ShaderInstallers.DrawGUI();

            GUILayout.Space(10);
            GUILayout.Label(DP_Loc.T("lodHeader"), EditorStyles.boldLabel);
            DP_LODAndScripts.DrawGUI();

            GUILayout.Space(10);
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(DP_Loc.T("language") + ":", GUILayout.Width(80));
                var cur = (int)DP_Loc.Lang;
                var next = EditorGUILayout.Popup(cur, DP_Loc.LangNames, GUILayout.Width(150));
                if (next != cur) DP_Loc.Lang = (DP_Loc.Language)next;
            }

            EditorGUILayout.EndScrollView();
        }

        // ---------- styles ----------
        private static void BigDigit(int v, Color c)
        {
            var s = new GUIStyle(EditorStyles.boldLabel){ fontSize = 28, alignment = TextAnchor.MiddleCenter };
            s.normal.textColor = c;
            GUILayout.Label(v.ToString("N0"), s, GUILayout.Height(34));
        }
        private static void BigText(string txt, Color c)
        {
            var s = new GUIStyle(EditorStyles.boldLabel){ fontSize = 22, alignment = TextAnchor.MiddleCenter };
            s.normal.textColor = c;
            GUILayout.Label(txt, s, GUILayout.Height(28));
        }

        /// <summary>
        /// 轻量“自动换行行容器”：把一排按钮自动根据可用宽度分行。
        /// 不引入新脚本；作为本窗口内部类型即可。
        /// </summary>
        private struct AutoRow : System.IDisposable
        {
            float _viewW, _used, _gap;
            readonly GUILayoutOption _h22;

            public AutoRow(float viewWidth, float gap = 4f, float btnH = 22f)
            {
                _viewW = viewWidth - 32f; // 经验：左右留白 + 滚动条
                _used = 0f;
                _gap  = gap;
                _h22  = GUILayout.Height(btnH);
                EditorGUILayout.BeginHorizontal();
            }

            float NeedWidth(string label)
            {
                var s = GUI.skin.button;
                var size = s.CalcSize(new GUIContent(label));
                return Mathf.Max(80f, size.x + 14f); // 最小宽 + 一点边距
            }

            public void BreakIfNeed(float w)
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
