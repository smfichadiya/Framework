﻿#!/bin/bash
# See also: https://unix.stackexchange.com/questions/27054/bin-bash-no-such-file-or-directory

echo "### Build.sh"

# ubuntu, dotnet, npm, node version check
lsb_release -a
dotnet --version
npm --version
node --version

BASH_XTRACEFD=1 # Print execute command to stdout. Not to stderr.
set -x # Enable print execute cammands to stdout.

FolderName=$(pwd)"/" # Working directory
FileNameErrorText=$FolderName"Error.txt"

function Main
{
	# Cli build
	echo "### Build.sh (Cli Build)"
	cd $FolderName
	cd Application.Cli
	dotnet build
	ErrorCheck

	# Config
	echo "### Build.sh (Config)"
    set +x # Prevent AzureGitUrl in log
    dotnet run --no-build -- config azureGitUrl="$AzureGitUrl" # Set AzureGitUrl
    set -x
	ErrorCheck

	# Build
	echo "### Build.sh (Build)"
	cd $FolderName
	cd Application.Cli
	dotnet run --no-build -- build
	ErrorCheck

	# Deploy
	echo "### Build.sh (Deploy)"
	cd $FolderName
	cd Application.Cli
	dotnet run --no-build -- deploy
	ErrorCheck
}

function ErrorCheck
{
    # Check exitstatus and stderr
	echo "### Build.sh (ErrorCheck)"
	
	if [ $? != 0 ] # Exit status
	then 
		exit $? 
	fi
	if [ -f $FileNameErrorText ] # File exists
	then
		if [ -s $FileNameErrorText ] # If Error.txt not empty
		then
			exit 1
		fi
	fi
}

function ErrorText
{
	echo "### Build.sh (ErrorText) - ExitStatus=$?"

    if [ -s $FileNameErrorText ] # If Error.txt not empty
	then
	set +x # Disable print command to avoid Error.txt double in log.
	echo "### Error"
	echo "$(<$FileNameErrorText)" # Print file Error.txt 
	exit 1 # Set exit code
	fi
}

trap ErrorText EXIT # Run ErrorText if exception

Main 2> $FileNameErrorText # Run main with stderr to Error.txt.
ErrorText