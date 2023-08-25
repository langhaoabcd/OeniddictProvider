FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY . .

RUN ln -sf /usr/share/zoneinfo/Asia/ShangHai /etc/localtime
RUN echo "Asia/Shanghai" > /etc/timezone
RUN dpkg-reconfigure -f noninteractive tzdata

ENV ASPNETCORE_URLS http://+:7098
EXPOSE 7098
ENTRYPOINT ["dotnet", "OeniddictProvider.dll"]
