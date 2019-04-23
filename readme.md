# Demo Project

This is a simple demo project for MapLarge. The goals were to create a simple API + SPA that can browse files / folders from a configurable base directory running on the server.

## Features

- Users can view all directories and files from a configurable Root URL
- Developers can change the root URL via `Program.cs` configuration (moving this to an INI/JSON config format is very simple)
- Users can copy where they are in the current hierarchy and send it to other people
- Users can copy files and directories (recursively)
- Users can move files and directories (recursively)
- Users can delete files and directories
- Users can upload new files into the current directory
  - You can also drag and drop a file into the fieldset
- Users can download files

## Building

Requirements:

1. `dotnetcore` SDK installed ([download](https://dotnet.microsoft.com/download))
2. clone the project (`git clone https://github.com/Ha1fDead/rest_file_api.git`)
3. run `dotnet run` from the `rest_file_api` directory (NOT the root solution)

For Visual Studio Code:

1. `dotnetcore` SDK installed ([download](https://dotnet.microsoft.com/download))
2. Install the [C# Extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
3. clone the project (`git clone https://github.com/Ha1fDead/rest_file_api.git`)
4. Open the solution in VSCode
5. Hit F5, your default browser should open

The app should be running on `http://localhost:5000` and `https://localhost:5001`. It should be rendering your root directory.
NOTE: File operations done on your directory are permanent!

To configure the root directory, simply change the configuration in `Program.cs`.

## Testing

Run `dotnet test` from the solution directory or the tests project.

Or install [NET Core Test Explorer](https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet-test-explorer)

## Architecture

To automate client-side rendering & templating, I used [lit-html](https://lit-html.polymer-project.org). I like web components, and `lit-html` seemed very similar to Knockout, so I felt it would be acceptable to use.

I try to use a more functional approach to client-side rendering (view changes when data changes).

## Security

Security is pretty non-existent. XSS attacks are not a concern & I block [path-traversal](https://www.owasp.org/index.php/Path_Traversal) attacks, but otherwise everything listed in [OWASP](https://www.owasp.org/index.php/Unrestricted_File_Upload) is valid.

To secure this, I would...

1. Restrict file uploads to trusted users
2. Validate the entire file names (restrict anything with special characters (`"`, `/`, `\`, `.`, `'`, etc.))
3. Add a reasonable upload size limit appropriate for the expected file sizes
4. Apply a business-case maximum upload limit for users / orgs
5. Restrict file uploads to specific types & validate those types (i.e. exposing user avatar uploads, I would validate the files can be loaded as images)
6. Not expose the underlying file system through the API. Generally I would prefer putting the files in a database and "virtualize" the directories in the application

## Scalability

My biggest concern for this service is scalability -- how well would it scale with 1/10/100/1000 requests per second. File IO is notoriously the bottleneck.

I would probably avoid a homebrew file system uploader in favour of off-the-shelf OSS & proprietary software. But, for the sake of the exercise, here's what I would consider:

1. Move file upload operations into a background thread that is queued
2. Move file uploads from memory-buffered to streamed w/ a rate-limiter attached, explained on [asp.net core](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-2.2) documentation
3. Move storage system to a distributed file system

My biggest concerns would be in multi-user scenarios. Alice deletes record A at the same time Bob moves record A to directory B.

## Improvements

1. Add webpack or rollupjs to build chain
2. Introduce typescript & convert frontend
3. Switch to React, Angular, or VueJS
4. Vastly improve scalability
5. Vastly improve security