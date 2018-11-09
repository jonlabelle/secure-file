#!/usr/bin/env bash

set -e
set -o pipefail

readonly PROJECT_ROOT="$(cd "$(dirname "${0}")"; echo "$(pwd)")"

delete_dir_if_exists() {
    local dirpath="${1}"

    if [ -d "$dirpath" ]; then
        echo "Deleting: $dirpath"
        rm -rf "$dirpath"
    fi
}

pushd "$PROJECT_ROOT"

delete_dir_if_exists "build"
delete_dir_if_exists "secure-file/bin"
delete_dir_if_exists "secure-file/obj"

dotnet restore
dotnet publish secure-file -f netcoreapp2.0 -o ../build -c Release

if [ -f "build/secure-file" ]; then
    echo "Making 'build/secure-file' executable..."
    chmod +x build/secure-file
fi
