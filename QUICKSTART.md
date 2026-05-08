# 🚀 GUÍA RÁPIDA DE EJECUCIÓN

## ⚡ INICIO RÁPIDO (3 pasos)

### 1. Iniciar servicios
```bash
docker compose up --build
```

### 2. Verificar que todo esté corriendo
```bash
docker ps
```

Deberías ver 3 contenedores:
- orders-api
- notification-worker
- rabbitmq

### 3. Ejecutar pruebas
```bash
chmod +x test-orders.sh
./test-orders.sh
```

---

## 📊 VERIFICACIÓN VISUAL

### Panel de RabbitMQ
```
URL: http://localhost:15672
Usuario: guest
Password: guest
```

**Navega a:**
1. Queues → `pedidos` (ver cola creada)
2. Exchanges → `orders.exchange` (ver exchange creado)
3. Click en "Get messages" para ver mensajes

---

## 📝 VER LOGS EN TIEMPO REAL

### Todos los servicios:
```bash
docker compose logs -f
```

### Solo API:
```bash
docker logs orders-api -f
```

### Solo Worker:
```bash
docker logs notification-worker -f
```

### Solo RabbitMQ:
```bash
docker logs rabbitmq -f
```

---

## 🎯 LO QUE DEBERÍAS VER

### En la API:
```
[INFO] 📥 Nuevo pedido recibido: Juan Pérez - Libro x2
[INFO] 📤 Mensaje enviado a RabbitMQ: Pedido creado ID=xxx
[INFO] ✅ Pedido procesado exitosamente
```

### En el Worker:
```
[INFO] 📨 Mensaje recibido de RabbitMQ
[INFO] 🔍 Procesando pedido ID=xxx
[INFO] ✅ Pedido procesado correctamente
```

---

## 🛑 DETENER SERVICIOS

```bash
docker compose down
```

### Con limpieza completa (incluyendo datos):
```bash
docker compose down -v
```

---

## 🔄 REINICIAR SERVICIOS

```bash
docker compose restart
```

### Servicio específico:
```bash
docker compose restart orders-api
```

---

## 🐛 SOLUCIÓN DE PROBLEMAS

### Puerto ya en uso
```bash
# Ver qué está usando el puerto
netstat -an | grep 3000
netstat -an | grep 5672
netstat -an | grep 15672

# Cambiar puertos en docker-compose.yml si es necesario
```

### Contenedor no inicia
```bash
# Ver logs de error
docker logs orders-api
docker logs notification-worker
docker logs rabbitmq

# Reconstruir desde cero
docker compose down -v
docker compose up --build --force-recreate
```

### RabbitMQ Management no accessible
```bash
# Verificar que el contenedor está corriendo
docker ps | grep rabbitmq

# Ver logs
docker logs rabbitmq

# Esperar unos segundos (RabbitMQ tarda en iniciar)
# Luego ir a http://localhost:15672
```

---

## 📖 MÁS INFORMACIÓN

- [README.md](README.md) - Información general
- [README-RABBITMQ.md](README-RABBITMQ.md) - Documentación completa de la arquitectura
