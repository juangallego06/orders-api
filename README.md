# 🧾 Orders API

API REST desarrollada en **.NET 9**, dockerizada, que recibe pedidos y los envía a RabbitMQ.

---

## 🚀 Tecnologías utilizadas

* .NET 9
* ASP.NET Core Web API
* Docker
* Docker Compose

---

## 📦 Requisitos

* Docker (versión 29+)
* Docker Compose (incluido en Docker Desktop)
* Git

---

## 📁 Estructura del proyecto

```
/orders-api
│
├── docker-compose.yml
├── docker-compose.override.yml
│
└── Orders.API/
    ├── Controllers/
    ├── Requests/
    ├── Responses/
    ├── Dockerfile
    └── Orders.API.csproj
```

---

## 🐳 Cómo ejecutar el proyecto

### 1. Clonar repositorio

```
git clone https://github.com/juangallego06/orders-api.git
cd orders-api
```

---

### 2. Ejecutar con Docker

```
docker compose up --build
```

---

### 3. API disponible en:

```
http://localhost:3000
```

---

## 🧪 Prueba de la API (POSTMAN)

### Endpoint:

```
POST http://localhost:3000/api/orders
```

### Body (JSON):

```
{
  "customerName": "Carlos",
  "product": "Libro de arquitectura",
  "quantity": 1
}
```

---

### ✅ Respuesta esperada:

```
{
  "status": "accepted",
  "message": "Pedido recibido y enviado a RabbitMQ",
  "orderId": "ORD-1001"
}
```

---

## ⚠️ Validaciones

La API valida automáticamente:

* Campos obligatorios
* Tipo de datos (`quantity` debe ser número)
* Valores válidos (no negativos)

Si el JSON es inválido, la API responde con `400 Bad Request`.

---

## 🔧 Configuración

Puerto expuesto:

```
3000
```

Variable de entorno usada:

```
ASPNETCORE_URLS=http://+:3000
```

---

## 🧹 Comandos útiles

Detener contenedores:

```
docker compose down
```

Reconstruir:

```
docker compose up --build
```

Ver logs:

```
docker logs orders-api
```

---

## 📌 Notas

* No requiere instalación de .NET local
* Docker maneja todo el entorno
* Ideal para pruebas y aprendizaje de APIs dockerizadas

---

## 👨‍💻 Autor

Juan Gallego
