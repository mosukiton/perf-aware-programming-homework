using Homework001.Instructions;
using TUnit.Assertions.AssertConditions.Throws;

namespace Decoder.Tests.Instructions;

public class OpcodeTests
{
    [Test]
    [Arguments(0b_1000_1000, OperationCode.Mov_RegOrMemTo_FromReg)]

    [Arguments(0b_1100_0110, OperationCode.Mov_ImmediateToRegOrMem)]

    [Arguments(0b_1011_0000, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1010_0001, OperationCode.Mov_MemToAcc)]
    [Arguments(0b_1010_0011, OperationCode.Mov_AccToMem)]

    [Arguments(0b_0000_0011, OperationCode.Add_RegOrMemWithRegToEither)]
    [Arguments(0b_0000_0101, OperationCode.Add_ImmediateToAcc)]

    [Arguments(0b_0010_1011, OperationCode.Sub_RegOrMemAndRegToEither)]
    [Arguments(0b_0010_1100, OperationCode.Sub_ImmediateFromAcc)]

    [Arguments(0b_0011_1011, OperationCode.Cmp_RegOrMemAndReg)]
    [Arguments(0b_0011_1101, OperationCode.Cmp_ImmediateWithAcc)]

    [Arguments(0b_1000_0000, OperationCode.Add_Sub_Cmp_ImmediateToRegOrMem)]

    [Arguments(0b_0111_0000, OperationCode.JO)]
    [Arguments(0b_0111_0001, OperationCode.JNO)]
    [Arguments(0b_0111_0010, OperationCode.JB_JNAE)]
    [Arguments(0b_0111_0011, OperationCode.JNB_JAE)]
    [Arguments(0b_0111_0100, OperationCode.JE_JZ)]
    [Arguments(0b_0111_0101, OperationCode.JNE_JNZ)]
    [Arguments(0b_0111_0110, OperationCode.JBE_JNA)]
    [Arguments(0b_0111_0111, OperationCode.JNBE_JA)]
    [Arguments(0b_0111_1000, OperationCode.JS)]
    [Arguments(0b_0111_1001, OperationCode.JNS)]
    [Arguments(0b_0111_1010, OperationCode.JP_JPE)]
    [Arguments(0b_0111_1011, OperationCode.JNP_JPO)]
    [Arguments(0b_0111_1100, OperationCode.JL_JNGE)]
    [Arguments(0b_0111_1101, OperationCode.JNL_JGE)]
    [Arguments(0b_0111_1110, OperationCode.JLE_JNG)]
    [Arguments(0b_0111_1111, OperationCode.JNLE_JG)]

    [Arguments(0b_1110_0010, OperationCode.Loop)]
    [Arguments(0b_1110_0001, OperationCode.LoopZ_LoopE)]
    [Arguments(0b_1110_0000, OperationCode.LoopNZ_LoopNE)]
    [Arguments(0b_1110_0011, OperationCode.JCXZ)]
    public async Task When_GivenCorrectBits_ParseOpcode(
        byte inputBits,
        OperationCode expected)
    {
        await Assert.That(() =>
        {
            Opcode objectUnderTest = new(inputBits);

        }).ThrowsNothing();

        await Assert.That(new Opcode(inputBits).Code).IsEqualTo(expected);
    }


    [Test]
    [Arguments(0b_1000_1011, OperationCode.Mov_RegOrMemTo_FromReg)]
    [Arguments(0b_1000_1001, OperationCode.Mov_RegOrMemTo_FromReg)]
    [Arguments(0b_1000_1010, OperationCode.Mov_RegOrMemTo_FromReg)]

    [Arguments(0b_1100_0110, OperationCode.Mov_ImmediateToRegOrMem)]
    [Arguments(0b_1100_0111, OperationCode.Mov_ImmediateToRegOrMem)]

    [Arguments(0b_1011_0000, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_0001, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_0010, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_0011, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_0100, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_0101, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_0110, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_0111, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_1000, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_1001, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_1010, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_1011, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_1100, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_1101, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_1110, OperationCode.Mov_ImmediateToReg)]
    [Arguments(0b_1011_1111, OperationCode.Mov_ImmediateToReg)]
    public async Task When_GivenExtraBits_Then_IgnoreThemAndParseCorrectly(
        byte inputBits,
        OperationCode expected)
    {
        await Assert.That(() =>
        {
            Opcode objectUnderTest = new(inputBits);

        }).ThrowsNothing();

        await Assert.That(new Opcode(inputBits).Code).IsEqualTo(expected);
    }
}
