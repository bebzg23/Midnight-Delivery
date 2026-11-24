using System;
using System.Collections.Generic;
using UnityEngine;

/// Schedules N jumpscares at random times during the night.
/// Scares are picked from the list without repeating (until the list runs out).
public class JumpscareDirector : MonoBehaviour
{
    [SerializeField] List<JumpscareEvent> scares = new List<JumpscareEvent>();
    [Tooltip("No scare before/after this fraction of the night (0-1).")]
    [SerializeField, Range(0f, 1f)] float earliest = 0.2f;
    [SerializeField, Range(0f, 1f)] float latest = 0.9f;

    readonly List<(float time, JumpscareEvent scare)> queue = new List<(float, JumpscareEvent)>();
    float elapsed;
    bool running;

    public void Schedule(int count, float nightDuration, System.Random rng)
    {
        StopAll();
        if (count <= 0 || scares.Count == 0) return;

        var pool = new List<JumpscareEvent>(scares);
        for (int i = 0; i < count; i++)
        {
            if (pool.Count == 0) pool.AddRange(scares); // reuse if not enough unique scares
            int idx = rng.Next(pool.Count);
            float t = nightDuration * Mathf.Lerp(earliest, latest, (float)rng.NextDouble());
            queue.Add((t, pool[idx]));
            pool.RemoveAt(idx);
        }
        queue.Sort((a, b) => a.time.CompareTo(b.time));
        elapsed = 0f;
        running = true;
    }

    public void StopAll()
    {
        running = false;
        queue.Clear();
    }

    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;

        while (queue.Count > 0 && elapsed >= queue[0].time)
        {
            queue[0].scare.Trigger();
            queue.RemoveAt(0);
        }
    }
}
