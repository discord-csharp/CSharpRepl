while($true) {
	dotnet CSDiscordService.dll

	if($LastExitCode -ne 0) {
		exit $LastExitCode
	}
}