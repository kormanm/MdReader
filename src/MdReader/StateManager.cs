using System.IO;
using Newtonsoft.Json;

namespace MdReader;

public class AppState
{
    public List<string> OpenFiles { get; set; } = new();
    public int ActiveTabIndex { get; set; } = -1;
}

public class StateManager
{
    private readonly string _stateFilePath;

    public StateManager()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MdReader"
        );

        Directory.CreateDirectory(appDataPath);
        _stateFilePath = Path.Combine(appDataPath, "state.json");
    }

    public void SaveState(AppState state)
    {
        try
        {
            var json = JsonConvert.SerializeObject(state, Formatting.Indented);
            File.WriteAllText(_stateFilePath, json);
        }
        catch
        {
            // Ignore errors when saving state
        }
    }

    public AppState? LoadState()
    {
        try
        {
            if (File.Exists(_stateFilePath))
            {
                var json = File.ReadAllText(_stateFilePath);
                var state = JsonConvert.DeserializeObject<AppState>(json);
                
                // Validate deserialized state
                if (state != null)
                {
                    state.OpenFiles ??= new List<string>();
                    if (state.ActiveTabIndex < -1)
                    {
                        state.ActiveTabIndex = -1;
                    }
                }
                
                return state;
            }
        }
        catch
        {
            // Ignore errors when loading state
        }

        return null;
    }
}
