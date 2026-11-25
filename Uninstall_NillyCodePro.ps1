# ============================================================================
# NillyCode Pro — Clean Uninstaller (Zero Admin · HKCU)
# created by Nilly graphics (Sotiris Christos) — https://nillygraphics.com/
# ============================================================================
param([switch]$Silent)

$ErrorActionPreference = 'SilentlyContinue'
Write-Host ''
Write-Host '  Αφαίρεση εγγραφών & συντομεύσεων του NillyCode Pro...' -ForegroundColor Yellow

Stop-Process -Name 'NillyCodePro' -Force -ErrorAction SilentlyContinue

# Remove HKCU ProgID, Application, Capabilities & Explorer Context Menu
Remove-Item -Path 'HKCU:\Software\Classes\NillyCodePro.Document' -Recurse -Force
Remove-Item -Path 'HKCU:\Software\Classes\Applications\NillyCodePro.exe' -Recurse -Force
Remove-Item -Path 'HKCU:\Software\NillyGraphics\NillyCodePro' -Recurse -Force
& reg.exe delete "HKCU\Software\Classes\*\shell\NillyCodePro" /f 2>$null | Out-Null
Remove-Item -LiteralPath 'HKCU:\Software\Classes\Directory\shell\NillyCodePro' -Recurse -Force
Remove-ItemProperty -Path 'HKCU:\Software\RegisteredApplications' -Name 'NillyCodePro' -Force

# Clean OpenWithProgids entries
Get-ChildItem 'HKCU:\Software\Classes' -ErrorAction SilentlyContinue | Where-Object { $_.PSChildName.StartsWith('.') } | ForEach-Object {
    $owp = Join-Path $_.PSPath 'OpenWithProgids'
    if (Test-Path $owp) {
        Remove-ItemProperty -Path $owp -Name 'NillyCodePro.Document' -Force -ErrorAction SilentlyContinue
    }
}

# Remove shortcuts
$desktopLnk = Join-Path ([Environment]::GetFolderPath('Desktop')) 'NillyCode Pro.lnk'
$startLnk = Join-Path ([Environment]::GetFolderPath('Programs')) 'Nilly graphics\NillyCode Pro.lnk'
if (Test-Path $desktopLnk) { Remove-Item $desktopLnk -Force }
if (Test-Path $startLnk) { Remove-Item $startLnk -Force }

try {
    Add-Type -Namespace NillyCodeShell -Name Notify -MemberDefinition '[System.Runtime.InteropServices.DllImport("shell32.dll")] public static extern void SHChangeNotify(int wEventId, int uFlags, System.IntPtr dwItem1, System.IntPtr dwItem2);' -ErrorAction SilentlyContinue
    [NillyCodeShell.Notify]::SHChangeNotify(0x08000000, 0x2000, [IntPtr]::Zero, [IntPtr]::Zero)
} catch {}

Write-Host '  OK -> Αφαιρέθηκαν καθαρά όλες οι εγγραφές & συντομεύσεις του NillyCode Pro.' -ForegroundColor Green
Write-Host ''
if (-not $Silent) { Read-Host '  Πάτησε Enter για κλείσιμο' }
