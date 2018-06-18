#pragma once

#include <functional>
#include <ICommunicationService.h>

#ifdef RMLCOMMUNICATIONDIRECTCLRDLL_EXPORTS
#define RMLCOMMUNICATIONDIRECTCLRDLL_API __declspec(dllexport)
#else
#define RMLCOMMUNICATIONDIRECTCLRDLL_API __declspec(dllimport)
#endif

namespace Rml
{
namespace Communication
{
namespace Direct
{
namespace Clr
{

class RMLCOMMUNICATIONDIRECTCLRDLL_API DirectCommunicationService : public Communication::Clr::ICommunicationService
{
public:
    DirectCommunicationService();

    ~DirectCommunicationService() override;

    bool Send(void* buffer, int size) override;

    void SetReceiveCallback(Communication::Clr::ReceiveCallback receiveCallback) override;

    void SetReceiveCallback(std::function<void(void*,int)> receiveCallback) override;

    int GetConnectCount() override;

    void* GetInstance() override;

    void SetTarget(DirectCommunicationService* target);

private:
    Communication::Clr::CommunicationServiceBase _base;
};

}
}
}
}