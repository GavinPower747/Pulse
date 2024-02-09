using System.Buffers;
using System.Text;

namespace Pulse.Shared.Encoders;

public class Base64UrlEncoder
{
    public static string Encode(string input) => Encode(Encoding.UTF8.GetBytes(input));

    public static string Encode(ReadOnlySpan<byte> input)
    {
        const int stackAllocThreshold = 128;

        if (input.Length == 0)
        {
            return string.Empty;
        }

        // Use the stack for small inputs, otherwise rent an array to avoid large object heap.
        char[]? rented = null;
        Span<char> output =
            input.Length <= stackAllocThreshold
                ? stackalloc char[GetArraySizeRequiredToEncode(input.Length)]
                : rented = ArrayPool<char>.Shared.Rent(GetArraySizeRequiredToEncode(input.Length));

        var encoded = Convert.TryToBase64Chars(input, output, out var charsWritten);

        if (!encoded)
        {
            throw new InvalidOperationException("The input could not be encoded.");
        }

        // Rewrite some url unsafe characters so we can include the result in a URL.
        for (int i = 0; i < charsWritten; i++)
        {
            if (output[i] == '/')
            {
                output[i] = '_';
            }
            else if (output[i] == '+')
            {
                output[i] = '-';
            }
            else if (output[i] == '=')
            {
                charsWritten = i;
                break;
            }
        }

        var result = new string(output[..charsWritten]);

        if (rented != null)
        {
            ArrayPool<char>.Shared.Return(rented);
        }

        return result;
    }

    public static byte[] Decode(string input)
    {
        if (input.Length == 0)
        {
            return [];
        }

        var paddingCharsToAdd = GetNumBase64PaddingCharsToAddForDecode(input.Length);
        var arraySizeRequired = checked(input.Length + paddingCharsToAdd);
        var buffer = new char[arraySizeRequired];

        if (buffer.Length < arraySizeRequired)
        {
            throw new InvalidOperationException("The input is not a valid Base64 string.");
        }

        // Copy input into buffer, fixing up '-' -> '+' and '_' -> '/'.
        int i = 0;
        for (; i < input.Length; i++)
        {
            buffer[i] = input[i] switch
            {
                '-' => '+',
                '_' => '/',
                _ => input[i]
            };
        }

        // Add the padding characters back.
        for (; paddingCharsToAdd > 0; i++, paddingCharsToAdd--)
        {
            buffer[i] = '=';
        }

        return Convert.FromBase64CharArray(buffer, 0, arraySizeRequired);
    }

    private static int GetArraySizeRequiredToEncode(int length)
    {
        // Base64 encoding is divided into blocks of 3 input bytes, which get mapped to 4 output characters.
        var numWholeOrPartialInputBlocks = checked(length + 2) / 3;
        return checked(numWholeOrPartialInputBlocks * 4);
    }

    private static int GetArraySizeRequiredToDecode(int length)
    {
        if (length == 0)
        {
            return 0;
        }

        var numPaddingCharsToAdd = GetNumBase64PaddingCharsToAddForDecode(length);

        return checked(length + numPaddingCharsToAdd);
    }

    private static int GetNumBase64PaddingCharsToAddForDecode(int length) =>
        (length % 4) switch
        {
            0 => 0,
            2 => 2,
            3 => 1,
            _ => throw new InvalidOperationException("The input is not a valid Base64 string."),
        };
}
