# Copyright (c) 2018 Dmitrii Evdokimov. All rights reserved.
# Licensed under the Apache License, Version 2.0.
# Source https://github.com/diev/Verba-OW-Automation

Set-StrictMode -Version Latest

# Archives helper functions

function Expand-Cab { # TODO like Expand-Archive
    [CmdletBinding()]
    param (
        # Specifies the path to the archive cab file.
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline)]
        [string]
        $Path,

        # Specifies the path to the folder in which you want the command to save extracted files.
        [Parameter(Position=1)]
        [string]
        $DestinationPath = '.',

        # Filemask of extracted files
        [string]
        $Filter = '*.*'
    )

    ./bin/cabarc.exe -o X $Path $DestinationPath $Filter
}

function Expand-Arj { # TODO like Expand-Archive
    [CmdletBinding()]
    param (
        # Specifies the path to the archive arj file.
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline)]
        [string]
        $Path,

        # Specifies the path to the folder in which you want the command to save extracted files.
        [Parameter(Position=1)]
        [string]
        $DestinationPath = '.',

        # Filemask of extracted files
        [string]
        $Filter = '*.*'
    )

    ./bin/arj32.exe x -v -y $Path $DestinationPath $Filter
}

function Compress-Arj { # TODO like Compress-Archive
    [CmdletBinding()]
    param (
        # Specifies the path to the files that you want to add to the archive file.
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline)]
        [string]
        $Path,

        # Specifies the path to the archive arj output file.
        [Parameter(Position=1)]
        [string]
        $DestinationPath = $Path,

        # Filemask of source files
        [string]
        $Filter = '*.*',

        [switch]
        $Move
    )

    if ($Move) {$cmd = 'm'} else {$cmd = 'a'}
    ./bin/arj32.exe $cmd -e -y -v5000k $DestinationPath $Path/$Filter
}

# Bulk helper functions

filter Get-Path {
    $path = $_

    # Expand Date
    if ($path.Contains('%d')) {
        # %Y/%m/%d = YYYY/MM/DD
        $path = Get-Date -UFormat $path
    }

    # Create Folder
    if (-not (Test-Path -Path $path -PathType Container)) {
        $test = Join-Path -Path $path -ChildPath test.tmp
        $null = New-Item -Path $test -ItemType File -Force
        Remove-Item -Path $test
    }

    return $path
}

function BulkClone {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline)]
        [string]
        $Path,

        [string]
        $Filter = '*.*',

        [Parameter(Position=1)]
        [string]
        $Destination = '.',

        [switch]
        $Move
    )

    $Destination = $Destination | Get-Path
    if ($Move) {
        Get-ChildItem -Path $Path -Filter $Filter -File |
            Move-Item -Destination $Destination -Force
    } else {
        Get-ChildItem -Path $Path -Filter $Filter -File |
            Copy-Item -Destination $Destination -Force
    }
}

function BulkClear {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline)]
        [string]
        $Path,

        [string]
        $Filter = '*.*',

        [Parameter(Position=1)]
        [string]
        $Destination = '.',

        [string]
        $Extension = '.*'
    )

    Get-ChildItem -Path $Path -Filter $Filter -File | ForEach-Object {
        $file = $_.FullName

        if ($Extension -eq '.*') {
            $fileOut = Join-Path -Path $Destination -ChildPath $_.Name
        } else {
            $fileOut = Join-Path -Path $Destination -ChildPath ($_.BaseName + $Extension)
        }

        if (Test-Path -Path $fileOut -PathType Leaf) {
            Remove-Item -Path $file -Force
        }
    }
}

# Bulk Verba functions

Add-Type -Path ../verba/Verba.cs

function BulkEncrypt {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline)]
        [string]
        $Path,

        [string]
        $Filter = '*.*',

        [Parameter(Position=1)]
        [string]
        $Destination = '.',

        [string]
        $Extension = '.*',

        [string]
        $Pub = '/pub',

        [Parameter(Mandatory=$true)]
        [string]
        $KeyFrom,

        [Parameter(Mandatory=$true)]
        [string]
        $KeyTo,

        [switch]
        $Move
    )

    [Verba.PoshEx]::BulkEncrypt($Path, $Filter, $Destination, $Extension, $Pub, $KeyFrom, $KeyTo, $Move)
}

function BulkDecrypt {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline)]
        [string]
        $Path,

        [string]
        $Filter = '*.*',

        [string]
        $Destination = '.',

        [string]
        $Extension = '.*',

        [string]
        $Pub = '/pub',

        [Parameter(Mandatory=$true)]
        [string]
        $KeyFrom,

        [Parameter(Mandatory=$true)]
        [string]
        $KeyTo,

        [switch]
        $Move
    )

    [Verba.PoshEx]::BulkDecrypt($Path, $Filter, $Destination, $Extension, $Pub, $KeyFrom, $KeyTo, $Move)
}

function BulkSign {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline)]
        [string]
        $Path,

        [string]
        $Filter = '*.*',

        [string]
        $Destination = '.',

        [string]
        $Pub = '/pub',

        [string]
        $Key = '206594104002',

        [switch]
        $Move
    )

    [Verba.PoshEx]::BulkSign($Path, $Filter, $Destination, $Pub, $Key, $Move)
}

function BulkVerify {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline)]
        [string]
        $Path,

        [string]
        $Filter = '*.*',

        [string]
        $Destination = '.',

        [string]
        $Pub = '/pub',

        [switch]
        $Move
    )

    [Verba.PoshEx]::BulkVerify($Path, $Filter, $Destination, $Pub, $Move)
}

function BulkUnsign {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline)]
        [string]
        $Path,

        [string]
        $Filter = '*.*',

        [string]
        $Destination = '.',

        [switch]
        $Move
    )

    [Verba.PoshEx]::BulkUnsign($Path, $Filter, $Destination, $Move)
}
