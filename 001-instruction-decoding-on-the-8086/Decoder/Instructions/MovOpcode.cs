namespace Homework001.Instructions;
    
public enum MovOpcode
{
    RegisterOrMemoryTo_FromRegister,
    ImmediateToRegisterOrMemory,
    ImmediateToRegister,
    MemoryToAccumulator,
    AccumulatorToMemory,
    RegisterOrMemoryToSegmentRegister,
    SegmentRegisterToRegisterOrMemory,
}
