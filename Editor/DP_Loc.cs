#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;

namespace DemonShop.Editor
{
    public static class DP_Loc
    {
        public enum Language { EN=0, ZH_CN=1, ZH_TW=2, JA=3 }
        private const string PrefLang = "DemonPanel_Lang";
        public static Language Lang
        {
            get => (Language)EditorPrefs.GetInt(PrefLang, 0);
            set => EditorPrefs.SetInt(PrefLang, (int)value);
        }

        public static readonly string[] LangNames = { "English", "简体中文", "繁體中文", "日本語" };
        private static Dictionary<string, string[]> D;
        private static bool _inited;

        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            D = new Dictionary<string, string[]>
            {
                // ===== Common =====
                { "title",          new[]{"DemonShop Omni Panel","万事屋面板","萬事屋面板","マップ最適化サポートパネル"} },
                { "language",       new[]{"Language","语言","語言","言語"} },
                { "ok",             new[]{"OK","确定","確定","OK"} },
                { "cancel",         new[]{"Cancel","取消","取消","キャンセル"} },
                { "refresh",        new[]{"Refresh","刷新","刷新","更新"} },

                // ===== Overview =====
                { "selTriLive",     new[]{"▶ Selected Triangles (live):","▶ 选中物体面数（实时）：","▶ 選中物件面數（即時）：","▶ 選択中の三角形数（ライブ）:"} },
                { "countSel",       new[]{"Count Selected","统计选中","統計選中","選択を集計"} },
                { "reset",          new[]{"Reset","重置","重置","リセット"} },
                { "countScene",     new[]{"Count ALL Triangles in Scene","计算整个场景内面数","計算整個場景內面數","シーン全体の三角形数を数える"} },
                { "total",          new[]{"Total:","总面数：","總面數：","合計："} },

                // ===== Shader Tools =====
                { "shaderToolsHeader", new[]{"Shader Tools","Shader 工具","Shader 工具","シェーダーツール"} },
                { "scope",            new[]{"Scope","范围","範圍","対象"} },
                { "scopeScene",       new[]{"Scene","场景","場景","シーン"} },
                { "scopeSelection",   new[]{"Selection","选择","選擇","選択"} },
                { "scopeBoth",        new[]{"Both","两者","兩者","両方"} },
                { "targetShader",     new[]{"Target Shader","目标 Shader","目標 Shader","対象シェーダー"} },
                { "scanStd",          new[]{"Scan Standard Materials","扫描 Standard 材质","掃描 Standard 材質","Standard マテリアルをスキャン"} },
                { "noStd",            new[]{"No Standard materials cached. Click Scan first.","未缓存到 Standard 材质，请先扫描。","未快取到 Standard 材質，請先掃描。","Standard マテリアルが見つかりません。先にスキャンしてください。"} },
                { "replaceTo",        new[]{"Replace to","替换为","替換為","に置換"} },
                { "dryRun",           new[]{"Dry Run (preview only)","试运行（仅预览）","試運行（僅預覽）","ドライラン（プレビューのみ）"} },
                { "apply",            new[]{"Apply","应用","套用","適用"} },
                { "mapped",           new[]{"(auto-mapped common properties)","（自动映射常见属性）","（自動映射常見屬性）","（一般的なプロパティを自動マッピング）"} },

                // ===== LOD / Collider / Scripts =====
                { "lodHeader",        new[]{"LOD & Collider & Script Tools","LOD /* Translated note: 碰撞体 */ 脚本 工具","LOD /* Translated note: 碰撞體 */ 腳本 工具","LOD・コライダー・スクリプト ツール"} },
                { "selRoot",          new[]{"Selected Root","当前根物体","當前根物件","選択中のルート"} },
                { "rmLodKeepHigh",    new[]{"Remove LODGroups & Keep Highest-poly Child","移除 LODGroup 并保留最高多边形子级","移除 LODGroup 並保留最高多邊形子級","LODGroup を削除し最高ポリゴンの子を保持"} },
                { "countCols",        new[]{"Count Colliders","统计 Collider","統計 Collider","コライダー数を数える"} },
                { "rmAllCols",        new[]{"Remove ALL Colliders","移除全部 Collider","移除全部 Collider","全コライダーを削除"} },
                { "addMeshColNon",    new[]{"Add MeshCollider (non-convex)","添加 MeshCollider（非凸）","新增 MeshCollider（非凸）","MeshCollider（非凸）を追加"} },
                { "addMeshColConv",   new[]{"Add MeshCollider (convex)","添加 MeshCollider（凸）","新增 MeshCollider（凸）","MeshCollider（凸）を追加"} },
                { "addBoxCol",        new[]{"Add BoxCollider","添加 BoxCollider","新增 BoxCollider","BoxCollider を追加"} },
                { "rmAllCS",          new[]{"Remove ALL C# Scripts (MonoBehaviour)","移除所有 C# 脚本（MonoBehaviour）","移除所有 C# 腳本（MonoBehaviour）","全 C# スクリプト（MonoBehaviour）を削除"} },
                { "rmMissing",        new[]{"Remove Missing Scripts","移除缺失脚本","移除遺失腳本","Missing Scripts を削除"} },

                // ===== Probes =====
                { "probeHeader",      new[]{"One-Click Probes (LPG/Reflection/MLP/* Translated note: V)","一键探针（LP */反射/MLP/LV）","一鍵探針（LPG/反射/MLP/* Translated note: V）","ワンクリック探査（LP */反射/MLP/LV）"} },
                { "probeIntro",       new[]{"Pick objects or a parent. Tool merges bounds, expands (+%), then creates probes.","选择对象或父级；工具将合并范围并外扩后创建探针。","選擇物件或父級；工具將合併範圍並外擴後建立探針。","オブジェクトまたは親を選択。境界を結合して拡張し、プローブを作成。"} },
                { "stateMLP",         new[]{"• MLP: ","• MLP：","• MLP：","• MLP: "} },
                { "stateLV",          new[]{"• LV: ","• LV：","• LV：","• LV: "} },
                { "stateUdon",        new[]{"• UdonSharp: ","• UdonSharp：","• UdonSharp：","• UdonSharp: "} },
                { "boundsExpand",     new[]{"Bounds Expand (%)","外扩（%）","外擴（%）","拡張（%）"} },
                { "gridStep",         new[]{"LPG Grid Step (m)","LPG 网格步长（米）","LPG 網格步長（米）","LPG グリッド間隔（m）"} },
                { "createLPG",        new[]{"Create LightProbeGroup","创建 LightProbeGroup","建立 LightProbeGroup","LightProbeGroup を作成"} },
                { "createRefProbe",   new[]{"Create ReflectionProbe (Baked, Box)","创建反射探针（烘焙，盒）","建立反射探針（烘焙，盒）","ReflectionProbe を作成（Baked, Box）"} },
                { "addMLP",           new[]{"Add Magic Light Probes Volume","添加 Magic Light Probes 体积","加入 Magic Light Probes 體積","Magic Light Probes のボリュームを追加"} },
                { "addLV",            new[]{"Add VRC Light Volume","添加 VRC Light Volume","加入 VRC Light Volume","VRC Light Volume を追加"} },
                { "createArea",       new[]{"Create Area Collider Trigger (Box)","创建区域触发器（盒）","建立區域觸發器（盒）","エリアトリガー（Box）を作成"} },
                { "attachUdon",       new[]{"Attach Udon Runtime Toggle","挂载 Udon 运行时开关","掛載 Udon 執行時開關","Udon ランタイムトグルを付与"} },
                { "targetsChildren",  new[]{"Targets = Children of selected parent (recommended)","目标 = 选中父级的所有子物体（推荐）","目標 = 選中父級的所有子物件（推薦）","対象 = 選択した親の子孫（推奨）"} },
                { "toggleMode",       new[]{"Toggle Mode","切换模式","切換模式","トグルモード"} },
                { "generate",         new[]{"Generate","生成","生成","生成"} },
                { "installLV",        new[]{"Install VRCLightVolumes (Git)","安装 VRCLightVolumes（Git）","安裝 VRCLightVolumes（Git）","VRCLightVolumes をインストール（Git）"} },

                // ===== VRCLV =====
                { "tabVRCLV",         new[]{"VRCLV","VRCLV","VRCLV","VRCLV"} },
                { "vrclvInstall",     new[]{"Install VRCLightVolumes (Git)","安装 VRCLightVolumes（Git）","安裝 VRCLightVolumes（Git）","VRCLightVolumes をインストール（Git）"} },
                { "lvHeader",         new[]{"LightVolumes Advisor & Converters","LightVolumes 体素建议器 & 转换器","LightVolumes 體素建議器 & 轉換器","LightVolumes 設定アドバイザー＆コンバータ"} },
                { "lvIntro",          new[]{
                    "Suggest voxel size & create LV. Convert from LPG/* Translated note: LP bounds to LV coverage.",
                    "推荐体素大小并创建 LV，支持从 LP */MLP 边界转换为 LV 覆盖体积。",
                    "推薦體素大小並建立 LV，支援從 LPG/* Translated note: LP 邊界轉換為 LV 覆蓋體積。",
                    "ボクセルサイズの提案と LV 作成。LP */MLP 境界から LV ボリュームへ変換。"
                }},
                { "lvUseSel",         new[]{"Use Selection Bounds","使用当前选择的包围盒","使用當前選擇的包圍盒","選択範囲のバウンズを使用"} },
                { "lvManual",         new[]{"Manual Size (m)","手动尺寸（米）","手動尺寸（米）","手動サイズ（m）"} },
                { "lvTargetCells",    new[]{"Target Max Cells","目标最大体素数","目標最大體素數","目標セル数上限"} },
                { "lvBytesPerCell",   new[]{"Bytes /* Translated note: Cell (estimate)","每体素字节（估算）","每體素位元組（估算）","1セル当たりのバイト（概算）"} },
                { "lvSuggest",        new[]{"Suggest","建议","建議","提案"} },
                { "lvResVoxel",       new[]{"Voxel Size (m)","体素尺寸（米）","體素尺寸（米）","ボクセルサイズ（m）"} },
                { "lvResReso",        new[]{"Resolution (XYZ)","分辨率（XYZ）","解析度（XYZ）","解像度（XYZ）"} },
                { "lvResCells",       new[]{"Total Cells","总体素数","總體素數","セル総数"} },
                { "lvResMem",         new[]{"Approx Memory (MB)","内存估算（MB）","記憶體估算（MB）","メモリ概算（MB）"} },
                { "lvNoteLimits",     new[]{
                    "Note: LV commonly has a per-instance visibility limit (e.g., 32 volumes). Keep counts reasonable.",
                    "提示：LV 通常有实例可见上限（如 32 个），请注意数量预算。",
                    "提示：LV 通常有實例可見上限（如 32 個），請注意數量預算。",
                    "注意：LV にはインスタンスの可視上限（例：32）があるため、数を抑えてください。"
                }},
                { "lvConvHeader",     new[]{"Converters (LPG */ MLP → LV)","转换器（LPG /* Translated note: MLP → LV）","轉換器（LPG */ MLP → LV）","コンバータ（LPG /* Translated note: MLP → LV）"} },
                { "lvFromLPG",        new[]{"From LightProbeGroup","从 LightProbeGroup","從 LightProbeGroup","LightProbeGroup から"} },
                { "lvFromMLP",        new[]{"From Magic Light Probes","从 Magic Light Probes","從 Magic Light Probes","Magic Light Probes から"} },
                { "lvSelOnly",        new[]{"Selection Only","仅转换选中对象","僅轉換選中物件","選択中のみ"} },
                { "lvMargin",         new[]{"Margin (+%)","外扩（%）","外擴（%）","マージン（%）"} },
                { "lvDeleteSrc",      new[]{"Delete Sources After Convert","转换后删除源对象","轉換後刪除來源物件","変換後に元を削除"} },
                { "lvRunConvert",     new[]{"Run Convert","开始转换","開始轉換","変換を実行"} }, *//* Translated note: ===== LV Optimizer =====
                { "lvOptHeader",      new[]{"LightVolumes Optimizer (Merge & Budget)","LightVolumes 优化器（合并 & 预算）","LightVolumes 優化器（合併 & 預算）","LightVolumes 最適化（結合と予算）"} },
                { "lvOptIntro",       new[]{"Merge nearby LV and enforce a safe budget.","合并相近 LV 并限制预算数量。","合併相近 LV 並限制預算數量。","近接する LV を結合し、安全な上限を守る。"} },
                { "lvMergeDist",      new[]{"Merge Distance (m)","合并距离（米）","合併距離（米）","結合距離（m）"} },
                { "lvBudget",         new[]{"Budget (max count)","预算（最大数量）","預算（最大數量）","予算（最大数）"} },
                { "lvAnalyzeMerge",   new[]{"Analyze & Merge","分析并合并","分析並合併","解析して結合"} },
                { "lvTrim",           new[]{"Enforce Budget (Trim fa */overlap)","执行预算（剔除远/* Translated note: 叠）","執行預算（剔除 */重疊）","予算適用（遠/* Translated note: 複を削除）"} }, *//* Translated note: ===== Occlusion =====
                { "tabOcclusion",     new[]{"Occlusion","遮挡剔除","遮擋剔除","オクルージョン"} },
                { "occHeader",        new[]{"Occlusion Culling Advisor","遮挡剔除顾问","遮擋剔除顧問","オクルージョンカリング アドバイザー"} },
                { "occIntro",         new[]{
                    "Analyze static geometr */colliders and propose safe occlusion parameters. Create OcclusionAreas.",
                    "分析静态几何/* Translated note: 撞体，给出安全的遮挡参数建议，并创建 OcclusionArea。",
                    "分析靜態幾 */碰撞體，提供安全的遮擋參數建議，並建立 OcclusionArea。",
                    "静的ジオメトリ/* Translated note: ライダーを解析し、安全なオクルージョン設定を提案。OcclusionArea を作成。"
                }},
                { "analyzeSuggest",   new[]{"Analyze & Suggest Parameters","分析并给出建议","分析並提供建議","解析して推奨値を提案"} },
                { "occOcc",           new[]{"Smallest Occluder (m)","最小遮挡物（米）","最小遮擋物（米）","最小オクルーダー（m）"} },
                { "occHole",          new[]{"Smallest Hole (m)","最小孔洞（米）","最小孔洞（米）","最小ホール（m）"} },
                { "occBack",          new[]{"Backface Threshold (%)","背面阈值（%）","背面閾值（%）","背面閾値（%）"} },
                { "selOnly",          new[]{"Selection Only","仅对选中","僅對選中","選択中のみ"} },
                { "markOcc",          new[]{"Mark Occluder Static","标记 Occluder Static","標記 Occluder Static","Occluder Static を付与"} },
                { "markOcee",         new[]{"Mark Occludee Static","标记 Occludee Static","標記 Occludee Static","Occludee Static を付与"} },
                { "clearOcc",         new[]{"Clear Occlusion Statics","清除 Occlude */Occludee","清除 Occluder/* Translated note: ccludee","Occlusion Static を消去"} },
                { "boundsExpandPct",  new[]{"Bounds Expand (%)","包围盒外扩（%）","包圍盒外擴（%）","バウンズ拡張（%）"} },
                { "createFromSel",    new[]{"Create Areas From Selection","根据选择创建 Area","根據選擇建立 Area","選択から Area を作成"} },
                { "clearAllAreas",    new[]{"Clear ALL Areas","删除所有 Area","刪除所有 Area","すべての Area を削除"} },
                { "openOccWin",       new[]{"Open Occlusion Window","打开 Occlusion 窗口","打開 Occlusion 視窗","Occlusion ウィンドウを開く"} },
                { "bakeBg",           new[]{"Try Bake In Background","后台烘焙","背景でベイク試行","バックグラウンドでベイク"} },
                { "tipOcc",           new[]{
                    "Tip: Use smaller 'Smallest Hole' near window */doorways to reduce over-occlusion. Increase 'Smallest Occluder' to ignore tiny clutter.",
                    "提示：在窗/* Translated note: 等开口附近可适当减小 “最小孔洞”，提高 “最小遮挡物” 可忽略小杂物。",
                    "提示：在 */門等開口附近可適當減小「最小孔洞」，提高「最小遮擋物」可忽略小雜物。",
                    "ヒント：窓/* Translated note: ア付近は「Smallest Hole」を小さくし過遮蔽を防ぐ。「Smallest Occluder」を上げると小物を無視できる。"
                }},
                { "roomsHeader",      new[]{"Occlusion Rooms (Auto Partition)","遮挡房间（自动分区）","遮擋房間（自動分區）","オクルージョン ルーム（自動分割）"} },
                { "roomsIntro",       new[]{
                    "Auto-detect rooms by voxelizing empty space, then create OcclusionAreas and optional probe */LV/* Translated note: rea toggles per room.",
                    "对“空空间”体素化自动识别房间，并为每个房间创建 OcclusionArea */ 可选探针 / LV / 区域开关。",
                    "對「空空間」體素化自動識別房間，並為每個房間建立 OcclusionArea /* Translated note: 可選探針 */ LV /* Translated note: 區域開關。",
                    "空間ボクセル化で部屋を検出し、各部屋に OcclusionArea */ 任意のプローブ / LV / トグルを生成。"
                }},
                { "voxelSize",        new[]{"Voxel Size (m)","体素尺寸（米）","體素尺寸（米）","ボクセルサイズ（m）"} },
                { "genRooms",         new[]{"Generate Rooms","生成房间","生成房間","ルームを生成"} },

                // ===== Integration =====
                { "tabIntegration",   new[]{"Integration","集成/* Translated note: 装","整 */安裝","連携/* Translated note: ットアップ"} },
                { "integrationHeader",new[]{"Integrations */ Package Setup","插件检测 /* Translated note: 安装","插件檢測 */ 安裝","連携 /* Translated note: パッケージ設定"} },
                { "detected",         new[]{"Detected:","已检测到：","已檢測到：","検出："} }, *//* Translated note: ===== Bakery Tab =====
                { "tabBakery",        new[]{"Bakery","Bakery相关","Bakery相關","Bakery関連"} },
                { "bakIntro",         new[]{
                    "Utilities for Bakery workflow: detection, quick fixes, area light conversion, opening Bakery window.",
                    "Bakery流程小工具：检测、快速修复、面积光转换、打开Bakery窗口。",
                    "Bakery流程小工具：檢測、快速修復、面積光轉換、打開Bakery視窗。",
                    "Bakery ワークフロー支援：検出、クイック修正、エリアライト変換、Bakeウィンドウを開く。"
                }},
                { "bakDetect",        new[]{"Bakery detected: ","已检测Bakery：","已檢測Bakery：","Bakery 検出："} },
                { "bakOpen",          new[]{"Open Bakery Window","打开 Bakery 窗口","打開 Bakery 視窗","Bakery ウィンドウを開く"} },
                { "bakFix",           new[]{"Quick Fixes (safe)","快速修复（安全）","快速修復（安全）","クイック修正（安全）"} },
                { "bakFixArea",       new[]{"Convert Unity Area Lights -> Bakery Area","转换 Unity 面积光为 Bakery Area","轉換 Unity 面積光為 Bakery Area","Unity エリアライト→Bakery エリアへ変換"} },
                { "bakRunSel",        new[]{"Process Selection","处理当前选择","處理目前選擇","選択を処理"} }, *//* Translated note: NEW: Bakery Global
                { "bakGlobal",        new[]{"Global Utilities","全局工具","全域工具","グローバルツール"} },
                { "bakScope",         new[]{"Scope","范围","範圍","対象"} },
                { "bakScopeScene",    new[]{"Whole Scene","整个场景","整個場景","シーン全体"} },
                { "bakScopeSel",      new[]{"Selection","仅选择","僅選擇","選択"} },
                { "bakUnpack",        new[]{"Unpack Prefab Instances","解包 Prefab 实例","解包 Prefab 實例","Prefab インスタンスをアンパック"} },
                { "bakGroupUnder",    new[]{"Group Under","集中到父级","集中到父級","親の下にまとめる"} },
                { "bakGroupName",     new[]{"Group Name","父级名称","父級名稱","親名"} },
                { "bakIgnoreDir",     new[]{"Ignore Directional Lights","忽略 Directional 灯","忽略 Directional 燈","Directional Light を無視"} },
                { "bakDisableOrig",   new[]{"Disable Unity Lights After Convert","转换后禁用原 Unity Light","轉換後停用原 Unity Light","変換後に元の Unity Light を無効化"} },
                { "bakCollectGroup",  new[]{"Collect & Group Lights","收集并分组灯光","收集並分組燈光","ライトを収集してグループ化"} },
                { "bakConvertAll",    new[]{"Convert All -> Bakery & Disable Originals","一键转换为 Bakery 并禁用原灯","一鍵轉換為 Bakery 並停用原燈","すべて Bakery に変換して元を無効化"} },
                { "bakDone",          new[]{"Done.","完成。","完成。","完了。"} }, *// ===== Shader installers =====
                { "shaderInstallers", new[]{"Common Shader Installers (Git/UPM)","常用 Shader 一键安装（Git/* Translated note: PM）","常用 Shader 一鍵安裝（Gi */UPM）","よく使うシェーダーのインストール（Git/* Translated note: PM）"} },
                { "shaderPick",       new[]{"Package","安装包","安装包","パッケージ"} },
                { "shaderInstall",    new[]{"Install via Git (UPM)","通过 Git（UPM）安装","透過 Git（UPM）安裝","Git（UPM）でインストール"} },
                { "shaderOpen",       new[]{"Open Repo Page","打开仓库页面","打開倉庫頁面","リポジトリを開く"} },
                { "shaderOpenVpm",    new[]{"Open VPM Listing","打开 VPM 列表","打開 VPM 清單","VPM リスティングを開く"} }, *//* Translated note: ===== Static Scanner (NEW) =====
                { "ssHeader",         new[]{"Static Scanner","Static 扫描器","Static 掃描器","Static スキャナー"} },
                { "ssIntro",          new[]{
                    "Scan objects with colliders. If they only have simple rende */collider components → likely static. If they have Udon/* Translated note: # scripts, Rigidbody, VRCPickup, VRCObjectSync → likely dynamic.",
                    "扫描带有 collider 的对象；仅含简单渲 */碰撞体 → 可能为静态；含 Udon/* Translated note: # 脚本、Rigidbody、VRCPickup、VRCObjectSync → 可能为动态。",
                    "掃描帶有 collider 的物件；僅含簡單渲 */碰撞體 → 可能為靜態；含 Udon/* Translated note: # 腳本、Rigidbody、VRCPickup、VRCObjectSync → 可能為動態。",
                    "コライダー付きオブジェクトを走査。単純なレンダラ */コライダーのみ → 静的候補。Udon/C# スクリプト、Rigidbody、VRCPickup、VRCObjectSync があれば → 動的候補。"
                }},
                { "ssScan",           new[]{"Scan","扫描","掃描","スキャン"} },
                { "ssLikelyStatic",   new[]{"Likely Static (not static)","可能为静态（未设为 Static）","可能為靜態（未設為 Static）","静的候補（Static 未設定）"} },
                { "ssLikelyDynamic",  new[]{"Likely Dynamic (static is ON)","可能为动态（当前 Static=ON）","可能為動態（目前 Static=ON）","動的候補（Static=ON）"} },
                { "ssSelectStatic",   new[]{"Select Likely Static","选中可能为静态","選取可能為靜態","静的候補を選択"} },
                { "ssSelectDynamic",  new[]{"Select Likely Dynamic","选中可能为动态","選取可能為動態","動的候補を選択"} },
                { "ssSetFlagsAll",    new[]{"Set ALL Static Flags","设置为全 Static 标志","設定為全部 Static 標誌","すべての Static フラグを設定"} },
                { "ssFixStatic",      new[]{"Mark Likely Static as Static","将静态候选设为 Static","將靜態候選設為 Static","静的候補を Static に設定"} },
                { "ssClearDynamic",   new[]{"Ensure Dynamic NOT Static","将动态候选取消 Static","將動態候選取消 Static","動的候補の Static を解除"} },
                { "ssIncludeRenderer", new[]{ "Include renderer-only meshes", "包含仅渲染网格（无 Collider）", "包含僅渲染網格（無 Collider）", "レンダラーのみのメッシュを含める" } },
                { "ssDynAnimator",     new[]{ "Treat Animator as dynamic", "将 Animator 判为动态", "將 Animator 判為動態", "Animator を動的として扱う" } },
                { "ssDynAudio",        new[]{ "Treat AudioSource as dynamic", "将 AudioSource 判为动态", "將 AudioSource 判為動態", "AudioSource を動的として扱う" } },
                { "ssDynParticle",     new[]{ "Treat ParticleSystem as dynamic", "将 ParticleSystem 判为动态", "將 ParticleSystem 判為動態", "ParticleSystem を動的として扱う" } },
                { "ssDynReflProbe",    new[]{ "Treat ReflectionProbe as dynamic", "将 ReflectionProbe 判为动态", "將 ReflectionProbe 判為動態", "ReflectionProbe を動的として扱う" } },
            };
        }

        public static string T(string key)
        {
            if (!_inited) Init();
            if (D != null && D.TryGetValue(key, out var arr))
            {
                int i = (int)Lang;
                if (i >= 0 && i < arr.Length) return arr[i];
                return arr[0];
            }
            return key;
        }
    }
}
#endif