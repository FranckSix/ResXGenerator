﻿using System.Diagnostics.CodeAnalysis;

namespace Aigamo.ResXGenerator;

internal static class StringExtensions
{
	public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

	public static string? NullIfEmpty(this string? value) => value.IsNullOrEmpty() ? null : value;
}
