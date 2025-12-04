using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace GltronMobileGame.Sound;

public class SoundManager
{
    private static SoundManager _instance;
    public static SoundManager Instance => _instance ??= new SoundManager();

    private ContentManager _content;
    private SoundEffect _music;
    private SoundEffect _engine;
    private SoundEffect _crash;
    private SoundEffect _recognizer;
    private SoundEffectInstance _engineInstance;
    private SoundEffectInstance _recognizerInstance;

    private SoundManager() { }

    public void Initialize(ContentManager content)
    {
        _content = content;
        try
        {
            _music = _content.Load<SoundEffect>("Assets/song_revenge_of_cats");
            _engine = _content.Load<SoundEffect>("Assets/game_engine");
            _crash = _content.Load<SoundEffect>("Assets/game_crash");
            _recognizer = _content.Load<SoundEffect>("Assets/game_recognizer");
            System.Diagnostics.Debug.WriteLine("GLTRON: Initialize: Loaded audio assets: music, engine, crash, recognizer");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GLTRON: Sound content load failed: {ex}");
            throw;
        }
    }

    private SoundEffectInstance _musicInstance;

    public void PlayMusic(bool loop = true, float volume = 0.6f)
    {
        if (_music == null) { System.Diagnostics.Debug.WriteLine("GLTRON: PlayMusic: _music is null"); return; }
        _musicInstance ??= _music.CreateInstance();
        _musicInstance.IsLooped = loop;
        _musicInstance.Volume = volume;
        if (_musicInstance.State != SoundState.Playing)
        {
            _musicInstance.Play();
        }
        System.Diagnostics.Debug.WriteLine($"GLTRON: PlayMusic called: state={_musicInstance.State}, vol={_musicInstance.Volume}, loop={_musicInstance.IsLooped}");
    }

    public void StopMusic()
    {
        if (_musicInstance != null && _musicInstance.State == SoundState.Playing)
        {
            _musicInstance.Stop();
            System.Diagnostics.Debug.WriteLine("GLTRON: StopMusic called: music stopped");
        }
    }

    public void PlayEngine(float volume = 0.4f, bool loop = true)
    {
        if (_engine == null) return;
        _engineInstance ??= _engine.CreateInstance();
        _engineInstance.Volume = volume;
        _engineInstance.IsLooped = loop;
        if (_engineInstance.State != SoundState.Playing)
            _engineInstance.Play();
        System.Diagnostics.Debug.WriteLine($"GLTRON: PlayEngine: state={_engineInstance.State}, vol={_engineInstance.Volume}, loop={_engineInstance.IsLooped}");
    }

    public void StopEngine()
    {
        if (_engineInstance != null && _engineInstance.State == SoundState.Playing)
            _engineInstance.Stop();
    }

    public void PlayCrash(float volume = 0.8f)
    {
        _crash?.Play(volume, 0f, 0f);
    }

    public void PlayRecognizer(float volume = 0.4f, bool loop = true)
    {
        if (_recognizer == null) return;
        _recognizerInstance ??= _recognizer.CreateInstance();
        _recognizerInstance.Volume = volume;
        _recognizerInstance.IsLooped = loop;
        if (_recognizerInstance.State != SoundState.Playing)
            _recognizerInstance.Play();
        System.Diagnostics.Debug.WriteLine($"GLTRON: PlayRecognizer: state={_recognizerInstance.State}, vol={_recognizerInstance.Volume}, loop={_recognizerInstance.IsLooped}");
    }

    public void StopRecognizer()
    {
        if (_recognizerInstance != null && _recognizerInstance.State == SoundState.Playing)
            _recognizerInstance.Stop();
    }

    public void StopSound(int soundId)
    {
        // Handle legacy sound ID system from Java version
        switch (soundId)
        {
            case 1: // CRASH_SOUND
                // Crash sounds are one-shot, no need to stop
                break;
            case 2: // ENGINE_SOUND
                StopEngine();
                break;
            case 3: // MUSIC_SOUND
                StopMusic();
                break;
            case 4: // RECOGNIZER_SOUND
                StopRecognizer();
                break;
        }
    }
}
