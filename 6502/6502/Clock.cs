namespace _6502;

/// <summary>
/// Our clock works in reverse,  rather than it being a crystal which emits pulses,  we instead
/// allow the CPU to tell us how many cycles the current instruction used.
/// </summary>
public class Clock
{
    private int _cycles = 0;
    
    public void Reset()
    {
        _cycles = 0;
    }

    public void Pulse(int cycles = 1)
    {
        _cycles += cycles;
    }
}