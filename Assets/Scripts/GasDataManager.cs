using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

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
    private float limitePresionGas = 100f;
    private float velocidadActualReporte = 0f;
    private float velocidadAnteriorReporte = 0f;
    private string tendenciaVelocidad = "";
    private float updateTimer = 0f;
    private const float updateInterval = 1f;
    private StringBuilder reportBuilder = new StringBuilder();

    // Velocidades de referencia FIJAS para cada gas
    private const float velocidadReferenciaHidrogeno = 8.00f;
    private const float velocidadReferenciaOxigeno = 4.00f;
    private const float velocidadReferenciaCO2 = 2.50f;

    // Variables para control de estado
    private string gasActivoAnterior = "";
    private bool primerReporte = true;

    private void Update()
    {
        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            ActualizarDatos();
            updateTimer = 0f;
        }
    }

    private void ActualizarDatos()
    {
        DetectarGasActivo();
        ActualizarLimitePresion();
        GestionarCambioDeGas();
        SimularCambioVelocidad();
        DeterminarTendencia();
        ActualizarReporte();
    }

    private void DetectarGasActivo()
    {
        nombreGasActivo = "";

        if (oxigenoController != null && oxigenoController.targetParticleSystem != null &&
            oxigenoController.targetParticleSystem.isPlaying)
        {
            nombreGasActivo = "OXÍGENO";
        }
        else if (hidrogenoController != null && hidrogenoController.targetParticleSystem != null &&
                 hidrogenoController.targetParticleSystem.isPlaying)
        {
            nombreGasActivo = "HIDRÓGENO";
        }
        else if (co2Controller != null && co2Controller.targetParticleSystem != null &&
                 co2Controller.targetParticleSystem.isPlaying)
        {
            nombreGasActivo = "DIÓXIDO DE CARBONO";
        }
    }

    private void GestionarCambioDeGas()
    {
        // Si cambió el gas activo, resetear la velocidad al valor inicial del nuevo gas
        if (nombreGasActivo != gasActivoAnterior && !string.IsNullOrEmpty(nombreGasActivo))
        {
            velocidadActualReporte = GetVelocidadReferenciaGas();
            primerReporte = false;
        }

        gasActivoAnterior = nombreGasActivo;
    }

    private void ActualizarLimitePresion()
    {
        if (botonSubir != null)
        {
            limitePresionGas = botonSubir.ObtenerLimiteActual();
        }
        else
        {
            limitePresionGas = 100f;
        }
    }

    private void SimularCambioVelocidad()
    {
        velocidadAnteriorReporte = velocidadActualReporte;

        // Solo simular cambios si hay un gas activo
        if (!string.IsNullOrEmpty(nombreGasActivo))
        {
            bool fuegoActivo = temperatureController != null && temperatureController.IsFireActive;
            float velocidadReferencia = GetVelocidadReferenciaGas();

            if (fuegoActivo)
            {
                // Aumentar velocidad cuando el fuego está activo
                velocidadActualReporte += 0.02f;
            }
            else if (velocidadActualReporte > velocidadReferencia)
            {
                // Disminuir velocidad cuando el fuego está apagado
                velocidadActualReporte -= 0.02f;
                // Nunca menos que la velocidad de referencia
                velocidadActualReporte = Mathf.Max(velocidadActualReporte, velocidadReferencia);
            }
            else if (primerReporte)
            {
                // Para el primer reporte, asegurar que empiece en la velocidad de referencia
                velocidadActualReporte = velocidadReferencia;
                primerReporte = false;
            }
        }
        else
        {
            // Si no hay gas activo, resetear a 0
            velocidadActualReporte = 0f;
        }
    }

    private void DeterminarTendencia()
    {
        if (velocidadActualReporte > velocidadAnteriorReporte)
        {
            tendenciaVelocidad = "▲"; // Aumentando
        }
        else if (velocidadActualReporte < velocidadAnteriorReporte)
        {
            tendenciaVelocidad = "▼"; // Disminuyendo
        }
        else
        {
            tendenciaVelocidad = "►"; // Estable
        }
    }

    private void ActualizarReporte()
    {
        if (textoReporte == null) return;

        float presion = botonSubir != null ? botonSubir.GetCurrentPressure() : 0f;
        float volumen = botonSubir != null ? botonSubir.GetCurrentVolume() : 0f;
        float temperatura = temperatureController != null ? temperatureController.GetCurrentTemperature() : 0f;

        textoReporte.enableAutoSizing = true;
        textoReporte.fontSizeMin = 1;
        textoReporte.fontSizeMax = 18;
        textoReporte.fontStyle = FontStyles.Bold;

        Color colorPresion = ObtenerColorSegunValor(presion, botonSubir.minPressure, botonSubir.maxPressure);
        Color colorVolumen = ObtenerColorSegunValor(volumen, botonSubir.minVolume, botonSubir.maxVolume, true);
        Color colorTemp = ObtenerColorSegunValor(temperatura, temperatureController.minTemperature, temperatureController.maxTemperature);
        Color colorVelocidad = ObtenerColorVelocidad(velocidadActualReporte);

        reportBuilder.Clear();

        // Encabezado con gas activo
        if (!string.IsNullOrEmpty(nombreGasActivo))
        {
            reportBuilder.AppendLine($"<b><color=#FF0000>GAS DE {nombreGasActivo} LIBERADO</color></b>");

        
        }
        else
        {
            reportBuilder.AppendLine("<color=#000000><b>NINGÚN GAS ACTIVO</b></color>");
        }

        // Datos del sistema
        reportBuilder.AppendLine("<color=#000000><b>DATOS DEL SISTEMA:</b></color>");
        reportBuilder.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(colorPresion)}><b>Límite de presión = {limitePresionGas:0} atm</b></color>");
        reportBuilder.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(colorPresion)}><b>Presión actual = {presion:0.00} atm</b></color>");
        reportBuilder.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(colorVolumen)}><b>Volumen = {volumen:0.000} m³</b></color>");
        reportBuilder.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(colorTemp)}><b>Temperatura = {temperatura:0} °K</b></color>");

        // Velocidad del reporte
        if (!string.IsNullOrEmpty(nombreGasActivo))
        {
            reportBuilder.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(colorVelocidad)}><b>Velocidad actual = {velocidadActualReporte:0.00} m/s {tendenciaVelocidad}</b></color>");
        }
        else
        {
            reportBuilder.AppendLine($"<color=#000000><b>Velocidad actual = 0.00 m/s</b></color>");
        }

        textoReporte.text = reportBuilder.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(textoReporte.rectTransform);
    }

    private float GetVelocidadReferenciaGas()
    {
        switch (nombreGasActivo)
        {
            case "HIDRÓGENO": return velocidadReferenciaHidrogeno;
            case "OXÍGENO": return velocidadReferenciaOxigeno;
            case "DIÓXIDO DE CARBONO": return velocidadReferenciaCO2;
            default: return 0f;
        }
    }

    private Color ObtenerColorVelocidad(float velocidad)
    {
        float velocidadReferencia = GetVelocidadReferenciaGas();

        if (velocidadReferencia <= 0 || velocidad <= 0) return colorNormal;

        // Calcular qué tan por encima está de la velocidad de referencia
        float ratio = velocidad / velocidadReferencia;

        if (ratio >= 2.0f) // Más del doble = PELIGRO
            return colorPeligro;
        else if (ratio >= 1.5f) // 50% más = ALERTA
            return colorAlerta;
        else if (Mathf.Approximately(velocidad, velocidadReferencia)) // Exactamente en referencia
            return new Color(1f, 1f, 0f); // Verde para velocidad referencia

        return colorNormal; // Negro para valores normales
    }

    private Color ObtenerColorSegunValor(float valor, float min, float max, bool esVolumen = false)
    {
        if (min >= max) return colorNormal;

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