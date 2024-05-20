bits 16

mov [bp + di], byte 7
mov [di + 901], word 347

add byte [bx], 34
add word [bx], 34
add byte [bp + si + 1000], 29
add word [bp + si + 1000], 29
add word [bp + si + 1000], 1234

sub byte [bx], 34
sub word [bx], 34
sub byte [bx + di], 29
sub word [bx + di], 29
sub word [bx + di], 1234

cmp byte [bx], 34
cmp word [bx], 34
cmp byte [4834], 29
cmp word [4834], 29
cmp word [4834], 1234
