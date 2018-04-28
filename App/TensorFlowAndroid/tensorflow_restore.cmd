:: ##################################################################
::
:: Convenience batch script to call the powershell script
::
:: ##################################################################

@echo off

:: We need to bypass the execution policy to ensure the powershell instance
:: is allowed to execute scripts
powershell -ExecutionPolicy ByPass "& "".\tensorflow_restore.ps1"""