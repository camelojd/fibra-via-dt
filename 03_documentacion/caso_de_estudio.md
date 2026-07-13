# Caso de Estudio: Gemelo Digital para Pavimento Inteligente con Generación Piezoeléctrica

**Proyecto:** FibraVia
**Dominio:** Infraestructura vial / Energy harvesting

---

## Desafío

Las vías terciarias en Colombia se deterioran a toda velocidad y, encima, muchas zonas rurales no tienen electrificación. El enfoque Solarpunk plantea una capa de rodadura de asfalto modificado con gránulos de caucho reciclado (GCR), con módulos piezoeléctricos integrados que convierten el paso de los carros en electricidad para luminarias y servicios rurales. Y aquí el reto es doble: por un lado detectar a tiempo la fatiga del pavimento (esa deformación que se va acumulando) antes de que toque reconstruir todo; y por el otro, vigilar cómo se genera y se almacena la energía.

## Solución

Lo que armé fue un **gemelo digital** de mantenimiento predictivo vial:

- Escribí un **simulador en Python** que publica cada segundo un JSON con la deformación del pavimento, el voltaje piezoeléctrico generado, el estado de carga de las baterías LFP y la temperatura de la mezcla asfáltica, todo al tópico MQTT `solarpunk/fibravia/estado`.
- Del otro lado, una escena de **Unity** (con el cliente M2MqttUnity) le aplica un mapa tipo semáforo: **verde** (deformación < 2 mm), **amarillo** (2 a 4 mm, toca programar intervención) y **rojo** (> 4 mm, intervención urgente), con un panel de variables en vivo.

## Tecnologías

- **Unity** (C#, uGUI) con **M2MqttUnity**
- **Python** (paho-mqtt)
- **MQTT** (broker `broker.emqx.io`) con mensajes **JSON**

## Resultados

- Terminé con un mapa del estado del pavimento en tiempo real y su semáforo de deterioro, que es como la versión digital de un mapa predictivo de mantenimiento vial.
- Lo mejor es que las alertas tempranas de fatiga me dejan hacer mantenimiento preventivo (sellar fisuras) en lugar de reconstruir; que al final es la decisión que más plata ahorra en gestión vial.
- Y en paralelo superviso la energía distribuida que se genera (el voltaje por módulo y el SoC de las baterías) y que alimenta las luminarias rurales.
