# 🧾 Orders API con RabbitMQ

Sistema completo de procesamiento de pedidos usando **arquitectura basada en mensajería** con RabbitMQ en **.NET 9**.

---

## 🎯 ARQUITECTURA IMPLEMENTADA

```
Cliente HTTP → Orders API → RabbitMQ → Notification Worker
   ↓              ↓              ↓            ↓
POST          Productor      Broker      Consumidor
```

**📘 [Documentación completa de la arquitectura](README-RABBITMQ.md)**

---

## 🚀 Tecnologías utilizadas

* **.NET 9** - Última versión
* **ASP.NET Core Web API** - API REST
* **RabbitMQ** - Message Broker (con Management Plugin)
* **BackgroundService** - Worker consumidor
* **Docker** - Contenedorización
* **Docker Compose** - Orquestación
* **Serilog** - Logging estructurado

---

## 🏗️ COMPONENTES

### 1. **Orders API** (Productor)
- Recibe pedidos vía HTTP POST
- Valida datos
- Genera OrderId único
- Publica mensajes en RabbitMQ
- **Puerto**: 3000

### 2. **Notification Worker** (Consumidor)
- BackgroundService .NET
- Consume mensajes de RabbitMQ
- Procesa pedidos (simula email, inventario)
- Acknowledgment automático

### 3. **RabbitMQ** (Broker)
- Message Broker con Management UI
- Exchange: `orders.exchange`
- Cola: `pedidos`
- **Puertos**: 5672 (AMQP), 15672 (Management)

---

## 📦 Requisitos

* Docker (versión 20+)
* Docker Compose
* Git
* curl o Postman para pruebas

---

## 🐳 CÓMO EJECUTAR

### Iniciar todos los servicios

```bash
# Clonar repositorio
git clone https://github.com/juangallego06/orders-api.git
cd orders-api

# Iniciar servicios
docker compose up --build
```

### Servicios disponibles

| Servicio | URL | Credenciales |
|----------|-----|--------------|
| Orders API | http://localhost:3000 | - |
| RabbitMQ Management | http://localhost:15672 | guest/guest |

---

## 🧪 PRUEBAS

### Opción 1: Script automatizado

```bash
chmod +x test-orders.sh
./test-orders.sh
```

### Opción 2: curl manual

```bash
curl -X POST http://localhost:3000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Juan Pérez",
    "product": "Libro de Arquitectura",
    "quantity": 2
  }'
```

### Opción 3: Postman

**Endpoint**: `POST http://localhost:3000/api/orders`

**Body**:
```json
{
  "customerName": "Juan Pérez",
  "product": "Libro de Arquitectura",
  "quantity": 2
}
```

**Respuesta esperada**:
```json
{
  "status": "accepted",
  "message": "Pedido recibido y enviado a RabbitMQ",
  "orderId": "ORD-A1B2C3D4"
}
```

---

## 📊 EVIDENCIAS DE FUNCIONAMIENTO

### ✅ 1. Cola Creada

**RabbitMQ Management UI** → Queues → `pedidos`

### ✅ 2. Mensaje Enviado

**Logs de orders-api**:
```
[INFO] 📤 Mensaje enviado a RabbitMQ: Pedido creado ID=xxx
```

### ✅ 3. Mensaje en Cola

**RabbitMQ Management UI** → Queues → `pedidos` → Get messages

### ✅ 4. Consumidor Procesando

**Logs de notification-worker**:
```
[INFO] 📨 Mensaje recibido de RabbitMQ
[INFO] 🔍 Procesando pedido ID=xxx
[INFO] ✅ Pedido procesado correctamente ID=xxx
```

---

## 📁 ESTRUCTURA DEL PROYECTO

```
/orders-api
│
├── Controllers/           # Controladores API
├── Services/              # Productor RabbitMQ
├── Models/                # Entidades y DTOs
├── Configuration/         # Settings
├── Requests/              # DTOs de entrada
├── Responses/             # DTOs de salida
│
├── notification-worker/   # 🆕 Worker consumidor
│   ├── Services/          # BackgroundService
│   ├── Models/            # DTOs
│   └── Configuration/     # Settings
│
├── docker-compose.yml     # Orquestación completa
├── Dockerfile             # Imagen API
├── test-orders.sh         # Script de pruebas
├── README.md              # Este archivo
└── README-RABBITMQ.md     # Documentación detallada
```

---

## 🧹 COMANDOS ÚTILES

```bash
# Ver logs de todos los servicios
docker compose logs -f

# Ver logs específicos
docker logs orders-api -f
docker logs notification-worker -f
docker logs rabbitmq -f

# Detener servicios
docker compose down

# Detener y limpiar volúmenes
docker compose down -v

# Reiniciar un servicio
docker compose restart orders-api

# Ver contenedores activos
docker ps
```

---

## 📝 NOTAS IMPORTANTES

✅ Implementación completa de arquitectura de mensajería
✅ OrderIds únicos (Guid)
✅ Logging estructurado con Serilog
✅ Health checks para dependencias
✅ Datos persistentes en RabbitMQ
✅ Reconexión automática a RabbitMQ
✅ Manejo de errores y acknowledgment

---

## 📘 DOCUMENTACIÓN COMPLETA

Para más detalles sobre la arquitectura, flujo de datos y evidencias, consulta:

**[README-RABBITMQ.md](README-RABBITMQ.md)**

---

## 👨‍💻 AUTOR

Juan Gallego
