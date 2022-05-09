# .Net Microservices
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-blue?style=flat&logo=linkedin&labelColor=blue")](https://www.linkedin.com/in/andr%C3%A9-terra-2a7728145/)

## Introduction
This is a two services system to manage Platforms and Commands.

I made this project to learn the basics of .Net microservices architecture and technologies like: Asp.net Core, Entity Framework, Microsoft SQL Server, Docker, Kubernetes, RabbitMQ, gRPC, etc.

This project is mostly made with this [11-hours-long code-along youtube tutorial](https://youtu.be/DgVjEo3OGBI) from [Les Jackson](https://www.youtube.com/channel/UCIMRGVXufHT69s1uaHHYJIA)

See [Improvements](#improvements) section for the features I've added.

## Description Breakdown:
Platform Service
- Models: Platform
- Uses its own MSSQL Server to store Platforms (for development and production environment)
- Create, Delete and Fetch Platforms from its MSSQL Server
- Implements POST, GET and DELETE Http verbs for Platforms
- Publishes messages to RabbitMQ when creating and deleting Platforms
- Implements gRPC method to get all Platforms

Command Service
- Models: Command, Platform
- Uses its own MSSQL Server to store its Commands and Platforms (for development and production environment)
- Create, Delete and Fetch Commands and Platforms from its MSSQL Server
- Implements POST, GET and DELETE Http verbs for Commands
- Implements POST, GET Http verbs for Platforms
- Subscribes to RabbitMQ and listen for Platforms Create and Delete messages
- Uses gRPC from Platform Service to fetch all platforms  

Kubernetes:
- Platform and Command services Deployment file with Cluter IP, MSSQL Connection String as environment variable (Kubernetes Secret)
- Persistent Volume Claim file for Platform and Command service MSSQL Server
- MSSQL Server container for Platform and Command service with Cluster IP, LoadBalancer, SA Password as environment variable (Kubernetes Secret)
- Nginx Ingress Controller to expose Platform and Command Service routes 
- RabbitMQ container with Cluster IP and LoadBalancer

## Improvements
Here is a list of features I've added apart from the code along tutorial
- MSSQL Server for Command Service (Production and Development environment)
- MSSQL Server for Platform Server (Development environment)
- DELETE Http verb for Platforms (Platform Service)
- DELETE Http verb for Commands (Command Service)
- Platform Service: Platform Deleted message publish to RabbitMQ
- Command Service: Platform Deleted message subscribe to RabbitMQ

## Built with
- .Net
- Asp.net Core
- Entity Framework
- Microsoft SQL Server
- Docker
- Kubernetes
- RabbitMQ
- gRPC
- Nginx Ingress Controller

## Getting Started
### Prerequisites
- Docker with Kubernetes enabled
### Running the Application
1. Clone this repository

```bash 
git clone https://github.com/AndreTerra5348/net-microservices
```

2. Restore packages

```bash
cd PlatformService
dotnet restore

cd CommandService
dotnet restore
```

2. Build and push docker image

```bash
cd PlatformService
docker build -t <username>/platformservice .
docker push <username>/platformservice

cd CommandService
docker build -t <username>/commandservice .
docker push <username>/commandservice
```

3. Change platforms-depl.yaml and commands-depl.yaml image tag to the docker image you just pushed

```bash
image: <username>/commandservice:latest
```

```bash
image: <username>/platformservice:latest
```

4. Create Kubernetes Secrets to SA Password and Connection String for MSSQL Server

```bash
kubectl create secret generic platforms-mssql --from-literal=password=<password> --from-literal=constr=<connection-string>

kubectl create secret generic commands-mssql --from-literal=password=<password> --from-literal=constr=<connection-string>
```

5. Apply Kubernetes Service and Deployment files

```bash
cd K8S
kubectl apply -f ingress-srv.yaml
kubectl apply -f rabbitmq-depl.yaml

kubectl apply -f cammands-local-pvc.yaml
kubectl apply -f cammands-mssql-depl.yaml
kubectl apply -f commands-depl.yaml

kubectl apply -f platform-local-pvc.yaml
kubectl apply -f platform-mssql-depl.yaml
kubectl apply -f platforms-depl.yaml
```

6. (Optional) To run in development environment, set local environment variables containing MSSQL Connection String

```bash
setx SQLCONNSTR_PlatformsConn "Server=localhost,1433;Initial Catalog=platformsdb;User Id=sa;Password=<password>;"

setx SQLCONNSTR_CommandsConn "Server=localhost,1434;Initial Catalog=platformsdb;User Id=sa;Password=<password>;"
```

7. (Optional) Run the application in development environment

```bash
cd PlatformService
dotnet run

cd CommandService
dotnet run
```

## Acknowledgments and Resources

- [Les Jackson Tutorial](https://youtu.be/DgVjEo3OGBI)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Asp.Net Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0)
- [Azuredevcollage](https://azuredevcollege.com/trainingdays/day7/challenges/challenge-3.html)

## License
Distributed under the MIT License. See LICENSE.txt for more information.

## Author
[André Terra](https://www.linkedin.com/in/andr%C3%A9-terra-2a7728145/)