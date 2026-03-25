namespace EPR.RegistrationValidation.IntegrationTests.Fakes;

using Microsoft.FeatureManagement;

public class InMemoryFeatureManager : IFeatureManager
{
    private readonly HashSet<string> _enabledFlags;

    public InMemoryFeatureManager(IEnumerable<string> enabledFlags)
    {
        _enabledFlags = new HashSet<string>(enabledFlags ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
    }

    public IAsyncEnumerable<string> GetFeatureNamesAsync()
    {
        return GetFeatureNames();
    }

    public Task<bool> IsEnabledAsync(string feature)
    {
        return Task.FromResult(_enabledFlags.Contains(feature));
    }

    public Task<bool> IsEnabledAsync<TContext>(string feature, TContext context)
    {
        return Task.FromResult(_enabledFlags.Contains(feature));
    }

    private async IAsyncEnumerable<string> GetFeatureNames()
    {
        foreach (var flag in _enabledFlags)
        {
            yield return flag;
            await Task.CompletedTask;
        }
    }
}
