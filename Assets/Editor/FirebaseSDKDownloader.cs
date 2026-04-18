#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Net;

[InitializeOnLoad]
public class FirebaseNativeAutoDownloader
{
    static FirebaseNativeAutoDownloader()
    {
        string basePath = Path.Combine(Application.dataPath, "Firebase/Plugins/x86_64");

        string[] files = {
            "FirebaseCppApp-13_4_0.so",
            "FirebaseCppApp-13_4_0.bundle"
        };

        string[] urls = {
            "https://www.dropbox.com/scl/fi/mv3bj61ld6n8bmsa538u3/FirebaseCppApp-13_4_0.so?rlkey=6fcfojtdpzjr16je6vsnz4b89&st=2u05j1aq&dl=1",
            "https://www.dropbox.com/scl/fi/mmjpekx5yjl3qpnylixuu/FirebaseCppApp-13_4_0.bundle?rlkey=bei9uretu3blf9su24u97j4j5&st=zmwjl0gq&dl=1"
        };

        for (int i = 0; i < files.Length; i++)
        {
            string targetPath = Path.Combine(basePath, files[i]);
            string downloadUrl = urls[i];

            if (!File.Exists(targetPath))
            {
                try
                {
                    Directory.CreateDirectory(basePath);
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(downloadUrl, targetPath);
                        Debug.Log($"✅ 다운로드 완료: {files[i]}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"❌ 다운로드 실패: {files[i]} → {ex.Message}");
                }
            }
            else
            {
                Debug.Log($"ℹ️ 이미 존재함: {files[i]}");
            }
        }
    }
}
#endif
