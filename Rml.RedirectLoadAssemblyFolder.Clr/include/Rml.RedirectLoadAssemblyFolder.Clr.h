#pragma once

#ifdef RMLREDIRECTLOADASSEMBLY_DLL_EXPORTS
#define RMLREDIRECTLOADASSEMBLY_DLL_API __declspec(dllexport) 
#else
#define RMLREDIRECTLOADASSEMBLY_DLL_API __declspec(dllimport) 
#endif

namespace Rml
{
class RMLREDIRECTLOADASSEMBLY_DLL_API RedirectLoadAssemblyFolder
{
public:
    static void Redirect(const char* folderPath);
    static void RedirectExecutingAssemblyFolder();
};
}
