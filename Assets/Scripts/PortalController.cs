using UnityEngine;

public class PortalController : MonoBehaviour
{
    [Header("Portal Effects")]
    [SerializeField] private GameObject portalEffect;
    [SerializeField] private Animator portalAnimator;
    [SerializeField] private AudioSource portalSound;
    
    private nextMap nextMapScript;
    
    void Start()
    {
        nextMapScript = GetComponent<nextMap>();
        SetPortalActive(false);
    }
    
    void Update()
    {
        bool shouldBeActive = !nextMapScript || BossManager.IsBossDefeated;
        SetPortalActive(shouldBeActive);
    }
    
    private void SetPortalActive(bool active)
    {
        if (portalEffect != null)
            portalEffect.SetActive(active);
            
        if (portalAnimator != null)
            portalAnimator.enabled = active;
            
        if (portalSound != null && active && !portalSound.isPlaying)
            portalSound.Play();
        else if (portalSound != null && !active)
            portalSound.Stop();
    }
}