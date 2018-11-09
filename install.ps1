$ProgressPreference='SilentlyContinue'

$temp = $env:temp
if ($isLinux) {
	$temp = '/tmp'
}

$zipPath = Join-Path $temp 'secure-file.zip'
[Net.ServicePointManager]::SecurityProtocol = "tls12, tls11, tls"
(New-Object Net.WebClient).DownloadFile('https://github.com/jonlabelle/secure-file/archive/master.zip', $zipPath)
Expand-Archive $zipPath -DestinationPath (Join-Path (pwd).path "appveyor-tools")
if ($isLinux) {
	chmod +x ./appveyor-tools/secure-file
}
del $zipPath
