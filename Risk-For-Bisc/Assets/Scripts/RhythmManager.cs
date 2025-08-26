using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class RhythmManager : MonoBehaviour
{
    [Header("Playlist")]
    public List<Beatmap> beatmaps = new List<Beatmap>();
    public bool loopPlaylist = true;
    public int startIndex = 0;


    [Header("Audio Sources")]
    public AudioSource musicSource;
    private AudioSource altSource; // Cloned so we can crossfade music
    
    [Header("Timing")]
    public double startDelay = 0.1;
    public double lookAheadTime = 0.2;
    public float crossfadeDuration = 2f; // Crossfade
    public float startFadeDuration = 1f; // Fade in
    public float endFadeDuration = 1f; // Fade out

    // Any SFX should use AudioSource.PlayScheduled(scheduledDspTime)
    public event Action<BeatEvent, double> OnBeatEventScheduled;
    class TrackState
    {
        public AudioSource src;
        public Beatmap map;
        public List<BeatEvent> events;
        public int nextIndex;
        public double dspStartTime;
        public bool active => src != null && src.isPlaying;
        public double dspEndTime => dspStartTime + (src && src.clip ? src.clip.length : 0.0);
        public bool nextScheduledForPlaylist = false;
    }

    private TrackState trackA = new TrackState();
    private TrackState trackB = new TrackState();

    private int currentPlaylistIndex = 0;
    private bool isPlaying = false;

    // Public
    public void Play()
    {
        if (beatmaps == null || beatmaps.Count == 0) return;
        currentPlaylistIndex = Mathf.Clamp(startIndex, 0, beatmaps.Count - 1);
        isPlaying = true;

        // Start the first track immediately (scheduled)
        StartTrack(GetNextPlaylistIndex(), GetAvailableTrack(trackA, trackB), AudioSettings.dspTime + startDelay, startFadeDuration);
    }

    public void Stop()
    {
        isPlaying = false;
        StopAllCoroutines();
        if (trackA.src != null) trackA.src.Stop();
        if (trackB.src != null) trackB.src.Stop();
        ClearTrack(trackA);
        ClearTrack(trackB);
    }

    public void SkipToNext()
    {
        if (!isPlaying) return;
        // Immediately schedule the next track to crossfade in over crossfadeDuration,
        // starting now + small startDelay so PlayScheduled still used.
        var available = GetAvailableTrack(trackA, trackB);
        double dspStart = AudioSettings.dspTime + startDelay;
        StartTrack(GetNextPlaylistIndex(), available, dspStart, crossfadeDuration);
        // Force crossfade-out of the currently playing other track
        var other = (available == trackA) ? trackB : trackA;
        if (other.src != null && other.src.isPlaying)
        {
            // start fade-out now to 0 over crossfadeDuration and stop at end
            StartCoroutine(FadeAndStop(other.src, crossfadeDuration, dspStart + crossfadeDuration));
        }
    }
    void Start()
    {
        if (musicSource == null)
        {
            Debug.Log("Music Source not setup");
            return;
        }

        // Clone music source to crossfade
        altSource = Instantiate(musicSource, musicSource.transform);
        altSource.playOnAwake = false;

        trackA.src = musicSource;
        trackB.src = altSource;

        musicSource.volume = 0f;
        altSource.volume = 0f;

        Play();
    }
    void Update()
    {
        if (!isPlaying) return;

        double dspNow = AudioSettings.dspTime;

        UpdateBeatScheduling(trackA, dspNow);
        UpdateBeatScheduling(trackB, dspNow);

        // Check if we need to schedule next track for A or B (crossfade)
        TryScheduleNextSong(trackA, dspNow);
        TryScheduleNextSong(trackB, dspNow);
    }
    private void UpdateBeatScheduling(TrackState track, double dspNow)
    {
        if (track == null || track.events == null || track.nextIndex >= track.events.Count) return;
        // schedule events that fall within lookahead window
        while (track.nextIndex < track.events.Count)
        {
            var be = track.events[track.nextIndex];
            double scheduledDsp = track.dspStartTime + be.time;
            double songTime = dspNow - track.dspStartTime;
            if (scheduledDsp <= dspNow + lookAheadTime)
            {
                // Call the beat BE
                OnBeatEventScheduled?.Invoke(be, scheduledDsp);
                track.nextIndex++;
            }
            else break;
        }
    }

    private void TryScheduleNextSong(TrackState track, double dspNow)
    {
        if (!track.active || track.src.clip == null) return;
        if (!HasMoreInPlaylist()) return;

        double endDsp = track.dspEndTime;
        double nextStartDsp = endDsp - Mathf.Max(0f, crossfadeDuration);

        if (track.nextScheduledForPlaylist) return;

        if (dspNow + lookAheadTime >= nextStartDsp)
        {
            if (currentPlaylistIndex >= beatmaps.Count && !loopPlaylist) return; // HANDLE running out of maps

            int nextIdx = currentPlaylistIndex; // because StartTrack used GetNextPlaylistIndex which advanced currentPlaylistIndex
            // acquire other track and start it exactly at nextStartDsp
            var otherTrack = (track == trackA) ? trackB : trackA;
            StartTrack(nextIdx, otherTrack, nextStartDsp, crossfadeDuration);
            // start fade-out of the current track over crossfadeDuration starting at nextStartDsp
            StartCoroutine(ScheduleFadeVolume(track.src, nextStartDsp, crossfadeDuration, track.src.volume, 0f, stopAtEnd: true));
            track.nextScheduledForPlaylist = true;
        }
    }
    private void StartTrack(int playlistIndex, TrackState track, double dspStart, float fadeInDuration)
    {
        if (playlistIndex < 0 || playlistIndex >= beatmaps.Count)
            return;

        var map = beatmaps[playlistIndex];
        if (map == null || map.clip == null)
            throw new Exception("Null Beatmap Clip at index " + playlistIndex);


        // Prepare
        track.src.clip = map.clip;
        track.map = map;
        track.events = BeatmapImporter.ParseFromCsv(map.beatsCSV);
        track.events.Sort((a, b) => a.time.CompareTo(b.time));
        track.nextIndex = 0;
        track.dspStartTime = dspStart;
        track.nextScheduledForPlaylist = false;

        track.src.volume = 0f;
        track.src.PlayScheduled(dspStart);

        // Schedule Fade-In
        float targetVol = 1f;
        StartCoroutine(ScheduleFadeVolume(track.src, dspStart, fadeInDuration, 0f, targetVol));


        track.nextScheduledForPlaylist = false;
    }

    #region Fading Music
    private IEnumerator ScheduleFadeVolume(AudioSource src, double dspTargetStart, float duration, float fromVol, float toVol, bool stopAtEnd = false)
    {
        double wait = dspTargetStart - AudioSettings.dspTime;
        if (wait > 0) yield return new WaitForSecondsRealtime((float)wait);
        yield return StartCoroutine(FadeVolume(src, fromVol, toVol, duration));
        if (stopAtEnd) src.Stop();
    }


    private IEnumerator FadeVolume(AudioSource src, float from, float to, float duration)
    {
        if (src == null) yield break;
        float start = Time.realtimeSinceStartup;
        float end = start + Mathf.Max(0.0001f, duration);
        src.volume = from;
        while (Time.realtimeSinceStartup < end)
        {
            float t = (Time.realtimeSinceStartup - start) / Mathf.Max(0.0001f, duration);
            src.volume = Mathf.Lerp(from, to, t);
            yield return null;
        }
        src.volume = to;
    }
    private IEnumerator FadeAndStop(AudioSource src, float duration, double stopDspTime)
    {
        yield return StartCoroutine(FadeVolume(src, src.volume, 0f, duration));
        double wait = stopDspTime - AudioSettings.dspTime;
        if (wait > 0) yield return new WaitForSecondsRealtime((float)wait);
        src.Stop();
    }
    private void ClearTrack(TrackState t)
    {
        t.map = null;
        t.events = null;
        t.nextIndex = 0;
        t.dspStartTime = 0.0;
        t.nextScheduledForPlaylist = false;
    }
    #endregion

    #region misc
    private bool HasMoreInPlaylist() => beatmaps != null && beatmaps.Count > 0 && (loopPlaylist || currentPlaylistIndex < beatmaps.Count);
    private int GetNextPlaylistIndex()
    {
        int idx = currentPlaylistIndex;
        
        currentPlaylistIndex++;
        if (currentPlaylistIndex >= beatmaps.Count)
        {
            if (loopPlaylist) currentPlaylistIndex = 0;
            else currentPlaylistIndex = beatmaps.Count;
        }
        return idx;
    }
    private TrackState GetAvailableTrack(TrackState a, TrackState b)
    {
        if (!a.active) return a;
        if (!b.active) return b;
        return (a.src.volume <= b.src.volume) ? a : b;
    }
    #endregion
}

[Serializable]
public class BeatEvent
{
    public int type;
    public float time;
}


[CreateAssetMenu(menuName = "Audio/Beatmap")]
public class Beatmap : ScriptableObject
{
    public AudioClip clip;
    public float bpm;
    public float offset;
    public TextAsset beatsCSV;
}

public static class BeatmapImporter
{
    public static List<BeatEvent> ParseFromCsv(TextAsset csvAsset)
    {
        if (csvAsset == null) return new List<BeatEvent>();
        return ParseFromCsvText(csvAsset.text);
    }

    public static List<BeatEvent> ParseFromCsvText(string text)
    {
        var list = new List<BeatEvent>();
        if (string.IsNullOrWhiteSpace(text)) return list;

        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); // Split CSV 
        int start = 0;
        
        // Remove Header
        if (lines.Length > 0 && lines[0].TrimStart().StartsWith("type", StringComparison.OrdinalIgnoreCase))
            start = 1;

        for (int i = start; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0) continue;

            var parts = line.Split(',');
            if (parts.Length < 2) continue;

            int type = 0;
            float time = 0f;

            int.TryParse(parts[0].Trim(), out type);
            float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out time);

            list.Add(new BeatEvent { type = type, time = time });
        }

        list.Sort((a, b) => a.time.CompareTo(b.time));
        return list;
    }
}