# FurCoNZ

## Development Setup:

### Initial setup:

* [Download and install the latest (current) .NET Core SDK](https://dotnet.microsoft.com/download/dotnet-core)
* Clone this repo to your local machine: `git clone https://github.com/NZFurs/FurCoNZ.git`
* Mac: Ensure `/usr/local/share/dotnet` and `~/.dotnet/tools` are in your path


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

### Install libman

Libman installs client-side dependencies.

```
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
# restart your terminal
libman restore
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

### Set current user as Admin
Note: This will only work when:
* The application is in DEBUG mode (`dotnet build` or `dotnet build --configuration Debug`, not `dotnet build --configuration Release`)
* The application environment is set as a Development environment (the environment variable `ASPNETCORE_ENVIRONMENT` is `Development`)
* There are no other administrators in the current database

To set the current user as an administrator:
* Run the application (`dotnet run`)
* Navigate to `https://local.dev.furco.nz:62434/Debug/Tools/MakeAdmin` (you may be prompted to log in)
* You may need to delete your cookies after this operation (this is a known bug)

### Set some testing data in the database
Note: This will only work when:
* The application environment is set as a Development environment (the environment variable `ASPNETCORE_ENVIRONMENT` is `Development`)
* The current user must be an administrator
* No ticket types currently exist in the database

To set up some sample ticket types:
* Run the application (`dotnet run`)
* Navigate to `https://local.dev.furco.nz:62434/Debug/Tools/SeedData` (you may be prompted to log in)
