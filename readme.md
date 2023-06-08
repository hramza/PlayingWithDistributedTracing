# Distributed tracing
## Used tools :
- .NET 6
- Open Telemetry
- Zipkin
- Prometheus
- Elasticsearch
- Kibana
- Kafka

## Project
We have two services that use distributed tracing.

- Producer : .NET 6 Minimal API with compile time logging source generation that create a currency request and publish it to Kafka
- Consumer : .NET 6 worker service that consumes the Kafka record, call a currency API to get the currency low and high price and store them in Elasticsearch

By using distributed tracing, we see the context from calling /index endpoint to elastic search indexing.

We use telemetry tools to view the trace context.

### [OpenTelemetry](https://opentelemetry.io/), [Zipkin](https://zipkin.io/) & [Prometheus](https://prometheus.io/)

OpenTelemetry will capture the trace data(activity spans and tags) and use OpenTelemetry protocol grpc service to export it and make it available to Zipkin and Prometheus.

### Testing this set of tools

The only thing to do in order to run this sample is to fire up services using docker compose :

```
docker-compose up -d
```
![Alt text](Images/Docker.png?raw=true "docker services")


1- Go to [Swagger](https://localhost:8081/swagger) and call index endpoint

2- We should see a record in Kafka control center

![Alt text](Images/Kafka.png?raw=true "Kafka control center")

3- The currency response should be indexed in elastic search

![Alt text](Images/Kibana.png?raw=true "Kibana")

4-Zipkin

In Zipkin, you will many activity sources. For producer, consumer, elastic, http calls, ...

![Alt text](Images/Zipkin.png?raw=true "Kibana")

5-Prometheus

We have only one meter named RequestCount

![Alt text](Images/Prometheus.png?raw=true "Kibana")
