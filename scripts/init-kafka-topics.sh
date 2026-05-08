#!/bin/bash
# Wait for kafka to be ready then create topics
sleep 10
kafka-topics --create --if-not-exists \
  --bootstrap-server kafka:29092 \
  --topic biletix.events --partitions 3 --replication-factor 1

kafka-topics --create --if-not-exists \
  --bootstrap-server kafka:29092 \
  --topic biletix.tickets --partitions 3 --replication-factor 1

kafka-topics --create --if-not-exists \
  --bootstrap-server kafka:29092 \
  --topic biletix.bookings --partitions 3 --replication-factor 1

kafka-topics --create --if-not-exists \
  --bootstrap-server kafka:29092 \
  --topic biletix.notifications --partitions 3 --replication-factor 1

kafka-topics --create --if-not-exists \
  --bootstrap-server kafka:29092 \
  --topic biletix.outbox --partitions 3 --replication-factor 1

echo "Topics created successfully"
