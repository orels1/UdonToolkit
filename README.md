![](https://cdn.vrchat.sh/ut/promo/github_banner.png)
<hr>
<p align="center">
  <strong>Purpose-built Udon Behaviours and tools to make your own</strong>
</p>

<p align="center">
  <sub>Built with ❤︎ by
  <a href="https://twitter.com/orels1_">orels1</a> and
  <a href="https://github.com/orels1/UdonToolkit/graphs/contributors">
    contributors
  </a>
  </sub>
</p>

## Quick Start Guide

### Requirements

- [VRC SDK3 with Udon](https://vrchat.com/home/download) (v 2021.06.03.14.57+)
- [Udon Sharp Compiler](https://github.com/Merlin-san/UdonSharp) (v 0.19.12+)
- [Optional] PostProcessing from Unity Package Manager (required for the Camera System)

## Installation

- Grab the latest package [from releases](https://github.com/orels1/UdonToolkit/releases)
- Import it into the project
> **If you're upgrading from version prior to 1.x.x - [you'll need to follow these upgrade steps to upgrade](https://ut.orels.sh/v/v1.x/extras/migration-to-v1.0.0)**
- When importing you can select what to import:
  - **Internal: REQUIRED**. Contains all the editor functionality
  - **Systems**: Various systems listed in the [SYSTEMS section of the documentation](https://ut.orels.sh/v/v1.x/systems/camera-system)
  - **Behaviours**: A collection of scripts that perform actions in the world. You can find the full list in the [BEHAVIOURS section of the documentation](https://ut.orels.sh/v/v1.x/behaviours/overview)
  - **Demo**: assets and examples scenes for all the systems and behaviours. You should import this only if you are importing the full package
- Open your `Project Settings` -> `UdonSharp` and in the `Default Behaviour Editor` select `UdonToolkit Editor`

### [Check out the wiki for all the information](https://ut.orels.sh/v/v1.x/)

### [If you still have questions - join the discord](https://discord.com/invite/fR869XP)

## Copyright

Copyright (c) 2020 orels1

Licensed under the [MIT license](LICENSE).
