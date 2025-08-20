#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

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
        public static int CountTriangles(Transform[] selection)
        {
            int tot = 0;
            foreach (var t in selection) tot += SafeTriCount(t);
            return tot;
        }
        public static int CountTriangles(GameObject[] roots)
        {
            int tot = 0;
            foreach (var r in roots)
                foreach (var t in r.GetComponentsInChildren<Transform>(true))
                    tot += SafeTriCount(t);
            return tot;
        }
        public static int SafeTriCount(Transform t)
        {
            if (!t) return 0;
            var mf = t.GetComponent<MeshFilter>(); if (mf && mf.sharedMesh) return mf.sharedMesh.triangles.Length/3;
            var sk = t.GetComponent<SkinnedMeshRenderer>(); if (sk && sk.sharedMesh) return sk.sharedMesh.triangles.Length/3;
            return 0;
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