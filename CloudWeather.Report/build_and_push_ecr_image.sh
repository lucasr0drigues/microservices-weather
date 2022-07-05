set -e

aws ecr get-login-password --region us-east-1 --profile weather-ecr-agent | 
docker login --username AWS --password-stdin 018574742015.dkr.ecr.us-east-1.amazonaws.com 
docker build -f ./Dockerfile -t cloud-weather-report:latest . 
docker tag cloud-weather-report:latest 018574742015.dkr.ecr.us-east-1.amazonaws.com/cloud-weather-report:latest 
docker push 018574742015.dkr.ecr.us-east-1.amazonaws.com/cloud-weather-report:latest