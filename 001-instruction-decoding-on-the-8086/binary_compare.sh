#!/usr/bin/env sh
echo "testing homework001 - listing0038 many register"
cmp -l Decoder/Input/homework001_listing_0038 Decoder/Input/listing_0038_many_register_mov 
echo "testing homework002 - listing0039 more movs"
cmp -l Decoder/Input/homework002_listing_0039 Decoder/Input/listing_0039_more_movs
echo "testing homework002 bonus - listing0040 challenge movs"
cmp -l Decoder/Input/homework002_listing_0040 Decoder/Input/listing_0040_challenge_movs 
