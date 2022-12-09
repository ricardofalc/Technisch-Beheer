#!/bin/bash


#Install dotnet
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update -y
sudo apt-get install dotnet-sdk-6.0 -y
sudo apt-get install dotnet-ef -y
dotnet --list-sdks

#Install SQL Server 2022 (v16.x)
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/20.04/mssql-server-2022.list)"
apt-get update
apt-get install -y mssql-server
/opt/mssql/bin/mssql-conf setup

#Install Git
apt-get install -y git

#Build Backend van FTS mbv dotnet CLI op basis van gegeven GitHub url.
git clone https://github.com/ricardofalc/Technisch-Beheer
cd Technisch-Beheer
sudo dotnet build Backend/

#Run backend
sudo dotnet run --project Backend/src/Trackable.Web/Trackable.Web.csproj
