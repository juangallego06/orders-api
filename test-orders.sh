#!/bin/bash

# Colores para output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

API_URL="http://localhost:3000/api/orders"

echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}  ORDERS API - SCRIPT DE PRUEBA${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo ""

# Test 1: Pedido válido
echo -e "${GREEN}📤 Test 1: Enviando pedido válido...${NC}"
curl -X POST "$API_URL" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Juan Pérez",
    "product": "Libro de Arquitectura Limpia",
    "quantity": 2
  }' | jq '.'

echo -e "\n${YELLOW}⏳ Esperando 3 segundos...${NC}"
sleep 3

echo ""
echo -e "${GREEN}═══════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}📤 Test 2: Enviando segundo pedido...${NC}"
curl -X POST "$API_URL" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "María González",
    "product": "Curso de .NET 9",
    "quantity": 1
  }' | jq '.'

echo -e "\n${YELLOW}⏳ Esperando 3 segundos...${NC}"
sleep 3

echo ""
echo -e "${GREEN}═══════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}📤 Test 3: Enviando tercer pedido...${NC}"
curl -X POST "$API_URL" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Carlos López",
    "product": "Licencia de Visual Studio",
    "quantity": 5
  }' | jq '.'

echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}✅ Tests completados${NC}"
echo -e "${BLUE}📊 Revisa los logs de los contenedores para ver el procesamiento${NC}"
echo -e "${BLUE}🔗 Panel de RabbitMQ: http://localhost:15672 (guest/guest)${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
