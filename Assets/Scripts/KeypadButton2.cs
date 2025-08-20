using UnityEngine;

public class KeypadButton2 : MonoBehaviour
{
    public KeypadLock2 keypadLock2; // Referencia al KeypadLock2 específico
    public string digitOrAction; // Ej: "1", "Clear", "Enter"

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
            keypadLock2.SaveCode();
        }
        else if (digitOrAction == "Clear")
        {
            keypadLock2.ClearCode();
        }
        else
        {
            keypadLock2.AddDigit(digitOrAction);
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