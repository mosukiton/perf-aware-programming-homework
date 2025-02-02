#!/usr/bin/env sh

exe=./Decoder.Tests/bin/x64/Release/net8.0/linux-x64/Homework.001.Decoder

function Decode() {
    echo --- Decoding \"$1\" to \"$2\" ---
    $exe $1 $2
}

dotnet publish

input_0037=./Decoder.Tests/Input/listing_0037_single_register_mov
input_0038=./Decoder.Tests/Input/listing_0038_many_register_mov
input_0039=./Decoder.Tests/Input/listing_0039_more_movs
input_0040=./Decoder.Tests/Input/listing_0040_challenge_movs
input_0041=./Decoder.Tests/Input/listing_0041_add_sub_cmp_jnz

asm_0037=./Decoder.Tests/Output/homework_0037.asm
asm_0038=./Decoder.Tests/Output/homework_0038.asm
asm_0039=./Decoder.Tests/Output/homework_0039.asm
asm_0040=./Decoder.Tests/Output/homework_0040.asm
asm_0041=./Decoder.Tests/Output/homework_0041.asm

Decode $input_0037 $asm_0037
Decode $input_0038 $asm_0038
Decode $input_0039 $asm_0039
Decode $input_0040 $asm_0040
Decode $input_0041 $asm_0041

nasm $asm_0037
nasm $asm_0038
nasm $asm_0039
nasm $asm_0040
nasm $asm_0041

output_0037=./Decoder.Tests/Output/homework_0037
output_0038=./Decoder.Tests/Output/homework_0038
output_0039=./Decoder.Tests/Output/homework_0039
output_0040=./Decoder.Tests/Output/homework_0040
output_0041=./Decoder.Tests/Output/homework_0041

cmp -l $input_0037 $output_0037
cmp -l $input_0038 $output_0038
cmp -l $input_0039 $output_0039
cmp -l $input_0040 $output_0040
cmp -l $input_0041 $output_0041
