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

void* CreateInstance()
{
    auto instance = gcnew Direct::DirectCommunicationService();
    return new gcroot<Direct::DirectCommunicationService^>(instance);
}

DirectCommunicationService::DirectCommunicationService()
    : _base(CreateInstance())
{
}

DirectCommunicationService::~DirectCommunicationService()
{
}

bool DirectCommunicationService::Send(void* buffer, int size)
{
    return _base.Send(buffer, size);
}

void DirectCommunicationService::SetReceiveCallback(Communication::Clr::ReceiveCallback receiveCallback)
{
    _base.SetReceiveCallback(receiveCallback);
}

void DirectCommunicationService::SetReceiveCallback(std::function<void(void*,int)> receiveCallback)
{
    _base.SetReceiveCallback(receiveCallback);
}

int DirectCommunicationService::GetConnectCount()
{
    return _base.GetConnectCount();
}

void* DirectCommunicationService::GetInstance()
{
    return _base.GetInstance();
}

void DirectCommunicationService::SetTarget(DirectCommunicationService* target)
{
    auto instancePtr = static_cast<gcroot<Direct::DirectCommunicationService^>*>(_base.GetInstance());
    auto targetInstancePtr = static_cast<gcroot<Direct::DirectCommunicationService^>*>(target->_base.GetInstance());
    (*instancePtr)->SetTarget(*targetInstancePtr);
}

}
}
}
}