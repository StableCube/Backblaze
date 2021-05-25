#!/bin/bash

# Packs and pushes a package to nuget server
# Run with ./nuget-pack-push.sh <PACKAGE PATH>

set -e

echo ""
echo "Deleteing old nupkg files"
echo "-------------"
rm -f $1/bin/local/Release/*.nupkg

echo ""
echo "Cleaning old packages"
echo "-------------"
dotnet clean $1

echo ""
echo "Packing"
echo "-------------"
dotnet pack -c Release $1

echo ""
echo "Pushing to nuget"
echo "-------------"
dotnet nuget push -k $NUGET_ORG_KEY $1/bin/local/Release/*.nupkg

echo ""
echo "Clearing nuget cache"
echo "-------------"
dotnet nuget locals http-cache --clear