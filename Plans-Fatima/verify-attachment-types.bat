@echo off
REM Verify AttachmentTypes in Database
echo.
echo ====== Checking Database for AttachmentTypes ======
sqlcmd -S "(localdb)\MSSQLLocalDB" -d BOG -Q "SELECT [Id], [Name], [NameAr], [IsMandatory], [IsActive] FROM [dbo].[AttachmentTypes] WHERE [IsDeleted] = 0 ORDER BY [Id]"

echo.
echo ====== Testing API Endpoint ======
echo Starting BOG.API...
cd /d C:\Users\Lenovo\Desktop\Claude\BOG\src\Backend\BOG.API

REM Kill any existing dotnet processes on port 5001
echo Checking for existing processes...
netstat -ano | findstr ":5001" && echo "Found process on port 5001" && taskkill /PID <PID> /F || echo "No process on port 5001"

REM Start the API
timeout /t 2
start cmd /k "dotnet run"

REM Wait for API to start
echo Waiting for API to start...
timeout /t 10

REM Test the endpoint
echo Testing http://localhost:5001/api/lookups/attachment-types
curl http://localhost:5001/api/lookups/attachment-types

pause
