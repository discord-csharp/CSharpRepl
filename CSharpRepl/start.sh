#!/bin/bash
dotnet CSharpRepl.dll
rm -rf /tmp/* > /dev/null 2>&1
rm -rf /var/* > /dev/null 2>&1