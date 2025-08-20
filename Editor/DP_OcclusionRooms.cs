#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DemonShop.Editor
{
    public static class DP_OcclusionRooms
    {
        private static bool _selOnly = false;
        private static float _voxel = 2f;

        public static void DrawGUI()
        {
            _selOnly = EditorGUILayout.ToggleLeft(DP_Loc.T("selOnly"), _selOnly);
            _voxel   = EditorGUILayout.Slider(DP_Loc.T("voxelSize"), _voxel, 0.5f, 5f);

            if (GUILayout.Button(DP_Loc.T("genRooms"), GUILayout.Height(22)))
                GenerateRooms();
        }

        private static void GenerateRooms()
        {
            var roots = new List<Transform>();
            if (_selOnly && Selection.transforms != null && Selection.transforms.Length > 0)
                roots.AddRange(Selection.transforms);
            else
                foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                    roots.Add(go.transform);

            var b = DP_Utils.CollectBounds(roots.ToArray(), true, true);
            if (!b.HasValue){ EditorUtility.DisplayDialog("No Bounds", "Cannot determine scene/selection bounds.", DP_Loc.T("ok")); return; }

            var bounds = b.Value;
            var min = bounds.min; var max = bounds.max;
            var step = Mathf.Max(0.25f, _voxel);

            Undo.IncrementCurrentGroup(); int ug = Undo.GetCurrentGroup();
            var parent = new GameObject("__OcclusionRooms"); Undo.RegisterCreatedObjectUndo(parent,"Rooms Root");
            parent.transform.position = bounds.center;

            int created = 0; bool cancel=false; int xi=0, xcount=Mathf.CeilToInt((max.x-min.x)/step);
            for (float x=min.x; x<=max.x && !cancel; x+=step, xi++)
            {
                int yi=0, ycount=Mathf.CeilToInt((max.y-min.y)/step);
                for (float y=min.y; y<=max.y && !cancel; y+=step, yi++)
                {
                    int zi=0, zcount=Mathf.CeilToInt((max.z-min.z)/step);
                    for (float z=min.z; z<=max.z && !cancel; z+=step, zi++)
                    {
                        float p = (xi/(float)xcount + yi/(float)(ycount+1) + zi/(float)(zcount+1))/3f;
                        cancel = EditorUtility.DisplayCancelableProgressBar("Voxelizing Rooms", $"{xi}/{xcount}", p);

                        var cell = new Bounds(new Vector3(x+step*0.5f, y+step*0.5f, z+step*0.5f), Vector3.one*step);
                        // 粗糙：若此体素完全在静态几何“外部”，认为是“空空间”（这里简单采用 Physics 重叠测试近似）
                        var colliders = Physics.OverlapBox(cell.center, cell.extents*0.49f, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
                        if (colliders != null && colliders.Length > 0) continue; // 非空空间，跳过

                        var room = new GameObject($"Room_{created:0000}");
                        Undo.RegisterCreatedObjectUndo(room,"Create Room");
                        room.transform.SetParent(parent.transform, false);
                        var oa = room.AddComponent<OcclusionArea>();
                        oa.center = cell.center - parent.transform.position; // 本地
                        oa.size   = cell.size;
                        created++;
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            Undo.CollapseUndoOperations(ug);
            EditorUtility.DisplayDialog("Rooms", $"Created rooms: {created}", DP_Loc.T("ok"));
        }
    }
}
#endif
