# Diskor

## Overview
.NET global tool that scans directories and lists folders that exceed a specified storage quota. It helps locate large directories.

## Features
- Scans all drives by default or a specified directory.
- Lists folders exceeding a default quota of 512MB.
- Supports custom quota specification in megabytes.
- Limits displayed depth (default: 5 levels deep).
- Displays sizes in human-readable format (KB, MB, GB).
- Handles unauthorized access exceptions gracefully.

## Usage
Install tool using dotnet CLI:

```bash
dotnet tool install --global Diskor
```

Run the tool from the command line:

```
diskor [start_path] -q [quota_MB] -d [max_depth]
```

### Parameters

- `start_path` *(optional)*: Specifies the directory to scan. If omitted, all available drives are scanned.
- `quota_MB` *(optional)*: The size threshold in megabytes (default is 512MB).
- `max_depth` *(optional)*: The maximum depth of subdirectories displayed (default is 5).

### Examples
Scan all drives with default settings:

```
diskor
```

Scan a specific directory with a custom quota of 1GB:

```
diskor C:\Users -q 1024
```

Scan with a custom depth limit:

```
diskor D:\Projects -q 200 -d 3
```