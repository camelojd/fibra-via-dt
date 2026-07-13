"""
Simulador de datos - Proyecto FibraVia
Gemelo Digital de pavimento con caucho reciclado (GCR) y modulos
piezoelectricos para generacion electrica distribuida.

Publica un JSON unico cada segundo al topico solarpunk/fibravia/estado
con deformacion del pavimento, voltaje generado, soc_baterias,
temp_mezcla y timestamp.
"""

import time
import math
import json
import random

import paho.mqtt.client as mqtt

BROKER = "broker.emqx.io"
PORT = 1883
CLIENT_ID = "fibravia_simulador"

TOPIC_ESTADO = "solarpunk/fibravia/estado"

# --- Rangos de las variables ---
DEFORMACION_MIN, DEFORMACION_MAX = 0.0, 6.0    # mm (verde <2, amarillo 2-4, rojo >4)
VOLTAJE_MIN, VOLTAJE_MAX = 20.0, 120.0         # V pico por modulo (doc: 20-60 liviano, 80-120 pesado)
SOC_MIN, SOC_MAX = 20.0, 100.0                 # % baterias LFP
TEMP_MEZCLA_MIN, TEMP_MEZCLA_MAX = 10.0, 35.0  # C ciclo dia/noche

INTERVALO_SEGUNDOS = 1


def onda_suave(t, minimo, maximo, periodo_segundos, ruido=0.0):
    """Onda seno acotada + ruido, imitando un sensor real."""
    amplitud = (maximo - minimo) / 2
    centro = minimo + amplitud
    valor = centro + amplitud * math.sin(2 * math.pi * t / periodo_segundos)
    valor += random.uniform(-ruido, ruido)
    return max(minimo, min(maximo, valor))


def main():
    client = mqtt.Client(callback_api_version=mqtt.CallbackAPIVersion.VERSION2, client_id=CLIENT_ID)
    client.connect(BROKER, PORT, keepalive=60)
    client.loop_start()

    print(f"Conectado a {BROKER}:{PORT}. Publicando JSON en {TOPIC_ESTADO} ...")

    t = 0
    try:
        while True:
            deformacion = onda_suave(t, DEFORMACION_MIN, DEFORMACION_MAX, periodo_segundos=240, ruido=0.15)
            voltaje = onda_suave(t, VOLTAJE_MIN, VOLTAJE_MAX, periodo_segundos=120, ruido=3.0)
            soc_baterias = onda_suave(t, SOC_MIN, SOC_MAX, periodo_segundos=600, ruido=0.5)
            temp_mezcla = onda_suave(t, TEMP_MEZCLA_MIN, TEMP_MEZCLA_MAX, periodo_segundos=300, ruido=0.3)

            payload = {
                "deformacion": round(deformacion, 2),
                "voltaje": round(voltaje, 2),
                "soc_baterias": round(soc_baterias, 2),
                "temp_mezcla": round(temp_mezcla, 2),
                "timestamp": int(time.time()),
            }

            client.publish(TOPIC_ESTADO, json.dumps(payload))

            print(
                f"[t={t:04d}s] Deformacion={deformacion:.2f}mm  Voltaje={voltaje:.2f}V  "
                f"SoC={soc_baterias:.2f}%  TempMezcla={temp_mezcla:.2f}C"
            )

            t += INTERVALO_SEGUNDOS
            time.sleep(INTERVALO_SEGUNDOS)
    except KeyboardInterrupt:
        print("\nSimulador detenido por el usuario.")
    finally:
        client.loop_stop()
        client.disconnect()


if __name__ == "__main__":
    main()
