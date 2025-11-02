# Changelog
## 1.1.3 - small fix
- Fixed Script not calculating triangles total when selecting a main gameObject.

## 1.1.2 — First public release 🎉
**Hello world.** This is the first officially announced build of **DemonPanel**.
It’s the “creator-side toolbox” for VRChat worlds: lighting scaffolding, probes, occlusion,
LOD/script utilities, shader helpers, an advisor, and an optional Udon Area Toggle sample.

**Highlights**
- **VPM repo ready** — Works with VCC, ALCOM (vrc-get GUI), and CLI.  
  Repository: `https://assencypher.github.io/demonshop-vpm-lite/index.json`
- **Runtime moved to Samples** — The Udon Area Toggle is an optional sample; import it only if you need it.
- **Localization** — Built-in UI strings for **English / Simplified Chinese / Traditional Chinese / Japanese**.  
  Strings live in `Editor/DP_Loc.cs`. Want to add a language? Ping us on Discord and we’ll credit you.
- **No hard SDK pin** — Depends on `com.vrchat.worlds` **3.x** to avoid unnecessary downgrades.
- **Unity** — Targeted at 2022.3 LTS.
- **Lots of sanding & labeling** — Cleaned up menu structure, tooltips, and first‑run defaults.
- **License** — Open-source, modification allowed, **strictly non‑commercial** redistribution.

**Authors**: E‑Mommy 电子妈咪, limbo 新海美冬

---

## 1.0.9
- Moved Udon runtime to **Samples~**. Import sample to Assets to use the Area toggle.
- Editor tools remain as package assemblies. No UdonSharp dependency at package compile time.


##1.1.4
-Fixed manifest errors

