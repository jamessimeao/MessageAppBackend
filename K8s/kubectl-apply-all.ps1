kubectl create namespace messageapp
kubectl apply -f mssql.yaml -n messageapp
kubectl apply -f auth-service-deploy.yaml -n messageapp # requires mssql

# Kafka
kubectl create namespace kafka
# Install strimzi Kafka operator
kubectl apply -f kafka/strimzi.yaml -n kafka
# Apply the Kafka Cluster CR file
#kubectl apply -f kafka/kafka-persistent.yaml -n kafka # closer to production code, but more expensive
kubectl apply -f kafka/kafka-single-node.yaml -n kafka # I'll use a single node, to reduce memory consumption
# Create a Kafka topic. After this, we can start sending messages to this topic.
kubectl apply -f kafka/kafka-topic.yaml -n kafka

kubectl apply -f kafka-consumer-deploy.yaml -n messageapp

kubectl apply -f message-service-deploy.yaml -n messageapp # requires auth (only authorized users can access) and kafka
kubectl apply -f gateway.yaml -n messageapp # gateway for client to access auth and message services