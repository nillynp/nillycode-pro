# ============================================================================
# NillyCode Pro — Universal Windows 10/11 Installer (Zero Admin · Multi-PC)
# created by Nilly graphics (Sotiris Christos) — https://nillygraphics.com/
# Registers 50+ Code, Web, Script & Text formats + Explorer Right-Click (HKCU)
# ============================================================================
param(
    [switch]$Silent,
    [switch]$OpenDefaultApps
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$exePath = Join-Path $root 'NillyCodePro.exe'
$icoPath = Join-Path $root 'NillyCodePro.ico'
$srcPath = Join-Path $root 'desktop\NillyCodePro.cs'

Write-Host ''
Write-Host '  ============================================================' -ForegroundColor Cyan
Write-Host '   NillyCode Pro — Εγκατάσταση & Σύνδεση 50+ Code/Text Formats' -ForegroundColor White
Write-Host '   created by Nilly graphics (Sotiris Christos)' -ForegroundColor DarkGray
Write-Host '   https://nillygraphics.com/' -ForegroundColor DarkGray
Write-Host '  ============================================================' -ForegroundColor Cyan
Write-Host ''

# 1. Unblock downloaded files if extracted from GitHub Portable ZIP
Get-ChildItem -Path $root -Recurse -File -ErrorAction SilentlyContinue | Unblock-File -ErrorAction SilentlyContinue

# 2. Auto-compile NillyCodePro.exe on any Windows 10/11 PC if missing or older than source
$needBuild = (-not (Test-Path $exePath)) -or ((Test-Path $srcPath) -and ((Get-Item $srcPath).LastWriteTimeUtc -gt (Get-Item $exePath).LastWriteTimeUtc))
if ($needBuild) {
    $fxDir = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
    if (-not (Test-Path "$fxDir\csc.exe")) { $fxDir = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319' }
    $csc = Join-Path $fxDir 'csc.exe'
    if (-not (Test-Path $csc)) { throw 'Δεν βρέθηκε ο .NET Framework 4.x C# compiler (csc.exe).' }
    Write-Host '  [1/3] Μεταγλώττιση NillyCodePro.exe (100% Native Windows Sublime-Style Host)...' -ForegroundColor Yellow
    $cscArgs = @(
        '/nologo', '/target:winexe', '/optimize+', '/utf8output', "/out:$exePath",
        '/r:System.dll', '/r:System.Core.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll'
    )
    if (Test-Path $icoPath) { $cscArgs += "/win32icon:$icoPath" }
    $cscArgs += $srcPath
    & $csc @cscArgs
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path $exePath)) {
        throw "Αποτυχία μεταγλώττισης NillyCodePro.exe (exit $LASTEXITCODE)"
    }
    Write-Host ("        OK -> " + [math]::Round((Get-Item $exePath).Length / 1KB, 1) + " KB") -ForegroundColor Green
} else {
    Write-Host '  [1/3] NillyCodePro.exe έτοιμο.' -ForegroundColor Green
}

# 3. Register NillyCode Pro for 50+ Code, Web, Config & Text formats + Explorer Right-Click (Zero Admin!)
Write-Host '  [2/3] Εγγραφή του NillyCode Pro στα Windows (50+ formats & δεξί κλικ)...' -ForegroundColor Yellow
$iconRef = if (Test-Path $icoPath) { "$icoPath,0" } else { "$exePath,0" }
$openCmd = "`"$exePath`" `"%1`""

$progKey = 'HKCU:\Software\Classes\NillyCodePro.Document'
New-Item -Path "$progKey\DefaultIcon" -Force | Out-Null
New-Item -Path "$progKey\shell\open\command" -Force | Out-Null
Set-ItemProperty -Path $progKey -Name '(Default)' -Value 'NillyCode Pro Document'
Set-ItemProperty -Path $progKey -Name 'FriendlyTypeName' -Value 'NillyCode Pro Document'
Set-ItemProperty -Path "$progKey\DefaultIcon" -Name '(Default)' -Value $iconRef
Set-ItemProperty -Path "$progKey\shell\open" -Name 'FriendlyAppName' -Value 'NillyCode Pro'
Set-ItemProperty -Path "$progKey\shell\open\command" -Name '(Default)' -Value $openCmd

$appKey = 'HKCU:\Software\Classes\Applications\NillyCodePro.exe'
New-Item -Path "$appKey\DefaultIcon" -Force | Out-Null
New-Item -Path "$appKey\shell\open\command" -Force | Out-Null
New-Item -Path "$appKey\SupportedTypes" -Force | Out-Null
Set-ItemProperty -Path $appKey -Name 'FriendlyAppName' -Value 'NillyCode Pro'
Set-ItemProperty -Path "$appKey\DefaultIcon" -Name '(Default)' -Value $iconRef
Set-ItemProperty -Path "$appKey\shell\open\command" -Name '(Default)' -Value $openCmd

$capKey = 'HKCU:\Software\NillyGraphics\NillyCodePro\Capabilities'
New-Item -Path "$capKey\FileAssociations" -Force | Out-Null
Set-ItemProperty -Path $capKey -Name 'ApplicationName' -Value 'NillyCode Pro'
Set-ItemProperty -Path $capKey -Name 'ApplicationDescription' -Value 'Ultra-Light Native Sublime-Style Code & Text Editor — created by Nilly graphics (Sotiris Christos) — https://nillygraphics.com/'

$allExts = @(
    '.txt','.log','.md','.markdown','.csv','.tsv',
    '.html','.htm','.xhtml','.css','.scss','.sass','.less',
    '.js','.mjs','.cjs','.jsx','.ts','.tsx','.vue','.svelte',
    '.php','.phtml','.json','.jsonc','.webmanifest',
    '.xml','.svg','.xaml','.csproj','.config',
    '.cs','.c','.cpp','.h','.java','.go','.rs','.rb','.py','.pyw',
    '.ps1','.psm1','.psd1','.bat','.cmd','.sh',
    '.sql','.ini','.cfg','.conf','.env','.yml','.yaml','.toml',
    '.gitignore','.htaccess','.editorconfig','.properties'
)

foreach ($ext in $allExts) {
    $owp = "HKCU:\Software\Classes\$ext\OpenWithProgids"
    New-Item -Path $owp -Force | Out-Null
    Set-ItemProperty -Path $owp -Name 'NillyCodePro.Document' -Value ([byte[]]@()) -Type Binary
    Set-ItemProperty -Path "$appKey\SupportedTypes" -Name $ext -Value ''
    Set-ItemProperty -Path "$capKey\FileAssociations" -Name $ext -Value 'NillyCodePro.Document'
}

New-Item -Path 'HKCU:\Software\RegisteredApplications' -Force | Out-Null
Set-ItemProperty -Path 'HKCU:\Software\RegisteredApplications' -Name 'NillyCodePro' -Value 'Software\NillyGraphics\NillyCodePro\Capabilities'

# Windows Explorer Right-Click Context Menu: "Edit with NillyCode Pro" & "Open Folder in NillyCode Pro"
# Use .NET Registry API for '*' key so PowerShell never treats '*' as a wildcard glob!
$kFile = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey('Software\Classes\*\shell\NillyCodePro')
$kFile.SetValue('', 'Edit with NillyCode Pro')
$kFile.SetValue('Icon', $iconRef)
$kFileCmd = $kFile.CreateSubKey('command')
$kFileCmd.SetValue('', $openCmd)
$kFileCmd.Close(); $kFile.Close()

$dirCtx = 'HKCU:\Software\Classes\Directory\shell\NillyCodePro'
New-Item -Path "$dirCtx\command" -Force | Out-Null
Set-ItemProperty -LiteralPath $dirCtx -Name '(Default)' -Value 'Open Folder in NillyCode Pro'
Set-ItemProperty -LiteralPath $dirCtx -Name 'Icon' -Value $iconRef
Set-ItemProperty -LiteralPath "$dirCtx\command" -Name '(Default)' -Value "`"$exePath`" `"%V`""

try {
    Add-Type -Namespace NillyCodeShell -Name Notify -MemberDefinition '[System.Runtime.InteropServices.DllImport("shell32.dll")] public static extern void SHChangeNotify(int wEventId, int uFlags, System.IntPtr dwItem1, System.IntPtr dwItem2);' -ErrorAction SilentlyContinue
    [NillyCodeShell.Notify]::SHChangeNotify(0x08000000, 0x2000, [IntPtr]::Zero, [IntPtr]::Zero)
} catch {}

Write-Host ("        OK -> Εγγράφηκαν " + $allExts.Count + " formats + δεξί κλικ «Edit with NillyCode Pro»!") -ForegroundColor Green

# 4. Desktop & Start Menu Shortcuts
Write-Host '  [3/3] Δημιουργία συντομεύσεων (Επιφάνεια Εργασίας & Start Menu)...' -ForegroundColor Yellow
$wsh = New-Object -ComObject WScript.Shell
$desktopDir = [Environment]::GetFolderPath('Desktop')
$startMenuDir = Join-Path ([Environment]::GetFolderPath('Programs')) 'Nilly graphics'
New-Item -ItemType Directory -Force -Path $startMenuDir | Out-Null

foreach ($lnkPath in @(
    (Join-Path $desktopDir 'NillyCode Pro.lnk'),
    (Join-Path $startMenuDir 'NillyCode Pro.lnk')
)) {
    $sc = $wsh.CreateShortcut($lnkPath)
    $sc.TargetPath = $exePath
    $sc.WorkingDirectory = $root
    $sc.Description = 'NillyCode Pro — Sublime-Style Code & Text Editor | created by Nilly graphics (Sotiris Christos) — https://nillygraphics.com/'
    if (Test-Path $icoPath) { $sc.IconLocation = "$icoPath,0" }
    $sc.Save()
}
Write-Host '        OK -> Έτοιμες οι συντομεύσεις!' -ForegroundColor Green
Write-Host ''
Write-Host '  ΟΛΟΚΛΗΡΩΘΗΚΕ! Για να ανοίγουν τα αρχεία σου αυτόματα με διπλό κλικ:' -ForegroundColor Cyan
Write-Host '   -> Ρυθμίσεις Windows -> Προεπιλεγμένες εφαρμογές -> NillyCode Pro' -ForegroundColor White
Write-Host '   -> Ή δεξί κλικ σε οποιοδήποτε αρχείο -> "Edit with NillyCode Pro"' -ForegroundColor White
Write-Host ''

if ($OpenDefaultApps) {
    Start-Process 'ms-settings:defaultapps'
} elseif (-not $Silent) {
    $ans = Read-Host '  Θες να ανοίξω τώρα τις Προεπιλεγμένες Εφαρμογές των Windows (Ν/Ο)?'
    if ($ans -match '^[yYνΝ]') { Start-Process 'ms-settings:defaultapps' }
}
