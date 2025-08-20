#if UDONSHARP
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AreaRuntimeToggle : UdonSharpBehaviour
{
    [Tooltip("Targets to enable while player is inside, disable when outside.")]
    public GameObject[] targets;

    [Tooltip("Start with targets disabled (recommended).")]
    public bool startDisabled = true;

    // 0=SetActive,1=Renderers.enabled,2=Light.enabled,3=AudioSource.enabled,4=ParticleSystem.Play/Stop
    [Tooltip("0=SetActive, 1=Renderers, 2=Light, 3=Audio, 4=Particles")]
    public int mode = 0;

    [Tooltip("Debounce seconds to avoid rapid toggling.")]
    public float debounce = 0.1f;

    private float _nextAllowed = 0f;
    private bool _inside = false;

    void Start()
    {
        if (startDisabled) Apply(false);
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player) || !player.isLocal) return;
        if (Time.time < _nextAllowed) return;
        _nextAllowed = Time.time + debounce;
        _inside = true;
        Apply(true);
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player) || !player.isLocal) return;
        if (Time.time < _nextAllowed) return;
        _nextAllowed = Time.time + debounce;
        _inside = false;
        Apply(false);
    }

    private void Apply(bool on)
    {
        if (targets == null) return;
        var self = this.gameObject;
        for (int i = 0; i < targets.Length; i++)
        {
            var go = targets[i];
            if (!Utilities.IsValid(go) || go == self) continue;

            switch (mode)
            {
                default: // SetActive
                    go.SetActive(on);
                    break;
                case 1: // Renderers
                    var rends = go.GetComponentsInChildren<Renderer>(true);
                    for (int r=0;r<rends.Length;r++) if (Utilities.IsValid(rends[r])) rends[r].enabled = on;
                    break;
                case 2: // Lights
                    var ls = go.GetComponentsInChildren<Light>(true);
                    for (int l=0;l<ls.Length;l++) if (Utilities.IsValid(ls[l])) ls[l].enabled = on;
                    break;
                case 3: // Audio
                    var auds = go.GetComponentsInChildren<AudioSource>(true);
                    for (int a=0;a<auds.Length;a++) if (Utilities.IsValid(auds[a])) auds[a].enabled = on;
                    break;
                case 4: // Particles
                    var ps = go.GetComponentsInChildren<ParticleSystem>(true);
                    for (int p=0;p<ps.Length;p++)
                    {
                        var sys = ps[p]; if (!Utilities.IsValid(sys)) continue;
                        if (on) sys.Play(true); else sys.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                    break;
            }
        }
    }
}
#else
// UdonSharp not installed: leave file empty to avoid compile errors.
#endif
