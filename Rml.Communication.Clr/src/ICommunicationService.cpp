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

ICommunicationService::ICommunicationService()
{
}

ICommunicationService::~ICommunicationService()
{
}

ref class ReceiveEventReceiver
{
public:
    void Hnadler(System::Object^ sender, ReceiveEventArgs^ args)
    {
        if (_receiveCallback == nullptr && _receiveCallbackFunc == nullptr)
        {
            return;
        }

        pin_ptr<System::Byte> buffetPtr = &args->Buffer[0];
        unsigned char* ptr = buffetPtr;
        if (_receiveCallback)
        {
            _receiveCallback(reinterpret_cast<char*>(ptr), args->Buffer->Length);
        }
        if (_receiveCallbackFunc)
        {
            (*_receiveCallbackFunc)(reinterpret_cast<char*>(ptr), args->Buffer->Length);
        }
    }

    void SetRecvCallback(ReceiveCallback receiveCallback)
    {
        _receiveCallback = receiveCallback;
        _receiveCallbackFunc = nullptr;
    }

    void SetRecvCallback(std::function<void(void*,int)>* receiveCallback)
    {
        _receiveCallback = nullptr;
        _receiveCallbackFunc = receiveCallback;
    }

private:
    ReceiveCallback _receiveCallback;
    std::function<void(void*,int)>* _receiveCallbackFunc;
};

CommunicationServiceBase::CommunicationServiceBase(void* instance)
    : _instance(instance)
    , _receiveEventReceiver(nullptr)
{
    auto instancePtr = static_cast<gcroot<Communication::ICommunicationService^>*>(_instance);

    auto receiveEventReceiver = gcnew ReceiveEventReceiver();
    _receiveEventReceiver = new gcroot<ReceiveEventReceiver^>(receiveEventReceiver);

    (*instancePtr)->Receive += gcnew EventHandler<ReceiveEventArgs^>(receiveEventReceiver, &ReceiveEventReceiver::Hnadler);
}

CommunicationServiceBase::~CommunicationServiceBase()
{
    if (_instance != nullptr)
    {
        auto instancePtr = static_cast<gcroot<Communication::ICommunicationService^>*>(_instance);
        delete instancePtr;
        _instance = nullptr;
    }
}

bool CommunicationServiceBase::Send(void* buffer, int size)
{
    auto instancePtr = static_cast<gcroot<Communication::ICommunicationService^>*>(_instance);

    auto bufferTemp = gcnew array<Byte>(size);
    Marshal::Copy((IntPtr)buffer, bufferTemp, 0, size);
    return (*instancePtr)->Send(bufferTemp);
}

void CommunicationServiceBase::SetReceiveCallback(ReceiveCallback receiveCallback)
{
    auto recvEventReceiverPtr = static_cast<gcroot<ReceiveEventReceiver^>*>(_receiveEventReceiver);
    (*recvEventReceiverPtr)->SetRecvCallback(receiveCallback);
}

void CommunicationServiceBase::SetReceiveCallback(std::function<void(void*,int)> receiveCallback)
{
    _receiveCallbackFunc = receiveCallback;
    auto recvEventReceiverPtr = static_cast<gcroot<ReceiveEventReceiver^>*>(_receiveEventReceiver);
    (*recvEventReceiverPtr)->SetRecvCallback(&_receiveCallbackFunc);
}

int CommunicationServiceBase::GetConnectCount()
{
    auto instancePtr = static_cast<gcroot<Communication::ICommunicationService^>*>(_instance);
    return (*instancePtr)->GetConnectCount();
}

void* CommunicationServiceBase::GetInstance()
{
    return _instance;
}

}
}
}