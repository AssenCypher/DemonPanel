#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using DemonShop.Editor;

namespace DemonShop.Editor
{
    public class DemonPanelLightWindow : EditorWindow
    {
        private Vector2 _scroll;
        private int _tab = 0;
        private static GUIContent[] _tabs;

        [MenuItem("DemonShop/DemonPanel/Light Settings", false, 11)]
        public static void Open()
        {
            DP_Loc.Init();
            DP_PackageDetector.Ensure();
            var w = GetWindow<DemonPanelLightWindow>();
            w.titleContent = new GUIContent("Light Settings");
            w.minSize = new Vector2(700, 540);
            w.Show();
        }

        private void OnEnable() => BuildTabs();

        private void BuildTabs()
        {
            _tabs = new[]
            {
                new GUIContent("Probes"),
                new GUIContent(DP_Loc.T("tabVRCLV")),
                new GUIContent(DP_Loc.T("tabBakery")),
                new GUIContent(DP_Loc.T("tabOcclusion")),
                new GUIContent(DP_Loc.T("tabIntegration")),
            };
        }

        private void OnGUI()
        {
            // NOTE: 顶栏：标题 + 语言  — translated; if this looks odd, blame past-me and IMGUI.
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Light Settings", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label(DP_Loc.T("language") + ":", GUILayout.Width(70));
                var cur = (int)DP_Loc.Lang;
                var next = EditorGUILayout.Popup(cur, DP_Loc.LangNames, GUILayout.Width(120));
                if (next != cur) { DP_Loc.Lang = (DP_Loc.Language)next; BuildTabs(); }
            }

            // NOTE: Tab 工具条（可自动分行）  — translated; if this looks odd, blame past-me and IMGUI.
            DrawToolbarWrapped(position.width - 8);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            switch (_tab)
            {
                case 0:
                    GUILayout.Space(6);
                    GUILayout.Label(DP_Loc.T("probeHeader"), EditorStyles.boldLabel);
                    DP_ProbeTools.DrawGUI();
                    break;

                case 1:
                    DP_VRCLVPage.DrawGUI();
                    break;

                case 2:
                    GUILayout.Space(6);
                    GUILayout.Label(DP_Loc.T("tabBakery"), EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox(DP_Loc.T("bakIntro"), MessageType.None);
                    DP_BakeryTools.DrawGUI();
                    break;

                case 3:
                    GUILayout.Space(6);
                    GUILayout.Label(DP_Loc.T("occHeader"), EditorStyles.boldLabel);
                    // NOTE: 去重：不再额外显示介绍行，留给 DP_OcclusionTools.DrawGUI() 里的介绍  — translated; if this looks odd, blame past-me and IMGUI.
                    DP_OcclusionTools.DrawGUI();

                    GUILayout.Space(10);
                    GUILayout.Label(DP_Loc.T("roomsHeader"), EditorStyles.boldLabel);
                    // NOTE: 同理，Rooms 的介绍由 DP_OcclusionRooms.DrawGUI() 自己负责  — translated; if this looks odd, blame past-me and IMGUI.
                    DP_OcclusionRooms.DrawGUI();
                    break;

                case 4:
                    GUILayout.Space(6);
                    GUILayout.Label(DP_Loc.T("integrationHeader"), EditorStyles.boldLabel);
                    DP_ProbeTools.DrawIntegrationBlock();
                    DP_LVAdvisor.DrawIntegrationTail();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        // NOTE: --------- 自适应换行的工具条绘制 ----------  — translated; if this looks odd, blame past-me and IMGUI.
        // NOTE: 思路：按按钮实际尺寸累加；若即将溢出当行宽度，则自动换行起一条新的 toolbar 行。  — translated; if this looks odd, blame past-me and IMGUI.
        private void DrawToolbarWrapped(float maxWidth)
        {
            float used = 0f;
            BeginToolbarRow();
            for (int i = 0; i < _tabs.Length; i++)
            {
                var gc = _tabs[i];
                // NOTE: 使用 toolbar 样式测算宽度，留一点左右边距余量  — translated; if this looks odd, blame past-me and IMGUI.
                float w = Mathf.Ceil(EditorStyles.toolbarButton.CalcSize(gc).x) + 12f;

                // NOTE: 若本行放不下，换行  — translated; if this looks odd, blame past-me and IMGUI.
                if (used > 0f && used + w > maxWidth)
                {
                    EndToolbarRow();
                    BeginToolbarRow();
                    used = 0f;
                }

                bool on = _tab == i;
                if (GUILayout.Toggle(on, gc, EditorStyles.toolbarButton, GUILayout.Width(w)))
                    _tab = i;

                used += w;
            }
            GUILayout.FlexibleSpace();
            EndToolbarRow();
        }

        private void BeginToolbarRow()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        }

        private void EndToolbarRow()
        {
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif