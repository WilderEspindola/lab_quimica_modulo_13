using UnityEngine;
using TMPro;
using System.Collections;

public class KeypadLock5 : MonoBehaviour
{
    [Header("Keypad 5 Config")]
    public TextMeshPro passCodeDisplay;
    public GameObject[] keyButtons;

    private string currentInput = "";  // Cambiado de currentCode a currentInput
    private int savedValue = 1;       // Cambiado de string savedCode="?" a int con valor inicial 1

    // Evento estático (modificado para enviar int en lugar de string)
    public static System.Action<char, int> OnKeypadValueChanged;
    public char associatedLetter = 'E';

    [Header("Sonido")] // ← AÑADIR ESTO
    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        passCodeDisplay.text = savedValue.ToString();  // Mostrar el valor numérico inicial
        SetKeypadVisible(false);
        // Configurar AudioSource ← AÑADIR ESTO
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    private void PlayClickSound()
    {
        if (clickSound != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }


    public void AddDigit(string digit)
    {
        if (currentInput.Length < 2)  // Limitar a 2 dígitos (para coeficientes 1-12)
            currentInput += digit;

        passCodeDisplay.text = currentInput;
    }

    public void SaveCode()
    {
        if (!string.IsNullOrEmpty(currentInput))
        {
            savedValue = int.Parse(currentInput);
            passCodeDisplay.text = savedValue.ToString();
            OnKeypadValueChanged?.Invoke(associatedLetter, savedValue);
        }
        currentInput = "";

        // Retrasar la desactivación para permitir que el sonido se reproduzca
        StartCoroutine(DeactivateKeypadWithDelay());
    }

    private IEnumerator DeactivateKeypadWithDelay()
    {
        yield return new WaitForSeconds(0.2f); // Tiempo suficiente para el sonido
        SetKeypadVisible(false);
    }


    // --- Métodos que NO cambiaron ---
    public void ToggleKeypad()
    {
        // Reproducir sonido al abrir/cerrar el keypad
        PlayClickSound();
        bool newState = !keyButtons[0].activeSelf;
        SetKeypadVisible(newState);
        if (newState) currentInput = "";
    }

    private void SetKeypadVisible(bool visible)
    {
        foreach (var button in keyButtons)
            button.SetActive(visible);

        passCodeDisplay.text = visible ? currentInput : savedValue.ToString();
    }

    public void ClearCode()
    {
        currentInput = "";
        passCodeDisplay.text = savedValue.ToString();
    }
}