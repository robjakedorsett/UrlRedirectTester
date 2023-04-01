# Url Redirect Tester

## Info

This is a simple CLI tool built with .NET 7, which can used to test old to new redirect mappings, to prevent manually needing to test the redirect mappings.

This package is functional, but is not fully battle hardened.

## Setup

There's a few options with setting up.

### Option 1
- Clone Repo
- Run Build locally
- Navigate to build (within Bin folder)
- Add CSV files ensuring headers `oldUrl`, `newUrl`
    - Must be in a folder `CSVFiles` on the root of the build
- Run `UrlRedirectTester.exe {Base Url} {CSV File Name}`
    - Where Base Url is the full base url with protocol.  This is used for the to fire requests to the old urls
        - E.g. `https://example.com`
    - Where CSV File name is the file name of the CSV file you want to test the redirects.
        - E.g. `my-old-new-redirects.csv` (`.csv` is optional)

### Option 2
- Download the latest release package.
- Add CSV files ensuring headers `oldUrl`, `newUrl`
    - MUst be in a folder `CSVFiles` on the root of the build
- Click into EXE
- Enter Base Url
- Enter CSV File Name