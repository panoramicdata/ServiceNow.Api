﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Naming",
	"CA1707:Identifiers should not contain underscores",
	Justification = "Underscores form part of the default three-part naming convention for unit tests",
	Scope = "module")]
[assembly: SuppressMessage("Style",
	"IDE0058:Expression value is never used",
	Justification = "Fluent assertion takes this form",
	Scope = "module")]
