kubectl delete -f kafka-consumer-deploy.yaml -n messageapp

kubectl delete -f mssql.yaml -n messageapp

kubectl delete -f auth-service-deploy.yaml -n messageapp

kubectl delete -f rooms-service-deploy.yaml -n messageapp

kubectl delete -f message-service-deploy.yaml -n messageapp

kubectl delete -f gateway.yaml -n messageapp

kubectl delete secret mssql -n messageapp
kubectl delete secret jwt-settings -n messageapp
kubectl delete secret db-settings -n messageapp

kubectl delete pvc mssql-mssql-0 -n messageapp

# Delete Kafka cluster
kubectl -n kafka delete $(kubectl get strimzi -o name -n kafka)
kubectl delete pvc -l strimzi.io/name=my-cluster-kafka -n kafka
# Delete Strimzi cluster operator
kubectl -n kafka delete -f strimzi.yaml
# Delete kafka namespace
kubectl delete namespace kafka