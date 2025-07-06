﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.Utils.Parser.KV3.Utils;

#pragma warning disable CA1032 // Implement standard exception constructors
public class UnexpectedMagicException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
{
    private readonly string Magic;
    private readonly string MagicNameof;

    private readonly bool IsAssertion;

    public override string Message => IsAssertion
        ? base.Message
        : $"{base.Message} for variable '{MagicNameof}': {Magic}";

    public UnexpectedMagicException(string message, int magic, string nameofMagic) : base(message)
    {
        Magic = $"{magic} (0x{magic:X})";
        MagicNameof = nameofMagic;
    }

    public UnexpectedMagicException(string message, uint magic, string nameofMagic) : base(message)
    {
        Magic = $"{magic} (0x{magic:X})";
        MagicNameof = nameofMagic;
    }

    public UnexpectedMagicException(string message, string magic, string nameofMagic) : base(message)
    {
        Magic = magic;
        MagicNameof = nameofMagic;
    }

    private UnexpectedMagicException(string customAssertMessage) : base(customAssertMessage)
    {
        IsAssertion = true;
    }

    public static void Assert<T>(bool condition, T actualMagic,
        [CallerArgumentExpression(nameof(condition))] string conditionExpression = null)
    {
        if (!condition)
        {
            var formattedMagic = actualMagic is int or uint or byte
                ? $"{actualMagic} (0x{actualMagic:X})"
                : $"{actualMagic}";
            throw new UnexpectedMagicException($"Assertion '{conditionExpression}' failed. Value: {formattedMagic}");
        }
    }
}
