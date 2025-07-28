# Ventoy Multi-Format Hash Calculator

A cross-platform tool to calculate multiple hash types (MD5, SHA1, SHA256, SHA512) for ISO, WIM, IMG, VHD/VHDX, and EFI files on Ventoy-supported drives.

## Features

- Calculates MD5, SHA1, SHA256, SHA512 hashes
- Supports ISO, WIM, IMG, VHD/VHDX, and EFI files
- Interactive and command-line modes
- File filtering by type, pattern, and size
- Option to skip files with existing hash files
- Progress bar and ETA display
- Windows, Linux, and macOS support
- Runs as a self-contained single executable with .NET 9.0

## Installation

### Pre-built Releases

1. Download the appropriate file for your OS from the [Releases](https://github.com/FurkanHuman/Ventoy-MultiHash-Calculator/releases) page.
2. Extract the `.zip` (Windows) or `.tar.gz` (Linux/macOS) archive.
3. On Linux/macOS, make the file executable:

   ```sh
   chmod +x Ventoy_MultiHash_Calculator
   ```

### Build from Source

Requires .NET 9.0 SDK.

```sh
git clone https://github.com/FurkanHuman/Ventoy-MultiHash-Calculator.git
cd Src/Ventoy_Hash_Calculator
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Usage

### Basic Usage

```sh
Ventoy_MultiHash_Calculator.exe [OPTIONS]
```

### Options

| Option                   | Description                                              |
|--------------------------|---------------------------------------------------------|
| -h, --help               | Show help message                                       |
| -i, --interactive        | Interactive mode (default)                              |
| -d, --drive LETTER       | Specify drive letter (e.g., E)                          |
| --dir PATH               | Specify directory path                                  |
| -f, --filter TYPE        | Filter by file type (all, iso, wim, img, vhd, ...)      |
| -p, --pattern PATTERN    | File name pattern (wildcard supported)                  |
| --skip-existing          | Skip files with existing hash files                     |
| --min-size MB            | Minimum file size (MB)                                  |
| --max-size MB            | Maximum file size (MB)                                  |
| --hashes md5,sha1,...    | Hash types to calculate                                 |
| --quiet                  | Less output                                             |
| --verbose                | Verbose output                                          |

### Examples

Calculate hashes for all ISO files:

```sh
Ventoy_MultiHash_Calculator.exe --filter iso
```

For a specific drive:

```sh
Ventoy_MultiHash_Calculator.exe --drive E
```

With specific hash types:

```sh
Ventoy_MultiHash_Calculator.exe --hashes sha256,sha512
```

## Supported Platforms

- Windows x64 / ARM64
- Linux x64 / ARM64
- macOS x64 / ARM64

## License

MIT License. See [LICENSE](LICENSE) for details.

---

Developer: Furkan Bozkurt  
Feel free to open issues or pull requests for feedback and// Copyright 2025 Furkan Bozkurt
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     <https://www.apache.org/licenses/LICENSE-2.0>
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
