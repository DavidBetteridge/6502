namespace _6502;

public enum AddressMode
{
    Immediate,
    ZeroPage,
    ZeroPageX,
    Absolute,
    AbsoluteX,
    AbsoluteY,
    IndirectX,
    IndirectY
}

public struct ClockCycles(int value)
{
    public readonly int Value = value;

    public static ClockCycles Zero => new(0);
}

public class Processor(Clock clock)
{
    // http://www.6502.org/users/obelisk/6502/instructions.html

    private byte[] _memory = new byte[32 * 1024];

    private byte A = 0;
    private byte X = 0;
    private byte Y = 0;
    private byte PC;

    private bool N_Flag = false;
    private bool Z_Flag = false;

    /// <summary>
    /// Returns the number of clock cycles used by the opcode.
    /// </summary>
    /// <returns></returns>
    public ClockCycles ExecuteNextInstruction()
    {
        var opCode = _memory[PC];
        PC++;

        var clockCycles = opCode switch
        {
            0xA9 => LDA(AddressMode.Immediate),
            0xA5 => LDA(AddressMode.ZeroPage),
            0xB5 => LDA(AddressMode.ZeroPageX),
            _ => ClockCycles.Zero
        };

        return clockCycles;
    }

    private byte ReadImmediate()
    {
        var value = _memory[PC];
        PC++;
        return value;
    }

    private byte ReadZeroPage()
    {
        // Read 8 bit address 0x0000-->0x00FF
        var address = _memory[PC];
        PC++;
        return _memory[address];
    }

    private byte ReadZeroPageX()
    {
        // Read 8 bit address 0x0000-->0x00FF
        var address = (_memory[PC] + X) % 0xFF;
        PC++;
        return _memory[address];
    }
    
    private byte ReadAbsolute()
    {
        // Read 16-bit address (Little Endian)
        var address = (_memory[PC + 1] << 8) + _memory[PC];
        PC+=2;
        return _memory[address];
    }
    
    private byte ReadAbsoluteX()
    {
        // Read 16-bit address (Little Endian)
        var address = (_memory[PC + 1] << 8) + _memory[PC] + X;
        PC+=2;
        
        //TODO:  If adding X meant a different page was selected (high byte) then an extra clock cycle is used.
        clock.Pulse();
        
        return _memory[address];
    }
    
    private byte ReadAbsoluteY()
    {
        // Read 16-bit address (Little Endian)
        var address = (_memory[PC + 1] << 8) + _memory[PC] + Y;
        PC+=2;
        return _memory[address];
    }
    
    private byte ReadValue(AddressMode addressMode)
    {
        return addressMode switch
        {
            AddressMode.Immediate => ReadImmediate(),
            AddressMode.ZeroPage => ReadZeroPage(),
            AddressMode.ZeroPageX => ReadZeroPageX(),
            AddressMode.Absolute => ReadAbsolute(),
            _ => 0
        };
    }
    
    
    
    private ClockCycles LDA(AddressMode addressMode)
    {
        A = ReadValue(addressMode);
        Z_Flag = (A == 0);
        N_Flag = (A & 0b0100_0000) == 0b0100_0000;

        return addressMode switch
        {
            AddressMode.Immediate => new ClockCycles(2),
            AddressMode.ZeroPage => new ClockCycles(3),
            AddressMode.ZeroPageX => new ClockCycles(4),
            AddressMode.Absolute => new ClockCycles(4),
            _ => ClockCycles.Zero
        };
    }
}