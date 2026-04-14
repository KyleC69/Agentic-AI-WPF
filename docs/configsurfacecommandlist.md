




Got it. You want **a single, flat, no‑format list** of **commands + parameters**, combining:

1. The *main diagnostic/config surfaces*  
2. The *obscure pinpoint tools*  

No categories, no explanations, no markdown — just the raw commands and switches.

Here it is.

---

## ✅ **Unified Raw Command/Parameter List (no formatting, no grouping)**

reg query HKLM\SYSTEM  
reg query HKLM\SOFTWARE  
reg query HKLM\HARDWARE  
reg query HKLM\SECURITY  
reg query HKLM\SAM  
reg query HKLM\BCD00000000  
reg query HKLM\COMPONENTS  
reg query HKLM\DRIVERS  
reg query HKLM\BCD-template  
reg query HKU\.DEFAULT  
reg query HKU\S-1-5-18  
reg query HKU\S-1-5-19  
reg query HKU\S-1-5-20  

wmic process list full  
wmic service list full  
wmic qfe list  
wmic product get name,version  
wmic logicaldisk get caption,freespace,size  
wmic nicconfig list full  
wmic startup list full  
wmic useraccount list full  
wmic group list full  
wmic volume list full  
wmic shadowcopy list  

Get-CimInstance -Namespace root\wmi -ClassName *  
Get-CimInstance -Namespace root\Subscription -ClassName *  
Get-CimInstance -Namespace root\RSOP -ClassName *  
Get-CimInstance -Namespace root\Policy -ClassName *  
Get-CimInstance -Namespace root\Microsoft\Windows\Defender -ClassName *  
Get-CimInstance -Namespace root\Microsoft\Windows\BitLocker -ClassName *  
Get-CimInstance -Namespace root\Microsoft\Windows\DeviceGuard -ClassName *  
Get-CimInstance -Namespace root\Microsoft\Windows\WindowsUpdate -ClassName *  

logman query providers  
logman query providers -ets  
logman start trace -p Microsoft-Windows-Kernel-Process  
logman start trace -p Microsoft-Windows-Kernel-File  
logman start trace -p Microsoft-Windows-Kernel-Network  
logman start trace -p Microsoft-Windows-Kernel-Power  
logman start trace -p Microsoft-Windows-Security-Auditing  
logman start trace -p Microsoft-Windows-FilterManager  
logman start trace -p Microsoft-Windows-ApplicationExperience  
logman start trace -p Microsoft-Windows-DriverFrameworks-UserMode  
logman start trace -p Microsoft-Windows-DriverFrameworks-KernelMode  
logman start trace -p Microsoft-Windows-Storage-ClassPnP  
logman start trace -p Microsoft-Windows-StorageSpaces-Driver  

wevtutil el  
wevtutil qe Security /f:text  
wevtutil qe System /f:text  
wevtutil qe Application /f:text  
wevtutil qe Microsoft-Windows-GroupPolicy/Operational /f:text  
wevtutil qe Microsoft-Windows-DNS-Client/Operational /f:text  
wevtutil qe Microsoft-Windows-TaskScheduler/Operational /f:text  
wevtutil qe Microsoft-Windows-PrintService/Admin /f:text  
wevtutil gl Microsoft-Windows-Kernel-Boot  
wevtutil gl Microsoft-Windows-Kernel-PnP  

icacls C:\Windows  
icacls C:\Windows\System32  
icacls HKLM:\SYSTEM  
icacls HKLM:\SOFTWARE  
icacls HKLM:\SECURITY  
icacls HKLM:\SAM  

auditpol /get /category:*  
auditpol /backup /file:audit.cfg  
auditpol /restore /file:audit.cfg  

netsh interface ipv4 show config  
netsh interface ipv6 show config  
netsh interface ipv4 show subinterfaces  
netsh interface tcp show global  
netsh interface teredo show state  
netsh interface httpstunnel show interfaces  
netsh interface portproxy show all  
netsh namespace show effectivepolicy  
netsh winhttp show proxy  
netsh trace start capture=yes  
netsh advfirewall firewall show rule name=all  
netsh advfirewall monitor show firewall  

ipconfig /all  
ipconfig /displaydns  
arp -a  
route print  
nbtstat -n  
nbtstat -c  
nbtstat -r  

Get-NetAdapter  
Get-NetTCPSetting  
Get-NetFirewallRule  
Get-NetIPConfiguration  
Get-NetRoute  
Get-DnsClientNrptPolicy  
Get-SmbConnection  
Get-SmbSession  
Get-SmbClientConfiguration  

schtasks /query /fo list /v  
schtasks /query /xml /tn *  
schtasks /query /tn "\Microsoft\Windows\Defrag\ScheduledDefrag" /xml  
schtasks /query /tn "\Microsoft\Windows\*" /fo list  

sc query  
sc qc <service>  
sc qfailure <service>  
sc enumdepend <service>  
sc query type= driver  
sc query type= service  

driverquery /v /fo csv  
pnputil /enum-drivers  
pnputil /enum-devices /connected  
fltmc filters  
fltmc volumes  
devcon find *  
devcon status *  

dism /online /get-intl  
dism /online /get-drivers  
dism /online /get-packages  
dism /online /get-features  
dism /online /cleanup-image /checkhealth  
dism /online /cleanup-image /scanhealth  
dism /online /cleanup-image /restorehealth  

gpresult /r  
gpresult /scope computer /v  
gpresult /h report.html  
gpupdate /force  
lgpo.exe /parse /q  

bcdedit /enum all  
bcdedit /enum firmware  
bcdedit /enum {bootmgr}  
bcdedit /enum osloader  
reagentc /info  

powercfg /energy  
powercfg /batteryreport  
powercfg /sleepstudy  
powercfg /devicequery wake_armed  
powercfg /requests  

fsutil fsinfo drives  
fsutil fsinfo volumeinfo C:  
fsutil reparsepoint query C:\path  
fsutil usn queryjournal C:  
fsutil usn readjournal C:  
fsutil resource setautoreset true C:\  

vssadmin list writers  
vssadmin list providers  
vssadmin list shadows  

esentutl /mh <db>  
esentutl /ms <db>  
esentutl /g <db>  

systeminfo  
whoami /groups  
whoami /priv  
cmdkey /list  
net accounts  
net user  
net localgroup  

CheckNetIsolation LoopbackExempt -s  
Get-AppxPackage -AllUsers  
DISM /Online /Get-ProvisionedAppxPackages  

wmic shadowcopy list  
diskshadow /l  
diskshadow /s script.txt  

sigverif  
sfc /verifyonly  
sfc /scannow  

winver  
ver  

---

If you want, I can now:

✔ remove duplicates  
✔ alphabetize  
✔ group by subsystem  
✔ convert into a machine‑readable JSON list  
✔ map each command to a tool operation name  
✔ generate a consolidated tool schema for your agent

Just tell me which direction you want.







