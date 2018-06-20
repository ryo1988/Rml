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

class RMLCOMMUNICATIONTCPCLRDLL_API TcpCommunicationService : public Communication::Clr::ICommunicationService
{
public:
    TcpCommunicationService();

    ~TcpCommunicationService() override;

    bool Send(void* buffer, int size) override;

    void SetReceiveCallback(Communication::Clr::ReceiveCallback receiveCallback) override;

    void SetReceiveCallback(std::function<void(void*,int)> receiveCallback) override;

    int GetConnectCount() override;

    void* GetInstance() override;

    bool StartListener(const char* ipAddress, int port);

    bool StartClient(const char* hostName, int port);

private:
    Communication::Clr::CommunicationServiceBase _base;
};

}
}
}
}