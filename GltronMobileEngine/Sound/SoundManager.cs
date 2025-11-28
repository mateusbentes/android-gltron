using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace GltronMobileEngine.Sound;

public class SoundManager
{
    private static SoundManager? _instance;
    public static SoundManager Instance => _instance ??= new SoundManager();

    private ContentManager? _content;
    private Song? _music;
    private SoundEffect? _engine;
    private SoundEffect? _crash;
    private SoundEffectInstance? _engineInstance;

    private SoundManager() { }

    public void Initialize(ContentManager content)
    {
        _content = content;
        try
        {
            _music = _content.Load<Song>("Assets/song_revenge_of_cats");
            _engine = _content.Load<SoundEffect>("Assets/game_engine");
            _crash = _content.Load<SoundEffect>("Assets/game_crash");
            // Sound content loaded successfully
        }
        catch (System.Exception)
        {
            // Sound content load failed - continue without sound
            throw;
        }
    }

    public void PlayMusic(bool loop = true, float volume = 0.5f)
    {
        if (_music == null) return;
        MediaPlayer.IsRepeating = loop;
        MediaPlayer.Volume = volume;
        MediaPlayer.Play(_music);
    }

    public void StopMusic()
    {
        if (MediaPlayer.State == MediaState.Playing)
            MediaPlayer.Stop();
    }

    public void PlayEngine(float volume = 0.3f, bool loop = true)
    {
        if (_engine == null) return;
        _engineInstance ??= _engine.CreateInstance();
        _engineInstance.Volume = volume;
        _engineInstance.IsLooped = loop;
        if (_engineInstance.State != SoundState.Playing)
            _engineInstance.Play();
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
}
