# FurCoNZ

## Development Setup:

### Initial setup:

* [Download and install the latest (current) .NET Core SDK](https://dotnet.microsoft.com/download/dotnet-core)
* Clone this repo to your local machine: `git clone https://github.com/NZFurs/FurCoNZ.git`

### Set secrets:
* Contact @tcfox or @nzsmartie for the latest copy of the development `secrets.json` file (they will want your PGP key or will send it to you via Keybase)
* Place this file at the following location (this is to ensure the secrets don't accidentally get committed to code): 

**Windows:**
```
%APPDATA%\Microsoft\UserSecrets\ae59f009-796a-43af-9da1-816735206ea9\secrets.json
```

**macOS/Linux:**
```
~/.microsoft/usersecrets/ae59f009-796a-43af-9da1-816735206ea9/secrets.json
```

### Initialise Database:

* In the terminal, navigate to `src/FurCoNZ.Web/`
* Run the following command:

```bash
dotnet ef database update
```

### Build the code:

* In the terminal, navigate to `src/FurCoNZ.Web/`
* Run the following command:

```bash
dotnet build
```

### Run a development instance
* In the terminal, navigate to `src/FurCoNZ.Web/`
* Run the following command:

```bash
dotnet run
```
