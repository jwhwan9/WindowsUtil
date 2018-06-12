set CertName=InfoSecIR

makecert.exe -n "CN=%CertName%,OU=Tool,O=InfoSec,C=TW" -r -sv %CertName%.pvk %CertName%.cer
pvk2pfx.exe -pvk %CertName%.pvk -spc %CertName%.cer -pfx %CertName%.pfx -f

