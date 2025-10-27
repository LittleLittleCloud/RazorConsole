### Hot Reload Support

Build, tweak, and iterate without leaving your running console app.

RazorConsole supports hot reload via its metadata update handler so UI changes are reflected instantly—no restart required.

#### How it works

1. Run your application with `dotnet watch`.
2. Update a Razor component.
3. Save the file.
4. Watch the running console UI refresh automatically.

```shell
dotnet watch run
```

> **Tip:** Hot reload shines for component tweaks. Certain structural changes may still need a full restart.
