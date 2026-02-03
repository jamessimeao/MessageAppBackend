#!/bin/bash

# From https://docs.nginx.com/nginx-gateway-fabric/install/helm/

# Uninstall NGINX Gateway Fabric
helm uninstall ngf -n nginx-gateway

# Remove namespace and CRDs
kubectl delete ns nginx-gateway
kubectl delete -f https://raw.githubusercontent.com/nginx/nginx-gateway-fabric/v2.4.0/deploy/crds.yaml

# Remove the Gateway API resources
kubectl kustomize "https://github.com/nginx/nginx-gateway-fabric/config/crd/gateway-api/standard?ref=v2.4.0" | kubectl delete -f -