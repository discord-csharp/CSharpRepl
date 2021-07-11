<!--
Based on the README.md template found at https://github.com/othneildrew/Best-README-Template/blob/master/README.md
-->

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![Build Status][build-sheld]][build-url]


<br />

<!-- TABLE OF CONTENTS -->
<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgements">Acknowledgements</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

CSharpRepl is a web service that provides C# interactive features over http. This service was developed to provide REPL capability in the [C# Discord](https://discord.gg/csharp) via the [MODiX Discord Bot](https://github.com/discord-csharp/MODiX).


<!-- GETTING STARTED -->
## Getting Started

Clone and open the project in your favorite IDE/Editor!

### Prerequisites

1. Install Visual Studio Code, Visual Studio 2019, or Rider
1. Install the dotnet SDK
1. Install Docker
1. (Optional) Install docker-compose

### Installation

Note: The container will cleanly exit after every request, or after 30 seconds with an active request. Use `--restart=always` to ensure the container remains available.
Command line:

```sh
docker run -d --user www-data --restart=always --read-only --tmpfs /tmp --tmpfs /var --memory 500M --cpus 2 -p 31337:31337 -e ASPNETCORE_URLS=http://+:31337 ghcr.io/discord-csharp/csharprepl:latest
```

Docker Compose:
```yml
version: '3.7'
services:
  repl:
    image: ghcr.io/discord-csharp/csharprepl:latest
    restart: always
    read_only: true
    user: www-data
    environment: 
      - ASPNETCORE_URLS=http://+:31337
    ports:
      - '31337:31337'
    tmpfs:
      - /tmp
      - /var
```


<!-- USAGE EXAMPLES -->
## Usage

```sh
curl -X POST -H 'Content-Type: text/plain' -d 'Console.WriteLine("Hello World");' http://localhost:31337/eval
```

```pwsh
Invoke-WebRequest -UseBasicParsing -ContentType text/plain -Method POST -Body "Console.WriteLine(`"Hello World`");" -Uri http://localhost:31337/eval | Select-Object Content
```


<!-- ROADMAP -->
## Roadmap

See the [open issues](https://github.com/discord-csharp/CSharpRepl/issues) for a list of proposed features (and known issues).



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to be learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request



<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE` for more information.



<!-- CONTACT -->
## Contact

Chris Curwick - [@Cisien on Discord](https://discord.gg/csharp)

Project Link: [https://github.com/discord-csharp/CSharpRepl](https://github.com/discord-csharp/CSharpRepl)



<!-- ACKNOWLEDGEMENTS -->
## Acknowledgements
* [dotnet](https://github.com/dotnet)
* [.NET Foundation](https://dotnetfoundation.org)
* [Img Shields](https://shields.io)



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/discord-csharp/CSharpRepl.svg?style=for-the-badge
[contributors-url]: https://github.com/discord-csharp/CSharpRepl/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/discord-csharp/CSharpRepl.svg?style=for-the-badge
[forks-url]: https://github.com/discord-csharp/CSharpRepl/network/members
[stars-shield]: https://img.shields.io/github/stars/discord-csharp/CSharpRepl.svg?style=for-the-badge
[stars-url]: https://github.com/discord-csharp/CSharpRepl/stargazers
[issues-shield]: https://img.shields.io/github/issues/discord-csharp/CSharpRepl.svg?style=for-the-badge
[issues-url]: https://github.com/discord-csharp/CSharpRepl/issues
[license-shield]: https://img.shields.io/github/license/discord-csharp/CSharpRepl.svg?style=for-the-badge
[license-url]: https://github.com/discord-csharp/CSharpRepl/blob/master/LICENSE.txt
[build-sheld]: https://img.shields.io/github/workflow/status/discord-csharp/CSharpRepl/build-csharprepl/main?style=for-the-badge
[build-url]: https://github.com/discord-csharp/CSharpRepl/actions/workflows/docker-publish.yml
