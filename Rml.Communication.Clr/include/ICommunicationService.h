#pragma once

#include <functional>
#include <utility>

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

class IReceiveCallback
{
public:
    virtual ~IReceiveCallback()
    {
    };

    virtual void Destroy() = 0;
    virtual void Execute(void* buffer, const int size) const = 0;
};

class ReceiveCallback : public IReceiveCallback
{
public:
    static ReceiveCallback* Create(const std::function<void(void*, int)> receiveCallback)
    {
        return new ReceiveCallback(receiveCallback);
    }

private:
    explicit ReceiveCallback(std::function<void(void*, int)> receiveCallback)
        : _receiveCallback(std::move(receiveCallback))
    {
    }

public:
    virtual void Destroy() override
    {
        delete this;
    }

    virtual void Execute(void* buffer, const int size) const override
    {
        _receiveCallback(buffer, size);
    }

private:
    std::function<void(void*, int)> _receiveCallback;
};

class RMLCOMMUNICATIONCLRDLL_API ICommunicationService
{
public:
    virtual ~ICommunicationService();

    virtual bool Send(void* buffer, int size) = 0;

    virtual void SetReceiveCallback(IReceiveCallback* receiveCallback) = 0;

    virtual int GetConnectCount() = 0;

    virtual void* GetInstance() = 0;
};

class CommunicationServiceBaseImpl;


class RMLCOMMUNICATIONCLRDLL_API CommunicationServiceBase : public ICommunicationService
{
public:
    explicit CommunicationServiceBase(void* instance);

    virtual ~CommunicationServiceBase();

    virtual bool Send(void* buffer, int size) override;

    virtual void SetReceiveCallback(IReceiveCallback* receiveCallback) override;

    virtual int GetConnectCount() override;

    virtual void* GetInstance() override;

private:
    CommunicationServiceBaseImpl* _impl;
};

}
}
}