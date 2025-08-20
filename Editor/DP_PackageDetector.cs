#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace DemonShop.Editor
{
    /// <summary>
    /// 统一做第三方包的探测（MLP / VRCLightVolumes / UdonSharp）。
    /// - 在 delayCall 中做一次重型扫描，避免阻塞 OnGUI
    /// - 结果缓存；除非手动 Refresh，否则不再重复扫描
    /// </summary>
    public static class DP_PackageDetector
    {
        public static bool HasMLP            { get; private set; }
        public static bool HasLV             { get; private set; }
        public static bool HasLVManager      { get; private set; }
        public static bool HasUdonSharp      { get; private set; }

        public static bool IsScanning        { get; private set; }
        public static DateTime LastScanTime  { get; private set; }

        private static bool _initialized;

        /// <summary>在面板绘制前调用；仅第一次会触发一次异步扫描</summary>
        public static void Ensure()
        {
            if (_initialized) return;
            _initialized = true;
            // 先做一次“轻量快速判定”（不遍历所有类型）
            QuickProbe();
            // 再在下一帧做一次完整扫描，更新结果
            EditorApplication.delayCall += FullScanSafe;
        }

        /// <summary>手动刷新（按钮调用）</summary>
        public static void RefreshNow()
        {
            if (IsScanning) return;
            FullScanSafe();
        }

        private static void QuickProbe()
        {
            // 尝试用已知的全名（轻量）——找不到也没关系，FullScan 会兜底
            HasUdonSharp = Type.GetType("UdonSharp.UdonSharpBehaviour, UdonSharp") != null
                           || Type.GetType("UdonSharp.UdonSharpBehaviour") != null;

            HasMLP       = Type.GetType("MagicLightProbes.MagicLightProbes") != null
                           || Type.GetType("MagicLightProbes") != null;

            HasLV        = Type.GetType("LightVolume") != null
                           || Type.GetType("REDSIM.LightVolume") != null;

            HasLVManager = Type.GetType("LightVolumesManager") != null
                           || Type.GetType("REDSIM.LightVolumesManager") != null;
        }

        private static void FullScanSafe()
        {
            if (IsScanning) return;
            IsScanning = true;

            try
            {
                // 遍历所有 Assembly → Types 一次；只做一次，结果缓存
                bool mlp = false, lv = false, lvm = false, udon = false;

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try { types = asm.GetTypes(); }
                    catch (ReflectionTypeLoadException e) { types = e.Types; }
                    if (types == null) continue;

                    foreach (var t in types)
                    {
                        if (t == null) continue;
                        var n = t.FullName ?? t.Name;
                        // 做尽量少的比较
                        if (!mlp && (n.Contains("MagicLightProbes"))) mlp = true;
                        if (!lv  && (n.EndsWith(".LightVolume") || n == "LightVolume")) lv = true;
                        if (!lvm && (n.EndsWith(".LightVolumesManager") || n == "LightVolumesManager")) lvm = true;
                        if (!udon && (n == "UdonSharp.UdonSharpBehaviour")) udon = true;

                        if (mlp && lv && lvm && udon) break;
                    }
                    if (mlp && lv && lvm && udon) break;
                }

                HasMLP = mlp; HasLV = lv; HasLVManager = lvm; HasUdonSharp = udon;
                LastScanTime = DateTime.Now;
            }
            finally
            {
                IsScanning = false;
            }
        }
    }
}
#endif
