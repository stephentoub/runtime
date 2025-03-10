// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//*****************************************************************************
// File: stdafx.h
//

// Prevent the inclusion of Random.h from disabling rand().  rand() is used by some other headers we include
// and there's no reason why DAC should be forbidden from using it.
#define DO_NOT_DISABLE_RAND

#define USE_COM_CONTEXT_DEF

#ifndef FEATURE_CDAC_UNWINDER
#include <common.h>
#include <debugger.h>
#include <methoditer.h>
#else // FEATURE_CDAC_UNWINDER
#include <windows.h>
#include <contract.h>
#include <daccess.h>
#include <clrnt.h>
#endif // FEATURE_CDAC_UNWINDER

#ifdef DACCESS_COMPILE
#include <dacprivate.h>
#include <dacimpl.h>
#endif // DACCESS_COMPILE
