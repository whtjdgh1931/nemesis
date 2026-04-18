using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임의 모든 사운드를 관리하는 매니저
/// - AudioSourcePool의 변경된 시그니처에 맞춰 업데이트됨.
/// - 각 Play 메서드는 풀을 사용하면 재생에 사용된 AudioSource를 반환합니다.
///   루프 재생 시에는 반환된 AudioSource를 보관해 두었다가 StopSfx / FadeOutSfx로 정지하세요.
/// </summary>
public class SoundManager : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] AudioSource _bgmSource;      // BGM 전용 AudioSource

    [Header("SFX")]
    [SerializeField] AudioSourcePool _sfxSourcePool; // SFX 재생용 AudioSource 풀
    [SerializeField] AudioSource _fallbackSfxSource; // 풀 없을 때 사용할 폴백 AudioSource (공유)

    [Header("Volume Settings")]
    [Range(0f, 1f)][SerializeField] float _masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] float _bgmVolume = 1f;
    [Range(0f, 1f)][SerializeField] float _sfxVolume = 1f;

    [Header("Audio Clips")]
    Dictionary<string, AudioClip> _bgmClips = new Dictionary<string, AudioClip>();
    Dictionary<string, AudioClip> _sfxClips = new Dictionary<string, AudioClip>();

    public float MasterVolume => _masterVolume;
    public float BGMVolume => _bgmVolume;
    public float SFXVolume => _sfxVolume;

    public void Initialize(ResourceManager resourceManager)
    {
        if (resourceManager == null)
        {
            return;
        }

        if (_bgmSource == null)
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
        }

        if (_fallbackSfxSource == null)
        {
            var go = new GameObject("FallbackSfxSource");
            go.transform.SetParent(transform);
            _fallbackSfxSource = go.AddComponent<AudioSource>();
            _fallbackSfxSource.playOnAwake = false;
            _fallbackSfxSource.spatialBlend = 0f; // 2D
        }

        if (_sfxSourcePool == null)
        {
            var go = new GameObject("SfxSourcePool");
            go.transform.SetParent(transform);
            _sfxSourcePool = go.AddComponent<AudioSourcePool>();
        }

        _bgmClips.Clear();
        _sfxClips.Clear();

        if (resourceManager.Bgm != null)
        {
            foreach (var clip in resourceManager.Bgm)
            {
                if (clip == null) continue;
                _bgmClips[clip.name] = clip;
            }
        }

        if (resourceManager.Sfx != null)
        {
            foreach (var clip in resourceManager.Sfx)
            {
                if (clip == null) continue;
                _sfxClips[clip.name] = clip;
            }
        }

        // 초기 소리 크기 적용
        _masterVolume = 1f;
        _bgmVolume = 0.1f;
        _sfxVolume = 1f;
        SetMasterVolume(_masterVolume);
        SetBGMVolume(_bgmVolume);
        SetSFXVolume(_sfxVolume);

        ApplyBgmVolume();
        ApplySfxVolume();
    }

    public void PlayBGM(string bgmName)
    {
        if (_bgmSource == null)
        {
            return;
        }

        if (_bgmClips.TryGetValue(bgmName, out var clip))
        {
            _bgmSource.clip = clip;
            _bgmSource.loop = true;
            ApplyBgmVolume();
            _bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (_bgmSource == null)
        {
            return;
        }
        _bgmSource.Stop();
    }

    /// <summary>
    /// UI용 2D SFX 재생 (위치 없음)
    /// 반환: 풀을 사용하면 사용된 AudioSource(제어 가능)를 반환합니다.
    /// </summary>
    public AudioSource PlaySfx(string sfxName, bool isLoop = false, float volume = 1f, float pitch = 1f)
    {
        if (!_sfxClips.TryGetValue(sfxName, out var clip))
        {
            return null;
        }

        float finalVolume = Mathf.Clamp01(volume) * _sfxVolume * _masterVolume;
        float finalPitch = Mathf.Clamp(pitch, -3f, 3f);

        if (_sfxSourcePool != null)
        {
            // spatialBlend 0 => 2D
            return _sfxSourcePool.PlayOneShotAt(clip, Vector3.zero, isLoop, finalVolume, finalPitch, 0f);
        }

        // 폴백: 공유 소스이므로 루프 재생은 다른 소리에 영향을 줄 수 있음
        if (_fallbackSfxSource != null)
        {
            _fallbackSfxSource.loop = isLoop;
            _fallbackSfxSource.pitch = finalPitch;
            _fallbackSfxSource.volume = finalVolume;
            if (isLoop)
            {
                _fallbackSfxSource.clip = clip;
                _fallbackSfxSource.Play();
                return _fallbackSfxSource;
            }
            else
            {
                _fallbackSfxSource.PlayOneShot(clip, finalVolume);
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// 위치 기반 SFX 재생
    /// 반환: 풀을 사용하면 사용된 AudioSource(제어 가능)를 반환합니다.
    /// </summary>
    public AudioSource PlaySfxAt(string sfxName, Vector3 pos, bool isLoop = false, float volume = 1f, float pitch = 1f)
    {
        if (!_sfxClips.TryGetValue(sfxName, out var clip))
        {
            return null;
        }

        float finalVolume = Mathf.Clamp01(volume) * _sfxVolume * _masterVolume;
        float finalPitch = Mathf.Clamp(pitch, -3f, 3f);

        if (_sfxSourcePool != null)
        {
            var src = _sfxSourcePool.PlayOneShotAt(clip, pos, isLoop, finalVolume, finalPitch, 1f);
            return src;
        }

        if (_fallbackSfxSource != null)
        {
            _fallbackSfxSource.pitch = finalPitch;
            _fallbackSfxSource.PlayOneShot(clip, finalVolume);
            return null;
        }

        return null;
    }

    /// <summary>
    /// 지정 시간(seconds)만 재생
    /// </summary>
    public AudioSource PlaySfxForSeconds(string sfxName, float seconds, bool isLoop = false, float volume = 1f, float pitch = 1f)
    {
        if (!_sfxClips.TryGetValue(sfxName, out var clip))
        {
            return null;
        }

        float finalVolume = Mathf.Clamp01(volume) * _sfxVolume * _masterVolume;
        float finalPitch = Mathf.Clamp(pitch, -3f, 3f);

        if (_sfxSourcePool != null)
        {
            return _sfxSourcePool.PlayForSecondsAt(clip, Vector3.zero, seconds, isLoop, finalVolume, finalPitch, 0f);
        }

        return null;
    }

    public AudioSource PlaySfxForSecondsAt(string sfxName, Vector3 pos, float seconds, bool isLoop = false, float volume = 1f, float pitch = 1f)
    {
        if (!_sfxClips.TryGetValue(sfxName, out var clip))
        {
            return null;
        }

        float finalVolume = Mathf.Clamp01(volume) * _sfxVolume * _masterVolume;
        float finalPitch = Mathf.Clamp(pitch, -3f, 3f);

        if (_sfxSourcePool != null)
        {
            return _sfxSourcePool.PlayForSecondsAt(clip, pos, seconds, isLoop, finalVolume, finalPitch, 1f);
        }

        return null;
    }

    /// <summary>
    /// 특정 AudioSource(핸들)를 즉시 정지하고 풀로 반환합니다.
    /// 반환된 AudioSource를 보관하고 있다가 호출하세요.
    /// </summary>
    public void StopSfx(AudioSource src)
    {
        if (src == null)
        {
            return;
        }

        if (_sfxSourcePool != null)
        {
            _sfxSourcePool.StopAndReturn(src);
            return;
        }

        // 폴백일 경우 주의: 공유 소스이므로 다른 소리도 중단될 수 있음
        if (src == _fallbackSfxSource)
        {
            src.Stop();
            src.loop = false;
            return;
        }

    }

    /// <summary>
    /// 지정한 AudioSource를 페이드아웃 한 뒤 반환합니다.
    /// </summary>
    public void FadeOutSfx(AudioSource src, float fadeDuration)
    {
        if (src == null)
        {
            return;
        }

        if (_sfxSourcePool != null)
        {
            _sfxSourcePool.FadeOutAndReturn(src, fadeDuration);
            return;
        }

        if (src == _fallbackSfxSource)
        {
            StartCoroutine(FadeOutFallback(src, fadeDuration));
            return;
        }
    }

    private IEnumerator FadeOutFallback(AudioSource src, float duration)
    {
        if (src == null) yield break;
        float startVolume = src.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(startVolume, 0f, t / Mathf.Max(0.0001f, duration));
            yield return null;
        }
        src.Stop();
        src.loop = false;
        src.volume = startVolume;
    }

    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        ApplyBgmVolume();
        ApplySfxVolume();
    }

    public void SetBGMVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);
        ApplyBgmVolume();
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        ApplySfxVolume();
    }

    private void ApplyBgmVolume()
    {
        if (_bgmSource != null)
            _bgmSource.volume = _masterVolume * _bgmVolume;
    }

    private void ApplySfxVolume()
    {
        // 풀을 통해 재생되는 소리는 Play 호출 시 전달된 최종 볼륨을 따르므로
        // 여기서는 폴백 소스의 기본 볼륨만 설정.
        if (_fallbackSfxSource != null)
            _fallbackSfxSource.volume = _masterVolume * _sfxVolume;
    }
}