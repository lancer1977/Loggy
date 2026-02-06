using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace PolyhydraGames.Loggy;

public static class Loggy
{
    private static readonly Action<ILogger, string, Guid, Exception?> StartNoArgs =
        LoggerMessage.Define<string, Guid>(
            LogLevel.Information,
            new EventId(1000, nameof(StartNoArgs)),
            "▶ {Name} [{LoggyId}]"
        );
    private static readonly Action<ILogger, string, Guid, object?, Exception?> StartWithArgs =
        LoggerMessage.Define<string, Guid, object?>(
            LogLevel.Information,
            new EventId(1001, nameof(StartWithArgs)),
            "▶ {Name} [{LoggyId}] {@Args}"
        );
    private static readonly Action<ILogger, string, Guid, Exception?> EndNoArgs =
        LoggerMessage.Define<string, Guid>(
            LogLevel.Information,
            new EventId(1002, nameof(EndNoArgs)),
            "■ {Name} [{LoggyId}]"
        );
    private static readonly Action<ILogger, string, Guid, object?, Exception?> EndWithArgs =
        LoggerMessage.Define<string, Guid, object?>(
            LogLevel.Information,
            new EventId(1003, nameof(EndWithArgs)),
            "■ {Name} [{LoggyId}] {@Args}"
        );
    private static readonly Action<ILogger, string, Guid, string, Exception?> InfoMessage =
        LoggerMessage.Define<string, Guid, string>(
            LogLevel.Information,
            new EventId(1004, nameof(InfoMessage)),
            "■ {Name} [{LoggyId}] {message}"
        );
    private static readonly Action<ILogger, string, Guid, string, Exception?> WarningMessage =
        LoggerMessage.Define<string, Guid, string>(
            LogLevel.Warning,
            new EventId(1005, nameof(WarningMessage)),
            "■ {Name} [{LoggyId}] {message}"
        );
    private static readonly Action<ILogger, string, Guid, string, Exception?> CriticalMessage =
        LoggerMessage.Define<string, Guid, string>(
            LogLevel.Critical,
            new EventId(1006, nameof(CriticalMessage)),
            "■ {Name} [{LoggyId}] {message}"
        );

    private static bool IsEnabled(ILogger? log, LogLevel level) => log?.IsEnabled(level) == true;

    private static LoggyId Start(ILogger? log, string name, object? args = null)
    {
        if (!IsEnabled(log, LogLevel.Information))
        {
            return default;
        }

        var id = NewId();
        if (args is null)
        {
            StartNoArgs(log!, name, id.Value, null);
        }
        else
        {
            StartWithArgs(log!, name, id.Value, args, null);
        }

        return id;
    }

    private static void End(ILogger? log, LoggyId id, string name, object? args = null)
    {
        if (!IsEnabled(log, LogLevel.Information))
        {
            return;
        }

        if (args is null)
        {
            EndNoArgs(log!, name, id.Value, null);
        }
        else
        {
            EndWithArgs(log!, name, id.Value, args, null);
        }
    }
    private static void Information(ILogger? log, LoggyId id, string name, string message)
    {
        if (!IsEnabled(log, LogLevel.Information))
        {
            return;
        }

        InfoMessage(log!, name, id.Value, message, null);
    }

    private static void Warning(
        ILogger? log,
        LoggyId id,
        string name,
        Exception? ex,
        string message
    )
    {
        if (!IsEnabled(log, LogLevel.Warning))
        {
            return;
        }

        WarningMessage(log!, name, id.Value, message, ex);
    }

    private static void Critical(
        ILogger? log,
        LoggyId id,
        string name,
        Exception ex,
        string message
    )
    {
        if (!IsEnabled(log, LogLevel.Critical))
        {
            return;
        }

        CriticalMessage(log!, name, id.Value, message, ex);
    }

    public static LoggyScope LoggyStart(
        this ILogger log,
        object? args = null,
        [CallerMemberName] string caller = ""
    ) => new(log, caller, args);

    public static LoggyScope LoggyStart(this ILogger? log, string name, object? args = null)
    {
        return new LoggyScope(log, name, args);
    }

    private static LoggyId NewId() => new(Guid.NewGuid());

    public sealed class LoggyScope : IDisposable
    {
        private readonly ILogger? _log;
        private readonly string _name;
        private bool _ended;
        public LoggyId Id { get; }

        public LoggyScope(ILogger? log, string name, object? args = null)
        {
            if (log is null)
            {
                return;
            }

            _log = log;
            _name = name;
            Id = Start(log!, name, args);
        }

        public void End(object? args = null)
        {
            if (_ended)
            {
                return;
            }

            _ended = true;
            Loggy.End(_log, Id, _name, args);
        }

        public void Dispose() => End();

        public void LogInformation(string message, params object?[] args)
        {
            if (_log is null)
            {
                return;
            }

            if (!IsEnabled(_log, LogLevel.Information))
            {
                return;
            }

            var arg = args.Length == 0 ? message : string.Format(message, args);
            Information(_log, Id, _name, arg);
        }

        public void LogWarning(string onChannelChatMessageLogFailed)
        {
            if (_log is null)
            {
                return;
            }

            Warning(_log, Id, _name, null, onChannelChatMessageLogFailed);
        }

        public void LogWarning(Exception exception, string onChannelChatMessageLogFailed)
        {
            if (_log is null)
            {
                return;
            }

            Warning(_log, Id, _name, exception, onChannelChatMessageLogFailed);
        }

        public void LogCritical(Exception exception, string onChannelChatMessageLogFailed)
        {
            if (_log is null)
            {
                return;
            }

            if (!IsEnabled(_log, LogLevel.Critical))
            {
                return;
            }

            Critical(_log, Id, _name, exception, onChannelChatMessageLogFailed);
        }
    }
}

public readonly struct LoggyId
{
    public Guid Value { get; }

    public LoggyId(Guid value) => Value = value;
}
