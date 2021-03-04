#!/bin/bash
#
#   Copyright 2021 Absa Group
#
#   Licensed under the Apache License, Version 2.0 (the "License");
#   you may not use this file except in compliance with the License.
#   You may obtain a copy of the License at
#
#       http://www.apache.org/licenses/LICENSE-2.0
#
#   Unless required by applicable law or agreed to in writing, software
#   distributed under the License is distributed on an "AS IS" BASIS,
#   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#   See the License for the specific language governing permissions and
#   limitations under the License.

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