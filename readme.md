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

## Architecture

To automate client-side rendering & templating, I used [lit-html](https://lit-html.polymer-project.org). I like web components.