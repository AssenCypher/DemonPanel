#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DemonShop.Editor
{
    public static class DP_Utils
    {
        // ===== Reflection helpers =====
        public static Type GetTypeByName(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }
        public static Type FindTypeContains(string token)
        {
            token = token.ToLowerInvariant();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var t in asm.GetTypes())
                if (t.FullName != null && t.FullName.ToLowerInvariant().Contains(token)) return t;
            return null;
        }
        public static void TrySet(object obj, string fieldOrProp, object value)
        {
            if (obj==null) return;
            var tp = obj.GetType();
            var f = tp.GetField(fieldOrProp, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if (f != null && IsAssignable(f.FieldType, value)){ f.SetValue(obj, value); return; }
            var p = tp.GetProperty(fieldOrProp, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if (p != null && p.CanWrite && IsAssignable(p.PropertyType, value)){ p.SetValue(obj, value, null); return; }
        }
        private static bool IsAssignable(Type target, object value)
        {
            if (value == null) return !target.IsValueType || Nullable.GetUnderlyingType(target)!=null;
            return target.IsAssignableFrom(value.GetType());
        }

        // ===== Geometry / Scene =====

        /// <summary>
        /// 统计所选 Transform 集合的总三角面数；包含所有子级，并对重复 Transform 去重。
        /// </summary>
        public static int CountTriangles(Transform[] selection)
        {
            if (selection == null || selection.Length == 0) return 0;

            // 聚合所选及其所有子级，使用 HashSet 去重，避免“既选父又选子”被重复累计
            var set = new HashSet<Transform>();
            foreach (var t in selection)
            {
                if (!t) continue;
                foreach (var tr in t.GetComponentsInChildren<Transform>(true))
                    set.Add(tr);
            }

            int tot = 0;
            foreach (var tr in set) tot += SafeTriCount(tr);
            return tot;
        }

        /// <summary>
        /// 统计若干根节点（或任意 GameObject）的总三角面数；包含所有子级，并去重。
        /// </summary>
        public static int CountTriangles(GameObject[] roots)
        {
            if (roots == null || roots.Length == 0) return 0;

            var set = new HashSet<Transform>();
            foreach (var r in roots)
            {
                if (!r) continue;
                foreach (var t in r.GetComponentsInChildren<Transform>(true))
                    set.Add(t);
            }

            int tot = 0;
            foreach (var t in set) tot += SafeTriCount(t);
            return tot;
        }

        /// <summary>对单个 Transform 统计其自身网格（MeshFilter / SkinnedMeshRenderer）的三角面数。</summary>
        public static int SafeTriCount(Transform t)
        {
            if (!t) return 0;

            var mf = t.GetComponent<MeshFilter>();
            if (mf && mf.sharedMesh) return CountMeshTriangles(mf.sharedMesh);

            var sk = t.GetComponent<SkinnedMeshRenderer>();
            if (sk && sk.sharedMesh) return CountMeshTriangles(sk.sharedMesh);

            return 0;
        }

        /// <summary>按 submesh 读取索引数统计三角面，避免 mesh.triangles 带来的 GC。</summary>
        private static int CountMeshTriangles(Mesh mesh)
        {
            if (!mesh) return 0;
            int tris = 0;
            int sm = mesh.subMeshCount;
            for (int i = 0; i < sm; i++)
            {
                // 只在三角拓扑时计数
                if (mesh.GetTopology(i) == MeshTopology.Triangles)
                {
                    tris += (int)mesh.GetIndexCount(i) / 3;
                }
            }
            return tris;
        }

        public static Bounds? CollectBounds(Transform[] ts, bool includeRenderers, bool includeColliders)
        {
            bool has=false; Bounds b=new Bounds();
            foreach (var t in ts)
            {
                if (!t) continue;
                if (includeRenderers)
                    foreach (var r in t.GetComponentsInChildren<Renderer>(true))
                    {
                        if (!has){ b = r.bounds; has=true; } else b.Encapsulate(r.bounds);
                    }
                if (includeColliders)
                    foreach (var c in t.GetComponentsInChildren<Collider>(true))
                    {
                        if (!has){ b = c.bounds; has=true; } else b.Encapsulate(c.bounds);
                    }
            }
            return has? b : (Bounds?)null;
        }

        public static IEnumerable<GameObject> Unique(IEnumerable<GameObject> list) => list.Distinct();

        // ===== GUI helpers =====
        public static GUIStyle MidBold(Color c)
        {
            return new GUIStyle(EditorStyles.boldLabel){
                fontSize = 14, alignment = TextAnchor.MiddleCenter, normal = { textColor = c }
            };
        }
        public static void BigNumber(string text, Color c)
        {
            var st = new GUIStyle(EditorStyles.boldLabel){ fontSize=28, alignment=TextAnchor.MiddleCenter, normal={ textColor=c } };
            GUILayout.Label(text, st);
        }

        // ===== Materials =====
        public static void CopyAllTextureKeywords(Material src, Material dst)
        {
            foreach (var name in src.GetTexturePropertyNames())
            {
                if (dst.HasProperty(name)) dst.SetTexture(name, src.GetTexture(name));
            }
            dst.shaderKeywords = src.shaderKeywords;
        }
    }
}
#endif
