# Description: Dream Cheeky LED Powershell Outlook Script
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
	Write-Host "Initializing LED object"
	$led = New-Object DreamCheekyUSB.DreamCheekyLED -ArgumentList @(0)
	#$led = New-Object DreamCheekyUSB.DreamCheekyLED -ArgumentList @(0x1D34,0x0004,0) #Specify VID,PID,DeviceIndex
	#Issues with calling the device path constructor from powershell. Error was New-Object : Index was outside the bounds of the array.
	#This finally worked:
	#$argArray = New-Object string[] 1
	#$argArray[0] = '\\?\hid#vid_1d34&pid_0004#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}'
	#$led = New-Object -TypeName DreamCheekyUSB.DreamCheekyLED (,$argArray)
	#This also worked:
	#$led = New-Object -TypeName DreamCheekyUSB.DreamCheekyLED (,'\\?\hid#vid_1d34&pid_0004#7&451d3da&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}')
} else {
	#TODO: Make sure it is connected and working!
}

#Test LED:
if(!$led.Test()){
	write-error "Testing LED failed!"
}

#Setup Outlook COM object. TODO: Check for errors (Outlook not open, etc). See security issues: http://www.outlookcode.com/article.aspx?id=52 and http://technet.microsoft.com/en-us/library/ff657852.aspx
#Alternative: http://psoutlookmanager.codeplex.com/
#Folder format: $ns.Folders.Item("Personal Folders - Old").Folders.Item("Inbox")
Write-Host "Creating Outlook COM object"
$ol = New-Object -Com Outlook.Application
$ns = $ol.GetNameSpace('MAPI')
$mail1 = $ns.Stores[1]
$mail2 = $ns.Stores[2]
$inbox1 = $mail1.GetDefaultFolder([Microsoft.Office.Interop.Outlook.OlDefaultFolders]::olFolderInbox)
$inbox2 = $mail2.GetDefaultFolder([Microsoft.Office.Interop.Outlook.OlDefaultFolders]::olFolderInbox)


#TODO: store state so that you can cycle colors and support multiple accounts.
#TODO: extract name of account and set color based on name not order, which can change
Write-Host "Starting mail checking loop"
while($true){
	if($inbox1.UnReadItemCount -gt 0){
		Write-Host "New mail (Account 1)! Count =" ($inbox1.UnReadItemCount)
		$led.Blink([System.Drawing.Color]::Magenta,$inbox1.UnReadItemCount,500) | Out-Null;	
		$led.FadeIn([System.Drawing.Color]::Magenta,2000) | Out-Null;
	} elseif($inbox2.UnReadItemCount -gt 0){
		Write-Host "New mail (Account 2)! Count =" ($inbox2.UnReadItemCount)
		$led.Blink([System.Drawing.Color]::Navy,$inbox2.UnReadItemCount,500) | Out-Null;	
		$led.FadeIn([System.Drawing.Color]::Navy,2000) | Out-Null;
	} else {
		Write-Host "No mail waiting..."
		$led.Off() | Out-Null		
	}
	Start-Sleep -Seconds 15
}