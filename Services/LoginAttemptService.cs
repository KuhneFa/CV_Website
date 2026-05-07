using Microsoft.Extensions.Caching.Memory;

namespace CVWebsite.Services;

public interface ILoginAttemptService
{
    bool IsLocked(string key, out TimeSpan remaining);
    void RegisterFailedAttempt(string key);
    void Reset(string key);
}

public class LoginAttemptService : ILoginAttemptService
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(10);

    private readonly IMemoryCache _cache;

    public LoginAttemptService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool IsLocked(string key, out TimeSpan remaining)
    {
        var state = GetState(key);
        if (state.LockedUntil is not { } lockedUntil || lockedUntil <= DateTimeOffset.UtcNow)
        {
            remaining = TimeSpan.Zero;
            return false;
        }

        remaining = lockedUntil - DateTimeOffset.UtcNow;
        return true;
    }

    public void RegisterFailedAttempt(string key)
    {
        var state = GetState(key);
        state.FailedAttempts++;

        if (state.FailedAttempts >= MaxFailedAttempts)
        {
            state.LockedUntil = DateTimeOffset.UtcNow.Add(LockoutDuration);
        }

        _cache.Set(key, state, AttemptWindow.Add(LockoutDuration));
    }

    public void Reset(string key)
    {
        _cache.Remove(key);
    }

    private LoginAttemptState GetState(string key)
    {
        return _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = AttemptWindow.Add(LockoutDuration);
            return new LoginAttemptState();
        })!;
    }

    private sealed class LoginAttemptState
    {
        public int FailedAttempts { get; set; }
        public DateTimeOffset? LockedUntil { get; set; }
    }
}
