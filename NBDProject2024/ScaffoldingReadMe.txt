Support for ASP.NET Core Identity was added to your project.

For setup and configuration information, see https://go.microsoft.com/fwlink/?linkid=2116645.

az login
az account list --output table
az account set --subscription "NOMBRE_O_ID_DE_LA_SUBSCRIPCION"
Luego despliegas desde Linux así:

cd /home/andres-v/Documents/projects/myprojects/NBD2024Project/NBDProject2024
dotnet publish -c Release -o ../publish
cd ../publish && zip -r ../publish.zip . && cd ../NBDProject2024
az webapp deploy --resource-group NBDLandscaping_rg --name NBDLandscaping --src-path ./publish.zip --type zip

Correr el proyecto en local 
ara correrlo en localhost, haz esto:

Abre terminal en la raíz del workspace:
/home/andres-v/Documents/projects/myprojects/NBD2024Project

(Opcional la primera vez) restaura paquetes:
dotnet restore

Ejecuta el proyecto con perfil HTTPS:
dotnet run --project NBDProject2024.csproj --launch-profile https

Abre en navegador:
https://localhost:7248
o
http://localhost:5162

