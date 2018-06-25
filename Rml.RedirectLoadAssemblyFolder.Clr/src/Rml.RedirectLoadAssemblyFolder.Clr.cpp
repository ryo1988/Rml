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
    RedirectLoadAssemblyFolderHandler(String^ folderPath)
    {
        _folderPath = folderPath;
        auto files = Directory::GetFiles(folderPath, "*.dll", SearchOption::AllDirectories);
        _haveAssembly = gcnew HashSet<String^>(files);
    }

    Assembly^ Handler(System::Object^ sender, ResolveEventArgs^ args)
    {
        auto assemblyName = gcnew AssemblyName(args->Name);
        auto path = Path::Combine(_folderPath, assemblyName->Name + ".dll");
        if (_haveAssembly->Contains(path) == false)
        {
            return nullptr;
        }

        try
        {
            auto assembly = Assembly::LoadFile(path);
            return assembly;
        }
        catch (Exception^)
        {
            // ignored
        }

        return nullptr;
    }

    static void AttachHandler(String^ folderPath)
    {
        auto handler = gcnew RedirectLoadAssemblyFolderHandler(folderPath);
        AppDomain::CurrentDomain->AssemblyResolve += gcnew ResolveEventHandler(handler, &RedirectLoadAssemblyFolderHandler::Handler);
    }

private:
    String^ _folderPath;
    HashSet<String^>^ _haveAssembly;
};

void Rml::RedirectLoadAssemblyFolder::Redirect(const char * folderPath)
{
    RedirectLoadAssemblyFolderHandler::AttachHandler(gcnew String(folderPath));
}

void Rml::RedirectLoadAssemblyFolder::RedirectExecutingAssemblyFolder()
{
    auto assembly = Assembly::GetExecutingAssembly();
    auto folderPath = Path::GetDirectoryName(assembly->Location);
    RedirectLoadAssemblyFolderHandler::AttachHandler(folderPath);
}
