;Sample AutoHotKey script for DreamCheekyBTN.exe
;Author: Greg Bray 2012SEP07
;Usage: Copy to your My Documents folder and then start AutoHotKey
;See http://www.autohotkey.com/docs/ for full details
;!=Alt, ^=Ctrl, +=Shift, #=Win
Process, Close, DreamCheekyBTN.exe		;Kill any existing DreamCheekyBTN process whenever this script is loaded/reloaded

Run, D:\Projects\DreamCheekyUSB\DreamCheekyBTN\bin\Release\DreamCheekyBTN.exe MACRO=`%+{F1} ;,,Hide
;Above will start the DreamCheekyBTN program to listen for button presses and convert them to Alt+Shift+F1
;Change ;,,Hide to just ,,Hide to start program in hidden mode.

;^!r::Reload		;Ctrl+Alt+r = Reload this AutoHotKey script. Uncomment to enable fast debugging

!+F1::Run www.google.com		;Alt+Shift+F1 from DreamCheekyBTN opens google.com