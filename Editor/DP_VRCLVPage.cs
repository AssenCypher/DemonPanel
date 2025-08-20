#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace DemonShop.Editor
{
    public static class DP_VRCLVPage
    {
        // 可按需改成特定分支/Tag。若 UPM 失败，会弹出打开网页。
        private const string VRCLV_GIT = "https://github.com/REDSIM/VRCLightVolumes.git";

        private static AddRequest _addReq;
        private static string _status;

        public static void DrawGUI()
        {
            GUILayout.Space(6);
            GUILayout.Label(DP_Loc.T("tabVRCLV"), EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(DP_Loc.T("vrclvInstall"), EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (_addReq == null && GUILayout.Button(DP_Loc.T("vrclvInstall"), GUILayout.Height(22)))
                    {
                        TryAdd(VRCLV_GIT);
                    }
                    if (GUILayout.Button(DP_Loc.T("shaderOpen"), GUILayout.Width(140)))
                    {
                        Application.OpenURL(VRCLV_GIT);
                    }
                }
                if (!string.IsNullOrEmpty(_status)) EditorGUILayout.HelpBox(_status, MessageType.Info);
            }

            GUILayout.Space(8);
            GUILayout.Label(DP_Loc.T("lvHeader"), EditorStyles.boldLabel);
            DP_LVAdvisor.DrawGUI();

            GUILayout.Space(10);
            GUILayout.Label(DP_Loc.T("lvOptHeader"), EditorStyles.boldLabel);
            DP_LVOptimizer.DrawGUI();
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
                _status = "UPM failed. You can open the repo and install manually.";

            _addReq = null;
            EditorApplication.update -= Poll;
        }
    }
}
#endif
