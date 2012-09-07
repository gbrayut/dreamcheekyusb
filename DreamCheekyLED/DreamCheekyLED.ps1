# Description: Dream Cheeky LED Powershell Sample Script
# Author: Greg Bray
# Date: 8/9/2012
# License: Apache 2.0

if(([appdomain]::currentdomain.getassemblies() | Where {$_ -match "DreamCheekyLED"}) -eq $null){
	Write-Host "Loading DreamCheekyLED Assembly"
	$assembly = [Reflection.Assembly]::LoadFile("D:\Projects\DreamCheekyUSB\DreamCheekyLED\bin\release\DreamCheekyLED.exe")
}

#List all USB devices with VID=0x1D34
#[HidLibrary.HidDevices]::Enumerate() | where { $_.Attributes.VendorHexId -eq "0x1D34" }| ft isConnected,isOpen,Description -AutoSize

#Initialize led object
if($led -eq $null){
	#$led = New-Object DreamCheekyUSB.DreamCheekyLED #Find default device
	#$led = New-Object DreamCheekyUSB.DreamCheekyLED -ArgumentList @(0x1D34,0x0004,0) #Specify VID,PID,DeviceIndex
	#Issues with calling the device path constructor from powershell. Error was New-Object : Index was outside the bounds of the array.
	#This finally worked:
	$argArray = New-Object string[] 1
	$argArray[0] = '\\?\hid#vid_1d34&pid_0004#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}'
	$led = New-Object -TypeName DreamCheekyUSB.DreamCheekyLED (,$argArray)
	
}

#Run tests:
#$led.Test();
#start-sleep -Seconds 2
$led.TestBlink();

if($false){
$led.SetColor([System.Drawing.Color]::Red)
start-sleep -Seconds 1
$led.SetColor([System.Drawing.Color]::Green)
start-sleep -Seconds 1
$led.SetColor([System.Drawing.Color]::Blue)
start-sleep -Seconds 1
}

$led.Off()