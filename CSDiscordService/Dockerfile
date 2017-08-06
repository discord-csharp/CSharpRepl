FROM microsoft/aspnetcore:2.0
ARG source
WORKDIR /app
EXPOSE 80
COPY ${source:-obj/Docker/publish} .
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]
RUN set-itemproperty -path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord
HEALTHCHECK CMD powershell iwr -uri http://localhost/Eval -method POST -contentType text/plain -body 1+1 -UseBasicParsing -Headers @{'Authorization'='Token '+$(($env:Tokens -split ';')[0])}
ENTRYPOINT [ "dotnet", "CSDiscordService.dll"]
