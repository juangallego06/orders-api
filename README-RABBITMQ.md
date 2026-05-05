# 🐰 ARQUITECTURA DE MENSAJERÍA CON RABBITMQ

Sistema completo de procesamiento de pedidos usando arquitectura basada en mensajería.

---

## 🏗️ ARQUITECTURA IMPLEMENTADA

```
┌─────────────────┐
│   Cliente HTTP  │
└────────┬────────┘
         │
         │ POST /api/orders
         ▼
┌─────────────────────────────────────────┐
│         ORDERS API (Productor)          │
│  ┌──────────────────────────────────┐  │
│  │  OrdersController                │  │
│  │  - Recibe pedido                 │  │
│  │  - Valida datos                  │  │
│  │  - Genera ID único               │  │
│  │  - Publica en RabbitMQ           │  │
│  └──────────────────────────────────┘  │
│  ┌──────────────────────────────────┐  │
│  │  RabbitMQProducer                │  │
│  │  - Conexión a RabbitMQ           │  │
│  │  - Declara exchange/cola         │  │
│  │  - Publica mensajes              │  │
│  └──────────────────────────────────┘  │
└─────────────────┬───────────────────────┘
                  │
                  │ Publish (order.created)
                  ▼
┌─────────────────────────────────────────┐
│           RABBITMQ BROKER               │
│  ┌──────────────────────────────────┐  │
│  │  Exchange: orders.exchange       │  │
│  │  Type: Direct                    │  │
│  └──────────────┬───────────────────┘  │
│                 │                       │
│  ┌──────────────▼───────────────────┐  │
│  │  Queue: pedidos                  │  │
│  │  - Almacena mensajes             │  │
│  │  - Durable: true                 │  │
│  └──────────────┬───────────────────┘  │
└─────────────────┼───────────────────────┘
                  │
                  │ Consume
                  ▼
┌─────────────────────────────────────────┐
│    NOTIFICATION WORKER (Consumidor)     │
│  ┌──────────────────────────────────┐  │
│  │  OrderConsumerService            │  │
│  │  - BackgroundService             │  │
│  │  - Consume mensajes              │  │
│  │  - Procesa pedido                │  │
│  │  - Simula:                       │  │
│  │    * Email de confirmación       │  │
│  │    * Actualización inventario    │  │
│  └──────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

---

## 📁 ESTRUCTURA DEL PROYECTO

```
/orders-api
│
├── 📁 Controllers/
│   └── OrdersController.cs          # Controlador REST API
│
├── 📁 Services/
│   ├── IRabbitMQProducer.cs         # Interfaz productor
│   └── RabbitMQProducer.cs          # Implementación productor
│
├── 📁 Models/
│   ├── Order.cs                     # Entidad de dominio
│   └── OrderMessage.cs              # DTO para RabbitMQ
│
├── 📁 Configuration/
│   └── RabbitMQSettings.cs          # Configuración RabbitMQ
│
├── 📁 Requests/
│   └── OrderRequest.cs              # DTO de entrada
│
├── 📁 Responses/
│   └── OrderResponse.cs             # DTO de salida
│
├── 📁 notification-worker/          # 🆕 Worker consumidor
│   ├── 📁 Services/
│   │   └── OrderConsumerService.cs  # BackgroundService consumidor
│   ├── 📁 Models/
│   │   └── OrderMessage.cs          # DTO para mensajes
│   ├── 📁 Configuration/
│   │   └── RabbitMQSettings.cs      # Configuración
│   ├── Program.cs                   # Punto de entrada
│   ├── appsettings.json             # Configuración producción
│   ├── appsettings.Development.json # Configuración desarrollo
│   ├── notification-worker.csproj   # Proyecto .NET
│   └── Dockerfile                   # Definición imagen
│
├── Program.cs                        # Configuración API
├── appsettings.json                  # Configuración producción
├── appsettings.Development.json      # Configuración desarrollo
├── Dockerfile                        # Imagen API
├── docker-compose.yml                # 🆕 Orquestación completa
└── test-orders.sh                    # Script de pruebas
```

---

## 🚀 CÓMO EJECUTAR

### Opción 1: Con Docker Compose (Recomendado)

```bash
# Iniciar todos los servicios
docker compose up --build

# En otra terminal, ejecutar pruebas
chmod +x test-orders.sh
./test-orders.sh

# Ver logs específicos
docker logs orders-api -f
docker logs notification-worker -f
docker logs rabbitmq -f

# Ver todos los logs
docker compose logs -f
```

### Opción 2: Modo Desarrollo

```bash
# Terminal 1: Iniciar RabbitMQ con Docker
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3.13-management

# Terminal 2: Ejecutar API
cd orders-api
dotnet run

# Terminal 3: Ejecutar Worker
cd notification-worker
dotnet run
```

---

## 🧪 PRUEBAS

### 1. Verificar RabbitMQ Management UI

```
URL: http://localhost:15672
Usuario: guest
Contraseña: guest
```

**Verificar:**
- ✅ Exchange `orders.exchange` creado
- ✅ Cola `pedidos` creada
- ✅ Binding entre exchange y cola

### 2. Enviar Pedidos

```bash
curl -X POST http://localhost:3000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Juan Pérez",
    "product": "Libro de Arquitectura",
    "quantity": 2
  }'
```

**Respuesta esperada:**
```json
{
  "status": "accepted",
  "message": "Pedido recibido y enviado a RabbitMQ",
  "orderId": "ORD-a1b2c3d4"
}
```

### 3. Script Automatizado

```bash
chmod +x test-orders.sh
./test-orders.sh
```

---

## 📊 EVIDENCIAS DE FUNCIONAMIENTO

### ✅ Evidencia 1: Cola Creada

**Desde RabbitMQ Management UI:**
1. Ir a http://localhost:15672
2. Navegar a "Queues"
3. Verificar que existe la cola `pedidos`
4. Verificar las estadísticas de la cola

**Logs esperados:**
```
[INFO] ✅ Exchange declarado: orders.exchange
[INFO] ✅ Cola declarada: pedidos
[INFO] ✅ Binding creado: pedidos -> orders.exchange con RK: order.created
```

### ✅ Evidencia 2: Mensaje Enviado

**Logs de la API (orders-api):**
```
[INFO] 📥 Nuevo pedido recibido: Juan Pérez - Libro de Arquitectura x2
[INFO] 📋 Orden creada con ID: a1b2c3d4-e5f6-7890-abcd-ef1234567890
[INFO] 📤 [INFO] Mensaje enviado a RabbitMQ: Pedido creado ID=a1b2c3d4
[INFO] 📦 Contenido del mensaje: {"orderId":"a1b2c3d4-...","customerName":"Juan Pérez",...}
[INFO] ✅ Pedido procesado exitosamente. ID=a1b2c3d4
```

### ✅ Evidencia 3: Mensaje en Cola

**Desde RabbitMQ Management UI:**
1. Ir a "Queues" → "pedidos"
2. Ver "Ready" = 1 (o más mensajes)
3. Click en "Get messages" para ver el contenido

**Mensaje en cola:**
```json
{
  "orderId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "customerName": "Juan Pérez",
  "product": "Libro de Arquitectura",
  "quantity": 2,
  "createdAt": "2025-01-15T10:30:00Z",
  "status": "pending"
}
```

### ✅ Evidencia 4: Consumidor Procesando

**Logs del Worker (notification-worker):**
```
[INFO] 🔧 Iniciando OrderConsumerService...
[INFO] 🔗 Conectando a RabbitMQ en rabbitmq:5672...
[INFO] ✅ Cola declarada: pedidos
[INFO] 👂 Iniciando consumo de mensajes de la cola 'pedidos'...
[INFO] 🎉 OrderConsumerService iniciado y esperando mensajes...

[INFO] 📨 [INFO] Mensaje recibido de RabbitMQ
[INFO] 📦 Contenido: {"orderId":"a1b2c3d4-...",...}
[INFO] 🔍 [INFO] Procesando pedido ID=a1b2c3d4
[INFO] 📋 Detalle del pedido:
[INFO]    - ID: a1b2c3d4-e5f6-7890-abcd-ef1234567890
[INFO]    - Cliente: Juan Pérez
[INFO]    - Producto: Libro de Arquitectura
[INFO]    - Cantidad: 2
[INFO]    - Fecha: 2025-01-15 10:30:00
[INFO]    - Estado: pending
[INFO] 📧 Simulando envío de email a cliente: Juan Pérez
[INFO] 📦 Simulando actualización de inventario para producto: Libro de Arquitectura
[INFO] ✅ [INFO] Pedido procesado correctamente ID=a1b2c3d4
```

---

## 🔧 CONFIGURACIÓN

### Puertos

| Servicio | Puerto | Propósito |
|----------|--------|-----------|
| orders-api | 3000 | API REST |
| rabbitmq | 5672 | AMQP |
| rabbitmq | 15672 | Management UI |

### Variables de Entorno

**API:**
- `RabbitMQ__HostName`: rabbitmq
- `RabbitMQ__Port`: 5672
- `RabbitMQ__UserName`: guest
- `RabbitMQ__Password`: guest
- `RabbitMQ__ExchangeName`: orders.exchange
- `RabbitMQ__QueueName`: pedidos
- `RabbitMQ__RoutingKey`: order.created

**Worker:**
- `RabbitMQ__HostName`: rabbitmq
- `RabbitMQ__Port`: 5672
- `RabbitMQ__QueueName`: pedidos

---

## 🎯 FLUJO COMPLETO

1. **Cliente** → POST `/api/orders` con datos del pedido
2. **API** → Valida datos y genera OrderId único
3. **API** → Publica mensaje en RabbitMQ
4. **RabbitMQ** → Almacena mensaje en cola `pedidos`
5. **Worker** → Consume mensaje de la cola
6. **Worker** → Procesa pedido (simula email, inventario)
7. **Worker** → Acknowledge del mensaje

---

## 🛠️ COMANDOS ÚTILES

```bash
# Iniciar servicios
docker compose up --build

# Detener servicios
docker compose down

# Ver logs
docker compose logs -f orders-api
docker compose logs -f notification-worker
docker compose logs -f rabbitmq

# Ver todos los logs
docker compose logs -f

# Reiniciar un servicio
docker compose restart orders-api

# Ver contenedores activos
docker ps

# Entrar a un contenedor
docker exec -it orders-api sh
docker exec -it notification-worker sh
docker exec -it rabbitmq sh

# Limpiar todo (incluyendo volúmenes)
docker compose down -v
```

---

## 📝 NOTAS IMPORTANTES

1. **Health Check**: RabbitMQ tiene un health check configurado para asegurar que esté listo antes de iniciar API y Worker
2. **Restart Policy**: Todos los contenedores se reinician automáticamente (`unless-stopped`)
3. **Datos Persistentes**: RabbitMQ usa un volumen para persistir mensajes
4. **Logging**: Sistema de logging estructurado con Serilog en todos los servicios
5. **OrderIds Únicos**: Cada pedido genera un Guid único

---

## 🎓 CONCEPTOS IMPLEMENTADOS

✅ **Productor/Consumidor**: Patrón mensajería asíncrona
✅ **BackgroundService**: Worker .NET de larga duración
✅ **Direct Exchange**: Routing básico de mensajes
✅ **Cola Duradera**: Mensajes persisten si RabbitMQ se reinicia
✅ **Manual Ack**: Control de procesamiento de mensajes
✅ **Quality of Service**: prefetchCount=1 para distribución equitativa
✅ **Dependency Injection**: Inyección de dependencias .NET
✅ **Configuration Pattern**: Settings externalizados
✅ **Structured Logging**: Logs con contexto y timestamp
✅ **Health Checks**: Verificación de dependencias

---

## 👨‍💻 AUTOR

Juan Gallego - 2025
