// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// util.hpp
//

//
// Miscellaneous useful functions
//

#ifndef _H_UTIL
#define _H_UTIL

#include "utilcode.h"
#include "metadata.h"
#include "clrdata.h"
#include "xclrdata.h"
#include "posterror.h"
#include <type_traits>
#include "minipal/time.h"

#ifndef DACCESS_COMPILE
#if defined(TARGET_WINDOWS) && defined(TARGET_ARM64)
// Flag to check if atomics feature is available on
// the machine
extern bool g_arm64_atomics_present;
#endif
#endif

#ifndef TARGET_UNIX
// Copied from malloc.h: don't want to bring in the whole header file.
void * __cdecl _alloca(size_t);
#endif // !TARGET_UNIX

BOOL inline FitsInI1(int64_t val)
{
    LIMITED_METHOD_DAC_CONTRACT;
    return val == (int64_t)(int8_t)val;
}

BOOL inline FitsInI2(int64_t val)
{
    LIMITED_METHOD_CONTRACT;
    return val == (int64_t)(int16_t)val;
}

BOOL inline FitsInI4(int64_t val)
{
    LIMITED_METHOD_DAC_CONTRACT;
    return val == (int64_t)(int32_t)val;
}

BOOL inline FitsInU1(uint64_t val)
{
    LIMITED_METHOD_CONTRACT;
    return val == (uint64_t)(uint8_t)val;
}

BOOL inline FitsInU2(uint64_t val)
{
    LIMITED_METHOD_CONTRACT;
    return val == (uint64_t)(uint16_t)val;
}

BOOL inline FitsInU4(uint64_t val)
{
    LIMITED_METHOD_DAC_CONTRACT;
    return val == (uint64_t)(uint32_t)val;
}

#if defined(DACCESS_COMPILE)
#define FastInterlockedCompareExchange InterlockedCompareExchange
#define FastInterlockedCompareExchangeAcquire InterlockedCompareExchangeAcquire
#define FastInterlockedCompareExchangeRelease InterlockedCompareExchangeRelease
#else

#if defined(TARGET_WINDOWS) && defined(TARGET_ARM64)

FORCEINLINE LONG  FastInterlockedCompareExchange(
    LONG volatile *Destination,
    LONG Exchange,
    LONG Comperand)
{
    if (g_arm64_atomics_present)
    {
        return (LONG) __casal32((unsigned __int32*) Destination, (unsigned  __int32)Comperand, (unsigned __int32)Exchange);
    }
    else
    {
        return InterlockedCompareExchange(Destination, Exchange, Comperand);
    }
}

FORCEINLINE LONG FastInterlockedCompareExchangeAcquire(
  IN OUT LONG volatile *Destination,
  IN LONG Exchange,
  IN LONG Comperand
)
{
    if (g_arm64_atomics_present)
    {
        return (LONG) __casa32((unsigned __int32*) Destination, (unsigned  __int32)Comperand, (unsigned __int32)Exchange);
    }
    else
    {
        return InterlockedCompareExchangeAcquire(Destination, Exchange, Comperand);
    }
}

FORCEINLINE LONG FastInterlockedCompareExchangeRelease(
  IN OUT LONG volatile *Destination,
  IN LONG Exchange,
  IN LONG Comperand
)
{
    if (g_arm64_atomics_present)
    {
        return (LONG) __casl32((unsigned __int32*) Destination, (unsigned  __int32)Comperand, (unsigned __int32)Exchange);
    }
    else
    {
        return InterlockedCompareExchangeRelease(Destination, Exchange, Comperand);
    }
}

#endif // defined(TARGET_WINDOWS) && defined(TARGET_ARM64)

#endif //defined(DACCESS_COMPILE)


//************************************************************************
// CQuickHeap
//
// A fast non-multithread-safe heap for short term use.
// Destroying the heap frees all blocks allocated from the heap.
// Blocks cannot be freed individually.
//
// The heap uses exceptions to report errors.
//
// The heap does not use any internal synchronization so it is not
// multithreadsafe.
//************************************************************************
class CQuickHeap
{
    public:
        CQuickHeap();
        ~CQuickHeap();

        //---------------------------------------------------------------
        // Allocates a block of "sz" bytes. If there's not enough
        // memory, throws an OutOfMemoryError.
        //---------------------------------------------------------------
        LPVOID Alloc(UINT sz);


    private:
        enum {
#ifdef _DEBUG
            kBlockSize = 24
#else
            kBlockSize = 1024
#endif
        };

        // The QuickHeap allocates QuickBlock's as needed and chains
        // them in a single-linked list. Most QuickBlocks have a size
        // of kBlockSize bytes (not counting m_next), and individual
        // allocation requests are suballocated from them.
        // Allocation requests of greater than kBlockSize are satisfied
        // by allocating a special big QuickBlock of the right size.
        struct QuickBlock
        {
            QuickBlock  *m_next;
            BYTE         m_bytes[1];
        };


        // Linked list of QuickBlock's.
        QuickBlock      *m_pFirstQuickBlock;

        // Offset to next available byte in m_pFirstQuickBlock.
        LPBYTE           m_pNextFree;

        // Linked list of big QuickBlock's
        QuickBlock      *m_pFirstBigQuickBlock;
};

void PrintToStdErrA(const char *pszString);
void PrintToStdErrW(const WCHAR *pwzString);

#include "nativevaraccessors.h"

// --------------------------------------------------------------------------------
// GCX macros
//
// These are the normal way to change or assert the GC mode of a thread.  They handle
// the required stack discipline in mode switches with an autodestructor which
// automatically triggers on leaving the current scope.
//
// Usage:
// GCX_COOP();              Switch to cooperative mode, assume thread is setup
// GCX_PREEMP();            Switch to preemptive mode, NOP if no thread setup
// GCX_COOP_THREAD_EXISTS(Thread*);    Fast switch to cooperative mode, must pass non-null Thread
// GCX_PREEMP_THREAD_EXISTS(Thread*);  Fast switch to preemptive mode, must pass non-null Thread
//
// (There is an intentional asymmetry between GCX_COOP and GCX_PREEMP. GCX_COOP
// asserts if you call it without having a Thread setup. GCX_PREEMP becomes a NOP.
// This is because all unmanaged threads are effectively preemp.)
//
// (There is actually one more case here - an "EE worker thread" such as the debugger
// thread or GC thread, which we don't want to call SetupThread() on, but which is
// effectively in cooperative mode due to explicit cooperation with the collector.
// This case is not handled by these macros; the current working assumption is that
// such threads never use them. But at some point we may have to consider
// this case if there is utility code which is called from those threads.)
//
// GCX_MAYBE_*(BOOL);       Same as above, but only do the switch if BOOL is TRUE.
//
// GCX_ASSERT_*();          Same as above, but assert mode rather than switch to mode.
//                          Note that assert is applied during backout as well.
//                          No overhead in a free build.
//
// GCX_FORBID();            Add "ForbidGC" semantics to a cooperative mode situation.
//                          Asserts that the thread will not trigger a GC or
//                          reach a GC-safe point, or call anything that might
//                          do one of these things.
//
// GCX_NOTRIGGER();         "ForbidGC" without the automatic assertion for coop mode.
//
// --------------------------------------------------------------------------------

template<BOOL COOPERATIVE>
class AutoCleanupGCAssert;

template<BOOL COOPERATIVE>
class GCAssert;


typedef AutoCleanupGCAssert<TRUE>                   AutoCleanupGCAssertCoop;
typedef AutoCleanupGCAssert<FALSE>                  AutoCleanupGCAssertPreemp;

typedef GCAssert<TRUE>                  GCAssertCoop;
typedef GCAssert<FALSE>                 GCAssertPreemp;

#if !defined(DACCESS_COMPILE)

#ifdef ENABLE_CONTRACTS_IMPL
#define GCX_COOP()                      GCCoop __gcHolder("GCX_COOP", __FUNCTION__, __FILE__, __LINE__)
#define GCX_COOP_NO_DTOR()              GCCoopNoDtor __gcHolder; __gcHolder.Enter(TRUE, "GCX_COOP_NO_DTOR", __FUNCTION__, __FILE__, __LINE__)
#define GCX_COOP_NO_DTOR_END()          __gcHolder.Leave();
#else
#define GCX_COOP()                      GCCoop __gcHolder
#define GCX_COOP_NO_DTOR()              GCCoopNoDtor __gcHolder; __gcHolder.Enter(TRUE)
#define GCX_COOP_NO_DTOR_END()          __gcHolder.Leave();
#endif

#ifdef ENABLE_CONTRACTS_IMPL
#define GCX_PREEMP()                                    GCPreemp __gcHolder("GCX_PREEMP", __FUNCTION__, __FILE__, __LINE__)
#define GCX_PREEMP_NO_DTOR()                            GCPreempNoDtor __gcHolder; __gcHolder.Enter(TRUE, "GCX_PREEMP_NO_DTOR", __FUNCTION__, __FILE__, __LINE__)
#define GCX_PREEMP_NO_DTOR_HAVE_THREAD(curThreadNullOk) GCPreempNoDtor __gcHolder; __gcHolder.Enter(curThreadNullOk, TRUE, "GCX_PREEMP_NO_DTOR_HAVE_THREAD", __FUNCTION__, __FILE__, __LINE__)
#define GCX_PREEMP_NO_DTOR_END()                        __gcHolder.Leave();
#else
#define GCX_PREEMP()                                    GCPreemp __gcHolder
#define GCX_PREEMP_NO_DTOR_HAVE_THREAD(curThreadNullOk) GCPreempNoDtor __gcHolder; __gcHolder.Enter(curThreadNullOk, TRUE)
#define GCX_PREEMP_NO_DTOR()                            GCPreempNoDtor __gcHolder; __gcHolder.Enter(TRUE)
#define GCX_PREEMP_NO_DTOR_END()                        __gcHolder.Leave()
#endif

#ifdef ENABLE_CONTRACTS_IMPL
#define GCX_COOP_THREAD_EXISTS(curThread)   GCCoopThreadExists __gcHolder((curThread), "GCX_COOP_THREAD_EXISTS",  __FUNCTION__, __FILE__, __LINE__)
#else
#define GCX_COOP_THREAD_EXISTS(curThread)   GCCoopThreadExists __gcHolder((curThread))
#endif

#ifdef ENABLE_CONTRACTS_IMPL
#define GCX_PREEMP_THREAD_EXISTS(curThread) GCPreempThreadExists __gcHolder((curThread), "GCX_PREEMP_THREAD_EXISTS",  __FUNCTION__, __FILE__, __LINE__)
#else
#define GCX_PREEMP_THREAD_EXISTS(curThread) GCPreempThreadExists __gcHolder((curThread))
#endif

#ifdef ENABLE_CONTRACTS_IMPL
#define GCX_MAYBE_COOP(_cond)                             GCCoop __gcHolder(_cond, "GCX_MAYBE_COOP",  __FUNCTION__, __FILE__, __LINE__)
#define GCX_MAYBE_COOP_NO_DTOR(_cond)   GCCoopNoDtor __gcHolder; __gcHolder.Enter(_cond, "GCX_MAYBE_COOP_NO_DTOR", __FUNCTION__, __FILE__, __LINE__)
#define GCX_MAYBE_COOP_NO_DTOR_END()    __gcHolder.Leave();
#else
#define GCX_MAYBE_COOP(_cond)                             GCCoop __gcHolder(_cond)
#define GCX_MAYBE_COOP_NO_DTOR(_cond)   GCCoopNoDtor __gcHolder; __gcHolder.Enter(_cond)
#define GCX_MAYBE_COOP_NO_DTOR_END()    __gcHolder.Leave();
#endif

#ifdef ENABLE_CONTRACTS_IMPL
#define GCX_MAYBE_PREEMP(_cond)                           GCPreemp __gcHolder(_cond, "GCX_MAYBE_PREEMP",  __FUNCTION__, __FILE__, __LINE__)
#define GCX_MAYBE_PREEMP_NO_DTOR(_cond) GCPreempNoDtor __gcHolder; __gcHolder.Enter(_cond, "GCX_MAYBE_PREEMP_NO_DTOR", __FUNCTION__, __FILE__, __LINE__)
#define GCX_MAYBE_PREEMP_NO_DTOR_END()  __gcHolder.Leave();
#else
#define GCX_MAYBE_PREEMP(_cond)                           GCPreemp __gcHolder(_cond)
#define GCX_MAYBE_PREEMP_NO_DTOR(_cond) GCPreempNoDtor __gcHolder; __gcHolder.Enter(_cond)
#define GCX_MAYBE_PREEMP_NO_DTOR_END()  __gcHolder.Leave()
#endif

#ifdef ENABLE_CONTRACTS_IMPL
#define GCX_MAYBE_COOP_THREAD_EXISTS(curThread, _cond)    GCCoopThreadExists __gcHolder((curThread), (_cond), "GCX_MAYBE_COOP_THREAD_EXISTS",  __FUNCTION__, __FILE__, __LINE__)
#else
#define GCX_MAYBE_COOP_THREAD_EXISTS(curThread, _cond)    GCCoopThreadExists __gcHolder((curThread), (_cond))
#endif

#ifdef ENABLE_CONTRACTS_IMPL
#define GCX_MAYBE_PREEMP_THREAD_EXISTS(curThread, _cond)  GCPreempThreadExists __gcHolder((curThread), (_cond), "GCX_MAYBE_PREEMP_THREAD_EXISTS",  __FUNCTION__, __FILE__, __LINE__)
#else
#define GCX_MAYBE_PREEMP_THREAD_EXISTS(curThread, _cond)  GCPreempThreadExists __gcHolder((curThread), (_cond))
#endif

// This has a potential race with the GC thread.  It is currently
// used for a few cases where (a) we potentially haven't started up the EE yet, or
// (b) we are on a "special thread".
#ifdef ENABLE_CONTRACTS_IMPL
#define GCX_COOP_NO_THREAD_BROKEN()                 GCCoopHackNoThread __gcHolder("GCX_COOP_NO_THREAD_BROKEN",  __FUNCTION__, __FILE__, __LINE__)
#else
#define GCX_COOP_NO_THREAD_BROKEN()                 GCCoopHackNoThread __gcHolder
#endif


#ifdef ENABLE_CONTRACTS_IMPL
#define GCX_MAYBE_COOP_NO_THREAD_BROKEN(_cond)      GCCoopHackNoThread __gcHolder(_cond, "GCX_MAYBE_COOP_NO_THREAD_BROKEN",  __FUNCTION__, __FILE__, __LINE__)
#else
#define GCX_MAYBE_COOP_NO_THREAD_BROKEN(_cond)      GCCoopHackNoThread __gcHolder(_cond)
#endif

#else // !defined(DACCESS_COMPILE)

#define GCX_COOP()
#define GCX_COOP_NO_DTOR()
#define GCX_COOP_NO_DTOR_END()

#define GCX_PREEMP()
#define GCX_PREEMP_NO_DTOR()
#define GCX_PREEMP_NO_DTOR_HAVE_THREAD(curThreadNullOk)
#define GCX_PREEMP_NO_DTOR_END()

#define GCX_MAYBE_PREEMP(_cond)

#define GCX_COOP_NO_THREAD_BROKEN()
#define GCX_MAYBE_COOP_NO_THREAD_BROKEN(_cond)

#define GCX_PREEMP_THREAD_EXISTS(curThread)
#define GCX_COOP_THREAD_EXISTS(curThread)

#define GCX_POP()

#endif // !defined(DACCESS_COMPILE)

#if defined(_DEBUG_IMPL)

#define GCX_ASSERT_PREEMP()                 ::AutoCleanupGCAssertPreemp __gcHolder
#define GCX_ASSERT_COOP()                   ::AutoCleanupGCAssertCoop __gcHolder

#define BEGIN_GCX_ASSERT_COOP                                   \
    {                                                           \
        GCAssertCoop    __gcHolder;                             \
        __gcHolder.BeginGCAssert()

#define END_GCX_ASSERT_COOP                                     \
        __gcHolder.EndGCAssert();                               \
    }

#define BEGIN_GCX_ASSERT_PREEMP                                 \
    {                                                           \
        GCAssertPreemp  __gcHolder;                             \
        __gcHolder.BeginGCAssert()

#define END_GCX_ASSERT_PREEMP                                   \
        __gcHolder.EndGCAssert();                               \
    }


#else

#define GCX_ASSERT_PREEMP()
#define GCX_ASSERT_COOP()

#define BEGIN_GCX_ASSERT_COOP                                   \
    {
#define END_GCX_ASSERT_COOP                                     \
    }
#define BEGIN_GCX_ASSERT_PREEMP                                 \
    {
#define END_GCX_ASSERT_PREEMP                                   \
    }

#endif

#ifdef ENABLE_CONTRACTS_IMPL

#define GCX_FORBID()                        ::GCForbid __gcForbidHolder(__FUNCTION__, __FILE__, __LINE__)
#define GCX_NOTRIGGER()                     ::GCNoTrigger __gcNoTriggerHolder(__FUNCTION__, __FILE__, __LINE__)

#define GCX_MAYBE_FORBID(fConditional)      ::GCForbid __gcForbidHolder(fConditional, __FUNCTION__, __FILE__, __LINE__)
#define GCX_MAYBE_NOTRIGGER(fConditional)   ::GCNoTrigger __gcNoTriggerHolder(fConditional, __FUNCTION__, __FILE__, __LINE__)

#else


#define GCX_FORBID()
#define GCX_NOTRIGGER()

#define GCX_MAYBE_FORBID(fConditional)
#define GCX_MAYBE_NOTRIGGER(fConditional)

#endif

typedef BOOL (*FnLockOwner)(LPVOID);
struct LockOwner
{
    LPVOID lock;
    FnLockOwner lockOwnerFunc;
};

// this is the standard lockowner for things that require a lock owner but which really don't
// need any validation due to their simple/safe semantics
// the classic example of this is a hash table that is initialized and then never grows
extern LockOwner g_lockTrustMeIAmThreadSafe;

// The OS ThreadId is not a stable ID for a thread we a host uses fiber instead of Thread.
// For each managed Thread, we have a stable and unique id in Thread object.  For other threads,
// e.g. Server GC or Concurrent GC thread, debugger helper thread, we do not have a Thread object,
// and we use OS ThreadId to identify them since they are not managed by a host.
class EEThreadId
{
private:
    void *m_FiberPtrId;
public:
#ifdef _DEBUG
    EEThreadId()
    : m_FiberPtrId(NULL)
    {
        LIMITED_METHOD_CONTRACT;
    }
#endif

    void SetToCurrentThread()
    {
        WRAPPER_NO_CONTRACT;

        m_FiberPtrId = ClrTeb::GetFiberPtrId();
    }

    bool IsCurrentThread() const
    {
        WRAPPER_NO_CONTRACT;

        return (m_FiberPtrId == ClrTeb::GetFiberPtrId());
    }


#ifdef _DEBUG
    bool IsUnknown() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_FiberPtrId == NULL;
    }
#endif
    void Clear()
    {
        LIMITED_METHOD_CONTRACT;
        m_FiberPtrId = NULL;
    }
};

#ifndef TARGET_UNIX
HMODULE CLRLoadLibraryEx(LPCWSTR lpLibFileName, HANDLE hFile, DWORD dwFlags);
#endif // !TARGET_UNIX

HMODULE CLRLoadLibrary(LPCWSTR lpLibFileName);

BOOL CLRFreeLibrary(HMODULE hModule);

LPVOID
CLRMapViewOfFile(
    IN HANDLE hFileMappingObject,
    IN DWORD dwDesiredAccess,
    IN DWORD dwFileOffsetHigh,
    IN DWORD dwFileOffsetLow,
    IN SIZE_T dwNumberOfBytesToMap,
    IN LPVOID lpBaseAddress = NULL);

BOOL
CLRUnmapViewOfFile(
    IN LPVOID lpBaseAddress
    );

#ifndef DACCESS_COMPILE
FORCEINLINE void VoidCLRUnmapViewOfFile(void *ptr) { CLRUnmapViewOfFile(ptr); }
typedef Wrapper<void *, DoNothing, VoidCLRUnmapViewOfFile> CLRMapViewHolder;
#else
typedef Wrapper<void *, DoNothing, DoNothing> CLRMapViewHolder;
#endif

#ifdef TARGET_UNIX
#ifndef DACCESS_COMPILE
FORCEINLINE void VoidPALUnloadPEFile(void *ptr) { PAL_LOADUnloadPEFile(ptr); }
typedef Wrapper<void *, DoNothing, VoidPALUnloadPEFile> PALPEFileHolder;
#else
typedef Wrapper<void *, DoNothing, DoNothing> PALPEFileHolder;
#endif
#endif // TARGET_UNIX

#define SetupThreadForComCall(OOMRetVal)            \
    MAKE_CURRENT_THREAD_AVAILABLE_EX(GetThreadNULLOk()); \
    if (CURRENT_THREAD == NULL)                     \
    {                                               \
        CURRENT_THREAD = SetupThreadNoThrow();      \
        if (CURRENT_THREAD == NULL)                 \
            return OOMRetVal;                       \
    }                                               \

#define SetupForComCallHR() SetupThreadForComCall(E_OUTOFMEMORY)
#define SetupForComCallDWORD() SetupThreadForComCall(ERROR_OUTOFMEMORY)

// A holder for NATIVE_LIBRARY_HANDLE.
FORCEINLINE void VoidFreeNativeLibrary(NATIVE_LIBRARY_HANDLE h)
{
    WRAPPER_NO_CONTRACT;

    if (h == NULL)
        return;

#ifdef HOST_UNIX
    PAL_FreeLibraryDirect(h);
#else
    FreeLibrary(h);
#endif
}

typedef Wrapper<NATIVE_LIBRARY_HANDLE, DoNothing<NATIVE_LIBRARY_HANDLE>, VoidFreeNativeLibrary, 0> NativeLibraryHandleHolder;

extern thread_local size_t t_CantStopCount;

// For debugging, we can track arbitrary Can't-Stop regions.
// In V1.0, this was on the Thread object, but we need to track this for threads w/o a Thread object.
FORCEINLINE void IncCantStopCount()
{
    t_CantStopCount++;
}

FORCEINLINE void DecCantStopCount()
{
    t_CantStopCount--;
}

typedef StateHolder<IncCantStopCount, DecCantStopCount> CantStopHolder;

#ifdef _DEBUG
// For debug-only, this can be used w/ a holder to ensure that we're keeping our CS count balanced.
// We should never use this w/ control flow.
inline size_t GetCantStopCount()
{
    return t_CantStopCount;
}

// At places where we know we're calling out to native code, we can assert that we're NOT in a CS region.
// This is _debug only since we only use it for asserts; not for real code-flow control in a retail build.
inline bool IsInCantStopRegion()
{
    return (GetCantStopCount() > 0);
}
#endif // _DEBUG

BOOL IsValidMethodCodeNotification(ULONG32 Notification);

typedef DPTR(struct JITNotification) PTR_JITNotification;
struct JITNotification
{
    USHORT state; // values from CLRDataMethodCodeNotification
    TADDR clrModule;
    mdToken methodToken;

    JITNotification() { SetFree(); }
    BOOL IsFree() { return state == CLRDATA_METHNOTIFY_NONE; }
    void SetFree() { state = CLRDATA_METHNOTIFY_NONE; clrModule = 0; methodToken = 0; }
    void SetState(TADDR moduleIn, mdToken tokenIn, USHORT NType)
    {
        _ASSERTE(IsValidMethodCodeNotification(NType));
        clrModule = moduleIn;
        methodToken = tokenIn;
        state = NType;
    }
};

// The maximum number of TADDR sized arguments that the SOS exception notification can use
#define MAX_CLR_NOTIFICATION_ARGS 3
GARY_DECL(size_t, g_clrNotificationArguments, MAX_CLR_NOTIFICATION_ARGS);
extern void InitializeClrNotifications();

GPTR_DECL(JITNotification, g_pNotificationTable);
GVAL_DECL(ULONG32, g_dacNotificationFlags);

#if defined(TARGET_UNIX) && !defined(DACCESS_COMPILE)

inline void
InitializeJITNotificationTable()
{
    g_pNotificationTable = new (nothrow) JITNotification[1001];
}

#endif // TARGET_UNIX && !DACCESS_COMPILE

class JITNotifications
{
public:
    JITNotifications(JITNotification *jitTable);
    BOOL SetNotification(TADDR clrModule, mdToken token, USHORT NType);
    USHORT Requested(TADDR clrModule, mdToken token);

    // if clrModule is NULL, all active notifications are changed to NType
    BOOL SetAllNotifications(TADDR clrModule,USHORT NType,BOOL *changedOut);
    inline BOOL IsActive() { LIMITED_METHOD_CONTRACT; return m_jitTable!=NULL; }

    UINT GetTableSize();
#ifdef DACCESS_COMPILE
    static JITNotification *InitializeNotificationTable(UINT TableSize);
    // Updates target table from host copy
    BOOL UpdateOutOfProcTable();
#endif

private:
    UINT GetLength();
    void IncrementLength();
    void DecrementLength();

    BOOL FindItem(TADDR clrModule, mdToken token, UINT *indexOut);

    JITNotification *m_jitTable;
};

typedef DPTR(struct GcNotification) PTR_GcNotification;

inline
BOOL IsValidGcNotification(GcEvt_t evType)
{ return (evType < GC_EVENT_TYPE_MAX); }

#define CLRDATA_GC_NONE  0

struct GcNotification
{
    GcEvtArgs ev;

    GcNotification() { SetFree(); }
    BOOL IsFree() { return ev.typ == CLRDATA_GC_NONE; }
    void SetFree() { memset(this, 0, sizeof(*this)); ev.typ = (GcEvt_t) CLRDATA_GC_NONE; }
    void Set(GcEvtArgs ev_)
    {
        _ASSERTE(IsValidGcNotification(ev_.typ));
        ev = ev_;
    }
    BOOL IsMatch(GcEvtArgs ev_)
    {
        LIMITED_METHOD_CONTRACT;
        if (ev.typ != ev_.typ)
        {
            return FALSE;
        }
        switch (ev.typ)
        {
        case GC_MARK_END:
            if (ev_.condemnedGeneration == 0 ||
                (ev.condemnedGeneration & ev_.condemnedGeneration) != 0)
            {
                return TRUE;
            }
            break;
        default:
            break;
        }

        return FALSE;
    }
};

GPTR_DECL(GcNotification, g_pGcNotificationTable);

class GcNotifications
{
public:
    GcNotifications(GcNotification *gcTable);
    BOOL SetNotification(GcEvtArgs ev);
    GcEvtArgs* GetNotification(GcEvtArgs ev)
    {
        LIMITED_METHOD_CONTRACT;
        UINT idx;
        if (FindItem(ev, &idx))
        {
            return &m_gcTable[idx].ev;
        }
        else
        {
            return NULL;
        }
    }

    // if clrModule is NULL, all active notifications are changed to NType
    inline BOOL IsActive()
    { return m_gcTable != NULL; }

    UINT GetTableSize()
    { return Size(); }

#ifdef DACCESS_COMPILE
    static GcNotification *InitializeNotificationTable(UINT TableSize);
    // Updates target table from host copy
    BOOL UpdateOutOfProcTable();
#endif

private:
    UINT& Length()
    {
        LIMITED_METHOD_CONTRACT;
        _ASSERTE(IsActive());
        UINT *pLen = (UINT *) &(m_gcTable[-1].ev.typ);
        return *pLen;
    }
    UINT& Size()
    {
        _ASSERTE(IsActive());
        UINT *pLen = (UINT *) &(m_gcTable[-1].ev.typ);
        return *(pLen+1);
    }
    void IncrementLength()
    { ++Length(); }
    void DecrementLength()
    { --Length(); }

    BOOL FindItem(GcEvtArgs ev, UINT *indexOut);

    GcNotification *m_gcTable;
};


class MethodDesc;
class Module;
class ModuleBase;

class DACNotify
{
public:
    // types
    enum {
        MODULE_LOAD_NOTIFICATION=1,
        MODULE_UNLOAD_NOTIFICATION=2,
        JIT_NOTIFICATION=3,
        __UNUSED__=4,
        EXCEPTION_NOTIFICATION=5,
        GC_NOTIFICATION= 6,
        CATCH_ENTER_NOTIFICATION = 7,
        JIT_NOTIFICATION2=8,
    };

    // called from the runtime
    static void DoJITNotification(MethodDesc *MethodDescPtr, TADDR NativeCodeLocation);
    static void DoModuleLoadNotification(Module *Module);
    static void DoModuleUnloadNotification(Module *Module);
    static void DoExceptionNotification(class Thread* ThreadPtr);
    static void DoGCNotification(const GcEvtArgs& evtargs);
    static void DoExceptionCatcherEnterNotification(MethodDesc *MethodDescPtr, DWORD nativeOffset);

    // called from the DAC
    static int GetType(TADDR Args[]);
    static BOOL ParseJITNotification(TADDR Args[], TADDR& MethodDescPtr, TADDR& NativeCodeLocation);
    static BOOL ParseModuleLoadNotification(TADDR Args[], TADDR& ModulePtr);
    static BOOL ParseModuleUnloadNotification(TADDR Args[], TADDR& ModulePtr);
    static BOOL ParseExceptionNotification(TADDR Args[], TADDR& ThreadPtr);
    static BOOL ParseGCNotification(TADDR Args[], GcEvtArgs& evtargs);
    static BOOL ParseExceptionCatcherEnterNotification(TADDR Args[], TADDR& MethodDescPtr, DWORD& nativeOffset);
};

void DACNotifyCompilationFinished(MethodDesc *pMethodDesc, PCODE pCode);

// These wrap the SString:L:CompareCaseInsensitive function in a way that makes it
// easy to fix code that uses _stricmp. _stricmp should be avoided as it uses the current
// C-runtime locale rather than the invariance culture.
//
// Note that unlike the real _stricmp, these functions unavoidably have a throws/gc_triggers/inject_fault
// contract. So if need a case-insensitive comparison in a place where you can't tolerate this contract,
// you've got a problem.
int __cdecl stricmpUTF8(const char* szStr1, const char* szStr2);

BOOL DbgIsExecutable(LPVOID lpMem, SIZE_T length);

int GetRandomInt(int maxVal);

// ======================================================================================
// Simple, reusable 100ns timer for normalizing ticks. For use in Q/FCalls to avoid discrepency with
// tick frequency between native and managed.
class NormalizedTimer
{
private:
    static const int64_t NormalizedTicksPerSecond = 10000000 /* 100ns ticks per second (1e7) */;
    static Volatile<double> s_frequency;

    int64_t startTimestamp;
    int64_t stopTimestamp;

#if _DEBUG
    bool isRunning = false;
#endif // _DEBUG

public:
    NormalizedTimer()
    {
        LIMITED_METHOD_CONTRACT;
        if (s_frequency.Load() == -1)
        {
            double frequency;
            int64_t qpfValue = minipal_hires_tick_frequency();
            frequency = static_cast<double>(qpfValue);
            frequency /= NormalizedTicksPerSecond;
            s_frequency.Store(frequency);
        }

        startTimestamp = 0;
        stopTimestamp = 0;
    }

    // ======================================================================================
    // Start the timer
    inline
    void Start()
    {
        LIMITED_METHOD_CONTRACT;
        _ASSERTE(!isRunning);
        startTimestamp = minipal_hires_ticks();

#if _DEBUG
        isRunning = true;
#endif // _DEBUG
    }

    // ======================================================================================
    // stop the timer. If called before starting, sets the start time to the same as the stop
    inline
    void Stop()
    {
        LIMITED_METHOD_CONTRACT;
        _ASSERTE(isRunning);
        stopTimestamp = minipal_hires_ticks();

#if _DEBUG
        isRunning = false;
#endif // _DEBUG
    }

    // ======================================================================================
    // Return elapsed ticks. This will stop a running timer.
    // Will return 0 if called out of order.
    // Only recalculated this value if it has been stopped/started since previous calculation.
    inline
    int64_t Elapsed100nsTicks()
    {
        LIMITED_METHOD_CONTRACT;
        _ASSERTE(!isRunning);
        _ASSERTE(startTimestamp > 0);
        _ASSERTE(stopTimestamp > 0);
        return static_cast<int64_t>((stopTimestamp - startTimestamp) / s_frequency);
    }
};

#ifdef _DEBUG
#define FORCEINLINE_NONDEBUG
#else
#define FORCEINLINE_NONDEBUG FORCEINLINE
#endif

#ifndef TARGET_UNIX
// Extract the file version from an executable.
HRESULT GetFileVersion(LPCWSTR wszFilePath, ULARGE_INTEGER* pFileVersion);
#endif // !TARGET_UNIX

#define ENUM_PAGE_SIZES \
    ENUM_PAGE_SIZE(16384) \
    ENUM_PAGE_SIZE(32768) \
    ENUM_PAGE_SIZE(65536)

void FillStubCodePage(BYTE* pageBase, const void* code, SIZE_T codeSize, SIZE_T pageSize);

#ifdef TARGET_64BIT
// We use modified Daniel Lemire's fastmod algorithm (https://github.com/dotnet/runtime/pull/406),
// which allows to avoid the long multiplication if the divisor is less than 2**31.
// This is a copy of HashHelpers.cs, see that impl (or linked PR) for more details
inline UINT64 GetFastModMultiplier(UINT32 divisor)
{
    return UINT64_MAX / divisor + 1;
}

inline UINT32 FastMod(UINT32 value, UINT32 divisor, UINT64 multiplier)
{
    _ASSERTE(divisor <= INT_MAX);
    UINT32 highbits = (UINT32)(((((multiplier * value) >> 32) + 1) * divisor) >> 32);
    _ASSERTE(highbits == value % divisor);
    return highbits;
}
#endif

#endif /* _H_UTIL */
