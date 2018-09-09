using System.Reflection;

// The assembly version has following format :
//
// Major.Minor.Build.Revision
//
// You can specify all the values or you can use the default the Revision and
// Build Numbers by using the '*' as shown below:

[assembly: AssemblyVersion("1.5.0.0")]
#if (!CF)
[assembly: AssemblyFileVersion("1.5.0.0")]
#endif
