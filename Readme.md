# Trash Dash Sample Project

## Overview

Trash Dash is a sample project that demonstrates how to integrate Metaplay into an existing Unity game. Based on Unity's [Trash Dash endless runner sample](https://github.com/Unity-Technologies/EndlessRunnerSampleGame), this project showcases an incremental adoption approach that focuses on quick wins without requiring extensive refactoring.

## Key Features

- **Segmentation** - Create and manage player segments that update dynamically during gameplay
- **LiveOps Events** - Configure and schedule dynamic in-game events from the LiveOps Dashboard
- **Game Configs** - Build and update game economy configurations without client releases
- **A/B Testing** - Run experiments with alternative config values for specific player segments
- **Dashboard Customization** - Rich player state visualization with custom LiveOps Dashboard components

## Setup Instructions

1. Clone the repository: `git clone git@github.com:metaplay-shared/trashdash-sample.git`
2. Install the [Metaplay CLI](https://github.com/metaplay/cli)
3. Initialize the MetaplaySDK: `metaplay init sdk --sdk-version=34.2`
4. Run the server: `metaplay dev server`
5. The server will start and you can access the dashboard at [localhost:5550](http://localhost:5550/)

## Requirements

- Unity 2022.3.42f1 or later
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Node.js v22](https://nodejs.org/en/download/) (for LiveOps Dashboard)
- PNPM: `npm install -g pnpm` (for LiveOps Dashboard)

## Documentation

For detailed integration steps and tutorials, please refer to the [Metaplay Documentation](http://docs.metaplay.io/introduction/samples/trash-dash/).
