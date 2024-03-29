﻿//
//  OptionalNotPresentError.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace NosSmooth.LocalBinding.Errors;

public record OptionalNotPresentError(string TypeName) : ResultError($"The optional {TypeName} is not present.");