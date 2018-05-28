@rem QPS.cmd [Class C IP Segment] [Max Port] [Threads]
@rem QPS.cmd 192.168.1 2048 20

set startPort=0

echo %date% %time% > QPortScan_%1.txt

for /L %%i in (1,1,254) do QPortScan.exe %1.%%i 0 %2 %3 >> QPortScan_%1.txt
