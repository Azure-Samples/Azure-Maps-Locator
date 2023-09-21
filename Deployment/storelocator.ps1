[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [switch]$Help,

    [string]$Location = "westeurope",
    [string]$Name = "storelocator",
    [string]$DatabaseName = "storelocator"
)

# Help function
function DisplayHelp {
    @"
Usage:
    .\storelocator.ps1 [-Location <location>] [-Name <name>] [-DatabaseName <dbname>]

Options:
    -Location       Azure region where resources will be created. Default is 'westeurope'.
    -Name           Base name for the resources. Default is 'storelocator'.
    -DatabaseName   Name for the Cosmos DB database. Default is 'storelocator'.

    -Help           Display this help message.

Example:
    .\storelocator.ps1 -Location "northcentralus" -Name "myapp" -DatabaseName "mydb"
"@
    exit
}

# Check if help switch is present
if ($Help.IsPresent) {
    DisplayHelp
}

echo "Starting..."

# Generate a random suffix for the Cosmos DB and Web App
$suffix = Get-Random

# Configuration
$group = "rg-$Name"
$azuremaps = "map-$Name"
$cosmosdb = "db-$Name$suffix"
$webserverplan = "plan-$Name"
$webappname = "web-$Name$suffix"

# Create a resource group
echo "Creating Resource Group '$group' in location '$Location'..."
az group create --name $group --location $Location

# Create Azure Maps account
echo "Creating Azure Maps account '$azuremaps'..."
az maps account create -g $group --account-name $azuremaps --sku G2 --kind Gen2 --accept-tos

# Create Azure Cosmos DB
echo "Creating Azure Cosmos DB '$cosmosdb'..."
az cosmosdb create -g $group --name $cosmosdb --locations regionName=$Location --capabilities EnableServerless

#echo "Creating database '$DatabaseName'..."
az cosmosdb sql database create -g $group --account-name $cosmosdb --name $DatabaseName

# Create Webserver and Website
echo "Creating Webserver '$webserverplan' for Website '$webappname'..."
az appservice plan create -g $group -n $webserverplan --location $Location
az webapp create -g $group -p $webserverplan -n $webappname -r "dotnet:7"

# Use managed identities
echo "Use managed identities for Azure Maps and Cosmos DB"
az webapp identity assign -n $webappname -g $group
$principal = $(az webapp identity show -g $group --name $webappname --query principalId --output tsv)

# Azure Maps
$scope = $(az maps account show -g $group --name $azuremaps --query id --output tsv)
az role assignment create --assignee $principal --role "Azure Maps Data Reader" --scope $scope

# Cosmos DB
$roledefinition = Get-Content -Path "cosmosdb.json"
az cosmosdb sql role definition create -g $group --account-name $cosmosdb --body $roledefinition

$scope = $(az cosmosdb show -g $group --name $cosmosdb --query id --output tsv)
az cosmosdb sql role assignment create -g $group --account-name $cosmosdb --role-definition-name "Read Azure Cosmos DB Metadata" --principal-id $principal --scope $scope

$cosmosEndpoint = $(az cosmosdb show -g $group --name $cosmosdb --query documentEndpoint --output tsv)

# Deploy Azure Maps Store Locator
echo "Starting deployment Azure Maps Store Locator..."
az webapp config appsettings set -g $group -n $webappname --settings "locator:DatabaseName=$DatabaseName"
az webapp config appsettings set -g $group -n $webappname --settings "locator:DatabaseEndpoint=$cosmosEndpoint"
az webapp deployment source config-zip -g $group -n $webappname --src release.zip

# Done
echo "Done, your Azure Maps Store Locator infrastructure and website is ready."
