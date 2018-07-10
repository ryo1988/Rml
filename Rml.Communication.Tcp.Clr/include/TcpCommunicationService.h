#pragma once

#include <functional>
#include <ICommunicationService.h>

#ifdef RMLCOMMUNICATIONTCPCLRDLL_EXPORTS
#define RMLCOMMUNICATIONTCPCLRDLL_API __declspec(dllexport)
#else
#define RMLCOMMUNICATIONTCPCLRDLL_API __declspec(dllimport)
#endif

namespace Rml
{
namespace Communication
{
namespace Tcp
{
namespace Clr
{

class TcpCommunicationServiceImpl;

class RMLCOMMUNICATIONTCPCLRDLL_API TcpCommunicationService : public Communication::Clr::ICommunicationService
{
public:
    TcpCommunicationService();

    virtual ~TcpCommunicationService();

    virtual bool Send(void* buffer, int size) override;

    virtual void SetReceiveCallback(Communication::Clr::IReceiveCallback* receiveCallback) override;

    virtual void SetLogedCallback(Communication::Clr::ILogedCallback* logedCallback) override;

    virtual int GetConnectCount() override;

    virtual void* GetInstance() override;

    bool StartListener(const char* ipAddress, int port);

    bool StartClient(const char* hostName, int port);

private:
    TcpCommunicationServiceImpl* _impl;
};

}
}
}
}