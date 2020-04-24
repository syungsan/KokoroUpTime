using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;

public class WavesPlayer : IDisposable
{
    IDictionary<string, SoundPlayer> players;

    public WavesPlayer(IDictionary<string, string> waveFiles)
    {
        if (waveFiles == null) throw new ArgumentNullException("waveFiles");

        players = waveFiles.ToDictionary(s => s.Key, s => new SoundPlayer(s.Value));
    }

    public void Load()
    {
        foreach (var player in players.Values)
        {
            player.Load();
        }
    }

    public void LoadAsync()
    {
        foreach (var player in players.Values)
        {
            player.LoadAsync();
        }
    }

    public void Play(string name)
    {
        players[name].Play();
    }

    WavesPlayer()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        foreach (var player in players.Values)
        {
            try
            {
                player.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }
}
