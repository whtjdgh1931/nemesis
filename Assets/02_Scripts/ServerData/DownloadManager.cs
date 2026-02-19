using Firebase.Auth;
using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class DownloadManager : MonoBehaviour
{
    private ServerManager _serverManager;

    public void Initialize(ServerManager serverManager)
    {
        _serverManager = serverManager;
    }

    public async Task<bool> TryUploadPlayerStat()
    {
        if (_serverManager.currentUser == null)
        {
            _serverManager.ShowPopup("로그인된 사용자가 없어 업로드할 수 없습니다.");
            return false;
        }

        if (!File.Exists(Constants.FILE_PATH_PLAYERSTAT))
        {
            _serverManager.ShowPopup("업로드할 JSON 파일이 존재하지 않습니다.");
            return false;
        }

        string jsonText = File.ReadAllText(Constants.FILE_PATH_PLAYERSTAT);

        if(jsonText =="[]")
        {
            _serverManager.ShowPopup("업로드할 데이터가 없습니다.");
            return false; 
        }

        var data = new Dictionary<string, object>
    {
        { "playerStatJson", jsonText },
        { "updateTime", ServerValue.Timestamp }
    };

        try
        {
            await _serverManager.dbRef.Child("playerStats").Child(_serverManager.currentUser.UserId).UpdateChildrenAsync(data);
            return true;
        }
        catch (System.Exception ex)
        {
            _serverManager.ShowError("업로드 중 오류가 발생했습니다.", "Upload 오류: " + ex.Message);
            return false;
        }
    }

    public async void UploadPlayerStatFromLocal()
    {
        _serverManager.SetLoading(true);

        try
        {
            if (_serverManager.currentUser == null)
            {
                _serverManager.ShowPopup("로그인된 사용자가 없어 업로드할 수 없습니다.");
                return;
            }

            if (!File.Exists(Constants.FILE_PATH_PLAYERSTAT))
            {
                _serverManager.ShowPopup("업로드할 JSON 파일이 존재하지 않습니다.");
                await DownloadJsonToLocal(true);

                bool success = await TryUploadPlayerStat();

                if (success)
                {
                    TryDeleteFile(Constants.FILE_PATH_PLAYERSTAT);
                }

                return;
            }

            bool result = await TryUploadPlayerStat();

            if (result)
            {
                TryDeleteFile(Constants.FILE_PATH_PLAYERSTAT);
            }
        }
        catch (Exception ex)
        {
            _serverManager.ShowPopup($"업로드 중 오류 발생: {ex.Message}");
        }
        finally
        {
            _serverManager.SetLoading(false);
        }
    }

    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"파일 삭제 실패: {ex.Message}");
        }
    }



    public async void DownloadPlayerStatToLocal()
    {
        await DownloadJsonToLocal(fromGameBase: false);
    }

    public async Task DownloadJsonToLocal(bool fromGameBase)
    {
        _serverManager.SetLoading(true);

        string jsonText = null;

        try
        {
            if (fromGameBase)
            {
                var snapshot = await _serverManager.dbRef.Child("gameBaseJson").GetValueAsync();
                if (snapshot != null && snapshot.Exists)
                {
                    jsonText = snapshot.GetRawJsonValue();
                }
                else
                {
                    _serverManager.ShowPopup("초기 게임 데이터가 Firebase에 없습니다.");
                    _serverManager.SetLoading(false);
                    return;
                }
            }
            else // fromGameBase가 false (사용자 데이터 다운로드 시도)
            {
                if (_serverManager.currentUser == null)
                {
                    _serverManager.ShowPopup("로그인된 사용자가 없습니다.");
                    _serverManager.SetLoading(false);
                    return;
                }

                var snapshot = await _serverManager.dbRef.Child("playerStats").Child(_serverManager.currentUser.UserId).GetValueAsync();
                if (snapshot != null && snapshot.Exists && snapshot.HasChild("playerStatJson"))
                {
                    jsonText = snapshot.Child("playerStatJson").Value.ToString();

                    var baseSnapshot = await _serverManager.dbRef.Child("gameBaseJson").GetValueAsync();
                    if (baseSnapshot != null && baseSnapshot.Exists)
                    {
                        string baseJson = baseSnapshot.GetRawJsonValue();

                        var localStats = JsonConvert.DeserializeObject<List<PlayerStatJsonData>>(jsonText);
                        var baseStats = JsonConvert.DeserializeObject<List<PlayerStatJsonData>>(baseJson);

                        var syncedStats = SyncWithGameBase(localStats, baseStats);
                        jsonText = JsonConvert.SerializeObject(syncedStats, Formatting.Indented);
                    }
                }
                else // 사용자 데이터가 Firebase에 없는 경우 (playerStatJson이 존재하지 않거나, snapshot 자체가 없는 경우)
                {
                    _serverManager.ShowPopup("사용자 데이터가 Firebase에 없습니다. 초기 게임 데이터로 초기화합니다.");
                    await DownloadJsonToLocal(fromGameBase: true); // gameBaseJson을 로컬에 다운로드
                                                                   // 이 호출이 성공하면 Constants.FILE_PATH_PLAYERSTAT에 gameBaseJson 내용이 저장됩니다.


                    if (_serverManager.currentUser != null) // 혹시 모를 경우를 대비한 null 체크
                    {
                        try
                        {
                            // 로컬에 방금 저장된 gameBaseJson 내용을 읽어 사용자의 playerStats로 Firebase에 업로드
                            bool uploadSuccess = await TryUploadPlayerStat(); // TryUploadPlayerStat이 로컬 파일을 읽어 업로드합니다.
                            if (uploadSuccess)
                            {
                                // 업로드 성공 후 jsonText 변수도 이 초기화된 값으로 업데이트하여 이후 로컬 파일 쓰기가 올바르게 되도록 함
                                jsonText = File.ReadAllText(Constants.FILE_PATH_PLAYERSTAT);
                            }
                            else
                            {
                                Debug.LogError("초기 게임 데이터를 사용자 데이터로 Firebase에 업로드하는 데 실패했습니다.");
                                _serverManager.ShowError("사용자 데이터 초기화 실패", "초기 게임 데이터 업로드 실패");
                                _serverManager.SetLoading(false);
                                return; // 업로드 실패 시 함수 종료
                            }
                        }
                        catch (System.Exception ex)
                        {
                            _serverManager.ShowError("사용자 데이터 초기화 중 오류가 발생했습니다.", "Initial playerStat upload error: " + ex.Message);
                            _serverManager.SetLoading(false);
                            return; // 오류 발생 시 함수 종료
                        }
                    }
                    else
                    { // currentUser가 null인데 여기까지 왔다면 뭔가 이상한 경우
                        _serverManager.ShowPopup("로그인된 사용자 정보가 없어 데이터를 초기화할 수 없습니다.");
                        _serverManager.SetLoading(false);
                        return;
                    }


                }
            }

            // 다운로드된 jsonText를 로컬 파일에 저장
            if (!string.IsNullOrEmpty(jsonText) && jsonText != "null")
            {
                string folderPath = Path.GetDirectoryName(Constants.FILE_PATH_PLAYERSTAT);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                File.WriteAllText(Constants.FILE_PATH_PLAYERSTAT, jsonText);
            }
            else
            {
                _serverManager.ShowPopup("서버에서 받은 데이터가 비어 있습니다.");
            }
        }
        catch (System.Exception ex)
        {
            _serverManager.ShowError("데이터 다운로드 중 오류가 발생했습니다.", "DownloadJson 오류: " + ex.Message);
        }
        finally
        {
            _serverManager.SetLoading(false);
        }
    }
    private List<PlayerStatJsonData> SyncWithGameBase(List<PlayerStatJsonData> localStats, List<PlayerStatJsonData> baseStats)
    {
        var updatedStats = new List<PlayerStatJsonData>();

        foreach (var local in localStats)
        {
            var baseStat = baseStats.FirstOrDefault(b => b.column == local.column);

            if (baseStat != null)
            {
                bool needsUpdate =
                    local.type != baseStat.type ||
                    local.description != baseStat.description ||
                    local.defaultValue != baseStat.defaultValue ||
                    local.upgradeValue != baseStat.upgradeValue ||
                    local.maxLevel != baseStat.maxLevel;

                if (needsUpdate)
                {
                    baseStat.currentLevel = local.currentLevel;
                    updatedStats.Add(baseStat);
                }
                else
                {
                    updatedStats.Add(local);
                }
            }
            else
            {
                updatedStats.Add(local);
            }
        }

        return updatedStats;
    }


    public async void UploadClearTime(float time)
    {
        await UploadClearTimeServer(time);

    }


    public async Task UploadClearTimeServer(float clearTime)
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        string uid = user.UserId;

        var data = new Dictionary<string, object>
    {
        { "time", clearTime },
        { "savedAt", ServerValue.Timestamp } // 서버 시간 기준 저장
    };

        await FirebaseDatabase.DefaultInstance
            .RootReference
            .Child("users")
            .Child(uid)
            .Child("clearTimes")
            .Push()
            .SetValueAsync(data);

        await SetChrome();
    }

    public async Task<List<(string email, float bestTime, long savedAt)>> GetClearTimeRanking()
    {
        var snapshot = await FirebaseDatabase.DefaultInstance
            .RootReference
            .Child("users")
            .GetValueAsync();

        var rankingList = new List<(string email, float bestTime, long savedAt)>();

        foreach (var userNode in snapshot.Children)
        {
            string email = userNode.Child("email").Value?.ToString() ?? "Unknown";
            var clearTimesNode = userNode.Child("clearTimes");

            float bestTime = float.MaxValue;
            long bestSavedAt = 0;

            foreach (var timeEntry in clearTimesNode.Children)
            {
                var timeValue = timeEntry.Child("time").Value?.ToString();
                var savedAtValue = timeEntry.Child("savedAt").Value;

                if (float.TryParse(timeValue, out float t) && savedAtValue != null && long.TryParse(savedAtValue.ToString(), out long timestamp))
                {
                    if (t < bestTime)
                    {
                        bestTime = t;
                        bestSavedAt = timestamp;
                    }
                }
            }

            if (bestTime != float.MaxValue)
            {
                rankingList.Add((email, bestTime, bestSavedAt));
            }
        }

        return rankingList.OrderBy(r => r.bestTime).ToList();
    }

    public async Task<int> GetChrome()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return 0;

        string uid = user.UserId;

        var snapshot = await FirebaseDatabase.DefaultInstance
            .RootReference
            .Child("users")
            .Child(uid)
            .Child("chrome")
            .GetValueAsync();

        if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int chrome))
        {
            return chrome;
        }

        return 0;
    }

    public async void SetChromeToServer()
    {
        if (GameManager.Instance.CurrencyManager.CurrentChrome <= 0) return;
        await SetChrome();
    }

    public async Task SetChrome()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        string uid = user.UserId;

        await FirebaseDatabase.DefaultInstance
            .RootReference
            .Child("users")
            .Child(uid)
            .Child("chrome")
            .SetValueAsync(GameManager.Instance.CurrencyManager.CurrentChrome);
    }
}