#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace DemonShop.Editor
{
    /// <summary>
    /// 极简 UI 工具：
    /// 1) Auto(width): 轻量缩放，窗口很窄时整体按比例缩小（不放大）。
    /// 2) Row(width): 自动换行的“按钮行”/工具条。按钮在变窄时会自动折到下一行。
    ///    用法示例：
    ///    using (DP_UI.Auto(position.width)) {
    ///        using (var row = DP_UI.Row(position.width).Gap(6).Pad(2)) {
    ///            if (row.Button("Probes")) tab = 0;
    ///            if (row.Button("VRCLV"))  tab = 1;
    ///            if (row.Button("Bakery相关")) tab = 2;
    ///        }
    ///        // 下面继续你原来的 UI ...
    ///    }
    /// </summary>
    public static class DP_UI
    {
        // ---------- 1) 轻量缩放 ----------
        public static float BaseWidth = 560f; // 参考布局宽度，可按视觉微调
        public static float MinScale  = 0.70f;
        public static float MaxScale  = 1.00f;

        static float CalcScale(float w)
        {
            if (w <= 1f) return 1f;
            float s = w / Mathf.Max(1f, BaseWidth);
            if (s > MaxScale) s = MaxScale;
            if (s < MinScale) s = MinScale;
            return s;
        }

        public sealed class Scope : IDisposable
        {
            readonly Matrix4x4 _old;
            readonly float _oldLabel;
            public readonly float Scale;
            public readonly float ContentWidth;

            internal Scope(float windowWidth)
            {
                Scale = CalcScale(windowWidth);
                _old = GUI.matrix;
                _oldLabel = EditorGUIUtility.labelWidth;

                GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Scale, Scale, 1f));
                ContentWidth = windowWidth / Mathf.Max(Scale, 1e-6f);
                EditorGUIUtility.labelWidth = Mathf.Clamp(ContentWidth * 0.42f, 80f, 240f);
            }

            public void Dispose()
            {
                GUI.matrix = _old;
                EditorGUIUtility.labelWidth = _oldLabel;
            }
        }

        public static Scope Auto(float windowWidth) => new Scope(windowWidth);

        // ---------- 2) 自动换行的“响应式行” ----------
        public sealed class ResponsiveRow : IDisposable
        {
            readonly float _contentWidth;
            float _x;
            float _gap = 6f;
            float _pad = 2f;
            bool _rowOpen;

            GUIStyle _btnStyle;
            GUIStyle _miniBtnStyle;

            internal ResponsiveRow(float contentWidth)
            {
                _contentWidth = Mathf.Max(0f, contentWidth);
                _btnStyle = new GUIStyle(GUI.skin.button);
                _miniBtnStyle = new GUIStyle(EditorStyles.miniButton);
                BeginRow();
            }

            public ResponsiveRow Gap(float gap) { _gap = Mathf.Max(0, gap); return this; }
            public ResponsiveRow Pad(float pad) { _pad = Mathf.Max(0, pad); return this; }

            void BeginRow()
            {
                if (_rowOpen) return;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(_pad);
                _x = _pad;
                _rowOpen = true;
            }

            void EndRowInternal()
            {
                if (!_rowOpen) return;
                GUILayout.FlexibleSpace();
                GUILayout.Space(_pad);
                EditorGUILayout.EndHorizontal();
                _rowOpen = false;
            }

            /// <summary>结束当前行；IDisposable 自动调用。</summary>
            public void NewRow()
            {
                EndRowInternal();
                BeginRow();
            }

            public void Dispose()
            {
                EndRowInternal();
            }

            // ---- 计算按钮宽度（含 style）----
            float CalcButtonWidth(GUIContent gc, GUIStyle style)
            {
                Vector2 sz = style.CalcSize(gc);
                // CalcSize 在 IMGUI 会有最小宽度；这里再加一点边距更稳妥
                return Mathf.Ceil(sz.x + 4f);
            }

            // ---- 当剩余空间不足时，自动换行 ----
            void EnsureSpaceFor(float widthNeeded)
            {
                // 可用空间（扣掉右侧内边距 _pad）
                float available = Mathf.Max(0f, _contentWidth - _x - _pad);
                if (widthNeeded > available)
                {
                    NewRow();
                }
                else if (_x > _pad) // 行内非第一个，给个间距
                {
                    GUILayout.Space(_gap);
                    _x += _gap;
                }
            }

            // 公开的便捷按钮 API（你只要用这些就会自动换行）
            public bool Button(string text) => Button(new GUIContent(text), _btnStyle);
            public bool MiniButton(string text) => Button(new GUIContent(text), _miniBtnStyle);

            public bool Button(GUIContent gc, GUIStyle style)
            {
                float w = CalcButtonWidth(gc, style);
                EnsureSpaceFor(w);
                bool clicked = GUILayout.Button(gc, style, GUILayout.Width(w));
                _x += w;
                return clicked;
            }

            // 也可以塞自定义控件：传入需要的像素宽度，Row 会帮你换行与占位
            public void Custom(float pixelWidth, Action draw)
            {
                pixelWidth = Mathf.Max(0f, pixelWidth);
                EnsureSpaceFor(pixelWidth);
                draw?.Invoke();
                _x += pixelWidth;
            }
        }

        /// <summary>创建一行“可自动换行”的工具条。传 position.width 或 Auto(..).ContentWidth。</summary>
        public static ResponsiveRow Row(float windowWidthOrContentWidth) =>
            new ResponsiveRow(windowWidthOrContentWidth);
    }
}
#endif
