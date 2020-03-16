setlocal
echo "Moving your mod's dll to its respective folder..."

SET modFolder=F:\Program Files\Steam\steamapps\common\GORN\Mods\GorniePathfinding\GorniePathfinding.dll
SET dllLocation=F:\Users\Imran\Desktop\Games\GORN Modding\2.0 Mods\GorniePathfinding\GorniePathfinding\bin\Debug\GorniePathfinding.dll
SET dllName=GorniePathfinding

move "%dllLocation%" "%modFolder%"

echo "%dllName% has been moved to %modFolder%!"
endlocal