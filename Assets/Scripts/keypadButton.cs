using UnityEngine;

public class KeypadButton : MonoBehaviour
{
    public KeypadLock keypadLock;
    public string digitOrAction;

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
            keypadLock.SaveCode();
        }
        else if (digitOrAction == "Clear")
        {
            keypadLock.ClearCode();
        }
        else
        {
            keypadLock.AddDigit(digitOrAction);
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