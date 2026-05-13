#!/bin/bash

# Registers the Debezium PostgreSQL connector that streams Biletix table changes to Kafka topics.
curl -X POST http://localhost:8083/connectors \
  -H "Content-Type: application/json" \
  -d '{
    "name": "biletix-postgres-connector",
    "config": {
      "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
      "database.hostname": "postgres",
      "database.port": "5433",
      "database.user": "biletix",
      "database.password": "biletix123",
      "database.dbname": "biletix",
      "database.server.name": "biletix",
      "plugin.name": "pgoutput",
      "slot.name": "debezium_biletix",
      "publication.name": "debezium_publication",
      "publication.autocreate.mode": "filtered",
      "table.include.list": "public.Events,public.Venues,public.Performers,public.TicketTypes",
      "topic.prefix": "biletix",
      "transforms": "unwrap",
      "transforms.unwrap.type": "io.debezium.transforms.ExtractNewRecordState",
      "transforms.unwrap.drop.tombstones": "false",
      "transforms.unwrap.delete.handling.mode": "rewrite",
      "transforms.unwrap.add.fields": "op,table,lsn,source.ts_ms",
      "key.converter": "org.apache.kafka.connect.json.JsonConverter",
      "key.converter.schemas.enable": "false",
      "value.converter": "org.apache.kafka.connect.json.JsonConverter",
      "value.converter.schemas.enable": "false"
    }
  }'

echo "Connector registered. Topics will be:"
echo "  biletix.public.Events"
echo "  biletix.public.Venues"
echo "  biletix.public.Performers"
echo "  biletix.public.TicketTypes"
