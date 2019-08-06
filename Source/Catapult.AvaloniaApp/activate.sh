#!/bin/bash

set -x

echo 'CATAPULT' | nc -U /tmp/catapultsocket
if [ $? -ne 0 ]
then
  cd "$(dirname "$0")"
  dotnet run --framework netcoreapp3.0
fi