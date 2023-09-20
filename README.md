# ImaPo [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://choosealicense.com/licenses/mit/) ![Build and release](https://github.com/pleonex/imapo/workflows/Build%20and%20release/badge.svg)

> Tool to help to translate images via PO files in Weblate.

![screenshot](https://pleonex.dev/ImaPo/images/screenshot.png)

<!-- prettier-ignore -->
| Release | Package                                                                                                                                                         |
|---------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Stable  | ![GitHub commits since latest release (by SemVer)](https://img.shields.io/github/commits-since/pleonex/ImaPo/latest)                                            |
| Preview | ![GitHub commits since latest release (by SemVer including pre-releases)](https://img.shields.io/github/commits-since/pleonex/ImaPo/latest?include_prereleases) |

## Installation

In progress...

1. Install [Ubuntu Nerd Font](https://www.nerdfonts.com/font-downloads)

## Usage

In progress...

## Documentation

Feel free to ask any question in the
[project Discussion site!](https://github.com/pleonex/imapo/discussions)

## Build

The project requires to build .NET 6.0 SDK (Linux and MacOS require also Mono).
If you open the project with VS Code and you did install the
[VS Code Remote Containers](https://code.visualstudio.com/docs/remote/containers)
extension, you can have an already pre-configured development environment with
Docker or Podman.

To build, test and generate artifacts run:

```sh
# Only required the first time
dotnet tool restore

# Default target is Stage-Artifacts
dotnet cake
```

To just build and test quickly, run:

```sh
dotnet cake --target=BuildTest
```
