# Serilog Get Started

## [Medium Article: Logging with Serilog](https://blog.devgenius.io/logging-with-serilog-f6903b0c176)

How to configure Serilog and split logs into different files?

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/I3I63W4OK)

![serilog](./serilog.png)

## Serilog Configuration Basics

[Serilog](https://serilog.net/) is a famous logging tool for .NET and ASP.NET applications. We can easily create a globally-shared logger using the following line of code.

```csharp
Log.Logger = new LoggerConfiguration().CreateLogger();
```

The logger created in this way will have the same lifetime as that of our application. You can also use a `using` statement to create a short-lived logger, but the use case is rare.

Serilog can use a simple C# API to configure the logger directly in code, and can also load external configurations from settings files. For a minimum configuration, we need to attach a logging sink to the global static logger so that messages can be written to some place. For example, we can add a Console sink to record log events as follows.

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
```

After a global logger is created, we need to tell .NET or ASP.NET about the logger so that .NET or ASP.NET can pipe the messages to Serilog. Otherwise, without assigning a logging provider to a Host, messages logged by `ILogger<T>` don't have an outlet. To register Serilog as a logging provider, we can call the `UseSerilog()` method on the `IHostBuilder` as follows.

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .Configure******
        .UseSerilog();
```

Thereafter, we can log our message to desired sinks using the logging system in .NET/ASP.NET. We can also use the global logger directly at any place. The following line shows an example usage of the global logger.

```csharp
Log.Information("Application Starts");
```

Serilog provides a variety of sinks ([link](https://github.com/serilog/serilog/wiki/Provided-Sinks)). We can add them as needed. For example, to save logs to files, we can attach a file sink to the logger as follows.

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("log.txt", LogEventLevel.Information, rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

## Split / Suppress Logs

Sometimes we may want to have more granular controls over the categories of log messages for each sink. For example, we want to log messages with different LogLevels to different files so that errors and warnings are stood out from low-criticality messages. Another example, we want to log messages for background jobs to a different file from the file that logs normal routine. Or we want to suppress a part of messages to reduce noise level. For these use cases, Serilog allows us to set up logging pipelines using filters to include or exclude certain log events thus splitting logs into different sinks. For example, the following configuration creates a Console logger which will output all messages, and a sub-logger which only writes certain events to the `log.txt` file based on the criteria defined in the line `Filter.ByIncludingOnly(...)`.

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(...)
        .WriteTo.File("log.txt"))
    .CreateLogger();
```

Most examples about using `Filter` focus on introducing the filter expression which uses a powerful SQL like syntax. The [filter expression docs](https://github.com/serilog/serilog-expressions) give the full details. I like the filter expression because it gives great flexibility in configurations in XML or JSON files.

Here I want to introduce another convenient way of filtering log events: by including/excluding properties in the `LogContext`.

For example, a method has two dependencies `_myService1` and `_myService2`.

```csharp
public async Task MyMethod()
{
    _logger.LogInformation("foo bar start");
    await _myService1.Foo();
    await _myService2.Bar();
    _logger.LogInformation("foo bar end");
}
```

Now we want to write all execution messages under this method to a separate file called `foobar.txt`. We can use the filter express to figure out the LogContext by class names. However, that would be not scalable if we introduce another dependency to the method. An easier way is to filter by property names and/or values.

For demo purposes, we can set a property to the LogEvents in this method execution path. The following code snippet shows an example.

```csharp
public async Task MyMethod()
{
    using (LogContext.PushProperty("foobar", 1))
    {
        _logger.LogInformation("foo bar start");
        await _myService1.Foo();
        await _myService2.Bar();
        _logger.LogInformation("foo bar end");
    }
}
```

The `using` statement creates a scope and ensures the desired property is not leaked to other methods. Within the scope, all log events will contain the property. Thus we can filter these log events based on the key-value pairs. Note that you can create a class for all property key-value pairs to avoid magic strings/values.

The following code snippet shows a filtering example.

```csharp
const string logTemplate = @"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}";
Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Logger(l =>
        {
            l.WriteTo.File("log.txt", LogEventLevel.Information, logTemplate,
                rollingInterval: RollingInterval.Day
            );
            l.Filter.ByExcluding(e => e.Properties.ContainsKey("foobar"));
        })
        .WriteTo.Logger(l =>
        {
            l.WriteTo.File("foobar.txt", LogEventLevel.Information, logTemplate,
                rollingInterval: RollingInterval.Day
            );
            l.Filter.ByIncludingOnly(e => e.Properties.ContainsKey("foobar"));
        })
        .CreateLogger();
```

With the configuration above, normal logs (without the property `foobar` in log events) will be saved to the `log.txt` file, while logs with the property `foobar` will be saved to the `foobar.txt` file.

## License

Feel free to use the code in this repository as it is under MIT license.

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/I3I63W4OK)
