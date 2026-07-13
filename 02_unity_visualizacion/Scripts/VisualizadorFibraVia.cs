/*
 * VisualizadorFibraVia.cs
 * Gemelo Digital - Proyecto FibraVia
 *
 * Cliente MQTT basado en M2MqttUnity. Se suscribe al topico unico
 * solarpunk/fibravia/estado, parsea el JSON y actualiza color y UI:
 *   VERDE    -> deformacion < 2 mm
 *   AMARILLO -> deformacion 2-4 mm
 *   ROJO     -> deformacion > 4 mm
 */

using System;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;

[Serializable]
public class EstadoFibraVia
{
    public float deformacion;
    public float voltaje;
    public float soc_baterias;
    public float temp_mezcla;
    public long timestamp;
}

public class VisualizadorFibraVia : M2MqttUnityClient
{
    private const string TOPIC_ESTADO = "solarpunk/fibravia/estado";

    [Header("Umbrales de deformacion (mm)")]
    public float deformacionPrecaucion = 2.0f;
    public float deformacionCritica = 4.0f;

    [Header("Referencias de escena")]
    public Renderer objetoIndicador;
    public Text textoDeformacion;
    public Text textoVoltaje;
    public Text textoSocBaterias;
    public Text textoTempMezcla;

    private EstadoFibraVia estado = new EstadoFibraVia();

    protected override void Start()
    {
        brokerAddress = "broker.emqx.io";
        brokerPort = 1883;
        autoConnect = true;
        base.Start();
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(
            new string[] { TOPIC_ESTADO },
            new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
        Debug.Log("VisualizadorFibraVia: suscrito a " + TOPIC_ESTADO);
    }

    protected override void UnsubscribeTopics()
    {
        client.Unsubscribe(new string[] { TOPIC_ESTADO });
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        if (topic != TOPIC_ESTADO)
        {
            return;
        }

        string json = System.Text.Encoding.UTF8.GetString(message);
        try
        {
            estado = JsonUtility.FromJson<EstadoFibraVia>(json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("VisualizadorFibraVia: JSON invalido: " + ex.Message);
        }
    }

    protected override void Update()
    {
        base.Update();
        ActualizarColorEstado();
        ActualizarTextosUI();
    }

    private void ActualizarColorEstado()
    {
        if (objetoIndicador == null)
        {
            return;
        }

        Color colorEstado;

        if (estado.deformacion > deformacionCritica)
        {
            colorEstado = Color.red;
        }
        else if (estado.deformacion >= deformacionPrecaucion)
        {
            colorEstado = Color.yellow;
        }
        else
        {
            colorEstado = Color.green;
        }

        objetoIndicador.material.color = colorEstado;
    }

    private void ActualizarTextosUI()
    {
        if (textoDeformacion != null) textoDeformacion.text = $"Deformacion: {estado.deformacion:F2} mm";
        if (textoVoltaje != null) textoVoltaje.text = $"Voltaje: {estado.voltaje:F1} V";
        if (textoSocBaterias != null) textoSocBaterias.text = $"SoC baterias: {estado.soc_baterias:F0} %";
        if (textoTempMezcla != null) textoTempMezcla.text = $"Temp mezcla: {estado.temp_mezcla:F1} C";
    }
}
