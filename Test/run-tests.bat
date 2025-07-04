@echo off
echo ===============================================
echo  SISTEMA DE MONITOREO - EJECUCION DE PRUEBAS
echo ===============================================
echo.

echo 1. Compilando proyecto de pruebas...
dotnet build Test/Test.csproj --configuration Release --verbosity quiet

if %ERRORLEVEL% neq 0 (
    echo ? Error en la compilacion
    pause
    exit /b 1
)

echo ? Compilacion exitosa
echo.

echo 2. Ejecutando todas las pruebas...
echo.

dotnet test Test/Test.csproj ^
    --configuration Release ^
    --logger "console;verbosity=detailed" ^
    --logger "trx;LogFileName=TestResults.trx" ^
    --results-directory TestResults ^
    --collect:"XPlat Code Coverage" ^
    --settings Test/coverlet.runsettings

if %ERRORLEVEL% neq 0 (
    echo.
    echo ? Algunas pruebas fallaron
    echo.
    echo Revisa los resultados detallados en: TestResults/
) else (
    echo.
    echo ? Todas las pruebas pasaron exitosamente
    echo.
)

echo.
echo 3. Generando reporte de cobertura...

if exist TestResults/ (
    echo Resultados de pruebas guardados en: TestResults/
    echo - TestResults.trx: Resultados detallados
    echo - coverage.cobertura.xml: Reporte de cobertura de codigo
)

echo.
echo ===============================================
echo  EJECUCION COMPLETA
echo ===============================================
pause