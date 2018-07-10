#include "stdafx.h"
#include "..\include\DirectCommunicationService.h"

#include <vcclr.h>

using namespace System;  
using namespace System::Runtime::InteropServices;

namespace Rml
{
namespace Communication
{
namespace Direct
{
namespace Clr
{

gcroot<Direct::DirectCommunicationService^>* CreateInstance()
{
    auto instance = gcnew Direct::DirectCommunicationService();
    return new gcroot<Direct::DirectCommunicationService^>(instance);
}

class DirectCommunicationServiceImpl
{
    friend class DirectCommunicationService;

public:
    DirectCommunicationServiceImpl()
        : _instance(CreateInstance())
        , _base(new Rml::Communication::Clr::CommunicationServiceBase(_instance))
    {
    }

    ~DirectCommunicationServiceImpl()
    {
        delete _base;
        delete _instance;
    }

private:
    gcroot<Direct::DirectCommunicationService^>* _instance;
    Rml::Communication::Clr::CommunicationServiceBase* _base;
};

DirectCommunicationService::DirectCommunicationService()
    : _impl(new DirectCommunicationServiceImpl())
{
}

DirectCommunicationService::~DirectCommunicationService()
{
    delete _impl;
}

bool DirectCommunicationService::Send(void* buffer, int size)
{
    return _impl->_base->Send(buffer, size);
}

void DirectCommunicationService::SetReceiveCallback(Communication::Clr::IReceiveCallback* receiveCallback)
{
    _impl->_base->SetReceiveCallback(receiveCallback);
}

void DirectCommunicationService::SetLogedCallback(Communication::Clr::ILogedCallback* logedCallback)
{
    _impl->_base->SetLogedCallback(logedCallback);
}

int DirectCommunicationService::GetConnectCount()
{
    return _impl->_base->GetConnectCount();
}

void* DirectCommunicationService::GetInstance()
{
    return _impl->_base->GetInstance();
}

void DirectCommunicationService::SetTarget(DirectCommunicationService* target)
{
    (*_impl->_instance)->SetTarget(*target->_impl->_instance);
}

}
}
}
}