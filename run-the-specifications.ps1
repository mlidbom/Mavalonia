#Requires -Version 7
<#
.SYNOPSIS
Builds and runs Mavalonia's specifications, all of them or the classes whose names match -Filter.

.DESCRIPTION
Runs from the repository's folder whatever folder it is started in: global.json there opts dotnet test into
Microsoft Testing Platform, and dotnet test started elsewhere fails, asking for VSTest.

.EXAMPLE
pwsh C:/Dev/Mavalonia/run-the-specifications.ps1 -Filter Text_shown_as_a_link
#>
param(
   # A part of the names of the specification classes to run, such as Text_shown_as_a_link. All of them when left out.
   [string]$Filter
)

$ErrorActionPreference = 'Stop'
$onlyThoseMatching = if($Filter) { @('--filter-class', "*$Filter*") } else { @() }

Push-Location $PSScriptRoot
try
{
   dotnet test --solution Mavalonia.slnx @onlyThoseMatching
   exit $LASTEXITCODE
}
finally
{
   Pop-Location
}
