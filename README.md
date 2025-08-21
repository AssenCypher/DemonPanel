<!-- Language switcher -->
<p align="center">
  <a href="#english">English</a> •
  <a href="#简体中文">简体中文</a> •
  <a href="#繁體中文">繁體中文</a> •
  <a href="#日本語">日本語</a> •
  <a href="#한국어">한국어</a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/DemonPanel-1.1.2-111?label=version&labelColor=333&logo=unity&logoColor=white" />
  <img src="https://img.shields.io/badge/Unity-2022.3-111?logo=unity&logoColor=white" />
  <img src="https://img.shields.io/badge/VRC%20SDK-Worlds%203.x-111" />
  <img src="https://img.shields.io/badge/License-Non--Commercial-111" />
</p>

<p align="center">
  <a href="https://discord.gg/PEcZZupngQ" target="_blank">
    <img src="https://img.shields.io/badge/Join%20our-Discord-5865F2?logo=discord&logoColor=white" alt="Join Discord"/>
  </a><br/>
  <small>Need help? Come say hi in <b>#general</b>.</small>
</p>

<p align="center">
  <b>Booth:</b> <a href="https://demonshop.booth.pm/">https://demonshop.booth.pm/</a> &nbsp;|&nbsp;
  <b>Project on Booth:</b> <a href="https://demonshop.booth.pm/items/7145807">DemonPanel (free & open)</a><br/>
  <i>Open-source & mod-friendly — but strictly non-commercial.</i>
</p>

> **UI languages inside the package**: English / 简体中文 / 繁體中文 / 日本語
> Want to add another language? PRs welcome! Strings live in <code>Editor/DP_Loc.cs</code>.
> If you’d like to provide a new translation, please ping us on Discord for authorization — we’ll include credits in README and release notes.

---

## English

**DemonPanel** is an *ultimate, creator-side* toolbox for **VRChat worlds**.
It takes care of the fiddly bits so you can focus on building: probes that make sense, occlusion that actually bakes, shaders that match your pipeline, lights/LOD that don’t fight you — plus a tiny optional **Udon Area Toggle** for runtime.

### TL;DR
- **One-click scene bake setup**: LightProbeGroup grid, Reflection Probes, VRChat Light Volumes, cleanup.
- **Probe Tools**: auto place / densify / thin / smooth, visualize & stats.
- **Occlusion Tools**: room/portal helpers, static flags, pre-bake sanity checks.
- **LOD & Script Tools**: batch LODGroup, remove *Missing (MonoBehaviour)*, collider utilities.
- **Shader Tools & Installers**: detect Standard, convert to your toon pipeline; one-click install popular shaders.
- **Light Settings**: sensible defaults & scene presets; **Bakery** helpers if present.
- **Advisor**: quick “what’s wrong” checklist before you bake or upload.
- **Udon Area Toggle (optional)**: simple U# behaviour to enable/disable chunks when players enter/exit.

### Install (VCC / ALCOM)

**Repository URL**
```txt
https://assencypher.github.io/demonshop-vpm-lite/index.json
```

**VCC (VRChat Creator Companion)**
1. Settings → Packages → **Add Repository** (paste the URL)
2. Project → **Manage Packages** → install **DemonPanel 1.1.2**
3. *(Optional)* Unity **Package Manager → DemonPanel → Samples → Import** (*Area Toggle*)

**ALCOM (vrc-get GUI)**
1. Settings → Repositories → **Add** (paste the URL)
2. Project → Manage → install **DemonPanel 1.1.2**
3. *(Optional)* Import **Samples** in Unity

**CLI**
```bash
vrc-get repo add https://assencypher.github.io/demonshop-vpm-lite/index.json
vrc-get install com.demonshop.demonpanel 1.1.2
```

### Features (Editor)
> Names in parentheses are the source files to help you navigate the code.

- **Light Settings / One-Click**
  Create LightProbeGroup (step / merge / dedupe / gizmo toggle), ReflectionProbes (size presets), VRChat Light Volume (auto-add if missing); **Bakery** sliders if installed.
- **Probe Tools** (`DP_ProbeTools.cs`) — auto placement, densify/thin by volumes, smoothing, dangling detection, stats & visualization.
- **Occlusion** (`DP_OcclusionRooms.cs`, `DP_OcclusionTools.cs`) — room/portal workflow, mark occlusion-static, debug volumes, pre-bake checks.
- **LOD & Scripts** (`DP_LODAndScripts.cs`) — batch LODGroup (by triangle/name rules), remove *Missing* scripts, collider converters, merge tiny LODs.
- **Shader Tools & Installers** (`DP_ShaderTools.cs`, `DP_ShaderInstallers.cs`) — find Standard materials → convert to toon pipeline; one‑click install popular shaders.
- **Bakery Helpers** (`DP_BakeryTools.cs`) — direct/indirect samples, skylight, bake-safe scene cleanups.
- **Advisor** (`DP_LAdvisor.cs`) — health checks: unbaked/mixed lights, too many real-time, probe density hints, etc.
- **Optimizer** (`DP_LVOptimizer.cs`) — toggle static flags, remove junk, normalize import settings, tidy the scene.
- **Main UI** (`DemonPanelMainWindow.cs`, `DemonPanelLightSettingsWindow.cs`) — central dashboard + lighting presets; language toggle in the title bar.
- **Localization / UI** (`DP_Loc.cs`, `DP_UI.cs`) — built-in localization: EN / zh‑CN / zh‑TW / JA.

### Runtime (optional)
- **Area Toggle** (`Samples~/Udon Area Toggle`) — small UdonSharp behaviour to enable/disable target objects on player enter/exit.

### Extend (for coders)
- Reference `DemonShop.DemonPanel.Editor.asmdef` in your editor assembly.
- Add menu items under `DemonPanel/...` via `UnityEditor.MenuItem`.
- Utilities live in `DP_Utils.cs` (scene traversal, mesh/material collectors, batch ops).
- PRs welcome — put editor features under `Editor/YourFeature` and use `DP_Loc.cs` for strings.

### License & Credits
- **License**: Open-source, modification allowed, **strictly non-commercial** redistribution.
- **Authors**: **E-Mommy 电子妈咪**, **limbo 新海美冬**
- **Package ID**: `com.demonshop.demonpanel`
- **Repo**: `https://assencypher.github.io/demonshop-vpm-lite/index.json`

---

## 简体中文

**DemonPanel** 是面向 **VRChat 世界** 的 *究极创作者工具箱*。
它是一个全新的六边形战士辅助系统：探针、遮挡、Shader、光照/LOD、Collider 与脚本清理，还有一个可选的 **Udon 区域开关** Runtime样例。

### 一句话说明
- **一键搭骨架**：LightProbeGroup / Reflection Probe / VRChat Light Volume + 清理
- **探针工具**：自动布点、增密/稀疏、平滑、可视化与统计
- **遮挡工具**：房间/门洞工作流、静态标记、烘焙前体检
- **LOD / 脚本**：批量 LODGroup、清理 Missing、Collider 小工具
- **Shader 工具 / 安装器**：识别 Standard → 批量替换目标 toon；常用 Shader 一键安装
- **光照设置**：预设 + **Bakery** 快捷项
- **顾问**：上传/烘焙前把你漏掉的未标记静态内容全部一键查出来
- **Udon 区域开关（可选）**：玩家进出时启用/禁用一批对象

### 安装（VCC / ALCOM）

**仓库链接**
```txt
https://assencypher.github.io/demonshop-vpm-lite/index.json
```

- **VCC**：Settings → Packages → Add Repository → 在项目里安装 **DemonPanel 1.1.2**
- **ALCOM**：设置 → 仓库 → 添加 → 在项目里安装 **1.1.2**
- 需要运行时 → Unity **Package Manager → DemonPanel → Samples → Import**

**CLI**
```bash
vrc-get repo add https://assencypher.github.io/demonshop-vpm-lite/index.json
vrc-get install com.demonshop.demonpanel 1.1.2
```

### 功能（编辑器）
- **光照一键骨架**：创建 LightProbeGroup（步长/合并/去重/可视化）、ReflectionProbe（尺寸预设）、VRChat Light Volume（自动补齐）；支持 **Bakery** 调参
- **探针工具**（`DP_ProbeTools.cs`） / **遮挡工具/房间**（`DP_OcclusionRooms.cs`, `DP_OcclusionTools.cs`）
- **LOD / 脚本**（`DP_LODAndScripts.cs`） / **Shader 工具 / 安装器**（`DP_ShaderTools.cs`, `DP_ShaderInstallers.cs`）
- **Bakery 辅助**（`DP_BakeryTools.cs`） / **顾问 / 优化器**（`DP_LAdvisor.cs`, `DP_LVOptimizer.cs`）
- **主面板与光照页**（`DemonPanelMainWindow.cs`, `DemonPanelLightSettingsWindow.cs`）
- **本地化 / UI**（`DP_Loc.cs`, `DP_UI.cs`）

### 运行时（可选）
- **区域开关样例**（`Samples~/Udon Area Toggle`）：基于 UdonSharp，玩家进入/离开触发对象启用/禁用。

### 扩展
引用 `DemonShop.DemonPanel.Editor.asmdef`，菜单放在 `DemonPanel/...`；公共工具见 `DP_Utils.cs`；欢迎 PR（使用 `DP_Loc.cs` 做多语言）。

### 许可与署名
- **许可**：完全开源，可自由修改，但**严禁商业化分发/售卖**
- **作者**：**E-Mommy 电子妈咪**、**limbo 新海美冬**
- 同步发布：<https://demonshop.booth.pm/items/7145807>（免费开源）

---

## 繁體中文

**DemonPanel** 是給 **VRChat 世界** 的 *究極作者工具箱*。
把那些又累人又容易出包的流程交給它：探針、遮擋、Shader、燈光/LOD、Collider 與腳本清理，另附 **Udon 區域切換** 執行範例（可選）。

### 重點先看
- **一鍵建好骨架**：LightProbeGroup / Reflection Probe / VRChat Light Volume + 清理雜物
- **探針工具**：自動佈點、加密/稀疏、平滑、視覺化與統計
- **遮擋工具**：房間/門洞工作流、靜態標記、烘焙前健檢
- **LOD / 腳本**：批次 LODGroup、清掉 Missing、Collider 小工具
- **Shader 工具 / 安裝器**：識別 Standard → 批量轉成指定 toon；常用 Shader 一鍵安裝
- **燈光設定**：合理預設 + **Bakery** 快速調整
- **顧問**：上傳/烘焙前的檢查清單
- **Udon 區域切換（可選）**：玩家進出時啟用/停用一批物件

### 安裝（VCC / ALCOM）

**倉庫連結**
```txt
https://assencypher.github.io/demonshop-vpm-lite/index.json
```

- **VCC**：Settings → Packages → Add Repository → 在專案安裝 **DemonPanel 1.1.2**
- **ALCOM**：設定 → 倉庫 → 新增 → 在專案安裝 **1.1.2**
- 需要執行範例 → Unity **Package Manager → DemonPanel → Samples → Import**

**CLI**
```bash
vrc-get repo add https://assencypher.github.io/demonshop-vpm-lite/index.json
vrc-get install com.demonshop.demonpanel 1.1.2
```

### 功能（編輯器）
- **燈光一鍵骨架**：建立 LightProbeGroup（步長/合併/去重/視覺化）、ReflectionProbe（尺寸預設）、VRChat Light Volume（自動補齊）；支援 **Bakery** 參數微調
- **探針工具**（`DP_ProbeTools.cs`） / **遮擋工具/房間**（`DP_OcclusionRooms.cs`, `DP_OcclusionTools.cs`）
- **LOD / 腳本**（`DP_LODAndScripts.cs`） / **Shader 工具 / 安裝器**（`DP_ShaderTools.cs`, `DP_ShaderInstallers.cs`）
- **Bakery 輔助**（`DP_BakeryTools.cs`） / **顧問 / 最佳化**（`DP_LAdvisor.cs`, `DP_LVOptimizer.cs`）
- **主面板與燈光頁**（`DemonPanelMainWindow.cs`, `DemonPanelLightSettingsWindow.cs`）
- **在地化 / UI**（`DP_Loc.cs`, `DP_UI.cs`）— 內建繁中

### 執行時（可選）
- **區域切換範例**（`Samples~/Udon Area Toggle`）：基於 UdonSharp，玩家進/出觸發物件啟用/停用。

### 擴充
引用 `DemonShop.DemonPanel.Editor.asmdef`，選單放在 `DemonPanel/...`；共用工具在 `DP_Utils.cs`。歡迎 PR（字串集中在 `DP_Loc.cs`）。

### 授權與署名
- **授權**：完全開源，可自由修改，但**嚴禁商業化散佈/販售**
- **作者**：**E-Mommy 电子妈咪**、**limbo 新海美冬**

---

## 日本語

**DemonPanel** は **VRChat ワールド** 制作用の *究極ツールボックス*。
面倒で壊れやすい作業（プローブ／オクルージョン／シェーダー／ライト&LOD／コライダー整理）を肩代わりし、任意の **Udon エリア切替** も同梱。

### インストール

**リポジトリ**
```txt
https://assencypher.github.io/demonshop-vpm-lite/index.json
```

- **VCC / ALCOM** に追加 → **DemonPanel 1.1.2** を導入
- ランタイムが必要なら Unity の **Samples** を Import

**機能**：Probe / Occlusion / LOD / Shader / Bakery / Advisor / Optimizer / Localization（EN/zh-CN/zh-TW/JA）。
**拡張**：`DemonShop.DemonPanel.Editor.asmdef` を参照、共通処理は `DP_Utils.cs`。
**ライセンス**：オープンソース／改変可／**商用不可**。作者：**E-Mommy 电子妈咪**、**limbo 新海美冬**。

---

## 한국어

> ⚠️ 현재 패키지에는 **한국어 UI가 포함되어 있지 않습니다.**
> 지원 언어: EN / 중국어 간체 / 중국어 번체 / 일본어.
> 한국어 번역을 제공하고 싶다면 Discord(<a href="https://discord.gg/PEcZZupngQ">초대 링크</a>)에서 연락해 주세요. 권한 확인 후 README/릴리즈 노트에 기여자 크레딧을 표기합니다.

**DemonPanel** 은 **VRChat 월드**를 위한 제작 도구입니다.
프로브/오클루전/셰이더/라이트·LOD/콜라이더 정리와 선택형 **Udon 영역 토글**을 제공합니다.

**리포지토리**
```txt
https://assencypher.github.io/demonshop-vpm-lite/index.json
```

VCC/ALCOM 에 리포 추가 후 **DemonPanel 1.1.2** 설치 → 필요 시 Unity **Samples** 에서 *Area Toggle* Import.
**라이선스**: 오픈소스, 수정 가능, **상업적 배포 금지**.
**저자**: **E-Mommy 电子妈咪**, **limbo 新海美冬**.
