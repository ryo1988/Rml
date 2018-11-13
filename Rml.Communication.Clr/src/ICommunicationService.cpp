#include "stdafx.h"
#include "..\include\ICommunicationService.h"

#include <vcclr.h>
#include <msclr/marshal_cppstd.h>
#include <msclr/marshal.h>
#include <msclr/marshal_windows.h>
#include <msclr/marshal_atl.h>

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
    ReceiveEventReceiver::ReceiveEventReceiver()
    : _receiveCallback(nullptr)
    {
        
    }

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

    void SetCallback(IReceiveCallback* receiveCallback)
    {
        _receiveCallback = receiveCallback;
    }

private:
    IReceiveCallback* _receiveCallback;
};

ref class LogedEventReceiver
{
public:
    LogedEventReceiver::LogedEventReceiver()
    : _logedCallback(nullptr)
    {
    }

    void Hnadler(System::Object^ sender, LogEventArgs^ args)
    {
        if (_logedCallback == nullptr)
        {
            return;
        }

        auto context = gcnew msclr::interop::marshal_context;
        auto str = context->marshal_as<std::string>(args->Log);
        delete context;

        if (_logedCallback)
        {
            _logedCallback->Execute(str.c_str(), str.size());
        }
    }

    void SetCallback(ILogedCallback* logedeCallback)
    {
        _logedCallback = logedeCallback;
    }

private:
    ILogedCallback* _logedCallback;
};

class CommunicationServiceBaseImpl
{
    friend class CommunicationServiceBase;
private:
    gcroot<Communication::ICommunicationService^>* _instance;

    gcroot<ReceiveEventReceiver^>* _receiveEventReceiver;
    IReceiveCallback* _receiveCallback;
    gcroot<EventHandler<ReceiveEventArgs^>^>* _receiveEventHandler;

    gcroot<LogedEventReceiver^>* _logedEventReceiver;
    ILogedCallback* _logedCallback;
    gcroot<EventHandler<LogEventArgs^>^>* _logedEventHandler;
};

CommunicationServiceBase::CommunicationServiceBase(void* instance)
    : _impl(new CommunicationServiceBaseImpl())
{
    _impl->_instance = static_cast<gcroot<Communication::ICommunicationService^>*>(instance);

    {
        auto receiveEventReceiver = gcnew ReceiveEventReceiver();
        _impl->_receiveEventReceiver = new gcroot<ReceiveEventReceiver^>(receiveEventReceiver);

        auto eventHandler = gcnew EventHandler<ReceiveEventArgs^>(receiveEventReceiver, &ReceiveEventReceiver::Hnadler);
        _impl->_receiveEventHandler = new gcroot<EventHandler<ReceiveEventArgs^>^>(eventHandler);
        (*_impl->_instance)->Receive += *_impl->_receiveEventHandler;
    }

    {
        auto logedEventReceiver = gcnew LogedEventReceiver();
        _impl->_logedEventReceiver = new gcroot<LogedEventReceiver^>(logedEventReceiver);

        auto eventHandler = gcnew EventHandler<LogEventArgs^>(logedEventReceiver, &LogedEventReceiver::Hnadler);
        _impl->_logedEventHandler = new gcroot<EventHandler<LogEventArgs^>^>(eventHandler);
        (*_impl->_instance)->Loged += *_impl->_logedEventHandler;
    }
}

CommunicationServiceBase::~CommunicationServiceBase()
{
    (*_impl->_instance)->Loged -= *_impl->_logedEventHandler;
    if (_impl->_logedCallback != nullptr)
    {
        _impl->_logedCallback->Destroy();
    }
    delete _impl->_logedEventReceiver;

    (*_impl->_instance)->Receive -= *_impl->_receiveEventHandler;
    if (_impl->_receiveCallback != nullptr)
    {
        _impl->_receiveCallback->Destroy();
    }
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
    (*_impl->_receiveEventReceiver)->SetCallback(_impl->_receiveCallback);
}

void CommunicationServiceBase::SetLogedCallback(ILogedCallback* logedCallback)
{
    _impl->_logedCallback = logedCallback;
    (*_impl->_logedEventReceiver)->SetCallback(_impl->_logedCallback);
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