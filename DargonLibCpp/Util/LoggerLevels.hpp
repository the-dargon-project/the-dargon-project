/// <summary>
/// Verbose Logger Level - for things that happen every single frame
/// </summary>
#define LL_VERBOSE  0x00000001UL

/// <summary>
/// Info Logger Level - for things that are guaranteed to happen,
/// but which occur repeatedly (ex: texture loads).
/// </summary>
#define LL_INFO     0x00000002UL

/// <summary>
/// Notice Logger Level - for things that are guaranteed to happen,
/// but which occur once.  (ex: Connect to Dargon Manager, Init D3D)
/// </summary>
#define LL_NOTICE   0x00000004UL

/// <summary>
/// Warning Logger Level - for things that are a bit strange,
/// but not potentially fatal.  Ex: Unable to find champion portrait
/// texture.  We can recover from warnings, usually.
/// </summary>
#define LL_WARN     0x00000008UL

/// <summary>
/// Error Logger Level - for things that are going to be fatal to Dargon
/// Ex: Dargon is injected to wrong process, Dargon can't read config files, etc.
/// </summary>
#define LL_ERROR    0x00000010UL

/// <summary>
/// Always Logger level - useful for when you're writing new features.
/// </summary>
#define LL_ALWAYS   0x01000000UL