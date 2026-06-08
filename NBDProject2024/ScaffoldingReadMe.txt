Support for ASP.NET Core Identity was added to your project.

For setup and configuration information, see https://go.microsoft.com/fwlink/?linkid=2116645.

az login
az account list --output table
az account set --subscription "NOMBRE_O_ID_DE_LA_SUBSCRIPCION"
Luego despliegas desde Linux así:
-------------------------------------------------------------------------------------------------------
cd /home/andres-v/Documents/projects/myprojects/NBD2024Project/NBDProject2024
dotnet publish -c Release -o ../publish
rm -f ../publish.zip ./publish.zip
cd ../publish && zip -r ../publish.zip . && cd ../NBDProject2024
az webapp deploy --resource-group NBDLandscaping_rg --name NBDLandscaping --src-path ../publish.zip --type zip

-------------------------------------
Recrear SQLite de dominio + Seed en produccion (RECOMENDADO):

Nota: este proceso recrea SOLO la BD de dominio y conserva Identity (Admin/Root).

1) Activa recreacion una sola vez:
az webapp config appsettings set --resource-group NBDLandscaping_rg --name NBDLandscaping --settings Bootstrap__RecreateDomainDatabase=true

2) (Opcional) Forzar ejecucion aunque exista marcador previo:
az webapp config appsettings set --resource-group NBDLandscaping_rg --name NBDLandscaping --settings Bootstrap__RecreateDomainDatabaseOverride=true

3) Reinicia la app:
az webapp restart --resource-group NBDLandscaping_rg --name NBDLandscaping

4) Desactiva banderas despues del primer arranque exitoso:
az webapp config appsettings set --resource-group NBDLandscaping_rg --name NBDLandscaping --settings Bootstrap__RecreateDomainDatabase=false Bootstrap__RecreateDomainDatabaseOverride=false

Nota adicional:
- El sistema usa version de seed en Program.cs (DomainSeedVersion).
- Si sube una nueva version de seed, se recrea la BD de dominio automaticamente al arrancar.
- Esto NO borra Identity, por lo que Admin/Root se conservan.











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



cd /home/andres-v/Documents/projects/myprojects/NBD2024Project/NBDProject2024
dotnet publish -c Release -o ../publish
rm -f ../publish.zip ./publish.zip
cd ../publish && zip -r ../publish.zip . && cd ../NBDProject2024
az webapp deploy --resource-group NBDLandscaping_rg --name NBDLandscaping --src-path ../publish.zip --type zip
az webapp restart --resource-group NBDLandscaping_rg --name NBDLandscaping

_________________________________________________________

Comandos finales recomendados (version + deploy):

cd /home/andres-v/Documents/projects/myprojects/NBD2024Project/NBDProject2024
dotnet publish -c Release -o ../publish
rm -f ../publish.zip ./publish.zip
cd ../publish && zip -r ../publish.zip . && cd ../NBDProject2024

# Asigna una marca visible en el footer para confirmar que estas viendo el build nuevo
az webapp config appsettings set --resource-group NBDLandscaping_rg --name NBDLandscaping --settings AppBuildVersion="2026-06-08.5"

az webapp deploy --resource-group NBDLandscaping_rg --name NBDLandscaping --src-path ../publish.zip --type zip
az webapp restart --resource-group NBDLandscaping_rg --name NBDLandscaping