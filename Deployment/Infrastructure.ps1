[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [switch]$Help,

    [string]$Location = "westeurope",
    [string]$Name = "storelocator",
    [string]$DatabaseName = "storelocator",
    [string]$Runtime = "dotnet:8"
)

# Help function
function DisplayHelp {
    @"
Usage: .\infrastructure.ps1 [-Location <location>] [-Name <name>] [-DatabaseName <dbname>] [-Runtime <runtime>]

Options:
    -Location       Azure region where resources will be created. Default is 'westeurope'.
    -Name           Base name for the resources. Default is 'storelocator'.
    -DatabaseName   Name for the CosmosDB database. Default is 'storelocator'.
    -Runtime        Runtime for the Azure Web App. Default is 'dotnet:8'.

    -Help           Display this help message.

Example:
    .\infrastructure.ps1 -Location "northcentralus" -Name "myapp" -DatabaseName "mydb" -Runtime "dotnet:8"
"@
    exit
}

# Check if help switch is present
if ($Help.IsPresent) {
    DisplayHelp
}

# Generate a random suffix for the Cosmos DB and Web App
$suffix = Get-Random

# Configuration
$resourceGroup = "rg-$Name"
$azuremaps = "map-$Name"
$cosmosdb = "db-$Name$suffix"
$webserverplan = "plan-$Name"
$webappname = "web-$Name$suffix"

# Create a resource group
Write-Output "Creating Resource Group '$resourceGroup' in '$Location'..."
az group create --name $resourceGroup --location $Location

# Create Azure Maps account
Write-Output "Creating Azure Maps account '$azuremaps'..."
az maps account create --resource-group $resourceGroup --account-name $azuremaps --sku G2 --kind Gen2 --accept-tos

# Create Azure Cosmos DB
Write-Output "Creating Azure Cosmos DB '$cosmosdb'..."
az cosmosdb create --resource-group $resourceGroup --name $cosmosdb --locations regionName=$Location --capabilities EnableServerless

Write-Output "Creating database '$DatabaseName'..."
az cosmosdb sql database create --resource-group $resourceGroup --account-name $cosmosdb --name $DatabaseName

# Create Webserver and Website
Write-Output "Creating WebServer '$webserverplan' for Website '$webappname'..."
az appservice plan create --resource-group $resourceGroup -n $webserverplan --location $Location
az webapp create --resource-group $resourceGroup -p $webserverplan -n $webappname -r $Runtime

# Done
Write-Output "Done, your Azure Maps Locator infrastructure is ready."
