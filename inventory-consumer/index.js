const { Kafka } = require('kafkajs');

const kafka = new Kafka({
  clientId: 'inventory-consumer',
  brokers: [process.env.KAFKA_BROKER || 'kafka:9092'],
  retry: { initialRetryTime: 3000, retries: 10 }
});

const consumer = kafka.consumer({
  groupId: process.env.KAFKA_GROUP_ID || 'inventory-service-group'
});

async function run() {
  console.log('[inventory-consumer] Conectando a Kafka...');
  await consumer.connect();
  console.log('[inventory-consumer] Conectado. Suscribiendo a orders.events...');

  await consumer.subscribe({
    topic: process.env.KAFKA_TOPIC || 'orders.events',
    fromBeginning: true
  });

  console.log('[inventory-consumer] Esperando eventos OrderCreated...');

  await consumer.run({
    eachMessage: async ({ topic, partition, message }) => {
      const event = JSON.parse(message.value.toString());
      console.log('==============================================');
      console.log('[inventory-consumer] Evento recibido:', event.eventType);
      console.log('[inventory-consumer] Order ID:', event.data.orderId);
      console.log('[inventory-consumer] Cliente:', event.data.customerName);
      console.log('[inventory-consumer] Producto:', event.data.product);
      console.log('[inventory-consumer] Cantidad:', event.data.quantity);
      console.log(`[inventory-consumer] topic=${topic} partition=${partition} offset=${message.offset}`);
      console.log('[inventory-consumer] Simulando reserva de inventario...');
      console.log(`[inventory-consumer] INVENTARIO RESERVADO para pedido ${event.data.orderId}`);
      console.log('==============================================');
    }
  });
}

run().catch(err => {
  console.error('[inventory-consumer] Error fatal:', err);
  process.exit(1);
});
