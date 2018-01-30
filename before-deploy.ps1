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
	docker pull "cisien/csdiscord:$tag"
	docker stop CSDiscord
	docker rm CSDiscord
	docker run -d -e "tokens=$env:TOKENS" -e "log_webhook_token=$env:LOG_WEBHOOK_TOKEN" -p 31337:80/tcp --name=CSDiscord --net=nat --net-alias=CSDiscord --hostname=CSDiscord --restart=always --memory=4g --health-interval=2m "cisien/csdiscord:$tag"
}
