PATH="/home/travis/.dotnet":$PATH

FolderName=$(pwd)

echo \#\#\# Build
cd $FolderName
cd Build
dotnet restore
dotnet build
# Set ConnectionString
dotnet run 01 "$ConnectionString" 
# InstallAll
dotnet run 02 

# Build RunSql
echo \#\#\# RunSql
cd $FolderName
cd Build
dotnet run 11

# Publish Server
echo \#\#\# Publish Server
cd $FolderName
cd Server
dotnet restore
dotnet build
dotnet publish
