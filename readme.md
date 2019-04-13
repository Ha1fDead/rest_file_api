# Demo Project

This is a simple demo project for MapLarge. The goals were to create a simple API + SPA that can browse files / folders from a configurable base directory running on the server.

Users should...

- Be able to view metadata of all files / directories from the configurable base directory
- Be able to search for files / directories from the configurable base directory
- Be able to upload files into the arbitrary directories
- Be able to download files from the arbitrary directories

Bonuses...

- Move files + directories between directories
- Delete files + directories from directories

The client should...

- Be deep-linkable (state stored in URL)
- Be a SPA

## Testing

Run `dotnet test` from the solution directory or the tests project.

Or install [NET Core Test Explorer](https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet-test-explorer)

## Architecture

To automate client-side rendering & templating, I used [lit-html](https://lit-html.polymer-project.org). I like web components.

## Security

Security is pretty non-existent. I block [path-traversal](https://www.owasp.org/index.php/Path_Traversal) attacks, but otherwise everything listed in [OWASP](https://www.owasp.org/index.php/Unrestricted_File_Upload) is valid.

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