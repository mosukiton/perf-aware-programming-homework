namespace Homework001.Instructions;
    
public enum Opcode
{
    MovRegisterOrMemoryTo_FromRegister,
    MovImmediateToRegisterOrMemory,
    MovImmediateToRegister,
    MovMemoryToAccumulator,
    MovAccumulatorToMemory,
    MovRegisterOrMemoryToSegmentRegister,
    MovSegmentRegisterToRegisterOrMemory,
}
