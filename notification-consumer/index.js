const { Kafka } = require('kafkajs');

const kafka = new Kafka({
  clientId: 'notification-consumer',
  brokers: [process.env.KAFKA_BROKER || 'kafka:9092'],
  retry: { initialRetryTime: 3000, retries: 10 }
});

const consumer = kafka.consumer({
  groupId: process.env.KAFKA_GROUP_ID || 'notification-service-group'
});

async function run() {
  console.log('[notification-consumer] Conectando a Kafka...');
  await consumer.connect();
  console.log('[notification-consumer] Conectado. Suscribiendo a orders.events...');

  await consumer.subscribe({
    topic: process.env.KAFKA_TOPIC || 'orders.events',
    fromBeginning: true
  });

  console.log('[notification-consumer] Esperando eventos OrderCreated...');

  await consumer.run({
    eachMessage: async ({ topic, partition, message }) => {
      const event = JSON.parse(message.value.toString());
      console.log('==============================================');
      console.log('[notification-consumer] Evento recibido:', event.eventType);
      console.log('[notification-consumer] Order ID:', event.data.orderId);
      console.log('[notification-consumer] Notificando a:', event.data.customerName);
      console.log('[notification-consumer] Producto:', event.data.product);
      console.log(`[notification-consumer] topic=${topic} partition=${partition} offset=${message.offset}`);
      console.log('[notification-consumer] Simulando envio de notificacion al cliente...');
      console.log(`[notification-consumer] NOTIFICACION ENVIADA a ${event.data.customerName} - pedido ${event.data.orderId}`);
      console.log('==============================================');
    }
  });
}

run().catch(err => {
  console.error('[notification-consumer] Error fatal:', err);
  process.exit(1);
});