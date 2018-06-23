#include "stdafx.h"
#include "CppUnitTest.h"

#include <TcpCommunicationService.h>
#include <Rml.RedirectLoadAssemblyFolder.Clr.h>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace Rml::Communication::Tcp::Clr;
using namespace Rml::Communication::Clr;

namespace RmlCommunicationTcpClrTest
{		
    TEST_CLASS(UnitTest1)
    {
    public:
        
        TEST_METHOD(TestMethod1)
        {
            Rml::RedirectLoadAssemblyFolder::RedirectExecutingAssemblyFolder();

            char* data = new char[2];
            data[0] = 1;
            data[1] = 2;

            auto a = new TcpCommunicationService();
            auto b = new TcpCommunicationService();

            a->StartListener("127.0.0.1", 1988);
            b->StartClient("127.0.0.1", 1988);

            while (a->GetConnectCount() != 1 || b->GetConnectCount() != 1)
            {
            }

            auto recvA = false;
            auto recvB = false;
            auto recvedA = false;
            auto recvedB = false;
            a->SetReceiveCallback(ReceiveCallback::Create([&data,&recvA,&recvedA](void* buffer, int size)
            {
                if (memcmp(buffer, data, size) == 0)
                {
                    recvA = true;
                }
                recvedA = true;
            }));
            b->SetReceiveCallback(ReceiveCallback::Create([&data,&recvB,&recvedB](void* buffer, int size)
            {
                if (memcmp(buffer, data, size) == 0)
                {
                    recvB = true;
                }
                recvedB = true;
            }));

            a->Send(data, 2);
            b->Send(data, 2);

            while (recvedA == false || recvedB == false)
            {
            }

            Assert::IsTrue(recvA);
            Assert::IsTrue(recvB);
        }

    };
}