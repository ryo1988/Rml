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

private ref class RedirectLoadAssemblyFolderHandler
{
public:
    RedirectLoadAssemblyFolderHandler(String^ folderPath)
    {
        _folderPath = folderPath;
    }

    Assembly^ Handler(System::Object^ sender, ResolveEventArgs^ args)
    {
        auto info = args->Name->Split(',');
        auto path = Path::Combine(_folderPath, info[0] + ".dll");

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
