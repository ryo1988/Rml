#include "stdafx.h"

#include "..\include\Rml.RedirectLoadAssemblyFolder.Clr.h"

using namespace System;
using namespace System::Threading;
using namespace System::Configuration;
using namespace System::Xml;
using namespace System::Reflection;
using namespace System::IO;
using namespace System::Diagnostics;
using namespace System::Globalization;
using namespace System::Collections::Generic;

private ref class RedirectLoadAssemblyFolderHandler
{
public:
    RedirectLoadAssemblyFolderHandler(String^ folderPath, AppDomain^ appDomain)
    {
        _folderPath = folderPath;
        _appDomain = appDomain;
        auto files = Directory::GetFiles(folderPath, "*.dll", SearchOption::AllDirectories);
        _haveAssembly = gcnew HashSet<String^>(files);
    }

    static Assembly^ LoadAssembly(HashSet<String^>^ haveAssembly, String^ folderPath, AppDomain^ appDomain, String^ name)
    {
        auto assemblyName = gcnew AssemblyName(name);
        auto path = Path::Combine(folderPath, assemblyName->Name + ".dll");
        if (haveAssembly->Contains(path) == false)
        {
            return nullptr;
        }

        try
        {
            for each(auto assembly in appDomain->GetAssemblies())
            {
                if (assembly->FullName == name)
                {
                    return assembly;
                }
            }
            {
                auto assembly = Assembly::LoadFile(path);
                return assembly;
            }
        }
        catch (Exception^)
        {
            // ignored
        }

        return nullptr;
    }

    Assembly^ Handler(System::Object^ sender, ResolveEventArgs^ args)
    {
        return LoadAssembly(_haveAssembly, _folderPath, _appDomain, args->Name);
    }

    static void AttachHandler(String^ folderPath, AppDomain^ appDomain)
    {
        auto handler = gcnew RedirectLoadAssemblyFolderHandler(folderPath, appDomain);
        appDomain->AssemblyResolve += gcnew ResolveEventHandler(handler, &RedirectLoadAssemblyFolderHandler::Handler);
    }

private:
    String^ _folderPath;
    HashSet<String^>^ _haveAssembly;
    AppDomain^ _appDomain;
};

void Rml::RedirectLoadAssemblyFolder::Redirect(const char * folderPath)
{
    RedirectLoadAssemblyFolderHandler::AttachHandler(gcnew String(folderPath), AppDomain::CurrentDomain);
}

void Rml::RedirectLoadAssemblyFolder::RedirectExecutingAssemblyFolder()
{
    auto assembly = Assembly::GetExecutingAssembly();
    auto folderPath = Path::GetDirectoryName(assembly->Location);
    RedirectLoadAssemblyFolderHandler::AttachHandler(folderPath, AppDomain::CurrentDomain);
}
