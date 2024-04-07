using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : SingletonMonoBehavior<SceneManager>
{
    [SerializeField] private string targetScene;
    [SerializeField] private LevelTransitionHandler transitionHandler;
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        if (transitionHandler == null) transitionHandler = GetComponent<LevelTransitionHandler>();
    }
    
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadNextScene()
    {
        transitionHandler.EndLevel();
        Invoke(nameof(LoadSceneAfter), transitionHandler.LoadTime);
    }

    private void LoadSceneAfter()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
    }

    private void OnSceneLoaded(Scene _, LoadSceneMode __)
    {
        transitionHandler.StartLevel();
    }
}
