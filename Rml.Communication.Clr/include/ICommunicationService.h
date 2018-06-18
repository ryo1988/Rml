#pragma once

#include <functional>

#ifdef RMLCOMMUNICATIONCLRDLL_EXPORTS
#define RMLCOMMUNICATIONCLRDLL_API __declspec(dllexport)
#else
#define RMLCOMMUNICATIONCLRDLL_API __declspec(dllimport)
#endif

namespace Rml
{
namespace Communication
{
namespace Clr
{
typedef void (*ReceiveCallback)(void*, int);

class RMLCOMMUNICATIONCLRDLL_API ICommunicationService
{
public:
    ICommunicationService();

    virtual ~ICommunicationService();

    virtual bool Send(void* buffer, int size) = 0;

    virtual void SetReceiveCallback(ReceiveCallback receiveCallback) = 0;

    virtual void SetReceiveCallback(std::function<void(void*,int)> receiveCallback) = 0;

    virtual int GetConnectCount() = 0;

    virtual void* GetInstance() = 0;
};

class RMLCOMMUNICATIONCLRDLL_API CommunicationServiceBase : public ICommunicationService
{
public:
    CommunicationServiceBase(void* instance);

    ~CommunicationServiceBase() override;

    bool Send(void* buffer, int size) override;

    void SetReceiveCallback(ReceiveCallback receiveCallback) override;

    void SetReceiveCallback(std::function<void(void*,int)> receiveCallback) override;

    int GetConnectCount() override;

    void* GetInstance() override;

private:
    void* _instance;
    void* _receiveEventReceiver;
    std::function<void(void*,int)> _receiveCallbackFunc;
};

}
}
}