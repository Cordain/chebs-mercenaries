#!/bin/bash
set -e
display_usage()
{
    ua=(
	target_path
	target_assembly
	valheim_path
	project_path
	deploy_path
	)
    echo "Usage: $0 ${ua[*]}"
    uv=(
        "ChebsMercenaries/bin/Release"
	"ChebsMercenaries.dll"
	"/home/$USER/.local/share/Steam/steamapps/common/Valheim"
	"$(pwd)"
	"/home/$USER/.local/share/Steam/steamapps/common/Valheim/BepInEx/plugins"
       )
    echo "Example values:"
    ua_len=${#ua[@]}
    for (( i=0; i<${ua_len}; i++ )); do
      echo "    ${ua[$i]} = ${uv[$i]}"
    done
}

# show help message if specified or args <= 5
if [[ ( $@ == "--help") ||  $@ == "-h" || $# -le 4 ]]
then
	display_usage
	exit 0
fi

TARGETPATH=$1
TARGETASSEMBLY=$2
VALHEIMPATH=$3
PROJECTPATH=$4
DEPLOYPATH=$5
VERSION=$6

# verify each argument
if [ ! -d "$TARGETPATH" ]; then
  echo "$TARGETPATH does not exist."
  exit 1
fi

if [ ! -d "$VALHEIMPATH" ]; then
  echo "$VALHEIMPATH does not exist."
  exit 1
fi

if [ ! -d "$PROJECTPATH" ]; then
  echo "$PROJECTPATH does not exist."
  exit 1
fi

cd "$(dirname "$0")"

# plugin name without .dll
name=$( basename "$TARGETASSEMBLY" .dll )

# target
TARGET=$( basename "$TARGETPATH" )

# handle each target differently
if [ $TARGET == "Release" ]; then
  echo "Packaging for Thunderstore..."
  packagePath="$PROJECTPATH/$name/Package"
  if [ -d "$packagePath/plugins" ]; then
    rm -rf "$packagePath/plugins"
  fi
  mkdir "$packagePath/plugins"
  mkdir "$packagePath/plugins/$name"
  cp "$TARGETPATH/$TARGETASSEMBLY" "$packagePath/plugins/$name"
  cp "README.md" "$packagePath"
  cp -r "$PROJECTPATH/$name/Assets" "$packagePath/plugins/$name"
  if [ ! -z "$VERSION" ]; then
    VERSION=".$VERSION"
  fi
  zipLocation="$TARGETPATH/$TARGETASSEMBLY$VERSION.zip"
  echo "Zipping to $PROJECTPATH/$zipLocation"
  cd $packagePath
  zip -r "$PROJECTPATH/$zipLocation" .
fi

if [ $TARGET == "Debug" ]; then
  if [ $DEPLOYPATH == "" ]; then
    $DEPLOYPATH="$VALHEIMPATH/BepInEx/plugins/"
  fi
  plug="$DEPLOYPATH/$name"
  # create plug and copy files there
  if [ -d "$plug" ]; then
	rm -rf $plug
  fi
  mkdir $plug
  cp "$TARGETPATH/$name.dll" $plug
  cp "$TARGETPATH/$name.pdb" $plug
  # mdb seems missing on Linux
  #cp -f "$TARGETPATH/$name.dll.mdb" $plug
  cp -r "$PROJECTPATH/$name/Assets" "$plug/Assets"
fi

echo "Finished"
