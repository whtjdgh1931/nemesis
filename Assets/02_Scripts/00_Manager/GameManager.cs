using UnityEngine;

/// <summary>
/// 게임 전체에서 싱글톤으로 접근 가능한 매니저 클래스
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    /// <summary>
    /// 어디에서나 접근 가능한 싱글톤 인스턴스
    /// Instance로 접근시 GameManager가 씬에 없으면 자동으로 생성
    /// </summary>
    public static GameManager Instance
    {
        // get 프로퍼티
        get
        {
            // 만약 GameManager.Instance로 접근했는데 없다면
            if (_instance == null)
            {
                // 씬에서 GameManager를 찾아보고
                _instance = FindAnyObjectByType<GameManager>();
                // 그래도 없다면
                if (_instance == null)
                {
                    // 게임오브젝트를 GameManager라는 이름으로 새로 만들고
                    GameObject obj = new GameObject("GameManager");
                    // GameManager 컴포넌트를 추가 후 _instance에 할당
                    _instance = obj.AddComponent<GameManager>();
                    _instance._resourceManager = obj.AddComponent<ResourceManager>();
                    _instance._interactableManager = obj.AddComponent<InteractableManager>();
                    _instance._dataManager = obj.AddComponent<DataManager>();
                    _instance._playerStatManager = obj.AddComponent<PlayerStatManager>();
                    _instance._languageManager = obj.AddComponent<LanguageManager>();
                    _instance._poolManager = obj.AddComponent<PoolManager>();
                    _instance._currencyManager = obj.AddComponent<CurrencyManager>();
                    _instance._sceneLoadManager = obj.AddComponent<SceneLoadManager>();
                    _instance._soundManager = obj.AddComponent<SoundManager>();

                    _instance.Initialize();


                    // 씬 전환시 파괴되지 않도록 설정
                    DontDestroyOnLoad(obj);
                }
            }
            // 문제 없이 찾았거나 생성했으면 _instance 반환
            return _instance;
        }
    }

    public void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }




    }


    void Initialize()
    {
        if (_skillManager == null)
        {
            _skillManager = Resources.Load<SkillManager>("Prefabs/Skill/SkillManager");
            _skillManager = Instantiate(_skillManager, transform);
            _skillManager.name = "SkillManager";
        }



        if (_uiManager == null)
        {
            _uiManager = Resources.Load<UIManager>("Prefabs/Skill/UIManager");
            _uiManager = Instantiate(_uiManager, transform);
            _uiManager.name = "UIManager";
        }
        if (_serverManager == null)
        {
            _serverManager = Resources.Load<ServerManager>("Prefabs/Skill/ServerManager");
            _serverManager = Instantiate(_serverManager, transform);
            _serverManager.name = "ServerManager";
        }
        _instance._resourceManager.Initialize();
        _instance._dataManager.Initialize(_instance._resourceManager);
        _skillManager.InitializeSkillManager();
        StartCoroutine(_uiManager.InitializeManager());
        _serverManager.Initialize();
        _currencyManager.Initialize();
        _poolManager.Initialize(_instance._resourceManager);
        _soundManager.Initialize(_resourceManager);
    }

    [SerializeField] ResourceManager _resourceManager;      // 리소스 매니저

    public ResourceManager ResourceManager => _resourceManager;

    [SerializeField] InteractableManager _interactableManager; // 상호작용 매니저
    public InteractableManager InteractableManager => _interactableManager;

    [SerializeField] DataManager _dataManager;             // 데이터 매니저
    public DataManager DataManager => _dataManager;

    [SerializeField] PlayerStatManager _playerStatManager; // 플레이어 스탯 매니저
    public PlayerStatManager PlayerStatManager => _playerStatManager;

    [SerializeField] PoolManager _poolManager;                     // 풀 매니저
    public PoolManager PoolManager => _poolManager;

    [SerializeField] CurrencyManager _currencyManager;             // 화폐 매니저
    public CurrencyManager CurrencyManager => _currencyManager;

    /// <summary>
    /// 스킬 매니저
    /// </summary>
    [SerializeField]
    private SkillManager _skillManager;
    public SkillManager skillManager
    {
        get
        {

            return _skillManager;
        }
    }

    /// <summary>
    /// UIManager
    /// </summary>
    [SerializeField]
    private UIManager _uiManager;
    public UIManager UIManager { get { return _uiManager; } }

    [SerializeField]
    private ServerManager _serverManager;
    public ServerManager serverManager { get { return _serverManager; } }

    [SerializeField] SceneLoadManager _sceneLoadManager;
    public SceneLoadManager SceneLoadManager => _sceneLoadManager;
    [SerializeField]
    private LanguageManager _languageManager;
    public LanguageManager languageManager => _languageManager;

    [SerializeField] SoundManager _soundManager;
    public SoundManager SoundManager => _soundManager;
}
