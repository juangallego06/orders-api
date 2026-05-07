const { Kafka } = require('kafkajs');

const kafka = new Kafka({
  clientId: 'billing-consumer',
  brokers: [process.env.KAFKA_BROKER || 'kafka:9092'],
  retry: { initialRetryTime: 3000, retries: 10 }
});

const consumer = kafka.consumer({
  groupId: process.env.KAFKA_GROUP_ID || 'billing-service-group'
});

async function run() {
  console.log('[billing-consumer] Conectando a Kafka...');
  await consumer.connect();
  console.log('[billing-consumer] Conectado. Suscribiendo a orders.events...');

  await consumer.subscribe({
    topic: process.env.KAFKA_TOPIC || 'orders.events',
    fromBeginning: true
  });

  console.log('[billing-consumer] Esperando eventos OrderCreated...');

  await consumer.run({
    eachMessage: async ({ topic, partition, message }) => {
      const event = JSON.parse(message.value.toString());
      const total = event.data.quantity * (event.data.unitPrice || 0);
      console.log('==============================================');
      console.log('[billing-consumer] Evento recibido:', event.eventType);
      console.log('[billing-consumer] Order ID:', event.data.orderId);
      console.log('[billing-consumer] Cliente:', event.data.customerName);
      console.log('[billing-consumer] Total a facturar: $', total);
      console.log(`[billing-consumer] topic=${topic} partition=${partition} offset=${message.offset}`);
      console.log('[billing-consumer] Simulando generacion de factura...');
      console.log(`[billing-consumer] FACTURA GENERADA FAC-${Date.now()} para pedido ${event.data.orderId}`);
      console.log('==============================================');
    }
  });
}

run().catch(err => {
  console.error('[billing-consumer] Error fatal:', err);
  process.exit(1);
});