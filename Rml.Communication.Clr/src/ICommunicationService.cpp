#include "stdafx.h"
#include "..\include\ICommunicationService.h"

#include <vcclr.h>

using namespace System;  
using namespace System::Runtime::InteropServices;

namespace Rml
{
namespace Communication
{
namespace Clr
{

ICommunicationService::~ICommunicationService()
{
}

ref class ReceiveEventReceiver
{
public:
    void Hnadler(System::Object^ sender, ReceiveEventArgs^ args)
    {
        if (_receiveCallback == nullptr)
        {
            return;
        }

        pin_ptr<System::Byte> buffetPtr = &args->Buffer[0];
        unsigned char* ptr = buffetPtr;
        if (_receiveCallback)
        {
            _receiveCallback->Execute(reinterpret_cast<char*>(ptr), args->Buffer->Length);
        }
    }

    void SetRecvCallback(IReceiveCallback* receiveCallback)
    {
        _receiveCallback = receiveCallback;
    }

private:
    IReceiveCallback* _receiveCallback;
};

class CommunicationServiceBaseImpl
{
    friend class CommunicationServiceBase;
private:
    gcroot<Communication::ICommunicationService^>* _instance;
    gcroot<ReceiveEventReceiver^>* _receiveEventReceiver;
    IReceiveCallback* _receiveCallback;
    gcroot<EventHandler<ReceiveEventArgs^>^>* _eventHandler;
};

CommunicationServiceBase::CommunicationServiceBase(void* instance)
    : _impl(new CommunicationServiceBaseImpl())
{
    _impl->_instance = static_cast<gcroot<Communication::ICommunicationService^>*>(instance);

    auto receiveEventReceiver = gcnew ReceiveEventReceiver();
    _impl->_receiveEventReceiver = new gcroot<ReceiveEventReceiver^>(receiveEventReceiver);

    auto eventHandler = gcnew EventHandler<ReceiveEventArgs^>(receiveEventReceiver, &ReceiveEventReceiver::Hnadler);
    _impl->_eventHandler = new gcroot<EventHandler<ReceiveEventArgs^>^>(eventHandler);
    (*_impl->_instance)->Receive += *_impl->_eventHandler;
}

CommunicationServiceBase::~CommunicationServiceBase()
{
    (*_impl->_instance)->Receive -= *_impl->_eventHandler;
    _impl->_receiveCallback->Destroy();
    delete _impl->_receiveEventReceiver;
    delete _impl;
}

bool CommunicationServiceBase::Send(void* buffer, int size)
{
    auto bufferTemp = gcnew array<Byte>(size);
    Marshal::Copy((IntPtr)buffer, bufferTemp, 0, size);
    return (*_impl->_instance)->Send(bufferTemp);
}

void CommunicationServiceBase::SetReceiveCallback(IReceiveCallback* receiveCallback)
{
    _impl->_receiveCallback = receiveCallback;
    (*_impl->_receiveEventReceiver)->SetRecvCallback(_impl->_receiveCallback);
}

int CommunicationServiceBase::GetConnectCount()
{
    return (*_impl->_instance)->GetConnectCount();
}

void* CommunicationServiceBase::GetInstance()
{
    return _impl->_instance;
}

}
}
}