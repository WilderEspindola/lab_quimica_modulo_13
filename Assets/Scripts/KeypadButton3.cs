using UnityEngine;

public class KeypadButton3 : MonoBehaviour
{
    public KeypadLock3 keypadLock3; // Referencia exclusiva al KeypadLock3
    public string digitOrAction; // Ej: "5", "Clear", "Enter"

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
            keypadLock3.SaveCode();
        }
        else if (digitOrAction == "Clear")
        {
            keypadLock3.ClearCode();
        }
        else
        {
            keypadLock3.AddDigit(digitOrAction);
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