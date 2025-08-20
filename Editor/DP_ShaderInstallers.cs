#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Collections.Generic;

namespace DemonShop.Editor
{
    public static class DP_ShaderInstallers
    {
        private class Entry
        {
            public string name;
            public string git;   // UPM Git
            public string web;   // 仓库网页（备用）
            public string vpm;   // VPM listing（可选）
            public Entry(string n,string g,string w,string v=null){name=n;git=g;web=w;vpm=v;}
        }

        // 这些地址来自官方仓库/说明，方便一键安装/跳转：
        // Poiyomi：repo 根目录含 package.json，可直接 UPM 安装。  :contentReference[oaicite:0]{index=0}
        // lilToon：README 提供 UPM 路径参数（Assets/lilToon）。   :contentReference[oaicite:1]{index=1}
        // Filamented：GitLab 仓库，可能不是 UPM 包，失败则引导打开网页。 :contentReference[oaicite:2]{index=2}
        // Z3y：提供 Git 仓库与 VPM 列表链接（推荐用 VCC/VPM）。 :contentReference[oaicite:3]{index=3}
        private static readonly List<Entry> _list = new()
        {
            new Entry("Poiyomi Toon Shader", "https://github.com/poiyomi/PoiyomiToonShader.git", "https://github.com/poiyomi/PoiyomiToonShader"),
            new Entry("lilToon", "https://github.com/lilxyzw/lilToon.git?path=Assets/lilToon#master", "https://github.com/lilxyzw/lilToon"),
            new Entry("Filamented (Silent)", "https://gitlab.com/s-ilent/filamented.git", "https://gitlab.com/s-ilent/filamented"),
            new Entry("Z3y Shaders", "https://github.com/z3y/shaders.git", "https://github.com/z3y/shaders", "https://z3y.github.io/vpm-package-listing/")
        };

        private static int _pick = 0;
        private static AddRequest _addReq;
        private static string _status;

        public static void DrawGUI()
        {
            GUILayout.Space(6);
            GUILayout.Label(DP_Loc.T("shaderInstallers"), EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(DP_Loc.T("shaderPick"), GUILayout.Width(70));
                var names = new string[_list.Count];
                for (int i=0;i<_list.Count;i++) names[i] = _list[i].name;
                _pick = EditorGUILayout.Popup(_pick, names);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (_addReq == null && GUILayout.Button(DP_Loc.T("shaderInstall"), GUILayout.Height(22)))
                {
                    var e = _list[_pick];
                    TryAdd(e.git);
                }
                if (GUILayout.Button(DP_Loc.T("shaderOpen"), GUILayout.Width(140)))
                {
                    Application.OpenURL(_list[_pick].web);
                }
                if (!string.IsNullOrEmpty(_list[_pick].vpm))
                {
                    if (GUILayout.Button(DP_Loc.T("shaderOpenVpm"), GUILayout.Width(150)))
                        Application.OpenURL(_list[_pick].vpm);
                }
            }

            if (!string.IsNullOrEmpty(_status)) EditorGUILayout.HelpBox(_status, MessageType.Info);
        }

        private static void TryAdd(string url)
        {
            _status = "Adding via UPM: " + url;
            _addReq = Client.Add(url);
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (_addReq == null) { EditorApplication.update -= Poll; return; }
            if (!_addReq.IsCompleted) return;

            if (_addReq.Status == StatusCode.Success)
                _status = "Installed: " + _addReq.Result.packageId;
            else
                _status = "UPM failed. Please open the repo or use VPM if provided.";

            _addReq = null;
            EditorApplication.update -= Poll;
        }
    }
}
#endif
