using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GasDataManager : MonoBehaviour
{
    [Header("Componentes de Control")]
    public DualPartButton botonSubir;
    public DualPartButton botonBajar;
    public ParticleControllerFire temperatureController;

    [Header("Controladores de Gas")]
    public ParticleOxigeno oxigenoController;
    public ParticleHidrogeno hidrogenoController;
    public ParticleDioxiodoCarbono co2Controller;

    [Header("UI de Reporte")]
    public TextMeshProUGUI textoReporte;

    [Header("Configuración de Colores")]
    public Color colorNormal = Color.black;
    public Color colorAlerta = new Color(1f, 0.5f, 0f);
    public Color colorPeligro = Color.red;
    [Range(0.5f, 0.9f)] public float umbralAlerta = 0.7f;
    [Range(0.8f, 1f)] public float umbralPeligro = 0.9f;

    private string nombreGasActivo = "";
    private float limitePresionGas = 100f; // Valor por defecto (vacío)

    private void Update()
    {
        ActualizarReporte();
        DetectarGasActivo();
        ActualizarLimitePresion(); // Nueva función
    }

    private void DetectarGasActivo()
    {
        if (oxigenoController.targetParticleSystem.isPlaying)
        {
            nombreGasActivo = "OXÍGENO";
        }
        else if (hidrogenoController.targetParticleSystem.isPlaying)
        {
            nombreGasActivo = "HIDRÓGENO";
        }
        else if (co2Controller.targetParticleSystem.isPlaying)
        {
            nombreGasActivo = "DIÓXIDO DE CARBONO";
        }
        else
        {
            nombreGasActivo = "";
        }
    }

    private void ActualizarLimitePresion()
    {
        if (botonSubir != null)
        {
            // Usar el método que ya existe en DualPartButton para obtener el límite
            limitePresionGas = botonSubir.ObtenerLimiteActual();
        }
        else
        {
            limitePresionGas = 100f; // Valor por defecto
        }
    }

    private void ActualizarReporte()
    {
        if (textoReporte == null) return;

        float presion = botonSubir.GetCurrentPressure();
        float volumen = botonSubir.GetCurrentVolume();
        float temperatura = temperatureController.GetCurrentTemperature();

        textoReporte.enableAutoSizing = true;
        textoReporte.fontSizeMin = 1;
        textoReporte.fontSizeMax = 18;
        textoReporte.fontStyle = FontStyles.Bold;

        Color colorPresion = ObtenerColorSegunValor(presion, botonSubir.minPressure, botonSubir.maxPressure);
        Color colorVolumen = ObtenerColorSegunValor(volumen, botonSubir.minVolume, botonSubir.maxVolume, true);
        Color colorTemp = ObtenerColorSegunValor(temperatura, temperatureController.minTemperature, temperatureController.maxTemperature);

        string encabezado = string.IsNullOrEmpty(nombreGasActivo)
            ? "<color=#000000><b>Datos del Sistema</b></color>\n"
            : $"<b>GAS DE {nombreGasActivo} LIBERADO</b>\n";

        textoReporte.text = encabezado +
                           
                           $"<color=#000000><b>Datos del Sistema:</b></color>\n" +
                           $"<color=#{ColorUtility.ToHtmlStringRGB(colorPresion)}><b>Límite de presión del Cilindro= {limitePresionGas:0} (atm)</b></color>\n" +
                           $"<color=#{ColorUtility.ToHtmlStringRGB(colorPresion)}><b>Presión = {presion:0.00} (atm)</b></color>\n" +
                           $"<color=#{ColorUtility.ToHtmlStringRGB(colorVolumen)}><b>Volumen = {volumen:0.000} (m³)</b></color>\n" +
                           $"<color=#{ColorUtility.ToHtmlStringRGB(colorTemp)}><b>Temperatura = {temperatura:0} (°K)</b></color>";

        LayoutRebuilder.ForceRebuildLayoutImmediate(textoReporte.rectTransform);
    }

    private Color ObtenerColorSegunValor(float valor, float min, float max, bool esVolumen = false)
    {
        float normalizado = Mathf.InverseLerp(min, max, valor);

        if (esVolumen)
        {
            normalizado = 1 - normalizado;
        }

        if (normalizado >= umbralPeligro)
            return colorPeligro;
        else if (normalizado >= umbralAlerta)
            return colorAlerta;

        return colorNormal;
    }
}