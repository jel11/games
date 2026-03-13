using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace NarutoRoguelike.Managers
{
    /// <summary>
    /// Null-safe audio manager. All methods silently no-op if audio is unavailable.
    /// </summary>
    public class AudioManager
    {
        private readonly ContentManager                   _content;
        private readonly Dictionary<string, SoundEffect?> _sfxCache  = new();
        private readonly Dictionary<string, Song?>        _songCache = new();

        private float _sfxVolume   = 0.8f;
        private float _musicVolume = 0.5f;
        private bool  _muted;

        public float SFXVolume
        {
            get => _sfxVolume;
            set { _sfxVolume = Math.Clamp(value, 0f, 1f); SoundEffect.MasterVolume = _sfxVolume; }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set { _musicVolume = Math.Clamp(value, 0f, 1f); MediaPlayer.Volume = _musicVolume; }
        }

        public bool Muted
        {
            get => _muted;
            set { _muted = value; MediaPlayer.IsMuted = _muted; }
        }

        public AudioManager(ContentManager content)
        {
            _content = content;
        }

        // ── SFX ───────────────────────────────────────────────────────────────────

        public void PlaySFX(string name, float volumeScale = 1f)
        {
            if (_muted) return;
            var sfx = GetSFX(name);
            sfx?.Play(_sfxVolume * volumeScale, 0f, 0f);
        }

        private SoundEffect? GetSFX(string name)
        {
            if (_sfxCache.TryGetValue(name, out var cached)) return cached;
            try
            {
                var sfx = _content.Load<SoundEffect>($"Audio/{name}");
                _sfxCache[name] = sfx;
                return sfx;
            }
            catch
            {
                _sfxCache[name] = null;   // cache miss to avoid repeated exceptions
                return null;
            }
        }

        // ── Music ─────────────────────────────────────────────────────────────────

        public void PlayMusic(string name, bool loop = true)
        {
            if (_muted) return;
            var song = GetSong(name);
            if (song == null) return;

            if (MediaPlayer.State == MediaState.Playing &&
                MediaPlayer.Queue.ActiveSong == song) return;  // already playing

            MediaPlayer.Volume     = _musicVolume;
            MediaPlayer.IsRepeating = loop;
            MediaPlayer.Play(song);
        }

        public void StopMusic() => MediaPlayer.Stop();

        public void PauseMusic()
        {
            if (MediaPlayer.State == MediaState.Playing) MediaPlayer.Pause();
        }

        public void ResumeMusic()
        {
            if (MediaPlayer.State == MediaState.Paused) MediaPlayer.Resume();
        }

        private Song? GetSong(string name)
        {
            if (_songCache.TryGetValue(name, out var cached)) return cached;
            try
            {
                var song = _content.Load<Song>($"Audio/{name}");
                _songCache[name] = song;
                return song;
            }
            catch
            {
                _songCache[name] = null;
                return null;
            }
        }

        // ── Named SFX helpers ─────────────────────────────────────────────────────

        public void PlayAttack()  => PlaySFX("sfx_attack");
        public void PlayHurt()    => PlaySFX("sfx_hurt");
        public void PlayPickUp()  => PlaySFX("sfx_pickup");
        public void PlayLevelUp() => PlaySFX("sfx_levelup");
        public void PlayJutsu()   => PlaySFX("sfx_jutsu");
        public void PlayStairs()  => PlaySFX("sfx_stairs");
    }
}
