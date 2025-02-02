namespace Homework001.Instructions;

public enum OperationCode : byte
{
    Mov_RegOrMemTo_FromReg,             // 1 0 0 0 _ 1 0 x x
    Mov_ImmediateToRegOrMem,            // 1 1 0 0 _ 0 1 1 x
    Mov_ImmediateToReg,                 // 1 0 1 1 _ x x x x
    Mov_MemToAcc,                       // 1 0 1 0 _ 0 0 0 x
    Mov_AccToMem,                       // 1 0 1 0 _ 0 0 1 x

    Add_RegOrMemWithRegToEither,        // 0 0 0 0 _ 0 0 x x
    Add_ImmediateToAcc,                 // 0 0 0 0 _ 0 1 0 x

    Sub_RegOrMemAndRegToEither,         // 0 0 1 0 _ 1 0 x x
    Sub_ImmediateFromAcc,               // 0 0 1 0 _ 1 1 0 x

    Cmp_RegOrMemAndReg,                 // 0 0 1 1 _ 1 0 x x
    Cmp_ImmediateWithAcc,               // 0 0 1 1 _ 1 1 0 x

    Add_Sub_Cmp_ImmediateToRegOrMem,    // 1 0 0 0 _ 0 0 x x

    JO      = 0b_0111_0000,
    JNO     = 0b_0111_0001,
    JB_JNAE = 0b_0111_0010,
    JNB_JAE = 0b_0111_0011,
    JE_JZ   = 0b_0111_0100,
    JNE_JNZ = 0b_0111_0101,
    JBE_JNA = 0b_0111_0110,
    JNBE_JA = 0b_0111_0111,
    JS      = 0b_0111_1000,
    JNS     = 0b_0111_1001,
    JP_JPE  = 0b_0111_1010,
    JNP_JPO = 0b_0111_1011,
    JL_JNGE = 0b_0111_1100,
    JNL_JGE = 0b_0111_1101,
    JLE_JNG = 0b_0111_1110,
    JNLE_JG = 0b_0111_1111,

    Loop            = 0b_1110_0010,           // 1 1 1 0 _ 0 0 1 0
    LoopZ_LoopE     = 0b_1110_0001,    // 1 1 1 0 _ 0 0 0 1
    LoopNZ_LoopNE   = 0b_1110_0000,  // 1 1 1 0 _ 0 0 0 0
    JCXZ            = 0b_1110_0011,           // 1 1 1 0 _ 0 0 1 1
}
