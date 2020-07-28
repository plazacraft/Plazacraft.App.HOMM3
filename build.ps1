
# REQUIREMENTS
# Microsoft SDK
# .NET Core 3.0

# CONFIG
$sdk = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\";

# TOOLS
$xsd = $sdk + "xsd.exe";

# FUNCTIONS
Function XSD 
{
    PARAM ([string]$xsdFile, [string]$outFile, [string] $namespace)

    $xsdItem = Get-Item $xsdFile
    $exists = Test-Path $outFile
    if (!$exists)
    {
        New-Item $outFile
    }

    $outItem = Get-Item $outFile
    if (($xsdItem.LastWriteTime -gt $outItem.LastWriteTime) -or !$exists)
    {
        $dir = $outItem.Directory.FullName;
        $outFileName = $outItem.Name;

        $tmpFileName = $xsdItem.Name;
        $tmpFileName = $tmpFileName.Replace(".","_");
        $tmpFileName = $tmpFileName.Replace("_xsd",".cs");

        Remove-Item $outItem;
        & $xsd "$xsdFile" /c "/o:$dir" "/n:$namespace";
        Rename-Item "$dir\$tmpFileName" $outFileName;

    }
};

Function Copy-IfNewer
{
    PARAM ([string]$source, [string]$dest)
    $sourceItem = Get-Item $source
    $dest = "$dest\\" + $sourceItem.Name
    $exists = Test-Path $dest
    $doCopy = $false;

    if ($exists)
    {
        $destItem = Get-Item $dest;
        if ($sourceItem.LastWriteTime -gt $destItem.LastWriteTime)
        {
            $doCopy = $true;
        }
    }
    else 
    {
        $doCopy = $true;
    }

    if ($doCopy)
    {
        Copy-Item $source $dest
    }
}

# BUILD 

# Fight contains Definition 
# XSD "DamageSymulator/Core/xsd/Plazacraft.HOMM3.DamageSymulator.Core.Definition.xsd" "DamageSymulator/Core/Definition.cs" "Plazacraft.HOMM3.DamageSymulator.Core.Definition"
XSD "DamageSymulator/Schemas/Plazacraft.HOMM3.DamageSymulator.Fight.xsd" "DamageSymulator/Core/xsd.cs" "Plazacraft.HOMM3.DamageSymulator.Core"
Copy-IfNewer "DamageSymulator/Schemas/Plazacraft.HOMM3.DamageSymulator.Definition.xsd" "DamageSymulator\Test\"
Copy-IfNewer "DamageSymulator/Schemas/Plazacraft.HOMM3.DamageSymulator.Fight.xsd" "DamageSymulator\Test\"

# Linked files doesn't work for debbuging
Copy-IfNewer "DamageSymulator/Config/Definition.xml" "DamageSymulator/WebService/Config/"

# dotnet build ./DamageSymulator/Core/Plazacraft.HOMM3.DamageSymulator.Core.csproj
dotnet build ./DamageSymulator/Console/Plazacraft.HOMM3.DamageSymulator.Console.csproj
dotnet publish -c Release -r win10-x64 ./DamageSymulator/Console/Plazacraft.HOMM3.DamageSymulator.Console.csproj

dotnet build ./DamageSymulator/WebService/Plazacraft.HOMM3.DamageSymulator.WebService.csproj
# dotnet publish -c Debug -r win10-x64 ./DamageSymulator/WebService/Plazacraft.HOMM3.DamageSymulator.WebService.csproj
dotnet publish -c Release -r win10-x86 ./DamageSymulator/WebService/Plazacraft.HOMM3.DamageSymulator.WebService.csproj
