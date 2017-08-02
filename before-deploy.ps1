$ErrorActionPreference = 'Continue';

if(-not [System.String]::IsNullOrWhitespace($env:APPVEYOR_PULL_REQUEST_NUMBER)) {
	return;
}

$tag = $env:APPVEYOR_REPO_BRANCH
if([System.String]::IsNullOrWhitespace($tag)) {
    $tag = "untagged"
}
if (Enter-OncePerDeployment "install_docker_image")
{
	docker stop CSDiscord
	docker rm CSDiscord
	docker pull "cisien/csdiscord:$tag"
	docker run -d -e "tokens=$env:TOKENS" -e "APPINSIGHTS_INSTRUMENTATIONKEY=e210bb4c-ee0f-4d1f-a892-9b9728fec526" -p 80:80/tcp --name CSDiscord --hostname CSDiscord --restart always "cisien/csdiscord:$tag"
}