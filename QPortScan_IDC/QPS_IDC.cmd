@rem QPS.cmd [Class C IP Segment] [Max Port] [Threads]
@rem QPS.cmd 192.168.1 %Threads%48 30

@echo off
set minPort=0
set maxPort=1025
set Threads=50

set DIR_Target=.\target\

set ip_address_string="IPv4"
set myip=""

for /f "usebackq tokens=2 delims=:(" %%f in (`ipconfig /all ^| findstr /c:%ip_address_string%`) do (
    set myip= %%f
	goto :SET_IP
)
:SET_IP
set "myip=%myip: =%"

mkdir %myip%
echo %myip%
set "myip=.\%myip%\%myip%"


@REM [Scan]: MIS Zone
set Target_Zone=HQ-MIS
set ClassC_IP=192.168.2

echo %date% %time% > %myip%_QPortScan_%Target_Zone%_%ClassC_IP%.txt
echo --- >> %myip%_QPortScan_%Target_Zone%_%ClassC_IP%.txt

QPortScan.exe %DIR_Target%\HQ-MIS_Zone.txt 0 %maxPort% %Threads% >> %myip%_QPortScan_%Target_Zone%_%ClassC_IP%.txt

echo === >> %myip%_QPortScan_%Target_Zone%_%ClassC_IP%.txt
echo %date% %time% >> %myip%_QPortScan_%Target_Zone%_%ClassC_IP%.txt




@echo on