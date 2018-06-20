#include "stdafx.h"
#include "..\include\TcpCommunicationService.h"

#include <vcclr.h>

using namespace System;  
using namespace System::Runtime::InteropServices;

namespace Rml
{
namespace Communication
{
namespace Tcp
{
namespace Clr
{

void* CreateInstance()
{
    auto instance = gcnew Tcp::TcpCommunicationService();
    return new gcroot<Tcp::TcpCommunicationService^>(instance);
}

TcpCommunicationService::TcpCommunicationService()
    : _base(CreateInstance())
{
}

TcpCommunicationService::~TcpCommunicationService()
{
}

bool TcpCommunicationService::Send(void* buffer, int size)
{
    return _base.Send(buffer, size);
}

void TcpCommunicationService::SetReceiveCallback(Communication::Clr::ReceiveCallback recvCallback)
{
    _base.SetReceiveCallback(recvCallback);
}

void TcpCommunicationService::SetReceiveCallback(std::function<void(void*,int)> recvCallback)
{
    _base.SetReceiveCallback(recvCallback);
}

int TcpCommunicationService::GetConnectCount()
{
    return _base.GetConnectCount();
}

void* TcpCommunicationService::GetInstance()
{
    return _base.GetInstance();
}

bool TcpCommunicationService::StartListener(const char * ipAddress, int port)
{
    auto instancePtr = static_cast<gcroot<Tcp::TcpCommunicationService^>*>(GetInstance());

    return (*instancePtr)->StartListener(gcnew String(ipAddress), port);
}

bool TcpCommunicationService::StartClient(const char * hostName, int port)
{
    auto instancePtr = static_cast<gcroot<Tcp::TcpCommunicationService^>*>(GetInstance());

    return (*instancePtr)->StartClient(gcnew String(hostName), port);
}

}
}
}
}