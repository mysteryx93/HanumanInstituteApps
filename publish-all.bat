@echo off
if [%1]==[] (
  @echo You must specify applcation version.
  exit /b 1
)


publish Player432hz %1
publish Converter432hz %1
publish PowerliminalsPlayer %1
