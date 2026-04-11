param(
    [ValidateSet('LAN', '4G', 'VSAT', 'HF', 'OFFLINE')][string]$Profile,
    [string]$ApiBaseUrl = 'http://localhost:8474',
    [string]$ProxyName = 'shore',
    [string]$Listen = '0.0.0.0:8666',
    [string]$Upstream = 'host.docker.internal:5000'
)

. "$PSScriptRoot\ResearchSync.Common.ps1"

function Invoke-ToxiproxyApi {
    param(
        [ValidateSet('GET', 'POST', 'DELETE')][string]$Method,
        [string]$Path,
        [object]$Body
    )

    $uri = ([Uri]::new($ApiBaseUrl.TrimEnd('/') + '/' + $Path.TrimStart('/'))).AbsoluteUri
    $arguments = @('-s', '-X', $Method, '-H', 'Content-Type: application/json', $uri)
    if ($Method -eq 'POST') {
        $arguments += @('-d', ($Body | ConvertTo-Json -Depth 10 -Compress))
    }

    $raw = & curl.exe @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Toxiproxy API call failed: $Method $uri"
    }

    if ([string]::IsNullOrWhiteSpace($raw)) {
        return $null
    }

    return $raw | ConvertFrom-Json
}

function Ensure-Proxy {
    try {
        $null = Invoke-ToxiproxyApi -Method GET -Path "proxies/$ProxyName"
    }
    catch {
        Write-ResearchLog -Message "Creating Toxiproxy proxy '$ProxyName' => $Listen -> $Upstream" -Level WARN
        $null = Invoke-ToxiproxyApi -Method POST -Path 'proxies' -Body @{
            name = $ProxyName
            listen = $Listen
            upstream = $Upstream
        }
    }
}

function Clear-Toxics {
    try {
        $toxics = Invoke-ToxiproxyApi -Method GET -Path "proxies/$ProxyName/toxics"
    }
    catch {
        return
    }

    foreach ($toxic in @($toxics)) {
        $toxicName = $null
        if ($toxic.PSObject.Properties['name']) {
            $toxicName = $toxic.name
        }
        elseif ($toxic.PSObject.Properties['Name']) {
            $toxicName = $toxic.Name
        }

        if (-not [string]::IsNullOrWhiteSpace($toxicName)) {
            $null = Invoke-ToxiproxyApi -Method DELETE -Path "proxies/$ProxyName/toxics/$toxicName"
        }
    }
}

function Add-Toxic {
    param(
        [string]$Name,
        [string]$Type,
        [string]$Stream,
        [hashtable]$Attributes,
        [double]$Toxicity = 1.0
    )

    $body = @{
        name = $Name
        type = $Type
        stream = $Stream
        toxicity = $Toxicity
        attributes = $Attributes
    }

    $null = Invoke-ToxiproxyApi -Method POST -Path "proxies/$ProxyName/toxics" -Body $body
}

Ensure-Proxy
Clear-Toxics

switch ($Profile) {
    'LAN' {
        Write-ResearchLog -Message 'Applied LAN profile: no toxics.'
    }
    '4G' {
        Add-Toxic -Name 'upstream_latency_4g' -Type 'latency' -Stream 'upstream' -Attributes @{ latency = 23; jitter = 6 }
        Add-Toxic -Name 'downstream_latency_4g' -Type 'latency' -Stream 'downstream' -Attributes @{ latency = 22; jitter = 6 }
        Write-ResearchLog -Message 'Applied 4G profile.'
    }
    'VSAT' {
        Add-Toxic -Name 'upstream_latency_vsat' -Type 'latency' -Stream 'upstream' -Attributes @{ latency = 345; jitter = 43 }
        Add-Toxic -Name 'downstream_latency_vsat' -Type 'latency' -Stream 'downstream' -Attributes @{ latency = 345; jitter = 42 }
        Add-Toxic -Name 'upstream_bandwidth_vsat' -Type 'bandwidth' -Stream 'upstream' -Attributes @{ rate = 250 }
        Add-Toxic -Name 'downstream_bandwidth_vsat' -Type 'bandwidth' -Stream 'downstream' -Attributes @{ rate = 250 }
        Write-ResearchLog -Message 'Applied VSAT profile.'
    }
    'HF' {
        Add-Toxic -Name 'upstream_latency_hf' -Type 'latency' -Stream 'upstream' -Attributes @{ latency = 1200; jitter = 250 }
        Add-Toxic -Name 'downstream_latency_hf' -Type 'latency' -Stream 'downstream' -Attributes @{ latency = 1200; jitter = 250 }
        Add-Toxic -Name 'upstream_bandwidth_hf' -Type 'bandwidth' -Stream 'upstream' -Attributes @{ rate = 8 }
        Add-Toxic -Name 'downstream_bandwidth_hf' -Type 'bandwidth' -Stream 'downstream' -Attributes @{ rate = 8 }
        Write-ResearchLog -Message 'Applied HF-like profile.'
    }
    'OFFLINE' {
        Add-Toxic -Name 'offline_cut' -Type 'timeout' -Stream 'downstream' -Attributes @{ timeout = 60000 }
        Write-ResearchLog -Message 'Applied OFFLINE profile.'
    }
}