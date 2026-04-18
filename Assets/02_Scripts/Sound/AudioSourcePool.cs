using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// AudioSource 풀: 위치 기반(3D) 또는 2D SFX 재생에 사용
/// PlayOneShot 대신 개별 AudioSource로 재생하여 개별 제어 가능
/// </summary>
public class AudioSourcePool : MonoBehaviour
{
    [SerializeField] private int initialSize = 12;
    [SerializeField] private AudioMixerGroup outputMixerGroup = null;

    private Queue<AudioSource> _pool = new Queue<AudioSource>();
    private HashSet<AudioSource> _inUse = new HashSet<AudioSource>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
            _pool.Enqueue(CreateNewSource());
    }

    private AudioSource CreateNewSource()
    {
        var go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        if (outputMixerGroup != null) src.outputAudioMixerGroup = outputMixerGroup;
        return src;
    }

    public AudioSource Get()
    {
        AudioSource src;
        if (_pool.Count == 0) src = CreateNewSource();
        else src = _pool.Dequeue();

        _inUse.Add(src);
        src.transform.SetParent(null);
        return src;
    }

    public void Return(AudioSource src)
    {
        if (src == null) return;
        if (!_inUse.Contains(src)) return;

        _inUse.Remove(src);

        src.clip = null;
        src.loop = false;
        src.spatialBlend = 0f;
        src.volume = 1f;
        src.pitch = 1f;
        src.transform.SetParent(transform);
        src.Stop();

        _pool.Enqueue(src);
    }

    // 수정: isLoop 파라미터 추가, 루프일 때는 자동 반환 예약하지 않음
    public AudioSource PlayOneShotAt(AudioClip clip, Vector3 position, bool isLoop = false, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
    {
        if (clip == null) return null;
        var src = Get();
        src.transform.position = position;
        src.spatialBlend = Mathf.Clamp01(spatialBlend);
        src.volume = Mathf.Clamp01(volume);
        src.pitch = Mathf.Clamp(pitch, -3f, 3f);
        src.clip = clip;
        src.loop = isLoop;
        src.Play();

        // 만약 루프가 아닌 경우에만 자동 반환 예약
        if (!isLoop)
            StartCoroutine(ReturnWhenFinished(src, clip.length / Mathf.Abs(src.pitch)));

        return src;
    }

    public AudioSource PlayForSecondsAt(AudioClip clip, Vector3 position, float seconds, bool isLoop = false, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
    {
        if (clip == null) return null;
        var src = Get();
        src.transform.position = position;
        src.spatialBlend = Mathf.Clamp01(spatialBlend);
        src.volume = Mathf.Clamp01(volume);
        src.pitch = Mathf.Clamp(pitch, -3f, 3f);
        src.clip = clip;
        src.loop = isLoop;
        src.Play();
        StartCoroutine(StopAfterSeconds(src, seconds));
        return src;
    }

    public void StopAndReturn(AudioSource src)
    {
        if (src == null) return;
        if (!_inUse.Contains(src)) return;
        src.Stop();
        Return(src);
    }

    public void FadeOutAndReturn(AudioSource src, float fadeDuration)
    {
        if (src == null) return;
        if (!_inUse.Contains(src)) return;
        StartCoroutine(FadeOutCoroutine(src, fadeDuration));
    }

    private IEnumerator ReturnWhenFinished(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, delay) + 0.05f);
        if (_inUse.Contains(src))
            Return(src);
    }

    private IEnumerator StopAfterSeconds(AudioSource src, float seconds)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, seconds));
        if (_inUse.Contains(src))
        {
            src.Stop();
            Return(src);
        }
    }

    private IEnumerator FadeOutCoroutine(AudioSource src, float duration)
    {
        if (duration <= 0f)
        {
            StopAndReturn(src);
            yield break;
        }

        float startVolume = src.volume;
        float t = 0f;
        while (t < duration)
        {
            if (!_inUse.Contains(src))
                yield break;

            t += Time.deltaTime;
            src.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        if (_inUse.Contains(src))
        {
            src.Stop();
            Return(src);
        }
    }
}