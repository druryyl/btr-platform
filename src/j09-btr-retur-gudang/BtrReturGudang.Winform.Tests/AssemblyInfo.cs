using Xunit;

// WinForms UI components, Windows Registry keys (HKCU), and static AudioHelper state
// must execute sequentially to prevent race conditions during test execution.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
