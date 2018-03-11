FROM microsoft/aspnetcore:2.0-nanoserver-1709
ARG source
WORKDIR /app
EXPOSE 80
COPY obj/Docker/publish .
#RUN reg add HKLM\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters /v ServerPriorityTimeLimit /t REG_DWORD /d 0 /f 
#HEALTHCHECK CMD powershell iwr -uri http://localhost/Eval -method POST -contentType text/plain -body 1+1 -UseBasicParsing -Headers @{'Authorization'='Token '+$(($env:Tokens -split ';')[0])}
ENTRYPOINT ["C:/Program Files/dotnet/dotnet", "CSDiscordService.dll"]
