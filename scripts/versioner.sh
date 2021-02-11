#!/bin/bash
VERSION_3_DIGIT=$1
VERSION_4_DIGIT=$2
VERSION_NUGET=$3
VERSION_COMMON_NUGET=$4

if [[ -z $VERSION_3_DIGIT || -z VERSION_4_DIGIT || -z VERSION_NUGET || -z VERSION_COMMON_NUGET ]];
then
   read -p "7 arguments required: <VERSION_3_DIGIT> <VERSION_4_DIGIT> <VERSION_NUGET> <VERSION_COMMON_NUGET>"
   exit 1
fi

echo "VERSION_3_DIGIT $VERSION_3_DIGIT"
echo "VERSION_4_DIGIT $VERSION_4_DIGIT"
echo "VERSION_NUGET $VERSION_NUGET"
echo "VERSION_COMMON_NUGET $VERSION_COMMON_NUGET"

sed -i -b "s|<Version>.*</Version>|<Version>$VERSION_4_DIGIT</Version>|g" ../src/Directory.Build.props
sed -i -b "s|<FileVersion>.*</FileVersion>|<FileVersion>$VERSION_4_DIGIT</FileVersion>|g" ../src/Directory.Build.props
sed -i -b "s|<InformationalVersion>.*</InformationalVersion>|<InformationalVersion>$VERSION_3_DIGIT</InformationalVersion>|g" ../src/Directory.Build.props

sed -i -b "s|<version>.*</version>|<version>$VERSION_NUGET</version>|g" ../build/GoldenS3*.nuspec
sed -i -b -r "s|(<dependency id=\"ABSA.RD.GoldenS3.*\" version=\").*(\" ?/>)|\1$VERSION_NUGET\2|g" ../build/GoldenS3*.nuspec
sed -i -b -r "s|(<dependency id=\"ABSA.RD.Common.*\" version=\").*(\" ?/>)|\1$VERSION_COMMON_NUGET\2|g" ../build/GoldenS3*.nuspec

find ../src -name "*.csproj" -exec sed -i -b -r "s|(<PackageReference Include=\"ABSA.RD.Common.*\" Version=\").*\" />|\1$VERSION_COMMON_NUGET\" />|g" {} +

echo "Finished"