$ErrorActionPreference = 'Stop';

#if (! (Test-Path Env:\APPVEYOR_REPO_TAG_NAME)) {
#  Write-Host "No version tag detected. Skip publishing."
#  exit 0
#}
$tag = $env:APPVEYOR_REPO_TAG_NAME

if([System.String]::IsNullOrWhitespace($tag)) {
    $tag = "untagged"
}

$env | out-host
Write-Host Starting deploy
docker login -u="$env:DOCKER_USER" -p="$env:DOCKER_PASS"

docker tag csdiscordservice cisien/csdiscord:$tag

docker push cisien/csdiscord:$tag
