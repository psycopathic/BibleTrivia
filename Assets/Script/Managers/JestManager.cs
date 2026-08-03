using System;
using System.Threading.Tasks;
using com.jest.sdk;
using UnityEngine;

[DefaultExecutionOrder(-1100)]
public class JestManager : MonoBehaviour
{
    public static JestManager Instance { get; private set; }

    public bool IsReady { get; private set; }
    public bool InitializationFailed { get; private set; }

    private Task initializationTask;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        EnsureInstance();
    }

    public static JestManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        Instance = FindAnyObjectByType<JestManager>();

        if (Instance != null)
        {
            return Instance;
        }

        GameObject managerObject = new GameObject("JestManager");
        Instance = managerObject.AddComponent<JestManager>();
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        initializationTask ??= InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await JestSDK.Instance.Init();
            IsReady = true;
            JestSDK.Instance.MarkGameLoaded();
            Debug.Log("[JestManager] Jest SDK initialized.", this);
        }
        catch (Exception exception)
        {
            InitializationFailed = true;
            Debug.LogWarning($"[JestManager] Jest SDK initialization failed. Continuing without platform SDK. {exception}", this);
        }
    }
}
