using UnityEngine;

public class KeypadButton5 : MonoBehaviour
{
    public KeypadLock5 keypadLock5; // Referencia única al KeypadLock5
    public string digitOrAction; // Ej: "3", "Clear", "Enter"

    [Header("Sonido")]
    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    private void Start()
    {
        // Obtener o crear AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void PressButton()
    {
        if (digitOrAction == "Enter")
        {
            keypadLock5.SaveCode();
        }
        else if (digitOrAction == "Clear")
        {
            keypadLock5.ClearCode();
        }
        else
        {
            keypadLock5.AddDigit(digitOrAction);
        }

        // REPRODUCIR SONIDO AL FINAL ← después de todas las acciones
        PlayClickSound();
    }

    private void PlayClickSound()
    {
        if (clickSound != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}