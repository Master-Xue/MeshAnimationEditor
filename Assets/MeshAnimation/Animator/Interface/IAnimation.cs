using System;

public interface IAnimation 
{
    bool Loop { get; set; }
    bool PlayOnAwake { get; set; }
    float Speed { get; set; }
    void Play();
    void Pause();
    void Stop();
    Action<string> AnimationFinished { get; set; }
}
