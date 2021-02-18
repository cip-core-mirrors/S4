#!/bin/bash
VERSION_3_DIGIT=$1
VERSION_4_DIGIT=$2
VERSION_NUGET=$3

if [[ -z $VERSION_3_DIGIT || -z VERSION_4_DIGIT || -z VERSION_NUGET ]];
then
   read -p "7 arguments required: <VERSION_3_DIGIT> <VERSION_4_DIGIT> <VERSION_NUGET>"
   exit 1
fi

echo "VERSION_3_DIGIT $VERSION_3_DIGIT"
echo "VERSION_4_DIGIT $VERSION_4_DIGIT"
echo "VERSION_NUGET $VERSION_NUGET"

sed -i -b "s|<Version>.*</Version>|<Version>$VERSION_4_DIGIT</Version>|g" ../src/Directory.Build.props
sed -i -b "s|<FileVersion>.*</FileVersion>|<FileVersion>$VERSION_4_DIGIT</FileVersion>|g" ../src/Directory.Build.props
sed -i -b "s|<InformationalVersion>.*</InformationalVersion>|<InformationalVersion>$VERSION_3_DIGIT</InformationalVersion>|g" ../src/Directory.Build.props

sed -i -b "s|<version>.*</version>|<version>$VERSION_NUGET</version>|g" ../build/S4*.nuspec
sed -i -b -r "s|(<dependency id=\"ABSA.RD.S4.*\" version=\").*(\" ?/>)|\1$VERSION_NUGET\2|g" ../build/S4*.nuspec

echo "Finished"