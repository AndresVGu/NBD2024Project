Support for ASP.NET Core Identity was added to your project.

For setup and configuration information, see https://go.microsoft.com/fwlink/?linkid=2116645.

az login
az account list --output table
az account set --subscription "NOMBRE_O_ID_DE_LA_SUBSCRIPCION"
Luego despliegas desde Linux así:

cd /home/andres-v/Documents/projects/myprojects/NBD2024Project/NBDProject2024
dotnet publish -c Release -o ./publish
cd publish && zip -r ../publish.zip . && cd ..
az webapp deploy --resource-group NBDLandscaping_rg --name NBDLandscaping --src-path ./publish.zip --type zip