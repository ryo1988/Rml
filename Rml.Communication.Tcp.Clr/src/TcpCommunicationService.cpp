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

gcroot<Tcp::TcpCommunicationService^>* CreateInstance()
{
    auto instance = gcnew Tcp::TcpCommunicationService();
    return new gcroot<Tcp::TcpCommunicationService^>(instance);
}

class TcpCommunicationServiceImpl
{
    friend class TcpCommunicationService;

public:
    TcpCommunicationServiceImpl()
        : _instance(CreateInstance())
        , _base(new Rml::Communication::Clr::CommunicationServiceBase(_instance))
    {
    }

    ~TcpCommunicationServiceImpl()
    {
        delete _base;
        delete _instance;
    }

private:
    gcroot<Tcp::TcpCommunicationService^>* _instance;
    Rml::Communication::Clr::CommunicationServiceBase* _base;
};



TcpCommunicationService::TcpCommunicationService()
    : _impl(new TcpCommunicationServiceImpl())
{
}

TcpCommunicationService::~TcpCommunicationService()
{
    delete _impl;
}

bool TcpCommunicationService::Send(void* buffer, int size)
{
    return _impl->_base->Send(buffer, size);
}

void TcpCommunicationService::SetReceiveCallback(Communication::Clr::IReceiveCallback* recvCallback)
{
    _impl->_base->SetReceiveCallback(recvCallback);
}

int TcpCommunicationService::GetConnectCount()
{
    return _impl->_base->GetConnectCount();
}

void* TcpCommunicationService::GetInstance()
{
    return _impl->_base->GetInstance();
}

bool TcpCommunicationService::StartListener(const char * ipAddress, int port)
{
    return (*_impl->_instance)->StartListener(gcnew String(ipAddress), port);
}

bool TcpCommunicationService::StartClient(const char * hostName, int port)
{
    return (*_impl->_instance)->StartClient(gcnew String(hostName), port);
}

}
}
}
}